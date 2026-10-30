// 프리셋 목록 패널·패드 검증(BasicFeatures.md 13절, 플레이 중, 인벤을 연 뒤): 프리셋1·2 저장 → 패널 표시·본문 → 가상 패드 SELECT 짧게(선택 1→2)·RT(프리셋2 장착)
var inv = InventoryManager.instance;
if (!inv.inventory.activeSelf) return "inventory closed";
var sb = new System.Text.StringBuilder();
GearPresets.SaveCurrent(0);
inv.CreatItem("LeafStaff"); inv.CreatItem("StoneCap");
ItemCheck staff = null, cap = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "LeafStaff") staff = c; if (c.name == "StoneCap") cap = c; }
inv.EquipFromInventory(staff); inv.EquipFromInventory(cap);
GearPresets.SaveCurrent(1);
int miss; GearPresets.Apply(0, out miss);
var w0 = inv.EquippedFor("Weapon");
sb.Append("after apply1 W=" + (w0 != null ? w0.name : "-") + " sel=" + GearPresets.Selected + " | ");
var pad = UnityEngine.InputSystem.Gamepad.current;
if (pad == null) { pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); }
pad.MakeCurrent();
System.Action<UnityEngine.InputSystem.LowLevel.GamepadButton> tap = (b) => {
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(b));
    DG.Tweening.DOVirtual.DelayedCall(0.5f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true);
};
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => tap(UnityEngine.InputSystem.LowLevel.GamepadButton.Select), true);
DG.Tweening.DOVirtual.DelayedCall(3.0f, () => { Debug.Log("[SIDELIST] after select sel=" + GearPresets.Selected); var st = new UnityEngine.InputSystem.LowLevel.GamepadState(); st.rightTrigger = 1f; UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, st); DG.Tweening.DOVirtual.DelayedCall(0.5f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true); }, true);
DG.Tweening.DOVirtual.DelayedCall(6.0f, () => { var w = inv.EquippedFor("Weapon"); Debug.Log("[SIDELIST] after RT W=" + (w != null ? w.name : "-") + " sel=" + GearPresets.Selected + " showing=" + GearPresetPanel.Instance.IsShowing + " body=" + GearPresetPanel.Instance.BodyText.Replace("\n", " / ")); }, true);
return sb + "showing=" + GearPresetPanel.Instance.IsShowing + " padDevices=" + UnityEngine.InputSystem.Gamepad.all.Count;
