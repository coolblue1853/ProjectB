using System.Collections.Generic;

/// <summary>
/// 공격 예고 → 반응 추적 — 순수 로직(EditMode 테스트 대상). 회피 습관 학습용 k:"av" 한 줄의 재료를 만든다.
/// 창 열기: 적 상태가 예고(Tele) 또는 공격(Attack)으로 들어오고 가로 OpenRangeX·세로 OpenRangeY 안. 같은 적은 창 하나만.
/// 예고 없이 바로 공격하는 몹(BT 가 isAttack 과 판정을 같은 프레임에 켬)은 공격 시작에서 열리고 Tele=false 다.
/// 창 동안: 플레이어 입력(점프·대시·후퇴·전진·방어·공격)의 첫 반응 시각과 종류, 누른 입력 비트를 모은다.
/// 이 적의 공격 시도(피격 훅)가 오면 결과를 남긴다(hit > guard > parry > immune > iframe > miss). 창이 열리기 직전(같은 프레임~RecentGrace 초 전)
/// 시도는 잠시 보관했다가 창이 열릴 때 붙인다(훅은 매 프레임, 관측은 20Hz 라 공격 첫 프레임의 시도가 창보다 먼저 온다).
/// 창 닫기: 예고·공격 상태가 EndGrace 초 넘게 안 보이거나 MaxWindow 초가 지남. 결과가 없으면 "none"(맞지 않음).
/// </summary>
public sealed class PlayRecThreat
{
    public const float OpenRangeX = 8f;
    public const float OpenRangeY = 4f;
    public const float EndGrace = 0.6f;
    public const float MaxWindow = 3f;
    public const int MaxOpen = 8;
    public const float RecentGrace = 0.3f;
    private const int RecentMax = 16;

    public const int StIdle = 0;
    public const int StChase = 1;
    public const int StTele = 2;
    public const int StAttack = 3;
    public const int StHit = 4;
    public const int StDead = 5;

    public const int InJump = 1;
    public const int InDash = 2;
    public const int InBack = 4;
    public const int InFwd = 8;
    public const int InGuard = 16;
    public const int InAttack = 32;

    public sealed class Window
    {
        public int Id;
        public string Species;
        public float T0;
        public float Dx0;
        public float LastActive;
        public float ReactAt = -1f;
        public int React;
        public int Inputs;
        public string Result;
        public bool AttackSeen;
        /// <summary>예고 상태에서 열렸나(false = 공격 시작에서 열림)</summary>
        public bool Tele;
    }

    private struct Recent
    {
        public int Id;
        public string Result;
        public float T;
    }

    private readonly List<Window> _open = new List<Window>(MaxOpen);
    private readonly Stack<Window> _pool = new Stack<Window>(MaxOpen);
    private readonly List<Recent> _recent = new List<Recent>(RecentMax);

    public int OpenCount => _open.Count;

    public void Clear()
    {
        for (int i = 0; i < _open.Count; i++)
        {
            _pool.Push(_open[i]);
        }
        _open.Clear();
        _recent.Clear();
    }

    /// <summary>틱마다 가까운 적 하나씩. dx = 적 x - 플레이어 x.</summary>
    public void Enemy(int id, string species, int state, float dx, float dy, float t)
    {
        Window w = Find(id);
        bool active = state == StTele || state == StAttack;
        if (w != null)
        {
            if (active)
            {
                w.LastActive = t;
                w.AttackSeen |= state == StAttack;
            }
            return;
        }
        if (!active || _open.Count >= MaxOpen || Abs(dx) > OpenRangeX || Abs(dy) > OpenRangeY)
        {
            return;
        }
        w = _pool.Count > 0 ? _pool.Pop() : new Window();
        w.Id = id;
        w.Species = species;
        w.T0 = t;
        w.Dx0 = dx;
        w.LastActive = t;
        w.ReactAt = -1f;
        w.React = 0;
        w.Inputs = 0;
        w.Result = null;
        w.AttackSeen = state == StAttack;
        w.Tele = state == StTele;
        _open.Add(w);
        TakeRecent(w, t);
    }

    /// <summary>플레이어 입력 한 번. moveSign 은 이동 입력일 때 방향(-1/+1), 아니면 0.</summary>
    public void Input(int bit, int moveSign, float t)
    {
        for (int i = 0; i < _open.Count; i++)
        {
            Window w = _open[i];
            int b = bit;
            if (moveSign != 0)
            {
                bool away = (w.Dx0 > 0f && moveSign < 0) || (w.Dx0 < 0f && moveSign > 0);
                b = away ? InBack : InFwd;
            }
            w.Inputs |= b;
            if (w.ReactAt < 0f)
            {
                w.ReactAt = t;
                w.React = b;
            }
        }
    }

    /// <summary>attackerId 의 공격 시도 결과(hit·guard·parry·immune·iframe·miss).</summary>
    public void Attempt(int attackerId, string result, float t)
    {
        Window w = Find(attackerId);
        if (w == null)
        {
            if (_recent.Count >= RecentMax)
            {
                _recent.RemoveAt(0);
            }
            _recent.Add(new Recent { Id = attackerId, Result = result, T = t });
            return;
        }
        if (Rank(result) > Rank(w.Result))
        {
            w.Result = result;
        }
    }

    /// <summary>닫을 창을 closed 에 옮긴다(호출자는 다 쓴 뒤 Recycle). 닫힌 수를 돌려준다.</summary>
    public int Collect(float t, List<Window> closed)
    {
        int n = 0;
        for (int i = _open.Count - 1; i >= 0; i--)
        {
            Window w = _open[i];
            if (t - w.LastActive > EndGrace || t - w.T0 > MaxWindow)
            {
                if (w.Result == null)
                {
                    w.Result = "none";
                }
                _open.RemoveAt(i);
                closed.Add(w);
                n++;
            }
        }
        return n;
    }

    public void Recycle(List<Window> closed)
    {
        for (int i = 0; i < closed.Count; i++)
        {
            _pool.Push(closed[i]);
        }
        closed.Clear();
    }

    public static string InputName(int bit)
    {
        switch (bit)
        {
            case InJump: return "jump";
            case InDash: return "dash";
            case InBack: return "back";
            case InFwd: return "fwd";
            case InGuard: return "guard";
            case InAttack: return "atk";
            default: return "none";
        }
    }

    /// <summary>창이 열리기 직전 RecentGrace 초 안의 같은 적 시도를 붙이고, 오래된 보관분은 버린다.</summary>
    private void TakeRecent(Window w, float t)
    {
        for (int i = _recent.Count - 1; i >= 0; i--)
        {
            Recent r = _recent[i];
            if (t - r.T > RecentGrace)
            {
                _recent.RemoveAt(i);
            }
            else if (r.Id == w.Id)
            {
                if (Rank(r.Result) > Rank(w.Result))
                {
                    w.Result = r.Result;
                }
                _recent.RemoveAt(i);
            }
        }
    }

    private Window Find(int id)
    {
        for (int i = 0; i < _open.Count; i++)
        {
            if (_open[i].Id == id)
            {
                return _open[i];
            }
        }
        return null;
    }

    private static int Rank(string r)
    {
        switch (r)
        {
            case "hit": return 6;
            case "guard": return 5;
            case "parry": return 4;
            case "immune": return 3;
            case "iframe": return 2;
            case "miss": return 1;
            default: return 0;
        }
    }

    private static float Abs(float v)
    {
        return v < 0f ? -v : v;
    }
}
