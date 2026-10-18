// ④구역 늪지 거대 슬라임·모기 공격 판정 교체 (B-124, TowerWorld 14절) — build_ent_boss_roots.cs 방식.
// 보스: PassiveTerror 판정(갈퀴 ClowAttack·불꽃 PassiveTerrorAttackPrefab) → 점액 내려찍기 GiantSlimeSlam·점액 탄 GiantSlimeGelShot (원본 공유 프리팹 미수정, 복제 + OverrideController 로 클립만 교체, 키 시간 유지)
//       2페이즈 분열: BossMinionCall(늪 슬라임 2) + 보스 스포너 풀 minion/minionDead/leaf(슬라임 공격 키)
// 모기: FlowerMole 유도탄(FlowerAttackPrefab) → 짧은 돌진 물기 MosquitoBite(ClowAttack 복제 + EnemyLungeBite), 풀 키 flower 유지
// 스포너 objectInfos 키는 유지하고 프리팹만 바꾼다. 보스·모기 BT 의 damageOb 캐시 GUID 도 새 판정으로 텍스트 교체. 에디트 모드, 재실행 안전.
var reimportSince = System.DateTime.Now.AddSeconds(-1);
var sb = new System.Text.StringBuilder();
string SRC = "Assets/Prefab/Enemy/EnemyDamge/", DST = SRC + "Swamp/", ART = "Assets/Art/Monsters/";
System.IO.Directory.CreateDirectory(DST);
AssetDatabase.Refresh();
System.Func<string, int, float, Sprite[]> slice = (rel, fs, ppu) => {
    string p = ART + rel + ".png";
    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    int n = tex.width / fs;
    string nm = System.IO.Path.GetFileNameWithoutExtension(p);
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple; imp.spritePixelsPerUnit = ppu;
    imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
    var metas = new SpriteMetaData[n];
    for (int i = 0; i < n; i++) { metas[i].name = nm + "_" + i; metas[i].rect = new Rect(i * fs, 0, fs, fs); metas[i].alignment = (int)SpriteAlignment.Center; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
    var list = new System.Collections.Generic.List<Sprite>();
    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) { if (o is Sprite) { list.Add((Sprite)o); } }
    list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    return list.ToArray(); };

// 원본과 같은 월드 크기: 갈퀴 256px@64ppu → 64px@16ppu, 불꽃 256px@100ppu → 32px@12.5ppu. 물기는 갈퀴 프리팹 스케일(0.82)에서 약 1.3유닛 → 32px@20ppu
var slam = slice("GiantSlime/SlimeSlam", 64, 16f);
var gel = slice("GiantSlime/GelShot", 32, 12.5f);
var bite = slice("SwampMosquito/MosquitoBite", 32, 20f);

System.Func<string, string, Sprite[], GameObject> make = (srcName, dstName, frames) => {
    string src = SRC + srcName + ".prefab", dst = DST + dstName + ".prefab";
    if (!System.IO.File.Exists(dst)) { AssetDatabase.CopyAsset(src, dst); }
    var srcGo = AssetDatabase.LoadAssetAtPath<GameObject>(src);
    var baseCtrl = srcGo.GetComponent<Animator>().runtimeAnimatorController;
    var srcClip = baseCtrl.animationClips[0];
    if (baseCtrl.animationClips.Length != 1) { sb.Append("[warn " + srcName + " clips=" + baseCtrl.animationClips.Length + "] "); }
    string clipPath = DST + dstName + ".anim";
    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
    clip.frameRate = srcClip.frameRate;
    var binding = AnimationUtility.GetObjectReferenceCurveBindings(srcClip)[0];
    var keys = AnimationUtility.GetObjectReferenceCurve(srcClip, binding);
    var nk = new ObjectReferenceKeyframe[keys.Length];
    for (int i = 0; i < keys.Length; i++) { nk[i].time = keys[i].time; nk[i].value = frames[Mathf.Min(i * frames.Length / Mathf.Max(1, keys.Length), frames.Length - 1)]; }
    AnimationUtility.SetObjectReferenceCurve(clip, binding, nk);
    AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(srcClip));
    string ocPath = DST + dstName + ".overrideController";
    var oc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(ocPath);
    if (oc == null) { oc = new AnimatorOverrideController(baseCtrl); AssetDatabase.CreateAsset(oc, ocPath); }
    oc.runtimeAnimatorController = baseCtrl;
    oc[srcClip] = clip;
    EditorUtility.SetDirty(oc); EditorUtility.SetDirty(clip);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = dstName;
    pr.GetComponent<Animator>().runtimeAnimatorController = oc;
    var sr = pr.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.color = Color.white;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append(dstName + " keys" + keys.Length + "/frames" + frames.Length + "; ");
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };

var slamGo = make("ClowAttack", "GiantSlimeSlam", slam);
var gelGo = make("PassiveTerrorAttackPrefab", "GiantSlimeGelShot", gel);
var biteGo = make("ClowAttack", "MosquitoBite", bite);

// 물기: 짧은 근접 판정(발사 없음) + 돌진, 콜라이더는 침 궤적 크기
{
    string path = DST + "MosquitoBite.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var d = pr.GetComponent<EnemyDamageObject>();
    d.damage = 12; d.holdingTime = 0.4f; d.isDestroyByTime = true; d.isLaunch = false; d.isGroundDestroy = false;
    d.knockForce = 15f; d.stiffnessTime = 0.2f;
    var box = pr.GetComponent<BoxCollider2D>();
    box.size = new Vector2(1.5f, 0.5f); box.offset = new Vector2(0.2f, 0f);
    if (pr.GetComponent<EnemyLungeBite>() == null) { pr.AddComponent<EnemyLungeBite>(); }
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
}

