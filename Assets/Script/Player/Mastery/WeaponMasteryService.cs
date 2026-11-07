using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 숙련도 서비스 (WeaponSkillProgression.md). EXP 획득 시 장착 무기에 같은 양을 쌓고, 숙련 레벨로 스킬 슬롯 해금을 판정한다.
/// ES3 신규 키만 쓴다: weaponMastery(WeaponMasterySave), weaponMasteryMigrated(bool). 기존 Datas 는 건드리지 않는다.
/// 데이터가 아직 없으면(EditMode·로드 전) 잠금 판정은 열림으로 본다.
/// </summary>
public static class WeaponMasteryService
{
    public const string KeySave = "weaponMastery";
    public const string KeyMigrated = "weaponMasteryMigrated";

    public static WeaponMasteryData Data { get; private set; }
    /// <summary>(무기 키, 새 숙련 레벨) — 레벨 상승 알림용(P3)</summary>
    public static event Action<string, int> LevelUp;

    static bool _dirty;
    static AttackManager _attackManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Data = null;
        _dirty = false;
        _attackManager = null;
        LevelUp = null;
    }

    /// <summary>세이브 로드(SaveManager.DataLoad 끝). hadSaveFile=false 면 새 게임이라 마이그레이션 없음</summary>
    public static void Load(bool hadSaveFile, Datas d)
    {
        Data = new WeaponMasteryData();
        try
        {
            bool hasKey = ES3.KeyExists(KeySave);
            if (hasKey)
            {
                Data.LoadFrom(ES3.Load<WeaponMasterySave>(KeySave));
            }
            bool migrated = ES3.KeyExists(KeyMigrated) && ES3.Load<bool>(KeyMigrated);
            if (!migrated)
            {
                if (hadSaveFile && !hasKey)
                {
                    MigrateLegacy(d);
                }
                ES3.Save(KeyMigrated, true);
                _dirty = true;
                Save();
            }
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "무기 숙련 로드 실패, 빈 상태로 시작: " + e.Message);
        }
    }

    /// <summary>구세이브: 이미 스킬을 쓰던 무기는 잠기지 않게 만렙 EXP 로 시작(현재 인벤·창고·장착 무기)</summary>
    static void MigrateLegacy(Datas d)
    {
        if (d == null)
        {
            return;
        }
        List<string> keys = WeaponMasteryData.CollectWeaponKeys(d.invenItem, d.chestItem, d.equipGear, IsWeaponKey);
        foreach (string k in keys)
        {
            Data.RaiseTo(k, WeaponMasteryRules.LegacyStartExp);
        }
        GameLog.Info(GameLog.Cat.Save, "무기 숙련 마이그레이션 " + keys.Count + "종 → Lv" + WeaponMasteryRules.MaxLevel);
    }

    static bool IsWeaponKey(string name)
    {
        return Resources.Load<GameObject>("Weapon/" + name) != null;
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
            GameLog.Warn(GameLog.Cat.Save, "무기 숙련 저장 실패: " + e.Message);
        }
    }

    /// <summary>EXP 획득 직후 호출. 지금 장착한 무기에 같은 양을 쌓는다</summary>
    public static void AddForEquipped(int amount)
    {
        if (Data == null || amount <= 0)
        {
            return;
        }
        if (_attackManager == null)
        {
            _attackManager = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
        }
        Weapon w = _attackManager != null ? _attackManager.equipWeapon : null;
        if (w == null)
        {
            return;
        }
        string key = w.MasteryKey;
        WeaponKind kind = KindOf(key);
        if (WeaponMasteryRules.PerWeaponMastery)
        {
            if (!Data.Has(key))
            {
                Data.RaiseTo(key, WeaponMasteryRules.StartExp(StartMasteryOf(key))); // 첫 획득 시 시작 숙련(StartMastery)
            }
            bool catchUp = kind != WeaponKind.None
                && Data.AnyAtLevel(WeaponMasteryRules.CatchUpSourceLevel, k => k != key && KindOf(k) == kind);
            AddExp(key, amount * WeaponMasteryRules.GainMultiplier(Data.GetLevel(key), catchUp));
        }
        else
        {
            AddExp(key, amount); // 키별 누적은 유지(불 스테이지 개조 대비). 따라잡기·StartMastery 는 꺼진다
        }
        AddTypeExp(key, kind, amount);
    }

    /// <summary>타입 EXP 누적(항상). 타입 숙련이 해금 기준일 때만 레벨업 알림·저장을 건다</summary>
    static void AddTypeExp(string key, WeaponKind kind, int amount)
    {
        int lv = TypeMasteryService.AddExp(kind, amount);
        if (lv <= 0)
        {
            return;
        }
        GameLog.Info(GameLog.Cat.Player, "타입 숙련 " + kind + " Lv" + lv);
        if (!WeaponMasteryRules.PerWeaponMastery)
        {
            LevelUp?.Invoke(key, lv);
        }
        if (SaveManager.instance != null)
        {
            SaveManager.instance.DataSave();
        }
    }

    static readonly Dictionary<string, WeaponKind> KindCache = new Dictionary<string, WeaponKind>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetKindCache()
    {
        KindCache.Clear();
    }

    /// <summary>아이템 DB 가 로드돼 타입을 알 수 있는가</summary>
    public static bool DbReady
    {
        get { return DatabaseManager.instance != null && DatabaseManager.instance.Items != null; }
    }

    public static WeaponKind KindOf(string key)
    {
        WeaponKind kind;
        if (!KindCache.TryGetValue(key, out kind))
        {
            ItemData it = FindItem(key); // 구 무기는 프리팹 _kind 가 비어 있어 ItemSheet WeaponType 을 기준으로 한다
            if (it == null)
            {
                return WeaponKind.None; // DB 미로드: 캐시하지 않는다
            }
            WeaponGimmickRules.TryParseKind(it.weaponType, out kind);
            KindCache[key] = kind;
        }
        return kind;
    }

    /// <summary>ItemSheet StartMastery 컬럼(빈칸 = 1)</summary>
    static int StartMasteryOf(string key)
    {
        ItemData it = FindItem(key);
        return it != null ? it.startMastery : 1;
    }

    static ItemData FindItem(string key)
    {
        if (DatabaseManager.instance != null && DatabaseManager.instance.Items != null)
        {
            foreach (ItemData it in DatabaseManager.instance.Items)
            {
                if (it.name == key)
                {
                    return it;
                }
            }
        }
        return null;
    }

    /// <summary>숙련 데미지 배율(Lv5 +5%). 데이터 미로드 = 1</summary>
    public static float DamageBonus(string key)
    {
        if (!WeaponMasteryRules.PerWeaponMastery)
        {
            return 1f; // 타입 숙련 모드: Lv5 +5% 무효
        }
        return Data != null ? WeaponMasteryRules.Bonus(Data.GetLevel(key)) : 1f;
    }

    public static void AddExp(string key, int amount)
    {
        if (Data == null || string.IsNullOrEmpty(key))
        {
            return;
        }
        bool up = Data.Add(key, amount);
        _dirty = true;
        if (up && WeaponMasteryRules.PerWeaponMastery)
        {
            int lv = Data.GetLevel(key);
            GameLog.Info(GameLog.Cat.Player, "무기 숙련 " + key + " Lv" + lv);
            LevelUp?.Invoke(key, lv);
            if (SaveManager.instance != null)
            {
                SaveManager.instance.DataSave();
            }
        }
    }

    /// <summary>해금 판정용 숙련 레벨. 타입 모드(기본) = 그 무기 타입 Lv(1~10), 개별 모드 = 키별 Lv(1~5). 데이터·타입 없음 = 최대(열림)</summary>
    public static int LevelOf(string key)
    {
        if (WeaponMasteryRules.PerWeaponMastery)
        {
            return Data != null ? Data.GetLevel(key) : WeaponMasteryRules.MaxLevel;
        }
        return TypeMasteryService.LevelOf(KindOf(key));
    }

    /// <summary>장착 무기 기준 유효 숙련 레벨 — AttackManager·SkillCooldown·툴팁은 이 함수만 쓴다(V.5)</summary>
    public static int EffectiveLevel(Weapon w)
    {
        return w == null ? TypeMasteryRules.MaxLevel : LevelOf(w.MasteryKey);
    }

    /// <summary>EffectiveLevel 에 대응하는 누적 EXP(툴팁용)</summary>
    public static int EffectiveExp(Weapon w)
    {
        if (w == null)
        {
            return 0;
        }
        return WeaponMasteryRules.PerWeaponMastery
            ? (Data != null ? Data.GetExp(w.MasteryKey) : 0)
            : TypeMasteryService.ExpOf(KindOf(w.MasteryKey));
    }

    /// <summary>슬롯 스킬이 열려 있는가. 스킬 없는 슬롯·데이터 없음은 열림</summary>
    public static bool IsSlotUnlocked(Weapon w, int slot)
    {
        return Data == null || w == null || UnlockLevelFor(w, slot) <= EffectiveLevel(w);
    }

    /// <summary>슬롯이 열리는 숙련 레벨(스킬 없음·무기 없음 = 1)</summary>
    public static int UnlockLevelFor(Weapon w, int slot)
    {
        if (w == null || w.skill == null || slot < 0 || slot >= w.skill.Length || w.skill[slot] == null)
        {
            return 1;
        }
        int count = 0;
        int rank = 0;
        for (int i = 0; i < w.skill.Length; i++)
        {
            if (w.skill[i] == null)
            {
                continue;
            }
            if (i == slot)
            {
                rank = count;
            }
            count++;
        }
        return WeaponMasteryRules.UnlockLevel(rank, count);
    }
}
