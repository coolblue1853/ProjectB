using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 에이전트(builder 등)가 MCP 로 직접 Play 하는 시험이 사람 기록 폴더(Logs/playrec)에 섞이지 않게 하는 환경 플래그.
/// `Tools/u.sh` 가 호출될 때마다 `Logs/.agent_active` 를 갱신한다. 플레이 시작 때 이 파일이 최근(30분 안)이고 기록 출처가 미리 정해지지
/// 않았으면 출처를 "test"(Logs/playrec_test)로 한다. 사람이 에디터에서 직접 Play 하면 파일이 오래돼 그대로 "human".
/// </summary>
public static class PlayRecAgentFlag
{
    public const string FileName = ".agent_active";
    public const double WindowSeconds = 1800.0;

    /// <summary>마지막 갱신이 창 안이면 참(순수)</summary>
    public static bool IsActive(DateTime lastWriteUtc, DateTime nowUtc)
    {
        double age = (nowUtc - lastWriteUtc).TotalSeconds;
        return age >= -5.0 && age <= WindowSeconds;
    }

    /// <summary>에디터 전용: 플래그 파일이 최근인가</summary>
    public static bool IsActiveNow()
    {
#if UNITY_EDITOR
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", FileName);
        return File.Exists(path) && IsActive(File.GetLastWriteTimeUtc(path), DateTime.UtcNow);
#else
        return false;
#endif
    }
}
