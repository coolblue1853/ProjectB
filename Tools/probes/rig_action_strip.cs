// 한 프로브 안에서 동작 실행 → 게임 시간 기준 일정 간격 연속 촬영(프로브 컴파일 멈춤 1초가 사이에 끼지 않게).
// __ACTION__: jump | attack | dash | combo  /  결과: Logs/playtest/rig_act___ACTION__.png + 클립 기록(AppDomain pb_rigact)
var pc = PlayerController.instance;
var rig = pc.Rig;
var fReq = typeof(PixelRig).GetField("_requested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
string action = "__ACTION__";
int n = 8, w = 256; float step = 0.08f;
var strip = new Texture2D(w * n, w, TextureFormat.RGB24, false);
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_rigact", log);
System.Action<int> shot = (int i) =>
{
    var go = new GameObject("CloseupCam");
    var cam = go.AddComponent<Camera>();
    cam.orthographic = true; cam.orthographicSize = 1.6f;
    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
    Vector3 p = pc.transform.position;
    go.transform.position = new Vector3(p.x + 0.3f, p.y + 0.5f, p.z - 50f);
    var rt = new RenderTexture(w, w, 24); rt.filterMode = FilterMode.Point;
    cam.targetTexture = rt; cam.Render();
    RenderTexture.active = rt; strip.ReadPixels(new Rect(0, 0, w, w), i * w, 0); RenderTexture.active = null;
    cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt);
    log.Append(i + ":" + (fReq != null ? fReq.GetValue(rig) : "?") + "@y" + pc.transform.position.y.ToString("F2") + " ");
    if (i == n - 1)
    {
        strip.Apply();
        System.IO.File.WriteAllBytes("Logs/playtest/rig_act___ACTION__.png", strip.EncodeToPNG());
    }
};
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
if (action == "jump") typeof(PlayerController).GetMethod("Jump", flags).Invoke(pc, null);
if (action == "dash") typeof(PlayerController).GetMethod("Dash", flags).Invoke(pc, null);
if (action == "attack") Weapon.Equipped.MeleeAttack();
if (action == "combo")
{
    Weapon.Equipped.MeleeAttack();
    DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Weapon.Equipped.MeleeAttack(), false);
}
for (int k = 0; k < n; k++)
{
    int kk = k;
    DG.Tweening.DOVirtual.DelayedCall(0.02f + kk * step, () => shot(kk), false);
}
return "scheduled " + action;
