// 낚시 자동 조작(MiniGames.md, 플레이 중, input_bg_on 먼저): 가상 ↑ 키로 캐스팅(0.8초 홀드=최적) → 입질에 누름 → 장력 조절.
// __MODE__ = win(안전 구간 가운데 기준으로 누르고 떼기) / snap(계속 누름 → 줄 끊어짐). 진행 기록은 "[FishProbe] ..." 오브젝트 이름에 쌓인다(fishing_check.cs 로 읽기)
var kb = UnityEngine.InputSystem.Keyboard.current;
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[FishProbe]")) UnityEngine.Object.Destroy(g);
var inv = InventoryManager.instance;
string[] ids = { "Forest Carp", "Moss Loach", "Amber Trout", "Spirit Koi" };
int before = 0; foreach (var id in ids) before += inv.CountItem(id);
var log = new UnityEngine.GameObject("[FishProbe] __MODE__ before=" + before);
bool win = "__MODE__" == "win";
float t0 = UnityEngine.Time.time;
bool down = false, begun = false;
string lastPhase = "";
System.Action<bool> set = p => { if (p == down) return; down = p; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, p ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.UpArrow) : new UnityEngine.InputSystem.LowLevel.KeyboardState()); };
DG.Tweening.Tween tw = null;
tw = DG.Tweening.DOVirtual.Float(0f, 1f, 120f, v =>
{
    var spot = FishingSpot.Current;
    var s = spot != null ? spot.Session : null;
    float el = UnityEngine.Time.time - t0;
    if (s == null)
    {
        if (!begun) { set(true); }
        else { set(false); int after = 0; foreach (var id in ids) after += inv.CountItem(id); log.name += " END after=" + after + " ES3rod=" + FishingSave.LoadRod(); DG.Tweening.TweenExtensions.Kill(tw, false); }
        return;
    }
    begun = true;
    string ph = s.Phase.ToString();
    if (ph != lastPhase) { lastPhase = ph; log.name += " " + ph + "@" + el.ToString("F1"); }
    switch (s.Phase)
    {
        case FishingPhase.Casting: set(s.PhaseTime < 0.8f); break;
        case FishingPhase.Waiting: set(false); break;
        case FishingPhase.Bite: set(true); break;
        case FishingPhase.Reeling: set(win ? s.Tension.Tension < (s.Tension.SafeMin + s.Tension.SafeMax) * 0.5f : true); break;
        default:
            set(false);
            if (!log.name.Contains("res=")) log.name += " res=" + s.Result + " cast=" + s.Cast + " fish=" + s.Fish.Id + " x" + s.Count + " t=" + (s.Tension != null ? s.Tension.Tension.ToString("F0") + "/" + s.Tension.Progress.ToString("F0") : "-") + " bagFull=" + spot.BagFull + " lock=" + UiLock.IsHeld(FishingSpot.LockOwner);
            break;
    }
});
DG.Tweening.TweenSettingsExtensions.SetUpdate(tw, true);
return "driving __MODE__ before=" + before + " near spot? current=" + (FishingSpot.Current != null);
