// A-201: 보스 5종의 도트 밀도 18 → 24 (PixelLook 24 격자에서 도트 크기 균일). 에디트 모드, 재실행 안전(이미 24면 건너뜀).
// 시트 PPU ×4/3, 발 위치가 그대로이도록 피벗 재계산, 몸 콜라이더 ×0.75(발 기준), hpHeight ×0.75. BT·공격 판정 프리팹은 그대로.
if (EditorApplication.isPlaying) return "stop play first";
var bosses = new[] { "AncientEnt_BD", "BanditChief_BD", "GargoyleLord_BD", "Gatekeeper_BD", "GiantSlime_BD" };
const float k = 24f / 18f;
var sb = new System.Text.StringBuilder();
foreach (var name in bosses)
{
    string prefabPath = "Assets/Prefab/Enemy_BD/" + name + ".prefab";
    var root = PrefabUtility.LoadPrefabContents(prefabPath);
    try
    {
        var box = root.GetComponent<BoxCollider2D>();
        float feetLocal = box.offset.y - box.size.y / 2f;
        float scale = Mathf.Abs(root.transform.localScale.x);
        // 시트 = 애니메이터 클립이 쓰는 텍스처들
        var textures = new System.Collections.Generic.HashSet<string>();
        foreach (var clip in root.GetComponent<Animator>().runtimeAnimatorController.animationClips)
        {
            foreach (var b in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                foreach (var kf in AnimationUtility.GetObjectReferenceCurve(clip, b))
                {
                    if (kf.value is Sprite) textures.Add(AssetDatabase.GetAssetPath(((Sprite)kf.value).texture));
                }
            }
        }
        // 같은 폴더의 Death 등 나머지 시트도
        var dirs = new System.Collections.Generic.HashSet<string>();
        foreach (var t in textures) dirs.Add(System.IO.Path.GetDirectoryName(t).Replace('\\', '/'));
        foreach (var d in dirs) foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { d })) textures.Add(AssetDatabase.GUIDToAssetPath(g));
        int changed = 0;
        float oldPpu = 0f;
        foreach (var path in textures)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null || imp.spriteImportMode != SpriteImportMode.Multiple) continue;
            float ppu = imp.spritePixelsPerUnit;
            if (Mathf.Abs(ppu - 18f * scale) > 0.05f) continue; // 이미 24 이거나 다른 시트
            oldPpu = ppu;
            float newPpu = ppu * k;
#pragma warning disable 618
            var metas = imp.spritesheet;
            for (int i = 0; i < metas.Length; i++)
            {
                float h = metas[i].rect.height;
                float feetRow = metas[i].pivot.y * h + feetLocal * ppu; // 발 픽셀 행(아래에서)
                metas[i].alignment = (int)SpriteAlignment.Custom;
                metas[i].pivot = new Vector2(metas[i].pivot.x, (feetRow - feetLocal * newPpu) / h);
            }
            imp.spritesheet = metas;
#pragma warning restore 618
            imp.spritePixelsPerUnit = newPpu;
            imp.SaveAndReimport();
            changed++;
        }
        if (changed == 0) { sb.AppendLine(name + ": skip (이미 적용)"); continue; }
        box.size = box.size / k;
        box.offset = new Vector2(box.offset.x / k, feetLocal + box.size.y / 2f);
        var eh = root.GetComponent<EnemyHealth>();
        if (eh != null) eh.hpHeight = eh.hpHeight / k;
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        sb.AppendLine(name + ": sheets " + changed + " ppu " + oldPpu.ToString("F2") + "→" + (oldPpu * k).ToString("F2") + " box " + box.size + " hpHeight " + (eh != null ? eh.hpHeight.ToString("F2") : "-"));
    }
    finally { PrefabUtility.UnloadPrefabContents(root); }
}
return sb.ToString();
