using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 적·투사체 관측(읽기 전용, 봇 재사용). 가까운 적 Count 마리의 종·위치·속도·HP%·상태·공격 판정·거리, 가까운 투사체 ProjMax 개.
/// 상태는 실제 몹 코드의 신호만 쓴다(추측 없음):
///  - 사망: EnemyHealth.ObsDead / 피격: stiffShakeTime > 0 또는 isStun(덫·기절)
///  - 공격 중: 이 몹이 소유한 근접 판정(EnemyDamageObject, 발사형 아님)이 살아 있음, TempMobDash 돌진, 또는 같은 isAttack 구간에서 판정이 이미 나온 뒤(후딜)
///  - 공격 예고: Animator isAttack 이 켜졌고 아직 판정 전(BT 준비 대기), TempMobDash 웅크림, TempMobShooter 조준(Windup)
///  - 추적: EnemyHealth.ChaseLatched / 대기: 그 밖
/// 속도는 위치 변화량(트윈 이동도 잡힌다). 상태 신호 종류(Sig): 1 isAttack 애니, 2 돌진, 4 사격. 0 = 예고 신호 없음(몸박 전용 등).
/// </summary>
public sealed class PlayObsEnemies
{
    public const int Count = 6;
    public const int ProjMax = 4;
    public const int SigAnim = 1;
    public const int SigDash = 2;
    public const int SigShoot = 4;
    private const float MemForgetSeconds = 10f;

    public struct Foe
    {
        public int Uid;
        public string Id;
        public float Dx, Dy, Vx, Vy, Dist;
        public int HpPercent;
        public int State;
        public bool Hitbox;
        public int Sig;
    }

    public struct Proj
    {
        public float Dx, Dy, Vx, Vy;
    }

    private sealed class Mem
    {
        public float X, Y, T, Vx, Vy, Seen;
        public bool SpanHit;
    }

    public readonly Foe[] Foes = new Foe[Count];
    public int FoeCount;
    public readonly Proj[] Projs = new Proj[ProjMax];
    public int ProjCount;
    public int ActiveTotal;

    private readonly EnemyHealth[] _near = new EnemyHealth[Count];
    private readonly float[] _nearD = new float[Count];
    private readonly float[] _projD = new float[ProjMax];
    private readonly HashSet<int> _hitboxOwners = new HashSet<int>();
    private readonly Dictionary<int, Mem> _mem = new Dictionary<int, Mem>();
    private readonly List<int> _forget = new List<int>();
    private float _nextForget;

    public void Clear()
    {
        FoeCount = 0;
        ProjCount = 0;
        ActiveTotal = 0;
    }

    public void Observe(Vector3 p, float t)
    {
        ScanDamageObjects(p, t);
        PickNearest(p);
        for (int i = 0; i < Count; i++)
        {
            EnemyHealth e = _near[i];
            if (e == null)
            {
                break;
            }
            Fill(ref Foes[i], e, p, t);
            FoeCount = i + 1;
            _near[i] = null; // 참조를 오래 쥐지 않는다
        }
        if (t >= _nextForget)
        {
            _nextForget = t + 5f;
            Forget(t);
        }
    }

