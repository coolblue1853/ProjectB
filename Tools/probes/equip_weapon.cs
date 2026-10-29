// 테스트용 무기 장착 (Resources/Weapon/{name} 프리팹을 장비칸 로직으로 로드·활성화). 세이브는 프로브 전후로 백업/복원할 것
// 이전 무기(AttackManager.equipWeapon 과 남아 있는 Weapon)는 해제·파괴하고 세트 수를 되돌린다 — 예전에는 두 무기가 동시에 활성이라
// FindFirstObjectByType<Weapon> 이 입력을 받지 않는 무기를 잡았다.
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var name = "__WEAPON__";
var db = DatabaseManager.instance;
string removed = "";
if (am != null && am.equipWeapon != null)
{
    am.UnEquipMainWeaopon();
    am.equipWeapon = null;
}
foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    string oldName = old.name.Replace("(Clone)", "").Trim();
    int idx = db.FindItemDataIndex(oldName);
    if (idx >= 0)
    {
        var od = db.LoadItemData(idx);
        if (od != null && !string.IsNullOrEmpty(od.tfName)) { DatabaseManager.MinusSetDict(od.tfName, 1); }
    }
    old.gameObject.SetActive(false);
    UnityEngine.Object.Destroy(old.gameObject);
    removed += oldName + " ";
}
// 장비칸이 기억하는 무기 참조를 끊어야 ActivePrefab 이 새 무기를 연결한다
var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
if (wf != null) { wf.SetValue(box, null); }
box.equipPrefab = null;
var data = db.LoadItemData(db.FindItemDataIndex(name));
box.LoadPrefab(name, "Weapon", data.tfName);
box.ActivePrefab("Weapon");
var w = am != null ? am.equipWeapon : null;
return (wf == null ? "WARN EquipBoxCheck.weapon 필드 없음 — 새 무기 미연결 가능. " : "") + "removed [" + removed + "] equipped " + name + " -> weapon=" + (w ? w.name : "null") + " skills=" + (w ? w.skill.Length : 0);
