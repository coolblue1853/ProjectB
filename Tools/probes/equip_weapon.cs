// 테스트용 무기 장착 (Resources/Weapon/{name} 프리팹을 장비칸 로직으로 로드). 세이브는 프로브 전후로 백업/복원할 것
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var name = "__WEAPON__";
var data = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(name));
box.LoadPrefab(name, "Weapon", data.tfName);
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
return "equipped " + name + " -> weapon=" + (w ? w.name : "null") + " skills=" + (w ? w.skill.Length : 0);
