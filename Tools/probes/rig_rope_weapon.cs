// 로프 중 무기 숨김 확인(PixelRigDesign 10-25 (17))
// 편집 모드: 칼 쥔 리그를 로프 → 대기로 바꾸며 LateUpdate 를 직접 돌려 무기 보임 여부 확인
var prefab = Resources.Load<GameObject>("PixelRig/Adventurer");
var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
var rig = go.GetComponent<PixelRig>();
var awake = typeof(PixelRig).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var late = typeof(PixelRig).GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
awake.Invoke(rig, null);
var wp = (GameObject)UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Weapon/RustedSword"));
var spr = typeof(Weapon).GetField("RightWeqponSprite").GetValue(wp.GetComponent<Weapon>()) as GameObject;
spr.transform.SetParent(rig.GetBoneSocket("RightWeapon"), false);
rig.AdoptWeapon(spr.transform, false);
var sr = spr.GetComponentInChildren<SpriteRenderer>();
rig.Play("RopeStay"); late.Invoke(rig, null); bool onRope = sr.enabled;
rig.Play("Idle"); late.Invoke(rig, null); bool idle = sr.enabled;
UnityEngine.Object.DestroyImmediate(wp); UnityEngine.Object.DestroyImmediate(go);
return "weapon visible on rope=" + onRope + " idle=" + idle;
