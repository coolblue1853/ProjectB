// 주인공 조명 A/B: 같은 장면을 조명 끔/켬으로 두 컷 → Logs/playtest/hero_light___NAME__.png (왼쪽 끔, 오른쪽 켬)
var pc = PlayerController.instance;
var light = pc.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
if (light == null) return "no HeroLight";
float on = light.intensity;
int w = 320;
var strip = new Texture2D(w * 2, w, TextureFormat.RGB24, false);
for (int i = 0; i < 2; i++)
{
    light.intensity = i == 0 ? 0f : on;
    var go = new GameObject("Cam"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 2.2f;
    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
    go.transform.position = new Vector3(pc.transform.position.x, pc.transform.position.y + 0.4f, -50f);
    var rt = new RenderTexture(w, w, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt; cam.Render();
    RenderTexture.active = rt; strip.ReadPixels(new Rect(0, 0, w, w), i * w, 0); RenderTexture.active = null;
    cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt);
}
light.intensity = on;
strip.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/hero_light___NAME__.png", strip.EncodeToPNG());
return "ok intensity=" + on;
