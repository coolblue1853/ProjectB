// 보스 근처 귀환 주문서 거부 안내 문구 확인
var pc = PlayerController.instance; float best = 9999; foreach (var e in EnemyHealth.Active) if (e.isBossMob) best = Mathf.Min(best, Vector2.Distance(e.transform.position, pc.transform.position));
bool b = RecallCast.Begin("귀환 주문서"); string found = "";
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (t.text != null && t.text.Contains("보스가 가까워")) found += t.name + "(active=" + t.gameObject.activeInHierarchy + ",a=" + t.canvasRenderer.GetAlpha().ToString("F1") + "):" + t.text + " ";
return "bossDist=" + best + " Begin=" + b + " casting=" + RecallCast.IsCasting + " ui=" + found;
