// 가상 키를 누른 채로 둔다(__KEYS__ = 쉼표로 구분한 Key 이름, 비우면 전부 뗌). 먼저 input_bg_on.
// 결과: 지금 리그가 재생 중인 클립·위치·속도 + Logs/playtest/rig_k___NAME__.png 확대 캡처
var kb = UnityEngine.InputSystem.Keyboard.current;
string keys = "__KEYS__";
var list = new System.Collections.Generic.List<UnityEngine.InputSystem.Key>();
foreach (var k in keys.Split(',')) { if (k.Trim().Length > 0) list.Add((UnityEngine.InputSystem.Key)System.Enum.Parse(typeof(UnityEngine.InputSystem.Key), k.Trim())); }
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(list.ToArray()));
var pc = PlayerController.instance;
var rb = pc.GetComponent<Rigidbody2D>();
var rig = pc.Rig;
var f = typeof(PixelRig).GetField("_requested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
string clip = f != null && rig is PixelRig ? (string)f.GetValue(rig) : "?";
var go = new GameObject("CloseupCam");
var cam = go.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 1.4f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
Vector3 p = pc.transform.position;
go.transform.position = new Vector3(p.x, p.y + 0.3f, p.z - 50f);
var rt = new RenderTexture(384, 384, 24); rt.filterMode = FilterMode.Point;
cam.targetTexture = rt; cam.Render();
RenderTexture.active = rt;
var tex = new Texture2D(384, 384, TextureFormat.RGB24, false);
tex.ReadPixels(new Rect(0, 0, 384, 384), 0, 0); tex.Apply();
RenderTexture.active = null; cam.targetTexture = null;
System.IO.File.WriteAllBytes("Logs/playtest/rig_k___NAME__.png", tex.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt);
return "keys=[" + keys + "] clip=" + clip + " pos=" + pc.transform.position.ToString("F2") + " vel=" + rb.linearVelocity.ToString("F2") + " scaleX=" + pc.transform.localScale.x + " ground=" + pc.isGround;
