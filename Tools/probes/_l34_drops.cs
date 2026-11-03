var d = new System.Collections.Generic.Dictionary<string,int>();
foreach (var c in UnityEngine.Object.FindObjectsByType<DropItemCheck>(UnityEngine.FindObjectsSortMode.None)) { if (!c.gameObject.activeInHierarchy) continue; string k = c.name; d[k] = (d.ContainsKey(k) ? d[k] : 0) + 1; }
var sb = new System.Text.StringBuilder(); foreach (var kv in d) sb.Append(kv.Key + "x" + kv.Value + " ");
return sb.ToString();
