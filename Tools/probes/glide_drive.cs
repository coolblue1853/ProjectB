// 활공 검증 드라이버(플레이 중). 모드: short(탭 2번 = 기존 2단 점프) / hold(공중에서 홀드, 소진까지) / updraft(기류 안 홀드) / course(코스 평가·보상).
// 결과는 "[GlideRes] ..." 오브젝트 이름(glide_read.cs 로 읽기). 키: C = 점프, RightArrow = 오른쪽.
var mode = "__MODE__";
var kb = UnityEngine.InputSystem.Keyboard.current;
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var rb = pc.GetComponent<Rigidbody2D>();
var m = GlideMotor.Instance;
var res = new UnityEngine.GameObject("[GlideRes] " + mode + " running");
System.Action<UnityEngine.InputSystem.Key[]> keys = ks => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(ks));
System.Func<float, float> groundAt = x => { var f = new UnityEngine.ContactFilter2D(); f.SetLayerMask(UnityEngine.LayerMask.GetMask("Ground")); f.useTriggers = false; var h = new UnityEngine.RaycastHit2D[4]; int n = UnityEngine.Physics2D.Raycast(new UnityEngine.Vector2(x, 9f), UnityEngine.Vector2.down, f, h, 50f); return n > 0 ? h[0].point.y : -999f; };
System.Action<float, float> tp = (x, dy) => { pc.transform.position = new UnityEngine.Vector3(x, groundAt(x) + dy, pc.transform.position.z); rb.linearVelocity = UnityEngine.Vector2.zero; };
var seq = DG.Tweening.DOTween.Sequence();
int steps = 40;
float dtStep = 0.05f;
bool anyGlide = false; float minVy = 99f, maxVy = -99f, firstGlideT = -1f, zeroT = -1f, vyGlideSum = 0f; int vyGlideN = 0; float y0 = 0f; float maxY = -999f; int jr0 = 0; float vyAfterSecond = -99f;
float startY = 0f;
if (mode == "short") { tp(-433f, 0.5f); steps = 60; }
else if (mode == "hold") { tp(-340f, 45f); steps = 360; keys(new UnityEngine.InputSystem.Key[] { UnityEngine.InputSystem.Key.C }); }
else if (mode == "updraft") { tp(-414f, 0.5f); steps = 140; }
else if (mode == "course") { tp(-433f, 0.5f); steps = 100; }
y0 = pc.transform.position.y; jr0 = pc.jumpsRemaining;
var cs = UnityEngine.Object.FindFirstObjectByType<GlideCourse>();
int invBefore = InventoryManager.instance != null ? InventoryManager.instance.CountItem(EnhanceRules.StoneName) : -1;
string extra = "";
for (int i = 0; i <= steps; i++)
{
    int k = i;
    DG.Tweening.TweenSettingsExtensions.AppendInterval(seq, dtStep);
    DG.Tweening.TweenSettingsExtensions.AppendCallback(seq, () =>
    {
        float vy = rb.linearVelocity.y;
        float t = k * dtStep;
        if (mode == "short")
        {
            if (k == 0) keys(new UnityEngine.InputSystem.Key[] { UnityEngine.InputSystem.Key.C });
            if (k == 2) keys(new UnityEngine.InputSystem.Key[0]);
            if (k == 14) { keys(new UnityEngine.InputSystem.Key[] { UnityEngine.InputSystem.Key.C }); extra += " jr@2nd=" + pc.jumpsRemaining; }
            if (k == 16) keys(new UnityEngine.InputSystem.Key[0]);
            if (k >= 15 && k <= 20 && vy > vyAfterSecond) vyAfterSecond = vy;
        }
        else if (mode == "updraft")
        {
            if (k == 0) keys(new UnityEngine.InputSystem.Key[] { UnityEngine.InputSystem.Key.C });
            if (k % 20 == 0) extra += " y" + k + "=" + pc.transform.position.y.ToString("F1") + "/vy" + vy.ToString("F1") + "/g" + (m.Gliding ? 1 : 0) + "/rem" + m.Remaining.ToString("F1");
        }
        else if (mode == "course")
        {
            // 시작 표지에서 1.5초 → 도착 표지로 순간이동(3초 이상 경과 필요), 두 번째 판은 쿨다운 뒤 반복 규칙 확인
            if (k == 10) extra += " running@10=" + (cs != null && cs.Running);
            if (k == 70) tp(-276f, 1f);
        }
        if (m.Gliding)
        {
            anyGlide = true;
            if (firstGlideT < 0f) firstGlideT = t;
            if (vy < minVy) minVy = vy;
            if (vy > maxVy) maxVy = vy;
            if (t - firstGlideT > 1f) { vyGlideSum += vy; vyGlideN++; }
        }
        else if (firstGlideT >= 0f && zeroT < 0f) { zeroT = t; extra += " glideEnd@" + t.ToString("F2") + " vy=" + vy.ToString("F1") + " rem=" + m.Remaining.ToString("F2"); }
        if (pc.transform.position.y > maxY) maxY = pc.transform.position.y;
        if (k == steps)
        {
            keys(new UnityEngine.InputSystem.Key[0]);
            int invAfter = InventoryManager.instance != null ? InventoryManager.instance.CountItem(EnhanceRules.StoneName) : -1;
            res.name = "[GlideRes] " + mode + " anyGlide=" + anyGlide + " firstGlideT=" + firstGlideT.ToString("F2") + " vyMin=" + minVy.ToString("F2") + " vyMax=" + maxVy.ToString("F2") + " vyGlideAvg=" + (vyGlideN > 0 ? (vyGlideSum / vyGlideN).ToString("F2") : "n/a") + " maxY-y0=" + (maxY - y0).ToString("F1") + " jr0=" + jr0 + " jrEnd=" + pc.jumpsRemaining + " vyAfter2nd=" + vyAfterSecond.ToString("F1") + " maxDur=" + m.MaxDuration.ToString("F2") + " inv " + invBefore + "->" + invAfter + (cs != null && cs.HasResult ? " grade=" + cs.LastGrade + " reward=" + cs.LastReward + " first=" + cs.LastFirstOfDay + " falls=" + cs.Falls + " t=" + cs.Elapsed.ToString("F1") : "") + extra;
        }
    });
}
return "driver started " + mode + " ground=" + groundAt(pc.transform.position.x).ToString("F1");
