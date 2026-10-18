// W-09: 평타(X) 직후 후딜 중에 스킬(A)을 누르면 버퍼로 발동되는지. 먼저 input_bg_on 실행. 결과는 pb_skillbuf
var kb = UnityEngine.InputSystem.Keyboard.current;
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
PlayerHealthManager.Instance.nowStemina = 999;
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_skillbuf", log);
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.X)), true);
DG.Tweening.DOVirtual.DelayedCall(0.36f, () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); log.Append("afterX canMelee=" + Weapon.Equipped.CanMeleeNow + " combo=" + Weapon.Equipped.nowConboCount + "; "); }, true);
// 후딜 중(평타 0.1초 뒤) 스킬 A
DG.Tweening.DOVirtual.DelayedCall(0.42f, () => { log.Append("pressA canSkill=" + Weapon.Equipped.skill[0].CanActivate(0) + "; "); UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.A)); }, true);
DG.Tweening.DOVirtual.DelayedCall(0.48f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
DG.Tweening.DOVirtual.DelayedCall(1.2f, () => log.Append("skill0 cooldown=" + SkillCooldown.instance.isCooldown[0]), true);
return "scheduled";
