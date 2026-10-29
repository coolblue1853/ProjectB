// 임시 도트 무기(gen_tmp_weapons.py) 임포트 설정: 스프라이트·점 필터·무압축·24 PPU·피벗 = 손잡이(json grip, ShortSword 와 같은 식). 재실행 안전
var sb = new System.Text.StringBuilder();
foreach (var n in new[] { "TmpKatana", "TmpHammer", "TmpCannon", "TmpKnife", "TmpHook", "TmpPistol", "TmpRifle", "TmpSword", "TmpGreatSword", "TmpDagger", "TmpSpear", "TmpStaff", "TmpBow", "TmpShield", "TmpKnuckle", "TmpWhip", "TmpCutlass", "TmpAnchor", "TmpEyeStaff", "TmpTome" })
{
    string p = "Assets/Resources/PixelRig/Weapons/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var ti = (TextureImporter)AssetImporter.GetAtPath(p);
    var j = JsonUtility.FromJson<GripMeta>(System.IO.File.ReadAllText("Assets/Resources/PixelRig/Weapons/" + n + ".json"));
    ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
    ti.filterMode = FilterMode.Point; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false;
    ti.spritePixelsPerUnit = 24f;
    var s = new TextureImporterSettings(); ti.ReadTextureSettings(s);
    s.spriteAlignment = (int)SpriteAlignment.Custom;
    s.spritePivot = new Vector2((j.grip[0] + 0.5f) / j.size[0], (j.size[1] - j.grip[1] - 0.5f) / j.size[1]);
    ti.SetTextureSettings(s);
    ti.SaveAndReimport();
    sb.Append(n + " " + s.spritePivot + "; ");
}
return sb.ToString();
}
[System.Serializable] class GripMeta { public int[] grip; public int[] size; }
class _Dummy {
