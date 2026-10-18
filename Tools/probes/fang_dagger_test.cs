// Fang Dagger 장착 → F 스킬(Howl of the Pack) → 영혼 늑대 소환 확인. 결과 pb_fang
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("FangDagger"));
box.LoadPrefab("FangDagger", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
PlayerHealthManager.Instance.nowStemina = 999;
var log = new System.Text.StringBuilder("weapon=" + Weapon.Equipped.name + " row=" + d.itemNameT + "; ");
System.AppDomain.CurrentDomain.SetData("pb_fang", log);
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => { var s = Weapon.Equipped.skill[3]; log.Append("F=" + (s ? s.skillName : "null") + " can=" + (s && s.CanActivate(3)) + "; "); Weapon.Equipped.ActiveWeaponSkill(3); }, false);
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => { int n = 0; foreach (var r in UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsSortMode.None)) n++; log.Append("spirits=" + n + " cd=" + SkillCooldown.instance.isCooldown[3]); }, false);
return "scheduled";
