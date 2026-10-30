var f = new UnityEngine.ContactFilter2D(); f.SetLayerMask(UnityEngine.LayerMask.GetMask("Ground")); f.useTriggers = false;
var hits = new UnityEngine.RaycastHit2D[8];
var sb = new System.Text.StringBuilder();
for (float x = -440f; x <= -240f; x += 4f)
{
    int n = UnityEngine.Physics2D.Raycast(new UnityEngine.Vector2(x, 9f), UnityEngine.Vector2.down, f, hits, 50f);
    sb.Append((int)x).Append(':').Append(n > 0 ? hits[0].point.y.ToString("0.#") : "-");
    // 위쪽 장애물: y 9 위로 40 안에 막는 것
    int m = UnityEngine.Physics2D.Raycast(new UnityEngine.Vector2(x, 9f), UnityEngine.Vector2.up, f, hits, 40f);
    if (m > 0) sb.Append("^").Append(hits[0].point.y.ToString("0.#"));
    sb.Append(' ');
}
return sb.ToString();
