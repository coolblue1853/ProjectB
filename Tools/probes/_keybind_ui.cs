// AUTO-116: 키 설정 UI 점검(플레이 모드, execute_code). 설정 저장 파일(SaveFile.es3)은 실행 전 백업·후 복원할 것
var sb = new System.Text.StringBuilder();
var reb = InputBindings.Rebindable;
sb.AppendLine("[1] 행: " + reb.Length + " 액션 + RESET + BACK");
var before = new System.Collections.Generic.Dictionary<string, string>();
foreach (var a in reb) { string k = InputBindings.Display(a, false), g = InputBindings.Display(a, true); before[a] = k + "|" + g; sb.AppendLine("  " + a + " 키보드=" + k + " 패드=" + g); }
KeyBindWindow.Open();
sb.AppendLine("[1b] 창 열림 IsOpen=" + KeyBindWindow.IsOpen);
// 2 충돌
string swapped; var asset = InputBindings.Master.asset;
string jumpK = asset.FindAction("Player/Jump").bindings[InputBindings.BindingIndex(asset.FindAction("Player/Jump"), false)].effectivePath;
string dashK = asset.FindAction("Player/Dash").bindings[InputBindings.BindingIndex(asset.FindAction("Player/Dash"), false)].effectivePath;
var r1 = InputBindings.Assign(asset, "Jump", false, dashK, out swapped);
string jumpAfter = InputBindings.Display("Jump", false), dashAfter = InputBindings.Display("Dash", false);
sb.AppendLine("[2] 충돌(Jump<-Dash 키) = " + r1 + " swapped=" + swapped + " Jump=" + jumpAfter + " Dash=" + dashAfter + " (교환 기대)");
var r2 = InputBindings.Assign(asset, "Jump", false, "<Keyboard>/escape", out swapped);
var r3 = InputBindings.Assign(asset, "Jump", false, "<Keyboard>/m", out swapped);
var r4 = InputBindings.Assign(asset, "Jump", true, "<Gamepad>/start", out swapped);
var r5 = InputBindings.Assign(asset, "Jump", false, "<Keyboard>/leftArrow", out swapped);
sb.AppendLine("[2b] 예약키 esc=" + r2 + " M=" + r3 + " 패드Start=" + r4 + " 이동키(←)=" + r5);
// 4 필수 액션 미지정
var r6 = InputBindings.Assign(asset, "Attack", false, "", out swapped);
var r7 = InputBindings.Assign(asset, "Attack", false, null, out swapped);
var r8 = InputBindings.Assign(asset, "NoSuchAction", false, "<Keyboard>/f9", out swapped);
sb.AppendLine("[4] 빈 경로 지정 Attack = " + r6 + "/" + r7 + " 없는 액션=" + r8 + " (Invalid 기대 → 키 제거 불가)");
// 3 저장 후 재로드
InputBindings.Assign(asset, "Jump", false, "<Keyboard>/f9", out swapped); InputBindings.Commit();
string saved = ES3.Load<string>(InputBindings.SaveKey, SaveSlots.SharedFile);
var fresh = new KeyAction(); UnityEngine.InputSystem.InputActionRebindingExtensions.LoadBindingOverridesFromJson(fresh.asset, saved);
var fa = fresh.asset.FindAction("Player/Jump");
sb.AppendLine("[3] 저장 JSON 길이 " + saved.Length + " 재로드 Jump=" + UnityEngine.InputSystem.InputActionRebindingExtensions.GetBindingDisplayString(fa, InputBindings.BindingIndex(fa, false)) + " 마스터=" + InputBindings.Display("Jump", false));
fresh.Dispose();
// 기본값 복원
InputBindings.ResetToDefaults();
bool same = true; foreach (var a in reb) { string now = InputBindings.Display(a, false) + "|" + InputBindings.Display(a, true); if (now != before[a]) { same = false; sb.AppendLine("  복원 불일치 " + a + " " + before[a] + " -> " + now); } }
string saved2 = ES3.Load<string>(InputBindings.SaveKey, SaveSlots.SharedFile);
sb.AppendLine("[5] 기본값 복원 일치=" + same + " 저장 JSON 길이 " + saved2.Length);
// 6 패드 조작: 창 UI 액션 패드 바인딩
var ka = new KeyAction();
foreach (var act in new[] { ka.UI.UPInventory, ka.UI.DownInventory, ka.UI.LeftInventory, ka.UI.RightInventory, ka.UI.Select })
{
    var ps = new System.Collections.Generic.List<string>(); foreach (var b in act.bindings) if (!b.isComposite) ps.Add(b.effectivePath);
    sb.AppendLine("[6] " + act.name + " : " + string.Join(", ", ps));
}
ka.Dispose();
bool begin = KeyBindWindow.BeginRebind("Attack", true);
sb.AppendLine("[6b] 패드 칸 재바인딩 진입=" + begin + " 대기중=" + KeyBindWindow.IsWaitingForKey);
var m = typeof(KeyBindWindow).GetMethod("CancelRebind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var inst = (KeyBindWindow)typeof(KeyBindWindow).GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
m.Invoke(inst, null);
sb.AppendLine("[6c] 취소 후 대기중=" + KeyBindWindow.IsWaitingForKey + " Attack 패드=" + InputBindings.Display("Attack", true) + " (취소 시작버튼 설정: 코드상 <Gamepad>/start, 제한시간 8초)");
return sb.ToString();
