using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 상태 관측(읽기 전용). PlayRecorder 의 스냅샷이 쓰고, 나중에 자동 플레이 봇(B단계)도 같은 클래스를 재사용한다.
/// Observe() 한 번에 필드를 갱신한다. 할당을 피한다: 가까운 적은 고정 배열, 포털·NPC·드롭은 몇 초마다 한 번만 FindObjectsByType 으로 캐시.
/// 맵 밖·타이틀처럼 값이 없어도 예외 없이 HasPlayer=false 등 빈 값으로 둔다. 게임 상태를 바꾸지 않는다.
/// </summary>
public sealed class PlayObservation
{
    public const int NearCount = 3;
    private const float StaticRescanSeconds = 3f;
    private const float DropRescanSeconds = 0.75f;

    // StatusFlag 순서: Invincible, SuperArmor, MoveLock, LadderLock, InputLock, DashLock
    private static readonly string[] FlagLetters = { "I", "S", "M", "L", "U", "D" };
    private static readonly string[] FlagCombos = BuildFlagCombos();

    public struct NearEnemy
    {
        public string Id;
        public float Dx;
        public float Dy;
        public int HpPercent;
    }

    public struct NearPoint
    {
        public bool Found;
        public float Dx;
        public float Dy;
    }

    public bool HasPlayer;
    public float X, Y, Vx, Vy;
    public bool Grounded, OnRope, NearLadder;
    public int Face = 1;
    public string Fsm = "";
    public string Flags = "";
    public bool HasHealth;
    public int Hp, HpMax, Mp, MpMax;
    public int Level, Exp, Gold;
    public int EnemyCount;
    public readonly NearEnemy[] Enemies = new NearEnemy[NearCount];
    public NearPoint Portal, Npc, Drop;

    private readonly EnemyHealth[] _near = new EnemyHealth[NearCount];
    private readonly float[] _nearD = new float[NearCount];
    private readonly List<Transform> _portals = new List<Transform>(16);
    private readonly List<Transform> _npcs = new List<Transform>(16);
    private readonly List<Transform> _drops = new List<Transform>(32);
    private float _nextStaticScan;
    private float _nextDropScan;
    private Rigidbody2D _rb;
    private PlayerController _rbOwner;

    /// <summary>열려 있는 UI 창(UiLock 소유자 목록, 열린 순서)</summary>
    public IReadOnlyList<string> UiWindows => UiLock.Held;

    public bool UiOpen => UiLock.Held.Count > 0;

    public void ForceRescan()
    {
        _nextStaticScan = 0f;
        _nextDropScan = 0f;
    }

    public void Observe()
    {
        PlayerController pc = PlayerController.instance;
        HasPlayer = pc != null;
        EnemyCount = 0;
        Portal = default(NearPoint);
        Npc = default(NearPoint);
        Drop = default(NearPoint);
        if (!HasPlayer)
        {
            OnRope = false;
            Grounded = false;
            NearLadder = false;
            Fsm = "";
            Flags = "";
            HasHealth = false;
            return;
        }
        Transform tf = pc.transform;
        Vector3 p = tf.position;
        X = p.x;
        Y = p.y;
        if (_rbOwner != pc)
        {
            _rb = pc.GetComponent<Rigidbody2D>();
            _rbOwner = pc;
        }
        Vector2 v = _rb != null ? _rb.linearVelocity : Vector2.zero;
        Vx = v.x;
        Vy = v.y;
        Grounded = pc.isOnGround;
        OnRope = pc.isUpLadder;
        NearLadder = pc.isLadder;
        Face = tf.localScale.x < 0f ? -1 : 1;
        Fsm = pc.states ?? "";
        Flags = FlagCombos[FlagMask()];

        PlayerHealthManager hm = PlayerHealthManager.Instance;
        HasHealth = hm != null;
        if (HasHealth)
        {
            Hp = hm.nowHp;
            HpMax = hm.fullHP;
            Mp = hm.nowStemina;
            MpMax = hm.fullStemina;
        }
        Level = LevelSystem.Level;
        Exp = LevelSystem.Exp;
        Gold = DatabaseManager.money;
        ObserveEnemies(p);
        RescanIfDue();
        Portal = Nearest(_portals, p, false);
        Npc = Nearest(_npcs, p, false);
        Drop = Nearest(_drops, p, true);
    }

