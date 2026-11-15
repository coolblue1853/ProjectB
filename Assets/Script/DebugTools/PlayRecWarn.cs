using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 경고·예외 기록(k:"warn", R1 그대로): Unity 로그의 Warning·Error·Exception 과 기록기 자체 오류(snapshot).
/// 같은 메시지는 처음 5번 + 100번째마다만 남긴다. 어느 스레드에서 불려도 된다(Gate 잠금).
/// </summary>
public sealed class PlayRecWarn
{
    private const int WarnMax = 200;
    private const int WarnKeysMax = 500;

    private readonly Dictionary<int, int> _counts = new Dictionary<int, int>();

    public void Subscribe()
    {
        Application.logMessageReceivedThreaded += OnLogMessage;
    }

    public void Unsubscribe()
    {
        Application.logMessageReceivedThreaded -= OnLogMessage;
    }

    public void Write(string type, string message, string stack)
    {
        int key = (message ?? "").GetHashCode();
        lock (PlayRecWriter.Gate)
        {
            if (_counts.Count >= WarnKeysMax && !_counts.ContainsKey(key))
            {
                _counts.Clear(); // 종류가 무한히 늘어나는 경우의 상한
            }
            _counts.TryGetValue(key, out int count);
            count++;
            _counts[key] = count;
            if (count > 5 && count % 100 != 0)
            {
                return;
            }
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("warn");
                PlayRecJson.KeyStr(sb, "ty", type);
                sb.Append(",\"msg\":");
                PlayRecJson.AppendTrimmed(sb, message, WarnMax);
                string stackLine = FirstLine(stack);
                if (!string.IsNullOrEmpty(stackLine))
                {
                    sb.Append(",\"stk\":");
                    PlayRecJson.AppendTrimmed(sb, stackLine, WarnMax);
                }
                if (count > 1)
                {
                    PlayRecJson.KeyInt(sb, "n", count);
                }
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!PlayRecorder.Enabled || !PlayRecWriter.IsOpen || type == LogType.Log)
        {
            return;
        }
        string ty = type == LogType.Warning ? "Warning" : type == LogType.Exception ? "Exception" : "Error";
        Write(ty, condition, stackTrace);
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        int nl = text.IndexOf('\n');
        return nl >= 0 ? text.Substring(0, nl) : text;
    }
}
