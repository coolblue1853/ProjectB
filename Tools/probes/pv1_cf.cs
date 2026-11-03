// PV1 조작감 CF-A~D 검증 (플레이 모드, Town 평지). 결과: Logs/pv1/cf.log . 가상 키 입력. 끝나면 "DONE" 한 줄.
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/cf.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
var iset = UnityEngine.InputSystem.InputSystem.settings;
iset.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
iset.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var kb = UnityEngine.InputSystem.Keyboard.current; UnityEngine.InputSystem.InputSystem.EnableDevice(kb); Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
var pc = PlayerController.instance; var rb = pc.GetComponent<Rigidbody2D>();
Time.timeScale = 1f;
MapInfo.GoToMap("Town", pc.gameObject);
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
var fl = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
System.Func<object, string, float> getF = (o, n) => { var f = o.GetType().GetField(n, fl); return f != null ? System.Convert.ToSingle(f.GetValue(o)) : -1f; };
string curKeys = "";
System.Action<UnityEngine.InputSystem.Key[]> Hold = (ks) => { string s = string.Join(",", System.Array.ConvertAll(ks, k => k.ToString())); if (s == curKeys) return; curKeys = s; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(ks)); };
var R = UnityEngine.InputSystem.Key.RightArrow; var Lf = UnityEngine.InputSystem.Key.LeftArrow; var Cj = UnityEngine.InputSystem.Key.C; var Sh = UnityEngine.InputSystem.Key.Z;
var none = new UnityEngine.InputSystem.Key[0];
var rec = new System.Collections.Generic.List<float[]>();
var steps = new System.Collections.Generic.List<System.Func<float, bool>>();
Vector3 startPos = Vector3.zero; int frames = 0; float fpsT0 = Time.realtimeSinceStartup;
// rec: 0 t,1 x,2 y,3 vx,4 vy,5 scaleX,6 jc,7 mc,8 turn,9 mul,10 atkWait,11 combo,12 ground,13 lossyX,14 hitDelay,15 total
System.Func<float, float[]> mk = (t) => { var w = Weapon.Equipped; return new float[] { t, pc.transform.position.x, pc.transform.position.y, rb.linearVelocity.x, rb.linearVelocity.y, pc.transform.localScale.x, w.CanJumpCancelNow ? 1 : 0, w.CanMoveCancelNow ? 1 : 0, w.CanTurnNow ? 1 : 0, w.AttackMoveSpeedMul, w.isAttackWait ? 1 : 0, w.nowConboCount, pc.isGround ? 1 : 0, w.transform.lossyScale.x, getF(w, "_attackHitDelay"), getF(w, "_attackWaitTotal") }; };
System.Func<System.Action, System.Func<float, bool>, System.Action, System.Func<float, bool>> Step = (init, per, end) =>
{
    bool started = false;
    return t => { if (!started) { started = true; init(); } bool d = per(t); if (d) { end(); } return d; };
};
System.Func<float, System.Func<float, bool>> Wait = (sec) => Step(() => { Hold(none); }, (t) => t >= sec, () => { });
System.Func<int, System.Func<float, bool>> Face = (dir) => Step(
    () => { pc.transform.position = startPos; rb.linearVelocity = Vector2.zero; },
    (t) => { if (t < 0.4f) { Hold(new[] { dir > 0 ? R : Lf }); return false; } Hold(none); return t >= 1.0f && pc.isGround && Weapon.Equipped.CanMeleeNow && Weapon.Equipped.nowConboCount == 0; },
    () => { });
