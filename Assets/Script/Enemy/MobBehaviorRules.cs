using UnityEngine;

/// <summary>몹 행동 속성(REWORK-3). MonsterSheet 맨 뒤 8열에서 읽고, 빈 값은 "현재 BT 동작 그대로"다.</summary>
public enum MobMove { Default, Patrol, Stay, Hop }
public enum MobEdge { Default, Turn, Fall }
public enum MobAggro { Default, First, Reactive, Passive }
public enum MobAttackKind { Default, Melee, Ranged, Contact }

/// <summary>
/// 파싱된 행동 속성. 필드가 비어 있으면(Default/null) 프리팹 BT 값을 그대로 쓴다.
/// move: 제자리(stay)·순찰(patrol)·순찰하며 뜀(hop). edge: 가장자리에서 돌아섬(turn)·그대로 낙하(fall).
/// chase: 추격 여부. chaseRange: 인식 가로 전체 폭. visionY: 인식·원거리 공격 수직 한계(위아래 각각). giveUp: 추격을 포기하는 가로 전체 폭(인식 상자 폭과 같은 단위).
/// aggro: 선공(first)·후공(reactive, 맞아야 반응)·비공격(passive, 인식 안 함). attack: 근접·원거리·접촉만.
/// </summary>
public sealed class MobBehavior
{
    public MobMove Move;
    public MobEdge Edge;
    public bool? Chase;
    public float? ChaseRange, VisionY, GiveUp;
    public MobAggro Aggro;
    public MobAttackKind Attack;

    /// <summary>전부 기본이면 true — 이 경우 BT 훅은 기존 코드 경로를 그대로 탄다.</summary>
    public bool IsDefault => Move == MobMove.Default && Edge == MobEdge.Default && !Chase.HasValue && !ChaseRange.HasValue
        && !VisionY.HasValue && !GiveUp.HasValue && Aggro == MobAggro.Default && Attack == MobAttackKind.Default;

    public bool ChasesPlayer => Chase ?? true;
}

/// <summary>행동 속성 순수 규칙: 파싱·인식 상자·후공 판정·수직 시야·히트박스 보정(BUG-U23). 테스트 대상.</summary>
public static class MobBehaviorRules
{
    /// <summary>BT 의 PlayerChecker 중 폭이 이 값 이상인 것이 "플레이어 인식" 검사다(공격 사거리 검사는 2~15).</summary>
    public const float DetectMinWidth = 18f;
    /// <summary>후공 몹이 맞은 뒤 반응하는 시간(초)</summary>
    public const float ProvokeSeconds = 8f;
    /// <summary>BUG-U23 몹 히트박스 월드 하한(가로·세로). 토끼 0.78x0.61·유령 0.92x0.79 같은 소형 몹이 스킬 판정에 안 걸리던 문제. 이보다 큰 몹은 그대로.</summary>
    public const float MinHitWidth = 1.2f, MinHitHeight = 1.1f;
    /// <summary>REWORK-3 P2: edge=fall 몹이 이만큼 이상 아래 발판에 내려앉으면 순찰 기준점을 그 자리로 옮긴다(작은 단차는 무시)</summary>
    public const float RehomeDrop = 1.5f;
    /// <summary>착지 판정 세로 속도 한계</summary>
    public const float RehomeMaxVy = 0.05f;

    /// <summary>
    /// 몹 킬플레인 y(REWORK-3 P2). 맵 카메라 하한과 가장 낮은 스폰 발판 중 낮은 쪽에서 플레이어 낙사 여유(FallRecovery.KillMargin)만큼 아래.
    /// 발판 기준을 같이 보는 것은 하한이 기본값(-10)으로 남은 깊은 맵에서 스폰 즉시 제거되지 않게 하려는 것.
    /// </summary>
    public static float KillPlaneY(float mapBottom, float lowestSpawnY)
    {
        return Mathf.Min(mapBottom, lowestSpawnY) - FallRecovery.KillMargin;
    }

    /// <summary>edge=fall 몹이 아래 발판에 착지했으면 true — 메이플처럼 그 발판에서 계속 활동하도록 기준점을 옮긴다</summary>
    public static bool ShouldRehome(MobEdge edge, float homeY, float y, float vy)
    {
        return edge == MobEdge.Fall && homeY - y > RehomeDrop && Mathf.Abs(vy) < RehomeMaxVy;
    }

    public static MobBehavior Parse(string move, string edge, string chase, string chaseRange, string visionY, string giveUp, string aggro, string attack)
    {
        var b = new MobBehavior
        {
            Move = ParseMove(move),
            Edge = ParseEdge(edge),
            Chase = (chase ?? "").Trim() == "yes" ? true : (chase ?? "").Trim() == "no" ? false : (bool?)null,
            ChaseRange = ParseFloat(chaseRange),
            VisionY = ParseFloat(visionY),
            GiveUp = ParseFloat(giveUp),
            Aggro = ParseAggro(aggro),
            Attack = ParseAttack(attack),
        };
        return b.IsDefault ? null : b;
    }

