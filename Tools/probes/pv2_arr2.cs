var pc = PlayerController.instance; var sb = new System.Text.StringBuilder();
Portal pick=null;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (p.name=="Portal_CoreToDeep") pick=p;
sb.AppendLine("use=" + pick.Use(pc.gameObject) + " dest=" + pick.destination.position.ToString("F1") + " zones=" + ArrivalSafeZone.Points.Count);
return sb.ToString();
