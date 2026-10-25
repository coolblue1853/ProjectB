// PixelSlash2D 검증 시트(편집 모드): 행 = 테마(white·gold·red·blue), 열 = 이펙트 4종의 대표 프레임 + 디졸브 0/0.35/0.7(WSlashDown 3프레임).
// 픽셀 1텍셀 = 6px 로 찍어 Logs/playtest/weapon_fx_sheet.png. 색 수 검사는 파이썬에서.
var mat = Resources.Load<Material>("FX/M_PixelSlash");
VfxId[] ids = { VfxId.WSlashDown, VfxId.WSlashSide, VfxId.WThrust, VfxId.WHitStar };
string[] names = { "Px_WSlashDown", "Px_WSlashSide", "Px_WThrust", "Px_WHitStar" };
int[] frameIx = { 3, 3, 2, 1 };
PixelRampId[] ramps = { PixelRampId.White, PixelRampId.Gold, PixelRampId.Red, PixelRampId.Blue };
float[] diss = { 0f, 0.35f, 0.7f };
int cols = ids.Length + diss.Length, cell = 420;
var tex = new Texture2D(cols * cell, ramps.Length * cell, TextureFormat.RGB24, false);
var camGo = new GameObject("FxCam"); var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 70f / 24f / 2f; // 70텍셀 높이 영역
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.30f, 0.33f, 0.36f, 1f);
camGo.transform.position = new Vector3(9000f, 9000f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt;
var go = new GameObject("FxSprite"); var sr = go.AddComponent<SpriteRenderer>(); sr.sharedMaterial = mat;
var mpb = new MaterialPropertyBlock();
int[] rampIds = { Shader.PropertyToID("_C0"), Shader.PropertyToID("_C1"), Shader.PropertyToID("_C2"), Shader.PropertyToID("_C3"), Shader.PropertyToID("_C4") };
for (int r = 0; r < ramps.Length; r++)
{
    Color[] c = PixelRamp.Get(ramps[r]);
    for (int col = 0; col < cols; col++)
    {
        int e = col < ids.Length ? col : 0;
        var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>("Assets/Art/PixelFx/" + names[e] + ".asset");
        sr.sprite = clip.frames[Mathf.Min(frameIx[e], clip.frames.Length - 1)];
        // 스프라이트 경계 중심에 맞춤
        go.transform.position = new Vector3(9000f, 9000f, 0f) - (Vector3)sr.sprite.bounds.center;
        mpb.Clear();
        for (int k = 0; k < 5; k++) mpb.SetColor(rampIds[k], c[k]);
        mpb.SetFloat("_Dissolve", col < ids.Length ? 0f : diss[col - ids.Length]);
        sr.SetPropertyBlock(mpb);
        cam.Render();
        RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, cell, cell), col * cell, (ramps.Length - 1 - r) * cell); RenderTexture.active = null;
    }
}
tex.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/weapon_fx_sheet.png", tex.EncodeToPNG());
cam.targetTexture = null;
UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
return "ok " + cols + "x" + ramps.Length;
