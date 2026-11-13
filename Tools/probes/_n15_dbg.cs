var sb = new System.Text.StringBuilder();
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
sb.Append("pos "+pc.transform.position+" ");
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.name.Contains("Rat")||s.name.Contains("Rabbit")) sb.Append(s.name+" act="+s.gameObject.activeInHierarchy+" en="+s.enabled+" day="+s.isDaytimeSpowner+" @"+s.transform.position+"; ");
return sb.ToString();
