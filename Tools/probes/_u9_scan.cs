// BUG-U9 계측: 활성 LvLabel 중 주인(EnemyHealth)이 꺼졌거나 죽었거나 hpCanvas 가 꺼진 것을 센다
var sb = new System.Text.StringBuilder();
int total = 0, shown = 0, orphan = 0;
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
{
    if (t.gameObject.name != "LvLabel") { continue; }
    total++;
    if (!t.gameObject.activeInHierarchy) { continue; }
    shown++;
    var eh = t.GetComponentInParent<EnemyHealth>(true);
    var bar = t.GetComponentInParent<EnemyHealthBar>(true);
    bool ownerOk = eh != null && eh.isActiveAndEnabled && eh.nowHP > 0;
    if (!ownerOk)
    {
        orphan++;
        sb.Append("ORPHAN ").Append(t.transform.root.name).Append("/").Append(t.transform.parent != null ? t.transform.parent.name : "-")
          .Append(" eh=").Append(eh != null ? eh.name + (eh.isActiveAndEnabled ? "(on)" : "(off)") + " hp=" + eh.nowHP : "null")
          .Append(" bar=").Append(bar != null).Append(" pos=").Append(t.transform.position).Append("\n");
    }
}
return "labels total=" + total + " shown=" + shown + " orphan=" + orphan + "\n" + sb;
