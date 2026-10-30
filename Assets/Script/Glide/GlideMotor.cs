using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 풍향 활공 이동 능력(FloorCoreContent.md 2절). 플레이어에 런타임으로 붙는다(씬 파일 수정 없음).
/// 점프 키 0.2초 홀드(공중·하강 중) → 활공: 낙하 −1.3 u/s, 수평 85%. 짧은 누름은 기존 2단 점프 그대로(PlayerController 는 건드리지 않는다).
/// 기류(AirflowZone)는 위치 폴링으로 판정한다. 지속 최대 12초(상승 부하 부족 시 −10~20%), 상승 기류 안 +3/s, 착지 시 초기화.
/// 해금: 2층계(Lv2_*) 맵 첫 진입 시 ES3 `glide.unlocked` 저장, 이후 어디서나 사용.
/// </summary>
[DefaultExecutionOrder(50)]
public class GlideMotor : MonoBehaviour
{
    public const string UnlockKey = "glide.unlocked";
    private const float GroundProbe = 0.3f;

    private PlayerController _pc;
    private Rigidbody2D _rb;
    private Collider2D _col;
    private int _groundMask;
    private float _held;
    private float _remaining;
    private float _lastOutX = float.NaN;
    private float _exitTimer;
    private float _lastUpVy;
    private GlideGauge _gauge;

    public static GlideMotor Instance { get; private set; }
    public static bool Unlocked { get; private set; }

    public bool Gliding { get; private set; }
    public bool Grounded { get; private set; }
    public float Remaining => _remaining;
    public float MaxDuration => GlideRules.MaxDurationFor(AscentLoad.Deficit);
    /// <summary>플레이어 몸 중심(기류 판정용)</summary>
    public Vector2 Center => _col != null ? (Vector2)_col.bounds.center : (Vector2)transform.position;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        Unlocked = false;
        Portal.MapChanged -= OnMapChanged;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        Unlocked = ES3.KeyExists(UnlockKey) && ES3.Load<bool>(UnlockKey);
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null && pc.GetComponent<GlideMotor>() == null)
        {
            pc.gameObject.AddComponent<GlideMotor>();
        }
    }

    private static void OnMapChanged(GameObject openMap)
    {
        if (!Unlocked && openMap != null && openMap.name.StartsWith("Lv2_"))
        {
            Unlock();
        }
    }

    /// <summary>해금(저장). 이미 해금이면 false</summary>
    public static bool Unlock()
    {
        if (Unlocked)
        {
            return false;
        }
        Unlocked = true;
        ES3.Save(UnlockKey, true);
        BossBanner.Show("Wind Glide", "HOLD JUMP IN THE AIR TO GLIDE", true);
        GameLog.Info(GameLog.Cat.Player, "활공 해금");
        return true;
    }

    private void Awake()
    {
        Instance = this;
        _pc = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _groundMask = LayerMask.GetMask("Ground");
        _remaining = GlideRules.MaxDuration;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (_pc.JumpHeld)
        {
            _held += Time.deltaTime;
        }
        else
        {
            _held = 0f;
        }
        UpdateGauge();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Grounded = ProbeGround();
        bool blocked = !Unlocked || DatabaseManager.isOpenUI || _pc.isUpLadder || _pc.isWall || _pc.states == "dash" || _pc.states == "wallJump";
        float vy = _rb.linearVelocity.y;
        float max = MaxDuration;
        if (Grounded)
        {
            _remaining = max;
        }
        bool wantGlide = _held >= GlideRules.HoldToStart && _pc.JumpHeld;
        Gliding = !blocked && !Grounded && wantGlide && _remaining > 0f && (Gliding || vy <= 0.01f);

        float windVy;
        float windX;
        bool inUp;
        bool hasWind = SampleWind(out windVy, out windX, out inUp);
        _remaining = GlideRules.TickRemaining(Mathf.Min(_remaining, max), dt, Gliding, inUp && !Grounded, max);

        if (hasWind && windVy > 0f)
        {
            _lastUpVy = windVy;
            _exitTimer = GlideRules.ExitDecay;
        }
        else if (_exitTimer > 0f)
        {
            _exitTimer -= dt;
        }
        if (Grounded)
        {
            _exitTimer = 0f;
        }
        if (!hasWind && !Gliding && _exitTimer <= 0f)
        {
            _lastUpVy = 0f;
            return;
        }
        float ratio = Mathf.Clamp01(_exitTimer / GlideRules.ExitDecay);
        float nvy = GlideRules.VerticalSpeed(vy, Gliding, hasWind, windVy, hasWind ? 0f : ratio, _lastUpVy);
        if (Gliding && (hasWind || vy <= GlideRules.GlideFall + 0.001f || nvy > vy + 0.0001f))
        {
            // 물리 스텝이 속도에 중력을 더한 뒤 이동하므로, 그 몫만큼 미리 보정해 실제 낙하가 −1.3 이 되게 한다
            nvy += -Physics2D.gravity.y * _rb.gravityScale * dt;
        }
        if (nvy != vy)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, nvy);
        }
    }

    private void LateUpdate()
    {
        if (!Gliding)
        {
            _lastOutX = float.NaN;
            return;
        }
        float x = _rb.linearVelocity.x;
        if (!float.IsNaN(_lastOutX) && Mathf.Approximately(x, _lastOutX))
        {
            return; // 이동 코드가 이번 프레임 속도를 다시 쓰지 않았다 → 이미 85% 적용된 값
        }
        float windVy;
        float windX;
        bool inUp;
        SampleWind(out windVy, out windX, out inUp);
        float nx = GlideRules.HorizontalSpeed(x, true, windX);
        _lastOutX = nx;
        _rb.linearVelocity = new Vector2(nx, _rb.linearVelocity.y);
    }

    private bool SampleWind(out float windVy, out float windX, out bool inUpdraft)
    {
        windVy = 0f;
        windX = 0f;
        inUpdraft = false;
        bool any = false;
        Vector2 c = Center;
        IReadOnlyList<AirflowZone> all = AirflowZone.All;
        for (int i = 0; i < all.Count; i++)
        {
            AirflowZone z = all[i];
            if (!z.isActiveAndEnabled || !z.Contains(c))
            {
                continue;
            }
            float vy = GlideRules.WindVy(z.Kind);
            if (vy != 0f)
            {
                if (!any || Mathf.Abs(vy) > Mathf.Abs(windVy))
                {
                    windVy = vy;
                }
                any = true;
                inUpdraft |= vy > 0f;
            }
            windX += GlideRules.WindVx(z.Kind);
        }
        return any;
    }

    private bool ProbeGround()
    {
        return Cast(_pc.groundCheck) || Cast(_pc.groundCheck2);
    }

    private bool Cast(GameObject probe)
    {
        if (probe == null)
        {
            return false;
        }
        return Physics2D.Raycast(probe.transform.position, Vector2.down, GroundProbe, _groundMask).collider != null;
    }

    private void UpdateGauge()
    {
        bool show = Unlocked && !Grounded && (Gliding || _remaining < MaxDuration - 0.05f);
        if (!show && _gauge == null)
        {
            return;
        }
        if (_gauge == null)
        {
            _gauge = GlideGauge.Create();
        }
        _gauge.Set(show, transform.position, MaxDuration > 0f ? _remaining / MaxDuration : 0f);
    }
}
