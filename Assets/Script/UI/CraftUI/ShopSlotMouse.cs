using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 상점 진열 칸 마우스 입력(REWORK-2 U27). ShopManager.OpenShop 이 진열된 ItemCheck 마다 런타임에 붙인다(씬 수정 없음).
/// 호버=커서 이동+툴팁, 왼쪽 클릭=선택(상세), 선택된 칸을 다시 클릭·더블클릭·오른쪽 클릭=구매 수량 창, 휠=페이지.
/// 키보드(↑↓←→ Z)·패드(십자 A)는 ShopManager.Update 에 그대로 있다.
/// </summary>
public sealed class ShopSlotMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IScrollHandler
{
    private ShopManager _shop;
    private ItemCheck _item;

    public void Init(ShopManager shop, ItemCheck item)
    {
        _shop = shop;
        _item = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_shop == null)
        {
            return;
        }
        _shop.HoverSlot(_item);
        HoverTip.Show(this, () => _shop.TipText(_item));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverTip.Hide(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_shop == null)
        {
            return;
        }
        bool right = eventData.button == PointerEventData.InputButton.Right;
        if (right || eventData.button == PointerEventData.InputButton.Left)
        {
            HoverTip.Hide(this);
            _shop.PickByMouse(_item, right);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (_shop == null || Mathf.Abs(eventData.scrollDelta.y) < 0.01f)
        {
            return;
        }
        _shop.PageByMouse(eventData.scrollDelta.y > 0f ? -1 : 1);
    }

    private void OnDisable()
    {
        HoverTip.Hide(this);
    }
}
