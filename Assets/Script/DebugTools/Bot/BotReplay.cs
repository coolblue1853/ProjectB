using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// B0 입력 재생: 사람 jsonl 의 <c>in</c> 행을 같은 상대 실제 시각에 가상 키보드로 다시 넣는다.
/// in 행에는 게임 시간 t 만 있어 실제 시각 u 는 바로 앞 tk 행(20Hz, t·u 둘 다 있음)에서 u + (t - t_tk) 로 잡는다 — 일시정지(timeScale 0) 중에는 t 가 멈춰 게임 시간으로는 재생할 수 없다.
/// 가상 입력 경로가 사람 키 입력과 같은 InputAction 을 같은 순서·간격으로 일으키는지 검증하는 용도다(게임 결과는 달라도 된다).
/// 기록 출처는 "replay"(Logs/playrec_bot/). 비교는 Tools/playrec_replay_cmp.py.
/// </summary>
public sealed class BotReplay : MonoBehaviour
{
    public struct Step
    {
        public float T;
        public string Path;
        public bool Down;
    }

    [Serializable]
    public struct InRow
    {
        public float t;
        public float u;
        public string k;
        public string a;
        public string ph;
        public string dev;
        public float v;
    }

    private static BotReplay _inst;

    private List<Step> _steps;
    private int _next;
    private float _t0;
    private readonly HashSet<string> _touched = new HashSet<string>();
    private BotInput _input;

    public static bool IsRunning => _inst != null;
    public static string LastStatus { get; private set; } = "idle";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _inst = null;
        LastStatus = "idle";
    }

    /// <summary>path 의 [from, from+seconds) 구간을 지금부터 재생한다. 단계 수를 돌려준다(실패 -1)</summary>
    public static int Play(string path, float from, float seconds)
    {
        if (_inst != null || PlayBot.IsRunning || !File.Exists(path))
        {
            return -1;
        }
        List<Step> steps = Build(ReadRows(path, from, from + seconds), InputBindings.Master.asset);
        GameObject go = new GameObject("BotReplay");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<BotReplay>();
        _inst.Begin(steps);
        return steps.Count;
    }

    public static void Stop()
    {
        if (_inst != null)
        {
            BotReplay r = _inst;
            r.Cleanup();
            Destroy(r.gameObject);
        }
    }

    private static List<InRow> ReadRows(string path, float from, float to)
    {
        var rows = new List<InRow>(4096);
        float tkT = 0f, tkU = 0f;
        using (var sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf("\"k\":\"tk\"", StringComparison.Ordinal) >= 0)
                {
                    InRow tk = JsonUtility.FromJson<InRow>(line);
                    tkT = tk.t;
                    tkU = tk.u;
                    continue;
                }
                if (line.IndexOf("\"k\":\"in\"", StringComparison.Ordinal) < 0)
                {
                    continue;
                }
                InRow r = JsonUtility.FromJson<InRow>(line);
                r.u = tkU + Mathf.Clamp(r.t - tkT, 0f, 0.05f);
                if (r.t >= to)
                {
                    break;
                }
                if (r.t >= from)
                {
                    rows.Add(r);
                }
            }
        }
        return rows;
    }

    /// <summary>
    /// 순수 변환: in 행 → 키보드 경로 누름/뗌 단계(T = 첫 행의 실제 시각 u 기준). 버튼 액션 S=누름·C=뗌, 1D 축(Move·horizontalCheck·verticalCheck)은 값 부호로 양쪽 키.
    /// 같은 키를 쓰는 액션이 여럿이면 마지막 단계가 이긴다(같은 프레임에 같이 나온다). 에셋에 없는 액션(Debug Menu 등)·키보드 칸 없는 액션은 건너뛴다.
    /// </summary>
    public static List<Step> Build(List<InRow> rows, InputActionAsset asset)
    {
        var steps = new List<Step>(rows.Count);
        float u0 = rows.Count > 0 ? rows[0].u : 0f;
        foreach (InRow r in rows)
        {
            if (r.dev != "kb" || string.IsNullOrEmpty(r.a) || r.a.StartsWith("Debug", StringComparison.Ordinal))
            {
                continue;
            }
            InputAction act = asset != null ? asset.FindAction(r.a) : null;
            string pos, neg;
            if (act == null || !BotKeyMap.Resolve(act, false, out pos, out neg) || pos == null)
            {
                continue;
            }
            float t = r.u - u0;
            if (neg != null)
            {
                int s = r.ph == "C" ? 0 : r.v > 0.5f ? 1 : r.v < -0.5f ? -1 : 0;
                steps.Add(new Step { T = t, Path = pos, Down = s > 0 });
                steps.Add(new Step { T = t, Path = neg, Down = s < 0 });
            }
            else if (r.ph == "S" || r.ph == "C")
            {
                steps.Add(new Step { T = t, Path = pos, Down = r.ph == "S" });
            }
        }
        return steps;
    }

    /// <summary>시험용: json 한 줄 → 단계 변환 입력</summary>
    public static List<Step> BuildFromLines(IEnumerable<string> lines, InputActionAsset asset)
    {
        var rows = new List<InRow>();
        foreach (string l in lines)
        {
            rows.Add(JsonUtility.FromJson<InRow>(l));
        }
        return Build(rows, asset);
    }

    private void Begin(List<Step> steps)
    {
        _steps = steps;
        _next = 0;
        _t0 = Time.unscaledTime;
        PlayRecorder.Restart("replay");
        _input = new BotInput(null, BotInput.Route.Keyboard, 0) { JitterMax = 0f };
        LastStatus = "running steps=" + steps.Count;
    }

    private void Update()
    {
        if (_input == null)
        {
            return;
        }
        float rel = Time.unscaledTime - _t0;
        _touched.Clear();
        while (_next < _steps.Count && _steps[_next].T <= rel)
        {
            Step s = _steps[_next];
            if (_input.IsPathDown(s.Path) != s.Down && !_touched.Add(s.Path))
            {
                break; // 같은 키를 한 프레임에 눌렀다 떼면 상태가 상쇄된다 — 다음 프레임으로 미룬다
            }
            _input.SetPath(s.Path, s.Down);
            _next++;
        }
        _input.Tick(Time.time);
        if (_next >= _steps.Count)
        {
            Stop();
        }
    }

    /// <summary>정상 종료: 키를 다 떼고 장치 제거, 기록 출처를 사람으로 되돌린다</summary>
    private void Cleanup()
    {
        if (_input == null)
        {
            return;
        }
        LastStatus = "done " + _next + "/" + _steps.Count;
        _input.ReleaseAll();
        Teardown();
        PlayRecorder.Restart("human");
    }

    private void Teardown()
    {
        if (_input != null)
        {
            _input.Dispose();
            _input = null;
        }
        if (_inst == this)
        {
            _inst = null;
        }
    }

    private void OnDestroy()
    {
        Teardown(); // 플레이 종료 중: 장치만 치운다
    }
}
