// 편집 모드: VfxLibrary 의 도트 이펙트 전부를 (25%·50%·75% 프레임) 한 장으로 → Logs/playtest/fx_lineup.png, 행 = 이펙트
var lib = Resources.Load<VfxLibrary>("FX/VfxLibrary");
var pxMat = Resources.Load<Material>("FX/M_PixelFx");
var slashMat = Resources.Load<Material>("FX/M_PixelSlash");
var list = new System.Collections.Generic.List<VfxLibrary.Entry>();
foreach (var e in lib.entries) if (e.pixel != null && e.pixel.frames != null && e.pixel.frames.Length > 0) list.Add(e);
int cols = 3, cell = 180;
var tex = new Texture2D(cols * cell, list.Count * cell, TextureFormat.RGB24, false);
var camGo = new GameObject("FxCam"); var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 2.4f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.33f, 0.38f, 0.33f, 1f);
camGo.transform.position = new Vector3(8000f, 8000f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt;
var go = new GameObject("FxSprite"); var sr = go.AddComponent<SpriteRenderer>();
var names = new System.Text.StringBuilder();
var mpb = new MaterialPropertyBlock();
for (int r = 0; r < list.Count; r++)
{
    var e = list[r];
    bool themed = e.id >= VfxId.WSlashDown && e.id <= VfxId.WHitStar;
    sr.sharedMaterial = themed ? slashMat : pxMat;
    mpb.Clear();
    if (themed) { var c = PixelRamp.Get(PixelRampId.Gold); string[] ids = { "_C0", "_C1", "_C2", "_C3", "_C4" }; for (int k = 0; k < 5; k++) mpb.SetColor(ids[k], c[k]); }
    sr.SetPropertyBlock(mpb);
    for (int c2 = 0; c2 < cols; c2++)
    {
        int f = Mathf.Clamp((int)(e.pixel.frames.Length * (c2 + 1) / 4f), 0, e.pixel.frames.Length - 1);
        sr.sprite = e.pixel.frames[f];
        go.transform.position = new Vector3(8000f, 8000f, 0f) - (Vector3)sr.sprite.bounds.center + Vector3.zero;
        cam.Render();
        RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, cell, cell), c2 * cell, (list.Count - 1 - r) * cell); RenderTexture.active = null;
    }
    names.Append(r + ":" + e.id + " ");
}
tex.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/fx_lineup.png", tex.EncodeToPNG());
cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
return names.ToString();