System.Func<string, string> Summ = (name) =>
{
    if (rec.Count < 2) return name + ": 기록 없음";
    var f = rec[0]; var l = rec[rec.Count - 1];
    float maxVx = 0, hit = 0, tot = 0; float tJc = -1, tMc = -1, tTn = -1, tFlip = -1, tFlipL = -1, tAtkEnd = -1;
    foreach (var r in rec)
    {
        maxVx = Mathf.Max(maxVx, Mathf.Abs(r[3])); hit = Mathf.Max(hit, r[14]); tot = Mathf.Max(tot, r[15]);
        if (tJc < 0 && r[6] > 0) tJc = r[0];
        if (tMc < 0 && r[7] > 0) tMc = r[0];
        if (tTn < 0 && r[8] > 0) tTn = r[0];
        if (tFlip < 0 && Mathf.Sign(r[5]) != Mathf.Sign(f[5])) tFlip = r[0];
        if (tFlipL < 0 && Mathf.Sign(r[13]) != Mathf.Sign(f[13])) tFlipL = r[0];
        if (tAtkEnd < 0 && r[0] > 0.05f && r[10] > 0) tAtkEnd = r[0];
    }
    string mul = ""; float nx = 0.1f;
    foreach (var r in rec) { if (r[0] >= nx) { mul += r[0].ToString("F2", ic) + ":vx=" + r[3].ToString("F2", ic) + "/mul=" + r[9].ToString("F2", ic) + " "; nx += 0.15f; } }
    return name + ": dx=" + (l[1] - f[1]).ToString("F2", ic) + " maxVx=" + maxVx.ToString("F2", ic) + " hitDelay=" + hit.ToString("F3", ic) + " recoveryTotal=" + tot.ToString("F3", ic) + " firstTrue[jumpCancel=" + tJc.ToString("F2", ic) + " moveCancel=" + tMc.ToString("F2", ic) + " turn=" + tTn.ToString("F2", ic) + "] flip[player.scaleX=" + tFlip.ToString("F2", ic) + " weapon.lossyX=" + tFlipL.ToString("F2", ic) + "] atkEnd=" + tAtkEnd.ToString("F2", ic) + " scaleX " + f[5].ToString("F2", ic) + "->" + l[5].ToString("F2", ic) + " | " + mul;
};
System.Func<string, UnityEngine.InputSystem.Key[], float, System.Func<float, bool>> AtkTest = (name, ks, dur) => Step(
    () => { rec.Clear(); Weapon.Equipped.MeleeAttack(); },
    (t) => { Hold(ks); rec.Add(mk(t)); frames++; return t >= dur; },
    () => { Hold(none); L(Summ(name)); });
System.Func<string, float, System.Func<float, bool>> JumpTest = (name, rel) =>
{
    float y0 = 0, maxY = 0; bool seenAir = false;
    return Step(() => { y0 = pc.transform.position.y; maxY = y0; seenAir = false; },
        (t) => { Hold(t < rel ? new[] { Cj } : none); float y = pc.transform.position.y; maxY = Mathf.Max(maxY, y); if (!pc.isGround) seenAir = true; return (t > 0.3f && seenAir && pc.isGround) || t > 3f; },
        () => { Hold(none); L("JUMP " + name + ": release@" + rel.ToString("F2", ic) + " height=" + (maxY - y0).ToString("F3", ic)); });
};
System.Func<string, float, System.Func<float, bool>> BufTest = (name, lead) =>
{
    float y0 = 0; int ph = 0; float pressAt = -1, landAt = -1, leadReal = 0, reboundAt = -1; bool airJump = false; float maxVyAfter = 0; float prevVy = 0;
    return Step(() => { y0 = pc.transform.position.y; ph = 0; pressAt = -1; landAt = -1; reboundAt = -1; leadReal = 0; airJump = false; maxVyAfter = 0; prevVy = 0; },
        (t) =>
        {
            float y = pc.transform.position.y, vy = rb.linearVelocity.y;
            if (ph == 0) { Hold(new[] { Cj }); if (t > 0.5f) { Hold(none); ph = 1; } return false; }
            if (ph == 1)
            {
                if (vy < -0.5f && y > y0 + 0.05f && pressAt < 0) { float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale); float d = y - y0, v0 = -vy; float tl = (-v0 + Mathf.Sqrt(v0 * v0 + 2f * g * d)) / g; if (tl <= lead) { Hold(new[] { Cj }); pressAt = t; leadReal = tl; } }
                if (pressAt >= 0 && t > pressAt + 0.05f) { Hold(none); }
                if (pressAt >= 0 && vy > 3f && reboundAt < 0) { reboundAt = t; }
                if (pressAt >= 0 && pc.isGround && landAt < 0) { landAt = t; }
                prevVy = vy; return t > 5f || (pressAt >= 0 && t > pressAt + 0.9f);
            }
            return true;
        },
        () => { Hold(none); L("JUMP-BUFFER " + name + ": leadBeforeLanding=" + lead.ToString("F2", ic) + " pressedAt=" + pressAt.ToString("F2", ic) + " realLead(kinematic)=" + leadReal.ToString("F3", ic) + " firstGround=" + landAt.ToString("F2", ic) + " reboundVy>3 at=" + reboundAt.ToString("F2", ic) + " => " + (reboundAt >= 0 ? "버퍼 점프 발동(착지 후 재점프)" : "발동 안 함")); });
};
System.Func<string, bool, System.Func<float, bool>> AirTest = (name, keep) =>
{
    int ph = 0; float atkT = -1; var vs = new System.Collections.Generic.List<float[]>(); float vxPre = 0;
    return Step(() => { ph = 0; atkT = -1; vs.Clear(); vxPre = 0; pc.transform.position = startPos; rb.linearVelocity = Vector2.zero; },
        (t) =>
        {
            float vy = rb.linearVelocity.y, vx = rb.linearVelocity.x;
            if (ph == 0) { if (t < 0.5f) { Hold(new[] { R }); return false; } Hold(new[] { R, Cj }); if (t > 0.65f) { ph = 1; } return false; }
            if (ph == 1)
            {
                Hold(new[] { R });
                if (t > 0.9f && !pc.isGround && vy <= 0.8f && Weapon.Equipped.CanMeleeNow) { vxPre = vx; Weapon.Equipped.MeleeAttack(); atkT = t; ph = 2; }
                return t > 3f;
            }
            Hold(keep ? new[] { R } : none); vs.Add(new float[] { t - atkT, vx, pc.isGround ? 1 : 0 }); return t - atkT > 0.5f;
        },
        () =>
        {
            Hold(none); string s = ""; float nx = 0f;
            foreach (var v in vs) { if (v[0] >= nx) { s += v[0].ToString("F2", ic) + ":vx=" + v[1].ToString("F2", ic) + (v[2] > 0 ? "(land)" : "") + " "; nx += 0.08f; } }
            L("AIR-ATTACK " + name + ": vxBefore=" + vxPre.ToString("F2", ic) + " moveSpeed=" + pc.moveSpeed.ToString("F1", ic) + " attackAt=" + atkT.ToString("F2", ic) + " | " + s);
        });
};

