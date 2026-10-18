// 처치 후 드롭/시체/스포너 슬롯 상태
var sb = new System.Text.StringBuilder();
sb.Append("drops=" + UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude).Length);
sb.Append(" bodies=" + UnityEngine.Object.FindObjectsByType<DeadBody>(FindObjectsInactive.Exclude).Length);
sb.AppendLine(" enemies=" + UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude).Length);
var f = typeof(EnemySpawner).GetField("enemySlot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude))
{
    var slots = f.GetValue(sp) as GameObject[];
    if (slots == null) continue;
    int filled = 0, inactive = 0;
    foreach (var s in slots) { if (s != null) { filled++; if (!s.activeInHierarchy) inactive++; } }
    if (filled > 0) sb.AppendLine(sp.name + " slots=" + slots.Length + " filled=" + filled + " filledButInactive=" + inactive);
}
return sb.ToString();
