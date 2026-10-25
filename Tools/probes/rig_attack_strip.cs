// 픽셀 리그 공격 확인: 무기 __WEAPON__ 장착 → 평타 → 0.05초 간격 8컷을 가로로 이어 Logs/playtest/rig_atk___WEAPON__.png 로 저장.
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("__WEAPON__"));
box.LoadPrefab("__WEAPON__", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var pc = PlayerController.instance;
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
int n = 8, w = 256;
var strip = new Texture2D(w * n, w, TextureFormat.RGB24, false);
System.Action<int> shot = (int i) =>
{
    var go = new GameObject("CloseupCam");
    var cam = go.AddComponent<Camera>();
    cam.orthographic = true; cam.orthographicSize = 1.8f;
    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
    Vector3 p = pc.transform.position;
    go.transform.position = new Vector3(p.x + 0.3f, p.y + 0.4f, p.z - 50f);
    var rt = new RenderTexture(w, w, 24); rt.filterMode = FilterMode.Point;
    cam.targetTexture = rt; cam.Render();
    RenderTexture.active = rt; strip.ReadPixels(new Rect(0, 0, w, w), i * w, 0); RenderTexture.active = null;
    cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt);
    if (i == n - 1)
    {
        strip.Apply();
        System.IO.File.WriteAllBytes("Logs/playtest/rig_atk___WEAPON__.png", strip.EncodeToPNG());
    }
};
DG.Tweening.DOVirtual.DelayedCall(0.15f, () => { if (amg.equipWeapon == null && Weapon.Equipped != null) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); } }, true);
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Weapon.Equipped.MeleeAttack(), true);
for (int k = 0; k < n; k++)
{
    int kk = k;
    DG.Tweening.DOVirtual.DelayedCall(0.3f + 0.02f + kk * 0.05f, () => shot(kk), true);
}
return "scheduled";
