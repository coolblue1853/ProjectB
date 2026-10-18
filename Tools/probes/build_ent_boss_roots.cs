// 고대숲의 엔트 보스 공격 판정 교체 (TowerWorld 13절): PassiveTerror 것(갈퀴 ClowAttack·불꽃 PassiveTerrorAttackPrefab) → 뿌리 판정.
// 원본 공유 프리팹은 그대로 두고 복제본(AncientEntRootSwipe / AncientEntRootThorn)을 만들어 스프라이트를 코드 도트 뿌리로 바꾼다.
// 애니메이션은 원본 컨트롤러 + AnimatorOverrideController(클립만 뿌리 프레임으로 교체). 보스 스포너 objectInfos 키(clow/fire)는 유지하고 프리팹만 바꾼다.
// 에디트 모드, 재실행 안전. 보스 BT 의 damageOb 참조(ClowAttack)는 이 프로브 뒤에 텍스트로 GUID 교체한다(아래 반환값에 GUID 출력).
var sb = new System.Text.StringBuilder();
string SRC = "Assets/Prefab/Enemy/EnemyDamge/", DST = SRC + "AncientEnt/", ART = "Assets/Art/Monsters/AncientEnt/";
System.IO.Directory.CreateDirectory(DST);
System.Func<string, int, float, Sprite[]> slice = (name, fs, ppu) => {
    string p = ART + name + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    int n = tex.width / fs;
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple; imp.spritePixelsPerUnit = ppu;
    imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
    var metas = new SpriteMetaData[n];
    for (int i = 0; i < n; i++) { metas[i].name = name + "_" + i; metas[i].rect = new Rect(i * fs, 0, fs, fs); metas[i].alignment = (int)SpriteAlignment.Center; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
    var list = new System.Collections.Generic.List<Sprite>();
    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) if (o is Sprite) list.Add((Sprite)o);
    list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    return list.ToArray(); };

// 원본과 같은 월드 크기: 갈퀴 256px@64ppu = 4유닛 → 64px@16ppu, 불꽃 256px@100ppu = 2.56유닛 → 32px@12.5ppu
var swipe = slice("RootSwipe", 64, 16f);
var thorn = slice("RootThorn", 32, 12.5f);

System.Func<string, string, Sprite[], Color, GameObject> make = (srcName, dstName, frames, color) => {
    string src = SRC + srcName + ".prefab", dst = DST + dstName + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(src, dst);
    var srcGo = AssetDatabase.LoadAssetAtPath<GameObject>(src);
    var baseCtrl = srcGo.GetComponent<Animator>().runtimeAnimatorController;
    var srcClip = baseCtrl.animationClips[0];
    // 뿌리 클립: 원본 스프라이트 키 시간을 그대로 쓰고 프레임만 교체
    string clipPath = DST + dstName + ".anim";
    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
    clip.frameRate = srcClip.frameRate;
    var binding = AnimationUtility.GetObjectReferenceCurveBindings(srcClip)[0];
    var keys = AnimationUtility.GetObjectReferenceCurve(srcClip, binding);
    var nk = new ObjectReferenceKeyframe[keys.Length];
    for (int i = 0; i < keys.Length; i++) { nk[i].time = keys[i].time; nk[i].value = frames[Mathf.Min(i, frames.Length - 1)]; }
    AnimationUtility.SetObjectReferenceCurve(clip, binding, nk);
    var st = AnimationUtility.GetAnimationClipSettings(srcClip); AnimationUtility.SetAnimationClipSettings(clip, st);
    string ocPath = DST + dstName + ".overrideController";
    var oc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(ocPath);
    if (oc == null) { oc = new AnimatorOverrideController(baseCtrl); AssetDatabase.CreateAsset(oc, ocPath); }
    oc.runtimeAnimatorController = baseCtrl;
    oc[srcClip] = clip;
    EditorUtility.SetDirty(oc); EditorUtility.SetDirty(clip);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = dstName;
    pr.GetComponent<Animator>().runtimeAnimatorController = oc;
    var sr = pr.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.color = color;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append(dstName + " keys" + keys.Length + "/frames" + frames.Length + " guid " + AssetDatabase.AssetPathToGUID(dst) + "; ");
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };
var swipeGo = make("ClowAttack", "AncientEntRootSwipe", swipe, Color.white);
var thornGo = make("PassiveTerrorAttackPrefab", "AncientEntRootThorn", thorn, Color.white);
AssetDatabase.SaveAssets();

// 보스 스포너 풀: 키는 그대로(clow/fire — 보스 BT 가 이 이름으로 꺼낸다), 프리팹만 교체
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
var map = GameObject.Find("Map").transform.Find("Lv1_DeepForest");
foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true))
{
    if (s.name != "BossSpawoner") continue;
    var arr = (System.Array)f.GetValue(s);
    var et = arr.GetType().GetElementType();
    for (int i = 0; i < arr.Length; i++)
    {
        var o = arr.GetValue(i); // 구조체여도 되도록 꺼내서 고치고 다시 넣는다
        var key = (string)et.GetField("objectName").GetValue(o);
        if (key == "clow") et.GetField("perfab").SetValue(o, swipeGo);
        else if (key == "fire") et.GetField("perfab").SetValue(o, thornGo);
        var pf = (GameObject)et.GetField("perfab").GetValue(o);
        sb.Append(key + "=" + (pf ? pf.name : "null") + " ");
        arr.SetValue(o, i);
    }
    f.SetValue(s, arr);
    EditorUtility.SetDirty(s);
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString();
