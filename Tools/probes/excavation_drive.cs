// 발굴 자동 조작(MiniGames.md ③, 플레이 중, excavation_setup → input_bg_on 먼저): 가상 키(↑ 시작, 방향키 커서, X 파기, ESC 중단).
// __MODE__ = win(유물 칸만 찾아 파기 → 성공) / collapse(유물 1개 캔 뒤 위험 칸 2개 → 붕괴, 캔 유물 유지) / esc(시작 직후 ESC → 중단)
//          / record(시작 직후 보상표를 '기록 100%'로 바꿔 win) / recordall(기록 5편을 모두 모은 상태로 record → 기어 코어 대체)
// 진행 기록은 "[ExcProbe] ..." 오브젝트 이름에 쌓인다(gather_check 와 같은 방식으로 읽기)
var kb = UnityEngine.InputSystem.Keyboard.current;
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[ExcProbe]")) UnityEngine.Object.Destroy(g);
var inv = InventoryManager.instance;
string[] ids = { "Ancient Relic Shard", "Ancient Blueprint Fragment", "Ancient Gear Core", "Forgotten Runestone" };
System.Func<string> counts = () => { var sb = new System.Text.StringBuilder(); foreach (var id in ids) sb.Append(inv.CountItem(id)).Append(','); return sb.ToString() + " rec=" + FathersRecords.LoadMask(); };
string mode = "__MODE__";
if (mode == "recordall") ES3.Save(FathersRecords.SaveKey, 31);
if (mode == "record") ES3.Save(FathersRecords.SaveKey, 0);
var log = new UnityEngine.GameObject("[ExcProbe] __MODE__ before=" + counts());
UnityEngine.InputSystem.Key held = UnityEngine.InputSystem.Key.None;
bool begun = false, swapped = false;
int hazardsDug = 0;
float t0 = UnityEngine.Time.time;
System.Action<UnityEngine.InputSystem.Key> press = k => { held = k; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, k == UnityEngine.InputSystem.Key.None ? new UnityEngine.InputSystem.LowLevel.KeyboardState() : new UnityEngine.InputSystem.LowLevel.KeyboardState(k)); };
DG.Tweening.Tween tw = null;
tw = DG.Tweening.DOVirtual.Float(0f, 1f, 70f, v =>
{
    var site = ExcavationSite.Current;
    var s = site != null ? site.Session : null;
    if (held != UnityEngine.InputSystem.Key.None) { press(UnityEngine.InputSystem.Key.None); return; } // 한 프레임 누름 뒤 바로 뗌
    if (s == null)
    {
        if (!begun) { press(UnityEngine.InputSystem.Key.UpArrow); return; }
        log.name += " END after=" + counts() + " lock=" + UiLock.IsHeld(ExcavationSite.Lock) + " ready=" + (site == null ? "-" : "?");
        DG.Tweening.TweenExtensions.Kill(tw, false); return;
    }
    if (!begun) { begun = true; log.name += " begun@" + (UnityEngine.Time.time - t0).ToString("F1") + " rare=" + s.Spec.Rare; return; }
    if ((mode == "record" || mode == "recordall") && !swapped)
    {
        swapped = true;
        var table = new GatherDef[] { new GatherDef(ExcavationRules.RecordId, ItemRarity.U, 100f, 1, 1) };
        var fresh = new ExcavationSession(s.Spec, table, () => UnityEngine.Random.value);
        typeof(ExcavationSite).GetProperty("Session").SetValue(site, fresh, null);
        return;
    }
    if (s.Done)
    {
        if (!log.name.Contains("res=")) log.name += " res=" + s.Result + " finds=" + s.Finds.Count + " picks=" + s.PicksLeft + " collapse=" + s.Collapse + " recordFound=" + site.RecordFound + " replaced=" + site.RecordReplaced + " bagFull=" + site.BagFull;
        return;
    }
    if (mode == "esc") { press(UnityEngine.InputSystem.Key.Escape); return; }
    int target = -1;
    for (int i = 0; i < s.Size * s.Size && target < 0; i++)
    {
        if (s.IsDug(i)) continue;
        bool hazard = s.ContentAt(i) == DigContent.Hazard;
        bool relic = s.ContentAt(i) == DigContent.Relic;
        if (mode == "collapse" && s.RelicsFound >= 1 ? hazard : relic) target = i;
    }
    if (target < 0) return;
    int dx = target % s.Size - s.Cursor % s.Size;
    int dy = target / s.Size - s.Cursor / s.Size;
    if (dx > 0) press(UnityEngine.InputSystem.Key.RightArrow);
    else if (dx < 0) press(UnityEngine.InputSystem.Key.LeftArrow);
    else if (dy > 0) press(UnityEngine.InputSystem.Key.DownArrow);
    else if (dy < 0) press(UnityEngine.InputSystem.Key.UpArrow);
    else { press(UnityEngine.InputSystem.Key.X); if (s.ContentAt(target) == DigContent.Hazard) hazardsDug++; }
});
DG.Tweening.TweenSettingsExtensions.SetUpdate(tw, true);
return "driving __MODE__ before=" + counts();
