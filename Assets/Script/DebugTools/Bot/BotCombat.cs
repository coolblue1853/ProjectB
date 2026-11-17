using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기 사거리(순수 로직): 계열별 평타가 닿는 가로 거리 기본값 + 빗나감 보정. 같은 목표에 연속 MissLimit 번 휘둘러 HP 가 안 줄면 0.2 씩 줄인다(MinRange 까지).
/// 값은 무기 판정 크기를 보고 잡은 시작값이다 — 보정이 실측을 대신한다.
/// </summary>
public sealed class BotRange
{
    public const float MinRange = 0.9f;
    public const int MissLimit = 3;

    private WeaponKind _kind = (WeaponKind)(-1);
    private int _misses;

    public float Range { get; private set; } = 1.3f;

    public static float Base(WeaponKind k)
    {
        switch (k)
        {
            case WeaponKind.Bow: return 7f;
            case WeaponKind.Crossbow: return 6f;
            case WeaponKind.Tome: return 5f;
            case WeaponKind.Javelin: return 5f;
            case WeaponKind.Whip: return 2.6f;
            case WeaponKind.Spear: return 2.3f;
            case WeaponKind.Flail: return 2.1f;
            case WeaponKind.Greatsword:
            case WeaponKind.Scythe: return 1.9f;
            case WeaponKind.Tachi: return 1.8f;
            case WeaponKind.Staff:
            case WeaponKind.Axe: return 1.7f;
            case WeaponKind.Sword:
            case WeaponKind.Mace:
            case WeaponKind.Adae: return 1.6f;
            case WeaponKind.Dual:
            case WeaponKind.Bulwark: return 1.4f;
            case WeaponKind.Dagger:
            case WeaponKind.Claw: return 1.2f;
            case WeaponKind.Knuckle: return 1.1f;
            default: return 1.3f;
        }
    }

    /// <summary>장착 무기 계열이 바뀌면 기본값으로 되돌린다</summary>
    public void SetKind(WeaponKind k)
    {
        if (k != _kind)
        {
            _kind = k;
            Range = Base(k);
            _misses = 0;
        }
    }

    /// <summary>한 묶음을 휘두른 뒤: 맞췄나(목표 HP 감소)</summary>
    public void Swing(bool hit)
    {
        if (hit)
        {
            _misses = 0;
            return;
        }
        if (++_misses >= MissLimit)
        {
            _misses = 0;
            Range = Mathf.Max(MinRange, Range - 0.2f);
        }
    }
}

/// <summary>
/// 공격 예고 회피(순수 로직): 가까운 적(가로 WatchX·세로 WatchY 안)이 예고·공격 상태로 들어오면 avoid.rt 만큼 뒤에 avoid.r.* 비율대로 반응 하나를 Active 초 동안 낸다.
/// 같은 적은 그 상태가 끝날 때까지 한 번만. 사람 기록의 반응 종류는 atk 70%·fwd 16%·back 7%·jump 5%·dash 2%·none 1%(11-15).
/// </summary>
public sealed class BotAvoid
{
    public const float WatchX = 3.5f;
    public const float WatchY = 1.5f;
    public const float Active = 0.35f;
    public const float FallbackRt = 0.15f;

    public enum React { None, Attack, Forward, Back, Jump, Dash }

    private static readonly string[] Keys = { "avoid.r.atk", "avoid.r.fwd", "avoid.r.back", "avoid.r.jump", "avoid.r.dash", "avoid.r.none" };
    private static readonly React[] Kinds = { React.Attack, React.Forward, React.Back, React.Jump, React.Dash, React.None };

    private readonly HashSet<int> _open = new HashSet<int>();
    private readonly List<int> _drop = new List<int>();
    private React _pending = React.None;
    private float _due;
    private int _pendingDir;
    private React _active = React.None;
    private float _until;

    public int Windows { get; private set; }
    /// <summary>이동·점프·대시 반응(공격·무반응 제외)이 시작된 횟수</summary>
    public int Moves { get; private set; }
    /// <summary>현재 반응의 적 쪽 방향(-1·1)</summary>
    public int FoeDir { get; private set; } = 1;

