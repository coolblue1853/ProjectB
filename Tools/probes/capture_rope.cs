// 로프 오르기 인게임 확인(10-30, 뒷모습 오르기 셀이 실제 로프와 맞는지): 씬의 Ladder 태그 로프로 플레이어를 옮겨 컨트롤러처럼 로프 정렬·그리기 순서를 맞추고
// RopeUp 을 재생하며 위로 올린다 → Logs/playtest/rope/f_###.png (배경 켠 채)
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); pc.enabled = false;
GameObject rope = null; float best = float.MaxValue;
foreach (var go in GameObject.FindGameObjectsWithTag("Ladder"))
{
    float d = Mathf.Abs(go.transform.position.x - pc.transform.position.x) + Mathf.Abs(go.transform.position.y - pc.transform.position.y);
    if (d < best) { best = d; rope = go; }
}
if (rope == null) return "no Ladder in scene";
var rb = pc.GetComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.linearVelocity = Vector2.zero;
var col = rope.GetComponent<Collider2D>();
float y0 = col != null ? col.bounds.min.y + 0.6f : rope.transform.position.y;
pc.transform.position = new Vector3(rope.transform.position.x, y0, pc.transform.position.z);
var sR = rope.GetComponent<SpriteRenderer>(); var playerSR = pc.GetComponent<SpriteRenderer>();
// 컨트롤러와 같은 그리기 순서: 로프는 LadderLayer(몸 뒤) 그대로
rig.Play("RopeUp");
System.IO.Directory.CreateDirectory("Logs/playtest/rope");
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.ShortcutExtensions.DOMoveY(pc.transform, y0 + 1.2f, 1.8f), true);
for (int k = 0; k < 54; k++)
{
    int kk = k;
    DG.Tweening.DOVirtual.DelayedCall(0.1f + kk / 30f, () =>
    {
        var go = new GameObject("VidCam"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 1.4f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.15f, 0.17f, 0.21f, 1f);
        Vector3 p = pc.transform.position; go.transform.position = new Vector3(p.x, p.y + 0.7f, p.z - 50f);
        var rt = new RenderTexture(240, 240, 24); cam.targetTexture = rt; cam.Render();
        var tex = new Texture2D(240, 240, TextureFormat.RGB24, false); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 240, 240), 0, 0); RenderTexture.active = null;
        System.IO.File.WriteAllBytes($"Logs/playtest/rope/f_{kk:000}.png", tex.EncodeToPNG());
        cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
    }, true);
}
return "rope " + rope.name + " at " + rope.transform.position + " sort " + (sR != null ? sR.sortingOrder : -1);
