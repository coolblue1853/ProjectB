using TMPro;
using UnityEngine;

/// <summary>
/// 구역 보고 안내판: 깊은 숲·늪·산적 요새·탑 근교 입구(마차 자리 옆)에 서서, ↑ 로 이브가 보고 대상인 퀘스트를
/// 일반 숲까지 돌아가지 않고 보고하게 한다(QuestEntrance.ZoneReport). 수락은 하지 않는다(보고 전용).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ZoneReportSign : MonoBehaviour
{
    /// <summary>안내판을 세울 맵(마차 정류장 id 기준). 깊은 숲 심부·안개 늪은 부모 구역 안내판을 쓴다</summary>
    public static readonly string[] Maps = { "Lv1_DeepForest", "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_TowerOutskirts" };

    /// <summary>마차 NPC 에서 안내판까지 떨어진 거리(마차 대화 범위와 겹치지 않게)</summary>
    public const float OffsetFromCoach = 3.5f;

    private TextMeshPro _prompt;
    private bool _near;

    private void Awake()
    {
        var go = new GameObject("SignPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 3.4f, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme != null && theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = "↑ 퀘스트 보고";
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme != null ? theme.unique : Color.yellow;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    private void OnEnable()
    {
        Floor1Input.Acquire();
    }

    private void OnDisable()
    {
        Floor1Input.Release();
        _near = false;
    }

    private void Update()
    {
        bool mine = InteractArbiter.Contend(this, transform.position, _near);
        bool show = mine && !NpcQuestWindow.IsOpen;
        if (_prompt.gameObject.activeSelf != show)
        {
            _prompt.gameObject.SetActive(show);
        }
        if (show && Floor1Input.UpPressed && !DatabaseManager.isOpenUI)
        {
            QuestEntrance.Open(QuestEntrance.ZoneReport());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = false;
        }
    }

    /// <summary>맵 하나에 안내판을 세운다(x 는 마차 자리 + OffsetFromCoach, 높이는 지면 스냅)</summary>
    public static GameObject Create(Transform map, float x, float y, SpriteRenderer sortingSource)
    {
        var go = new GameObject("ZoneReportSign");
        go.transform.SetParent(map, false);
        go.transform.position = new Vector3(x, y, map.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3.5f, 3f);
        box.offset = new Vector2(0f, 1.5f);
        var art = new GameObject("Art");
        art.transform.SetParent(go.transform, false);
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sprite = BountyKioskPlacer.SignSprite();
        if (sortingSource != null)
        {
            sr.sortingLayerID = sortingSource.sortingLayerID;
            sr.sortingOrder = sortingSource.sortingOrder;
        }
        go.AddComponent<ZoneReportSign>();
        go.AddComponent<GroundSnapOnce>();
        return go;
    }
}
