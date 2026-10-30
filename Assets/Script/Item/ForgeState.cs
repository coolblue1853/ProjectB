/// <summary>
/// 대장간 공용 진행(FloorCoreContent 4.7): 해금 비트 <c>forge.unlock</c> 과 천장 보조 <c>forge.pity</c>. 공용 세이브 파일, 없으면 0.
/// 비트: 1 = +11~+12, 2 = +13, 4 = +14, 8 = +15. 4층계 구역을 열 때 <see cref="Unlock"/> 으로 켠다.
/// </summary>
public static class ForgeState
{
    public const int BitLv12 = 1;
    public const int BitLv13 = 2;
    public const int BitLv14 = 4;
    public const int BitLv15 = 8;
    public const int SingleBitFrom = 1; // 정밀 단조는 비트 1 부터 쓴다(임시 가정, OpenQuestions)

    const string UnlockKey = "forge.unlock";
    const string PityKey = "forge.pity";

    public static int UnlockMask
    {
        get { return ES3.KeyExists(UnlockKey, SaveSlots.SharedFile) ? ES3.Load<int>(UnlockKey, SaveSlots.SharedFile) : 0; }
        set { ES3.Save(UnlockKey, value, SaveSlots.SharedFile); }
    }

    /// <summary>천장 보조용 +11 이상 연속 실패 수</summary>
    public static int Pity
    {
        get { return ES3.KeyExists(PityKey, SaveSlots.SharedFile) ? ES3.Load<int>(PityKey, SaveSlots.SharedFile) : 0; }
        set { ES3.Save(PityKey, value, SaveSlots.SharedFile); }
    }

    public static void Unlock(int bits)
    {
        UnlockMask |= bits;
    }

    /// <summary>목표 단계(+11~+15)에 필요한 비트. 범위 밖은 0</summary>
    public static int BitForTarget(int target)
    {
        switch (target)
        {
            case 11:
            case 12:
                return BitLv12;
            case 13:
                return BitLv13;
            case 14:
                return BitLv14;
            case 15:
                return BitLv15;
            default:
                return 0;
        }
    }

    /// <summary>현재 단계 level 에서 한 단계 올리는 것이 해금됐는가(고급 구간 한정)</summary>
    public static bool IsUnlocked(int level, int mask)
    {
        int bit = BitForTarget(level + 1);
        return bit != 0 && (mask & bit) != 0;
    }

    public static bool IsUnlocked(int level)
    {
        return IsUnlocked(level, UnlockMask);
    }
}
