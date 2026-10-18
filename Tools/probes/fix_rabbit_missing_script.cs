// Rabbit_BD 의 GroundCheck 자식에 남은 누락 스크립트(삭제된 스크립트 참조)를 지운다 — 스폰마다 "referenced script is missing" 경고 (2026-10-19 회귀 소크)
string path = "Assets/Prefab/Enemy_BD/Rabbit_BD.prefab";
var root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
int removed = 0;
foreach (var t in root.GetComponentsInChildren<Transform>(true))
{
    removed += UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
}
UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
UnityEditor.PrefabUtility.UnloadPrefabContents(root);
return "removed " + removed;
