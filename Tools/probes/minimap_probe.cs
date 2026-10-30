// 미니맵 검증(BasicFeatures.md 9절, 플레이 중): 영역·지형 텍스처·점 수·플레이어 점 위치 보고
var go = UnityEngine.GameObject.Find("[MinimapHud]");
if (go == null) return "no minimap";
var raw = go.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
var tex = raw != null ? raw.texture as UnityEngine.Texture2D : null;
int solid = 0;
if (tex != null) { foreach (var c in tex.GetPixels32()) if (c.a > 0) solid++; }
var dots = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
int dotCount = 0; foreach (var d in dots) if (d.name == "Dot" && d.gameObject.activeSelf) dotCount++;
var title = go.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
return "visible=" + go.transform.GetChild(0).gameObject.activeSelf + " area=" + MinimapHud.Area + " tex=" + (tex != null ? tex.width + "x" + tex.height : "none") + " solidPx=" + solid + " dots=" + dotCount + " title=" + (title != null ? title.text : "") + " pinOffset=" + MinimapHud.OccupiedHeight;
