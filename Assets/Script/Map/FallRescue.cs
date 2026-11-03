using UnityEngine;

/// <summary>낙사 복구 규칙(순수): 맵 지면의 가장 낮은 곳보다 한참 아래로 떨어졌으면 구조한다.</summary>
public static class FallRescueRules
{
    /// <summary>맵 최저 지면보다 이만큼 아래면 낙사로 본다(정상적인 계단식 낙하는 지면이 받쳐 주므로 여기까지 오지 않는다).</summary>
    public const float Margin = 12f;
    /// <summary>안전 위치를 갱신하는 간격(초). 구조는 한 칸 전(이보다 오래된) 위치로 한다: 가장자리에서 막 떨어진 자리가 아니라 안쪽.</summary>
    public const float SafeInterval = 0.5f;

    public static bool ShouldRescue(float y, float floorY)
    {
        return !float.IsNegativeInfinity(floorY) && y < floorY - Margin;
    }
}

/// <summary>
/// 낙사 복구(11-02): 마을처럼 가장자리가 뚫린 맵에서 끝까지 걸어 나가면 영원히 떨어지던 문제(조작 불능).
/// 서 있던 자리를 0.5초마다 기록해 두고, 맵 지면보다 12 아래로 떨어지면 암전 중에 기록해 둔 안쪽 자리로 되돌린다(1초 무적).
/// 맵이 바뀔 때(Portal.MapChanged·웨이포인트) 최저 지면과 기록을 다시 잡는다.
/// </summary>
public sealed class FallRescue : MonoBehaviour
{
    private static FallRescue _instance;
    private float _floorY = float.NegativeInfinity;
    private Vector3 _safeNow, _safePrev;
    private bool _hasSafe;
    private float _nextSample;
    private bool _rescuing;
    private int _groundMask;
    private Rigidbody2D _rb;
    private Collider2D _col;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[FallRescue]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FallRescue>();
    }

    private void Awake()
    {
        _groundMask = LayerMask.GetMask("Ground");
    }

    private void OnEnable()
    {
        Portal.MapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        Portal.MapChanged -= OnMapChanged;
    }

    private void Start()
    {
        MeasureFloor(null);
    }

    private void OnMapChanged(GameObject map)
    {
        MeasureFloor(map);
        _hasSafe = false;
    }

    private void MeasureFloor(GameObject map)
    {
        _floorY = float.NegativeInfinity;
        if (map == null)
        {
            GameObject root = GameObject.Find("Map");
            if (root == null)
            {
                return;
            }
            foreach (Transform m in root.transform)
            {
                if (m.gameObject.activeSelf && m.GetComponent<MapInfo>() != null)
                {
                    map = m.gameObject;
                    break;
                }
            }
        }
        if (map == null)
        {
            return;
        }
        float low = float.PositiveInfinity;
        foreach (Collider2D c in map.GetComponentsInChildren<Collider2D>(false))
        {
            if (!c.isTrigger && ((1 << c.gameObject.layer) & _groundMask) != 0)
            {
                low = Mathf.Min(low, c.bounds.min.y);
            }
        }
        if (!float.IsPositiveInfinity(low))
        {
            _floorY = low;
        }
    }

    private void Update()
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null || _rescuing || DatabaseManager.isUsePortal || ScreenFader.Busy)
        {
            return;
        }
        Vector3 pos = pc.transform.position;
        if (FallRescueRules.ShouldRescue(pos.y, _floorY) && _hasSafe)
        {
            Rescue(pc);
            return;
        }
        if (Time.time >= _nextSample)
        {
            _nextSample = Time.time + FallRescueRules.SafeInterval; // 공중이어도 간격마다만 확인(매 프레임 레이캐스트 방지)
            if (!IsStanding(pc))
            {
                return;
            }
            _safePrev = _hasSafe ? _safeNow : pos;
            _safeNow = pos;
            _hasSafe = true;
        }
    }

    private bool IsStanding(PlayerController pc)
    {
        if (_rb == null || _col == null)
        {
            _rb = pc.GetComponent<Rigidbody2D>();
            _col = pc.GetComponent<Collider2D>();
            return false;
        }
        if (Mathf.Abs(_rb.linearVelocity.y) > 0.05f)
        {
            return false;
        }
        Bounds b = _col.bounds; // 발바닥(콜라이더 아래 면)에서 아래로 짧게
        return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.3f, _groundMask).collider != null;
    }

    private void Rescue(PlayerController pc)
    {
        _rescuing = true;
        Vector3 target = _safePrev;
        GameLog.Info(GameLog.Cat.Player, "낙사 복구: " + pc.transform.position + " → " + target);
        MapTransit.Run(pc.gameObject, () =>
        {
            pc.transform.position = target;
            PlayerStatus.AcquireFor(StatusFlag.Invincible, 1.0f, "FallRescue");
        }, () => _rescuing = false);
    }
}