    public static React SampleKind(BotProfile p, double r)
    {
        float total = 0f;
        for (int i = 0; i < Keys.Length; i++)
        {
            total += p.RatioOr(Keys[i], 0f);
        }
        if (total <= 0f)
        {
            return React.Attack;
        }
        float acc = 0f;
        for (int i = 0; i < Keys.Length; i++)
        {
            acc += p.RatioOr(Keys[i], 0f) / total;
            if (r < acc)
            {
                return Kinds[i];
            }
        }
        return Kinds[Kinds.Length - 1];
    }

    public React Update(PlayObsEnemies foes, float now, BotProfile p, System.Random rng)
    {
        _drop.Clear();
        foreach (int uid in _open)
        {
            if (!Threatening(foes, uid))
            {
                _drop.Add(uid);
            }
        }
        for (int i = 0; i < _drop.Count; i++)
        {
            _open.Remove(_drop[i]);
        }
        for (int i = 0; i < foes.FoeCount; i++)
        {
            PlayObsEnemies.Foe f = foes.Foes[i];
            bool threat = (f.State == PlayRecThreat.StTele || f.State == PlayRecThreat.StAttack) && Mathf.Abs(f.Dx) <= WatchX && Mathf.Abs(f.Dy) <= WatchY;
            if (threat && _open.Add(f.Uid))
            {
                Windows++;
                if (_pending == React.None && now >= _until)
                {
                    _pending = SampleKind(p, rng.NextDouble());
                    _pendingDir = f.Dx >= 0f ? 1 : -1;
                    _due = now + p.Sample("avoid.rt", rng, FallbackRt);
                }
            }
        }
        if (_pending != React.None && now >= _due)
        {
            _active = _pending;
            if (_active != React.Attack)
            {
                Moves++;
            }
            FoeDir = _pendingDir;
            _pending = React.None;
            _until = now + Active;
        }
        return now < _until ? _active : React.None;
    }

    private static bool Threatening(PlayObsEnemies foes, int uid)
    {
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (foes.Foes[i].Uid == uid)
            {
                return foes.Foes[i].State == PlayRecThreat.StTele || foes.Foes[i].State == PlayRecThreat.StAttack;
            }
        }
        return false;
    }
}

/// <summary>
/// 포션(순수 로직): HP% 가 사람 임계(potion.hpPct 분포에서 한 번 쓸 때마다 새로 뽑음, 30~85 로 자름) 아래면 HP 포션 칸을 누른다.
/// 칸 고르기 = 이름(HP·Potion·Herbal, Stamina·MP·Mana 제외). 눌렀는데 1.5초 안에 HP 가 안 오르면 그 칸을 30초 뺀다.
/// </summary>
public sealed class BotPotion
{
    public const float Cooldown = 1.5f;
    public const float BadSeconds = 30f;
    public const float FallbackPct = 50f;
    /// <summary>급격한 피해: 최근 약 2초 안 최고 HP 에서 이만큼(최대 HP %) 깎이고 HP 가 BurstMaxPct 아래면 임계와 무관하게 마신다(몹 6마리가 겹쳐 초당 40씩 깎여 임계 50% 에서 마시면 늦던 DeepForest 사망, 11-17)</summary>
    public const float BurstDropPct = 30f;
    public const float BurstMaxPct = 75f;
    public const float BurstBucket = 1f;

    private float _threshold = -1f;
    private float _last = -100f;
    private int _lastSlot;
    private int _hpAtUse;
    private readonly float[] _badUntil = new float[4];
    private float _bucketAt = -100f;
    private int _bucketMax;
    private int _prevMax;

    public int Uses { get; private set; }

    public static bool IsHpPotion(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        string n = name.ToLowerInvariant();
        if (n.Contains("stamina") || n.Contains("mana") || n.Contains(" mp"))
        {
            return false;
        }
        return n.Contains("hp") || n.Contains("potion") || n.Contains("herbal");
    }

    /// <summary>최근 1~2초(1초 칸 두 개) 최고 HP 에서 BurstDropPct% 넘게 깎이고 HP 가 BurstMaxPct% 아래인가. 매 호출마다 칸을 갱신한다</summary>
    private bool Burst(int hp, int hpMax, float now)
    {
        if (now - _bucketAt >= BurstBucket || now < _bucketAt)
        {
            _prevMax = _bucketMax;
            _bucketMax = hp;
            _bucketAt = now;
        }
        _bucketMax = Mathf.Max(_bucketMax, hp);
        if (hpMax <= 0 || hp <= 0)
        {
            return false;
        }
        int recent = Mathf.Max(_bucketMax, _prevMax);
        return (recent - hp) * 100f / hpMax >= BurstDropPct && hp * 100f / hpMax < BurstMaxPct;
    }

