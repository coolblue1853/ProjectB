// 신규 몬스터를 플레이어 옆에 일렬로 세워 크기·지면 정렬 확인 (BT 끔). lineup_clear.cs로 정리
var names = new[]{ "CliffGoblin_BD", "SporeShroom_BD", "CliffSkeleton_BD", "GazeEye_BD", "DeathBringer_BD", "AbyssWizard_BD", "RatThief_BD" };
var pc = PlayerController.instance;
float x = pc.transform.position.x + 1.5f;
var sb = new System.Text.StringBuilder();
foreach (var n in names)
{
    var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + n + ".prefab");
    var go = (GameObject)UnityEngine.Object.Instantiate(pf, new Vector3(x, pc.transform.position.y + 1f, 0), Quaternion.identity);
    go.name = "LINEUP_" + n;
    var bt = go.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>(); if (bt) bt.enabled = false;
    var col = go.GetComponent<BoxCollider2D>();
    x += (col ? col.bounds.size.x : 1f) + 1.2f;
    sb.Append(n + " h=" + (col ? col.bounds.size.y.ToString("F2") : "?") + " ");
}
return sb.ToString();
