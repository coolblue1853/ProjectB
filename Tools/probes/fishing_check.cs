// fishing_drive.cs 진행 기록 읽기 + 지금 상태(단계·장력·잠금·인벤 수량)
var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[FishProbe]")) sb.Append(g.name + "\n");
var spot = FishingSpot.Current;
var s = spot != null ? spot.Session : null;
sb.Append("now: " + (s == null ? "idle" : s.Phase + "(" + s.Result + ")" + (s.Tension != null ? " tension " + s.Tension.Tension.ToString("F0") + " safe " + s.Tension.SafeMin + "-" + s.Tension.SafeMax + " prog " + s.Tension.Progress.ToString("F0") : "")));
sb.Append(" lock=" + UiLock.IsHeld(FishingSpot.LockOwner) + " isOpenUI=" + DatabaseManager.isOpenUI);
var inv = InventoryManager.instance;
foreach (var id in new[] { "Forest Carp", "Moss Loach", "Amber Trout", "Spirit Koi" }) sb.Append(" " + id + "=" + inv.CountItem(id));
var prompt = UnityEngine.GameObject.Find("FishPrompt");
sb.Append(" promptVisible=" + (prompt != null && prompt.activeInHierarchy));
return sb.ToString();
