// 봉인 토스트 표시 검증(플레이): 첫 PortalSeal 의 NotifySealed → 0.8초 뒤 PickupLog 줄 텍스트가 칸 안에 들어가는지(렌더 폭 ≤ 칸 폭, 글자 크기) 기록 + 스크린샷 Logs/playtest/portal_seal_toast.png. 결과 pb_seal
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_seal", log);
var ps = UnityEngine.Object.FindFirstObjectByType<PortalSeal>(FindObjectsInactive.Include);
if (ps == null) { return "no seal"; }
ps.NotifySealed();
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => {
    var root = GameObject.Find("[PickupLog]");
    if (root == null) { log.Append("no PickupLog; DONE"); return; }
    foreach (var t in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(false))
    {
        if (t.text.IndexOf("sealed", System.StringComparison.OrdinalIgnoreCase) < 0) { continue; }
        t.ForceMeshUpdate();
        log.Append("text='" + t.GetParsedText() + "' size=" + t.fontSize.ToString("F1") + " render=" + t.renderedWidth.ToString("F0") + " box=" + t.rectTransform.rect.width.ToString("F0") + " truncated=" + t.isTextTruncated + "; ");
    }
    ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/portal_seal_toast.png"));
    log.Append("DONE");
}, false);
return "notified";
