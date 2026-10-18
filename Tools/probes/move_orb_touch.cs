// B-121: 계보 무기 공용 판정 WispLantern/WispTouch → DamageObject/Common/OrbTouch (GUID 유지 이동, 재실행 안전)
string DO = "Assets/Resources/Weapon/DamageObject/";
string src = DO + "WispLantern/WispTouch.prefab", dst = DO + "Common/OrbTouch.prefab";
if (!AssetDatabase.IsValidFolder(DO + "Common"))
{
    AssetDatabase.CreateFolder("Assets/Resources/Weapon/DamageObject", "Common");
}
if (!System.IO.File.Exists(src))
{
    return "already moved: guid " + AssetDatabase.AssetPathToGUID(dst);
}
string g0 = AssetDatabase.AssetPathToGUID(src);
string err = AssetDatabase.MoveAsset(src, dst);
AssetDatabase.SaveAssets();
string g1 = AssetDatabase.AssetPathToGUID(dst);
var go = AssetDatabase.LoadAssetAtPath<GameObject>(dst);
return "err=" + (string.IsNullOrEmpty(err) ? "none" : err) + " guid " + g0 + " -> " + g1 + " same=" + (g0 == g1) + " root=" + (go == null ? "null" : go.name);
