using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 상점 되사기(BasicFeatures.md 12절): 상점에서 판 아이템을 기억했다가, 상점이 열려 있을 때 U 로 가장 최근 판매분을 판 값 그대로 되산다.
/// 판매 기록은 InventoryManager.SellItem 이 Record 로 넘긴다. 되사기는 InventoryService.Add(티어·강화 보존) 후 골드 차감.
/// 목록은 세션 동안만(게임을 다시 켜면 비어 있음). 패드는 RT(13절). 목록은 BuybackPanel 이 그린다.
/// </summary>
public class Buyback : MonoBehaviour
{
    private static Buyback _instance;
    private static readonly BuybackModel _model = new BuybackModel();
    private static bool _hinted;

    public static BuybackModel Model => _model;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        _model.Clear();
        _selected = 0;
        _hinted = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[Buyback]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<Buyback>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>판매 1건 기록(InventoryManager.SellItem 이 실제로 칸에서 뺀 뒤 부른다)</summary>
    public static void Record(string id, int count, int tier, int upgrade, int price)
    {
        _model.Record(new BuybackModel.Sold { Id = id, Count = count, Tier = tier, Upgrade = upgrade, Price = price });
        _selected = 0; // 새로 판 것이 맨 위 — 선택도 맨 위로
        if (!_hinted)
        {
            _hinted = true; // 세션 첫 판매 때 한 번 키를 알려 준다
            PickupLog.Report("U: 되사기 ([ ] 로 고르기)", 1, "C");
        }
    }

    private static int _selected;

    /// <summary>되살 줄(0 = 가장 최근). [ ] / 패드 LT+Y 로 옮기고 U·RT 로 산다</summary>
    public static int Selected => Mathf.Clamp(_selected, 0, Mathf.Max(0, _model.Count - 1));

    public static void MoveSelection(int delta)
    {
        int n = _model.Count;
        _selected = n <= 0 ? 0 : ((Selected + delta) % n + n) % n;
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (!CanUse())
        {
            return;
        }
        if (kb != null && kb.leftBracketKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if ((kb != null && kb.rightBracketKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.isPressed && pad.buttonNorth.wasPressedThisFrame))
        {
            MoveSelection(1);
        }
        bool pressed = (kb != null && kb.uKey.wasPressedThisFrame) || (pad != null && pad.rightTrigger.wasPressedThisFrame);
        if (pressed)
        {
            TryBuyBack(Selected);
        }
    }

    /// <summary>되사기 키(U·패드 RT)를 받는 때: 상점이 열려 있고 상점 커서 상태일 때</summary>
    public static bool CanUse()
    {
        InventoryManager inv = InventoryManager.instance;
        return inv != null && inv.inventory != null && inv.inventory.activeSelf && inv.ShopGameObject != null && inv.ShopGameObject.activeSelf
            && (string.IsNullOrEmpty(inv.state) || inv.state == "ShopCusorOn");
    }

    /// <summary>가장 최근 판매분을 되산다. 성공하면 true</summary>
    public static bool TryBuyBackLast()
    {
        return TryBuyBack(0);
    }

    /// <summary>목록의 index 번째(0 = 가장 최근)를 판 값 그대로 되산다. 성공하면 true. 골드·가방이 모자라면 목록은 그대로</summary>
    public static bool TryBuyBack(int index)
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null)
        {
            return false;
        }
        BuybackModel.Sold s = _model.PeekAt(index);
        if (s.Id == null)
        {
            PickupLog.Warn(Loc.T("Nothing to buy back"));
            return false;
        }
        if (DatabaseManager.money < s.Price)
        {
            PickupLog.Warn(Loc.T("Not enough gold") + " (" + s.Price.ToString("N0") + "G)");
            return false;
        }
        DatabaseManager db = DatabaseManager.instance;
        ItemData data = db != null ? db.LoadItemData(db.FindItemDataIndex(s.Id)) : null;
        if (data == null)
        {
            _model.TryTakeAt(index, out _); // 시트에서 사라진 이름 — 되살 수 없으니 버린다
            GameLog.Warn(GameLog.Cat.Item, "되사기 불가(시트에 없는 이름) 버림: " + s.Id);
            return false;
        }
        data.tear = s.Tier;
        data.upgrade = s.Upgrade;
        if (!inv.Service.Add(s.Id, s.Count, AddSource.Shop, data))
        {
            PickupLog.ReportFull();
            return false;
        }
        DatabaseManager.money -= s.Price;
        _model.TryTakeAt(index, out _);
        inv.SaveInventory();
        PickupLog.Report(data.itemNameT, s.Count, data.rarity);
        Sfx.Play(SfxId.Money);
        GameLog.Info(GameLog.Cat.Item, "되사기: " + s.Id + " x" + s.Count + " (" + s.Price + "G)");
        return true;
    }
}
