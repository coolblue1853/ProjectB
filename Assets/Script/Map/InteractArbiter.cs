using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ↑ 상호작용 중재(REWORK-0 U32). 포털·NPC·상점·창고·저장지점이 겹쳐 있어도 플레이어에게 가장 가까운 1개만 ↑ 를 소비한다.
/// 사용: 매 프레임 Update 에서 먼저 `bool mine = InteractArbiter.Contend(this, transform.position, 범위 안인지)` 를 부르고,
/// ↑ 입력 처리는 `mine &amp;&amp; 입력` 일 때만 한다. 범위 안인 후보는 매 프레임 자동으로 등록된다.
/// 후보 등록은 한 프레임 지연(이전 프레임 등록분 + 이번 프레임 먼저 등록한 것)을 합쳐 판정한다. 거리가 같으면 InstanceID 가 작은 쪽.
/// </summary>
public static class InteractArbiter
{
    private static readonly Dictionary<int, float> _prev = new Dictionary<int, float>();
    private static readonly Dictionary<int, float> _cur = new Dictionary<int, float>();
    private static int _frame = -1;

    /// <summary>거리 제곱 차가 이 값 이하면 같은 자리로 본다(약 0.5 유닛)</summary>
    private const float CoLocatedSqr = 0.25f;

    /// <summary>범위 안이면 후보로 등록하고, 지금 가장 가까운 후보가 owner 인지 돌려준다. 범위 밖이면 항상 false</summary>
    public static bool Contend(Object owner, Vector3 position, bool inRange)
    {
        if (owner == null || !inRange)
        {
            return false;
        }
        Roll();
        int id = owner.GetInstanceID();
        float dist = Distance(position);
        _cur[id] = dist;
        return IsNearest(id, dist, _prev) && IsNearest(id, dist, _cur);
    }

    private static void Roll()
    {
        if (_frame == Time.frameCount)
        {
            return;
        }
        _frame = Time.frameCount;
        _prev.Clear();
        foreach (KeyValuePair<int, float> kv in _cur)
        {
            _prev[kv.Key] = kv.Value;
        }
        _cur.Clear();
    }

    private static float Distance(Vector3 position)
    {
        PlayerController player = PlayerController.instance;
        if (player == null)
        {
            return 0f;
        }
        Vector2 delta = player.transform.position - position;
        return delta.sqrMagnitude;
    }

    private static bool IsNearest(int id, float dist, Dictionary<int, float> others)
    {
        foreach (KeyValuePair<int, float> kv in others)
        {
            if (kv.Key == id)
            {
                continue;
            }
            // 거의 같은 위치(한 NPC 에 붙은 자식 오브젝트 등)는 충돌로 보지 않는다 → 둘 다 통과
            if (kv.Value + CoLocatedSqr < dist)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>테스트·씬 전환용 초기화</summary>
    public static void Clear()
    {
        _prev.Clear();
        _cur.Clear();
        _frame = -1;
    }
}
