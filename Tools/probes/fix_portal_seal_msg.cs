// 봉인 토스트 문구 단축(PickupLog 칸 넘침): 씬의 모든 PortalSeal._sealedMessage → 코드 기본값과 같은 짧은 문구. 에디트 모드, 재실행 안전
int n = 0;
UnityEngine.SceneManagement.Scene scene = default(UnityEngine.SceneManagement.Scene);
foreach (var ps in UnityEngine.Object.FindObjectsByType<PortalSeal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    var so = new SerializedObject(ps);
    var p = so.FindProperty("_sealedMessage");
    if (p.stringValue != "Path sealed by the Tower.") { p.stringValue = "Path sealed by the Tower."; so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(ps); n++; }
    scene = ps.gameObject.scene;
}
if (n > 0) { UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene); }
return "changed " + n;
