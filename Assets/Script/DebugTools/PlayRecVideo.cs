using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

/// <summary>
/// 플레이 영상: 게임 화면(UI 포함 최종 화면)을 Width x Height·Fps 로 줄여 ffmpeg 표준입력으로 보내 H.264 mp4 로 굽는다.
/// 메인 스레드: 프레임 끝에 화면 → RenderTexture(축소) → AsyncGPUReadback 요청만. 파이프 쓰기는 전용 스레드·큐.
/// 큐가 넘치거나 읽기가 밀리면 그 프레임은 버리고 Dropped 를 센다. 실패하면 녹화만 끄고(Error) 기록은 계속한다.
/// mp4 는 조각(fragmented) 형식이라 비정상 종료로 끝이 잘려도 앞부분은 재생된다. 종료(남은 프레임 → 표준입력 닫기 → ffmpeg 대기·강제 종료)는
/// 파이프 스레드가 한다. Stop(false) 는 신호만 보내고 바로 돌아오며, 도메인 리로드 직전만 Stop(true) 로 최대 3초 기다린다.
/// 창 캡처가 아니라 게임 내부 캡처라 창이 가려져도 된다(에디터 최소화 중에는 Game 뷰가 그리는 만큼만 찍힌다).
/// </summary>
public sealed class PlayRecVideo
{
    public const int Width = 640;
    public const int Height = 360;
    public const int Fps = 15;
    public const int Crf = 30;
    private const int QueueMax = 8;
    private const int PendingMax = 4;
    private const int FrameBytes = Width * Height * 4;

    /// <summary>화면 캡처 RenderTexture 를 AsyncGPUReadback 으로 읽으면 맨 위 행부터 오는가(D3D·Vulkan·Metal = 예, OpenGL = 아니오).
    /// 11-15 D3D11 에디터 실측: 위 행부터 → ffmpeg 뒤집기 없음. 아니면 ffmpeg vflip.</summary>
    public static bool ReadbackTopDown => SystemInfo.graphicsUVStartsAtTop;

    public bool Active { get; private set; }
    public string Error { get; private set; }
    public string Path { get; private set; }
    /// <summary>ffmpeg 큐에 넣은 프레임 수(= 다음 프레임 번호)</summary>
    public long Frames => Interlocked.Read(ref _frames);
    public int Dropped => _dropped;
    /// <summary>마지막으로 큐에 넣은 프레임의 캡처 시각(unscaledTime)</summary>
    public float LastFrameReal { get; private set; } = -1f;

    private readonly object _lock = new object();
    private readonly Queue<byte[]> _queue = new Queue<byte[]>(QueueMax);
    private readonly Stack<byte[]> _free = new Stack<byte[]>(QueueMax + PendingMax);
    private readonly Queue<float> _pendingT = new Queue<float>(PendingMax);
    private Action<AsyncGPUReadbackRequest> _onReadback;
    private Process _ff;
    private Stream _stdin;
    private Thread _thread;
    private volatile bool _closing;
    private volatile string _threadError;
    private bool _stopped;
    private long _frames;
    private int _dropped;
    private RenderTexture _full;
    private RenderTexture _small;
    private float _nextReal;
    private readonly System.Text.StringBuilder _stderr = new System.Text.StringBuilder();

