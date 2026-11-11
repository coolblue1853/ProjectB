string s = "";
foreach (var e in EnemyHealth.Active)
{
    if (e.name.StartsWith("BanditArcher"))
    {
        var cv = e.hpBar.hpBarRect.parent.GetComponent<Canvas>();
        s += "canvas=" + (cv != null) + (cv != null ? " mode=" + cv.renderMode + " cam=" + (cv.worldCamera != null ? cv.worldCamera.name : "null") + " rootCanvas=" + cv.rootCanvas.renderMode : "");
        s += " barIsHpCanvas=" + (e.hpBar.hpBarRect.gameObject == e.hpCanvas) + " barParent=" + e.hpBar.hpBarRect.parent.name;
        s += " hpBarOn=" + e.hpBar.hpBarRect.gameObject.activeInHierarchy;
    }
}
return s + " mainCam=" + (Camera.main != null ? Camera.main.name : "null");
