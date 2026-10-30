// 제작 핀 검증(BasicFeatures.md 7절, 플레이 중). __PHASE__:
//  check = 핀 상태·HUD 글자 보고 (K 로 제작 창 → P 로 고정한 뒤)
//  fill  = 핀 재료를 전부 지급(다 모였을 때 알림 확인용) 후 보고
//  clear = 핀 해제(같은 결과로 Toggle)
string phase = "__PHASE__";
var pin = CraftPinHud.Pin;
if (pin == null) return "no hud";
var go = UnityEngine.GameObject.Find("[CraftPinHud]");
var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
if (phase == "fill")
{
    for (int i = 0; i < pin.Count; i++)
    {
        int need = pin.NeedAt(i) - InventoryManager.instance.CountItem(pin.IdAt(i));
        for (int k = 0; k < need; k++) InventoryManager.instance.CreatItem(pin.IdAt(i));
    }
    InventoryManager.instance.RebuildItemStackCache();
}
if (phase == "clear")
{
    CraftPinHud.Toggle(pin.Result, null, null);
}
bool chip = text != null && text.transform.parent.gameObject.activeInHierarchy;
string saved = ES3.KeyExists(CraftPinHud.SaveKey) ? ES3.Load<string>(CraftPinHud.SaveKey) : "(none)";
return "result=" + pin.Result + " count=" + pin.Count + " chipVisible=" + chip + " saved=" + saved + " text=" + (text != null ? text.text.Replace("\n", " / ") : "");