    public bool Start(string path)
    {
        Path = path;
        string exe = FindFfmpeg();
        if (exe == null)
        {
            Fail("ffmpeg 를 찾지 못함(PATH·WinGet)");
            return false;
        }
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = BuildArgs(path),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = false,
            };
            _ff = Process.Start(psi);
            _ff.ErrorDataReceived += OnStderr;
            _ff.BeginErrorReadLine();
            _stdin = _ff.StandardInput.BaseStream;
        }
        catch (Exception e)
        {
            KillQuietly(_ff); // 시작 뒤 설정에서 실패했으면 고아 프로세스를 남기지 않는다
            _ff = null;
            _stdin = null;
            Fail("ffmpeg 시작 실패: " + e.Message);
            return false;
        }
        _onReadback = OnReadback;
        _closing = false;
        _thread = new Thread(PipeLoop) { IsBackground = true, Name = "PlayRecVideo" };
        _thread.Start();
        Active = true;
        return true;
    }

    public static string BuildArgs(string path)
    {
        return "-hide_banner -loglevel error -nostats -y -f rawvideo -pix_fmt rgba -s " + Width + "x" + Height + " -r " + Fps
            + " -i - " + (ReadbackTopDown ? "" : "-vf vflip ") + "-c:v libx264 -preset veryfast -crf " + Crf + " -g " + (Fps * 2)
            + " -pix_fmt yuv420p -movflags +frag_keyframe+empty_moov+default_base_moof \"" + path + "\"";
    }

    /// <summary>프레임 끝(WaitForEndOfFrame)에서 부른다. 차례가 된 프레임만 찍는다.</summary>
    public void CaptureIfDue(float real)
    {
        if (!Active)
        {
            return;
        }
        if (_threadError != null)
        {
            Fail(_threadError);
            return;
        }
        if (real < _nextReal)
        {
            return;
        }
        _nextReal = real - _nextReal > 0.5f ? real + 1f / Fps : _nextReal + 1f / Fps;
        if (_pendingT.Count >= PendingMax)
        {
            _dropped++;
            return;
        }
        try
        {
            EnsureTargets();
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_full);
            Graphics.Blit(_full, _small);
            _pendingT.Enqueue(real);
            AsyncGPUReadback.Request(_small, 0, TextureFormat.RGBA32, _onReadback);
        }
        catch (Exception e)
        {
            Fail("캡처 실패: " + e.Message);
        }
    }

    private void EnsureTargets()
    {
        int w = Mathf.Max(16, Screen.width);
        int h = Mathf.Max(16, Screen.height);
        if (_full == null || _full.width != w || _full.height != h)
        {
            ReleaseTexture(ref _full);
            _full = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { name = "PlayRecFull" };
        }
        if (_small == null)
        {
            _small = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32) { name = "PlayRecSmall", filterMode = FilterMode.Bilinear };
        }
    }

    private void OnReadback(AsyncGPUReadbackRequest req)
    {
        float t = _pendingT.Count > 0 ? _pendingT.Dequeue() : Time.unscaledTime;
        if (!Active || req.hasError)
        {
            if (Active)
            {
                _dropped++;
            }
            return;
        }
        byte[] buf;
        lock (_lock)
        {
            if (_queue.Count >= QueueMax)
            {
                _dropped++;
                return;
            }
            buf = _free.Count > 0 ? _free.Pop() : new byte[FrameBytes];
        }
        req.GetData<byte>().CopyTo(buf);
        lock (_lock)
        {
            _queue.Enqueue(buf);
            Monitor.Pulse(_lock);
        }
        Interlocked.Increment(ref _frames);
        LastFrameReal = t;
    }

    private void PipeLoop()
    {
        while (true)
        {
            byte[] buf;
            lock (_lock)
            {
                while (_queue.Count == 0 && !_closing)
                {
                    Monitor.Wait(_lock);
                }
                if (_queue.Count == 0)
                {
                    break; // 닫는 중이고 남은 프레임 없음
                }
                buf = _queue.Dequeue();
            }
            try
            {
                _stdin.Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                _threadError = "ffmpeg 파이프 쓰기 실패: " + e.Message;
                lock (_lock)
                {
                    _queue.Clear();
                }
                break;
            }
            lock (_lock)
            {
                _free.Push(buf);
            }
        }
        FinishProcess();
    }

    /// <summary>파이프 스레드 끝: 표준입력을 닫아 ffmpeg 가 mp4 를 마무리하게 하고, 5초 안에 안 끝나면 강제 종료한다.</summary>
    private void FinishProcess()
    {
        try
        {
            _stdin?.Close();
        }
        catch (Exception)
        {
            // 이미 닫힘(쓰기 실패 등)
        }
        try
        {
            if (_ff != null && !_ff.WaitForExit(5000))
            {
                Debug.LogWarning("[PB][PlayRec] ffmpeg 가 5초 안에 끝나지 않아 강제 종료(mp4 는 조각 형식이라 앞부분은 남는다)");
                _ff.Kill();
            }
        }
        catch (Exception)
        {
            // 이미 끝남
        }
        _ff?.Dispose();
    }

    private static void KillQuietly(Process p)
    {
        try
        {
            if (p != null && !p.HasExited)
            {
                p.Kill();
            }
        }
        catch (Exception)
        {
            // 이미 끝남
        }
    }

    /// <summary>
    /// 녹화를 멈춘다. 남은 프레임 전송·표준입력 닫기·ffmpeg 대기는 파이프 스레드가 하므로 block=false 면 메인 스레드는 기다리지 않는다.
    /// block=true(도메인 리로드 직전: 스레드가 중단되기 전에 끝내야 함)면 최대 3초 기다리고, 그래도 안 끝나면 ffmpeg 를 강제 종료한다.
    /// 여러 번 불러도 된다.
    /// </summary>
    public void Stop(bool block)
    {
        if (_stopped)
        {
            return;
        }
        _stopped = true;
        Active = false;
        Thread th = _thread;
        _thread = null;
        if (th != null)
        {
            lock (_lock)
            {
                _closing = true;
                Monitor.PulseAll(_lock);
            }
            if (block && !th.Join(3000))
            {
                Debug.LogWarning("[PB][PlayRec] 영상 마무리가 3초 안에 끝나지 않아 ffmpeg 강제 종료");
                KillQuietly(_ff);
            }
        }
        _pendingT.Clear();
        ReleaseTexture(ref _full);
        ReleaseTexture(ref _small);
    }

    public string StderrTail()
    {
        lock (_stderr)
        {
            return _stderr.ToString();
        }
    }

    private void OnStderr(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        lock (_stderr)
        {
            if (_stderr.Length > 1000)
            {
                _stderr.Remove(0, _stderr.Length - 500);
            }
            _stderr.Append(e.Data).Append(' ');
        }
    }

    private void Fail(string message)
    {
        Error = message;
        Debug.LogWarning("[PB][PlayRec] 영상 녹화 끔(기록은 계속): " + message);
        Stop(false);
    }

    private static void ReleaseTexture(ref RenderTexture rt)
    {
        if (rt == null)
        {
            return;
        }
        rt.Release();
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(rt);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(rt);
        }
        rt = null;
    }

    /// <summary>PATH(프로세스·사용자·시스템) → WinGet 설치 폴더 순으로 ffmpeg.exe 를 찾는다.</summary>
    public static string FindFfmpeg()
    {
        string name = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer ? "ffmpeg.exe" : "ffmpeg";
        string[] paths =
        {
            Environment.GetEnvironmentVariable("PATH"),
            SafeEnv(EnvironmentVariableTarget.User),
            SafeEnv(EnvironmentVariableTarget.Machine),
        };
        for (int p = 0; p < paths.Length; p++)
        {
            if (string.IsNullOrEmpty(paths[p]))
            {
                continue;
            }
            foreach (string dir in paths[p].Split(System.IO.Path.PathSeparator))
            {
                string f = SafeCombine(dir.Trim(), name);
                if (f != null && File.Exists(f))
                {
                    return f;
                }
            }
        }
        try
        {
            string root = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Packages");
            if (Directory.Exists(root))
            {
                foreach (string pkg in Directory.GetDirectories(root, "*FFmpeg*"))
                {
                    string[] hits = Directory.GetFiles(pkg, name, SearchOption.AllDirectories);
                    if (hits.Length > 0)
                    {
                        return hits[0];
                    }
                }
            }
        }
        catch (Exception)
        {
            // 폴더 접근 실패는 "없음"으로 본다
        }
        return null;
    }

    private static string SafeEnv(EnvironmentVariableTarget target)
    {
        try
        {
            return Environment.GetEnvironmentVariable("PATH", target);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string SafeCombine(string dir, string name)
    {
        try
        {
            return dir.Length > 0 ? System.IO.Path.Combine(dir, name) : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
