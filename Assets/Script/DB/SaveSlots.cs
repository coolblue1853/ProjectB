using System;
using UnityEngine;

/// <summary>
/// 세이브 슬롯·백업·버전 (AssetIntegrationPlan 6절 6, Easy Save 3).
/// - 모든 ES3 호출은 기본 파일을 쓰므로, 씬 로드 전에 ES3Settings.defaultSettings.path 를 슬롯 파일로 바꾼다.
///   슬롯 1 = 기존 SaveFile.es3(호환), 2·3 = SaveSlot2/3.es3. 슬롯 번호는 PlayerPrefs "pb.saveSlot".
/// - 시작할 때 파일을 끝까지 읽어 본다. 정상이면 .bak 을 만들고, 읽기 예외(손상)면 .bak 에서 복구한다.
/// - saveVersion 키가 없으면 CurrentVersion 을 기록한다(이후 데이터 마이그레이션 기준점).
/// ⚠️ 실행 중 슬롯 전환 UI 는 아직 없다: 여러 싱글턴이 AfterSceneLoad 에서 한 번만 로드해 씬 재로드로는 데이터가 섞인다(TechDebt A-202).
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

    /// <summary>다음 실행부터 쓸 슬롯을 정한다(실행 중 전환은 A-202 해결 후).</summary>
    public static void SetNextSlot(int slot)
    {
        PlayerPrefs.SetInt(PrefKey, Mathf.Clamp(slot, 1, Count));
        PlayerPrefs.Save();
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
            return AscentLoad.RankName(rank).ToUpperInvariant() + " EXPLORER  " + t.ToString("MM-dd HH:mm");
        }
        catch (Exception)
        {
            return "DAMAGED";
        }
    }
}
