using System.Collections.Generic;

/// <summary>한 무기 타입의 공격 중 이동 지정(스텝별 모드·Lunge 거리). 스텝 인덱스는 콤보 단계 - 1, 배열보다 길면 마지막 값.</summary>
public sealed class WeaponMoveProfile
{
    public readonly MoveWeightClass Class;
    readonly AttackMoveMode[] _modes;
    readonly float[] _dists;

    public WeaponMoveProfile(MoveWeightClass cls, AttackMoveMode[] modes, float[] dists)
    {
        Class = cls;
        _modes = modes;
        _dists = dists;
    }

    public AttackMoveMode ModeAt(int step)
    {
        return _modes[Index(step, _modes.Length)];
    }

    public float LungeDistAt(int step)
    {
        return _dists[Index(step, _dists.Length)];
    }

    static int Index(int step, int length)
    {
        int i = step - 1;
        return i < 0 ? 0 : i >= length ? length - 1 : i;
    }

    public int StepCount => _modes.Length > _dists.Length ? _modes.Length : _dists.Length;
}

/// <summary>
/// 무기 타입 → 이동 프로파일 표(CombatInputRedesign §3.2, CI-P1) + 단계별 적용 화이트리스트(CI-P2·P3).
/// 표는 모든 WeaponKind 를 담고, <see cref="EnabledKinds"/> 에 든 타입만 실제로 새 규칙을 쓴다. 롤백 = 화이트리스트에서 타입 한 줄 삭제.
/// 수치는 전부 임시(Q-CI1).
/// </summary>
public static class WeaponMoveProfiles
{
    static readonly AttackMoveMode Lunge = AttackMoveMode.LungeOnForward;
    static readonly AttackMoveMode InPlace = AttackMoveMode.InPlace;
    static readonly AttackMoveMode Free = AttackMoveMode.Free;

    static WeaponMoveProfile Lunges(MoveWeightClass cls, params float[] dists)
    {
        return new WeaponMoveProfile(cls, new[] { Lunge }, dists);
    }

    static WeaponMoveProfile Still(MoveWeightClass cls)
    {
        return new WeaponMoveProfile(cls, new[] { InPlace }, new[] { 0f });
    }

    static readonly Dictionary<WeaponKind, WeaponMoveProfile> Table = new Dictionary<WeaponKind, WeaponMoveProfile>
    {
        { WeaponKind.Sword, Lunges(MoveWeightClass.Standard, 0.9f, 0.9f, 1.3f) },
        { WeaponKind.Dagger, Lunges(MoveWeightClass.Light, 0.6f, 0.6f, 0.9f) },
        { WeaponKind.Dual, Lunges(MoveWeightClass.Light, 0.5f, 0.5f, 0.5f, 0.7f) },
        { WeaponKind.Knuckle, Lunges(MoveWeightClass.Light, 0.7f) },
        { WeaponKind.Claw, Lunges(MoveWeightClass.Standard, 0.8f) },
        { WeaponKind.Scythe, Lunges(MoveWeightClass.Standard, 0.8f) },
        { WeaponKind.Spear, Lunges(MoveWeightClass.Standard, 1.5f) },
        { WeaponKind.Whip, Lunges(MoveWeightClass.Light, 0.5f) },
        { WeaponKind.Flail, new WeaponMoveProfile(MoveWeightClass.Standard, new[] { Lunge, InPlace }, new[] { 0.6f, 0f }) },
        { WeaponKind.Axe, new WeaponMoveProfile(MoveWeightClass.Standard, new[] { InPlace, Lunge }, new[] { 0f, 0.8f }) },
        { WeaponKind.Tachi, new WeaponMoveProfile(MoveWeightClass.Standard, new[] { InPlace, Lunge }, new[] { 0f, 0.7f }) },
        { WeaponKind.Greatsword, Still(MoveWeightClass.Heavy) },
        { WeaponKind.Mace, Still(MoveWeightClass.Heavy) },
        { WeaponKind.Bulwark, Still(MoveWeightClass.Heavy) },
        { WeaponKind.Bow, Still(MoveWeightClass.Cast) },
        { WeaponKind.Crossbow, Still(MoveWeightClass.Cast) },
        { WeaponKind.Javelin, Still(MoveWeightClass.Cast) },
        { WeaponKind.Tome, Still(MoveWeightClass.Cast) },
        { WeaponKind.Staff, new WeaponMoveProfile(MoveWeightClass.Free, new[] { Free }, new[] { 0f }) },
        { WeaponKind.Adae, new WeaponMoveProfile(MoveWeightClass.Free, new[] { Free }, new[] { 0f }) },
    };

