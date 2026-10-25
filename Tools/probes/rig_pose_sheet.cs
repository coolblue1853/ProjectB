// 편집 모드: __CLIPS__(쉼표 목록) 클립을 한 줄에 한 칸씩(클립 가운데 시점) 칼을 쥔 채 크게 → Logs/playtest/rig_poses.png
// 점프 단계·착지·제동처럼 한두 포즈짜리 클립 비교용(이동 주기는 rig_cycle_sheet.cs)
var go = (GameObject)UnityEngine.Object.Instantiate(Resources.Load<GameObject>("PixelRig/Adventurer"));
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
var clips = new SerializedObject(rig).FindProperty("_clips");
string[] want = "__CLIPS__".Split(',');
int cell = 200;
var tex = new Texture2D(want.Length * cell, cell, TextureFormat.RGB24, false);
var camGo = new GameObject("SheetCam"); var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 1.3f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
camGo.transform.position = new Vector3(5000.25f, 5001.1f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt;
var miss = new System.Text.StringBuilder();
for (int c = 0; c < want.Length; c++)
{
    AnimationClip clip = null;
    for (int k = 0; k < clips.arraySize; k++) { var e = clips.GetArrayElementAtIndex(k); if (e.FindPropertyRelative("name").stringValue == want[c]) clip = (AnimationClip)e.FindPropertyRelative("clip").objectReferenceValue; }
    if (clip == null) { miss.Append(want[c] + " "); continue; }
    clip.SampleAnimation(go, clip.length * 0.5f);
    keep.Invoke(rig, null);
    cam.Render();
    RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, cell, cell), c * cell, 0); RenderTexture.active = null;
}
tex.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/rig_poses.png", tex.EncodeToPNG());
cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
return miss.Length > 0 ? "없음: " + miss : "ok";
