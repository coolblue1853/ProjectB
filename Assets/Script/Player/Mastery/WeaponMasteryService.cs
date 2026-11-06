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
        AddExp(w.MasteryKey, amount);
    }

    public static void AddExp(string key, int amount)
    {
        if (Data == null || string.IsNullOrEmpty(key))
        {
            return;
        }
        bool up = Data.Add(key, amount);
        _dirty = true;
        if (up)
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

    public static int LevelOf(string key)
    {
        return Data != null ? Data.GetLevel(key) : WeaponMasteryRules.MaxLevel;
    }

    /// <summary>슬롯 스킬이 열려 있는가. 스킬 없는 슬롯·데이터 없음은 열림</summary>
    public static bool IsSlotUnlocked(Weapon w, int slot)
    {
        return Data == null || w == null || UnlockLevelFor(w, slot) <= LevelOf(w.MasteryKey);
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
