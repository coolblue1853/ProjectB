using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 분해 입력(BasicFeatures.md 8절): 인벤토리에서 커서가 장비 위일 때 D(패드 L3)를 두 번(2.5초 안) 누르면 분해한다.
/// 첫 번째는 확인 문구만 띄운다. 실행·규칙은 DismantleService/DismantleRules.
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
        if (!Pressed() || !(string.IsNullOrEmpty(inv.state) || inv.state == "detail"))
        {
            return;
        }
        Request(inv, inv.CursorItem());
    }

    private static bool Pressed()
    {
        Keyboard kb = Keyboard.current;
        Gamepad gp = Gamepad.current;
        return (kb != null && kb.dKey.wasPressedThisFrame) || (gp != null && gp.leftStickButton.wasPressedThisFrame);
    }

    /// <summary>분해 요청(첫 요청은 확인 문구, 같은 아이템 두 번째 요청이 실행). 실행했으면 true. 프로브용 공개 창구</summary>
    public static bool Request(InventoryManager inv, ItemCheck item)
    {
        if (_instance == null || item == null)
        {
            return false;
        }
        DismantleService.Check check = DismantleService.CanTry(inv, item);
        if (check != DismantleService.Check.Ok)
        {
            PickupLog.Warn(Loc.T("Only equipment in the bag can be dismantled"));
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
        bool detail = inv.state == "detail";
        check = DismantleService.TryDismantle(inv, item, out List<KeyValuePair<string, int>> outputs);
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
