var pc = PlayerController.instance; var sb = new System.Text.StringBuilder(); Portal pick=null; float best=999;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { float d=Vector2.Distance(p.transform.position, pc.transform.position); sb.AppendLine(p.name+" d="+d.ToString("F0")+" -> "+(p.openMap?p.openMap.name:"-")); if (d<best && p.destination!=null){best=d;pick=p;} }
sb.AppendLine("use " + pick.name + "=" + pick.Use(pc.gameObject));
return sb.ToString();
