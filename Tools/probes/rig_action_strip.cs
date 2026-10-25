// 한 프로브 안에서 동작 실행 → 게임 시간 기준 일정 간격 연속 촬영(프로브 컴파일 멈춤 1초가 사이에 끼지 않게).
// __ACTION__: jump | attack | dash | combo | combo3(콤보 1·2·3단 생성 함수를 0.42초 간격 직접 호출)  /  결과: Logs/playtest/rig_act___ACTION__.png + 클립 기록(AppDomain pb_rigact)
var pc = PlayerController.instance;
var rig = pc.Rig;
var fReq = typeof(PixelRig).GetField("_requested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
string action = "__ACTION__";
bool loco = action == "walk" || action == "run";
int n = action == "combo3" ? 12 : action == "jump" ? 10 : 8, w = 256; float step = action == "combo3" ? 0.1f : loco ? 0.06f : action == "jump" ? 0.07f : 0.08f;
float lead = loco ? 0.3f : 0.02f; // 걷기·달리기는 가속이 끝난 뒤부터 찍는다
float camSize = loco || action == "jump" ? 1.15f : 1.6f; // 이동 동작은 크게(빈칸 확인용)
var strip = new Texture2D(w * n, w, TextureFormat.RGB24, false);
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_rigact", log);
System.Action<int> shot = (int i) =>
{
    var go = new GameObject("CloseupCam");
    var cam = go.AddComponent<Camera>();
    cam.orthographic = true; cam.orthographicSize = camSize;
    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
    Vector3 p = pc.transform.position;
    go.transform.position = new Vector3(p.x + 0.2f, p.y + (camSize < 1.5f ? 0.15f : 0.5f), p.z - 50f);
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
// walk/run: 오른쪽(+Shift) 키를 누른 채 찍고 마지막 컷 뒤에 뗀다. 먼저 input_bg_on
var kbd = UnityEngine.InputSystem.Keyboard.current;
if (action == "walk") UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow));
// 달리기는 이동 중 Shift 를 누르는 순간 켜지는 토글 → 오른쪽 먼저, 0.12초 뒤 Shift 탭
if (action == "run")
{
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow));
    DG.Tweening.DOVirtual.DelayedCall(0.12f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow, UnityEngine.InputSystem.Key.LeftShift)), false);
    DG.Tweening.DOVirtual.DelayedCall(0.2f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow)), false);
}
if (loco) DG.Tweening.DOVirtual.DelayedCall(lead + n * step, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState()), false);
if (action == "dash") typeof(PlayerController).GetMethod("Dash", flags).Invoke(pc, null);
if (action == "attack") Weapon.Equipped.MeleeAttack();
if (action == "combo3")
{
    var cap = typeof(Weapon).GetMethod("CreatAttackPrefab", flags, null, new System.Type[] { typeof(int) }, null);
    for (int st = 1; st <= 3; st++)
    {
        int s3 = st;
        DG.Tweening.DOVirtual.DelayedCall((s3 - 1) * 0.42f, () => cap.Invoke(Weapon.Equipped, new object[] { s3 }), false);
    }
}
if (action == "combo")
{
    Weapon.Equipped.MeleeAttack();
    DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Weapon.Equipped.MeleeAttack(), false);
}
for (int k = 0; k < n; k++)
{
    int kk = k;
    DG.Tweening.DOVirtual.DelayedCall(lead + kk * step, () => shot(kk), false);
}
return "scheduled " + action;
