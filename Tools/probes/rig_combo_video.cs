// 기본 칼 영상(MotionSet §7): 녹슨 검 장착 → 대기 0.4초 → 평타 3타(후딜이 풀릴 때마다 입력) → 피격(2.8초) → 단타 찌르기 후 멈춤(4.0초, 회복 셀) → 대기.
// 플레이어 근접 카메라로 1/30초마다 찍어 Logs/playtest/vid/f_000.png … 로 저장. ffmpeg 로 묶는다.
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var pc = PlayerController.instance;
var rig = pc.GetComponentInChildren<PixelRig>();
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
pc.transform.position += new Vector3(4f, 0f, 0f); // 시작 지점 포털에서 비켜 선다
System.IO.Directory.CreateDirectory("Logs/playtest/vid");
int w = 320, h = 240, total = 165;
int hits = 0;
System.Action<int> shot = (int i) =>
{
    var go = new GameObject("VidCam");
    var cam = go.AddComponent<Camera>();
    cam.orthographic = true; cam.orthographicSize = 1.5f;
    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.6f, 0.65f, 1f);
    Vector3 p = pc.transform.position;
    go.transform.position = new Vector3(p.x + 0.4f, p.y + 0.5f, p.z - 50f);
    var rt = new RenderTexture(w, h, 24); rt.filterMode = FilterMode.Point;
    cam.targetTexture = rt; cam.Render();
    var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
    RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); RenderTexture.active = null;
    System.IO.File.WriteAllBytes($"Logs/playtest/vid/f_{i:000}.png", tex.EncodeToPNG());
    cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
};
DG.Tweening.DOVirtual.DelayedCall(0.15f, () => { if (amg.equipWeapon == null && Weapon.Equipped != null) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); } }, true);
// 0.4초부터 0.05초마다 평타 가능 여부를 보고 3번까지 누른다
for (int k = 0; k < 40; k++)
{
    DG.Tweening.DOVirtual.DelayedCall(0.4f + k * 0.05f, () => { if (hits < 3 && Weapon.Equipped != null && Weapon.Equipped.CanMeleeNow) { Weapon.Equipped.MeleeAttack(); hits++; } }, true);
}
DG.Tweening.DOVirtual.DelayedCall(2.8f, () => rig.SetControlParamInt("Eye Param", 4), true);
// 콤보 없이 한 번만 찌르고 멈춤 → 회복 셀(StabEnd) 확인
DG.Tweening.DOVirtual.DelayedCall(4.0f, () => { if (Weapon.Equipped != null && Weapon.Equipped.CanMeleeNow) Weapon.Equipped.MeleeAttack(); }, true);
for (int k = 0; k < total; k++)
{
    int kk = k;
    DG.Tweening.DOVirtual.DelayedCall(0.3f + kk / 30f, () => shot(kk), true);
}
return "scheduled";
