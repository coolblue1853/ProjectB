// W-12: 공중에서 평타 → 공격 종료 후 isJumpAnim 복귀 / W-13: 지팡이 평타 직후 반대 방향 입력 → 방향 유지. 먼저 input_bg_on. 결과 pb_airturn
var kb = UnityEngine.InputSystem.Keyboard.current;
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("LeafStaff"));
box.LoadPrefab("LeafStaff", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
var pc = PlayerController.instance;
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_airturn", log);
var f = typeof(PlayerController).GetField("isJumpAnim", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
// W-13: 지상에서 오른쪽 보고 공격 → 0.05초 뒤 왼쪽 화살표 → 0.1초 시점 방향
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
DG.Tweening.DOVirtual.DelayedCall(0.2f, () => Weapon.Equipped.MeleeAttack(), true);
DG.Tweening.DOVirtual.DelayedCall(0.25f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.LeftArrow)), true);
DG.Tweening.DOVirtual.DelayedCall(0.33f, () => log.Append("turnlock scaleX=" + pc.transform.localScale.x.ToString("F2") + "; "), true);
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); log.Append("later scaleX=" + pc.transform.localScale.x.ToString("F2") + "; "); }, true);
// W-12: 위로 띄우고 공격 → 1초 뒤(공격 끝, 아직 공중) isJumpAnim
DG.Tweening.DOVirtual.DelayedCall(1.6f, () => { pc.transform.position += new Vector3(0f, 6f, 0f); pc.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, 2f); }, true);
DG.Tweening.DOVirtual.DelayedCall(1.7f, () => Weapon.Equipped.MeleeAttack(), true);
DG.Tweening.DOVirtual.DelayedCall(2.5f, () => log.Append("air after attack: isGround=" + pc.isGround + " isJumpAnim=" + f.GetValue(pc)), true);
return "scheduled";
