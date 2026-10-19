// 음악·앰비언트 임포트 설정: Streaming + Vorbis 0.6 (용량·메모리 절약). 재실행 안전
var sb = new System.Text.StringBuilder();
foreach (var p in new[]{ "Assets/Resources/Ambience", "Assets/ThirdParty/Music" })
{
    foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[]{ p }))
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var imp = AssetImporter.GetAtPath(path) as AudioImporter;
        var s = imp.defaultSampleSettings;
        if (s.loadType == AudioClipLoadType.Streaming && s.compressionFormat == AudioCompressionFormat.Vorbis) continue;
        s.loadType = AudioClipLoadType.Streaming; s.compressionFormat = AudioCompressionFormat.Vorbis; s.quality = 0.6f;
        imp.defaultSampleSettings = s; imp.SaveAndReimport();
        sb.Append(System.IO.Path.GetFileName(path) + "; ");
    }
}
return "set: " + sb;
