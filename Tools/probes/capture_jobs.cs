// 직업 20개 이어 붙이기 캡처(10-29 사용자 "직업마다 무기만 바꿔 3~4동작"), 원본 = 무기별 이어 붙이기 캡처(이펙트 없음, 10-29 사용자 "무기만 연결한 상태로"): 무기 종류마다 그 무기의 동작 2~4개를 연달아 재생 → Logs/playtest/jobs/<번호>/f_###.png
// 무기는 임시 도트 무기(gen_tmp_weapons.py). 이어서 python 으로 GIF(Logs/combo_names.json 이름)
// PixelRig.MotionFxEnabled 를 끄고 찍은 뒤 다시 켠다
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); pc.enabled = false;
foreach (var mb in pc.GetComponents<MonoBehaviour>()) if (mb.GetType().Name.Contains("Animator") || mb.GetType().Name.Contains("State")) mb.enabled = false;
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
Transform sockR = rig.GetBoneSocket("RightWeapon"), sockL = rig.GetBoneSocket("LeftWeapon");
// 배경(맵·풀·하늘)은 끄고 캐릭터·이펙트만 — 풀 덩어리가 몸을 가렸다
foreach (var rr in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) if (!rr.transform.IsChildOf(pc.transform) && !rr.name.StartsWith("PXFX") && rr.transform.root.name != "[Vfx]") rr.enabled = false;
foreach (var s in new[] { sockR, sockL }) foreach (var r in s.GetComponentsInChildren<SpriteRenderer>(true)) r.gameObject.SetActive(false);
PixelRig.MotionFxEnabled = false;
var groups = new (string name, string r, string l, (string, float)[] seq)[] {
    ("해적선장", "TmpSword", "TmpPistol", new (string, float)[] { ("SwingDown",0.46f),("SwordPistol",0.96f),("Stab",0.39f),("GreatSideOne",0.92f) }),
    ("사무라이", "TmpKatana", "", new (string, float)[] { ("Iaido",1.12f),("SwingDown",0.46f),("Sheathe",1.34f) }),
    ("기사(검+방패)", "TmpSword", "TmpShield", new (string, float)[] { ("Guard",1.06f),("SwingDown",0.46f),("ShieldBash",0.99f) }),
    ("광전사(대검)", "TmpGreatSword", "", new (string, float)[] { ("GreatDown",1.04f),("GreatSide",1.03f),("GreatUpper",0.99f) }),
    ("창기사", "TmpSpear", "", new (string, float)[] { ("Stab",0.39f),("Lunge",0.91f),("LowSweep",0.93f) }),
    ("암살자(쌍단검)", "TmpDagger", "TmpDagger", new (string, float)[] { ("DaggerCross",0.93f),("DaggerFlurry",0.62f),("Backstep",0.92f),("Throw",0.99f) }),
    ("궁수", "TmpBow", "", new (string, float)[] { ("Bow",1.01f),("Backstep",0.92f),("Bow",1.01f) }),
    ("총잡이(쌍권총)", "TmpPistol", "TmpPistol", new (string, float)[] { ("DualPistol",0.87f),("Backstep",0.92f),("DualPistol",0.87f) }),
    ("저격수(소총)", "TmpRifle", "", new (string, float)[] { ("Rifle",0.98f),("Backstep",0.92f),("Rifle",0.98f) }),
    ("포병(대포)", "TmpCannon", "", new (string, float)[] { ("ShoulderAim",1.02f),("Backstep",0.92f) }),
    ("마법사", "TmpStaff", "", new (string, float)[] { ("Cast",1.0f),("Charge",1.37f),("Raise",1.24f) }),
    ("격투가", "TmpKnuckle", "TmpKnuckle", new (string, float)[] { ("Punch",0.93f),("Kick",0.81f),("Grab",0.98f) }),
    ("망치 전사", "TmpHammer", "", new (string, float)[] { ("GreatDown",1.04f),("GreatUpper",0.99f),("GreatSide",1.03f) }),
    ("사신(낫)", "TmpHook", "", new (string, float)[] { ("GreatSide",1.03f),("LowSweep",0.93f),("Pull",0.98f) }),
    ("채찍술사", "TmpWhip", "", new (string, float)[] { ("WhipSnap",0.87f),("SwingDown",0.46f),("Pull",0.98f) }),
    ("도적(투척)", "TmpKnife", "", new (string, float)[] { ("Throw",0.99f),("Stab",0.39f),("Backstep",0.92f) }),
    ("닌자", "TmpKatana", "", new (string, float)[] { ("DashSlash",0.94f),("Throw",0.99f),("Backstep",0.92f) }),
    ("성기사(망치+방패)", "TmpHammer", "TmpShield", new (string, float)[] { ("SwingDown",0.46f),("Guard",1.06f),("Raise",1.24f) }),
    ("폭탄병", "", "", new (string, float)[] { ("Throw",0.99f),("Place",0.98f),("Throw",0.99f) }),
    ("기병(창)", "TmpSpear", "", new (string, float)[] { ("Ride",0.88f),("Lunge",0.91f),("Stab",0.39f) }),
};
var temps = new System.Collections.Generic.List<GameObject>();
var dummy = new Texture2D(1, 1);
System.Func<string, Transform, bool, GameObject> hold = (nm, sock, left) =>
{
    var go = new GameObject("Tmp_" + nm); go.transform.SetParent(sock, false);
    var sr = go.AddComponent<SpriteRenderer>(); var sp = Sprite.Create(dummy, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)); sp.name = nm; sr.sprite = sp;
    rig.AdoptWeapon(go.transform, left); return go;
};
System.IO.Directory.CreateDirectory("Logs/playtest/jobs");
float t = 0.3f; int gi = 0;
foreach (var g in groups)
{
    var gg = g; string dir = $"Logs/playtest/jobs/{gi++:00}"; System.IO.Directory.CreateDirectory(dir);
    float t0 = t;
    DG.Tweening.DOVirtual.DelayedCall(t, () =>
    {
        foreach (var o in temps) if (o != null) UnityEngine.Object.Destroy(o);
        temps.Clear();
        if (gg.r != "") temps.Add(hold(gg.r, sockR, false));
        if (gg.l != "") temps.Add(hold(gg.l, sockL, true));
    }, true);
    float tt = t + 0.02f;
    foreach (var m in gg.seq) { var mm = m; DG.Tweening.DOVirtual.DelayedCall(tt, () => rig.Play(mm.Item1), true); tt += mm.Item2; }
    DG.Tweening.DOVirtual.DelayedCall(tt, () => rig.Play("Idle"), true); tt += 0.3f;
    int frames = Mathf.CeilToInt((tt - t) * 30f);
    for (int k = 0; k < frames; k++)
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
    t = tt + 0.2f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => PixelRig.MotionFxEnabled = true, true);
return "scheduled " + t;
