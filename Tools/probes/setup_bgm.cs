// 구역 BGM (AssetIntegrationPlan W1): 보유 음악 팩에서 꺼낸 OGG 를 MasterAudio 재생목록에 추가. 에디트 모드, 재실행 안전
var ma = UnityEngine.Object.FindFirstObjectByType<DarkTonic.MasterAudio.MasterAudio>(FindObjectsInactive.Include);
string dir = "Assets/ThirdParty/Music/";
var add = new string[][] {
    new[]{ "Town", "LOOP_Marketplace" },
    new[]{ "Field", "Forest LOOP" },
    new[]{ "Night", "Moonlit Forest - Main Loop" },
    new[]{ "Boss", "Boss Battle 2 Loop" },
    new[]{ "Field2", "LOOP_Fields of Mystery" },
    new[]{ "Field2", "LOOP_Forbidden Forest" },
};
var sb = new System.Text.StringBuilder();
var template = ma.musicPlaylists.Find(p => p.playlistName == "Field");
foreach (var a in add)
{
    var pl = ma.musicPlaylists.Find(p => p.playlistName == a[0]);
    if (pl == null)
    {
        pl = new DarkTonic.MasterAudio.MasterAudio.Playlist();
        pl.playlistName = a[0];
        pl.songTransitionType = template.songTransitionType;
        pl.crossFadeTime = template.crossFadeTime;
        pl.MusicSettings = new System.Collections.Generic.List<DarkTonic.MasterAudio.MusicSetting>();
        ma.musicPlaylists.Add(pl);
        sb.Append("new playlist " + a[0] + "; ");
    }
    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(dir + a[1] + ".ogg");
    if (clip == null) { sb.Append("missing " + a[1] + "; "); continue; }
    if (pl.MusicSettings.Exists(m => m.clip == clip)) continue;
    var ms = DarkTonic.MasterAudio.MusicSetting.Clone(template.MusicSettings[0], pl);
    ms.clip = clip; ms.songName = clip.name; ms.alias = clip.name; ms.volume = 0.8f;
    pl.MusicSettings.Add(ms);
    sb.Append(a[0] + "+=" + clip.name + "; ");
}
EditorUtility.SetDirty(ma);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ma.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(ma.gameObject.scene);
return sb.ToString();
