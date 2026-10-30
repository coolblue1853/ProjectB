// 채집·채굴 자동 조작(MiniGames.md, 플레이 중, gather_setup → input_bg_on 먼저): 가상 ↑ 키.
// __MODE__ = perfect(표본: 커서가 목표 ±2 안일 때 누름) / miss(표본: 목표와 30 이상 떨어질 때만 누름 → 3회 실패) / crit(광맥: 약점이 빛날 때만) / plain(광맥: 0.6초마다, 빛날 때 피함)
// 진행 기록은 "[GatherProbe] ..." 오브젝트 이름에 쌓인다(gather_check.cs 로 읽기)
var kb = UnityEngine.InputSystem.Keyboard.current;
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[GatherProbe]")) UnityEngine.Object.Destroy(g);
var inv = InventoryManager.instance;
string[] ids = { "GreenHurb", "Forest Mushroom", "Glow Moth Wing", "Moonpetal", "T0_Ore", "Bog Copper Ore", "Starfall Ore" };
System.Func<string> counts = () => { var sb = new System.Text.StringBuilder(); foreach (var id in ids) sb.Append(inv.CountItem(id)).Append(','); return sb.ToString(); };
var log = new UnityEngine.GameObject("[GatherProbe] __MODE__ before=" + counts());
string mode = "__MODE__";
bool down = false, begun = false;
float lastPress = -9f;
float t0 = UnityEngine.Time.time;
System.Action<bool> set = p => { if (p == down) return; down = p; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, p ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.UpArrow) : new UnityEngine.InputSystem.LowLevel.KeyboardState()); };
DG.Tweening.Tween tw = null;
tw = DG.Tweening.DOVirtual.Float(0f, 1f, 60f, v =>
{
    var node = GatherNode.Current;
    var s = node != null ? node.Session : null;
    float now = UnityEngine.Time.time;
    if (s == null)
    {
        if (!begun) { set(!down); return; } // 눌렀다 뗐다 → 시작
        set(false); log.name += " END after=" + counts() + " lock=" + UiLock.IsHeld(GatherNode.LockOwner) + " ready=" + (node == null ? "?" : "-"); DG.Tweening.TweenExtensions.Kill(tw, false); return;
    }
    if (!begun) { begun = true; set(false); log.name += " begun@" + (now - t0).ToString("F1"); return; }
    if (s.Done)
    {
        set(false);
        if (!log.name.Contains("res=")) log.name += " res=" + s.Result + " item=" + s.Item.Id + " x" + s.Count + (s is SpecimenSession ? " judge=" + ((SpecimenSession)s).LastJudge + " tries=" + ((SpecimenSession)s).AttemptsUsed : " swings=" + ((MiningSession)s).Swings + " crits=" + ((MiningSession)s).Crits) + " bagFull=" + node.BagFull;
        return;
    }
    if (down) { set(false); return; } // 한 프레임 누름 뒤 바로 뗌
    bool want = false;
    var sp = s as SpecimenSession;
    var m = s as MiningSession;
    if (sp != null && !sp.Paused)
    {
        float d = Mathf.Abs(sp.Cursor - sp.Target);
        want = mode == "perfect" ? d <= 2f : d >= 30f;
        if (want && now - lastPress < 0.3f) want = false;
    }
    else if (m != null)
    {
        want = mode == "crit" ? m.Glowing : (!m.Glowing && now - lastPress >= 0.6f);
        if (want && now - lastPress < 0.35f) want = false;
    }
    if (want) { lastPress = now; set(true); }
});
DG.Tweening.TweenSettingsExtensions.SetUpdate(tw, true);
return "driving __MODE__ before=" + counts();
