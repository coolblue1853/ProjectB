using System;

/// <summary>
/// 스킬트리 서비스 (WeaponSkillProgression.md V.2~V.4·V.6). 투자·환불·리스펙·집계(Mod)와 ES3 skillTree 저장.
/// 포인트는 저장하지 않는다: earned = 타입 Lv-1 / 플레이어 Lv-1, available = earned - spent.
/// </summary>
public static class SkillTreeService
{
    public const string KeySave = "skillTree";

    public static SkillTreeData Data { get; private set; }

    private static bool _dirty;
    private static bool _needValidate;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Data = null;
        _dirty = false;
        _needValidate = false;
    }

    /// <summary>세이브 로드(SaveManager.DataLoad, 레벨·타입 숙련 로드 뒤)</summary>
    public static void Load()
    {
        Data = new SkillTreeData();
        try
        {
            if (ES3.KeyExists(KeySave))
            {
                Data.LoadFrom(ES3.Load<SkillTreeSave>(KeySave));
            }
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "스킬트리 로드 실패, 빈 상태로 시작: " + e.Message);
        }
        _needValidate = true;
        EnsureValid();
    }

    /// <summary>spent &gt; earned 면 무료 초기화. 타입 숙련 이행이 끝나기 전에는 earned 를 모르니 보류한다</summary>
    private static void EnsureValid()
    {
        if (!_needValidate || Data == null || !TypeMasteryService.Ready)
        {
            return;
        }
        _needValidate = false;
        int fixedTrees = Data.FixOverspent(EarnedPoints);
        if (fixedTrees > 0)
        {
            _dirty = true;
            GameLog.Warn(GameLog.Cat.Save, "스킬트리 포인트 초과 투자 " + fixedTrees + "개 트리 자동 초기화");
        }
    }

    /// <summary>SaveManager.DataSave 가 부른다. 바뀐 게 있을 때만 쓴다</summary>
    public static void Save()
    {
        if (Data == null || !_dirty)
        {
            return;
        }
        try
        {
            ES3.Save(KeySave, Data.ToSave());
            _dirty = false;
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "스킬트리 저장 실패: " + e.Message);
        }
    }

    public static string TreeOf(WeaponKind kind)
    {
        return kind == WeaponKind.None ? null : kind.ToString();
    }

    /// <summary>트리 획득 포인트(파생값)</summary>
    public static int EarnedPoints(string tree)
    {
        if (tree == SkillTreeRules.CommonTree)
        {
            return SkillTreeRules.CommonEarnedPoints(LevelSystem.Level);
        }
        WeaponKind kind;
        if (!TypeMasteryData.IsValidKind(tree) || !Enum.TryParse(tree, out kind))
        {
            return 0;
        }
        return TypeMasteryRules.EarnedPoints(TypeMasteryService.LevelOf(kind));
    }

    public static int Spent(string tree)
    {
        return Data != null ? Data.Spent(tree) : 0;
    }

    public static int Available(string tree)
    {
        EnsureValid();
        return EarnedPoints(tree) - Spent(tree);
    }

    public static int LevelOf(string tree, string nodeId)
    {
        return Data != null ? Data.GetLevel(tree, nodeId) : 0;
    }

    /// <summary>노드 1레벨 투자</summary>
    public static InvestResult Invest(string tree, string nodeId)
    {
        EnsureValid();
        if (Data == null)
        {
            return InvestResult.UnknownNode;
        }
        InvestResult r = Data.Invest(tree, nodeId, EarnedPoints(tree));
        if (r == InvestResult.Ok)
        {
            _dirty = true;
            SaveNow();
        }
        return r;
    }

    /// <summary>다음 리스펙 골드(첫 1회 무료)</summary>
    public static int RespecCost(string tree)
    {
        return Data != null ? Data.RespecCostNow(tree) : 0;
    }

    /// <summary>트리 전체 초기화. 골드는 DatabaseManager.money 에서 뺀다</summary>
    public static RespecResult Respec(string tree)
    {
        EnsureValid();
        if (Data == null || Data.Spent(tree) <= 0)
        {
            return RespecResult.Nothing;
        }
        int cost = Data.RespecCostNow(tree);
        if (cost > DatabaseManager.money)
        {
            return RespecResult.NoGold;
        }
        DatabaseManager.money -= cost;
        Data.ClearForRespec(tree);
        _dirty = true;
        SaveNow();
        return RespecResult.Ok;
    }

    private static void SaveNow()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.DataSave();
        }
        else
        {
            Save();
        }
    }

    /// <summary>
    /// 집계: 공용 + kind 타입 트리 효과(%), 합산 상한 적용. 쿨·스태미나 소모는 감소량(양수).
    /// kind 가 None 이면 공용만. 극의(Ultimate)는 타입 트리만(0/1).
    /// </summary>
    public static float Mod(TreeStat stat, WeaponKind kind)
    {
        EnsureValid();
        return Data != null ? Data.Mod(stat, TreeOf(kind)) : 0f;
    }
}
