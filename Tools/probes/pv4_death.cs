var phm = PlayerHealthManager.Instance;
var m = typeof(PlayerHealthManager).GetMethod("Die", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
string o = "";
for (int i = 0; i < 2; i++) { m.Invoke(phm, null); o += "death" + i + " visible=" + EventBanner.DeathVisible + " text=" + EventBanner.DeathText.Replace("\n"," / ") + "\n"; }
ScreenCapture.CaptureScreenshot("Logs/pv4/death.png");
return o;
