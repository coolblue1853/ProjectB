var d = new System.Collections.Generic.Dictionary<string,int>(); int tot = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { if (!e.gameObject.activeInHierarchy) continue; tot++; string k = e.MonsterId + (e.IsElite ? "(E)" : ""); d[k] = d.ContainsKey(k) ? d[k] + 1 : 1; }
var sb = new System.Text.StringBuilder("map=" + "?" + " day=" + TimeChange.instance.isDaytime + " lock=" + TimeChange.instance.LockDaytime + " tot=" + tot + " | ");
foreach (var kv in d) sb.Append(kv.Key + "=" + kv.Value + " ");
return sb.ToString();
