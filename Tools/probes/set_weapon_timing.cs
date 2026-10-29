// 무기 프리팹에 무기군 타이밍 프로파일·사슬 넣기(MotionSpec39 §3 기본 타이밍 표, 10-30): Weapon._windupMul/_recoverMul/_chain
// 기본 3타를 쓰는 한손검·방패 계열은 1·1 그대로(사용자 확정 기본 동작 보존). 재실행 안전
var table = new System.Collections.Generic.Dictionary<string, (float w, float r, bool chain)> {
    // 단검·쌍단검·클로 — 빠르게
    { "CutthroatDagger", (0.8f, 0.8f, false) }, { "DoubleDagger", (0.8f, 0.8f, false) }, { "FangDagger", (0.8f, 0.8f, false) }, { "RatCheifDagger", (0.8f, 0.8f, false) },
    { "ScareClow", (0.8f, 0.85f, false) }, { "SkyfallTalon", (0.8f, 0.85f, false) }, { "BearClaw", (0.85f, 0.85f, false) },
    // 맨손·너클
    { "Leather Knuckles", (0.75f, 0.8f, false) }, { "TuskKnuckles", (0.75f, 0.8f, false) }, { "PebbleGauntlet", (0.8f, 0.85f, false) },
    // 레이피어 — 찌르기 빠르게
    { "BloodstingRapier", (0.85f, 0.9f, false) },
    // 양손 대검·둔기·도끼·곡괭이 — 무겁게
    { "HeartwoodGreatsword", (1.25f, 1.15f, false) }, { "SacreGreatSword", (1.25f, 1.15f, false) }, { "QuakestoneMaul", (1.4f, 1.2f, false) },
    { "CrocJawAxe", (1.3f, 1.15f, false) }, { "PickAxe", (1.3f, 1.1f, false) }, { "ToxicPickAxe", (1.3f, 1.1f, false) }, { "SlimeMace", (1.2f, 1.1f, false) },
    // 장병기·낫
    { "SentryHalberd", (1.15f, 1.1f, false) }, { "Wood Spear", (1.1f, 1.05f, false) }, { "PhantomScythe", (1.15f, 1.05f, false) }, { "ScareSide", (1.15f, 1.05f, false) },
    // 채찍 — 사슬(줄)
    { "FrogTongueWhip", (1.1f, 1.0f, false) }, // 혀 그림이 이미 끝까지 그려져 있어 사슬은 끔 — 손잡이만 있는 채찍 그림이 생기면 켠다
    // 지팡이·완드·등불 — 시전 조금 길게
    { "AbyssStaff", (1.1f, 1.0f, false) }, { "CliffStaff", (1.1f, 1.0f, false) }, { "LeafStaff", (1.1f, 1.0f, false) }, { "RootStaff", (1.1f, 1.0f, false) },
    { "ScareStaff", (1.1f, 1.0f, false) }, { "FaeWand", (1.0f, 1.0f, false) }, { "WispLantern", (1.05f, 1.0f, false) },
};
var sb = new System.Text.StringBuilder();
foreach (var kv in table)
{
    string path = "Assets/Resources/Weapon/" + kv.Key + ".prefab";
    var root = PrefabUtility.LoadPrefabContents(path);
    if (root == null) { sb.Append(kv.Key + "=없음 "); continue; }
    var w = root.GetComponent<Weapon>();
    if (w == null) { PrefabUtility.UnloadPrefabContents(root); sb.Append(kv.Key + "=Weapon없음 "); continue; }
    var so = new SerializedObject(w);
    so.FindProperty("_windupMul").floatValue = kv.Value.w;
    so.FindProperty("_recoverMul").floatValue = kv.Value.r;
    so.FindProperty("_chain").boolValue = kv.Value.chain;
    if (kv.Value.chain)
    {
        so.FindProperty("_chainLinks").intValue = 14;
        so.FindProperty("_chainColor").colorValue = new Color(0.62f, 0.35f, 0.4f, 1f); // 개구리 혀 — 분홍빛 갈색
    }
    so.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.SaveAsPrefabAsset(root, path);
    PrefabUtility.UnloadPrefabContents(root);
    sb.Append(kv.Key + "=" + kv.Value.w + "/" + kv.Value.r + (kv.Value.chain ? "+사슬" : "") + " ");
}
return sb.ToString();
