// B-121 검증: OrbTouch 를 참조하던 프리팹들이 이동 뒤에도 같은 에셋을 가리키는지
string dst = "Assets/Resources/Weapon/DamageObject/Common/OrbTouch.prefab";
var target = AssetDatabase.LoadAssetAtPath<GameObject>(dst);
if (target == null)
{
    return "OrbTouch missing";
}
string[] users = { "FaeWand/FaeOrb", "SlimeMace/GelHopper", "SlimeMace/SlimeHopper", "WispLantern/WispOrb", "WispLantern/WispRing" };
var sb = new System.Text.StringBuilder();
foreach (string u in users)
{
    var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Weapon/DamageObject/" + u + ".prefab");
    if (go == null)
    {
        sb.Append(u + " missing prefab; ");
        continue;
    }
    int hits = 0, missing = 0;
    foreach (var c in go.GetComponentsInChildren<Component>(true))
    {
        if (c == null)
        {
            continue;
        }
        var so = new SerializedObject(c);
        var it = so.GetIterator();
        while (it.Next(true))
        {
            if (it.propertyType != SerializedPropertyType.ObjectReference)
            {
                continue;
            }
            if (it.objectReferenceValue == target)
            {
                hits++;
            }
            else if (it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
            {
                missing++;
            }
        }
    }
    sb.Append(u + " ref=" + hits + " missing=" + missing + "; ");
}
return sb.ToString();