// 보스 분열: 2페이즈에 늪 슬라임 2
{
    string path = "Assets/Prefab/Enemy_BD/GiantSlime_BD.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var mc = pr.GetComponent<BossMinionCall>(); if (mc == null) { mc = pr.AddComponent<BossMinionCall>(); }
    var so = new SerializedObject(mc);
    so.FindProperty("_count").intValue = 2;
    so.FindProperty("_spread").floatValue = 2.5f;
    so.FindProperty("_bannerText").stringValue = "SPLITS — Slimes burst out!";
    so.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
}
AssetDatabase.SaveAssets();

// BT damageOb 캐시 참조 GUID 교체(런타임은 풀 키로 꺼내므로 표시용 — 엔트와 같은 정리)
// 루트 fileID 도 함께 바꾼다(모기: 유도탄 원본과 물기 복제 원본이 달라 GUID 만 바꾸면 참조가 끊긴다)
System.Func<string, string, string, int> swapGuid = (prefab, from, to) => {
    string gFrom = AssetDatabase.AssetPathToGUID(from), gTo = AssetDatabase.AssetPathToGUID(to);
    long idFrom, idTo; string g;
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(AssetDatabase.LoadAssetAtPath<GameObject>(from), out g, out idFrom);
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(AssetDatabase.LoadAssetAtPath<GameObject>(to), out g, out idTo);
    string a = "{fileID: " + idFrom + ", guid: " + gFrom, b = "{fileID: " + idTo + ", guid: " + gTo;
    string txt = System.IO.File.ReadAllText(prefab);
    int c = 0, i = 0;
    while ((i = txt.IndexOf(a, i, System.StringComparison.Ordinal)) >= 0) { c++; i += a.Length; }
    if (c > 0) { System.IO.File.WriteAllText(prefab, txt.Replace(a, b)); AssetDatabase.ImportAsset(prefab, ImportAssetOptions.ForceUpdate); }
    return c; };
sb.Append("guid boss=" + swapGuid("Assets/Prefab/Enemy_BD/GiantSlime_BD.prefab", SRC + "ClowAttack.prefab", DST + "GiantSlimeSlam.prefab"));
sb.Append(" fire=" + swapGuid("Assets/Prefab/Enemy_BD/GiantSlime_BD.prefab", SRC + "PassiveTerrorAttackPrefab.prefab", DST + "GiantSlimeGelShot.prefab"));
sb.Append(" mosq=" + swapGuid("Assets/Prefab/Enemy_BD/SwampMosquito_BD.prefab", SRC + "FlowerAttackPrefab.prefab", DST + "MosquitoBite.prefab") + "; ");
sb.Append(" reimported=" + ProjectB.EditorTools.MonsterBuilder.ReimportModifiedSince(reimportSince) + "; ");

// 스포너 풀: 키 유지, 프리팹 교체 + 보스 부하 키 추가
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
var map = GameObject.Find("Map").transform.Find("Lv1_Swamp");
if (map == null) { return sb.ToString() + " ABORT: Lv1_Swamp 없음"; }
GameObject slimeMob = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BogSlime_BD.prefab");
GameObject slimeDead = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Lv2/BogSlimeDeadBody.prefab");
GameObject slimeLeaf = null;
foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true))
{
    if (s.name != "Slime1") { continue; }
    var arr = (System.Array)f.GetValue(s); var et = arr.GetType().GetElementType();
    foreach (var o in arr) { if ((string)et.GetField("objectName").GetValue(o) == "leaf") { slimeLeaf = (GameObject)et.GetField("perfab").GetValue(o); } }
}
if (slimeMob == null || slimeDead == null || slimeLeaf == null) { return sb.ToString() + " ABORT: 부하 프리팹 없음 mob=" + (slimeMob != null) + " dead=" + (slimeDead != null) + " leaf=" + (slimeLeaf != null); }
foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true))
{
    bool isBoss = s.name == "BossSpawoner", isMosq = s.name.StartsWith("Mosquito");
    if (!isBoss && !isMosq) { continue; }
    var arr = (System.Array)f.GetValue(s);
    var et = arr.GetType().GetElementType();
    var list = new System.Collections.Generic.List<object>();
    foreach (var o in arr)
    {
        var key = (string)et.GetField("objectName").GetValue(o);
        if (isBoss && key == "clow") { et.GetField("perfab").SetValue(o, slamGo); }
        else if (isBoss && key == "fire") { et.GetField("perfab").SetValue(o, gelGo); }
        else if (isMosq && key == "flower") { et.GetField("perfab").SetValue(o, biteGo); }
        if (isBoss && (key == "minion" || key == "minionDead" || key == "leaf")) { continue; } // 아래에서 다시 넣는다(재실행 안전)
        list.Add(o);
    }
    if (isBoss)
    {
        System.Action<string, GameObject, int> add = (k, go, n) => {
            var o = System.Activator.CreateInstance(et, true);
            et.GetField("objectName").SetValue(o, k); et.GetField("perfab").SetValue(o, go); et.GetField("count").SetValue(o, n);
            list.Add(o); };
        add("minion", slimeMob, 2); add("minionDead", slimeDead, 2); add("leaf", slimeLeaf, 3);
    }
    var na = System.Array.CreateInstance(et, list.Count);
    for (int i = 0; i < list.Count; i++) { na.SetValue(list[i], i); }
    f.SetValue(s, na);
    EditorUtility.SetDirty(s);
    sb.Append(s.name + ":");
    foreach (var o in na) { var pf = (GameObject)et.GetField("perfab").GetValue(o); sb.Append(" " + et.GetField("objectName").GetValue(o) + "=" + (pf ? pf.name : "null")); }
    sb.Append("; ");
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString();
