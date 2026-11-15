using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

/// <summary>
/// 게임 이벤트 구독과 게임 코드 훅의 기록(k:"ev"·"dec"·"hitat"). PlayRecorder 의 정적 창구가 여기로 넘긴다.
/// - 피격 시도: 공격 쪽이 HitBy(공격자) → PlayerHealthManager.damage2Player 첫 줄 HitAttempt → 방어 판정 뒤 Guarded. 다음 틱에 한 줄로 쓰고 회피 추적(PlayRecThreat)에 넘긴다.
/// - 결정(buy·sell·equip·use·drop·quick·invest): ev 한 줄을 바로 쓰고, 다음 프레임에 전후 골드·스탯을 붙인 k:"dec" 를 쓴다.
/// 게임 로직을 바꾸지 않는다(읽기·구독만).
/// </summary>
public sealed class PlayRecHooks
{
    public const int StatCount = 8;
    private static readonly string[] DecisionKinds = { "buy", "sell", "equip", "use", "drop", "quick", "invest" };

    private struct Attempt
    {
        public int Uid;
        public string Species;
        public int Damage;
        public string Result;
    }

    private struct Decision
    {
        public float T;
        public int Frame;
        public string Kind, A, B;
        public int V;
        public int Gold0;
        public int S0, S1, S2, S3, S4, S5, S6, S7;
    }

    private readonly List<Attempt> _attempts = new List<Attempt>(8);
    private readonly List<Decision> _decisions = new List<Decision>(4);
    private readonly int[] _stats = new int[StatCount];
    private int _gold;
    private GameObject _attacker;
    private string _focus = "";
    private string _ui = "";

    public string Map = "";
    public PlayRecThreat Threat;
    public Action<string> MapChanged;

    private const int LogMax = 300;
    private static readonly string[] DmgKinds = { "basic", "skill", "basic+crit", "skill+crit" };
    private static string[] _catNames;

    // ───────── 정적 줄 쓰기(PlayRecorder 창구가 부른다) ─────────

