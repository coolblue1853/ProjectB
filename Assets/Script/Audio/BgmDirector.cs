using DarkTonic.MasterAudio;
using UnityEngine;

/// <summary>
/// 상황별 BGM 전환: 마을 → Town, 사냥터 낮 → Field, 밤 → Night, 보스 등장 → Boss.
/// 씬 수정 없이 런타임에 생성된다. 재생목록은 MasterAudio의 musicPlaylists (Town/Field/Night/Boss/Victory).
/// </summary>
public class BgmDirector : MonoBehaviour
{
    const float CheckInterval = 0.5f;
    const string TownMapName = "Town";

    string current;
    bool inTown = true;
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
        inTown = IsPlayerInTown();
    }

    void OnMapChanged(GameObject openMap)
    {
        inTown = openMap != null && openMap.name == TownMapName;
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

        string target = inTown ? "Town"
            : EnemyHealth.ActiveBossCount > 0 ? "Boss"
            : TimeChange.instance != null && !TimeChange.instance.isDaytime ? "Night"
            : "Field";
        if (target == current)
        {
            return;
        }
        current = target;
        GameLog.Info(GameLog.Cat.Audio, "BGM → " + target);
        MasterAudio.ChangePlaylistByName(target);
    }

    /// <summary>시작 시 플레이어가 어느 맵에 있는지: 세로로 쌓인 맵 루트 중 플레이어와 높이가 가장 가까운 활성 맵</summary>
    static bool IsPlayerInTown()
    {
        var map = GameObject.Find("Map");
        var player = PlayerController.instance;
        if (map == null || player == null)
        {
            return true;
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
        return best == null || best.name == TownMapName;
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