// ---- 단위 수준(CF-4 상수) ----
{
    var cb = new CoyoteTimer(CombatRules.CoyoteTime); cb.SetGrounded(true, 1f); cb.SetGrounded(false, 1.0f);
    var jb = new InputBuffer(CombatRules.JumpBufferTime); jb.Press(1f);
    L("CONST JumpBuffer=" + CombatRules.JumpBufferTime + " Coyote=" + CombatRules.CoyoteTime + " JumpCut=" + CombatRules.JumpCutMul + " MinHold=" + CombatRules.JumpMinHold + " DashBuffer=" + CombatRules.DashBufferTime);
    L("UNIT coyote: inCoyote@+0.10=" + cb.InCoyote(1.10f) + " @+0.14=" + cb.InCoyote(1.14f) + " | jumpBuffer.Peek@+0.14=" + jb.Peek(1.14f) + " @+0.16=" + jb.Peek(1.16f));
    L("UNLOCK floor=" + (MovementUnlocks.Instance != null ? MovementUnlocks.Instance.HighestFloor.ToString() : "null") + " canDash=" + pc.CanMovementDash + " cancelStartRatio(1)=" + MovementUnlockRules.CancelStartRatio(1) + " (3)=" + MovementUnlockRules.CancelStartRatio(3) + " (5)=" + MovementUnlockRules.CancelStartRatio(5));
}
steps.Add(Step(() => { Hold(none); }, (t) => { if (t < 1.2f) return false; return pc.isGround; }, () => { startPos = pc.transform.position; L("START pos=" + startPos + " weapon=" + Weapon.Equipped.name + " moveSpeed=" + pc.moveSpeed + " isOverHand=" + Weapon.Equipped.isOverHand); }));
steps.Add(Face(1)); steps.Add(AtkTest("A1 앞키(오른쪽) 평타1", new[] { R }, 1.3f));
steps.Add(Face(1)); steps.Add(AtkTest("A2 무입력 평타1", none, 1.3f));
steps.Add(Face(1)); steps.Add(AtkTest("A3 뒷키(왼쪽) 평타1 - 방향 전환/히트박스", new[] { Lf }, 1.3f));
steps.Add(Face(-1)); steps.Add(AtkTest("A4 왼쪽 바라보고 오른쪽 키 평타1 - 반대 전환", new[] { R }, 1.3f));
steps.Add(Face(1));
steps.Add(Step(() => { rec.Clear(); },
    (t) => { Hold(new[] { R }); var w = Weapon.Equipped; if (w.CanMeleeNow && t < 2.2f) { w.MeleeAttack(); } rec.Add(mk(t)); return t >= 3.0f; },
    () =>
    {
        Hold(none); var l = rec[rec.Count - 1];
        L("A5 콤보 연속 앞키: dx=" + (l[1] - rec[0][1]).ToString("F2", ic));
        for (int c = 1; c <= 3; c++)
        {
            float sv = 0; int n = 0; float mx = 0;
            foreach (var r in rec) { if ((int)r[11] == c && r[10] == 0f) { sv += Mathf.Abs(r[3]); n++; mx = Mathf.Max(mx, r[9]); } }
            L("A5 combo" + c + " 후딜 평균|vx|=" + (n > 0 ? sv / n : 0).ToString("F2", ic) + " (샘플 " + n + ") maxMul=" + mx.ToString("F2", ic));
        }
    }));
