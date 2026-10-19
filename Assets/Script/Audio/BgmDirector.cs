using DarkTonic.MasterAudio;
using UnityEngine;

/// <summary>
/// 상황별 BGM 전환: 마을 → Town, 사냥터 낮 → Field(2층계 이상 Field2), 밤 → Night, 보스 등장 → Boss.
/// 씬 수정 없이 런타임에 생성된다. 재생목록은 MasterAudio의 musicPlaylists (Town/Field/Night/Boss/Victory).
/// </summary>
public class BgmDirector : MonoBehaviour
{
    const float CheckInterval = 0.5f;
    const string TownMapName = "Town";

    string current;
    bool inTown = true;
    string mapName = "";
    int floor = 1;
    float timer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (FindFirstObjectByType<MasterAudio>() == null || FindFirstObjectByType<BgmDirector>() != null)
        {
            return;
        }
        var go = new GameObject("[BgmDirector]");
        DontDestroyOnLoad(go);
        go.AddComponent<BgmDirector>();
    }

    void OnEnable() => Portal.MapChanged += OnMapChanged;
    void OnDisable() => Portal.MapChanged -= OnMapChanged;

    void Start()
    {
        Transform map = FindPlayerMap();
        inTown = map == null || map.name == TownMapName;
        mapName = map != null ? map.name : "";
        floor = FloorOf(map);
    }

    static int FloorOf(Transform map)
    {
        MapInfo info = map != null ? map.GetComponent<MapInfo>() : null;
        return info != null ? info.floor : 1;
    }

    void OnMapChanged(GameObject openMap)
    {
        inTown = openMap != null && openMap.name == TownMapName;
        mapName = openMap != null ? openMap.name : "";
        floor = FloorOf(openMap != null ? openMap.transform : null);
        timer = 0f; // 즉시 반영
    }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;
        if (timer > 0f)
        {
            return;
        }
        timer = CheckInterval;

        bool night = TimeChange.instance != null && !TimeChange.instance.isDaytime;
        string target = inTown ? "Town"
            : BossNearPlayer() ? "Boss"
            : ZonePlaylist(mapName, night) ?? (night ? "Night" : floor >= 2 ? "Field2" : "Field");
        if (target == current)
        {
            return;
        }
        current = target;
        GameLog.Info(GameLog.Cat.Audio, "BGM → " + target);
        MasterAudio.ChangePlaylistByName(target);
    }

    const float BossMusicRange = 11f; // 화면 반폭(약 10유닛) 남짓 — 보스가 보일 때

    /// <summary>
    /// 보스 곡은 살아 있는 보스가 플레이어 근처(11유닛)에 있을 때만. 보스가 상주하는 구역(깊은 숲·탑 근교·늪 밤)에서
    /// 들어가자마자 보스 곡이 나와 구역 곡이 묻히던 문제(2026-10-19).
    /// </summary>
    static bool BossNearPlayer()
    {
        if (EnemyHealth.ActiveBossCount <= 0 || PlayerController.instance == null)
        {
            return false;
        }
        Vector2 p = PlayerController.instance.transform.position;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e != null && e.isBossMob && e.nowHP > 0 && ((Vector2)e.transform.position - p).sqrMagnitude < BossMusicRange * BossMusicRange)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 구역 전용 재생목록(Tools/probes/setup_bgm.cs 가 만든다). 늪은 밤에도 늪 곡, 깊은 숲·탑 근교는 낮만 전용 곡. 없으면 null.
    /// </summary>
    public static string ZonePlaylist(string map, bool night)
    {
        switch (map)
        {
            case "Lv1_Swamp": return "Swamp";
            case "Lv1_DeepForest": return night ? null : "DeepForest";
            case "Lv1_TowerOutskirts": return night ? null : "Ruins";
            default: return null;
        }
    }

    /// <summary>현재 맵 이름(앰비언트 공유용)</summary>
    public static string CurrentMap
    {
        get
        {
            var d = FindFirstObjectByType<BgmDirector>();
            return d != null ? d.mapName : "";
        }
    }

    /// <summary>시작 시 플레이어가 어느 맵에 있는지: 세로로 쌓인 맵 루트 중 플레이어와 높이가 가장 가까운 활성 맵</summary>
    static Transform FindPlayerMap()
    {
        var map = GameObject.Find("Map");
        var player = PlayerController.instance;
        if (map == null || player == null)
        {
            return null;
        }
        Transform best = null;
        float bestDy = float.MaxValue;
        foreach (Transform m in map.transform)
        {
            if (!m.gameObject.activeSelf)
            {
                continue;
            }
            float dy = Mathf.Abs(m.position.y - player.transform.position.y);
            if (dy < bestDy)
            {
                bestDy = dy;
                best = m;
            }
        }
        return best;
    }

    /// <summary>프로브용</summary>
    public static string CurrentPlaylist
    {
        get
        {
            var d = FindFirstObjectByType<BgmDirector>();
            return d != null ? d.current : null;
        }
    }
}
