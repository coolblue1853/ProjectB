Portal pick=null;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (p.openMap!=null && p.openMap.name=="Town") pick=p;
var core = GameObject.Find("Lv1_DeepForestCore");
if (core!=null) core.SetActive(false);
pick.Use(PlayerController.instance.gameObject);
return "town portal " + pick.name + " close=" + pick.closeMap.name;