steps.Add(Wait(0.8f)); steps.Add(JumpTest("탭 0.03s", 0.03f)); steps.Add(Wait(0.8f)); steps.Add(JumpTest("짧게 0.12s", 0.12f)); steps.Add(Wait(0.8f)); steps.Add(JumpTest("길게 1.0s", 1.0f));
steps.Add(Wait(0.8f)); steps.Add(BufTest("lead0.10", 0.10f)); steps.Add(Wait(1.0f)); steps.Add(BufTest("lead0.25", 0.25f));
steps.Add(Wait(1.0f)); steps.Add(AirTest("앞키 유지", true)); steps.Add(Wait(1.2f)); steps.Add(AirTest("키 뗌", false));
steps.Add(Step(() => { Hold(none); if (MovementUnlocks.Instance != null) { MovementUnlocks.Instance.DebugAll = true; } pc.transform.position = startPos; rb.linearVelocity = Vector2.zero; }, (t) => t > 1.2f && pc.isGround && Weapon.Equipped.CanMeleeNow, () => { L("DASH-SETUP canDash=" + pc.CanMovementDash + " stamina=" + PlayerHealthManager.Instance.nowStemina + " dashCost=" + pc.dashStemina); }));
{
    float dStart = -1;
    steps.Add(Step(() => { dStart = -1; Weapon.Equipped.MeleeAttack(); },
        (t) => { if (t >= 0.14f && t < 0.2f) { Hold(new[] { Sh }); } else { Hold(none); } if (pc.states == "dash" && dStart < 0) { dStart = t; } return t > 1.2f; },
        () => { Hold(none); L("DASH-CANCEL 평타 후딜 중(0.14s) 대시 입력: dashStarted@" + dStart.ToString("F2", ic) + " (-1=안 됨), recoveryTotal=" + getF(Weapon.Equipped, "_attackWaitTotal").ToString("F2", ic)); }));
    steps.Add(Wait(1.5f));
    foreach (float lead in new[] { 0.06f, 0.30f })
    {
        float ld = lead; int ph = 0; float tp1 = -1, tp2 = -1; int dcount = 0; bool prevDash = false; float cd0 = 0;
        steps.Add(Step(() => { ph = 0; tp1 = -1; tp2 = -1; dcount = 0; prevDash = false; cd0 = 0; pc.transform.position = startPos; rb.linearVelocity = Vector2.zero; },
            (t) =>
            {
                float dtm = getF(pc, "dashTimer"); bool isD = pc.states == "dash";
                if (isD && !prevDash) { dcount++; }
                prevDash = isD;
                if (ph == 0) { if (t < 0.6f) { Hold(none); return false; } Hold(new[] { Sh }); tp1 = t; ph = 1; return false; }
                if (ph == 1)
                {
                    if (t > tp1 + 0.05f) { Hold(none); }
                    if (dtm > cd0) { cd0 = dtm; }
                    if (tp2 < 0 && dtm > 0f && dtm <= ld && t > tp1 + 0.1f) { Hold(new[] { Sh }); tp2 = t; ph = 2; }
                    return t > tp1 + 3f;
                }
                if (t > tp2 + 0.05f) { Hold(none); }
                return t > tp2 + 0.6f;
            },
            () => { Hold(none); L("DASH-BUFFER 쿨다운 끝 " + ld.ToString("F2", ic) + "s 전 재입력: pressedAt=" + tp2.ToString("F2", ic) + " dashCount=" + dcount + " (2면 버퍼 발동, 1이면 무시) 최대쿨다운=" + cd0.ToString("F2", ic)); }));
        steps.Add(Wait(1.5f));
    }
}
steps.Add(Step(() => { }, (t) => true, () => { if (MovementUnlocks.Instance != null) { MovementUnlocks.Instance.DebugAll = false; } float rt = Time.realtimeSinceStartup - fpsT0; L("FPS frames(공격 구간)=" + frames + " realSec=" + rt.ToString("F1", ic) + " DONE"); }));

int si = 0; float t0 = -1; UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        if (si >= steps.Count) { UnityEditor.EditorApplication.update -= tick; Hold(none); return; }
        if (t0 < 0) { t0 = Time.time; }
        if (steps[si](Time.time - t0)) { si++; t0 = -1; }
    }
    catch (System.Exception ex) { L("EXCEPTION step " + si + ": " + ex.GetType().Name + " " + ex.Message + "\n" + ex.StackTrace); UnityEditor.EditorApplication.update -= tick; Hold(none); }
};
UnityEditor.EditorApplication.update += tick;
return "pv1_cf started steps=" + steps.Count;
