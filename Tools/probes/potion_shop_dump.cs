// 상점 LV1craftBox 구조 덤프(읽기 전용)
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
if (shop == null) return "no ShopManager";
var sb = new System.Text.StringBuilder();
for (int b = 0; b < shop.LV1craftBox.Length; b++)
{
    var box = shop.LV1craftBox[b].transform;
    sb.AppendLine("box" + b + " " + box.name + " pages=" + box.childCount);
    for (int p = 0; p < box.childCount; p++)
    {
        var page = box.GetChild(p);
        sb.AppendLine(" page" + p + " " + page.name + " lines=" + page.childCount);
        for (int l = 0; l < page.childCount; l++)
        {
            var line = page.GetChild(l);
            var names = new System.Collections.Generic.List<string>();
            for (int c = 0; c < line.childCount; c++) names.Add(line.GetChild(c).name);
            sb.AppendLine("  line" + l + " " + line.name + ": " + string.Join(",", names));
        }
    }
}
return sb.ToString();
