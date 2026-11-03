// PV1 (5) L5 방어구 레시피 30행 제작 UI 표시. phase 파일 Logs/pv1/recipes.phase: dump(기본) / open. 결과 Logs/pv1/recipes.log
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/recipes.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
CraftingManager cm = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<CraftingManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { L("CM " + c.name + " self=" + c.isSelfCraft + " weapon=" + c.isCraftWeapon + " boxes=" + c.LV1craftBox.Length + " p0=" + c.LV1craftBox[0].transform.childCount); if (!c.isSelfCraft && !c.isCraftWeapon) cm = c; }
if (cm == null) { return "no armor CM"; }
var csv = Resources.Load<TextAsset>(SelfCraftPages.ArmorResourcePath);
var recipes = SelfCraftPages.Parse(csv.text);
L("CSV recipes=" + recipes.Count);
var cells = new System.Collections.Generic.Dictionary<string, string>();
int total = 0, bad = 0;
for (int lv = 0; lv < cm.LV1craftBox.Length; lv++)
{
    var box = cm.LV1craftBox[lv].transform;
    for (int p = 0; p < box.childCount; p++)
    {
        var page = box.GetChild(p);
        for (int l = 0; l < page.childCount; l++)
        {
            var line = page.GetChild(l);
            for (int c = 0; c < line.childCount; c++)
            {
                var cell = line.GetChild(c); var chk = cell.GetComponent<CraftItemCheck>(); var need = cell.GetComponent<NeedItem>();
                total++;
                bool ok = chk != null && chk.itemData != null && need != null && need.needItem != null && need.needItem.Count > 0;
                if (!ok) { bad++; if (bad <= 40) L("  BROKEN lv" + lv + "/p" + p + "/l" + l + "/c" + c + " name=" + cell.name + " active=" + cell.gameObject.activeInHierarchy + " chk=" + (chk != null) + " itemData=" + (chk != null && chk.itemData != null) + " need=" + (need != null ? need.needItem.Count : -1)); }
                string key = cell.name;
                cells[key] = "lv" + lv + "/p" + p + " itemData=" + (chk != null && chk.itemData != null) + " img=" + (chk != null && chk.image != null && chk.image.sprite != null) + " need=" + (need != null ? string.Join("+", need.needItem.ToArray()) + "x" + string.Join("/", need.needItemAmount.ConvertAll(x => x.ToString()).ToArray()) : "-");
            }
        }
    }
}
L("UI cells total=" + total + " broken(itemData/need 없음)=" + bad);
int found = 0, missing = 0, noSprite = 0; var miss = new System.Text.StringBuilder(); var nspr = new System.Text.StringBuilder();
foreach (var r in recipes)
{
    if (cells.ContainsKey(r.Result)) { found++; if (cells[r.Result].Contains("img=False")) { noSprite++; nspr.Append(r.Result + " "); } }
    else { missing++; miss.Append(r.Result + " "); }
}
L("recipes in UI=" + found + " missing=" + missing + " [" + miss + "] noSprite=" + noSprite + " [" + nspr + "]");
foreach (var r in recipes) { if (cells.ContainsKey(r.Result)) L("  " + r.Result + " -> " + cells[r.Result]); }
return "ok total=" + total + " found=" + found + " missing=" + missing;
