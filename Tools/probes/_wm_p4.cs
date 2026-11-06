var sb = new System.Text.StringBuilder();
var pc = PlayerController.instance;
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon;
string key = w.MasteryKey;
WeaponMasteryService.Data.LoadFrom(new WeaponMasterySave());
string other = "PoisonSword";
sb.Append("key=" + key + " kind=" + w.Kind + " other=" + other);
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
System.Func<string> kill = () =>
{
    float gy = GroundProbe.Y(pc.transform.position + Vector3.right * 3f);
    var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + 3f, gy + 0.6f, 0f), Quaternion.identity);
    var t = go.GetComponent<EnemyHealth>(); t.maxHP = 100000; t.nowHP = 100000;
    int m0 = WeaponMasteryService.Data.GetExp(key); int p0 = LevelSystem.Exp; int pl0 = LevelSystem.Level;
    t.onlyDamage2Enemy(99999999);
    return " dMastery=" + (WeaponMasteryService.Data.GetExp(key) - m0) + " (pLv" + pl0 + "->" + LevelSystem.Level + " pExp " + p0 + "->" + LevelSystem.Exp + ")";
};
sb.Append(" |A no-catchup:" + kill());
WeaponMasteryService.Data.RaiseTo(other, WeaponMasteryRules.TotalExpForLevel(3));
sb.Append(" |B catchup:" + kill());
w.minDmg = 800; w.maxDmg = 1000; w.SetEnhance(w.enhanceLevel);
int min0 = w.damgeArray[0], max0 = w.damgeArray[1];
WeaponMasteryService.Data.RaiseTo(key, WeaponMasteryRules.TotalExpForLevel(5) - 1);
WeaponMasteryService.AddExp(key, 1);
sb.Append(" |Lv5 lv=" + WeaponMasteryService.LevelOf(key) + " min " + min0 + "->" + w.damgeArray[0] + " max " + max0 + "->" + w.damgeArray[1]);
return sb.ToString();