    static MobMove ParseMove(string s)
    {
        switch ((s ?? "").Trim())
        {
            case "patrol": return MobMove.Patrol;
            case "stay": return MobMove.Stay;
            case "hop": return MobMove.Hop;
            default: return MobMove.Default;
        }
    }

    static MobEdge ParseEdge(string s)
    {
        switch ((s ?? "").Trim())
        {
            case "turn": return MobEdge.Turn;
            case "fall": return MobEdge.Fall;
            default: return MobEdge.Default;
        }
    }

    static MobAggro ParseAggro(string s)
    {
        switch ((s ?? "").Trim())
        {
            case "first": return MobAggro.First;
            case "reactive": return MobAggro.Reactive;
            case "passive": return MobAggro.Passive;
            default: return MobAggro.Default;
        }
    }

    static MobAttackKind ParseAttack(string s)
    {
        switch ((s ?? "").Trim())
        {
            case "melee": return MobAttackKind.Melee;
            case "ranged": return MobAttackKind.Ranged;
            case "contact": return MobAttackKind.Contact;
            default: return MobAttackKind.Default;
        }
    }

    static float? ParseFloat(string s)
    {
        return float.TryParse((s ?? "").Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) && v > 0f ? v : (float?)null;
    }

    public static bool IsDetectBox(Vector2 prefabBox) => prefabBox.x >= DetectMinWidth;

    /// <summary>
    /// 인식 상자. 프리팹 값(가로 20·세로 10)을 chaseRange·visionY 로 덮는다.
    /// 추격 중(latched)이거나 맞아서 반응 중(provoked)이면 가로를 giveUp 까지 늘려 쉽게 놓치지 않게 한다.
    /// </summary>
    public static Vector2 DetectBox(MobBehavior b, Vector2 prefabBox, bool latched, bool provoked)
    {
        float w = b.ChaseRange ?? prefabBox.x;
        if ((latched || provoked) && b.GiveUp.HasValue)
        {
            w = Mathf.Max(w, b.GiveUp.Value);
        }
        float h = b.VisionY.HasValue ? b.VisionY.Value * 2f : prefabBox.y;
        return new Vector2(w, h);
    }

    /// <summary>새로 인식을 시작해도 되는가. 선공은 항상, 후공은 맞은 뒤에만, 비공격은 절대 안 한다. 이미 추격 중이면 유지한다.</summary>
    public static bool MayAcquire(MobBehavior b, bool latched, bool provoked)
    {
        if (b.Aggro == MobAggro.Passive)
        {
            return false;
        }
        if (latched || b.Aggro != MobAggro.Reactive)
        {
            return true;
        }
        return provoked;
    }

    /// <summary>원거리 공격 수직 한계. visionY 지정이 있으면 그 값, 없으면 기본 2.5(U16)</summary>
    public static float VerticalGap(MobBehavior b) => b != null && b.VisionY.HasValue ? b.VisionY.Value : RangedAttackRules.MaxVerticalGap;

    /// <summary>수직 시야 검사를 적용할 공격인가: 발사형 또는 attack=ranged</summary>
    public static bool LimitsVertical(MobBehavior b, bool isLaunch) => isLaunch || (b != null && b.Attack == MobAttackKind.Ranged);

    /// <summary>
    /// BUG-U23: 작은 몹 히트박스 하한. 월드 크기가 하한보다 작으면 하한까지 키우되 발(아래쪽)과 가로 중심은 그대로 둔다.
    /// 반환 = 새 로컬 크기·오프셋. 이미 충분하면 changed=false.
    /// </summary>
    public static bool FitHitbox(Vector2 localSize, Vector2 localOffset, Vector2 lossyScale, float minW, float minH, out Vector2 newSize, out Vector2 newOffset)
    {
        newSize = localSize;
        newOffset = localOffset;
        float sx = Mathf.Abs(lossyScale.x), sy = Mathf.Abs(lossyScale.y);
        if (sx < 0.0001f || sy < 0.0001f)
        {
            return false;
        }
        float worldW = localSize.x * sx, worldH = localSize.y * sy;
        bool changed = false;
        if (worldW < minW)
        {
            newSize.x = minW / sx;
            changed = true;
        }
        if (worldH < minH)
        {
            newSize.y = minH / sy;
            newOffset.y = localOffset.y - (localSize.y - newSize.y) * 0.5f; // 아래쪽 고정: 중심을 (늘어난 만큼/2) 위로
            changed = true;
        }
        return changed;
    }
}
