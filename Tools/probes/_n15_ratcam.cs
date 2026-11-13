var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (e.gameObject.activeInHierarchy && e.MonsterId.StartsWith("RatThief")) { pc.transform.position = new UnityEngine.Vector3(e.transform.position.x - 4f, e.transform.position.y + 0.5f, pc.transform.position.z); return "near " + e.MonsterId + " day=" + TimeChange.instance.isDaytime; }
return "no rat";
