var dm = UnityEngine.Object.FindFirstObjectByType<DropManager>(FindObjectsInactive.Include);
var pos = PlayerController.instance.transform.position;
var h = new System.Collections.Generic.SortedDictionary<int,int>();
for (int i=0;i<1000;i++){ int v=dm.DropGold("Rabbit_BD", false, false, pos); h[v]=(h.ContainsKey(v)?h[v]:0)+1; }
var sb = new System.Text.StringBuilder(); foreach (var kv in h) sb.Append(kv.Key+":"+kv.Value+" "); return sb.ToString();
