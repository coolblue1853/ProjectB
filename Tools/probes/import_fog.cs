// 어비스 안개 텍스처 임포트 설정: 24 PPU, Point, FullRect(타일 그리기), Repeat, 무압축. 재실행 안전
var sb = new System.Text.StringBuilder();
foreach (var n in new[]{ "FogFar", "FogNear" })
{
    string p = "Assets/Resources/Atmosphere/" + n + ".png";
    var imp = AssetImporter.GetAtPath(p) as TextureImporter; if (imp == null) { sb.Append("missing " + n + " "); continue; }
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
    imp.spritePixelsPerUnit = 24; imp.filterMode = FilterMode.Point; imp.wrapMode = TextureWrapMode.Repeat;
    imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.alphaIsTransparency = true;
    var s = new TextureImporterSettings(); imp.ReadTextureSettings(s); s.spriteMeshType = SpriteMeshType.FullRect; s.spriteAlignment = (int)SpriteAlignment.Center; imp.SetTextureSettings(s);
    imp.SaveAndReimport(); sb.Append(n + " ok ");
}
return sb.ToString();
