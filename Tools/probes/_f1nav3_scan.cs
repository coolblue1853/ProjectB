// 늪·성채 지면 연속성 스캔: 1m 간격 아래 레이, 틈(지면 없음)과 양끝 벽 여부 → Logs/playtest/f1nav3/scan.txt
int mask = LayerMask.GetMask("Ground");
var sb = new System.Text.StringBuilder();
string[] maps = new string[] { "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_DeepForest", "Lv1_TowerOutskirts" };
foreach (var mn in maps) {
    MapInfo.GoToMap(mn, PlayerController.instance.gameObject);
    var pp = PlayerController.instance.transform.position;
    float minX = 1e9f, maxX = -1e9f, minY = 1e9f, maxY = -1e9f;
    foreach (var c in GameObject.Find("Map").transform.Find(mn).GetComponentsInChildren<Collider2D>()) { if (c.gameObject.layer != LayerMask.NameToLayer("Ground")) continue; var b = c.bounds; minX = Mathf.Min(minX, b.min.x); maxX = Mathf.Max(maxX, b.max.x); minY = Mathf.Min(minY, b.min.y); maxY = Mathf.Max(maxY, b.max.y); }
    sb.AppendLine("== " + mn + " x[" + minX.ToString("F1") + "," + maxX.ToString("F1") + "] y[" + minY.ToString("F1") + "," + maxY.ToString("F1") + "] start=" + pp.ToString("F2"));
    float top = maxY + 3f; string gaps = ""; int run = 0; float gs = 0;
    for (float x = minX; x <= maxX; x += 0.5f) {
        var h = Physics2D.Raycast(new Vector2(x, top), Vector2.down, top - minY + 5f, mask);
        if (!h.collider) { if (run == 0) gs = x; run++; } else { if (run > 0) gaps += "[" + gs.ToString("F1") + ".." + (x - 0.5f).ToString("F1") + "] "; run = 0; }
    }
    if (run > 0) gaps += "[" + gs.ToString("F1") + "..end] ";
    sb.AppendLine("  no-ground x ranges: " + (gaps == "" ? "none" : gaps));
    // 가장 낮은 지면 위 높이에서 양끝을 향한 수평 레이(벽 여부)
    foreach (var side in new float[] { -1f, 1f }) {
        float sx = side < 0 ? minX + 1f : maxX - 1f;
        var h = Physics2D.Raycast(new Vector2(sx, top), Vector2.down, top - minY + 5f, mask);
        if (h.collider) { var w = Physics2D.Raycast(new Vector2(sx, h.point.y + 1f), new Vector2(side, 0), 3f, mask); sb.AppendLine("  edge " + (side < 0 ? "L" : "R") + " x=" + sx.ToString("F1") + " groundY=" + h.point.y.ToString("F2") + " wall=" + (w.collider ? w.collider.name + "@" + w.point.x.ToString("F1") : "NONE")); }
        else sb.AppendLine("  edge " + (side < 0 ? "L" : "R") + " no ground at " + sx);
    }
}
System.IO.File.WriteAllText("Logs/playtest/f1nav3/scan.txt", sb.ToString());
return sb.ToString();
