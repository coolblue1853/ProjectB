using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// 북마크("여기 이상해"): 키보드 F8 또는 패드 SELECT+R3. 그 순간 k:"bm" 한 줄 + 원본 해상도 PNG(Logs/playrec/세션/mark_NN.png).
/// 상태 전체(tk·st·cx·gd·포커스)는 PlayRecorder 가 같은 프레임에 강제로 한 번 더 쓴다. 화면에 "북마크 저장" 알림(PickupLog.Notice).
/// PNG: 프레임 끝 화면 → RenderTexture → AsyncGPUReadback → 스레드풀에서 PNG 인코딩·저장(메인 스레드 멈춤 없음).
/// </summary>
public sealed class PlayRecBookmark
{
    private const float MinGapSeconds = 0.5f;

    private int _count;
    private bool _captureQueued;
    private string _pendingPath;
    private float _lastReal = -99f;
    private RenderTexture _rt;

    public int Count => _count;

    /// <summary>이번 프레임에 북마크 입력이 있었나(F8 · SELECT+R3)</summary>
    public static bool Pressed()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f8Key.wasPressedThisFrame)
        {
            return true;
        }
        PadFieldInput.Refresh();
        return PadFieldInput.SelectR3;
    }

    /// <summary>북마크 한 건을 시작한다. 번호를 돌려준다(연타 0.5초 안은 무시하고 -1).</summary>
    public int Begin(float real, string map, PlayObservation o)
    {
        if (real - _lastReal < MinGapSeconds)
        {
            return -1;
        }
        _lastReal = real;
        _count++;
        string dir = PlayRecWriter.SessionDir;
        string file = "mark_" + _count.ToString("00") + ".png";
        _pendingPath = dir != null ? Path.Combine(dir, file) : null;
        _captureQueued = _pendingPath != null;
        WriteRow(real, map, o, file);
        PickupLog.Notice("북마크 저장 #" + _count);
        return _count;
    }

    /// <summary>프레임 끝(WaitForEndOfFrame)에서 부른다.</summary>
    public void CaptureIfQueued()
    {
        if (!_captureQueued)
        {
            return;
        }
        _captureQueued = false;
        string path = _pendingPath;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            int w = Mathf.Max(16, Screen.width);
            int h = Mathf.Max(16, Screen.height);
            if (_rt == null || _rt.width != w || _rt.height != h)
            {
                Release();
                _rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { name = "PlayRecMark" };
            }
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_rt);
            AsyncGPUReadback.Request(_rt, 0, TextureFormat.RGBA32, req => OnReadback(req, path)); // 요청마다 경로를 묶는다(북마크는 드물어 할당 허용)
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PB][PlayRec] 북마크 스크린샷 실패: " + e.Message);
        }
    }

    private static void OnReadback(AsyncGPUReadbackRequest req, string path)
    {
        if (req.hasError)
        {
            return;
        }
        int w = req.width;
        int h = req.height;
        byte[] data = req.GetData<byte>().ToArray();
        bool flip = PlayRecVideo.ReadbackTopDown; // EncodeArrayToPNG 는 아래 행부터 받는다
        ThreadPool.QueueUserWorkItem(_ => Save(data, w, h, flip, path));
    }

    private static void Save(byte[] data, int w, int h, bool flip, string path)
    {
        try
        {
            if (flip)
            {
                FlipRows(data, w * 4, h);
            }
            byte[] png = ImageConversion.EncodeArrayToPNG(data, GraphicsFormat.R8G8B8A8_UNorm, (uint)w, (uint)h);
            File.WriteAllBytes(path, png);
        }
        catch (Exception)
        {
            // 저장 실패는 기록에 영향 없다(bm 줄은 이미 남음)
        }
    }

    private static void FlipRows(byte[] data, int stride, int rows)
    {
        byte[] tmp = new byte[stride];
        for (int top = 0, bottom = rows - 1; top < bottom; top++, bottom--)
        {
            Buffer.BlockCopy(data, top * stride, tmp, 0, stride);
            Buffer.BlockCopy(data, bottom * stride, data, top * stride, stride);
            Buffer.BlockCopy(tmp, 0, data, bottom * stride, stride);
        }
    }

    private void WriteRow(float real, string map, PlayObservation o, string file)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("bm");
                PlayRecJson.KeyInt(sb, "n", _count);
                PlayRecJson.KeyNum(sb, "u", real, 3);
                PlayRecJson.KeyStr(sb, "png", PlayRecWriter.Session + "/" + file);
                PlayRecJson.KeyStr(sb, "map", map);
                if (o.HasPlayer)
                {
                    PlayRecJson.KeyNum(sb, "x", o.X, 2);
                    PlayRecJson.KeyNum(sb, "y", o.Y, 2);
                }
                PlayRecJson.KeyStr(sb, "fsm", o.Fsm);
                PlayRecJson.KeyStr(sb, "foc", o.Ui.Focus);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    public void Release()
    {
        if (_rt == null)
        {
            return;
        }
        _rt.Release();
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(_rt);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(_rt);
        }
        _rt = null;
    }
}
