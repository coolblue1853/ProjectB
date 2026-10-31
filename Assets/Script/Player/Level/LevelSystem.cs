using System;
using UnityEngine;

/// <summary>
/// 플레이어 레벨·EXP 상태 (ClassicMapleDesign.md). 처치 EXP, 레벨업 자동 성장, 사망 페널티, 세이브(Datas.level/exp).
/// 성장은 기존 스탯 체계에 얹는다: 공격=BasicDmg, 방어=Def 는 PlayerStats Add 모디파이어(Id "Level"), 최대 HP 는 PlayerHealthManager.fullHP 증가분.
/// 시작 기준(HP·공격·방어)은 지금 기본값을 그대로 두고 증가분만 더한다.
/// </summary>
public static class LevelSystem
{
    const string ModId = "Level";

    public static int Level { get; private set; } = 1;
    public static int Exp { get; private set; }

    /// <summary>레벨·EXP 가 바뀜 (HUD 갱신)</summary>
    public static event Action Changed;
    /// <summary>레벨이 오름(새 레벨)</summary>
    public static event Action<int> LeveledUp;

    static int appliedLevel = 1;          // HP 에 반영된 레벨
    static bool statsApplied;
    static bool pendingLevelUpFx;
    static Datas boundDatas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Level = 1;
        Exp = 0;
        appliedLevel = 1;
        statsApplied = false;
        pendingLevelUpFx = false;
        boundDatas = null;
        Changed = null;
        LeveledUp = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var go = new GameObject("[LevelSystem]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<LevelDriver>();
    }

    /// <summary>몹 처치 EXP (EnemyHealth.TryDie 가 한 번만 부른다 — 어떤 피해 경로로 죽어도 지급)</summary>
    public static void OnEnemyKilled(EnemyHealth e)
    {
        if (e == null)
        {
            return;
        }
        MonsterTable.Row r = MonsterTable.Get(e.name);
        int baseExp = r != null && r.exp.HasValue ? r.exp.Value : 0;
        GainExp(LevelRules.KillExp(baseExp, e.IsElite, e.isBossMob), e.name);
    }

    /// <summary>EXP 지급 (처치·퀘스트 공용)</summary>
    public static void GainExp(int amount, string source = null)
    {
        if (amount <= 0)
        {
            return;
        }
        int level = Level;
        int exp = Exp;
        int ups = LevelRules.AddExp(ref level, ref exp, amount);
        Level = level;
        Exp = exp;
        GameLog.Info(GameLog.Cat.Player, "EXP +" + amount + " (" + source + ") → Lv" + Level + " " + Exp + "/" + LevelRules.ExpToNext(Level));
        if (ups > 0)
        {
            pendingLevelUpFx = true;
            ApplyStats(true);
            LeveledUp?.Invoke(Level);
            if (SaveManager.instance != null)
            {
                SaveManager.instance.DataSave();
            }
        }
        Changed?.Invoke();
    }

    /// <summary>사망 시 현재 레벨 EXP 5% 감소 (PlayerHealthManager.Die)</summary>
    public static void ApplyDeathPenalty()
    {
        int before = Exp;
        Exp = LevelRules.ExpAfterDeath(Exp);
        GameLog.Info(GameLog.Cat.Player, "사망 EXP 손실 " + before + " → " + Exp + " (Lv" + Level + ")");
        Changed?.Invoke();
    }

    /// <summary>프로브·세이브 로드용 직접 설정 (레벨업 연출 없음)</summary>
    public static void SetState(int level, int exp)
    {
        Level = Mathf.Clamp(level, 1, LevelRules.MaxLevel);
        int cap = LevelRules.ExpToNext(Level);
        Exp = Level >= LevelRules.MaxLevel ? 0 : Mathf.Clamp(exp, 0, Mathf.Max(0, cap - 1));
        ApplyStats(false);
        Changed?.Invoke();
    }

    public static void WriteTo(Datas d)
    {
        if (d == null)
        {
            return;
        }
        d.level = Level;
        d.exp = Exp;
    }

    /// <summary>세이브에서 읽는다. level 이 0 이하(옛 세이브/신규)면 Lv1, EXP 0</summary>
    public static void LoadFrom(Datas d)
    {
        boundDatas = d;
        int lv = d != null && d.level >= 1 ? d.level : 1;
        SetState(lv, d != null ? d.exp : 0);
    }

    /// <summary>레벨 성장분을 스탯에 반영한다. HP 매니저가 준비 전이면 HP 만 보류(다음 프레임 재시도)</summary>
    static void ApplyStats(bool fullHeal)
    {
        PlayerStats.RemoveById(ModId);
        int atk = LevelRules.AtkBonus(Level);
        int def = LevelRules.DefBonus(Level);
        if (atk > 0)
        {
            PlayerStats.Add(new StatModifier(StatId.BasicDmg, ModType.Add, atk) { Id = ModId, Owner = "Level" });
        }
        if (def > 0)
        {
            PlayerStats.Add(new StatModifier(StatId.Def, ModType.Add, def) { Id = ModId, Owner = "Level" });
        }
        ApplyHp(fullHeal);
    }

    static void ApplyHp(bool fullHeal)
    {
        PlayerHealthManager hp = PlayerHealthManager.Instance;
        if (hp == null || !hp.Started)
        {
            statsApplied = false;
            return;
        }
        int delta = LevelRules.HpBonus(Level) - LevelRules.HpBonus(appliedLevel);
        if (delta != 0)
        {
            hp.AddLevelMaxHp(delta);
        }
        appliedLevel = Level;
        statsApplied = true;
        if (fullHeal)
        {
            hp.HpUp(hp.fullHP - hp.nowHp);
        }
    }

    /// <summary>레벨업 연출: 작은 텍스트 + 기존 효과음 재사용 (새 이펙트 없음)</summary>
    static void PlayLevelUpFx()
    {
        pendingLevelUpFx = false;
        HitFeedback.SpawnText(null, PlayerController.instance.transform.position + Vector3.up * 1.2f,
            Loc.T("LEVEL UP") + " Lv" + Level, HitFeedback.TextKind.Gold);
        Sfx.Play(SfxId.Victory, 0.6f);
    }

    sealed class LevelDriver : MonoBehaviour
    {
        void Update()
        {
            // 세이브가 (다시) 만들어지면 레벨을 읽는다
            SaveManager sm = SaveManager.instance;
            if (sm != null && !ReferenceEquals(sm.datas, boundDatas))
            {
                LoadFrom(sm.datas);
            }
            if (!statsApplied)
            {
                ApplyHp(false); // HP 매니저가 늦게 준비된 경우
            }
            if (pendingLevelUpFx && PlayerController.instance != null)
            {
                PlayLevelUpFx();
            }
        }
    }
}