    private void PickNearest(Vector3 p)
    {
        FoeCount = 0;
        ActiveTotal = 0;
        for (int i = 0; i < Count; i++)
        {
            _near[i] = null;
            _nearD[i] = float.MaxValue;
        }
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || !e.gameObject.activeInHierarchy)
            {
                continue;
            }
            ActiveTotal++;
            Vector3 q = e.transform.position;
            float dx = q.x - p.x;
            float dy = q.y - p.y;
            Insert(_near, _nearD, e, dx * dx + dy * dy);
        }
    }

    private static void Insert(EnemyHealth[] arr, float[] dist, EnemyHealth e, float d)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (d < dist[i])
            {
                for (int j = arr.Length - 1; j > i; j--)
                {
                    arr[j] = arr[j - 1];
                    dist[j] = dist[j - 1];
                }
                arr[i] = e;
                dist[i] = d;
                return;
            }
        }
    }

    private void Fill(ref Foe f, EnemyHealth e, Vector3 p, float t)
    {
        Vector3 q = e.transform.position;
        int uid = RuntimeHelpers.GetHashCode(e);
        Mem m = Remember(uid, q, t);
        f.Uid = uid;
        f.Id = e.KillMessageId;
        f.Dx = q.x - p.x;
        f.Dy = q.y - p.y;
        f.Dist = Mathf.Sqrt(f.Dx * f.Dx + f.Dy * f.Dy);
        f.Vx = m.Vx;
        f.Vy = m.Vy;
        f.HpPercent = e.maxHP > 0 ? Mathf.Clamp(Mathf.RoundToInt(e.nowHP * 100f / e.maxHP), 0, 100) : 0;
        TempMobDash dash = e.ObsDash;
        TempMobShooter shooter = e.ObsShooter;
        f.Sig = (e.ObsHasAttackAnim ? SigAnim : 0) | (dash != null ? SigDash : 0) | (shooter != null ? SigShoot : 0);
        bool ownHitbox = _hitboxOwners.Contains(RuntimeHelpers.GetHashCode(e.gameObject));
        f.Hitbox = ownHitbox || (dash != null && dash.IsDashing);
        f.State = StateOf(e, m, dash, shooter, f.Hitbox);
    }

    private static int StateOf(EnemyHealth e, Mem m, TempMobDash dash, TempMobShooter shooter, bool hitbox)
    {
        if (e.ObsDead)
        {
            m.SpanHit = false;
            return PlayRecThreat.StDead;
        }
        if (e.stiffShakeTime > 0f || e.isStun)
        {
            m.SpanHit = false;
            return PlayRecThreat.StHit;
        }
        bool attackFlag = e.ObsAttackFlag;
        if (!attackFlag)
        {
            m.SpanHit = false;
        }
        if (hitbox)
        {
            m.SpanHit |= attackFlag;
            return PlayRecThreat.StAttack;
        }
        if (attackFlag)
        {
            return m.SpanHit ? PlayRecThreat.StAttack : PlayRecThreat.StTele;
        }
        if ((dash != null && dash.IsCrouching) || (shooter != null && shooter.IsWindingUp))
        {
            return PlayRecThreat.StTele;
        }
        return e.ChaseLatched ? PlayRecThreat.StChase : PlayRecThreat.StIdle;
    }

    /// <summary>살아 있는 몹 판정: 근접 판정 소유자 집합 + 날아가는 투사체(부모 없음) 가까운 순</summary>
    private void ScanDamageObjects(Vector3 p, float t)
    {
        _hitboxOwners.Clear();
        ProjCount = 0;
        for (int i = 0; i < ProjMax; i++)
        {
            _projD[i] = float.MaxValue;
        }
        foreach (EnemyDamageObject d in EnemyDamageObject.Live)
        {
            if (d == null || d.isApcAttack)
            {
                continue;
            }
            Transform tr = d.transform;
            if (!d.isLaunch || tr.parent != null)
            {
                if (d.enemyOb != null)
                {
                    _hitboxOwners.Add(RuntimeHelpers.GetHashCode(d.enemyOb));
                }
                continue;
            }
            Vector3 q = tr.position;
            Mem m = Remember(RuntimeHelpers.GetHashCode(d), q, t);
            float dx = q.x - p.x;
            float dy = q.y - p.y;
            InsertProj(new Proj { Dx = dx, Dy = dy, Vx = m.Vx, Vy = m.Vy }, dx * dx + dy * dy);
        }
    }

    private void InsertProj(Proj pr, float d)
    {
        for (int i = 0; i < ProjMax; i++)
        {
            if (d < _projD[i])
            {
                for (int j = ProjMax - 1; j > i; j--)
                {
                    Projs[j] = Projs[j - 1];
                    _projD[j] = _projD[j - 1];
                }
                Projs[i] = pr;
                _projD[i] = d;
                if (ProjCount < ProjMax)
                {
                    ProjCount++;
                }
                return;
            }
        }
    }

    private Mem Remember(int uid, Vector3 q, float t)
    {
        Mem m;
        if (!_mem.TryGetValue(uid, out m))
        {
            m = new Mem { X = q.x, Y = q.y, T = t };
            _mem[uid] = m;
        }
        else if (t - m.Seen > 0.5f)
        {
            m.Vx = 0f; // 오래 안 본 것(풀 재사용 포함)은 속도를 새로 잰다
            m.Vy = 0f;
            m.X = q.x;
            m.Y = q.y;
            m.T = t;
        }
        else
        {
            float dt = t - m.T;
            if (dt > 0.0001f)
            {
                m.Vx = (q.x - m.X) / dt;
                m.Vy = (q.y - m.Y) / dt;
                m.X = q.x;
                m.Y = q.y;
                m.T = t;
            }
        }
        m.Seen = t;
        return m;
    }

    private void Forget(float t)
    {
        _forget.Clear();
        foreach (KeyValuePair<int, Mem> kv in _mem)
        {
            if (t - kv.Value.Seen > MemForgetSeconds)
            {
                _forget.Add(kv.Key);
            }
        }
        for (int i = 0; i < _forget.Count; i++)
        {
            _mem.Remove(_forget[i]);
        }
    }
}