    /// <summary>같은 타입 안의 개별 무기 편차(아이템 키 = 프리팹 이름). 검방은 방패 무게로 짧게.</summary>
    static readonly Dictionary<string, WeaponMoveProfile> ByItemKey = new Dictionary<string, WeaponMoveProfile>
    {
        { "RustedSwordandShield", Lunges(MoveWeightClass.Standard, 0.6f, 0.6f, 0.9f) },
    };

    /// <summary>
    /// 새 규칙을 실제로 쓰는 타입(단계별 화이트리스트).
    /// CI-P2: 한손검·검방·단검·쌍검(Sword·Dagger·Dual). CI-P3: 대검·둔기·방패·활·석궁·투창·마도서(InPlace).
    /// CI-P4: 창·낫·클로·너클·채찍·철퇴·도끼·태도(Lunge/InPlace 혼합) + 지팡이·아대(Free 명시, 이동 잠금 없음). Weapon.CheckAttackWait 가 <see cref="LocksMove"/> 로 잠금을 건다.
    /// </summary>
    public static readonly HashSet<WeaponKind> EnabledKinds = new HashSet<WeaponKind>
    {
        WeaponKind.Sword, WeaponKind.Dagger, WeaponKind.Dual,
        WeaponKind.Greatsword, WeaponKind.Mace, WeaponKind.Bulwark,
        WeaponKind.Bow, WeaponKind.Crossbow, WeaponKind.Javelin, WeaponKind.Tome,
        WeaponKind.Spear, WeaponKind.Scythe, WeaponKind.Claw, WeaponKind.Knuckle, WeaponKind.Whip,
        WeaponKind.Flail, WeaponKind.Axe, WeaponKind.Tachi, WeaponKind.Staff, WeaponKind.Adae,
    };

    /// <summary>표 전체 조회(화이트리스트 무관). 표에 없는 타입은 null</summary>
    public static WeaponMoveProfile ProfileOf(WeaponKind kind, string itemKey)
    {
        WeaponMoveProfile p;
        if (!string.IsNullOrEmpty(itemKey) && ByItemKey.TryGetValue(itemKey, out p))
        {
            return p;
        }
        return Table.TryGetValue(kind, out p) ? p : null;
    }

    /// <summary>화이트리스트에 든 타입만 프로파일을 돌려준다(없으면 null = 현행 경로)</summary>
    public static WeaponMoveProfile ActiveProfileOf(WeaponKind kind, string itemKey)
    {
        return EnabledKinds.Contains(kind) ? ProfileOf(kind, itemKey) : null;
    }

    /// <summary>이 단계(콤보 단계, 1부터)에서 프로파일이 이동 잠금을 거는가: Free 가 아니면 true (CI-P4, Weapon.CheckAttackWait 한 줄 훅)</summary>
    public static bool LocksMove(WeaponMoveProfile profile, int step)
    {
        return profile != null && profile.ModeAt(step) != AttackMoveMode.Free;
    }

    /// <summary>무기 후딜에 이동 잠금을 걸어야 하는가(화이트리스트 밖이면 false = 프리팹 isWeaponStopMove 만 따른다)</summary>
    public static bool LocksMove(Weapon w)
    {
        return w != null && LocksMove(ActiveProfileOf(w), w.nowConboCount);
    }

    /// <summary>장착 무기 → 활성 프로파일. 프리팹 _kind 가 비면 ItemSheet WeaponType 으로 보충한다(WeaponMasteryService.KindOf)</summary>
    public static WeaponMoveProfile ActiveProfileOf(Weapon w)
    {
        if (w == null)
        {
            return null;
        }
        WeaponKind kind = w.Kind != WeaponKind.None ? w.Kind : WeaponMasteryService.KindOf(w.MasteryKey);
        return ActiveProfileOf(kind, w.MasteryKey);
    }
}
