// Phase 10a: 장비 인스턴스 출처 모디파이어 — 착용 → 덤프 → 해제 → 모든 정수 스탯 0 확인 (플레이 모드)
var box = UnityEngine.Object.FindFirstObjectByType<EquipBoxCheck>(FindObjectsInactive.Include);
var sb = new System.Text.StringBuilder("before: " + PlayerStats.Dump() + "\n");
var names = new[] { "Head/CliffCap", "Chest/CliffArmor", "Leg/CliffLegArmor" };
var eqs = new System.Collections.Generic.List<Equipment>();
foreach (var n in names) { var go = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(n)); go.SetActive(false); var e = go.GetComponent<Equipment>(); e.enhanceLevel = 3; box.SetEquipment(e); eqs.Add(e); }
box.SetEquipment(eqs[0]); // 같은 장비 재적용 → 중복되지 않아야 함(정수 스탯)
sb.Append("equipped: " + PlayerStats.Dump() + "\n");
foreach (var e in eqs) { box.DisableEquipment(e); UnityEngine.Object.Destroy(e.gameObject); }
sb.Append("after: " + PlayerStats.Dump());
return sb.ToString();