    /// <summary>k:"ev" 한 줄. a·b 는 null 이면, v 는 int.MinValue 면 생략</summary>
    public static void WriteEvent(string name, string a, int v, string b)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("ev");
                PlayRecJson.KeyStr(sb, "e", name);
                if (a != null)
                {
                    PlayRecJson.KeyStr(sb, "a", a);
                }
                if (v != int.MinValue)
                {
                    PlayRecJson.KeyInt(sb, "v", v);
                }
                if (b != null)
                {
                    PlayRecJson.KeyStr(sb, "b", b);
                }
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    /// <summary>준 피해 ev dmg: kb = 넉백 힘(슈퍼아머면 0), cb = 무기 콤보 단계, hc = 히트 카운터</summary>
    public static void WriteDealt(EnemyHealth target, int amount, bool skill, bool crit, float knock)
    {
        Weapon w = Weapon.Equipped;
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("ev");
                PlayRecJson.KeyStr(sb, "e", "dmg");
                PlayRecJson.KeyStr(sb, "a", target != null ? target.KillMessageId : "");
                PlayRecJson.KeyInt(sb, "v", amount);
                PlayRecJson.KeyStr(sb, "b", DmgKinds[(skill ? 1 : 0) + (crit ? 2 : 0)]);
                PlayRecJson.KeyNum(sb, "kb", knock, 1);
                PlayRecJson.KeyInt(sb, "cb", w != null ? w.nowConboCount : 0);
                PlayRecJson.KeyInt(sb, "hc", CombatHud.Hits);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    /// <summary>GameLog.Info 미러 k:"log"</summary>
    public static void WriteLog(int cat, string message)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                if (_catNames == null)
                {
                    _catNames = Enum.GetNames(typeof(GameLog.Cat));
                }
                StringBuilder sb = PlayRecWriter.Begin("log");
                PlayRecJson.KeyStr(sb, "cat", cat >= 0 && cat < _catNames.Length ? _catNames[cat] : "?");
                sb.Append(",\"msg\":");
                PlayRecJson.AppendTrimmed(sb, message, LogMax);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    // ───────── 구독 ─────────

    public void Subscribe()
    {
        EnemyHealth.Killed += OnKilled;
        LevelSystem.LeveledUp += OnLevelUp;
        Portal.MapChanged += OnMapChanged;
        NpcQuestLog.Accepted += OnQuestAccepted;
        NpcQuestLog.Completed += OnQuestCompleted;
        Skill.Cast += OnSkillCast;
        PickupLog.Picked += OnPicked;
    }

    public void Unsubscribe()
    {
        EnemyHealth.Killed -= OnKilled;
        LevelSystem.LeveledUp -= OnLevelUp;
        Portal.MapChanged -= OnMapChanged;
        NpcQuestLog.Accepted -= OnQuestAccepted;
        NpcQuestLog.Completed -= OnQuestCompleted;
        Skill.Cast -= OnSkillCast;
        PickupLog.Picked -= OnPicked;
    }

    private static void OnKilled(EnemyHealth e)
    {
        PlayRecorder.Event("kill", e != null ? e.KillMessageId : null);
    }

    private static void OnLevelUp(int level)
    {
        PlayRecorder.Event("lvup", null, level);
    }

    private void OnMapChanged(GameObject openMap)
    {
        Map = openMap != null ? openMap.name : "";
        MapChanged?.Invoke(Map);
        PlayRecorder.Event("map", Map);
    }

    private static void OnQuestAccepted(NpcQuestDef q)
    {
        PlayRecorder.Event("qacc", q != null ? q.Id : null);
    }

    private static void OnQuestCompleted(NpcQuestDef q)
    {
        PlayRecorder.Event("qdone", q != null ? q.Id : null);
    }

    private static void OnSkillCast(Skill s)
    {
        PlayRecorder.Event("skill", s != null ? s.skillName : null, s != null ? s.ActiveSlot : -1);
    }

    private static void OnPicked(string item)
    {
        PlayRecorder.Event("pick", item);
    }

    // ───────── 피격 시도 ─────────

    public void HitBy(GameObject attacker)
    {
        _attacker = attacker;
    }

    public void HitMiss(GameObject attacker)
    {
        _attacker = null;
        AddAttempt(attacker, 0, "miss");
    }

    public void HitAttempt(int damage, bool hurtInvuln, bool invincible)
    {
        GameObject src = _attacker;
        _attacker = null;
        AddAttempt(src, damage, hurtInvuln ? "iframe" : invincible ? "immune" : "hit");
    }

    public void Guarded(bool guarded, bool parried)
    {
        if (!guarded || _attempts.Count == 0)
        {
            return;
        }
        Attempt a = _attempts[_attempts.Count - 1];
        a.Result = parried ? "parry" : "guard";
        _attempts[_attempts.Count - 1] = a;
    }

    private void AddAttempt(GameObject src, int damage, string result)
    {
        if (_attempts.Count >= 32)
        {
            return; // 한 틱에 이렇게 많으면 버린다(정상 플레이에서는 생기지 않음)
        }
        EnemyHealth eh = src != null ? src.GetComponent<EnemyHealth>() : null; // 피격 순간에만(프레임마다 아님)
        _attempts.Add(new Attempt
        {
            Uid = eh != null ? RuntimeHelpers.GetHashCode(eh) : 0,
            Species = eh != null ? eh.KillMessageId : (src != null ? src.name : "env"),
            Damage = damage,
            Result = result,
        });
    }

    // ───────── 결정 ─────────

    /// <summary>ev 를 쓴 뒤 PlayRecorder.Event 가 부른다. 결정 종류면 전 값을 잡아 둔다.</summary>
    public void OnEvent(string name, string a, int v, string b)
    {
        if (Array.IndexOf(DecisionKinds, name) < 0 || _decisions.Count >= 16)
        {
            return;
        }
        _decisions.Add(new Decision
        {
            T = Time.time,
            Frame = Time.frameCount,
            Kind = name,
            A = a,
            B = b,
            V = v,
            Gold0 = _gold,
            S0 = _stats[0], S1 = _stats[1], S2 = _stats[2], S3 = _stats[3],
            S4 = _stats[4], S5 = _stats[5], S6 = _stats[6], S7 = _stats[7],
        });
    }

    /// <summary>틱마다: 전후 비교용 스탯·골드·포커스를 잡아 둔다(결정 직전 값이 된다).</summary>
    public void CacheStats(string focus, string ui)
    {
        _gold = DatabaseManager.money;
        ReadStats(_stats);
        _focus = focus ?? "";
        _ui = ui ?? "";
    }

    /// <summary>[기본피해, 방어, 치명률, 치명피해, 피해증가, 이동속도, 공격속도, 최대HP]</summary>
    public static void ReadStats(int[] into)
    {
        into[0] = DatabaseManager.addbasicDmg;
        into[1] = DatabaseManager.playerDef;
        into[2] = DatabaseManager.playerCritRate;
        into[3] = DatabaseManager.playerCritDmgRate;
        into[4] = DatabaseManager.incDmg;
        into[5] = DatabaseManager.SpeedBuff;
        into[6] = DatabaseManager.attackSpeedBuff;
        PlayerHealthManager hm = PlayerHealthManager.Instance;
        into[7] = hm != null ? hm.fullHP : 0;
    }

    // ───────── 틱에서 쓰기 ─────────

    /// <summary>모인 피격 시도·결정을 쓴다(메인 스레드, 매 프레임).</summary>
    public void Flush(int[] scratch)
    {
        if (_attempts.Count > 0)
        {
            for (int i = 0; i < _attempts.Count; i++)
            {
                Attempt at = _attempts[i];
                WriteAttempt(at);
                if (Threat != null && at.Uid != 0)
                {
                    Threat.Attempt(at.Uid, at.Result, Time.time);
                }
            }
            _attempts.Clear();
        }
        if (_decisions.Count > 0)
        {
            // 결정이 난 프레임의 처리(스탯 재계산 등)가 끝난 다음 프레임에 후 값을 읽는다
            ReadStats(scratch);
            int frame = Time.frameCount;
            for (int i = _decisions.Count - 1; i >= 0; i--)
            {
                if (_decisions[i].Frame < frame)
                {
                    WriteDecision(_decisions[i], scratch);
                    _decisions.RemoveAt(i);
                }
            }
        }
    }

    private static void WriteAttempt(Attempt at)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("hitat");
                PlayRecJson.KeyStr(sb, "a", at.Species);
                PlayRecJson.KeyInt(sb, "id", at.Uid);
                PlayRecJson.KeyInt(sb, "v", at.Damage);
                PlayRecJson.KeyStr(sb, "r", at.Result);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private void WriteDecision(Decision d, int[] now)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("dec");
                PlayRecJson.KeyNum(sb, "t0", d.T, 3);
                PlayRecJson.KeyStr(sb, "d", d.Kind);
                if (d.A != null)
                {
                    PlayRecJson.KeyStr(sb, "a", d.A);
                }
                if (d.V != int.MinValue)
                {
                    PlayRecJson.KeyInt(sb, "v", d.V);
                }
                if (d.B != null)
                {
                    PlayRecJson.KeyStr(sb, "b", d.B);
                }
                PlayRecJson.KeyInt(sb, "g0", d.Gold0);
                PlayRecJson.KeyInt(sb, "g1", DatabaseManager.money);
                sb.Append(",\"s0\":[");
                AppendInts(sb, d.S0, d.S1, d.S2, d.S3, d.S4, d.S5, d.S6, d.S7);
                sb.Append("],\"s1\":[");
                AppendInts(sb, now[0], now[1], now[2], now[3], now[4], now[5], now[6], now[7]);
                sb.Append(']');
                PlayRecJson.KeyStr(sb, "ui", _ui);
                PlayRecJson.KeyStr(sb, "foc", _focus);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private static void AppendInts(StringBuilder sb, int a, int b, int c, int d, int e, int f, int g, int h)
    {
        PlayRecJson.AppendLong(sb, a).Append(',');
        PlayRecJson.AppendLong(sb, b).Append(',');
        PlayRecJson.AppendLong(sb, c).Append(',');
        PlayRecJson.AppendLong(sb, d).Append(',');
        PlayRecJson.AppendLong(sb, e).Append(',');
        PlayRecJson.AppendLong(sb, f).Append(',');
        PlayRecJson.AppendLong(sb, g).Append(',');
        PlayRecJson.AppendLong(sb, h);
    }
}
