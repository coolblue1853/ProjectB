using UnityEngine;

/// <summary>
/// 낚시 세이브(ES3 새 키만, 세이브 슬롯 파일). fishing.rod = 낚싯대 등급(int, 기본 0 = 낡은 낚싯대).
/// 등급을 올리는 경로(상점·제작)는 아직 없다(Q-436) — 키는 미리 읽어 둔다.
/// </summary>
public static class FishingSave
{
    public const string RodKey = "fishing.rod";

    public static int LoadRod()
    {
        try
        {
            return ES3.KeyExists(RodKey) ? ES3.Load<int>(RodKey) : 0;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PB][Fishing] 낚싯대 등급 읽기 실패, 기본값 사용: " + e.Message);
            return 0;
        }
    }

    public static void SaveRod(int grade)
    {
        ES3.Save(RodKey, grade);
    }
}
