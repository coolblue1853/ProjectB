using System.Text;
using DG.Tweening;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// 개발용 런타임 감시기 (에디터·개발 빌드에서만 자동 생성).
/// - 성능: 프레임 시간 스파이크, 프레임당 GC 할당(ProfilerRecorder), 활성 트윈 수 증가(누수 의심)
/// - 논리 이상: 플레이어 좌표 NaN/맵 밖 추락, 상태 잠금 토큰이 오래 유지됨, 활성 몬스터 수 폭증
/// 5초마다 요약을 [PB][Monitor]로 남기고, 이상은 GameLog.Warn/Anomaly로 즉시 남긴다.
/// </summary>
public class RuntimeMonitor : MonoBehaviour
{
    const float SummaryInterval = 5f;
    const float SpikeMs = 50f;            // 이 이상이면 스파이크
    const long GcSpikeBytes = 64 * 1024;  // 프레임당 64KB 이상 할당
    const float FallY = -260f; // Lv2 바닥(y≈-188) 아래
    const float StuckTokenSeconds = 12f; // 사망(2.5s)+부활 무적(2s)이 연달아도 오탐 없게
    const int MaxEnemies = 120;

    ProfilerRecorder gcAllocRecorder;
    ProfilerRecorder mainThreadRecorder;

    float summaryTimer;
    int frames;
    float worstMs;
    long gcTotal, gcWorst;
    int spikes, gcSpikes;
    int lastTweens = -1;
    int tweenGrowthStreak;
    readonly float[] tokenSince = new float[System.Enum.GetValues(typeof(StatusFlag)).Length];

    public static RuntimeMonitor Instance { get; private set; }
    public static int AnomalyCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
        {
            return;
        }
        var go = new GameObject("[RuntimeMonitor]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<RuntimeMonitor>();
        AnomalyCount = 0;
    }

    void OnStatusChanged(StatusFlag flag, bool on)
    {
        GameLog.Info(GameLog.Cat.Player, "상태 " + flag + (on ? " ON" : " OFF"));
    }

    void OnEnable()
    {
        PlayerStatus.Changed += OnStatusChanged;
        gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
    }

    void OnDisable()
    {
        PlayerStatus.Changed -= OnStatusChanged;
        gcAllocRecorder.Dispose();
        mainThreadRecorder.Dispose();
    }

    void Update()
    {
        float ms = Time.unscaledDeltaTime * 1000f;
        frames++;
        if (ms > worstMs)
        {
            worstMs = ms;
        }
        if (ms > SpikeMs && Time.frameCount > 120)
        {
            spikes++;
        }

        long gc = gcAllocRecorder.Valid ? gcAllocRecorder.LastValue : 0;
        gcTotal += gc;
        if (gc > gcWorst)
        {
            gcWorst = gc;
        }
        if (gc > GcSpikeBytes && Time.frameCount > 120)
        {
            gcSpikes++;
        }

        CheckPlayer();
        CheckTokens();

        summaryTimer += Time.unscaledDeltaTime;
        if (summaryTimer >= SummaryInterval)
        {
            Summarize();
            summaryTimer = 0f;
        }
    }

    void CheckPlayer()
    {
        var pc = PlayerController.instance;
        if (pc == null)
        {
            return;
        }
        Vector3 p = pc.transform.position;
        if (float.IsNaN(p.x) || float.IsNaN(p.y))
        {
            Report(GameLog.Cat.Player, "플레이어 좌표 NaN");
        }
        else if (p.y < FallY)
        {
            Report(GameLog.Cat.Player, "플레이어가 맵 밖으로 떨어짐 y=" + p.y.ToString("F1"));
        }
    }

    void CheckTokens()
    {
        float now = Time.time;
        for (int i = 0; i < tokenSince.Length; i++)
        {
            var flag = (StatusFlag)i;
            if (flag == StatusFlag.InputLock)
            {
                continue; // UI를 오래 열어두는 건 정상
            }
            if (!PlayerStatus.Has(flag))
            {
                tokenSince[i] = 0f;
                continue;
            }
            if (tokenSince[i] == 0f)
            {
                tokenSince[i] = now;
            }
            else if (now - tokenSince[i] > StuckTokenSeconds)
            {
                Report(GameLog.Cat.Player, flag + " 잠금이 " + StuckTokenSeconds + "초 넘게 유지됨: " + PlayerStatus.Dump().Replace("\n", " | "));
                tokenSince[i] = now; // 같은 경고 반복 억제
            }
        }
    }

    void Summarize()
    {
        int tweens = DOTween.TotalPlayingTweens();
        if (lastTweens >= 0 && tweens > lastTweens + 20)
        {
            tweenGrowthStreak++;
            if (tweenGrowthStreak >= 3)
            {
                GameLog.Warn(GameLog.Cat.Perf, "활성 트윈 수가 계속 증가(누수 의심): " + lastTweens + " → " + tweens);
            }
        }
        else
        {
            tweenGrowthStreak = 0;
        }
        lastTweens = tweens;

        int enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        if (enemies > MaxEnemies)
        {
            Report(GameLog.Cat.Spawn, "활성 몬스터 수 과다: " + enemies);
        }

        var sb = new StringBuilder(160);
        sb.Append("fps=").Append((frames / SummaryInterval).ToString("F0"))
          .Append(" worst=").Append(worstMs.ToString("F1")).Append("ms")
          .Append(" spikes=").Append(spikes)
          .Append(" gc/frame avg=").Append(frames > 0 ? (gcTotal / frames / 1024f).ToString("F1") : "0").Append("KB")
          .Append(" worst=").Append((gcWorst / 1024f).ToString("F1")).Append("KB")
          .Append(" gcSpikes=").Append(gcSpikes)
          .Append(" tweens=").Append(tweens)
          .Append(" enemies=").Append(enemies)
          .Append(" anomalies=").Append(AnomalyCount);
        GameLog.Info(GameLog.Cat.Monitor, sb.ToString());
        LastSummary = sb.ToString();

        frames = 0;
        worstMs = 0f;
        gcTotal = 0;
        gcWorst = 0;
        spikes = 0;
        gcSpikes = 0;
    }

    void Report(GameLog.Cat cat, string message)
    {
        AnomalyCount++;
        GameLog.Anomaly(cat, message);
    }

    /// <summary>프로브용 최근 요약</summary>
    public static string LastSummary { get; private set; } = "";
}