    private void ObserveEnemies(Vector3 p)
    {
        for (int i = 0; i < NearCount; i++)
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
            Vector3 q = e.transform.position;
            float dx = q.x - p.x;
            float dy = q.y - p.y;
            float d = dx * dx + dy * dy;
            for (int i = 0; i < NearCount; i++)
            {
                if (d < _nearD[i])
                {
                    for (int j = NearCount - 1; j > i; j--)
                    {
                        _near[j] = _near[j - 1];
                        _nearD[j] = _nearD[j - 1];
                    }
                    _near[i] = e;
                    _nearD[i] = d;
                    break;
                }
            }
        }
        for (int i = 0; i < NearCount; i++)
        {
            EnemyHealth e = _near[i];
            if (e == null)
            {
                break;
            }
            Vector3 q = e.transform.position;
            Enemies[i].Id = e.KillMessageId;
            Enemies[i].Dx = q.x - p.x;
            Enemies[i].Dy = q.y - p.y;
            Enemies[i].HpPercent = e.maxHP > 0 ? Mathf.Clamp(e.nowHP * 100 / e.maxHP, 0, 100) : 0;
            EnemyCount = i + 1;
            _near[i] = null; // 참조를 오래 쥐지 않는다
        }
    }

    private void RescanIfDue()
    {
        float now = Time.unscaledTime;
        if (now >= _nextStaticScan)
        {
            _nextStaticScan = now + StaticRescanSeconds;
            _portals.Clear();
            Portal[] portals = Object.FindObjectsByType<Portal>(FindObjectsSortMode.None);
            for (int i = 0; i < portals.Length; i++)
            {
                _portals.Add(portals[i].transform);
            }
            _npcs.Clear();
            AddAll(Object.FindObjectsByType<TalkNpc>(FindObjectsSortMode.None), _npcs);
            AddAll(Object.FindObjectsByType<ShopUI>(FindObjectsSortMode.None), _npcs);
            AddAll(Object.FindObjectsByType<EquipSmithUI>(FindObjectsSortMode.None), _npcs);
            AddAll(Object.FindObjectsByType<WeaponSmithUI>(FindObjectsSortMode.None), _npcs);
        }
        if (now >= _nextDropScan)
        {
            _nextDropScan = now + DropRescanSeconds;
            _drops.Clear();
            DropItemCheck[] drops = Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None);
            for (int i = 0; i < drops.Length; i++)
            {
                _drops.Add(drops[i].transform);
            }
        }
    }

    private static void AddAll<T>(T[] items, List<Transform> into) where T : Component
    {
        for (int i = 0; i < items.Length; i++)
        {
            into.Add(items[i].transform);
        }
    }

    private static NearPoint Nearest(List<Transform> list, Vector3 p, bool needActive)
    {
        NearPoint best = default(NearPoint);
        float bestD = float.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            Transform tr = list[i];
            if (tr == null || (needActive && !tr.gameObject.activeInHierarchy))
            {
                continue;
            }
            Vector3 q = tr.position;
            float dx = q.x - p.x;
            float dy = q.y - p.y;
            float d = dx * dx + dy * dy;
            if (d < bestD)
            {
                bestD = d;
                best.Found = true;
                best.Dx = dx;
                best.Dy = dy;
            }
        }
        return best;
    }

    private static int FlagMask()
    {
        int mask = 0;
        for (int i = 0; i < FlagLetters.Length; i++)
        {
            if (PlayerStatus.Has((StatusFlag)i))
            {
                mask |= 1 << i;
            }
        }
        return mask;
    }

    private static string[] BuildFlagCombos()
    {
        string[] combos = new string[1 << FlagLetters.Length];
        for (int m = 0; m < combos.Length; m++)
        {
            string s = "";
            for (int i = 0; i < FlagLetters.Length; i++)
            {
                if ((m & (1 << i)) != 0)
                {
                    s += FlagLetters[i];
                }
            }
            combos[m] = s;
        }
        return combos;
    }
}
