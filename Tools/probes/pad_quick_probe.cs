// 패드 퀵슬롯 검증(BasicFeatures.md 14절, 플레이 중·인벤 닫힘): 물약 3개 → 2·3번 칸 등록 → 가상 패드 십자 왼쪽 탭(퀵슬롯 2 사용). 결과는 3초 뒤 [PADQ] 로그
var inv = InventoryManager.instance;
inv.CreatItem("Potion"); inv.CreatItem("Potion"); inv.CreatItem("Potion");
ItemCheck potion = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "Potion") { potion = c; break; } }
QuickSlotBar.AssignFrom(1, potion);
var phm = PlayerHealthManager.Instance;
phm.nowHp = phm.fullHP / 2;
int before = inv.CountItem("Potion");
var pad = UnityEngine.InputSystem.Gamepad.current;
if (pad == null) { pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); }
pad.MakeCurrent();
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.DpadLeft));
DG.Tweening.DOVirtual.DelayedCall(0.5f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true);
DG.Tweening.DOVirtual.DelayedCall(3f, () => Debug.Log("[PADQ] potion " + before + "->" + inv.CountItem("Potion") + " hp=" + phm.nowHp + "/" + phm.fullHP + " label2=" + InputBindings.Display("Quick2", true) + " label3=" + InputBindings.Display("Quick3", true)), true);
return "queued dpad-left, potion=" + before + " hp=" + phm.nowHp;
