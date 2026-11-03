foreach (var s in UnityEngine.Object.FindObjectsByType<MiniBossSite>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None)) {
  if (s.Alive) { s.Enemy.ReleaseEnemy(true, true); return "released " + s.name; }
}
return "none";
