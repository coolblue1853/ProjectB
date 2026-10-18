// 열린 Import Unity Package 창 닫기 + TMP Essential Resources를 대화상자 없이 가져오기
var closed = 0;
foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
{
    var tn = w.GetType().Name;
    if (tn == "PackageImport" || tn.Contains("Welcome")) { w.Close(); closed++; }
}
var path = System.IO.Path.GetFullPath("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
var exists = System.IO.File.Exists(path);
if (exists) AssetDatabase.ImportPackage(path, false);
return "closed=" + closed + " import=" + exists + " " + path;
