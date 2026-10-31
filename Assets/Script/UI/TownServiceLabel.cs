using TMPro;
using UnityEngine;

/// <summary>NPC 방향 반전과 무관한 마을 이름표·가까운 서비스 안내.</summary>
public sealed class TownServiceLabel : MonoBehaviour
{
    private Transform _target;
    private TextMeshPro _text;
    private string _title;
    private string _nearText;
    private bool _near;
    private float _height = 2.2f;

    public void Init(Transform target, string title, string hint)
    {
        _target = target;
        _height = target.GetComponent<Chest>() != null ? 1.55f : 2.2f;
        _title = title;
        _nearText = title + "\n<size=75%>" + hint + "</size>";
        _text = gameObject.AddComponent<TextMeshPro>();
        _text.font = UiTheme.Get().pixelFont;
        _text.fontSize = 2.3f;
        _text.alignment = TextAlignmentOptions.Center;
        _text.rectTransform.sizeDelta = new Vector2(4.8f, 1.5f);
        _text.color = new Color(0.92f, 0.88f, 0.72f);
        _text.outlineWidth = 0.2f;
        _text.outlineColor = Color.black;
        var sr = target.GetComponentInChildren<SpriteRenderer>();
        _text.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
        _text.sortingOrder = 502;
        _text.text = _title;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = _target.position + Vector3.up * _height;
        bool visible = _target.gameObject.activeInHierarchy;
        _text.enabled = visible;
        bool near = false;
        if (visible && PlayerController.instance != null && !DatabaseManager.isOpenUI &&
            !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            Vector3 delta = PlayerController.instance.transform.position - _target.position;
            near = Mathf.Abs(delta.x) <= 1.1f && Mathf.Abs(delta.y) <= 2f;
        }
        if (near != _near)
        {
            _near = near;
            _text.text = near ? _nearText : _title;
        }
    }
}
