// B-115 조사: 씬 타일맵이 쓰는 텍스처의 임포트 설정·아틀라스 포함 여부, 픽셀 퍼펙트 카메라 설정
var sb = new System.Text.StringBuilder();
var tex = new System.Collections.Generic.Dictionary<string, int>();
foreach (var tm in UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    var b = tm.cellBounds;
    var tiles = tm.GetTilesBlock(b);
    foreach (var t in tiles)
    {
        var tt = t as UnityEngine.Tilemaps.Tile;
        if (tt == null || tt.sprite == null) continue;
        string p = AssetDatabase.GetAssetPath(tt.sprite.texture);
        tex.TryGetValue(p, out int n); tex[p] = n + 1;
    }
}
foreach (var kv in tex)
{
    var ti = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
    if (ti == null) { sb.AppendLine(kv.Key + " (no importer)"); continue; }
    var st = new TextureImporterSettings(); ti.ReadTextureSettings(st);
    sb.AppendLine(kv.Value + "  " + kv.Key + " | filter=" + ti.filterMode + " comp=" + ti.textureCompression + " mesh=" + st.spriteMeshType + " extrude=" + st.spriteExtrude + " ppu=" + ti.spritePixelsPerUnit + " mip=" + ti.mipmapEnabled + " wrap=" + ti.wrapMode);
}
foreach (var c in UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Universal.PixelPerfectCamera>(FindObjectsSortMode.None))
    sb.AppendLine("PPC " + c.name + " ppu=" + c.assetsPPU + " ref=" + c.refResolutionX + "x" + c.refResolutionY + " mode=" + c.gridSnapping + " crop=" + c.cropFrameX + c.cropFrameY + " enabled=" + c.enabled);
var grids = UnityEngine.Object.FindObjectsByType<Grid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
foreach (var g in grids) sb.AppendLine("Grid " + g.name + " cell=" + g.cellSize + " gap=" + g.cellGap);
return sb.ToString();
