var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
foreach (var z in UnityEngine.Object.FindObjectsByType<ZoneReportSign>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { pc.transform.position = new UnityEngine.Vector3(z.transform.position.x, z.transform.position.y + 0.5f, pc.transform.position.z); return "near sign " + z.transform.position + " in " + z.transform.parent.name; }
return "NO ACTIVE SIGN";
