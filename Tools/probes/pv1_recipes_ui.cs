// PV1 (5) 방어구 대장장이 제작 창을 열고 E 키로 페이지를 넘기며 캡처. 결과 Logs/pv1/recipes_ui.log, 캡처 Logs/pv1/recipes_pN.png
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/recipes_ui.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
var iset = UnityEngine.InputSystem.InputSystem.settings;
iset.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
iset.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var kb = UnityEngine.InputSystem.Keyboard.current; UnityEngine.InputSystem.InputSystem.EnableDevice(kb); Application.runInBackground = true;
Time.timeScale = 1f;
var pc = PlayerController.instance; var rb = pc.GetComponent<Rigidbody2D>();
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
MapInfo.GoToMap("Town", pc.gameObject);
CraftingManager cm = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<CraftingManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (!c.isSelfCraft && !c.isCraftWeapon) cm = c; }
var smith = UnityEngine.Object.FindFirstObjectByType<EquipSmithUI>();
if (cm == null || smith == null) { return "missing cm/smith"; }
var fl = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
int st = 0; float st0 = Time.time; int page = 0; int shots = 0;
int[] want = { 0, 2, 5, 6, 9, 11 };
UnityEditor.EditorApplication.CallbackFunction tick = null;
System.Action<UnityEngine.InputSystem.Key[]> Hold = (ks) => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(ks)); };
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        float t = Time.time - st0;
        if (st == 0)
        {
            if (t < 1.5f) { return; }
            pc.transform.position = new Vector3(smith.transform.position.x, smith.transform.position.y + 1f, 0f); rb.linearVelocity = Vector2.zero;
            L("smith pos=" + smith.transform.position + " near=" + smith.isShopClose + " isOpenUI=" + DatabaseManager.isOpenUI);
            st = 1; st0 = Time.time; return;
        }
        if (st == 1)
        {
            if (t < 0.8f) { return; }
            cm.EquipOpen(); L("opened UI active=" + cm.CraftUI.activeSelf);
            st = 2; st0 = Time.time; return;
        }
        if (st == 2)
        {   // 페이지 넘기기: E 키 눌렀다 떼기(0.15초 간격)
            int cur = (int)typeof(CraftingManager).GetField("nowPage", fl).GetValue(cm);
            if (t < 0.6f) { return; }
            bool shoot = System.Array.IndexOf(want, cur) >= 0;
            string need = ""; var ncheck = cm.nowCraftItem; if (ncheck != null) { need = ncheck.name + " itemData=" + (ncheck.itemData != null); }
            if (shoot && !System.IO.File.Exists("Logs/pv1/recipes_p" + cur + ".png"))
            {
                ScreenCapture.CaptureScreenshot("Logs/pv1/recipes_p" + cur + ".png"); shots++;
                var pg = cm.LV1craftBox[0].transform.GetChild(cur); string names = ""; for (int l = 0; l < pg.childCount; l++) { var ln = pg.GetChild(l); for (int c = 0; c < ln.childCount; c++) { var chk = ln.GetChild(c).GetComponent<CraftItemCheck>(); names += ln.GetChild(c).name + (chk != null && chk.itemData != null ? "" : "(!)") + " "; } }
                L("page " + cur + "/" + (cm.LV1craftBox[0].transform.childCount - 1) + " shot; cells: " + names + "| cursor=" + need);
                st0 = Time.time - 0.2f; return;
            }
            if (cur >= cm.LV1craftBox[0].transform.childCount - 1) { L("last page reached " + cur); st = 3; st0 = Time.time; Hold(new UnityEngine.InputSystem.Key[0]); return; }
            if (t < 0.9f) { Hold(new[] { UnityEngine.InputSystem.Key.E }); }
            else { Hold(new UnityEngine.InputSystem.Key[0]); st0 = Time.time; }
            return;
        }
        if (st == 3)
        {
            if (t < 1.5f) { return; }
            L("DONE shots=" + shots); UnityEditor.EditorApplication.update -= tick;
            return;
        }
    }
    catch (System.Exception ex) { L("EXCEPTION " + ex.Message + "\n" + ex.StackTrace); UnityEditor.EditorApplication.update -= tick; }
};
UnityEditor.EditorApplication.update += tick;
return "recipes_ui started";
