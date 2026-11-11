// REWORK-12 P5: Gatekeeper's Oath Gate Cleave 변형 프리팹 생성(공유 원본은 복제만 하고 무수정). 데이터 변형만(스케일·관통), 그래픽 없음
string src = "Assets/Resources/Weapon/DamageObject/GatekeepersOath/GatekeepersOathSlashWave.prefab";
string dst = "Assets/Resources/Weapon/Variants/GateCleave_Wide.prefab";
if (UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(dst) != null) return "exists";
if (!UnityEditor.AssetDatabase.CopyAsset(src, dst)) return "copy fail";
var root = UnityEditor.PrefabUtility.LoadPrefabContents(dst);
var d = root.GetComponent<DamageObject>();
string before = "scale=" + root.transform.localScale + " max=" + d.maxDamagedEnemies + " launch=" + d.isLaunch;
root.transform.localScale *= 1.7f;
d.maxDamagedEnemies += 2;
UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, dst);
UnityEditor.PrefabUtility.UnloadPrefabContents(root);
UnityEditor.AssetDatabase.SaveAssets();
return "made; base " + before;
