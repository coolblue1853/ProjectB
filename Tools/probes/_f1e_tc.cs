var tc = UnityEngine.Object.FindFirstObjectByType<TimeChange>(FindObjectsInactive.Include);
return tc == null ? "none" : "total=" + tc.totalTime + " start=" + tc.startNighttime + " end=" + tc.endNighttime + " period=" + tc.period + " lock=" + tc.LockDaytime;
