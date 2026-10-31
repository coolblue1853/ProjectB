using UnityEngine;

/// <summary>
/// 방문 목표 지점 표시 (야간 관측지·정예 둥지·별 제단). 플레이어가 닿으면 NpcQuestLog.OnVisit.
/// 해당 방문 퀘스트가 진행 중일 때만 별 모양 표시를 보인다. 처음 켜질 때 아래 지면에 붙는다.
/// </summary>
public class QuestVisitMarker : MonoBehaviour
{
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float RefreshInterval = 0.5f;

    private static Sprite s_star;

    [SerializeField] private string _markerId;
    private SpriteRenderer _sr;
    private bool _snapped;
    private bool _inside;
    private float _nextRefresh;

    public string MarkerId => _markerId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_star = null;
    }

    public void Init(string markerId, SpriteRenderer sr)
    {
        _markerId = markerId;
        _sr = sr;
    }

    private void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            GroundSnap.Snap(transform, SnapUp, SnapDown);
        }
        if (Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (_inside)
            {
                NpcQuestLog.OnVisit(_markerId);
            }
            if (_sr != null)
            {
                _sr.enabled = IsWanted();
            }
        }
    }

    private bool IsWanted()
    {
        foreach (NpcQuestDef d in NpcQuestLog.ActiveQuests())
        {
            if (d.Objective == NpcQuestObjective.Visit && d.Target == _markerId && NpcQuestLog.Progress(d) < d.Count)
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inside = false;
        }
    }

    private void OnDisable()
    {
        _inside = false;
    }

    /// <summary>금빛 네 갈래 별 (13x13, 브로치 문장) — 코드 도트</summary>
    public static Sprite StarSprite()
    {
        if (s_star != null)
        {
            return s_star;
        }
        const int n = 13;
        Color32[] px = MiniGameSprites.Blank(n * n);
        var gold = new Color32(232, 196, 92, 255);
        var dark = new Color32(120, 90, 40, 255);
        int c = n / 2;
        for (int d = -5; d <= 5; d++)
        {
            int len = 5 - Mathf.Abs(d);
            for (int k = -len / 2; k <= len / 2; k++)
            {
                px[(c + d) * n + c + k] = gold;
                px[(c + k) * n + c + d] = gold;
            }
        }
        px[(c - 5) * n + c] = dark;
        px[(c + 5) * n + c] = dark;
        px[c * n + c - 5] = dark;
        px[c * n + c + 5] = dark;
        s_star = MiniGameSprites.MakeSprite(px, n, n);
        return s_star;
    }
}

/// <summary>"Ground" 레이어 지면 맞추기 (MiniGameSpot.SnapToGround 와 같은 방식)</summary>
public static class GroundSnap
{
    public static void Snap(Transform t, float up, float down)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        filter.useTriggers = false;
        var hits = new RaycastHit2D[4];
        Vector2 from = (Vector2)t.position + Vector2.up * up;
        if (Physics2D.Raycast(from, Vector2.down, filter, hits, up + down) > 0)
        {
            Vector3 p = t.position;
            t.position = new Vector3(p.x, hits[0].point.y, p.z);
        }
        else
        {
            Debug.LogWarning("[PB][NpcQuest] 지면 못 찾음: " + t.name + " @" + t.position);
        }
    }
}

/// <summary>처음 활성화될 때 한 번 지면에 붙는다 (맵이 꺼져 있어 부트 때는 레이캐스트가 안 맞으므로)</summary>
public class GroundSnapOnce : MonoBehaviour
{
    private bool _done;

    private void Update()
    {
        if (_done)
        {
            return;
        }
        _done = true;
        GroundSnap.Snap(transform, 0.5f, 10f);
    }
}
