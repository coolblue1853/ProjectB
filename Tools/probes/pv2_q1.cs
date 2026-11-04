var o = new System.Text.StringBuilder();
string orig = NpcQuestLog.Serialize();
System.IO.File.WriteAllText("Logs/pv2/quest_orig.txt", orig);
NpcQuestLog.LevelGetter = () => 99;
var done = new[]{"Q101","Q102"};
var sb = new System.Text.StringBuilder(orig);
foreach (var d in done) if (!NpcQuestLog.IsDone(d)) { if (sb.Length>0) sb.Append(';'); sb.Append(d+":2:"+NpcQuestCatalog.Find(d).Count); }
NpcQuestLog.Deserialize(sb.ToString());
NpcQuestGiver ruen = null;
foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (g.NpcId=="ruen") ruen = g;
if (ruen==null) return "no ruen";
var pc = PlayerController.instance; pc.transform.position = ruen.transform.position + Vector3.up*0.5f;
NpcQuestStatus s = NpcQuestLog.BestFor("ruen", out var q);
return "ruen at " + ruen.transform.position + " best=" + s + " " + (q!=null?q.Id:"-") + " mark=" + ruen.MarkState + " orig=" + orig;
