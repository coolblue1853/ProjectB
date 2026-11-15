using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 회피 반응(k:"av")·사람 신호(k:"hs") 기록. 판정은 순수 로직(PlayRecThreat·PlayRecHuman)이 하고, 여기서는 관측·입력을 넘기고 줄을 쓴다.
/// 피격 시도 결과는 PlayRecHooks 가 Threat.Attempt 로 바로 넘긴다.
/// </summary>
public sealed class PlayRecSignals
{
    public readonly PlayRecThreat Threat = new PlayRecThreat();
    private readonly PlayRecHuman _human = new PlayRecHuman();
    private readonly List<PlayRecThreat.Window> _closed = new List<PlayRecThreat.Window>(8);
    private readonly List<string> _uiPrev = new List<string>(8);

    public void Reset(float real)
    {
        _human.Reset(real);
        Threat.Clear();
        _uiPrev.Clear();
    }

    /// <summary>틱마다: 가까운 적 상태를 넘기고 닫힌 회피 창을 쓴다.</summary>
    public void FeedThreat(PlayObsEnemies f, float now)
    {
        for (int i = 0; i < f.FoeCount; i++)
        {
            PlayObsEnemies.Foe e = f.Foes[i];
            Threat.Enemy(e.Uid, e.Id, e.State, e.Dx, e.Dy, now);
        }
        if (Threat.Collect(now, _closed) == 0)
        {
            return;
        }
        for (int i = 0; i < _closed.Count; i++)
        {
            WriteWindow(_closed[i]);
        }
        Threat.Recycle(_closed);
    }

    private static void WriteWindow(PlayRecThreat.Window w)
    {
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("av");
            PlayRecJson.KeyStr(sb, "a", w.Species);
            PlayRecJson.KeyInt(sb, "id", w.Id);
            PlayRecJson.KeyNum(sb, "t0", w.T0, 3);
            PlayRecJson.KeyNum(sb, "dur", w.LastActive - w.T0, 2);
            PlayRecJson.KeyNum(sb, "dx0", w.Dx0, 2);
            PlayRecJson.KeyStr(sb, "res", w.Result);
            PlayRecJson.KeyStr(sb, "r", PlayRecThreat.InputName(w.React));
            PlayRecJson.KeyNum(sb, "rt", w.ReactAt >= 0f ? w.ReactAt - w.T0 : -1f, 2);
            PlayRecJson.KeyInt(sb, "in", w.Inputs);
            PlayRecJson.KeyInt(sb, "atk", w.AttackSeen ? 1 : 0);
            PlayRecJson.KeyInt(sb, "tele", w.Tele ? 1 : 0);
            PlayRecWriter.Commit();
        }
    }

    /// <summary>버튼 시작·이동 방향 변화(PlayRecInput.Pressed). now = 게임 시간, real = 실제 시간, x = 플레이어 x</summary>
    public void OnPressed(string action, int moveSign, float now, float real, float x)
    {
        PlayRecHuman.Signal s;
        if (_human.OnActivity(real, out s))
        {
            Write(s);
        }
        if (moveSign != 0)
        {
            Threat.Input(0, moveSign, now);
            if (_human.OnMoveSign(moveSign, real, x, out s))
            {
                Write(s);
            }
            return;
        }
        int bit = ThreatBit(action);
        if (bit != 0)
        {
            Threat.Input(bit, 0, now);
        }
        if (_human.OnPress(action, real, out s))
        {
            Write(s);
        }
    }

    public static int ThreatBit(string action)
    {
        switch (action)
        {
            case "Player/Jump": return PlayRecThreat.InJump;
            case "Player/Dash": return PlayRecThreat.InDash;
            case "Player/Guard": return PlayRecThreat.InGuard;
            case "Player/Attack": return PlayRecThreat.InAttack;
            default:
                return action.StartsWith("Player/Skill", StringComparison.Ordinal) ? PlayRecThreat.InAttack : 0;
        }
    }

    /// <summary>틱마다: 무입력·일시정지·창 바로 닫기</summary>
    public void FeedHuman(float real, bool anyHeld, bool paused, IReadOnlyList<string> ui)
    {
        PlayRecHuman.Signal s;
        if (_human.Tick(real, anyHeld, out s))
        {
            Write(s);
        }
        if (_human.OnPause(paused, real, out s))
        {
            Write(s);
        }
        for (int i = 0; i < ui.Count; i++)
        {
            if (!_uiPrev.Contains(ui[i]))
            {
                _human.OnWindow(ui[i], true, real, out s);
            }
        }
        for (int i = 0; i < _uiPrev.Count; i++)
        {
            if (!Contains(ui, _uiPrev[i]) && _human.OnWindow(_uiPrev[i], false, real, out s))
            {
                Write(s);
            }
        }
        _uiPrev.Clear();
        for (int i = 0; i < ui.Count; i++)
        {
            _uiPrev.Add(ui[i]);
        }
    }

    private static bool Contains(IReadOnlyList<string> list, string v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == v)
            {
                return true;
            }
        }
        return false;
    }

    private static void Write(PlayRecHuman.Signal s)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("hs");
                PlayRecJson.KeyStr(sb, "s", PlayRecHuman.Name(s.Kind));
                if (s.A != null)
                {
                    PlayRecJson.KeyStr(sb, "a", s.A);
                }
                PlayRecJson.KeyNum(sb, "u0", s.T, 3);
                PlayRecJson.KeyNum(sb, "dur", s.Dur, 2);
                if (s.N > 0)
                {
                    PlayRecJson.KeyInt(sb, "n", s.N);
                }
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard(); // 입력 콜백에서도 불리므로 반쯤 쓴 줄을 남기지 않는다
            }
        }
    }
}
