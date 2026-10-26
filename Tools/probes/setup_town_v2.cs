// 마을 퀄리티업 v2 (2026-10-26): 주인공 셀과 같은 그림체의 NPC 4명 + 대장간·상점 건물. 에디트 모드, 재실행 안전.
// NPC 도트 = 주인공 Idle 셀(64px, PPU 36 × 1.5배 = 24도트/유닛)을 edit_image_pro_flash 로 편집(Docs/ArtCompare/Town).
// 시트는 2칸(기본·상체 1px 내림) → SpriteFlipbook 숨쉬기. 건물은 PPU 24(같은 밀도), 바닥선 피벗.
var town = GameObject.Find("Map").transform.Find("Town");
var sb = new System.Text.StringBuilder();
const float Ground = 135.5f;
System.Func<string, float, int, Sprite[]> import = (path, ppu, frames) =>
{
    var imp = (TextureImporter)AssetImporter.GetAtPath(path);
    imp.textureType = TextureImporterType.Sprite; imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false;
    imp.textureCompression = TextureImporterCompression.Uncompressed; imp.spritePixelsPerUnit = ppu; imp.alphaIsTransparency = true;
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int w = tex.width / frames, h = tex.height;
    imp.spriteImportMode = SpriteImportMode.Multiple;
#pragma warning disable 618
    var metas = new SpriteMetaData[frames];
    for (int i = 0; i < frames; i++) { metas[i].name = System.IO.Path.GetFileNameWithoutExtension(path) + "_" + i; metas[i].rect = new Rect(i * w, 0, w, h); metas[i].alignment = (int)SpriteAlignment.BottomCenter; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
    var list = new System.Collections.Generic.List<Sprite>();
    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Sprite) list.Add((Sprite)o);
    list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    return list.ToArray();
};
System.Action<string, string, float, int, float> npc = (objName, file, x, facing, frameTime) =>
{
    var t = town.Find(objName);
    var frames = import("Assets/Art/NPC/Town/" + file + ".png", 36f, 2);
    t.position = new Vector3(x, Ground, 0f);
    t.localScale = new Vector3(1.5f * facing, 1.5f, 1f);
    var sr = t.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.flipX = false; sr.sortingOrder = 25;
    var fb = t.GetComponent<SpriteFlipbook>(); if (fb == null) fb = t.gameObject.AddComponent<SpriteFlipbook>();
    fb.Frames = frames;
    var so = new SerializedObject(fb); so.FindProperty("_frameTime").floatValue = frameTime; so.ApplyModifiedPropertiesWithoutUndo();
    var col = t.GetComponent<BoxCollider2D>();
    col.size = new Vector2(1.6f / 1.5f, 2.2f / 1.5f); col.offset = new Vector2(0f, 1.1f / 1.5f);
    var talk = t.GetComponent<TalkNpc>();
    if (talk != null)
    {
        var ts = new SerializedObject(talk);
        ts.FindProperty("_promptHeight").floatValue = 2.6f / 1.5f;
        ts.FindProperty("_portrait").objectReferenceValue = frames[0];
        ts.ApplyModifiedPropertiesWithoutUndo();
    }
    sb.Append(objName + "@" + x + " ");
};
System.Func<string, string, float, GameObject> building = (objName, file, x) =>
{
    var old = town.Find(objName); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var sp = import("Assets/Art/NPC/Town/" + file + ".png", 24f, 1)[0];
    var go = new GameObject(objName); go.transform.SetParent(town, false);
    go.transform.position = new Vector3(x, Ground - 1f / 24f, 0f);
    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingOrder = 20;
    sr.sharedMaterial = town.Find("House").GetComponent<SpriteRenderer>().sharedMaterial;
    return go;
};
System.Action<GameObject, Vector2, Color, float, float> glow = (parent, local, color, radius, intensity) =>
{
    var lg = new GameObject("Glow"); lg.transform.SetParent(parent.transform, false); lg.transform.localPosition = local;
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
    l.color = color; l.intensity = intensity; l.pointLightOuterRadius = radius; l.pointLightInnerRadius = radius * 0.15f; l.falloffIntensity = 0.7f;
    lg.AddComponent<LightFlicker>();
};

var forge = building("Bld_Forge", "Bld_forge", -418.3f);
glow(forge, new Vector2(0.3f, 1.25f), new Color(1f, 0.55f, 0.2f), 2.6f, 1.1f);
var stall = building("Bld_Stall", "Bld_stall", -427.5f);
glow(stall, new Vector2(-2.6f, 2.75f), new Color(1f, 0.75f, 0.4f), 1.8f, 0.8f);

npc("Shop", "Npc_merchant", -423.6f, 1, 0.62f);
npc("WeaponSmith", "Npc_weaponsmith", -420.1f, 1, 0.55f);
npc("EquipSmith", "Npc_armorsmith", -416.7f, 1, 0.68f);
npc("NPC_Elder", "Npc_elder", -396.6f, -1, 0.75f);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(town.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(town.gameObject.scene);
return sb.ToString();
