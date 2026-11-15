using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 세션 메타(k:"meta", 세션에 한 번): 기록기 판·git 해시·Unity 판·해상도·입력 장치·영상 설정·시작 레벨/맵/장비/골드.
/// 플레이어가 생긴 첫 틱(또는 시작 10초 뒤)에 쓴다. git 해시는 에디터에서 .git 을 직접 읽는다(프로세스 실행 없음).
/// </summary>
public static class PlayRecMeta
{
    public const string Version = "R2";

    public static void Write(string map, PlayObservation o, PlayRecVideo video)
    {
        PlayObsContext c = o.Context;
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("meta");
                PlayRecJson.KeyStr(sb, "ver", Version);
                PlayRecJson.KeyStr(sb, "session", PlayRecWriter.Session);
                PlayRecJson.KeyStr(sb, "git", GitHash());
                PlayRecJson.KeyStr(sb, "unity", Application.unityVersion);
                PlayRecJson.KeyStr(sb, "platform", Application.platform.ToString());
                PlayRecJson.KeyInt(sb, "sw", Screen.width);
                PlayRecJson.KeyInt(sb, "sh", Screen.height);
                PlayRecJson.KeyStr(sb, "dev", Devices());
                PlayRecJson.KeyStr(sb, "map", map);
                PlayRecJson.KeyInt(sb, "lv", c.Level);
                PlayRecJson.KeyInt(sb, "gold", c.Gold);
                PlayRecJson.KeyInt(sb, "hasPl", o.HasPlayer ? 1 : 0);
                PlayRecTick.WriteContext(sb, c);
                PlayRecJson.KeyStr(sb, "video", video != null && video.Active ? PlayRecVideo.Width + "x" + PlayRecVideo.Height + "@" + PlayRecVideo.Fps + " crf" + PlayRecVideo.Crf : "off");
                if (video != null && video.Error != null)
                {
                    PlayRecJson.KeyStr(sb, "videoErr", video.Error);
                }
                PlayRecJson.KeyInt(sb, "tkHz", PlayRecorder.TickHz);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private static string Devices()
    {
        StringBuilder sb = new StringBuilder();
        foreach (InputDevice d in InputSystem.devices)
        {
            if (d is Keyboard || d is Gamepad || d is Mouse)
            {
                if (sb.Length > 0)
                {
                    sb.Append(';');
                }
                sb.Append(d.layout).Append(':').Append(d.displayName);
            }
        }
        return sb.ToString();
    }

    /// <summary>현재 커밋 해시 앞 10자리. 못 읽으면 "?"(빌드는 Application.version)</summary>
    public static string GitHash()
    {
#if UNITY_EDITOR
        try
        {
            string git = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".git");
            string head = File.ReadAllText(Path.Combine(git, "HEAD")).Trim();
            if (!head.StartsWith("ref: ", StringComparison.Ordinal))
            {
                return Short(head);
            }
            string refName = head.Substring(5).Trim();
            string refPath = Path.Combine(git, refName.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(refPath))
            {
                return Short(File.ReadAllText(refPath).Trim());
            }
            string packed = Path.Combine(git, "packed-refs");
            if (File.Exists(packed))
            {
                foreach (string line in File.ReadAllLines(packed))
                {
                    if (line.EndsWith(" " + refName, StringComparison.Ordinal))
                    {
                        return Short(line.Substring(0, line.IndexOf(' ')));
                    }
                }
            }
        }
        catch (Exception)
        {
            // 읽기 실패는 "?"
        }
        return "?";
#else
        return Application.version;
#endif
    }

    private static string Short(string hash)
    {
        return hash.Length > 10 ? hash.Substring(0, 10) : hash;
    }
}
