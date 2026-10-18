// W-08: 이동정지 무기로 공중에서 평타 연타 → 낙하하는지 (y 감소). RustedSword 장착 후 위로 띄우고 0.05초마다 평타
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var pc = PlayerController.instance;
var rb = pc.GetComponent<Rigidbody2D>();
pc.transform.position += new Vector3(0f, 4f, 0f);
rb.linearVelocity = Vector2.zero;
float y0 = pc.transform.position.y;
System.AppDomain.CurrentDomain.SetData("pb_air", "start y=" + y0.ToString("F2"));
for (int k = 0; k < 16; k++)
    DG.Tweening.DOVirtual.DelayedCall(0.05f * k, () => { var w = Weapon.Equipped; if (w != null && w.CanMeleeNow) w.MeleeAttack(); }, true);
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => System.AppDomain.CurrentDomain.SetData("pb_air", "start y=" + y0.ToString("F2") + " after0.8s y=" + pc.transform.position.y.ToString("F2") + " vy=" + rb.linearVelocity.y.ToString("F2")), true);
return "scheduled";
