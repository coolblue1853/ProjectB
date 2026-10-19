// 늪·마법 절벽 배경 재배치(ArtStyle 기존 강화 2026-10-19). 카메라가 y −182.6~−171.4 를 보는데 구름 윗부분이 화면 위로 빠져 있었고,
// 하늘(19유닛)을 40유닛으로 세로 타일링해 이음매 수평선이 보였다. 구름 윗부분을 화면 상단에, 하늘은 원본 높이로 그라데이션이 걸치게 둔다.
// 에디트 모드 실행 → 씬 저장. 재실행 안전(절대값 지정)
var sb = new System.Text.StringBuilder();
foreach (var mapName in new[]{ "Lv1_Swamp", "Lv2_MagicCliffs" })
{
    var map = GameObject.Find("Map").transform.Find(mapName); var bg = map.Find("Background");
    Color tint = mapName == "Lv1_Swamp" ? new Color(0.50f, 0.60f, 0.60f) : Color.white;
    foreach (var sr in bg.GetComponentsInChildren<SpriteRenderer>(true))
    {
        Undo.RecordObject(sr, "bg"); Undo.RecordObject(sr.transform, "bg");
        var p = sr.transform.position; float h = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        if (sr.name == "sky") { sr.size = new Vector2(320f, h); p.y = -174.5f; }
        else if (sr.name == "clouds") { sr.size = new Vector2(320f, h); p.y = -179.6f; }
        else if (sr.name == "far-grounds") { sr.size = new Vector2(320f, h); p.y = -181f + h * 0.5f; } // 7 로 잡으면 아래 2px 가 위에 반복돼 수평선이 생겼다
        else if (sr.name == "sea") { sr.size = new Vector2(320f, h); }
        sr.transform.position = p; sr.color = tint;
        sb.Append(mapName + "/" + sr.name + " y=" + p.y + " h=" + sr.size.y + "; ");
    }
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
return sb.ToString();
