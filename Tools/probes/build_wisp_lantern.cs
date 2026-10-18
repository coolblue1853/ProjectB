// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
// 계보 무기 3호 Wisp Lantern (TowerWorld 12.1): 도깨비불 → 지팡이. ScareStaff 복제.
// L Wisp Bolt = 유도 도깨비불 3발(HomingOrb), S Ghostlight Ring = 공전 도깨비불 3개(OrbitWisps, 6초), D 유지(이름만).
// 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/", F = DO + "WispLantern/";
System.IO.Directory.CreateDirectory(F);
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
AssetDatabase.ImportAsset("Assets/Resources/Item/WispLantern.png");
var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/WispLantern.png");
imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode; imp.SaveAndReimport();

// 보이지 않는 작은 피격 판정 (DaggerPowerSteb 복제, 스프라이트 숨김·판정 축소)
string touchPath = DO + "Common/OrbTouch.prefab"; // 계보 무기 공용 판정 (B-121: Wisp 전용 폴더에서 분리)
if (!AssetDatabase.IsValidFolder(DO + "Common")) AssetDatabase.CreateFolder("Assets/Resources/Weapon/DamageObject", "Common");
if (!System.IO.File.Exists(touchPath)) AssetDatabase.CopyAsset(DO + "DoubleDagger/DaggerPowerSteb.prefab", touchPath);
var touch = PrefabUtility.LoadPrefabContents(touchPath);
foreach (var r in touch.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(1f, 1f, 1f, 0f);
// 원본(DaggerPowerSteb) 콜라이더 기준으로 계산 — 재실행해도 공용 판정이 계속 줄지 않게. 없으면 새 GUID 가 생겨 오브 5종 참조가 끊기므로 move_orb_touch 로 옮긴 파일을 쓸 것
var srcCol = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerPowerSteb.prefab").GetComponent<BoxCollider2D>();
var tcol = touch.GetComponent<BoxCollider2D>(); tcol.size = new Vector2(srcCol.size.x * 0.4f, srcCol.size.y * 0.6f);
touch.GetComponent<DamageObject>().maxDamagedEnemies = 2;
PrefabUtility.SaveAsPrefabAsset(touch, touchPath); PrefabUtility.UnloadPrefabContents(touch);
var touchGo = AssetDatabase.LoadAssetAtPath<GameObject>(touchPath);

var idle = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/Wisp/Idle.png")) if (o is Sprite) idle.Add((Sprite)o);
idle.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
System.Func<string, GameObject> orbVisual = (name) => {
    var g = new GameObject(name);
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = idle[0]; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 2;
    var fb = g.AddComponent<SpriteFlipbook>(); fb.Frames = idle.ToArray();
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.1f; fso.ApplyModifiedPropertiesWithoutUndo();
    var lg = new GameObject("Glow"); lg.transform.SetParent(g.transform, false);
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>(); l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; l.color = new Color(0.45f, 0.75f, 1f); l.intensity = 1f; l.pointLightOuterRadius = 1.6f;
    g.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
    return g; };

// WispOrb (유도 탄)
var orb = orbVisual("WispOrb");
var ho = orb.AddComponent<HomingOrb>(); var hso = new SerializedObject(ho); hso.FindProperty("_hitPrefab").objectReferenceValue = touchGo; hso.ApplyModifiedPropertiesWithoutUndo();
var orbPrefab = PrefabUtility.SaveAsPrefabAsset(orb, F + "WispOrb.prefab"); UnityEngine.Object.DestroyImmediate(orb);

// WispRing (공전) — 자식 0번이 불 모양 원본
var ring = new GameObject("WispRing");
var ringOrb = orbVisual("Orb"); ringOrb.transform.SetParent(ring.transform, false);
var ow = ring.AddComponent<OrbitWisps>(); var oso = new SerializedObject(ow); oso.FindProperty("_hitPrefab").objectReferenceValue = touchGo; oso.ApplyModifiedPropertiesWithoutUndo();
var ringPrefab = PrefabUtility.SaveAsPrefabAsset(ring, F + "WispRing.prefab"); UnityEngine.Object.DestroyImmediate(ring);

// 시전 프리팹 = SummonSacre 복제 (스킬 경로가 DamageObject 를 요구하므로)
System.Func<string, GameObject, int, GameObject> cast = (name, obj, count) => {
    string p = F + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "ScareStaff/SummonSacre.prefab", p);
    var r = PrefabUtility.LoadPrefabContents(p);
    var ss = r.GetComponent<SummonSkill>(); ss.summonObject = obj; ss.summonCount = count; ss.summonDistance = 0.3f; ss.yPos = 0.4f;
    var s0 = r.GetComponent<SpriteRenderer>(); if (s0 != null) s0.color = new Color(1f, 1f, 1f, 0f);
    PrefabUtility.SaveAsPrefabAsset(r, p); PrefabUtility.UnloadPrefabContents(r);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };
var boltCast = cast("WispBoltCast", orbPrefab, 3);
var ringCast = cast("WispRingCast", ringPrefab, 1);

// 무기
string path = W + "WispLantern.prefab";
if (!System.IO.File.Exists(path)) AssetDatabase.CopyAsset(W + "ScareStaff.prefab", path);
var root = PrefabUtility.LoadPrefabContents(path);
root.name = "WispLantern";
var w = root.GetComponent<Weapon>();
w.minDmg = 11; w.maxDmg = 19; w.skillDmg = 15; w.energyColor = new Color(0.45f, 0.75f, 1f);
var skills = root.GetComponentsInChildren<Skill>(true);
skills[0].skillName = "Wisp Bolt"; skills[0].skillprefab = new GameObject[]{ boltCast }; skills[0].SkillCoolTime = 1.2f; skills[0].skillWaitTime = 0.2f;
skills[1].skillName = "Ghostlight Ring"; skills[1].skillprefab = new GameObject[]{ ringCast }; skills[1].SkillCoolTime = 10f;
if (skills.Length > 2) skills[2].skillName = "Wailing Light";
skills[0].skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Wisp Ember.png");
skills[1].skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/WispLantern.png");
foreach (var g in new[]{ w.RightWeqponSprite, w.leftWeqponSprite })
    if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(0.7f, 0.85f, 1f);
PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);

// 도깨비불 드롭 2%
var map = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    if (!(dm.name.StartsWith("RatThief") || dm.name.StartsWith("ScareCrow"))) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems");
    bool has = false; for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue == "WispLantern") has = true;
    if (has) continue;
    arr.arraySize++; var e = arr.GetArrayElementAtIndex(arr.arraySize - 1);
    e.FindPropertyRelative("itemName").stringValue = "WispLantern"; e.FindPropertyRelative("dropProbability").floatValue = 0.02f; e.FindPropertyRelative("offsetY").floatValue = 0.5f;
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return "built; skills=" + skills.Length + " drops+" + nd;
