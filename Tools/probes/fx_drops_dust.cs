// 플레이어 옆에 R/U/C 드롭 3개를 놓고(빛 기둥 확인), 대시 먼지·착지 먼지를 띄운 뒤 시간 정지 → 스크린샷용
var pc = PlayerController.instance;
var dm = UnityEngine.Object.FindFirstObjectByType<DropManager>();
if (dm == null) return "no DropManager";
var names = new[]{ "LeafCap", "RatCheifDagger", "Wood" };
var sb = new System.Text.StringBuilder();
for (int i = 0; i < names.Length; i++) {
  var go = dm.GetGo(names[i]); if (go == null) { sb.Append("null "); continue; }
  go.transform.position = pc.transform.position + new Vector3(2f + i * 1.6f, 0.5f, 0f);
  go.GetComponent<DropItemCheck>().SetItem(names[i]);
  var p = go.GetComponentInChildren<DropLightPillar>(true);
  sb.Append(names[i] + " pillar=" + (p != null && p.gameObject.activeInHierarchy) + "; ");
}
var feet = pc.groundCheck.transform.position;
DustPuffs.Dash(feet + Vector3.left * 2.5f, 1f);
DustPuffs.Land(feet + Vector3.left * 5f, 1f);
DG.Tweening.DOVirtual.DelayedCall(1.2f, () => { DustPuffs.Dash(pc.groundCheck.transform.position + Vector3.left * 2.5f, 1f); DustPuffs.Land(pc.groundCheck.transform.position + Vector3.left * 5f, 1f); }, false);
DG.Tweening.DOVirtual.DelayedCall(1.32f, () => Time.timeScale = 0f, false);
return sb.ToString();
