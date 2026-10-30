// 요리 자동 조작(MiniGames.md ②, 플레이 중, cooking_setup -> input_bg_on 먼저): 가상 키(↑ 시작, 좌우 고르기, X 확인/뒤집기/홀드, ESC 중단).
// __MODE__ = grill_perfect(커서가 목표에 올 때 X x3 -> 약초구이 토끼 2개) / grill_fail(안 누름 -> 탄 요리 1)
//          / boil_perfect(화력 52 아래면 X 홀드 -> 이끼미꾸라지 탕 2개) / boil_fail(계속 홀드 -> 과열 즉시 실패 -> 탄 요리 1) / esc(시작 뒤 ESC -> 재료 손실·결과 없음)
// 게임 시간을 늦춰(timeScale) 한 프레임의 커서 이동을 줄인다. 진행 기록은 "[CookProbe] ..." 오브젝트 이름에 쌓인다
var kb = UnityEngine.InputSystem.Keyboard.current;
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[CookProbe]")) UnityEngine.Object.Destroy(g);
var inv = InventoryManager.instance;
string mode = "__MODE__";
bool grill = mode.StartsWith("grill") || mode == "esc";
string target = grill ? "Herb Roasted Rabbit" : "Moss Loach Broth";
CookRecipe recipe = null;
foreach (var r in CookingRules.Recipes) if (r.Id == target) recipe = r;
foreach (var ing in recipe.Ingredients) { int have = inv.CountItem(ing.Id); if (have < ing.Count) inv.Service.Add(ing.Id, ing.Count - have, AddSource.Other); }
System.Func<string> counts = () =>
{
    var sb = new System.Text.StringBuilder();
    foreach (var ing in recipe.Ingredients) sb.Append(ing.Id).Append('=').Append(inv.CountItem(ing.Id)).Append(',');
    return sb.Append(target).Append('=').Append(inv.CountItem(target)).Append(",Burnt=").Append(inv.CountItem(CookingRules.BurntId)).ToString();
};
var log = new UnityEngine.GameObject("[CookProbe] __MODE__ before=" + counts());
UnityEngine.Time.timeScale = grill ? 0.25f : 0.5f;
UnityEngine.InputSystem.Key held = UnityEngine.InputSystem.Key.None;
bool sticky = false, begun = false, escSent = false;
float t0 = UnityEngine.Time.realtimeSinceStartup;
System.Action<UnityEngine.InputSystem.Key> press = k => { held = k; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, k == UnityEngine.InputSystem.Key.None ? new UnityEngine.InputSystem.LowLevel.KeyboardState() : new UnityEngine.InputSystem.LowLevel.KeyboardState(k)); };
DG.Tweening.Tween tw = null;
tw = DG.Tweening.DOVirtual.Float(0f, 1f, 160f, v =>
{
    var stove = CookingStove.Current;
    var s = stove != null ? stove.Session : null;
    if (!sticky && held != UnityEngine.InputSystem.Key.None) { press(UnityEngine.InputSystem.Key.None); return; } // 한 프레임 누름 뒤 바로 뗌
    if (s == null)
    {
        if (!begun) { press(UnityEngine.InputSystem.Key.UpArrow); return; }
        press(UnityEngine.InputSystem.Key.None);
        UnityEngine.Time.timeScale = 1f;
        log.name += " END after=" + counts() + " lock=" + UiLock.IsHeld(CookingStove.Lock);
        DG.Tweening.TweenExtensions.Kill(tw, false); return;
    }
    if (!begun) { begun = true; log.name += " begun@" + (UnityEngine.Time.realtimeSinceStartup - t0).ToString("F1") + " list=" + s.Recipes.Count; return; }
    if (s.Phase == CookPhase.Select)
    {
        if (s.Recipes[s.Index].Id != target) press(UnityEngine.InputSystem.Key.RightArrow);
        else { log.name += " pick=" + s.Recipes[s.Index].Id + " method=" + s.Recipes[s.Index].Method; press(UnityEngine.InputSystem.Key.X); }
        return;
    }
    if (s.Phase == CookPhase.Result)
    {
        if (sticky) { sticky = false; press(UnityEngine.InputSystem.Key.None); }
        if (!log.name.Contains("res=")) log.name += " res=" + s.Grade + " cancelled=" + s.Cancelled + " overheated=" + s.Overheated + " errors=" + string.Join("/", s.Errors) + " misses=" + s.Misses + " zone=" + (s.InZoneSeconds / CookingRules.BoilTime).ToString("F2") + " got=" + stove.ResultId + "x" + stove.ResultCount + " bagFull=" + stove.BagFull + " consumed=" + stove.Consumed;
        return;
    }
    if (mode == "esc") { if (!escSent) { escSent = true; press(UnityEngine.InputSystem.Key.Escape); } return; }
    if (grill)
    {
        if (mode == "grill_perfect" && s.WindowOpen && System.Math.Abs(s.GrillCursor - s.GrillTarget) <= 3f) press(UnityEngine.InputSystem.Key.X);
        return;
    }
    // 끓이기
    if (mode == "boil_fail") { if (!sticky) { sticky = true; press(UnityEngine.InputSystem.Key.X); } return; }
    if (s.Heat < 52f && !sticky) { sticky = true; press(UnityEngine.InputSystem.Key.X); }
    else if (s.Heat >= 54f && sticky) { sticky = false; press(UnityEngine.InputSystem.Key.None); }
});
DG.Tweening.TweenSettingsExtensions.SetUpdate(tw, true);
return "driving __MODE__ before=" + counts();
