// 빈칸 검출용: 무기 없이 모든 클립 10시점을 자홍 배경으로 → Logs/playtest/rig_holes.png (Tools/pixelfx/find_rig_holes.py 가 판정)
// 편집 모드: 픽셀 리그 프리팹의 모든 클립을 6등분 시점으로 샘플링해 한 장(행=클립)으로 찍는다 → Logs/playtest/rig_holes.png
//  이 비어 있지 않으면 그 무기 프리팹의 RightWeqponSprite 를 오른손 소켓에 붙여 함께 찍는다.
var prefab = Resources.Load<GameObject>("PixelRig/Adventurer");
var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
go.transform.position = new Vector3(5000f, 5000f, 0f);
go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
var rig = go.GetComponent<PixelRig>();
string wname = "";
if (wname.Length > 0)
{
    var wp = Resources.Load<GameObject>("Weapon/" + wname);
    var wi = (GameObject)UnityEngine.Object.Instantiate(wp);
    var w = wi.GetComponent<Weapon>();
    var f = typeof(Weapon).GetField("RightWeqponSprite");
    var spr = f.GetValue(w) as GameObject;
    var sock = rig.GetBoneSocket("RightWeapon");
    spr.transform.SetParent(sock, false);
    spr.transform.localPosition = Vector3.zero;
    spr.transform.localRotation = Quaternion.identity;
    rig.AdoptWeapon(spr.transform, false);
    UnityEngine.Object.DestroyImmediate(wi);
}
var so = new SerializedObject(rig);
var clips = so.FindProperty("_clips");
int cols = 10, cell = 160;
var tex = new Texture2D(cols * cell, clips.arraySize * cell, TextureFormat.RGB24, false);
var camGo = new GameObject("SheetCam");
var cam = camGo.AddComponent<Camera>();
cam.orthographic = true; cam.orthographicSize = 1.9f;
cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(1f, 0f, 1f, 1f);
camGo.transform.position = new Vector3(5000.3f, 5001.3f, -10f);
var rt = new RenderTexture(cell, cell, 24); rt.filterMode = FilterMode.Point;
cam.targetTexture = rt;
var names = new System.Text.StringBuilder();
for (int r = 0; r < clips.arraySize; r++)
{
    var e = clips.GetArrayElementAtIndex(r);
    var clip = (AnimationClip)e.FindPropertyRelative("clip").objectReferenceValue;
    names.Append(e.FindPropertyRelative("name").stringValue + " ");
    for (int c = 0; c < cols; c++)
    {
        clip.SampleAnimation(go, clip.length * c / (cols - 1) * 0.99f);
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, cell, cell), c * cell, (clips.arraySize - 1 - r) * cell);
        RenderTexture.active = null;
    }
}
tex.Apply();
System.IO.Directory.CreateDirectory("Logs/playtest");
System.IO.File.WriteAllBytes("Logs/playtest/rig_holes.png", tex.EncodeToPNG());
cam.targetTexture = null;
UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
return "rows: " + names;
