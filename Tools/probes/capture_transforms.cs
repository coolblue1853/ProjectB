// 변신형 시제품(메카 슈트·골렘, TransformSpec.md ① — 마스터 모션 키 그대로 외형만 교체, 10-30) 캡처. 원본: 직업 4개 최종본(사용자 지정 외형: 블러드 헌터·사령술사·기계공학자·암살자, 10-30) 캡처 — capture_final_jobs.cs 를 직업 외형 셀(J<직업>_*)로
// 직업 4개 최종본 캡처(10-29 밤 사용자 최종 목표: 동작 + 이펙트까지 붙인 직업 4개). 플레이 모드에서 실행.
// 직업마다 임시 도트 무기를 쥐고 동작을 이어 재생, 스미어 색 = 직업 테마(PixelRig.MotionFxRamp), 직업 고유 이펙트(총구 연기·물보라·보라 마법)를 타격 시각에 덧붙임
// → Logs/playtest/transforms/<번호>/f_###.png, 이어서 python Tools/pixelfx/final_jobs_reel.py
var pc = PlayerController.instance; var rig = pc.GetComponentInChildren<PixelRig>(); pc.enabled = false;
foreach (var mb in pc.GetComponents<MonoBehaviour>()) if (mb.GetType().Name.Contains("Animator") || mb.GetType().Name.Contains("State")) mb.enabled = false;
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
Transform sockR = rig.GetBoneSocket("RightWeapon"), sockL = rig.GetBoneSocket("LeftWeapon");
foreach (var rr in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) if (!rr.transform.IsChildOf(pc.transform) && !rr.name.StartsWith("PXFX") && rr.transform.root.name != "[Vfx]") rr.enabled = false;
foreach (var s in new[] { sockR, sockL }) foreach (var r in s.GetComponentsInChildren<SpriteRenderer>(true)) r.gameObject.SetActive(false);
PixelRig.MotionFxEnabled = true;
// (직업, 오른손, 왼손, 테마, 동작 순서(이름, 길이), 덧붙일 이펙트(동작 번호, 동작 시작 후 초, 이펙트, 앞 px, 위 px, 크기))
var jobs = new (string name, string r, string l, PixelRampId ramp, (string, float)[] seq, (int, float, VfxId, float, float, float)[] fx)[] {
    ("변신: 메카 슈트", "", "", PixelRampId.Blue,
        new (string, float)[] { ("JMC_Punch", 0.93f), ("JMC_GreatDown", 1.05f), ("JMC_ShoulderAim", 1.02f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.55f, VfxId.WHit7, 26f, 16f, 0.9f), (0, 0.56f, VfxId.BurstBlue, 26f, 16f, 0.3f), (1, 0.5f, VfxId.GroundShock, 24f, 0f, 0.9f), (1, 0.5f, VfxId.DustStone, 24f, 0f, 0.8f), (2, 0.45f, VfxId.BurstGold, 34f, 24f, 0.4f), (2, 0.48f, VfxId.SmokeGray, 30f, 24f, 0.4f), (2, 0.5f, VfxId.BurstRed, 90f, 4f, 0.7f) }),
    ("변신: 골렘", "", "", PixelRampId.Gold,
        new (string, float)[] { ("JGL_Punch", 0.93f), ("JGL_GreatDown", 1.05f), ("JGL_ShieldBash", 0.99f) },
        new (int, float, VfxId, float, float, float)[] { (0, 0.55f, VfxId.DustStone, 26f, 14f, 0.7f), (1, 0.5f, VfxId.GroundShock, 24f, 0f, 1.0f), (1, 0.5f, VfxId.DustStone, 24f, 0f, 1.0f), (2, 0.4f, VfxId.DustBrown, 20f, 0f, 0.9f), (2, 0.42f, VfxId.WHit7, 26f, 14f, 0.8f) }),
};
var idles = new[] { "JMC_Idle", "JGL_Idle" };
// 히트스톱·흔들림(GPT 2차 리뷰 "무기군별 히트스톱·무게감으로 전투 맛을 달리") — (직업 번호, 동작 번호, 동작 시작 후 초, 멈춤 초, 흔들림 px)
var stops = new (int job, int m, float at, float dur, float shake)[] {
    (0, 0, 0.55f, 0.06f, 1.5f), (0, 1, 0.5f, 0.12f, 3f), (0, 2, 0.45f, 0.05f, 2f), (1, 0, 0.55f, 0.08f, 2f), (1, 1, 0.5f, 0.14f, 3.5f), (1, 2, 0.4f, 0.1f, 2.5f),
};
// 무기군 타이밍 프로파일(준비 배율, 회복 배율) — 닻은 무겁게 길게, 사무라이는 장전 길고 회복 짧게(발도 문법)
var timing = new (float w, float r)[] { (1.2f, 1.1f), (1.4f, 1.25f) }; // 변신체는 무겁게
var shakeUntil = new float[1]; var shakeAmp = new float[1];
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
    int gi2 = gi; var jj = j; string dir = $"Logs/playtest/transforms/{gi++:00}"; System.IO.Directory.CreateDirectory(dir);
    DG.Tweening.DOVirtual.DelayedCall(t, () =>
    {
        foreach (var o in temps) if (o != null) UnityEngine.Object.Destroy(o);
        temps.Clear();
        if (jj.r != "") temps.Add(hold(jj.r, sockR, false));
        if (jj.l != "") temps.Add(hold(jj.l, sockL, true));
        PixelRig.MotionFxRamp = jj.ramp;
        rig.SetTimingProfile(timing[gi2].w, timing[gi2].r);
        rig.Play(idles[gi2]);
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
            if (mm.Item1 == "Pull")
            {
                // 사슬 선 임시(T1 전): 손 → 앞쪽 걸린 점, 당기는 동안 짧아짐(GPT 리뷰 "사슬이 안 보여 연결감이 없다")
                var lgo = new GameObject("PXFX_Chain"); var lr = lgo.AddComponent<LineRenderer>(); lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lr.endColor = new Color(0.32f, 0.36f, 0.4f, 1f); lr.startWidth = lr.endWidth = 2f / 36f; lr.positionCount = 2; lr.sortingOrder = 49;
                float x0 = pc.transform.position.x + 2.2f;
                DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.TweenSettingsExtensions.OnComplete(DG.Tweening.DOVirtual.Float(0f, 1f, 0.9f, v =>
                {
                    Vector3 h = sockR.position; float tip = Mathf.Lerp(x0, h.x + 0.5f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((v - 0.35f) / 0.4f)));
                    lr.SetPosition(0, h); lr.SetPosition(1, new Vector3(tip, h.y - 0.05f, h.z));
                }), () => UnityEngine.Object.Destroy(lgo)), true);
            }
            if (mm.Item1 == "SwingDown")
            {
                DG.Tweening.DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor("SwingDown", rig), () => Vfx.PlayThemed(VfxId.WSmearDown, rig.CelRoot.position, false, Mathf.Abs(rig.CelRoot.lossyScale.y), jj.ramp, false, false, 1f, true), true);
            }
            if (mm.Item1 == "Stab")
            {
                DG.Tweening.DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor("Stab", rig), () => Vfx.PlayThemed(VfxId.WSmearStab, rig.CelRoot.position, false, Mathf.Abs(rig.CelRoot.lossyScale.y), jj.ramp, false, false, 1f, true), true);
            }
        }, true);
        tt += mm.Item2 * (timing[gi2].w + timing[gi2].r) * 0.5f;
        foreach (var st in stops) if (st.job == gi - 1 && st.m == k) tt += st.dur; // 멈춘 만큼 다음 동작을 늦춘다
    }
    int ji = gi - 1;
    foreach (var st in stops)
    {
        if (st.job != ji) continue;
        var ss = st;
        DG.Tweening.DOVirtual.DelayedCall(starts[ss.m] + ss.at * timing[gi2].w, () =>
        {
            // 캐릭터만 멈춘다(Time.timeScale 은 DOTween 예약까지 늦춰 캡처가 꼬였다)
            rig.SetAnimationSpeed(0f); shakeAmp[0] = ss.shake; shakeUntil[0] = Time.realtimeSinceStartup + ss.dur + 0.08f;
            DG.Tweening.DOVirtual.DelayedCall(ss.dur, () => rig.SetAnimationSpeed(1f), true);
        }, true);
    }
    foreach (var f in jj.fx)
    {
        var ff = f;
        DG.Tweening.DOVirtual.DelayedCall(starts[ff.Item1] + ff.Item2 * timing[gi2].w, () =>
            Vfx.Play(ff.Item3, (ff.Item4 >= 900f ? sockL.position + new Vector3((ff.Item4 - 1000f) / Ppu, ff.Item5 / Ppu, 0f) : pc.transform.position + new Vector3(ff.Item4 / Ppu, ff.Item5 / Ppu, 0f)), false, ff.Item6), true);
    }
    DG.Tweening.DOVirtual.DelayedCall(tt, () => rig.Play(idles[gi2]), true); tt += 0.4f;
    int frames = Mathf.CeilToInt((tt - t) * 30f);
    for (int k = 0; k < frames; k++)
    {
        int kk = k;
        DG.Tweening.DOVirtual.DelayedCall(t + 0.12f + kk / 30f, () =>
        {
            var go = new GameObject("VidCam"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 1.6f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.13f, 0.15f, 0.19f, 1f);
            Vector3 p = pc.transform.position; go.transform.position = new Vector3((camX[0] + p.x) * 0.5f + 0.35f, p.y + 0.8f, p.z - 50f);
            if (Time.realtimeSinceStartup < shakeUntil[0]) go.transform.position += new Vector3(((kk % 2) * 2 - 1) * shakeAmp[0] / Ppu, ((kk / 2 % 2) * 2 - 1) * shakeAmp[0] * 0.5f / Ppu, 0f); // 반쯤 따라감 — 백스텝·돌진 이동은 보이되 화면 밖으로 안 나가게
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
DG.Tweening.DOVirtual.DelayedCall(t, () => { PixelRig.MotionFxRamp = PixelRampId.Steel; rig.SetTimingProfile(1f, 1f); }, true);
return "scheduled " + t + " s, " + jobs.Length + " jobs";
