var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var sys = UnityEngine.Object.FindFirstObjectByType<ArtifactSystem>();
ArtifactSystem.Unlock(ArtifactId.StarBurst);
typeof(ArtifactSystem).GetField("equipped", BF).SetValue(sys, ArtifactId.StarBurst);
typeof(ArtifactSystem).GetField("readyAt", BF).SetValue(sys, 0f);
var pc = PlayerController.instance;
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/Scarecrow_BD.prefab");
var sb = new System.Text.StringBuilder();
var list = new System.Collections.Generic.List<EnemyHealth>();
foreach (float dx in new float[]{2f, 3.5f}) {
  float gy = GroundProbe.Y(pc.transform.position + Vector3.right * dx);
  var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + dx, gy + 0.6f, 0f), Quaternion.identity);
  var e = go.GetComponent<EnemyHealth>(); e.maxHP = 100000; e.nowHP = 100000; list.Add(e);
}
bool used = ArtifactSystem.TryUseNow();
sb.Append("used=" + used + "; ");
foreach (var e in list) sb.Append(e.name + " dist=" + Vector2.Distance(e.transform.position, pc.transform.position).ToString("0.0") + " hp=" + e.nowHP + "; ");
foreach (var e in list) UnityEngine.Object.Destroy(e.gameObject);
return sb.ToString();
