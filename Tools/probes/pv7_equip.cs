// PV7: 방패 장착 + 낮 고정 → ShieldGuard/PlayerGuard.Source 확인
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
string wn = "ShellShield";
if (Weapon.Equipped == null || !Weapon.Equipped.name.StartsWith(wn)) {
  var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
  box.LoadPrefab(wn, "Weapon", d.tfName); return "equipping"; }
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
if (amg.equipWeapon != Weapon.Equipped) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); }
var sg = Weapon.Equipped.GetComponentInChildren<ShieldGuard>(true); var cb = Weapon.Equipped.GetComponentInChildren<CounterBuff>(true);
return "w=" + Weapon.Equipped.name + " sg=" + (sg!=null) + " cb=" + (cb!=null) + " src=" + (PlayerGuard.Source!=null) + " day=" + TimeChange.instance.isDaytime;
