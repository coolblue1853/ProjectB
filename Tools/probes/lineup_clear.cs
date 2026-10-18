int n=0; foreach (var g in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("LINEUP_")) { UnityEngine.Object.Destroy(g); n++; } return "cleared " + n;
