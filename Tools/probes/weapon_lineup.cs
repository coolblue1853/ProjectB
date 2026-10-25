// 편집 모드: 픽셀 리그 주인공이 무기 프리팹을 하나씩 쥔 모습(대기 + 내려베기 중간) → Logs/playtest/weapon_lineup.png
// 열 = 무기, 행 = 대기 / 내려베기 0.12초. 오른손·왼손 무기 그림 모두 붙인다(실제 장착과 같은 AdoptWeapon 경로)
var prefab = Resources.Load<GameObject>("PixelRig/Adventurer");
var weapons = Resources.LoadAll<GameObject>("Weapon");
var list = new System.Collections.Generic.List<GameObject>();
foreach (var w in weapons) if (w.GetComponent<Weapon>() != null) list.Add(w);
int cell = 150, cols = list.Count;
var tex = new Texture2D(cols * cell, 2 * cell, TextureFormat.RGB24, false);
var camGo = new GameObject("LineupCam"); var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 1.7f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
camGo.transform.position = new Vector3(7000.3f, 7001.2f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt;
var names = new System.Text.StringBuilder();
var fR = typeof(Weapon).GetField("RightWeqponSprite"); var fL = typeof(Weapon).GetField("leftWeqponSprite");
for (int i = 0; i < cols; i++)
{
    var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
    go.transform.position = new Vector3(7000f, 7000f, 0f);
    go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    var rig = go.GetComponent<PixelRig>();
    var wi = (GameObject)UnityEngine.Object.Instantiate(list[i]);
    var w = wi.GetComponent<Weapon>();
    var r = fR.GetValue(w) as GameObject; var l = fL.GetValue(w) as GameObject;
    if (r != null) { r.transform.SetParent(rig.GetBoneSocket("RightWeapon"), false); r.transform.localPosition = new Vector3(0, 0, 25f); r.transform.localRotation = Quaternion.identity; rig.AdoptWeapon(r.transform, false); }
    if (l != null) { l.transform.SetParent(rig.GetBoneSocket("LeftWeapon"), false); l.transform.localPosition = new Vector3(0, 0, 35f); l.transform.localRotation = Quaternion.identity; rig.AdoptWeapon(l.transform, true); }
    UnityEngine.Object.DestroyImmediate(wi);
    var so = new SerializedObject(rig); var clips = so.FindProperty("_clips");
    AnimationClip idle = null, swing = null;
    for (int k = 0; k < clips.arraySize; k++) { var e = clips.GetArrayElementAtIndex(k); var n = e.FindPropertyRelative("name").stringValue; var c = (AnimationClip)e.FindPropertyRelative("clip").objectReferenceValue; if (n == "Idle") idle = c; if (n == "SwingDown") swing = c; }
    var keep = rig.GetType().GetMethod("KeepUpright", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    for (int row = 0; row < 2; row++)
    {
        (row == 0 ? idle : swing).SampleAnimation(go, row == 0 ? 0f : 0.12f);
        keep.Invoke(rig, null);
        cam.Render();
        RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, cell, cell), i * cell, (1 - row) * cell); RenderTexture.active = null;
    }
    names.Append(list[i].name + ",");
    UnityEngine.Object.DestroyImmediate(go);
}
tex.Apply();
System.IO.File.WriteAllBytes("Logs/playtest/weapon_lineup.png", tex.EncodeToPNG());
cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt);
return names.ToString();
