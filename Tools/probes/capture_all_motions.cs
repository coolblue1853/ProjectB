// 전체 마스터 모션 합본 캡처(10-29 사용자: 모션에 맞는 임시 무기 + 이펙트, 합본 영상). 플레이 모드에서 실행.
// 동작마다 원래 무기를 숨기고 임시 도트 무기(Resources/PixelRig/Weapons)를 손에 쥐여 셀 이름으로 재생 → 1/30초 캡처 → Logs/playtest/all/<번호>_<동작>/f_###.png
// 새 동작 이펙트는 리그(MotionFxTable)가 띄우고, 기본 3타 v7 스미어는 Weapon 대신 여기서 같은 방식으로 띄운다. 이어서 python Tools/pixelfx/motion_reel.py
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); pc.enabled = false;
foreach (var mb in pc.GetComponents<MonoBehaviour>()) if (mb.GetType().Name.Contains("Animator") || mb.GetType().Name.Contains("State")) mb.enabled = false;
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
Transform sockR = rig.GetBoneSocket("RightWeapon"), sockL = rig.GetBoneSocket("LeftWeapon");
// 배경(맵·풀·하늘)은 끄고 캐릭터·이펙트만 — 풀 덩어리가 몸을 가렸다
foreach (var rr in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) if (!rr.transform.IsChildOf(pc.transform) && !rr.name.StartsWith("PXFX") && rr.transform.root.name != "[Vfx]") rr.enabled = false;
foreach (var s in new[] { sockR, sockL }) foreach (var r in s.GetComponentsInChildren<SpriteRenderer>(true)) r.gameObject.SetActive(false);
// 동작, 오른손 무기, 왼손 무기("" = 없음)
var list = new (string anim, string r, string l)[] {
    ("Idle","ShortSword",""), ("Walk","ShortSword",""), ("Run","ShortSword",""), ("Dash","ShortSword",""), ("JumpCrouch","ShortSword",""), ("Land","ShortSword",""),
    ("RopeUp","",""), ("Hurt","ShortSword",""), ("Death","ShortSword",""),
    ("Stab","ShortSword",""), ("SwingDown","ShortSword",""), ("SwingUp","ShortSword",""),
    ("GreatDown","SacreGreatSword",""), ("GreatSide","SacreGreatSword",""), ("GreatSideOne","PoisonSword",""),
    ("DaggerFlurry","DoubleDagger","DoubleDagger"), ("DaggerCross","DoubleDagger","DoubleDagger"), ("AirSlash","ShortSword",""),
    ("Lunge","Wood Spear",""), ("Spin","SacreGreatSword",""), ("Cast","LeafStaff",""), ("Throw","TmpKnife",""),
    ("Bow","BanditShortbow",""), ("Guard","ShortSword","WoodShield"), ("Charge","ScareStaff",""), ("LeapSlam","TmpHammer",""),
    ("Backstep","DoubleDagger","DoubleDagger"), ("Punch","Rleatherknuckles","Lleatherknuckles"), ("Iaido","TmpKatana",""),
    ("ShoulderAim","TmpCannon",""), ("Raise","LeafStaff",""), ("Place","",""), ("Kick","Rleatherknuckles","Lleatherknuckles"),
    ("Grab","",""), ("Ride","Wood Spear",""), ("Knockdown","ShortSword",""), ("ShieldBash","ShortSword","WoodShield"),
    ("Slam","",""), ("Pull","TmpHook",""), ("LowSweep","Wood Spear",""), ("GreatUpper","SacreGreatSword",""), ("DashSlash","ShortSword",""), ("WhipSnap","TmpHook",""), ("Roundhouse","Rleatherknuckles","Lleatherknuckles"), ("Sheathe","TmpKatana",""), ("DownThrust","ShortSword",""),
};
var basic = new System.Collections.Generic.Dictionary<string, VfxId> { { "Stab", VfxId.WSmearStab }, { "SwingDown", VfxId.WSmearDown }, { "SwingUp", VfxId.WSmearUp } };
var temps = new System.Collections.Generic.List<GameObject>();
var dummy = new Texture2D(1, 1);
System.Func<string, Transform, bool, GameObject> hold = (nm, sock, left) =>
{
    var go = new GameObject("Tmp_" + nm); go.transform.SetParent(sock, false);
    var sr = go.AddComponent<SpriteRenderer>(); var sp = Sprite.Create(dummy, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)); sp.name = nm; sr.sprite = sp;
    rig.AdoptWeapon(go.transform, left); return go;
};
System.IO.Directory.CreateDirectory("Logs/playtest/all");
float t = 0.3f; int idx = 0;
foreach (var m in list)
{
    var mm = m; int id = idx++; string dir = $"Logs/playtest/all/{id:00}_{mm.anim}";
    System.IO.Directory.CreateDirectory(dir);
    DG.Tweening.DOVirtual.DelayedCall(t, () =>
    {
        foreach (var g in temps) if (g != null) UnityEngine.Object.Destroy(g);
        temps.Clear();
        if (mm.r != "") temps.Add(hold(mm.r, sockR, false));
        if (mm.l != "") temps.Add(hold(mm.l, sockL, true));
        rig.Play(mm.anim);
        if (basic.TryGetValue(mm.anim, out VfxId sm))
        {
            DG.Tweening.DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor(mm.anim, rig), () =>
                Vfx.PlayThemed(sm, rig.CelRoot.position, false, Mathf.Abs(rig.CelRoot.lossyScale.y), PixelRampId.Steel, false, false, 1f, true), true);
        }
    }, true);
    for (int k = 0; k < 42; k++)
    {
        int kk = k;
        DG.Tweening.DOVirtual.DelayedCall(t + 0.01f + kk / 30f, () =>
        {
            var go = new GameObject("VidCam"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 1.8f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.15f, 0.17f, 0.21f, 1f);
            Vector3 p = pc.transform.position; go.transform.position = new Vector3(p.x + 0.5f, p.y + 0.9f, p.z - 50f);
            var rt = new RenderTexture(360, 240, 24); cam.targetTexture = rt; cam.Render();
            var tex = new Texture2D(360, 240, TextureFormat.RGB24, false); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 360, 240), 0, 0); RenderTexture.active = null;
            System.IO.File.WriteAllBytes($"{dir}/f_{kk:000}.png", tex.EncodeToPNG());
            cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
        }, true);
    }
    t += 1.6f;
}
return "scheduled " + t + " s, " + list.Length + " motions";