    /// <summary>누를 퀵슬롯 1~4(없으면 0). slots = 칸별 아이템 이름</summary>
    public int Decide(int hp, int hpMax, float now, string[] slots, BotProfile p, System.Random rng)
    {
        if (_lastSlot > 0 && now - _last > Cooldown && hp <= _hpAtUse)
        {
            _badUntil[_lastSlot - 1] = now + BadSeconds; // 안 들었다(빈 칸·다른 물약)
            _lastSlot = 0;
        }
        else if (_lastSlot > 0 && hp > _hpAtUse)
        {
            _lastSlot = 0;
        }
        bool burst = Burst(hp, hpMax, now);
        if (hpMax <= 0 || hp <= 0 || now - _last < Cooldown)
        {
            return 0;
        }
        if (_threshold < 0f)
        {
            _threshold = Mathf.Clamp(p.Sample("potion.hpPct", rng, FallbackPct), 30f, 85f);
        }
        if (hp * 100f / hpMax >= _threshold && !burst)
        {
            return 0;
        }
        for (int i = 0; i < slots.Length && i < _badUntil.Length; i++)
        {
            if (now >= _badUntil[i] && IsHpPotion(slots[i]))
            {
                _last = now;
                _lastSlot = i + 1;
                _hpAtUse = hp;
                _threshold = -1f;
                Uses++;
                return i + 1;
            }
        }
        return 0;
    }
}

/// <summary>
/// 스킬 고르기(순수 로직): 쿨이 끝났고 HUD 에 아이콘이 있고(잠김 아님) 최근 실패(눌렀는데 쿨이 안 돎 = 스태미나 부족 등)가 아닌 칸 중, 사람 누름 횟수(press.Skill* n) 비율로 하나.
/// </summary>
public static class BotSkill
{
    public static readonly string[] Actions = { "SkillA", "SkillS", "SkillD", "SkillF" };
    private static readonly string[] PressKeys = { "press.SkillA", "press.SkillS", "press.SkillD", "press.SkillF" };

    /// <summary>칸 1~4(없으면 0)</summary>
    public static int Pick(float[] cooldowns, bool[] slotOn, float[] failUntil, float now, double r, BotProfile p)
    {
        float total = 0f;
        for (int i = 0; i < Actions.Length; i++)
        {
            total += Ready(cooldowns, slotOn, failUntil, now, i) ? Weight(p, i) : 0f;
        }
        if (total <= 0f)
        {
            return 0;
        }
        float acc = 0f;
        for (int i = 0; i < Actions.Length; i++)
        {
            if (!Ready(cooldowns, slotOn, failUntil, now, i))
            {
                continue;
            }
            acc += Weight(p, i) / total;
            if (r < acc)
            {
                return i + 1;
            }
        }
        for (int i = Actions.Length - 1; i >= 0; i--)
        {
            if (Ready(cooldowns, slotOn, failUntil, now, i))
            {
                return i + 1;
            }
        }
        return 0;
    }

    private static bool Ready(float[] cd, bool[] on, float[] fail, float now, int i)
    {
        return i < cd.Length && i < on.Length && on[i] && cd[i] <= 0f && now >= fail[i];
    }

    private static float Weight(BotProfile p, int i)
    {
        BotProfile.Dist d = p.FindDist(PressKeys[i]);
        return d != null && d.n > 0 ? d.n : 1f;
    }
}

/// <summary>HUD 읽기(화면에 보이는 것만): 스킬 칸 아이콘·잠김, 퀵슬롯 칸 아이템 이름. 게임 상태는 바꾸지 않는다</summary>
public static class BotHud
{
    public static void SkillSlots(bool[] into)
    {
        SkillCooldown cd = SkillCooldown.instance;
        Weapon w = Weapon.Equipped;
        for (int i = 0; i < into.Length; i++)
        {
            Image icon = cd != null && i < cd.cooldownImage.Length ? cd.cooldownImage[i] : null;
            into[i] = icon != null && icon.sprite != null && icon.gameObject.activeInHierarchy && w != null && WeaponMasteryService.IsSlotUnlocked(w, i);
        }
    }

    public static void QuickSlots(string[] into)
    {
        for (int i = 0; i < into.Length; i++)
        {
            into[i] = QuickSlotBar.SlotItem(i);
        }
    }
}
