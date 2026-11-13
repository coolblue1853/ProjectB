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

    // 아래 캐시는 id·entry·p3 가 로드 때만 채워진다는 전제(로드 후 변경 금지)
    string _refundKey;
    float _refundCd = float.NaN;
    bool _refundCdParsed;

    /// <summary>처치 환급 내부 쿨 키. 처음 한 번만 만들어 캐시(처치마다 문자열 생성 방지)</summary>
    public string RefundKey => _refundKey ?? (_refundKey = "Refund:" + id + "#" + entry);

    /// <summary>p3 내부 쿨(초). 비었거나 파싱 불가면 NaN. 처음 한 번만 파싱해 캐시</summary>
    public float RefundCooldown
    {
        get
        {
            if (!_refundCdParsed)
            {
                _refundCdParsed = true;
                _refundCd = !string.IsNullOrEmpty(p3) && float.TryParse(p3, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float cd) ? cd : float.NaN;
            }
            return _refundCd;
        }
    }
}
