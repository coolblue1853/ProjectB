using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 포커스 경로 관측(읽기 전용, 봇 재사용). 창이 열려 있을 때 "지금 무엇을 가리키는가"를 한 줄 경로로 만든다.
///  - 가방: inv/상태/페이지/커서아이템id(빈칸 -)
///  - 상점: shop/진열 아이템id (상점 화면에서 본 것)
///  - 그 밖: EventSystem 선택 오브젝트 ui/부모/이름(마우스·Navigation 을 쓰는 창). 자체 커서만 쓰는 창은 경로가 없다(창 이름만 ui 에 보인다).
/// 경로 문자열은 바탕 값(참조·정수)이 바뀔 때만 다시 만든다(할당 최소화).
/// </summary>
public sealed class PlayObsUi
{
    public string Focus = "";

    private GameObject _sel;
    private bool _invOpen;
    private string _invState;
    private int _invPage = -1;
    private ItemCheck _invItem;
    private bool _shopOpen;
    private ItemCheck _shopItem;

    /// <summary>경로가 바뀌면 true</summary>
    public bool Observe(bool uiOpen)
    {
        GameObject sel = null;
        bool invOpen = false;
        string invState = null;
        int invPage = -1;
        ItemCheck invItem = null;
        bool shopOpen = false;
        ItemCheck shopItem = null;
        if (uiOpen)
        {
            EventSystem es = EventSystem.current;
            sel = es != null ? es.currentSelectedGameObject : null;
            InventoryManager inv = InventoryManager.instance;
            if (inv != null)
            {
                invOpen = inv.inventory != null && inv.inventory.activeSelf;
                if (invOpen)
                {
                    invState = inv.state;
                    invPage = inv.nowBox;
                    invItem = inv.CursorItem();
                }
                ShopManager shop = inv.ShopUI;
                shopOpen = shop != null && shop.ShopUI != null && shop.ShopUI.activeInHierarchy;
                if (shopOpen)
                {
                    shopItem = shop.nowShopItem;
                }
            }
        }
        if (sel == _sel && invOpen == _invOpen && invState == _invState && invPage == _invPage
            && invItem == _invItem && shopOpen == _shopOpen && shopItem == _shopItem)
        {
            return false;
        }
        _sel = sel;
        _invOpen = invOpen;
        _invState = invState;
        _invPage = invPage;
        _invItem = invItem;
        _shopOpen = shopOpen;
        _shopItem = shopItem;
        string next = Build();
        if (next == Focus)
        {
            return false;
        }
        Focus = next;
        return true;
    }

    private string Build()
    {
        if (_shopOpen)
        {
            return "shop/" + (_shopItem != null ? _shopItem.name : "-");
        }
        if (_invOpen)
        {
            return "inv/" + (string.IsNullOrEmpty(_invState) ? "browse" : _invState) + "/" + _invPage + "/" + (_invItem != null ? _invItem.name : "-");
        }
        if (_sel != null)
        {
            Transform parent = _sel.transform.parent;
            return "ui/" + (parent != null ? parent.name + "/" : "") + _sel.name;
        }
        return "";
    }
}
