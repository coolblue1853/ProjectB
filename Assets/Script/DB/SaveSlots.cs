using System;
using UnityEngine;

/// <summary>
/// 세이브 슬롯·백업·버전 (AssetIntegrationPlan 6절 6, Easy Save 3).
/// - 모든 ES3 호출은 기본 파일을 쓰므로, 씬 로드 전에 ES3Settings.defaultSettings.path 를 슬롯 파일로 바꾼다.
///   슬롯 1 = 기존 SaveFile.es3(호환), 2·3 = SaveSlot2/3.es3. 슬롯 번호는 PlayerPrefs "pb.saveSlot".
/// - 시작할 때 파일을 끝까지 읽어 본다. 정상이면 .bak 을 만들고, 읽기 예외(손상)면 .bak 에서 복구한다.
/// - saveVersion 키가 없으면 CurrentVersion 을 기록한다(이후 데이터 마이그레이션 기준점).
/// - 슬롯 선택은 타이틀 화면(좌우). 실행 중 전환은 씬 재로드 대신 재시작(SwitchAndRestart) — 싱글턴·정적 상태가 섞이지 않게(A-202).
/// </summary>
public static class SaveSlots
{
    public const int Count = 3;
    public const int CurrentVersion = 1;
    public const string VersionKey = "saveVersion";
    /// <summary>슬롯과 무관한 설정(볼륨 등) 파일</summary>
    public const string SharedFile = "SaveFile.es3";
    const string PrefKey = "pb.saveSlot";

    public static int Current { get; private set; } = 1;
    /// <summary>시작 시 손상을 감지해 백업에서 복구했는지(프로브·로그용)</summary>
    public static bool RestoredFromBackup { get; private set; }

    public static string PathOf(int slot) => slot <= 1 ? SharedFile : "SaveSlot" + slot + ".es3";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Current = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 1), 1, Count);
        // 에디터에서 ES3Defaults 에셋 값이 남지 않도록 매번 명시한다(슬롯 1 이면 원래 경로)
        ES3Settings.defaultSettings.path = PathOf(Current);
        RestoredFromBackup = false;
        VerifyAndBackup(PathOf(Current));
    }

    /// <summary>다음 실행부터 쓸 슬롯을 정한다(실행 중 전환은 SwitchAndRestart).</summary>
    public static void SetNextSlot(int slot)
    {
        PlayerPrefs.SetInt(PrefKey, Mathf.Clamp(slot, 1, Count));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 실행 중 슬롯 전환 (A-202): 정적 상태(DatabaseManager 등)와 DontDestroyOnLoad 싱글턴이 많아 씬 재로드로는 슬롯이 섞이므로,
    /// 다음 슬롯을 기록하고 게임을 다시 연다. 빌드는 실행 파일을 새로 띄우고 종료, 에디터는 플레이 모드를 끈다(다시 재생하면 새 슬롯).
    /// </summary>
    public static void SwitchAndRestart(int slot)
    {
        SetNextSlot(slot);
        GameLog.Info(GameLog.Cat.Player, "세이브 슬롯 전환 → " + slot + " (재시작)");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        try
        {
            string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            System.Diagnostics.Process.Start(exe);
        }
        catch (Exception e)
        {
            Debug.LogWarning("재시작 실패, 다음 실행부터 슬롯 " + slot + ": " + e.Message);
        }
        Application.Quit();
#endif
    }

    static void VerifyAndBackup(string path)
    {
        if (!ES3.FileExists(path))
        {
            return;
        }
        try
        {
            foreach (string key in ES3.GetKeys(path))
            {
                ES3.Load<object>(key, path); // 값까지 실제로 읽어 본다(키 목록만으로는 값 중간 손상을 못 잡아 손상본으로 백업을 덮었었다)
            }
            if (!ES3.KeyExists(VersionKey, path))
            {
                ES3.Save(VersionKey, CurrentVersion, path);
            }
            ES3.CreateBackup(path);
        }
        catch (Exception e)
        {
            RestoredFromBackup = ES3.RestoreBackup(path);
            Debug.LogWarning("[PB][Save] 세이브 파일 손상(" + path + "): " + e.Message + (RestoredFromBackup ? " → 백업에서 복구" : " → 백업 없음"));
        }
    }

    /// <summary>슬롯 요약(타이틀 슬롯 UI 용): 비었으면 "EMPTY", 읽기 실패면 "DAMAGED"</summary>
    public static string Summary(int slot)
    {
        string path = PathOf(slot);
        if (!ES3.FileExists(path))
        {
            return "EMPTY";
        }
        try
        {
            int rank = ES3.KeyExists(AscentLoad.RankKey, path) ? ES3.Load<int>(AscentLoad.RankKey, path) : 0;
            DateTime t = ES3.GetTimestamp(path).ToLocalTime();
            return Loc.T(AscentLoad.RankName(rank)).ToUpperInvariant() + " " + Loc.T("EXPLORER") + "  " + t.ToString("MM-dd HH:mm");
        }
        catch (Exception)
        {
            return "DAMAGED";
        }
    }
}
