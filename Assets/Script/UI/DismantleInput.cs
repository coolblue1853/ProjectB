using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 분해 입력(BasicFeatures.md 8절): 인벤토리에서 커서가 장비 위일 때 D(패드 L3)를 누르면 공용 확인창(ConfirmPopup, 기본 아니오)이 뜨고 예를 고르면 분해한다.
/// Request(프로브용)는 예전처럼 2.5초 안에 두 번 요청하면 실행한다. 실행·규칙은 DismantleService/DismantleRules.
/// </summary>
public class DismantleInput : MonoBehaviour
{
    private const float ConfirmWindow = 2.5f;

    private static DismantleInput _instance;

    private ItemCheck _pending;
    private float _pendingUntil;

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
        var go = new GameObject("[DismantleInput]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<DismantleInput>();
    }

    private void Update()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.inventory == null || !inv.inventory.activeSelf)
        {
            _pending = null;
            return;
        }
        if (ConfirmPopup.IsBlocking || InventorySearch.IsBlocking || !Pressed() || !(string.IsNullOrEmpty(inv.state) || inv.state == "detail"))
        {
            return;
        }
        Ask(inv, inv.CursorItem());
    }

    /// <summary>분해 확인창을 연다(분해할 수 없으면 이유만 알린다). 예를 고르면 Execute</summary>
    private static void Ask(InventoryManager inv, ItemCheck item)
    {
        if (item == null || !Validate(inv, item))
        {
            return;
        }
        ConfirmPopup.Open(new ConfirmRequest
        {
            Title = "장비 분해",
            Message = "<color=#FFD633>" + item.itemData.itemNameT + "</color>\n분해할까요?\n<color=#BBBBBB>장비는 사라지고 재료가 남습니다.</color>",
            YesLabel = "분해",
            NoLabel = "취소",
            OnYes = () => Execute(inv, item),
        });
    }

    private static bool Pressed()
    {
        Keyboard kb = Keyboard.current;
        Gamepad gp = Gamepad.current;
        return (kb != null && kb.dKey.wasPressedThisFrame) || (gp != null && !gp.leftTrigger.isPressed && gp.leftStickButton.wasPressedThisFrame);
    }

    /// <summary>분해 요청(첫 요청은 확인 문구, 같은 아이템 두 번째 요청이 실행). 실행했으면 true. 프로브용 공개 창구</summary>
    public static bool Request(InventoryManager inv, ItemCheck item)
    {
        if (_instance == null || item == null)
        {
            return false;
        }
        if (!Validate(inv, item))
        {
            _instance._pending = null;
            return false;
        }
        float now = Time.unscaledTime;
        if (_instance._pending != item || now > _instance._pendingUntil)
        {
            _instance._pending = item;
            _instance._pendingUntil = now + ConfirmWindow;
            PickupLog.Warn(Loc.F("Press D again to dismantle {0}", item.itemData.itemNameT));
            return false;
        }
        _instance._pending = null;
        return Execute(inv, item);
    }

    /// <summary>분해할 수 있는지 본다. 못 하면 이유를 알리고 false</summary>
    private static bool Validate(InventoryManager inv, ItemCheck item)
    {
        DismantleService.Check check = DismantleService.CanTry(inv, item);
        if (check == DismantleService.Check.Ok)
        {
            return true;
        }
        string why = Loc.T("Only equipment in the bag can be dismantled");
        if (check == DismantleService.Check.Locked)
        {
            why = "잠금 해제 후 분해할 수 있습니다";
        }
        else if (check == DismantleService.Check.Rented)
        {
            why = "시연대 대여품은 반납(무기 고르기) 전까지 분해할 수 없습니다";
        }
        PickupLog.Warn(why);
        return false;
    }

    private static bool Execute(InventoryManager inv, ItemCheck item)
    {
        bool detail = inv.state == "detail";
        DismantleService.Check check = DismantleService.TryDismantle(inv, item, out List<KeyValuePair<string, int>> outputs);
        if (check == DismantleService.Check.NoRoom)
        {
            PickupLog.ReportFull();
            return false;
        }
        if (check != DismantleService.Check.Ok)
        {
            return false;
        }
        if (detail)
        {
            inv.DetailOff();
        }
        foreach (KeyValuePair<string, int> o in outputs)
        {
            ItemData d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(o.Key));
            PickupLog.Report(d != null ? d.itemNameT : o.Key, o.Value, d != null ? d.rarity : "C");
        }
        Sfx.Play(SfxId.Craft);
        return true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
