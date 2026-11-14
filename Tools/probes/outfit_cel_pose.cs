// 11-15 의상 셀 한 자세 고정(플레이 모드, 이어서 manage_camera screenshot 으로 찍는다 — 카메라 RT 렌더는 픽셀 퍼펙트 카메라 때문에 화면과 어긋남).
// __ITEM__ = 의상 아이템 이름("" = 기본 코트), __SHOT__ = 셀:프레임. CelEquipSync.Apply 실제 경로로 묶음 선택. __END__=1 이면 원래대로
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); var sync = rig.GetComponent<CelEquipSync>();
var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var T = typeof(PixelRig);
var apply = typeof(CelEquipSync).GetMethod("Apply", bf);
if ("__END__" == "1") { apply.Invoke(sync, new object[] { "|" }); Time.timeScale = 1f; rig.Play("Idle"); pc.enabled = true; sync.enabled = true; return "end"; }
pc.enabled = false; sync.enabled = false; Time.timeScale = 0f;
apply.Invoke(sync, new object[] { "__ITEM__" + "|" });
var byName = (System.Collections.Generic.Dictionary<string, CelClip>)T.GetField("_celByName", bf).GetValue(rig);
var celR = (SpriteRenderer)T.GetField("_cel", bf).GetValue(rig);
string n = "__SHOT__".Split(':')[0]; int f = int.Parse("__SHOT__".Split(':')[1]); CelClip cc = byName[n];
T.GetField("_endCel", bf).SetValue(rig, null); rig.Play(n); T.GetField("_endCel", bf).SetValue(rig, null);
T.GetField("_time", bf).SetValue(rig, cc.StartTimeOf(f) + 0.005f);
if (n == "JumpArc") { T.GetField("_arcAt", bf).SetValue(rig, (f + 0.5f) / cc.sprites.Length); }
T.GetMethod("LateUpdate", bf).Invoke(rig, null);
string path = celR.sprite != null ? UnityEditor.AssetDatabase.GetAssetPath(celR.sprite) : "null";
return (rig.OutfitId ?? "coat") + " " + n + (cc.outfit ? "[o]" : "[c]") + "=" + path;
