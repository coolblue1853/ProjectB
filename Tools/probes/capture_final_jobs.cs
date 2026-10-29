// 직업 4개 최종본 캡처(10-29 밤 사용자 최종 목표: 동작 + 이펙트까지 붙인 직업 4개). 플레이 모드에서 실행.
// 직업마다 임시 도트 무기를 쥐고 동작을 이어 재생, 스미어 색 = 직업 테마(PixelRig.MotionFxRamp), 직업 고유 이펙트(총구 연기·물보라·보라 마법)를 타격 시각에 덧붙임
// → Logs/playtest/final/<번호>/f_###.png, 이어서 python Tools/pixelfx/final_jobs_reel.py
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); pc.enabled = false;
foreach (var mb in pc.GetComponents<MonoBehaviour>()) if (mb.GetType().Name.Contains("Animator") || mb.GetType().Name.Contains("State")) mb.enabled = false;
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
Transform sockR = rig.GetBoneSocket("RightWeapon"), sockL = rig.GetBoneSocket("LeftWeapon");
foreach (var rr in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) if (!rr.transform.IsChildOf(pc.transform) && !rr.name.StartsWith("PXFX") && rr.transform.root.name != "[Vfx]") rr.enabled = false;
foreach (var s in new[] { sockR, sockL }) foreach (var r in s.GetComponentsInChildren<SpriteRenderer>(true)) r.gameObject.SetActive(false);
PixelRig.MotionFxEnabled = true;
// (직업, 오른손, 왼손, 테마, 동작 순서(이름, 길이), 덧붙일 이펙트(동작 번호, 동작 시작 후 초, 이펙트, 앞 px, 위 px, 크기))
var jobs = new (string name, string r, string l, PixelRampId ramp, (string, float)[] seq, (int, float, VfxId, float, float, float)[] fx)[] {
    ("사무라이", "TmpKatana", "", PixelRampId.White,
        new (string, float)[] { ("Iaido", 0.98f), ("DashSlash", 0.94f), ("SwingDown", 0.46f), ("Sheathe", 0.8f), ("Victory", 0.9f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.42f, VfxId.WHit7, 30f, 14f, 1f), (0, 0.33f, VfxId.StepDust, -6f, 0f, 1f), (1, 0.35f, VfxId.WHit7, 30f, 16f, 1f), (2, 0.2f, VfxId.WHit7, 22f, 12f, 0.9f), (3, 0.3f, VfxId.SlashWhite, 4f, 14f, 0.3f) }),
    ("해적 선장", "TmpCutlass", "TmpPistol", PixelRampId.Gold,
        new (string, float)[] { ("GreatSideOne", 0.92f), ("SwordPistol", 0.96f), ("Stab", 0.39f), ("Backstep", 0.92f), ("Victory", 0.9f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.3f, VfxId.WHit7, 24f, 14f, 0.9f), (1, 0.21f, VfxId.SmokeGray, 22f, 18f, 0.5f), (1, 0.21f, VfxId.BurstGold, 22f, 18f, 0.35f), (1, 0.46f, VfxId.SmokeGray, 22f, 18f, 0.5f), (1, 0.46f, VfxId.BurstGold, 22f, 18f, 0.35f), (2, 0.17f, VfxId.WHit7, 26f, 14f, 0.8f), (3, 0.2f, VfxId.DustBrown, 0f, 0f, 0.7f) }),
    ("심해 닻 전사", "TmpAnchor", "", PixelRampId.Blue,
        new (string, float)[] { ("GreatDown", 1.05f), ("Whirl", 0.95f), ("GreatUpper", 1.2f), ("Pull", 0.99f), ("Victory", 0.9f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.5f, VfxId.BurstBlue, 26f, 2f, 0.8f), (0, 0.5f, VfxId.DustStone, 26f, 0f, 0.8f), (1, 0.45f, VfxId.BurstBlue, 28f, 12f, 0.6f), (2, 0.6f, VfxId.PillarBlue, 24f, 0f, 0.6f), (3, 0.35f, VfxId.SmokeGray, 20f, 14f, 0.4f) }),
    ("마도학자", "TmpEyeStaff", "TmpTome", PixelRampId.Purple,
        new (string, float)[] { ("Cast", 1.0f), ("Charge", 1.37f), ("Raise", 0.95f), ("Victory", 0.9f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.35f, VfxId.BurstPurple, 30f, 18f, 0.7f), (1, 0.1f, VfxId.RunePurple, 0f, 0f, 0.8f), (1, 0.75f, VfxId.SmokePurple, 0f, 10f, 0.8f), (2, 0.44f, VfxId.PillarPurple, 30f, 0f, 0.8f), (2, 0.44f, VfxId.RunePurple, 30f, 0f, 0.7f) }),
};
var temps = new System.Collections.Generic.List<GameObject>();
var dummy = new Texture2D(1, 1);
System.Func<string, Transform, bool, GameObject> hold = (nm, sock, left) =>
{
    var go = new GameObject("Tmp_" + nm); go.transform.SetParent(sock, false);
    var sr = go.AddComponent<SpriteRenderer>(); var sp = Sprite.Create(dummy, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)); sp.name = nm; sr.sprite = sp;
    rig.AdoptWeapon(go.transform, left); return go;
};
const float Ppu = 36f;
var move = new System.Collections.Generic.Dictionary<string, float> { { "Backstep", -0.9f }, { "DashSlash", 0.9f }, { "Lunge", 0.9f } };
float t = 1.0f; int gi = 0; var camX = new float[1];
foreach (var j in jobs)
{
    var jj = j; string dir = $"Logs/playtest/final/{gi++:00}"; System.IO.Directory.CreateDirectory(dir);
    DG.Tweening.DOVirtual.DelayedCall(t, () =>
    {
        foreach (var o in temps) if (o != null) UnityEngine.Object.Destroy(o);
        temps.Clear();
        if (jj.r != "") temps.Add(hold(jj.r, sockR, false));
        if (jj.l != "") temps.Add(hold(jj.l, sockL, true));
        PixelRig.MotionFxRamp = jj.ramp;
        camX[0] = pc.transform.position.x;
    }, true);
    float tt = t + 0.25f; var starts = new float[jj.seq.Length];
    for (int k = 0; k < jj.seq.Length; k++)
    {
        var mm = jj.seq[k]; starts[k] = tt;
        DG.Tweening.DOVirtual.DelayedCall(tt, () =>
        {
            rig.Play(mm.Item1);
            if (move.TryGetValue(mm.Item1, out float dx)) DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.TweenSettingsExtensions.SetEase(DG.Tweening.ShortcutExtensions.DOMoveX(pc.transform, pc.transform.position.x + dx, 0.35f), DG.Tweening.Ease.OutQuad), true);
            if (mm.Item1 == "SwingDown")
            {
                DG.Tweening.DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor("SwingDown", rig), () => Vfx.PlayThemed(VfxId.WSmearDown, rig.CelRoot.position, false, Mathf.Abs(rig.CelRoot.lossyScale.y), jj.ramp, false, false, 1f, true), true);
            }
            if (mm.Item1 == "Stab")
            {
                DG.Tweening.DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor("Stab", rig), () => Vfx.PlayThemed(VfxId.WSmearStab, rig.CelRoot.position, false, Mathf.Abs(rig.CelRoot.lossyScale.y), jj.ramp, false, false, 1f, true), true);
            }
        }, true);
        tt += mm.Item2;
    }
    foreach (var f in jj.fx)
    {
        var ff = f;
        DG.Tweening.DOVirtual.DelayedCall(starts[ff.Item1] + ff.Item2, () =>
            Vfx.Play(ff.Item3, pc.transform.position + new Vector3(ff.Item4 / Ppu, ff.Item5 / Ppu, 0f), false, ff.Item6), true);
    }
    DG.Tweening.DOVirtual.DelayedCall(tt, () => rig.Play("Idle"), true); tt += 0.4f;
    int frames = Mathf.CeilToInt((tt - t) * 30f);
    for (int k = 0; k < frames; k++)
    {
        int kk = k;
        DG.Tweening.DOVirtual.DelayedCall(t + 0.01f + kk / 30f, () =>
        {
            var go = new GameObject("VidCam"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 1.6f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.13f, 0.15f, 0.19f, 1f);
            Vector3 p = pc.transform.position; go.transform.position = new Vector3((camX[0] + p.x) * 0.5f + 0.35f, p.y + 0.8f, p.z - 50f); // 반쯤 따라감 — 백스텝·돌진 이동은 보이되 화면 밖으로 안 나가게
            var rt = new RenderTexture(400, 260, 24); cam.targetTexture = rt; cam.Render();
            var tex = new Texture2D(400, 260, TextureFormat.RGB24, false); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 400, 260), 0, 0); RenderTexture.active = null;
            System.IO.File.WriteAllBytes($"{dir}/f_{kk:000}.png", tex.EncodeToPNG());
            cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
        }, true);
    }
    // 다음 직업 전에 제자리로(백스텝·돌진 이동 되돌림)
    float back = tt;
    DG.Tweening.DOVirtual.DelayedCall(back, () => { pc.transform.position = new Vector3(camX[0], pc.transform.position.y, pc.transform.position.z); }, true);
    t = tt + 0.3f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => { PixelRig.MotionFxRamp = PixelRampId.Steel; }, true);
return "scheduled " + t + " s, " + jobs.Length + " jobs";
