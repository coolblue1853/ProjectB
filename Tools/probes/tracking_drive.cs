// 흔적 추적 자동 조작(MiniGames.md ⑥, 플레이 중, tracking_setup → input_bg_on 먼저): 가상 키(↑ 시작, ← ↑ → 흔적 고르기, X 덫, ESC 중단).
// __MODE__ = perfect(정답 4번 → 커서 가운데에서 X → 완벽) / normal(정답 4번 → 5초 기다려 보통) / miss(오답 3번 → 실패) / esc(시작 직후 ESC)
// 진행 기록은 "[TrkProbe] ..." 오브젝트 이름에 쌓인다(tracking_check 로 읽기)
var kb = UnityEngine.InputSystem.Keyboard.current;
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[TrkProbe]")) UnityEngine.Object.Destroy(g);
string mode = "__MODE__";
var log = new UnityEngine.GameObject("[TrkProbe] __MODE__");
UnityEngine.InputSystem.Key held = UnityEngine.InputSystem.Key.None;
bool begun = false;
UnityEngine.InputSystem.Key[] dirs = { UnityEngine.InputSystem.Key.LeftArrow, UnityEngine.InputSystem.Key.UpArrow, UnityEngine.InputSystem.Key.RightArrow };
System.Action<UnityEngine.InputSystem.Key> press = k => { held = k; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, k == UnityEngine.InputSystem.Key.None ? new UnityEngine.InputSystem.LowLevel.KeyboardState() : new UnityEngine.InputSystem.LowLevel.KeyboardState(k)); };
DG.Tweening.Tween tw = null;
int lastStage = -1, lastMist = -1;
float wait = 0f;
tw = DG.Tweening.DOVirtual.Float(0f, 1f, 70f, v =>
{
    var spot = TrackingSpot.Current;
    var s = spot != null ? spot.Session : null;
    if (held != UnityEngine.InputSystem.Key.None) { press(UnityEngine.InputSystem.Key.None); return; }
    if (s == null)
    {
        if (!begun) { press(UnityEngine.InputSystem.Key.UpArrow); return; }
        var t = EnemySpawner.TrackedTarget;
        log.name += " END lock=" + UiLock.IsHeld(TrackingSpot.Lock) + " tracked=" + (t == null ? "-" : t.name + " elite=" + t.IsElite + " stun=" + t.isStun + " pool=" + t.IsInPool);
        DG.Tweening.TweenExtensions.Kill(tw, false); return;
    }
    if (!begun) { begun = true; log.name += " begun target=" + spot.TargetPrefab + " trace=" + s.Target; return; }
    if (s.Done)
    {
        if (!log.name.Contains("res=")) log.name += " res=" + s.Result + " q=" + s.Quality + " mist=" + s.Mistakes + " summoned=" + spot.Summoned + " bonus=" + string.Join("|", spot.Bonus);
        return;
    }
    if (mode == "esc") { press(UnityEngine.InputSystem.Key.Escape); return; }
    if (UnityEngine.Time.unscaledTime < wait) return;
    if (s.Phase == TrackPhase.Trap)
    {
        if (mode == "perfect" && Mathf.Abs(s.TrapCursor - 50f) < 4f) { press(UnityEngine.InputSystem.Key.X); log.name += " X@" + s.TrapCursor.ToString("F0"); }
        return;
    }
    if (s.Elapsed < 0.4f) return;
    if (s.Stage != lastStage || s.Mistakes != lastMist) { lastStage = s.Stage; lastMist = s.Mistakes; wait = UnityEngine.Time.unscaledTime + 0.3f; return; }
    int pick = s.Correct;
    if (mode == "miss") { for (int i = 0; i < 3; i++) if (i != s.Correct && !s.IsCrossed(i)) { pick = i; break; } }
    press(dirs[pick]);
});
DG.Tweening.TweenSettingsExtensions.SetUpdate(tw, true);
return "driving __MODE__";
