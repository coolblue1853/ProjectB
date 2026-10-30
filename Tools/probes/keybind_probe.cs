// 키 설정 검증(BasicFeatures.md 5절, 플레이 중). __PHASE__:
//  setup  = 물약 지급·퀵슬롯1 등록 → 일시정지 → 키 설정 창 → 퀵슬롯1 키보드 재지정 대기 (다음: press_key Digit7)
//  check  = 재지정 결과(표시·저장) 보고 (다음: press_key Escape 두 번 → press_key Digit7 → press_key V)
//  result = 물약 수량·아티팩트 쿨다운 보고 후 기본 키로 복원
string phase = "__PHASE__";
var inv = InventoryManager.instance;
if (phase == "setup")
{
    inv.CreatItem("Potion"); inv.CreatItem("Potion");
    ItemCheck potion = null;
    foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "Potion") { potion = c; break; } }
    QuickSlotBar.AssignFrom(0, potion);
    PauseMenu.OpenNow();
    KeyBindWindow.Open();
    bool started = KeyBindWindow.BeginRebind("Quick1", false);
    UnityEngine.PlayerPrefs.SetInt("pb_probe_potion", inv.CountItem("Potion"));
    return "started=" + started + " waiting=" + KeyBindWindow.IsWaitingForKey + " quick1=" + InputBindings.Display("Quick1", false) + " artifact=" + InputBindings.Display("Artifact", false);
}
if (phase == "check")
{
    string json = ES3.KeyExists(InputBindings.SaveKey, SaveSlots.SharedFile) ? ES3.Load<string>(InputBindings.SaveKey, SaveSlots.SharedFile) : "";
    return "waiting=" + KeyBindWindow.IsWaitingForKey + " quick1=" + InputBindings.Display("Quick1", false) + " savedHas7=" + json.Contains("<Keyboard>/7") + " windowOpen=" + KeyBindWindow.IsOpen + " pause=" + PauseMenu.IsOpen;
}
int p0 = UnityEngine.PlayerPrefs.GetInt("pb_probe_potion");
UnityEngine.PlayerPrefs.DeleteKey("pb_probe_potion");
string r = "windowOpen=" + KeyBindWindow.IsOpen + " pause=" + PauseMenu.IsOpen + " potion " + p0 + "->" + inv.CountItem("Potion") + " artifactCd=" + ArtifactSystem.CooldownLeft.ToString("F1");
InputBindings.ResetToDefaults();
return r + " reset quick1=" + InputBindings.Display("Quick1", false);
