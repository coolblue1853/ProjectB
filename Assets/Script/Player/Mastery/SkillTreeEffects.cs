using UnityEngine;

/// <summary>
/// 스킬트리 효과 연결 (WeaponSkillProgression.md V.2·V.3·V.8 P3·P4).
/// 장착 무기 타입이 바뀌거나 트리가 바뀌면 집계(SkillTreeService.Mod)를 다시 읽어 캐시하고,
/// 치명·공속·이동·방어는 PlayerStats 모디파이어(그룹 "SkillTree"), 최대 HP 는 fullHP 증가분으로 얹는다.
/// 데미지·쿨·스태미나는 캐시 값을 호출 지점(DamageObject·SkillEffectService)이 읽는다(할당 없음).
/// </summary>
public static class SkillTreeEffects
{
    const string Group = "SkillTree";
    /// <summary>극의 기본 효과: 평타 마지막 타 데미지 배율</summary>
    public const float UltimateFinalHitMul = 1.15f;
    /// <summary>극의 Staff·Tome: 발사체(전 타격) 데미지 배율</summary>
    public const float UltimateCasterMul = 1.10f;

    static WeaponKind _kind = WeaponKind.None;
    static float _atk, _skl, _cd, _stamina, _regen, _hp, _commonCd, _commonStamina;
    static bool _ultimate;
    static bool _hpDirty;
    static int _hpBonus;
    static PlayerHealthManager _hpTarget;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _kind = WeaponKind.None;
        _atk = _skl = _cd = _stamina = _regen = _hp = _commonCd = _commonStamina = 0f;
        _ultimate = false;
        _hpDirty = false;
        _hpBonus = 0;
        _hpTarget = null;
        SkillTreeService.Changed -= OnTreeChanged;
        SkillTreeService.Changed += OnTreeChanged;
    }

    public static WeaponKind Kind
    {
        get { return _kind; }
    }

    public static float AttackDamagePct
    {
        get { return _atk; }
    }

    public static float SkillDamagePct
    {
        get { return _skl; }
    }

    public static bool Ultimate
    {
        get { return _ultimate; }
    }

    /// <summary>스태미나 회복 간격 나눗셈 값(1 = 그대로)</summary>
    public static float StaminaRegenMul
    {
        get { return 1f + _regen / 100f; }
    }

    /// <summary>해당 종류 스킬의 쿨다운 감소 %(장착 무기 타입과 같을 때만, 공용 포함)</summary>
    public static float CooldownPct(WeaponKind kind)
    {
        return kind == _kind || kind == WeaponKind.None ? _cd : _commonCd;
    }

    public static float StaminaCostPct(WeaponKind kind)
    {
        return kind == _kind || kind == WeaponKind.None ? _stamina : _commonStamina;
    }

    /// <summary>타격 하나의 트리 데미지 배율(평타=평타 단련, 스킬=기술 단련, 극의 포함). 효과 없으면 1</summary>
    public static float DamageMul(bool isSkill)
    {
        float mul = 1f + (isSkill ? _skl : _atk) / 100f;
        if (!_ultimate)
        {
            return mul;
        }
        if (_kind == WeaponKind.Staff || _kind == WeaponKind.Tome)
        {
            return mul * UltimateCasterMul;
        }
        Weapon w = Weapon.Equipped;
        if (!isSkill && w != null && w.maxComboCount > 0 && w.nowConboCount >= w.maxComboCount)
        {
            return mul * UltimateFinalHitMul;
        }
        return mul;
    }

    static void OnTreeChanged()
    {
        Refresh(_kind);
    }

    /// <summary>집계를 다시 읽고 PlayerStats 모디파이어를 갈아 끼운다</summary>
    public static void Refresh(WeaponKind kind)
    {
        _kind = kind;
        _atk = SkillTreeService.Mod(TreeStat.AttackDamage, kind);
        _skl = SkillTreeService.Mod(TreeStat.SkillDamage, kind);
        _cd = SkillTreeService.Mod(TreeStat.SkillCooldown, kind);
        _stamina = SkillTreeService.Mod(TreeStat.StaminaCost, kind);
        _commonCd = SkillTreeService.Mod(TreeStat.SkillCooldown, WeaponKind.None);
        _commonStamina = SkillTreeService.Mod(TreeStat.StaminaCost, WeaponKind.None);
        _regen = SkillTreeService.Mod(TreeStat.StaminaRegen, kind);
        _hp = SkillTreeService.Mod(TreeStat.MaxHp, kind);
        _ultimate = SkillTreeService.Mod(TreeStat.Ultimate, kind) > 0f;
        PlayerStats.RemoveByGroup(Group);
        AddMod(StatId.CritRate, ModType.Add, SkillTreeService.Mod(TreeStat.CritChance, kind));
        AddMod(StatId.CritDmg, ModType.Add, SkillTreeService.Mod(TreeStat.CritDamage, kind));
        AddMod(StatId.AttackSpeed, ModType.Add, SkillTreeService.Mod(TreeStat.AttackSpeed, kind));
        AddMod(StatId.MoveSpeed, ModType.Add, SkillTreeService.Mod(TreeStat.MoveSpeed, kind));
        AddMod(StatId.Def, ModType.Percent, SkillTreeService.Mod(TreeStat.Defense, kind));
        _hpDirty = true;
    }

    static void AddMod(StatId stat, ModType type, float value)
    {
        if (value <= 0f)
        {
            return;
        }
        PlayerStats.Add(new StatModifier(stat, type, value) { Group = Group, Owner = "SkillTree" });
    }

    /// <summary>최대 HP 증가분 동기화(PlayerHealthManager 가 준비된 뒤). 프레임마다 불러도 가볍다</summary>
    public static void SyncMaxHp()
    {
        PlayerHealthManager phm = PlayerHealthManager.Instance;
        if (phm == null || !phm.Started)
        {
            return;
        }
        if (!ReferenceEquals(phm, _hpTarget))
        {
            _hpTarget = phm;
            _hpBonus = 0;
            _hpDirty = true;
        }
        if (!_hpDirty)
        {
            return;
        }
        _hpDirty = false;
        int basis = phm.fullHP - _hpBonus;
        int target = Mathf.RoundToInt(basis * _hp / 100f);
        int delta = target - _hpBonus;
        if (delta != 0)
        {
            _hpBonus = target;
            phm.ApplyEnhancementMaxHp(delta);
        }
    }

    /// <summary>현재 장착 무기 타입(DB 미로드면 None)</summary>
    public static WeaponKind CurrentKind()
    {
        Weapon w = Weapon.Equipped;
        return w == null ? WeaponKind.None : WeaponMasteryService.KindOf(w.MasteryKey);
    }
}

/// <summary>무기 교체·DB 로드 지연·최대 HP 동기화를 감시하는 상주 구동기</summary>
public sealed class SkillTreeEffectsDriver : MonoBehaviour
{
    static SkillTreeEffectsDriver _instance;
    Weapon _last;
    bool _applied;
    int _retry;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Spawn()
    {
        if (_instance != null)
        {
            return; // 씬 로드마다 중복 생성 방지
        }
        var go = new GameObject("[SkillTreeEffects]") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<SkillTreeEffectsDriver>();
    }

    void Update()
    {
        Weapon w = Weapon.Equipped;
        bool changed = !ReferenceEquals(w, _last) || !_applied;
        if (!changed && w != null && ++_retry >= 30)
        {
            _retry = 0;
            changed = WeaponMasteryService.DbReady && SkillTreeEffects.CurrentKind() != SkillTreeEffects.Kind; // 무기 타입을 DB 로드 뒤에야 알 수 있었던 경우
        }
        if (changed && (w == null || WeaponMasteryService.DbReady))
        {
            _last = w;
            _applied = true;
            SkillTreeEffects.Refresh(SkillTreeEffects.CurrentKind());
        }
        SkillTreeEffects.SyncMaxHp();
    }
}
