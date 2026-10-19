using DarkTonic.MasterAudio;
using UnityEngine;

/// <summary>
/// 구역·낮밤 앰비언트 (AssetIntegrationPlan 6절 4). BGM 아래에 자연음을 깔아 공간감을 준다.
/// 숲: 낮 새·개울 / 밤 풀벌레, 늪: 개구리·새 / 개구리·귀뚜라미, 성채·근교·2층계: 낮은 바람, 마을: 풍경 소리.
/// AudioSource 두 개를 번갈아 크로스페이드한다. 볼륨은 설정의 마스터 볼륨(MasterAudio.MasterVolumeLevel)을 따른다.
/// 클립: Resources/Ambience (보유 팩 SoundBits·Pro Sound Collection 에서 Tools/extract_audio.py 로 추출).
/// </summary>
public class AmbienceDirector : MonoBehaviour
{
    const float CheckInterval = 1f;
    const float FadeTime = 2.5f;
    const float BaseVolume = 0.32f;

    private AudioSource[] _src;
    private int _active;
    private string _current;
    private float _timer;
    private float _fade = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (FindFirstObjectByType<AmbienceDirector>() != null || FindFirstObjectByType<MasterAudio>() == null)
        {
            return;
        }
        var go = new GameObject("[AmbienceDirector]");
        DontDestroyOnLoad(go);
        go.AddComponent<AmbienceDirector>();
    }

    void Awake()
    {
        _src = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            _src[i] = gameObject.AddComponent<AudioSource>();
            _src[i].loop = true;
            _src[i].playOnAwake = false;
            _src[i].spatialBlend = 0f;
            _src[i].volume = 0f;
        }
    }

    /// <summary>맵·낮밤 → 앰비언트 클립 이름(Resources/Ambience). 빈 문자열이면 무음.</summary>
    public static string ClipFor(string map, bool night)
    {
        switch (map)
        {
            case "":
                return "";
            case "Town":
                return "background_wind_chimes_loop";
            case "Lv1_Swamp":
                return night ? "swamp_night_frogs_crickets_ducks_loop_02" : "swamp_bayou_frogs_birds_daytime_loop2";
            case "Lv1_BanditFortress":
            case "Lv1_TowerOutskirts":
            case "Lv2_MagicCliffs":
            case "Lv2_BrokenRampart":
                return "wind_general_soft_low_loop_04";
            default:
                return night ? "river_stream_night_flowing_water_insects_loop_02_long" : "ca12_forest_birds_small-stream";
        }
    }

    void Update()
    {
        _timer -= Time.unscaledDeltaTime;
        if (_timer <= 0f)
        {
            _timer = CheckInterval;
            bool night = TimeChange.instance != null && !TimeChange.instance.isDaytime;
            string want = ClipFor(BgmDirector.CurrentMap, night);
            if (want != _current)
            {
                SwitchTo(want);
            }
        }
        float master = MasterAudio.SafeInstance != null ? MasterAudio.MasterVolumeLevel : 1f;
        _fade = Mathf.MoveTowards(_fade, 1f, Time.unscaledDeltaTime / FadeTime);
        _src[_active].volume = BaseVolume * master * _fade;
        AudioSource other = _src[1 - _active];
        other.volume = BaseVolume * master * (1f - _fade);
        if (other.isPlaying && _fade >= 1f)
        {
            other.Stop();
        }
    }

    void SwitchTo(string clipName)
    {
        _current = clipName;
        _active = 1 - _active;
        _fade = 0f;
        AudioSource src = _src[_active];
        src.Stop();
        src.clip = string.IsNullOrEmpty(clipName) ? null : Resources.Load<AudioClip>("Ambience/" + clipName);
        if (src.clip != null)
        {
            src.Play();
            // 스트리밍 클립은 재생 전에 time 을 넣으면 재생이 안 된다 → 재생 후 시작 위치를 흩뜨린다
            src.timeSamples = Random.Range(0, Mathf.Max(1, src.clip.samples * 4 / 5));
        }
        GameLog.Info(GameLog.Cat.Audio, "앰비언트 → " + (string.IsNullOrEmpty(clipName) ? "(없음)" : clipName));
    }

    /// <summary>프로브용: 현재 클립 이름과 재생 여부</summary>
    public static string Current
    {
        get
        {
            var d = FindFirstObjectByType<AmbienceDirector>();
            if (d == null)
            {
                return null;
            }
            AudioSource s = d._src[d._active];
            return d._current + " playing=" + s.isPlaying + " vol=" + s.volume.ToString("F2");
        }
    }
}
