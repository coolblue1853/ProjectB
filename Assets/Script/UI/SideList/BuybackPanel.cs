using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 상점 되사기 목록(BasicFeatures.md 13절): 상점이 열려 있을 때 창 옆에 최근 판매분(최대 12, 최신이 위)을 보여 준다.
/// 맨 위 줄이 U·패드 RT 로 되살 것. 골드가 모자라는 줄은 이름 회색·가격 빨강. 실행은 Buyback 이 한다.
/// </summary>
public class BuybackPanel : SideListPanel
{
    private static BuybackPanel _instance;
    private const int NameMax = 14;

    private readonly StringBuilder _sb = new StringBuilder();
    private int _version = -1, _money = -1;
    private bool _korean;
    private Transform _root;
    private RectTransform _anchor;

    public static BuybackPanel Instance => _instance;

    protected override float Width => 560f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[BuybackPanel]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<BuybackPanel>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>판 것이 있을 때만(빈 목록 패널로 상점 화면을 가리지 않게)</summary>
    protected override bool Visible()
    {
        return Buyback.Model.Count > 0 && Buyback.CanUse();
    }

    protected override RectTransform Anchor()
    {
        // 상점 진열칸 배경(씬 계층 이름 "Background"). 없으면 상점 루트. 루트가 바뀔 때만 다시 찾는다
        Transform root = InventoryManager.instance.ShopGameObject.transform;
        if (root != _root || _anchor == null)
        {
            _root = root;
            Transform bg = root.Find("Background");
            _anchor = (bg != null ? bg : root) as RectTransform;
        }
        return _anchor;
    }

    /// <summary>골드가 바뀌면 살 수 있는 줄 색이 바뀐다</summary>
    protected override bool Changed()
    {
        int v = Buyback.Model.Version;
        int money = DatabaseManager.money;
        bool ko = Loc.IsKorean;
        if (v == _version && money == _money && ko == _korean)
        {
            return false;
        }
        _version = v;
        _money = money;
        _korean = ko;
        return true;
    }

    protected override void Render(TextMeshProUGUI title, TextMeshProUGUI body)
    {
        BuybackModel model = Buyback.Model;
        title.text = Loc.T("BUY BACK") + "  <size=75%><color=#A0A0A0>" + model.Count + "/" + BuybackModel.Capacity + "</color></size>";
        _sb.Clear();
        for (int i = 0; i < model.Count; i++)
        {
            BuybackModel.Sold s = model.PeekAt(i);
            ItemData d = LookUp(s.Id);
            bool afford = DatabaseManager.money >= s.Price;
            _sb.Append(i == 0 ? "<color=#FFD040>> </color>" : "   ");
            if (d == null)
            {
                _sb.Append(s.Id);
            }
            else if (afford)
            {
                _sb.Append(TooltipBuilder.Name(Shorten(d.itemNameT, NameMax), d.rarity, s.Upgrade));
            }
            else
            {
                _sb.Append("<color=#808080>").Append(EnhanceRules.DisplayName(Shorten(d.itemNameT, NameMax), s.Upgrade)).Append("</color>");
            }
            if (s.Count > 1)
            {
                _sb.Append(" x").Append(s.Count);
            }
            _sb.Append("<pos=80%>").Append(afford ? "<color=#FFE08A>" : "<color=#FF5A4D>").Append(s.Price.ToString("N0")).Append("G</color>\n");
        }
        _sb.Append("<size=80%><color=#A0A0A0>").Append(Loc.T("U / Pad RT: buy back top")).Append("</color></size>");
        body.text = _sb.ToString();
    }
}
