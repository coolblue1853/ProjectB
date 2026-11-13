var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (s.gameObject.activeInHierarchy && s.name.StartsWith("Rabbit3")) { pc.transform.position = new UnityEngine.Vector3(s.transform.position.x, s.transform.position.y + 1f, pc.transform.position.z); return "tp " + s.transform.position + " parent=" + s.transform.root.name + "/" + s.transform.parent.name; }
return "none";
