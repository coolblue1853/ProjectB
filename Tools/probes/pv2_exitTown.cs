var pc = PlayerController.instance; Portal pick=null;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (p.name=="PotalTown") pick=p;
var act = new System.Text.StringBuilder(); foreach (var m in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) act.Append(m.name+" ");
if (pick==null) return "no PotalTown; pos=" + pc.transform.position + " maps=" + act;
return "use=" + pick.Use(pc.gameObject) + " dest=" + pick.destination.position.ToString("F1") + " maps=" + act + " townHint=" + FirstHints.CurrentText;
