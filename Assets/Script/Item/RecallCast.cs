using UnityEngine;

/// <summary>귀환 주문서 시전 순수 규칙 (ClassicMapleDesign.md 3.3 / 구현 3단계)</summary>
public static class RecallRules
{
    /// <summary>시전 시간(초)</summary>
    public const float CastSeconds = 2f;
    /// <summary>시전 시작 위치에서 이만큼(월드 단위) 벗어나면 이동으로 보고 취소</summary>
    public const float MoveCancelDistance = 0.2f;

    public static bool Moved(Vector2 start, Vector2 now)
        => (now - start).sqrMagnitude > MoveCancelDistance * MoveCancelDistance;

    public static bool Completed(float elapsed) => elapsed >= CastSeconds;

    /// <summary>시전 취소 여부: 피격(HP 감소)·이동·사망</summary>
    public static bool ShouldCancel(bool hit, bool moved, bool dead) => hit || moved || dead;
}

/// <summary>
/// 귀환 주문서 시전기. 아이템은 시전이 끝나 이동이 성사될 때 1개 소비한다(취소하면 소모 없음).
/// 시전 중 피격·이동(2D 거리)·사망이면 취소. 이동 목적지는 마을(MapInfo.GoToTown; 사망 부활과 같은 곳 — 현재 거점은 마을뿐).
/// </summary>
public sealed class RecallCast : MonoBehaviour
{
    private static RecallCast _instance;

    private string _itemName;
    private float _elapsed;
    private Vector2 _start;
    private bool _hit;
    private PlayerHealthManager _hp;

    /// <summary>시전 바 표시용 이름</summary>
    public const string CastLabel = "귀환";
    /// <summary>시전 시작</summary>
    public static event System.Action Started;
    /// <summary>시전 종료. true = 완료(시간 다 채움), false = 취소</summary>
    public static event System.Action<bool> Ended;

    public static bool IsCasting => _instance != null && _instance.enabled;
    public static float Progress => IsCasting ? Mathf.Clamp01(_instance._elapsed / RecallRules.CastSeconds) : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        Started = null;
        Ended = null;
    }

    /// <summary>시전 시작. 이미 시전 중·마을·사망·플레이어 없음이면 false(소모 없음)</summary>
    public static bool Begin(string itemName)
    {
        PlayerHealthManager hp = PlayerHealthManager.Instance;
        if (hp == null || hp.IsDead || PlayerController.instance == null || IsCasting || IsInTown())
        {
            return false;
        }
        if (_instance == null)
        {
            var go = new GameObject("[RecallCast]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RecallCast>();
        }
        _instance.StartCast(itemName, hp);
        return true;
    }

    public static void Cancel(string reason)
    {
        if (IsCasting)
        {
            _instance.Stop(reason);
        }
    }

    private static bool IsInTown()
    {
        GameObject map = GameObject.Find("Map");
        Transform town = map != null ? map.transform.Find("Town") : null;
        return town != null && town.gameObject.activeInHierarchy;
    }

    private void StartCast(string itemName, PlayerHealthManager hp)
    {
        _itemName = itemName;
        _elapsed = 0f;
        _hit = false;
        _start = PlayerController.instance.transform.position;
        _hp = hp;
        _hp.OnHpDamaged += OnHit;
        enabled = true;
        Started?.Invoke();
        GameLog.Info(GameLog.Cat.Item, "귀환 시전 시작(" + RecallRules.CastSeconds + "초)");
    }

    private void OnHit(object sender, System.EventArgs e)
    {
        _hit = true;
    }

    private void Awake()
    {
        enabled = false;
    }

    private void Update()
    {
        PlayerController pc = PlayerController.instance;
        bool moved = pc == null || RecallRules.Moved(_start, pc.transform.position);
        if (RecallRules.ShouldCancel(_hit, moved, _hp == null || _hp.IsDead))
        {
            Stop(_hit ? "피격" : "이동");
            return;
        }
        _elapsed += Time.deltaTime;
        if (!RecallRules.Completed(_elapsed))
        {
            return;
        }
        string item = _itemName;
        Stop(null);
        if (InventoryManager.instance != null && InventoryManager.instance.RemoveOne(item))
        {
            MapInfo.GoToTown(pc.gameObject);
            GameLog.Info(GameLog.Cat.Item, "귀환 완료 → 마을");
        }
    }

    private void Stop(string cancelReason)
    {
        if (_hp != null)
        {
            _hp.OnHpDamaged -= OnHit;
        }
        bool wasCasting = enabled;
        enabled = false;
        if (wasCasting)
        {
            Ended?.Invoke(cancelReason == null);
        }
        if (cancelReason != null)
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            GameLog.Info(GameLog.Cat.Item, "귀환 취소(" + cancelReason + ")");
        }
    }

    private void OnDisable()
    {
        if (_hp != null)
        {
            _hp.OnHpDamaged -= OnHit;
        }
    }
}
