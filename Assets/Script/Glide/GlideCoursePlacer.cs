using UnityEngine;

/// <summary>
/// 활공 코스 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, CookingStovePlacer 와 같은 방식).
/// 지금: 2층계 무너진 성벽 초급 코스 1개 — 시작 표지 → 상승 기류 3 → 도착 표지. 지면 높이는 맵이 켜진 뒤 첫 Update 에 레이캐스트로 붙는다.
/// </summary>
public static class GlideCoursePlacer
{
    private const string MapName = "Lv2_BrokenRampart";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MapInfo m in maps)
        {
            if (m.name == MapName)
            {
                Create(m.transform);
                return;
            }
        }
        Debug.LogWarning("[PB][Glide] 맵 없음: " + MapName);
    }

    private static void Create(Transform parent)
    {
        var go = new GameObject("GlideCourse_Beginner");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(0f, 0f, parent.position.z);
        var course = go.AddComponent<GlideCourse>();
        course.Init(new GlideCourse.Spec
        {
            Id = "rampart.beginner",
            Title = "GLIDE COURSE - BEGINNER",
            BaseSeconds = 60f,
            BaseReward = 2,
            StartX = -433f,
            FinishX = -276f,
            YHint = 1f,
            Zones = new[]
            {
                new GlideCourse.Zone { X = -414f, Kind = AirflowKind.UpWeak, Width = 6f, Height = 14f },
                new GlideCourse.Zone { X = -366f, Kind = AirflowKind.UpStrong, Width = 6f, Height = 20f },
                new GlideCourse.Zone { X = -322f, Kind = AirflowKind.UpWeak, Width = 6f, Height = 14f },
            },
        });
    }
}
