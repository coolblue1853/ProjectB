using TMPro;
using UnityEngine;

/// <summary>기존 슬롯 위에 잠금 표시와 작은 획득 점을 표시한다. 이미지 제작 없이 공용 글꼴을 쓴다.</summary>
public sealed class ItemStatusBadge : MonoBehaviour
{
    private ItemCheck _item;
    private TextMeshProUGUI _label;
    private int _shown = -1;

    private void Awake()
    {
        _item = GetComponent<ItemCheck>();
    }

    private void LateUpdate()
    {
        if (_item.itemData == null)
        {
            return;
        }
        int flags = ItemFlagSave.Encode(_item.itemData.isLocked, _item.itemData.isNew);
        if (flags == _shown)
        {
            return;
        }
        _shown = flags;
        if (_label == null && flags != 0)
        {
            _label = new GameObject("ItemStatus", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            _label.transform.SetParent(transform, false);
            _label.font = UiTheme.Get().pixelFont;
            _label.fontSize = 22f;
            _label.alignment = TextAlignmentOptions.TopLeft;
            _label.raycastTarget = false;
            _label.outlineWidth = 0.25f;
            RectTransform rect = _label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 0f);
            rect.offsetMax = new Vector2(0f, -2f);
        }
        if (_label != null)
        {
            _label.text = (_item.itemData.isLocked ? "<color=#FFD480>L</color>" : "")
                + (_item.itemData.isNew ? "<color=#7DFFC1>•</color>" : "");
        }
    }
}
