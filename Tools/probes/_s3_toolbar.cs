// S3/S5 인벤 버튼 확인 → Logs/playtest/s3_tb.txt
var inv = InventoryManager.instance; var sb = new System.Text.StringBuilder();
var steps = new System.Collections.Generic.List<System.Func<string>>();
System.Func<string, UnityEngine.UI.Button> btn = nm => { var t = inv.inventory.transform.Find("InventoryToolbar/" + nm); return t != null ? t.GetComponent<UnityEngine.UI.Button>() : null; };
steps.Add(() => { foreach (var n in new[] { "Potion" }) { while (inv.CountItem(n) > 0) { inv.Service.Remove(n, inv.CountItem(n)); } }
  inv.state = ""; inv.chest = null; for (int i = 0; i < 3; i++) inv.CreatItem("Potion");
  inv.inventory.SetActive(true); inv.nowBox = 0; inv.OpenBox(0); UiLock.Set("InvGroup", true); inv.cusor.SetActive(true); return "setup"; });
steps.Add(null); steps.Add(null);
steps.Add(() => "toolbar=" + (inv.inventory.transform.Find("InventoryToolbar") != null) + " btn4=" + (btn("Btn4") != null) + " bagLabel=" + BagExpansion.ButtonLabel());
steps.Add(() => { ScreenCapture.CaptureScreenshot("Logs/playtest/s3_toolbar.png"); return "shot"; });
steps.Add(null); steps.Add(null);
steps.Add(() => { var it = new System.Collections.Generic.List<ItemCheck>(); foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") it.Add(c); inv.Service.SplitAt(it[0], 1, 0); return "split potions stacks=" + it.Count; });
steps.Add(null); steps.Add(null);
steps.Add(() => { int n = 0; foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") n++; return "potion stacks before=" + n; });
steps.Add(() => { btn("Btn1").onClick.Invoke(); return "click stack"; });
steps.Add(null); steps.Add(null);
steps.Add(() => { int n = 0, tot = 0; foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") { n++; tot += c.nowStack; } return "potion stacks after=" + n + " total=" + tot; });
steps.Add(() => { btn("Btn0").onClick.Invoke(); return "click sort"; });
steps.Add(() => { btn("Btn2").onClick.Invoke(); return "click filter -> " + InventoryFilterView.Current; });
steps.Add(() => { InventoryFilterView.Cycle(inv); InventoryFilterView.Cycle(inv); InventoryFilterView.Cycle(inv); return "filter back " + InventoryFilterView.Current; });
steps.Add(() => { var c = inv.CursorItem(); btn("Btn3").onClick.Invoke(); return "click lock item=" + (c != null ? c.name + " locked=" + c.itemData.isLocked : "null"); });
steps.Add(() => { var c = inv.CursorItem(); if (c != null) c.itemData.isLocked = false; int g0 = DatabaseManager.money; btn("Btn4").onClick.Invoke(); return "click bag (describe) money=" + g0 + " label=" + BagExpansion.ButtonLabel(); });
steps.Add(() => { foreach (var n in new[] { "Potion" }) { while (inv.CountItem(n) > 0) { inv.Service.Remove(n, inv.CountItem(n)); } } inv.inventory.SetActive(false); UiLock.Set("InvGroup", false); return "cleanup"; });
int idx = 0; double next = UnityEditor.EditorApplication.timeSinceStartup + 2;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < next) return; next = now + 0.15;
  if (idx >= steps.Count) { System.IO.File.WriteAllText("Logs/playtest/s3_tb.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
  var st = steps[idx++]; if (st == null) return; string r = null; try { r = st(); } catch (System.Exception ex) { r = "EX " + ex.Message + ex.StackTrace.Split('\n')[0]; } if (r != null) sb.AppendLine(r); };
UnityEditor.EditorApplication.update += upd; return "started";
