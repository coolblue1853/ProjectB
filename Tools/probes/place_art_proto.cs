// 시안 PNG를 플레이 중 플레이어 옆 바닥에 세운다(16도트/유닛, 바닥 피벗). 크기 비교 스크린샷용
var sb = new System.Text.StringBuilder();
var p = GameObject.FindWithTag("Player");
if (p == null) { return "no player"; }
var psr = p.GetComponentInChildren<SpriteRenderer>();
var files = new[]{ "Docs/ArtCompare/hero32/heroA.png", "Docs/ArtCompare/hero32/heroB.png", "Docs/ArtCompare/hero32/wolf48.png" };
var flip = new[]{ false, true, false };
var pr = p.GetComponentsInChildren<Renderer>(); Bounds b = pr[0].bounds; foreach (var r in pr) { b.Encapsulate(r.bounds); }
float floor = b.min.y;
sb.AppendLine("player bounds " + b.size + " floor " + floor);
for (int i = 0; i < files.Length; i++)
{
    var tex = new Texture2D(2, 2); tex.LoadImage(System.IO.File.ReadAllBytes(files[i])); tex.filterMode = FilterMode.Point;
    var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 16f);
    var go = new GameObject("ArtProto_" + i);
    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.flipX = flip[i];
    if (psr != null) { sr.sortingLayerID = psr.sortingLayerID; sr.sortingOrder = psr.sortingOrder + 5; }
    go.transform.position = new Vector3(b.center.x + 2.2f + i * 2.4f, floor, 0f);
}
var mobs = GameObject.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
sb.AppendLine("mobs " + mobs.Length);
return sb.ToString();
