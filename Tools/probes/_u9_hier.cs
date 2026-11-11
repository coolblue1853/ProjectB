string s = "";
foreach (var e in EnemyHealth.Active)
{
    if (e.name.StartsWith("BanditArcher"))
    {
        var r = e.hpBar.hpBarRect.transform; string p = "";
        for (var t = r; t != null; t = t.parent) { p = t.name + "(" + (t.gameObject.activeSelf ? "on" : "off") + ")/" + p; }
        s += p + "\nhpCanvas=" + (e.hpCanvas != null ? e.hpCanvas.name + " parentOf=" + (e.hpCanvas.transform.parent != null ? e.hpCanvas.transform.parent.name : "root") : "null");
        var cv = e.hpCanvas != null ? e.hpCanvas.GetComponentInParent<Canvas>(true) : null;
        s += " canvasMode=" + (cv != null ? cv.renderMode.ToString() : "-") + " lv=" + e.Level;
        var lbl = e.hpBar.hpBarRect.parent.Find("LvLabel");
        s += " lblExists=" + (lbl != null) + (lbl != null ? " lblOn=" + lbl.gameObject.activeSelf + " inH=" + lbl.gameObject.activeInHierarchy : "");
    }
}
return s;
