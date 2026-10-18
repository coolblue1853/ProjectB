// 계보 무기 1호 Fang Dagger (TowerWorld 12.1): 늑대 → 쌍단검 + F 스킬 "Howl of the Pack"(영혼 늑대 2마리가 돌진하며 물기).
// 아이콘 임포트, WolfSpirit/SummonWolf/FangDagger 프리팹, 일반 숲 드롭 테이블. 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
// 1) 아이콘
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "FangDagger", "Wolf Fang", "Boar Tusk", "Wisp Ember", "Snail Shell" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// 2) WolfSpirit (늑대 Run 프레임 + 파란 영혼 색 + SummonRunner)
System.IO.Directory.CreateDirectory(DO + "FangDagger");
var runFrames = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/ForestWolf/Run.png")) if (o is Sprite) runFrames.Add((Sprite)o);
runFrames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
var spirit = new GameObject("WolfSpirit");
var sr = spirit.AddComponent<SpriteRenderer>(); sr.sprite = runFrames[0]; sr.color = new Color(0.55f, 0.85f, 1f, 0.85f);
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 1;
var fb = spirit.AddComponent<SpriteFlipbook>(); fb.Frames = runFrames.ToArray();
var fbSo = new SerializedObject(fb); fbSo.FindProperty("_frameTime").floatValue = 0.07f; fbSo.ApplyModifiedPropertiesWithoutUndo();
var runner = spirit.AddComponent<SummonRunner>();
var rSo = new SerializedObject(runner); rSo.FindProperty("_speed").floatValue = 5f; rSo.FindProperty("_life").floatValue = 1.8f; rSo.FindProperty("_attackPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerPowerSteb.prefab"); rSo.ApplyModifiedPropertiesWithoutUndo();
var glow = new GameObject("Glow"); glow.transform.SetParent(spirit.transform, false);
var l = glow.AddComponent<UnityEngine.Rendering.Universal.Light2D>(); l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; l.color = new Color(0.5f, 0.8f, 1f); l.intensity = 1.2f; l.pointLightOuterRadius = 2.5f;
spirit.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
var spiritPrefab = PrefabUtility.SaveAsPrefabAsset(spirit, DO + "FangDagger/WolfSpirit.prefab");
UnityEngine.Object.DestroyImmediate(spirit);

// 3) SummonWolf = SummonSacre 복제, 소환 대상 교체
string sumPath = DO + "FangDagger/SummonWolf.prefab";
if (!System.IO.File.Exists(sumPath)) AssetDatabase.CopyAsset(DO + "ScareStaff/SummonSacre.prefab", sumPath);
var sumRoot = PrefabUtility.LoadPrefabContents(sumPath);
var ss = sumRoot.GetComponent<SummonSkill>(); ss.summonObject = spiritPrefab; ss.summonCount = 2; ss.summonDistance = 0.5f; ss.yPos = -0.2f;
var sumSr = sumRoot.GetComponent<SpriteRenderer>(); if (sumSr != null) sumSr.color = new Color(1f, 1f, 1f, 0f); // 허수아비 소환 연출 숨김
PrefabUtility.SaveAsPrefabAsset(sumRoot, sumPath); PrefabUtility.UnloadPrefabContents(sumRoot);
var summonWolf = AssetDatabase.LoadAssetAtPath<GameObject>(sumPath);
sb.Append("summon; ");

// 4) FangDagger = DoubleDagger 복제 + F 스킬(ScareStaff 소환 스킬 값 복사)
string fdPath = W + "FangDagger.prefab";
if (!System.IO.File.Exists(fdPath)) AssetDatabase.CopyAsset(W + "DoubleDagger.prefab", fdPath);
var fd = PrefabUtility.LoadPrefabContents(fdPath);
fd.name = "FangDagger";
var weapon = fd.GetComponent<Weapon>();
weapon.minDmg = 9; weapon.maxDmg = 15; weapon.critPer = 10; weapon.critDmg = 20;
weapon.energyColor = new Color(0.55f, 0.85f, 1f);
var old = fd.transform.Find("FSkill"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var staff = PrefabUtility.LoadPrefabContents(W + "ScareStaff.prefab");
var src = staff.transform.Find("LSkill").GetComponent<Skill>();
var fGo = new GameObject("FSkill"); fGo.transform.SetParent(fd.transform, false);
var fSkill = fGo.AddComponent<Skill>();
EditorUtility.CopySerialized(src, fSkill);
PrefabUtility.UnloadPrefabContents(staff);
var pivotBox = fd.transform.Find("PivotBox");
var piv = pivotBox.Find("SummonPivot");
if (piv == null) { piv = new GameObject("SummonPivot").transform; piv.SetParent(pivotBox, false); piv.localPosition = new Vector3(0.6f, 0f, 0f); }
fSkill.skillPivot = new GameObject[]{ piv.gameObject };
fSkill.skillprefab = new GameObject[]{ summonWolf };
fSkill.skillName = "Howl of the Pack";
fSkill.SkillCoolTime = 8f; fSkill.skillWaitTime = 0.3f;
fSkill.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Wolf Fang.png");
var names = new[]{ "Fang Rush", "Pack Step", "Hunter's Stride" };
var skills = fd.GetComponentsInChildren<Skill>(true);
for (int i = 0; i < skills.Length && i < names.Length; i++) if (skills[i] != fSkill) skills[i].skillName = names[i];
var slots = weapon.skill; if (slots.Length < 4) System.Array.Resize(ref slots, 4); slots[3] = fSkill; weapon.skill = slots;
PrefabUtility.SaveAsPrefabAsset(fd, fdPath); PrefabUtility.UnloadPrefabContents(fd);
sb.Append("weapon (skills=" + skills.Length + "); ");

// 5) 일반 숲 드롭 테이블
var map = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
var drops = new System.Collections.Generic.Dictionary<string, string[]>();
string[] wolfD = { "Wolf Fang", "0.35", "FangDagger", "0.02", "Potion", "0.04" };
string[] boarD = { "Boar Tusk", "0.35", "Food", "0.1", "Potion", "0.04" };
string[] wispD = { "Wisp Ember", "0.3", "Potion", "0.04" };
string[] snailD = { "Snail Shell", "0.3", "Potion", "0.04" };
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    string n = dm.name; string[] list = null;
    if (n.StartsWith("Rabbit")) list = wolfD; else if (n.StartsWith("Mole")) list = boarD;
    else if (n.StartsWith("RatThief") || n.StartsWith("ScareCrow")) list = wispD;
    else if (n.StartsWith("RatCheif") || n.StartsWith("CreepingTerror")) list = snailD;
    if (list == null) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems"); arr.arraySize = list.Length / 2;
    for (int i = 0; i < list.Length / 2; i++) { var e = arr.GetArrayElementAtIndex(i); e.FindPropertyRelative("itemName").stringValue = list[i * 2]; e.FindPropertyRelative("dropProbability").floatValue = float.Parse(list[i * 2 + 1], System.Globalization.CultureInfo.InvariantCulture); e.FindPropertyRelative("offsetY").floatValue = 0.5f; }
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
sb.Append("drops " + nd);
return sb.ToString();
