/// <summary>효과 행 종류 (EquipEffectDesign §2.2·2.3)</summary>
public enum EffectKind
{
    Mod,
    Rule,
}

/// <summary>수치 조정 op (EquipEffectDesign §2.2). 이름은 EquipEffects.csv 의 op 열과 같다</summary>
public enum EffectOp
{
    None = 0,
    CooldownPct,
    DamagePct,
    RangePct,
    ProjSpeedPct,
    DurationPct,
    ProjectileAdd,
    HitCountAdd,
    PierceAdd,
    SummonAdd,
    StaminaPct,
    ProcChance,
    CastCharge,
}

/// <summary>규칙 대체 핸들러 이름 (EquipEffectDesign §2.3). 핸들러 구현은 P3~P5</summary>
public enum EffectRule
{
    None = 0,
    FireMode,
    Homing,
    Retarget,
    OnHitSpawn,
    Pierce,
    Return,
    Chain,
    OnKillCast,
    Proc,
    CooldownRefund,
    Replace,
}

/// <summary>EquipEffects.csv 한 행. 순수 데이터</summary>
public sealed class EffectEntry
{
    public string id;
    public int entry;
    public EffectKind kind;
    public string selector;
    public EffectOp op;
    public float value;
    public string cond;
    public string group;
    public EffectRule rule;
    public string p1;
    public string p2;
    public string p3;
    public string nameKey;
    public string descKey;
    public string minGrade;
}
