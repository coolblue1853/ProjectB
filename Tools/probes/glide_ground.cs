// 성벽 지면 높이 표본(플레이 중, 맵 켜진 뒤)
var f = new UnityEngine.ContactFilter2D(); f.SetLayerMask(UnityEngine.LayerMask.GetMask("Ground")); f.useTriggers = false;
var hits = new UnityEngine.RaycastHit2D[8];
var sb = new System.Text.StringBuilder();
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
float y0 = pc.transform.position.y + 20f;
for (float x = -740f; x <= -140f; x += 10f)
{
    int n = UnityEngine.Physics2D.Raycast(new UnityEngine.Vector2(x, y0), UnityEngine.Vector2.down, f, hits, 120f);
    sb.Append((int)x).Append(':');
    for (int i = 0; i < n; i++) sb.Append(hits[i].point.y.ToString("0.#")).Append(i < n - 1 ? "," : "");
    sb.Append(" | ");
}
return "p" + pc.transform.position + " " + sb;
