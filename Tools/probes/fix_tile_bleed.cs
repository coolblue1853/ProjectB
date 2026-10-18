// B-115 타일맵 틈: LV1 타일 시트를 셀(256)마다 가장자리 픽셀을 PAD 만큼 늘린(extrude) 시트로 다시 만들고, 스프라이트 rect 를 옮긴다(이름·spriteID 유지 → 타일 참조 유지).
// 임포트도 원본 해상도(2048)·무압축·FullRect 로. 이미 처리된 시트(가로가 셀+2PAD 배수)는 건너뛴다.
const int CELL = 256, PAD = 4, STRIDE = CELL + PAD * 2;
var sb = new System.Text.StringBuilder();
var files = new[]{"TileMap1","TileMap1 1","TileMap2","TileMap3","TileMap4","TileMap5","Tilemap6"};
foreach (var name in files)
{
    string path = "Assets/Sprite/Map/LV1/" + name + ".png";
    var ti = (TextureImporter)AssetImporter.GetAtPath(path);
    var src = new Texture2D(2, 2); src.LoadImage(System.IO.File.ReadAllBytes(path));
    if (src.width % STRIDE == 0 && src.height % STRIDE == 0) { sb.AppendLine(name + " skip (already padded)"); continue; }
    int cols = src.width / CELL, rows = src.height / CELL;
    var f = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories(); f.Init();
    var dp = f.GetSpriteEditorDataProviderFromObject(ti); dp.InitSpriteEditorDataProvider();
    var rects = dp.GetSpriteRects();
    bool grid = true;
    foreach (var r in rects) if (r.rect.x % CELL != 0 || r.rect.y % CELL != 0 || r.rect.width != CELL || r.rect.height != CELL) grid = false;
    if (!grid) { sb.AppendLine(name + " skip (not 256 grid)"); continue; }
    var sp = src.GetPixels32();
    var dst = new Color32[cols * STRIDE * rows * STRIDE];
    int dw = cols * STRIDE;
    for (int cy = 0; cy < rows; cy++)
    for (int cx = 0; cx < cols; cx++)
    for (int y = -PAD; y < CELL + PAD; y++)
    for (int x = -PAD; x < CELL + PAD; x++)
    {
        int sx = cx * CELL + Mathf.Clamp(x, 0, CELL - 1), sy = cy * CELL + Mathf.Clamp(y, 0, CELL - 1);
        int dx = cx * STRIDE + PAD + x, dy = cy * STRIDE + PAD + y;
        dst[dy * dw + dx] = sp[sy * src.width + sx];
    }
    var outTex = new Texture2D(dw, rows * STRIDE, TextureFormat.RGBA32, false);
    outTex.SetPixels32(dst); outTex.Apply();
    System.IO.File.WriteAllBytes(path, outTex.EncodeToPNG());
    foreach (var r in rects)
    {
        int cx = (int)r.rect.x / CELL, cy = (int)r.rect.y / CELL;
        r.rect = new Rect(cx * STRIDE + PAD, cy * STRIDE + PAD, CELL, CELL);
    }
    dp.SetSpriteRects(rects); dp.Apply();
    ti.maxTextureSize = 4096; ti.textureCompression = TextureImporterCompression.Uncompressed;
    foreach (var plat in new[]{"Standalone","Android","iPhone","WebGL"}) { var ps = ti.GetPlatformTextureSettings(plat); if (ps.overridden) { ps.maxTextureSize = 4096; ps.format = TextureImporterFormat.RGBA32; ti.SetPlatformTextureSettings(ps); } }
    var st = new TextureImporterSettings(); ti.ReadTextureSettings(st); st.spriteMeshType = SpriteMeshType.FullRect; ti.SetTextureSettings(st);
    ti.SaveAndReimport();
    sb.AppendLine(name + " padded " + src.width + "x" + src.height + " -> " + dw + "x" + rows * STRIDE + " sprites=" + rects.Length);
}
return sb.ToString();
