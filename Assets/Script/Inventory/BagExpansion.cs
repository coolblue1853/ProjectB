using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 가방 칸 확장(메이플식, InventoryRefactorDesign.md §7). 상점이 열려 있을 때 N(패드 LT+X) 으로 다음 단계를 보여 주고,
/// 확인창에서 예를 고르면 골드로 산다. 칸 수는 Datas.bagUnlocked(구 세이브는 0 → 시작 40칸).
/// </summary>
public class BagExpansion : MonoBehaviour
{
    private static BagExpansionRules _rules;
    private static BagExpansion _instance;

    public static BagExpansionRules Rules
    {
        get
        {
            if (_rules == null)
            {
                TextAsset csv = Resources.Load<TextAsset>("BagExpansion");
                _rules = BagExpansionRules.Parse(csv != null ? csv.text : "");
            }
            return _rules;
        }
    }

    /// <summary>현재 열린 가방 칸 수(세이브 기준)</summary>
    public static int Unlocked => SaveManager.instance != null ? BagExpansionRules.Clamp(SaveManager.instance.datas.bagUnlocked) : BagExpansionRules.StartSlots;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _rules = null;
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[BagExpansion]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<BagExpansion>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        bool pressed = (kb != null && kb.nKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.isPressed && pad.buttonWest.wasPressedThisFrame);
        if (pressed && !ConfirmPopup.IsBlocking && (Buyback.CanUse() || InventoryToolbar.CanUseBag()))
        {
            Press();
        }
    }

    /// <summary>N·패드 LT+X·인벤 창 버튼 공용: 살 수 있으면 공용 확인창(기본 아니오)을 띄우고 예를 고르면 구매, 못 사면 이유 안내</summary>
    public static void Press()
    {
        int cur = Unlocked;
        BagExpansionRules.Block block = Rules.Check(cur, Floor, DatabaseManager.money, out BagExpansionRules.Step next);
        if (block == BagExpansionRules.Block.None)
        {
            ConfirmPopup.Open(new ConfirmRequest
            {
                Title = "가방 확장",
                Message = "가방 " + cur + "→" + next.Slots + "칸\n<color=#FFD633>" + next.Price.ToString("N0") + "G</color> 를 내고 확장할까요?",
                YesLabel = "구매",
                NoLabel = "취소",
                OnYes = () => TryBuy(),
            });
        }
        else if (block == BagExpansionRules.Block.NoGold)
        {
            TryBuy(); // 골드 부족 안내
        }
        else
        {
            Describe();
        }
    }

    /// <summary>인벤 창 버튼 글자: 다음 단계 칸 수·가격, 못 사는 이유</summary>
    public static string ButtonLabel()
    {
        int cur = Unlocked;
        BagExpansionRules.Block block = Rules.Check(cur, Floor, DatabaseManager.money, out BagExpansionRules.Step next);
        switch (block)
        {
            case BagExpansionRules.Block.Max:
                return "가방 최대";
            case BagExpansionRules.Block.NeedFloor:
                return "가방 " + next.MinFloor + "층계~";
            default:
                return "가방 +" + (next.Slots - cur) + "칸 " + next.Price.ToString("N0") + "G";
        }
    }

    private static int Floor => MovementUnlocks.Instance != null ? MovementUnlocks.Instance.HighestFloor : 1;

    /// <summary>다음 단계 안내(살 수 있으면 가격 알림)</summary>
    public static void Describe()
    {
        int cur = Unlocked;
        BagExpansionRules.Block block = Rules.Check(cur, Floor, DatabaseManager.money, out BagExpansionRules.Step next);
        switch (block)
        {
            case BagExpansionRules.Block.Max:
                PickupLog.Warn("가방 " + cur + "칸: 최대 크기");
                break;
            case BagExpansionRules.Block.NeedFloor:
                PickupLog.Warn("가방 " + cur + "칸: 더 높은 층계에서 확장 가능 (" + next.MinFloor + "층계~)");
                break;
            default:
                PickupLog.Notice("가방 " + cur + "→" + next.Slots + "칸 " + next.Price.ToString("N0") + "G");
                break;
        }
    }

    /// <summary>다음 단계를 산다. 성공하면 true</summary>
    public static bool TryBuy()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || SaveManager.instance == null)
        {
            return false;
        }
        int cur = Unlocked;
        BagExpansionRules.Block block = Rules.Check(cur, Floor, DatabaseManager.money, out BagExpansionRules.Step next);
        if (block == BagExpansionRules.Block.NoGold)
        {
            PickupLog.Warn(Loc.T("Not enough gold") + " (" + next.Price.ToString("N0") + "G)");
            return false;
        }
        if (block != BagExpansionRules.Block.None)
        {
            Describe();
            return false;
        }
        SaveManager.instance.datas.bagUnlocked = next.Slots;
        inv.ExpandBag(next.Slots);
        DatabaseManager.money -= next.Price;
        inv.SaveInventory();
        PickupLog.Notice("가방 확장: " + next.Slots + "칸");
        Sfx.Play(SfxId.Money);
        GameLog.Info(GameLog.Cat.Item, "가방 확장 " + cur + "→" + next.Slots + " (" + next.Price + "G)");
        return true;
    }
}
