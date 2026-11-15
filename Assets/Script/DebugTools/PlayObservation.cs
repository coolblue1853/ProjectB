using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 상태 관측(읽기 전용). PlayRecorder 의 스냅샷·학습 틱이 쓰고, 자동 플레이 봇(B단계)도 같은 클래스를 재사용한다.
/// Observe() 한 번에 필드를 갱신한다. 할당을 피한다: 가까운 적은 고정 배열, 포털·NPC·드롭은 몇 초마다 한 번만 FindObjectsByType 으로 캐시.
/// 하위 관측: Foes(적·투사체 6+4, PlayObsEnemies) · Terrain(격자·발판 끝·낙하, PlayObsTerrain) · Context(1초 맥락, ObserveContext) · Ui(포커스 경로).
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
    /// <summary>공격 모션 재생 중(막힘 판정 제외용)</summary>
    public bool Attacking;
    public int Face = 1;
    public string Fsm = "";
    public string Flags = "";
    public bool HasHealth;
    public int Hp, HpMax, Mp, MpMax;
    public int Level, Exp, Gold;
    public int EnemyCount;
    public readonly NearEnemy[] Enemies = new NearEnemy[NearCount];
    public NearPoint Portal, Npc, Drop;

    // 전투 상태(학습 틱)
    /// <summary>지금 피해를 받을 수 있는가(피격 무적·대시/스킬 무적이 아님)</summary>
    public bool CanBeHit;
    /// <summary>장착 무기의 현재 콤보 단계(Weapon.nowConboCount), 무기 없으면 0</summary>
    public int Combo;
    /// <summary>던파식 히트 카운터(CombatHud.Hits)</summary>
    public int Hits;
    /// <summary>스킬 칸 4개의 남은 쿨다운(초), 쿨다운 아님 = 0</summary>
    public readonly float[] Cooldowns = new float[4];
    public bool TerrainChanged;

    public readonly PlayObsEnemies Foes = new PlayObsEnemies();
    public readonly PlayObsTerrain Terrain = new PlayObsTerrain();
    public readonly PlayObsContext Context = new PlayObsContext();
    public readonly PlayObsUi Ui = new PlayObsUi();
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
        Terrain.ForceRescan();
    }

    /// <summary>진행 맥락(1초마다 부른다 — 퀘스트 목록 할당이 있어 매 틱 부르지 않는다)</summary>
    public void ObserveContext()
    {
        Context.Observe();
    }

    public void Observe()
    {
        PlayerController pc = PlayerController.instance;
        HasPlayer = pc != null;
        EnemyCount = 0;
        Portal = default(NearPoint);
        Npc = default(NearPoint);
        Drop = default(NearPoint);
        TerrainChanged = false;
        if (!HasPlayer)
        {
            OnRope = false;
            Grounded = false;
            NearLadder = false;
            Fsm = "";
            Flags = "";
            Attacking = false;
            HasHealth = false;
            CanBeHit = false;
            Foes.Clear();
            Terrain.Clear();
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
        Attacking = pc.IsAttackAnimating;
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
        CanBeHit = HasHealth && hm.CanBeHit;
        Level = LevelSystem.Level;
        Exp = LevelSystem.Exp;
        Gold = DatabaseManager.money;
        ObserveCombat();
        Foes.Observe(p, Time.time);
        CopyNearEnemies();
        TerrainChanged = Terrain.Observe(pc, Grounded, Face, Time.unscaledTime);
        RescanIfDue();
        Portal = Nearest(_portals, p, false);
        Npc = Nearest(_npcs, p, false);
        Drop = Nearest(_drops, p, true);
    }

    /// <summary>st(R1) 용 가까운 적 3마리 = Foes 앞 3개</summary>
    private void CopyNearEnemies()
    {
        EnemyCount = Mathf.Min(NearCount, Foes.FoeCount);
        for (int i = 0; i < EnemyCount; i++)
        {
            PlayObsEnemies.Foe f = Foes.Foes[i];
            Enemies[i].Id = f.Id;
            Enemies[i].Dx = f.Dx;
            Enemies[i].Dy = f.Dy;
            Enemies[i].HpPercent = f.HpPercent;
        }
    }

    private void ObserveCombat()
    {
        Weapon w = Weapon.Equipped;
        Combo = w != null ? w.nowConboCount : 0;
        Hits = CombatHud.Hits;
        SkillCooldown cd = SkillCooldown.instance;
        for (int i = 0; i < Cooldowns.Length; i++)
        {
            Cooldowns[i] = cd != null ? cd.Remaining(i) : 0f;
        }
    }

    private void RescanIfDue()
    {
        float now = Time.unscaledTime;
        if (now >= _nextStaticScan)
        {
            _nextStaticScan = now + StaticRescanSeconds;
            _portals.Clear();
            Portal[] portals = Object.FindObjectsByType<Portal>();
            for (int i = 0; i < portals.Length; i++)
            {
                _portals.Add(portals[i].transform);
            }
            _npcs.Clear();
            AddAll(Object.FindObjectsByType<TalkNpc>(), _npcs);
            AddAll(Object.FindObjectsByType<ShopUI>(), _npcs);
            AddAll(Object.FindObjectsByType<EquipSmithUI>(), _npcs);
            AddAll(Object.FindObjectsByType<WeaponSmithUI>(), _npcs);
        }
        if (now >= _nextDropScan)
        {
            _nextDropScan = now + DropRescanSeconds;
            _drops.Clear();
            DropItemCheck[] drops = Object.FindObjectsByType<DropItemCheck>();
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
