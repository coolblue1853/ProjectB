// Wisp Lantern: 일반 숲 늑대 근처에서 L(유도 탄)·S(공전) 발동 → HomingOrb/OrbitWisps 생성, 늑대 HP 감소. 결과 pb_wl
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("WispLantern")); box.LoadPrefab("WispLantern", "Weapon", d.tfName);
PlayerHealthManager.Instance.nowStemina = 999;
var log = new System.Text.StringBuilder(); System.AppDomain.CurrentDomain.SetData("pb_wl", log);
System.Func<int> wolfHp = () => { int s = 0; foreach (var e in EnemyHealth.Active) if (e.IsMonster) s += e.nowHP; return s; };
DG.Tweening.DOVirtual.DelayedCall(0.5f, () => { var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); log.Append("hp0=" + wolfHp() + "; "); Weapon.Equipped.ActiveWeaponSkill(0); }, false);
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => { int n = 0; foreach (var o in UnityEngine.Object.FindObjectsByType<HomingOrb>(FindObjectsSortMode.None)) n++; log.Append("orbs=" + n + "; "); }, false);
DG.Tweening.DOVirtual.DelayedCall(2.5f, () => { log.Append("hp1=" + wolfHp() + "; "); Weapon.Equipped.ActiveWeaponSkill(1); }, false);
DG.Tweening.DOVirtual.DelayedCall(3.5f, () => { int n = 0; foreach (var o in UnityEngine.Object.FindObjectsByType<OrbitWisps>(FindObjectsSortMode.None)) n++; log.Append("rings=" + n + "; "); }, false);
DG.Tweening.DOVirtual.DelayedCall(6f, () => log.Append("hp2=" + wolfHp()), false);
return "scheduled";
