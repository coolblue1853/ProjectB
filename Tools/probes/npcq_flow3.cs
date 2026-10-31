var sb = new System.Text.StringBuilder();
LevelSystem.SetState(1, 0);
System.Threading.Thread.Sleep(0);
var g = GameObject.Find("NPC_Elder").GetComponent<NpcQuestGiver>();
NpcQuestLog.Refresh();
sb.AppendLine("Lv1 elder mark=" + g.MarkState);
LevelSystem.SetState(3, 0);
NpcQuestLog.Refresh();
var dm = UnityEngine.Object.FindObjectsByType<DropManager>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)[0];

System.Func<int> Count = () => { int n = 0; foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(UnityEngine.FindObjectsSortMode.None)) if (d.gameObject.activeInHierarchy && d.name.Contains("StarBark")) n++; return n; };
var pos = PlayerController.instance.transform.position;
for (int i = 0; i < 60; i++) dm.DropExtra("ForestWolf_BD", pos);
sb.AppendLine("inactive quest: StarBark drops in world=" + Count());
NpcQuestLog.Accept("Q201");
sb.AppendLine("Q201 active wants=" + NpcQuestLog.WantsDrop("StarBarkFragment"));
for (int i = 0; i < 60; i++) dm.DropExtra("ForestWolf_BD", pos);
sb.AppendLine("active quest: StarBark drops in world=" + Count());
return sb.ToString();
