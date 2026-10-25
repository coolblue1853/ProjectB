// 편집 모드: 이동 주기 클립(Walk·Run)을 포즈마다 한 칸씩(키 사이 가운데 시점) 크게 → Logs/playtest/rig_cycle.png, 행 = 클립.
// 칼(RustedSword)을 쥔 채 찍는다 — 손 소켓이 손 이동을 따라가는지 함께 본다. 포즈 간격은 클립 길이 / 포즈 수(Walk 8, Run 6).
var prefab = Resources.Load<GameObject>("PixelRig/Adventurer");
var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
go.transform.position = new Vector3(5000f, 5000f, 0f);
go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
var rig = go.GetComponent<PixelRig>();
var wi = (GameObject)UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Weapon/RustedSword"));
var spr = typeof(Weapon).GetField("RightWeqponSprite").GetValue(wi.GetComponent<Weapon>()) as GameObject;
spr.transform.SetParent(rig.GetBoneSocket("RightWeapon"), false);
spr.transform.localPosition = Vector3.zero; spr.transform.localRotation = Quaternion.identity;
rig.AdoptWeapon(spr.transform, false);
UnityEngine.Object.DestroyImmediate(wi);
var keep = typeof(PixelRig).GetMethod("KeepUpright", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var so = new SerializedObject(rig);
var clips = so.FindProperty("_clips");
string[] want = { "Walk", "Run" };
int[] poses = { 8, 6 };
int cols = 8, cell = 200;
var tex = new Texture2D(cols * cell, want.Length * cell, TextureFormat.RGB24, false);
var camGo = new GameObject("SheetCam"); var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 1.3f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
camGo.transform.position = new Vector3(5000.25f, 5001.1f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt;
for (int r = 0; r < want.Length; r++)
{
    AnimationClip clip = null;
    for (int k = 0; k < clips.arraySize; k++) { var e = clips.GetArrayElementAtIndex(k); if (e.FindPropertyRelative("name").stringValue == want[r]) clip = (AnimationClip)e.FindPropertyRelative("clip").objectReferenceValue; }
    float step = clip.length / poses[r];
    for (int c = 0; c < cols; c++)
    {
        if (c >= poses[r]) continue;
        clip.SampleAnimation(go, step * (c + 0.5f));
        keep.Invoke(rig, null);
        cam.Render();
        RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, cell, cell), c * cell, (want.Length - 1 - r) * cell); RenderTexture.active = null;
    }
}
tex.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/rig_cycle.png", tex.EncodeToPNG());
cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
return "ok";
