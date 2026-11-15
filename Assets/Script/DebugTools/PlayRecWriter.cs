using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// 플레이 기록 파일(jsonl) 쓰기 담당. 한 줄 = JSON 한 객체. 모든 쓰기는 Gate 잠금 안에서 Sb 에 조립한 뒤 Commit 으로 파일 버퍼에 옮긴다.
/// 여러 스레드(로그 콜백)에서 불릴 수 있다. 디스크 오류가 나면 첫 실패에서 기록을 끈다(게임은 계속).
/// 세션 이름 = 시작 시각(yyyyMMdd_HHmmss). jsonl·mp4·북마크 폴더가 같은 이름을 쓴다.
/// </summary>
public static class PlayRecWriter
{
    private const int KeepSessions = 30;

    public static readonly object Gate = new object();
    public static readonly StringBuilder Sb = new StringBuilder(4096);

    private static StreamWriter _writer;
    private static char[] _copyBuf = new char[4096];
    private static int _mainThreadId;
    private static volatile float _lastNow;
    private static volatile float _lastReal;

    public static bool IsOpen => _writer != null;
    public static string Dir { get; private set; }
    public static string Session { get; private set; }
    public static string FilePath { get; private set; }

    /// <summary>북마크 PNG 를 둘 세션 폴더(필요할 때 만든다)</summary>
    public static string SessionDir => Dir != null ? Path.Combine(Dir, Session) : null;

    public static void ResetStatics()
    {
        Close();
        Sb.Length = 0;
        FilePath = null;
        Session = null;
    }

    /// <summary>메인 스레드에서 한 번. 기록 폴더를 만들고 오래된 세션을 지운 뒤 새 jsonl 을 연다.</summary>
    public static void Open()
    {
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
#if UNITY_EDITOR
        Dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "playrec");
#else
        Dir = Path.Combine(Application.persistentDataPath, "playrec");
#endif
        Directory.CreateDirectory(Dir);
        PruneOldSessions(Dir, KeepSessions);
        Session = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        FilePath = Path.Combine(Dir, Session + ".jsonl");
        _writer = new StreamWriter(new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false), 65536);
        _writer.NewLine = "\n";
    }

    public static void Close()
    {
        lock (Gate)
        {
            if (_writer == null)
            {
                return;
            }
            try
            {
                _writer.Flush();
                _writer.Dispose();
            }
            catch (Exception)
            {
                // 닫는 중 오류는 무시
            }
            _writer = null;
        }
    }

    public static void Flush()
    {
        lock (Gate)
        {
            if (_writer == null)
            {
                return;
            }
            try
            {
                _writer.Flush();
            }
            catch (Exception)
            {
                // 디스크 오류로 게임이 멈추면 안 된다
            }
        }
    }

    /// <summary>메인 스레드에서 매 프레임 갱신. 다른 스레드는 마지막 값을 쓴다.</summary>
    public static void Tick(float now, float real)
    {
        _lastNow = now;
        _lastReal = real;
    }

    public static float Now()
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            _lastNow = Time.time;
        }
        return _lastNow;
    }

    public static float Real()
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            _lastReal = Time.unscaledTime;
        }
        return _lastReal;
    }

    /// <summary>Gate 안에서 부른다: 공통 머리(t·k·src)를 쓰고 Sb 를 돌려준다.</summary>
    public static StringBuilder Begin(string kind)
    {
        return PlayRecJson.Begin(Sb, Now(), kind, PlayRecorder.Source);
    }

    /// <summary>Gate 안에서 부른다: 줄을 닫고 파일 버퍼로 옮긴다.</summary>
    public static void Commit()
    {
        PlayRecJson.End(Sb);
        WriteOut();
    }

    /// <summary>Gate 안에서 부른다: 이미 닫힌 줄(End 까지 쓴 Sb)을 그대로 옮긴다.</summary>
    public static void CommitRaw()
    {
        WriteOut();
    }

    /// <summary>Gate 안에서 부른다: 조립하다 실패한 줄을 버린다.</summary>
    public static void Discard()
    {
        Sb.Length = 0;
    }

    private static void WriteOut()
    {
        if (_writer == null)
        {
            Sb.Length = 0;
            return;
        }
        int len = Sb.Length;
        if (_copyBuf.Length < len)
        {
            _copyBuf = new char[Mathf.NextPowerOfTwo(len)];
        }
        Sb.CopyTo(0, _copyBuf, 0, len);
        Sb.Length = 0;
        try
        {
            _writer.Write(_copyBuf, 0, len);
        }
        catch (Exception)
        {
            PlayRecorder.Enabled = false; // 디스크가 가득 찼거나 파일이 사라지면(IO·이미 닫힘) 첫 실패에서 기록을 끈다
        }
    }

    /// <summary>오래된 세션은 최근 keep 개만 남긴다(jsonl + 같은 이름의 mp4·폴더·_frames·_summary). 실패해도 무시.</summary>
    private static void PruneOldSessions(string dir, int keep)
    {
        try
        {
            string[] files = Directory.GetFiles(dir, "*.jsonl");
            if (files.Length < keep)
            {
                return;
            }
            Array.Sort(files, StringComparer.Ordinal); // 파일명이 시각이라 사전순 = 시간순
            for (int i = 0; i <= files.Length - keep; i++)
            {
                try
                {
                    string stem = Path.Combine(dir, Path.GetFileNameWithoutExtension(files[i]));
                    File.Delete(files[i]);
                    DeleteIfExists(stem + ".mp4");
                    DeleteIfExists(stem + "_summary.md");
                    DeleteDirIfExists(stem);
                    DeleteDirIfExists(stem + "_frames");
                }
                catch (Exception)
                {
                    // 잠긴 파일 하나 때문에 나머지 정리를 멈추지 않는다
                }
            }
        }
        catch (Exception)
        {
            // 정리 실패는 기록에 영향 없다
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void DeleteDirIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
