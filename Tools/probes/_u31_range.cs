// U31: 씬 NPC 트리거 범위 전수(TalkNpc·NpcQuestGiver·FastTravelNpc) -> Logs/playtest/u31.txt
var sb = new System.Text.StringBuilder();
foreach (var mb in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(UnityEngine.FindObjectsInactive.Include)) {
    string n = mb.GetType().Name; if (n != "TalkNpc" && n != "NpcQuestGiver" && n != "FastTravelNpc") continue;
    foreach (var c in mb.GetComponents<Collider2D>()) sb.AppendLine(n + " " + mb.gameObject.name + " trig=" + c.isTrigger + " w=" + (c.bounds.size.x).ToString("0.00") + " h=" + (c.bounds.size.y).ToString("0.00") + " scene=" + mb.gameObject.scene.name);
}
System.IO.Directory.CreateDirectory("Logs/playtest"); System.IO.File.WriteAllText("Logs/playtest/u31.txt", sb.ToString()); return "lines=" + sb.Length;
