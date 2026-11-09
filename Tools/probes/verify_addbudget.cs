var sb = new System.Text.StringBuilder();
Vector3 at = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
int ok = 0;
for (int i = 0; i < 9; i++)
{
    if (Vfx.PlayThemed(VfxId.WSmearDown, at + Vector3.right * i * 0.1f, false, 1f, PixelRampId.Steel, false, true, 1f, true)) ok++;
}
var auras = UnityEngine.Object.FindObjectsByType<SmearAura>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
int glowOn = 0;
foreach (var a in auras) { var g = a.transform.Find("Glow"); if (g != null && g.GetComponent<SpriteRenderer>().enabled) glowOn++; }
return "played=" + ok + " active=" + AdditiveBudget.Active + " auras=" + auras.Length + " glowOn=" + glowOn;
