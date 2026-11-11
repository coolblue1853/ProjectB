using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;
using DamageNumbersPro;
using BehaviorDesigner.Runtime;
using System;
using UnityEngine.Pool;
using BehaviorDesigner.Runtime.Tasks;
public class EnemyHealth : PoolAble
{
    public int enemyNum = 0;
    [HideInInspector] public Vector3 spawnPosition;   // 스포너가 배치한 위치 (순찰 리쉬 기준)
    [HideInInspector] public bool hasSpawnPosition;
    public event System.Action<int,int,Vector3, bool, bool> OnReleasedToPool; // 이벤트 선언
    public EnemySpawner enemySpowner;
    public bool isBleeding = false;
    int bleedStacks;
    /// <summary>출혈 중첩 관리: 여러 출혈 중 하나가 먼저 끝나도 나머지가 남아 있으면 출혈 상태 유지</summary>
    public void AddBleedStack() { bleedStacks++; isBleeding = true; }
    public void RemoveBleedStack() { bleedStacks = Mathf.Max(0, bleedStacks - 1); isBleeding = bleedStacks > 0; }
    public DamageNumber damageNumber;
    public int enemyDef = 0;
    Animator anim;
    static readonly int IsAttackId = Animator.StringToHash("isAttack");
    bool animHasAttack; // 토끼 등 isAttack 이 없는 애니메이터에 GetBool 하면 맞을 때마다 경고가 쌓였다(10-26)
    public DG.Tweening.Sequence sequence;
    public int maxHP = 100;
    public int nowHP = 100;
    public EnemyHealthBar hpBar;
    public GameObject hpObject;
    public float hpHeight;
    public bool isSuperArmor;
    public GameObject hpCanvas;
    private Coroutine toggleCoroutine;
     Rigidbody2D rb;
    // 넉백에 사용될 방향
    Vector2 knockbackDirection = new Vector2(0f, 1f);
    public DropManager dropManager;
    BehaviorTree behaviorTree;

    public DeadBody deadBody;
    public bool isBossMob = false;

    // ── 정예(중간보스) — ProgressionDesign 1절 ──
    public const float EliteHpMul = 4f, EliteDefMul = 1.5f, EliteScaleMul = 1.25f, EliteAtkMul = 1.5f;
    public bool IsElite { get; private set; }
    /// <summary>미니보스(Floor1Completion 4-4): 정예 표식·몸박 x1.5 를 쓰지만 HP·방어는 MonsterSheet 값 그대로이고 EXP 는 정예 배수를 받지 않는다</summary>
    public bool IsMiniBoss { get; private set; }
    public const float MiniBossScaleMul = 1.5f;
    BossPhase bossPhase;
    int baseMaxHP, baseDef;
    int _trialBaseHP;
    /// <summary>문지기 시험 피해 계수(Floor1Completion 14-1). 문지기가 아니면 1</summary>
    public float TrialDamageMul { get; private set; } = 1f;

    /// <summary>문지기 스폰 시 달성도로 HP·피해 계수를 정하고 알림을 띄운다. 풀 재사용에도 기준 HP 에서 다시 계산한다.</summary>
    void ApplyGatekeeperTrial()
    {
        TrialDamageMul = 1f;
        if (!isBossMob || !GatekeeperTrial.IsGatekeeper(name))
        {
            return;
        }
        if (_trialBaseHP <= 0)
        {
            _trialBaseHP = maxHP;
        }
        float a = GatekeeperTrial.CurrentReadiness();
        maxHP = Mathf.RoundToInt(_trialBaseHP * GatekeeperTrial.HpMultiplier(a));
        TrialDamageMul = GatekeeperTrial.DamageMultiplier(a);
        PickupLog.Report(Loc.T("Trial readiness") + ": " + Mathf.RoundToInt(a * 100f) + "%", 1, "U");
        GameLog.Info(GameLog.Cat.Spawn, "문지기 시험 달성도 " + a.ToString("F2") + " HP " + maxHP + " 피해x" + TrialDamageMul.ToString("F2"), this);
    }
    Vector3 baseScale;
    Color baseColor;
    EliteAura eliteAura;

    /// <summary>스폰 직후 정예로 승격. 풀 반환(OnDisable) 때 Demote로 원래 값 복구</summary>
    public void PromoteElite(bool keepStats = false, float scaleMul = EliteScaleMul)
    {
        if (IsElite || isBossMob)
        {
            return;
        }
        IsElite = true;
        IsMiniBoss = keepStats;
        baseMaxHP = maxHP;
        baseDef = enemyDef;
        baseScale = transform.localScale;
        var sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        if (!keepStats)
        {
            maxHP = Mathf.RoundToInt(maxHP * EliteHpMul);
            enemyDef = Mathf.RoundToInt(enemyDef * EliteDefMul);
        }
        nowHP = maxHP;
        transform.localScale = baseScale * scaleMul;
        sr.color = new Color(1f, 0.92f, 0.7f, baseColor.a);
        if (eliteAura == null)
        {
            eliteAura = EliteAura.Attach(this);
        }
        eliteAura.gameObject.SetActive(true);
        if (!TryGetComponent(out SpriteHitFx eliteFx))
        {
            eliteFx = gameObject.AddComponent<SpriteHitFx>(); // 보통 첫 피격 때 붙는다 — 승격 외곽선을 위해 미리
        }
        eliteFx.SetElite(true);
        if (hpBar != null)
        {
            hpBar.setHpBar(this);
        }
        Sfx.Play(SfxId.BossSlam, 0.35f);
        MonsterTable.Row row = MonsterTable.Get(name);
        PickupLog.Report(Loc.T("Elite: ") + MonsterTable.DisplayName(name), 1, "U");
        GameLog.Info(GameLog.Cat.Spawn, "정예 승격: " + name + " HP " + maxHP + " DEF " + enemyDef, this);
    }

    void DemoteElite()
    {
        if (!IsElite)
        {
            return;
        }
        IsElite = false;
        IsMiniBoss = false;
        maxHP = baseMaxHP;
        enemyDef = baseDef;
        transform.localScale = baseScale;
        GetComponent<SpriteRenderer>().color = baseColor;
        if (TryGetComponent(out SpriteHitFx eliteFx))
        {
            eliteFx.SetElite(false);
        }
        if (eliteAura != null)
        {
            eliteAura.gameObject.SetActive(false);
        }
    }
    private static Camera _mainCam;
    /// <summary>Camera.main 정적 캐시. 씬 전환으로 파괴되면(Unity null) 다시 조회한다(LP-4)</summary>
    private static Camera MainCam()
    {
        if (_mainCam == null)
        {
            _mainCam = Camera.main;
        }
        return _mainCam;
    }
    /// <summary>현재 활성 보스 수 (BGM 전환 등에서 사용)</summary>
    public static int ActiveBossCount { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetBossCount()
    {
        ActiveBossCount = 0;
        _mainCam = null;
        Active.Clear();
    }

    /// <summary>행동 트리가 있는 진짜 몬스터인지 (나무·바위 같은 채집물 제외)</summary>
    public bool IsMonster => behaviorTree != null;

    /// <summary>몬스터 레벨(MonsterSheet level, 없으면 0 = 레벨 차 계산 안 함)</summary>
    public int Level { get; private set; }
    public string MonsterId { get; private set; }

    /// <summary>도감·기존 퀘스트의 저장/메시지 키는 지역 수치 키와 독립적으로 유지한다.</summary>
    public string KillMessageId => string.IsNullOrEmpty(_killKey) ? ExtraDrops.MonsterId(name) : _killKey;
    string _killKey;

    /// <summary>같은 아트를 쓰는 지역 변종. 풀 생성 직후, 정예 승격 전에 한 번만 적용한다.</summary>
    public void ApplySpawnProfile(string monsterId)
    {
        MonsterTable.Row row = MonsterTable.Get(monsterId);
        if (row == null || !MonsterTable.Apply(this, monsterId))
        {
            return;
        }
        MonsterId = row.id;
        Level = row.level ?? 0;
        _contactBase = row.contactDamage ?? 0;
        nowHP = maxHP;
        _killKey = row.killKey; // 새 몹은 프로필 id 로 처치 메시지를 보낸다
        if (!string.IsNullOrEmpty(row.killKey))
        {
            TempMobVisual.Apply(this, row);
        }
        if (TempMobTraits.Has(row.trait, "spore") && !TryGetComponent(out SporeCloud _))
        {
            gameObject.AddComponent<SporeCloud>();
        }
        _moveMul = TempMobTraits.MoveMul(row.trait);
        _knockbackMul = TempMobTraits.KnockbackMul(row.trait);
        _jumpMul = TempMobTraits.JumpMul(row.trait);
        _sturdy = TempMobTraits.Has(row.trait, "sturdy");
        if (TempMobTraits.Has(row.trait, "dash") && !TryGetComponent(out _dash))
        {
            _dash = gameObject.AddComponent<TempMobDash>();
        }
    }

    // 0 = 프로필 없음(1배). 필드 초기값에 기대지 않는다(에디터에서 만든 컴포넌트는 0으로 시작한 사례, 11-01 테스트)
    float _moveMul, _knockbackMul, _jumpMul;
    bool _sturdy; // trait sturdy = 경직 저항 50%
    TempMobDash _dash;
    // 필드 초기화에 기대지 않는다: 테스트·에디터가 만든 컴포넌트는 초기화가 안 돈 사례가 있다(11-01 _moveMul 와 같은 이유)
    MobReflectRules _reflectRules; // 반사 자세(F2-MOBREFLECT)
    MobDebuffRules _debuffRules; // 런타임 디버프(둔화·석화 연쇄 제한, F2-DEBUFF)
    MobReflectRules _reflect => _reflectRules ?? (_reflectRules = new MobReflectRules());
    MobDebuffRules _debuff => _debuffRules ?? (_debuffRules = new MobDebuffRules());

    /// <summary>추적·순찰 이동 배율(임시 몹 특성 slow·dash). 기존 몹은 1</summary>
    public float MoveSpeedMul => (_moveMul > 0f ? _moveMul : 1f) * (_dash != null ? _dash.ChaseMul : 1f) * _debuff.SlowMul;
    /// <summary>넉백 힘 배율(heavy 0.5). 기존 몹은 1</summary>
    public float KnockbackMul => _knockbackMul > 0f ? _knockbackMul : 1f;
    /// <summary>경직 저항 0~1(보스 0.6·sturdy 0.5·정예 0.3 중 최대, 던파식 경직 수치 정리). 슈퍼아머 몹은 경직 자체가 없다</summary>
    public float StaggerResist => CombatRules.StaggerResist(isBossMob, _sturdy, IsElite);
    /// <summary>BT 도약 높이 배율(lowhop 0.8). 기존 몹은 1</summary>
    public float JumpMul => _jumpMul > 0f ? _jumpMul : 1f;
    int _contactBase;

    /// <summary>BUG-U23: 작은 몹의 본체 콜라이더(플레이어 스킬 판정 대상)를 월드 하한까지 키운다. 발·가로 중심 유지.</summary>
    void EnsureMinHitbox()
    {
        if (isBossMob || !TryGetComponent(out BoxCollider2D box))
        {
            return;
        }
        if (MobBehaviorRules.FitHitbox(box.size, box.offset, transform.lossyScale, MobBehaviorRules.MinHitWidth, MobBehaviorRules.MinHitHeight, out Vector2 size, out Vector2 offset))
        {
            box.size = size;
            box.offset = offset;
        }
    }

    /// <summary>REWORK-3 행동 속성(없으면 null = 프리팹 BT 동작 그대로). MonsterTable.Apply 가 채운다.</summary>
    public MobBehavior Behavior { get; set; }
    /// <summary>추격 유지 중(인식 성공 뒤 giveUp 거리까지 놓치지 않음). 풀 재사용 때 초기화</summary>
    public bool ChaseLatched { get; set; }
    float _provokedUntil;
    /// <summary>맞아서 반응 중인가(후공 몹용)</summary>
    public bool IsProvoked => Time.time < _provokedUntil;
    public void MarkProvoked() { _provokedUntil = Time.time + MobBehaviorRules.ProvokeSeconds; }
    float _enabledAt;
    const float SpawnGraceTime = 0.3f;

    /// <summary>몸박 공격력(방어 적용 전). 정예는 x1.5, 값이 없으면 0</summary>
    public int ContactDamage => IsElite ? Mathf.RoundToInt(_contactBase * EliteAtkMul) : _contactBase;

    /// <summary>지금 몸박을 줄 수 있는가: 죽는 중·사라지는 중·막 소환된 직후(0.3초)는 아니다</summary>
    public bool CanContact => !deadOnec && nowHP > 0 && Time.time - _enabledAt >= SpawnGraceTime
        && (disappearSequence == null || !disappearSequence.IsActive());

    /// <summary>활성 몬스터 목록 (유도 탄 등 매 프레임 FindObjects 를 피하려고)</summary>
    public static readonly System.Collections.Generic.HashSet<EnemyHealth> Active = new System.Collections.Generic.HashSet<EnemyHealth>();

    /// <summary>플레이어 직격(damage2Enemy) 처치 시 발생 — 도트(onlyDamage2Enemy) 처치는 제외. 처치 시 효과(Primordial Gel 소환 등)용.
    /// 풀 반환 직후(비활성) 호출되므로 구독자는 위치만 읽고 참조를 보관하지 않는다.</summary>
    public static event System.Action<EnemyHealth> Killed;
    public event EventHandler OnDamaged;
    public event EventHandler OnHealed;

    float disapearInterval = 0.5f;
    public EnemyHealth(int hpAmount)
    {
        maxHP = hpAmount;
        this.nowHP = hpAmount;
    }
    
    public void Damage(int amount)
    {
        nowHP -= amount;
        MarkProvoked();
        CombatState.Mark(Time.unscaledTime);
        if (isBossMob)
        {
            (bossPhase != null ? bossPhase : bossPhase = GetComponent<BossPhase>())?.OnDamaged(); // 페이즈 임계값 검사 (C4)
        }

        if (OnDamaged != null) OnDamaged(this, EventArgs.Empty);
    }
    public void Heal(int amount)
    {
        nowHP += amount;
        if (nowHP > maxHP)
        {
            nowHP = maxHP;
        }
        if (OnHealed != null) OnHealed(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)nowHP / maxHP;
    }
    public void SetMaxHealth(int maxHealth)
    {
        maxHP = maxHealth;
        if (nowHP > maxHP)
        {
            nowHP = maxHP;
        }
    }

    private void Awake()
    {
        MonsterId = ExtraDrops.MonsterId(name);
        MonsterTable.Apply(this); // Resources/MonsterSheet.csv 값이 있으면 프리팹 값을 덮어쓴다 (C5)
        MonsterTable.Row levelRow = MonsterTable.Get(name);
        Level = levelRow != null && levelRow.level.HasValue ? levelRow.level.Value : 0;
        EnsureMinHitbox();
        _contactBase = levelRow != null && levelRow.contactDamage.HasValue ? levelRow.contactDamage.Value : 0;
        BossPhase.AttachIfBoss(this); // 보스 페이즈 (C4)
        anim = transform.GetComponent<Animator>();
        animHasAttack = false;
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in anim.parameters)
            {
                if (p.nameHash == IsAttackId)
                {
                    animHasAttack = true;
                    break;
                }
            }
        }
        dropManager = transform.GetComponent<DropManager>();
        rb = transform.GetComponent<Rigidbody2D>();
        behaviorTree = transform.GetComponent<BehaviorTree>();


    }
    DG.Tweening.Sequence wait;
    private void OnEnable()
    {
        Active.Add(this);
        _enabledAt = Time.time;
        ChaseLatched = false;
        _provokedUntil = 0f;
        ApplyGatekeeperTrial();
        ///isReleased = false;
        nowHP = maxHP;
        // 이미 끝나 Kill 된 시퀀스에 IsPlaying 을 부르면 DOTween 이 '죽은 트윈' 경고를 남긴다(풀 워밍업 때 다수) → IsActive 로 확인
        if (wait != null && wait.IsActive())
        {
            wait.Kill();
        }
        wait = DOTween.Sequence()
        .AppendInterval(0.3f)
        .AppendCallback(() => canDisapear = true);
        SpriteRenderer img = this.GetComponent<SpriteRenderer>();
        img.DOFade(1, 0);
        deadOnec = false;
        nowHP = maxHP;
        stiffShakeTime = 0;
        if (isBossMob)
        {
            ActiveBossCount++;
            BossPortrait.Ensure(this); // HP바 초상·이름 (G10)
            BossIntro.Play(this); // 등장 연출 (W2)
        }
        bleedStacks = 0;
        isBleeding = false;
        if (behaviorTree != null)
        {
            behaviorTree.enabled = true; // 경직 중 반환되어 꺼진 채로 재사용되는 것 방지
        }
        isStun = false; // 덫·경직 중 반환된 채 재사용 방지
        _staggerChain = 0;
        _lastStaggerAt = -10f;
        _debuff.Clear();
        _reflect.Clear();
        if (hpBar != null)
            hpBar.setHpBar(this);
    }
    private void Start()
    {
        if(deadBody != null)
        {
            deadBody.parentEnemy = this.transform.gameObject;
        }


        deadOnec = false;
        EnemyTelegraph.AttachIfAttacker(this); // 공격 예고 (C2)
    }
    bool notStiff;


    /*  0~ 9번까지 순서대로
   public int minDmg=0;        // 최소댐 0
    public int maxDmg=0;       // 최대댐  1
    public int critPer = 0;   //  치명 확률% 2 
    public int critDmg = 0;   //  치명피해% 3
    public int incDmg = 0;   // 데미지증가% 4
    public int ignDef = 0; // 방어력 무시% 5
    public int skillDmg = 0; // 스킬 공격력% 6
    public int finDmg = 0; // 최종뎀%   7
    public int addDmg = 0; // 추가뎀%   8
    public int ArmAtp = 0; // 방어구 기본뎀 9  
     */
    bool isCrit = false;
    float finDmg;
    float addDmg;
    float shakeTime;
    public float ShakeCorrection = 1.0f;
    DG.Tweening.Sequence striffSequence;

    public float stiffShakeTime = 0;
    bool deadOnec = false;
    public void damage2Enemy(int[] damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange, bool isSkill, float shaking, float dmgRatio)
    {    

        if (MissRules.PlayerAttackMisses(Level, isBossMob))
        {
            GameLog.Info(GameLog.Cat.Combat, name + " MISS (몹 Lv " + Level + " / 내 Lv " + LevelSystem.Level + ")", this);
            HitFeedback.SpawnText(damageNumber, transform.position + Vector3.up, "MISS", HitFeedback.TextKind.Miss);
            return; // 빗나감: 피해·넉백·경직·히트 피드백 없음
        }
        shakeTime = shaking;
        var result = DamageCalculator.Calculate(damage, DamageCalculator.PlayerOffense.FromDatabase(), enemyDef,
            isBleeding, isSkill, dmgRatio, UnityEngine.Random.value, UnityEngine.Random.Range(0f, 100f),
            DamageCalculator.LevelGapMul(Level, LevelSystem.Level));
        isCrit = result.isCrit;
        result.damage = Mathf.Max(1, Mathf.RoundToInt(result.damage * AscentLoad.OutgoingMul())); // 상승 부하: 부족분만큼 주는 피해 감소
        int reflected = _reflect.Reflect(result.damage);
        if (reflected > 0 && PlayerHealthManager.Instance != null)
        {
            PlayerHealthManager.Instance.damage2Player(reflected, 0f, 0f, Vector2.zero, transform.position.x, false, 0.25f); // 반사 자세(방어 가로채기 적용)
        }
        bool downed = _air != null && _air.IsDown;
        bool downAttack = downed && PlayerController.instance != null && PlayerController.instance.IsDownAttackHeld;
        if (downAttack)
        {
            result.damage = Mathf.Max(1, Mathf.RoundToInt(result.damage * CombatRules.DownDamageMul(true, true))); // 다운 공격(↓+공격) x1.3
        }
        finDmg = result.damage;
        if (isBossMob && bossPhase == null)
        {
            bossPhase = GetComponent<BossPhase>();
        }
        addDmg = result.extraDamage;
        GameLog.Info(GameLog.Cat.Combat, name + " 피격 " + result.damage + (result.isCrit ? " 치명" : "") + (result.extraDamage > 0 ? " +추가 " + result.extraDamage : "") + (isSkill ? " [스킬]" : "") + " HP " + nowHP + "→" + (nowHP - result.damage), this);

       if(isBossMob == false)
        {
            ToggleObject();

        }

       // 최종데미지
        Damage((int)finDmg);
        HitFeedback.SpawnNumber(damageNumber, transform.position + Vector3.up, (int)finDmg,
            isCrit ? HitFeedback.NumberKind.Crit : HitFeedback.NumberKind.Normal);


        if (isSuperArmor == true)
        {
            transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
            HitFeedback.SpawnText(damageNumber, transform.position + Vector3.up, "ARMOR", HitFeedback.TextKind.Block); // 슈퍼아머 표시(0.5초에 한 번)
        }
        if ((int)addDmg != 0) // 추가 데미지 판정
        {
           Damage((int)addDmg);
            HitFeedback.SpawnNumber(damageNumber, transform.position + Vector3.up, (int)addDmg, HitFeedback.NumberKind.Extra);
        }


        CombatHud.OnPlayerHit((int)finDmg + (int)addDmg); // 히트 수 집계(던파식 콤보 카운터)
        bool killed = TryDie();
        HitFeedback.OnEnemyHit(this, isCrit, isSkill, killed); // 플래시·히트스톱·흔들림
        if (killed)
        {
            Killed?.Invoke(this);
            return; // 사망 후에는 피격 연출(넉백·경직)을 걸지 않는다
        }

        if(isSuperArmor == false)
        {
            ApplyAirAndStagger(ref stiffTime, ref force, ref knockbackDir, downAttack);

            if (anim != null)
            {
                anim.SetBool("isHit", true);
            }
            if (behaviorTree != null) // 원래enemyFSM 였음
            {

                if(stiffTime == 0)
                {
                    notStiff = true;
                }
                behaviorTree.sequence.Kill();
                behaviorTree.enabled = false;
            }


            if (stiffShakeTime < shakeTime + stiffTime)
            {
                striffSequence.Kill(); // 재공격시 경직 시간 초기화.
                stiffShakeTime = shakeTime + stiffTime;
                isAttackGround = true;
                striffSequence = DOTween.Sequence()
                .AppendCallback(() => transform.DOShakePosition(shakeTime, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false))
                .AppendInterval(shakeTime)
                .AppendCallback(() => KnockbackActive(force, knockbackDir, x, isDirChange))
                .AppendInterval(stiffTime)
                .OnComplete(() => EndStiffness());
            }
            else
            {
                onlyShakeSeq.Kill(); // 재공격시 경직 시간 초기화.
                onlyShakeSeq = DOTween.Sequence()
               .AppendCallback(() => transform.DOShakePosition(shakeTime, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false));
            }
        }

    }

   DG.Tweening.Sequence onlyShakeSeq;
    public void onlyDamage2Enemy(int damage)
    {
        ToggleObject();

        Damage(damage);
        TryDie();



    }
    public void ReleaseEnemy(bool isDrop = true, bool isItemDrop = true)
    {
        if (IsInPool)
        {
            return;
        }
        canDisapear = false;
        OnReleasedToPool?.Invoke(enemyNum, Mathf.Abs(nowHP),this.transform.position, isDrop, isItemDrop); // 4번째 요소는 deadbody를 생성할것인지 말것인지


        if (hpBar != null)
        {
            foreach (Transform damagedBar in hpBar.damagedBars)
            {
                if (damagedBar != null)
                    Destroy(damagedBar.gameObject);
            }
            hpBar.damagedBars.Clear(); // 리스트 초기화
        }
        ReleaseObject();
    }
    public bool isAttackGround;

    public void ResetBoolParameters()
    {
        // 애니메이터에 등록된 모든 파라미터 가져오기
        AnimatorControllerParameter[] parameters = anim.parameters;

        // 모든 불리언 파라미터의 값을 false로 설정
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {

                anim.SetBool(parameter.name, false);
            }
        }
    }

    // ── 던파식 경직 연쇄·공중 콤보 (CombatSpec 9절) ──
    EnemyAirState _air;
    int _staggerChain;
    float _lastStaggerAt = -10f;

    /// <summary>피격 때: 경직 연쇄/저항으로 경직 시간을 줄이고, 공중 추가타면 중력을 낮추고, 다운 중이면 넉백을 눕힌다</summary>
    void ApplyAirAndStagger(ref float stiffTime, ref float force, ref Vector2 knockbackDir, bool downAttack)
    {
        if (stiffTime > 0f)
        {
            _staggerChain = CombatRules.NextChain(_staggerChain, Time.time - _lastStaggerAt);
            stiffTime *= CombatRules.StiffnessScale(_staggerChain, StaggerResist);
        }
        _lastStaggerAt = Time.time;
        if (behaviorTree == null || rb == null)
        {
            return; // 나무·바위 같은 채집물은 공중 콤보 대상이 아니다
        }
        if (_air == null && !TryGetComponent(out _air))
        {
            _air = gameObject.AddComponent<EnemyAirState>();
        }
        if (_air.IsDown)
        {
            knockbackDir.y = 0f; // 누운 적은 더 안 뜬다
            force *= 0.5f;
            if (downAttack)
            {
                _air.ExtendDown(0.3f);
            }
        }
        _air.RegisterHit();
    }

    private void EndStiffness()
    {
        if (_air != null && _air.TryDeferRecovery(FinishStiffness))
        {
            return; // 띄워진 몹은 착지 + 다운 시간 뒤에 행동 복귀
        }
        FinishStiffness();
    }

    private void FinishStiffness()
    {
        snareTween?.Kill(); // 경직이 먼저 끝나면 덫도 같이 풀린다(남은 덫 트윈이 다음 경직을 끊지 않게)
        behaviorTree.enabled = true;
        isStun = false;
        if (this != null && behaviorTree != null)
        {
            if (anim != null)
            {
                anim.SetBool("isHit", false);
                if (animHasAttack && anim.GetBool(IsAttackId))
                {
                    anim.SetBool(IsAttackId, false);
                }
            }

            isAttackGround = false;


        }

    }
    private void Update()
    {
        if(isBossMob == false && hpObject!= null)
        {
            hpObject.transform.position = MainCam().WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
        }
        if(stiffShakeTime > 0)
        {
            stiffShakeTime -= Time.deltaTime;
        }
        _debuff.Tick(Time.deltaTime);
        _reflect.Tick(Time.deltaTime);
    }

    /// <summary>반사 자세 시작: seconds 동안 받은 피해의 일부를 플레이어에게 되돌린다(천칭 석상 등)</summary>
    public void BeginReflect(float seconds)
    {
        _reflect.Begin(seconds);
    }

    /// <summary>시간제한 둔화(덩굴 묶기 등). 순찰·추적이 MoveSpeedMul 로 같은 값을 읽는다</summary>
    public void ApplySlow(float mul, float seconds)
    {
        _debuff.ApplySlow(mul, seconds);
    }

    /// <summary>석화 = 경직(Snare 재사용). 보스 제외, 정예·저항 반영, 끝난 뒤 면역 시간 동안 연쇄 불가</summary>
    public void ApplyPetrify(float seconds)
    {
        if (behaviorTree == null || isBossMob)
        {
            return; // Snare 가 거절할 대상은 면역 타이머도 소모하지 않는다(리뷰 #8)
        }
        float actual = _debuff.TryPetrify(seconds, StaggerResist, Time.time);
        if (actual > 0f)
        {
            Snare(actual);
        }
    }

    PosionAttack _burn;

    /// <summary>화상 = 독 틱 재사용(표시만 BURN). attackPower 는 시전자 공격력. 중첩 금지: 재적용은 기존 틱을 갱신(리뷰 #4)</summary>
    public void ApplyBurn(int attackPower)
    {
        int dmg = MobDebuffRules.BurnDamage(attackPower);
        if (_burn != null)
        {
            _burn.ActivePoison(dmg, MobDebuffRules.BurnInterval, MobDebuffRules.BurnCount);
            return;
        }
        _burn = CreatPoisonPrefab(dmg, MobDebuffRules.BurnInterval, MobDebuffRules.BurnCount, "BURN");
    }

    public float maxXSpeed =10f; // x축 넉백 속도 상한
    public float maxYSpeed =10f; //y 축 넉백 속도 상한
    public bool isStun;
    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x, bool isDirChange)
    {
        isStun = true;


        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            if (x> transform.position.x && isDirChange == true)
            {
                knockbackDir.x = -knockbackDir.x;
            }
            // 힘을 가해 넉백을 적용
            rb.AddForce(knockbackDir.normalized * knockbackForce * KnockbackMul, ForceMode2D.Impulse);
            //    transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
            Vector2 currentVelocity = rb.linearVelocity;


            // x 속도가 최대값을 초과하는지 확인하고, 초과하면 최대값으로 설정
            if (Mathf.Abs(currentVelocity.x) > maxXSpeed)
            {
                currentVelocity.x = Mathf.Sign(currentVelocity.x) * maxXSpeed;
            }

            // y 속도가 최대값을 초과하는지 확인하고, 초과하면 최대값으로 설정
            if (Mathf.Abs(currentVelocity.y) > maxYSpeed)
            {
                currentVelocity.y = Mathf.Sign(currentVelocity.y) * maxYSpeed;
            }

            // 최종 속도 적용
            rb.linearVelocity = currentVelocity;
            if (_air != null)
            {
                _air.ApplyJuggle();                      // 공중 추가타: 수평 감쇠 + 위로 살짝 띄움
                _air.NoteKnockVelocity(rb.linearVelocity); // 위로 크게 뜬 몹은 착지하면 다운
            }
        }

    }
    private void ToggleObject()
    {
        // 이전에 실행 중인 코루틴이 있다면 중지
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }
        if(this.gameObject != null&& this.gameObject.activeSelf != false)
        {
            // 새로운 코루틴 시작
            toggleCoroutine = StartCoroutine(ToggleObjectRoutine());
        }

    }

    private IEnumerator ToggleObjectRoutine()
    {
        // 오브젝트를 2초 동안 켜기
        hpCanvas.SetActive(true);

        // 2초 대기
        yield return new WaitForSeconds(2f);

        // 오브젝트를 꺼주기
        hpCanvas.SetActive(false);
    }

    public GameObject poisonPrefab;
    public GameObject bleedingPrefab;

    public PosionAttack CreatPoisonPrefab(int poisonDamage, float damageInterval,int damageCount, string label = "POISON")
    {
        PosionAttack posionAttack = null;
        if (this.gameObject != null)
        {
            StatusTint.Ensure(gameObject);
            if (GetComponentInChildren<PosionAttack>() == null)
            {
                HitFeedback.SpawnText(damageNumber, transform.position + Vector3.up, label, HitFeedback.TextKind.Status); // 새로 걸릴 때만
            }
            GameObject poisonObject = Instantiate(poisonPrefab, transform.position, transform.rotation, this.transform);
            posionAttack = poisonObject.GetComponent<PosionAttack>();
            posionAttack.enemyHealth = this.GetComponent<EnemyHealth>();
            posionAttack.ActivePoison(poisonDamage, damageInterval, damageCount);
        }
        return posionAttack;
    }
    public void CreatBleedingPrefab(int bleedingDamage, float bleedingDamageInterval, int bleedingDamageCount)
    {
        if(this.gameObject != null)
        {
            StatusTint.Ensure(gameObject);
            if (GetComponentInChildren<BleedingAttack>() == null)
            {
                HitFeedback.SpawnText(damageNumber, transform.position + Vector3.up, "BLEED", HitFeedback.TextKind.Status); // 새로 걸릴 때만
            }
            GameObject poisonObject = Instantiate(bleedingPrefab, transform.position, transform.rotation, this.transform);
            BleedingAttack posionAttack = poisonObject.GetComponent<BleedingAttack>();
            posionAttack.enemyHealth = this.GetComponent<EnemyHealth>();
            posionAttack.ActivePoison(bleedingDamage, bleedingDamageInterval, bleedingDamageCount);
        }
    }
    
    public void StopAllActions()
    {
        // GetActiveTasks 메서드가 null을 반환할 수 있으므로 예외 처리
        List<BehaviorDesigner.Runtime.Tasks.Task> activeTasks = behaviorTree.GetActiveTasks();
        if (activeTasks != null)
        {
            // Behavior Tree에서 실행 중인 모든 Task를 찾아서 중지
            foreach (BehaviorDesigner.Runtime.Tasks.Task task in activeTasks)
            {
                if (task is EnemyAction)
                {
                    EnemyAction enemyAction = (EnemyAction)task;
                    if (enemyAction != null)
                    {
                        enemyAction.StopAction();
                    }
                }
            }
        }
    }

   public  bool canDisapear = true;
    DG.Tweening.Sequence disappearSequence;

    /// <summary>HP가 0 이하가 되면 한 번만 사망 처리(드롭·시체·슬롯 비우기 포함)한다.</summary>
    bool TryDie()
    {
        if (nowHP > 0 || deadOnec)
        {
            return false;
        }
        deadOnec = true;
        GameLog.Info(GameLog.Cat.Combat, name + " 사망 (overkill " + (-nowHP) + ")", this);
        // 퀘스트 처치 카운트 (Quest Machine 카운터가 "Kill" + 프리팹 이름을 듣는다, G11)
        PixelCrushers.MessageSystem.SendMessage(this, "Kill", KillMessageId);
        if (IsElite || isBossMob)
        {
            PixelCrushers.MessageSystem.SendMessage(this, "KillElite", KillMessageId); // NPC 퀘스트 killElite 목표
        }
        LevelSystem.OnEnemyKilled(this); // 처치 EXP (정예 x4, 보스 고정)
        // 사망 연출(연기·정예/보스 폭발): 반환 전, 정예 여부·스프라이트 경계가 살아 있을 때 (사망 1회만 조회)
        DeathFx.Play(this, TryGetComponent(out SpriteRenderer body) ? body.bounds : new Bounds(transform.position, Vector3.one));
        ReleaseEnemy(true, true);
        return true;
    }

    /// <summary>드롭·시체 없이 즉시 반환 (맵 이동 등 강제 정리). 스포너 슬롯은 비운다.</summary>
    public void ForceEnemyRelease()
    {
        ReleaseEnemy(false, false);
    }

    /// <summary>1.5초 페이드아웃 후 드롭 없이 반환 (영역 이탈·시간대 변경).</summary>
    public void DisaperByTime()
    {
        if (IsInPool || deadOnec || (disappearSequence != null && disappearSequence.IsActive()))
        {
            return; // 이미 사라지는 중이거나 반환됨
        }
        SpriteRenderer img = this.GetComponent<SpriteRenderer>();
        disappearSequence = DOTween.Sequence()
            .Append(img.DOFade(0, 1.5f))
            .OnComplete(() => ReleaseEnemy(false, false));
    }

    private void OnDisable()
    {
        Active.Remove(this);
        DemoteElite();
        if (isBossMob)
        {
            ActiveBossCount = Mathf.Max(0, ActiveBossCount - 1);
        }
        // 반환된 뒤에 지연 콜백이 재사용된 개체를 건드리지 않도록 정리
        disappearSequence?.Kill();
        striffSequence?.Kill();
        onlyShakeSeq?.Kill();
        wait?.Kill();
        snareTween?.Kill();
        transform.DOKill();
    }

    Tween snareTween;

    /// <summary>
    /// 덫에 걸림(MiniGames.md ⑥ 흔적 추적): seconds 동안 BT 를 멈춘다(경직과 같은 방식). 맞아서 경직이 먼저 끝나면 그때 풀린다.
    /// </summary>
    public void Snare(float seconds)
    {
        if (seconds <= 0f || behaviorTree == null || isBossMob)
        {
            return;
        }
        behaviorTree.sequence?.Kill();
        behaviorTree.enabled = false;
        isStun = true;
        snareTween?.Kill();
        snareTween = DOVirtual.DelayedCall(seconds, () =>
        {
            if (stiffShakeTime <= 0f)
            {
                EndStiffness(); // 맞아서 경직 중이면 경직이 끝날 때 풀린다
            }
        }, false).SetLink(gameObject);
    }

    /// <summary>
    /// 풀 반환 직전: BT 이동·공격 노드가 만든 시퀀스(DOMoveX 등)를 끊는다. SetLink 는 파괴만 막아,
    /// 반환된(비활성) 몹의 transform 을 계속 움직이고 재사용 시 이전 이동이 섞였다(B-141).
    /// OnDisable 의 transform.DOKill() 은 Sequence 안에 중첩된 트윈을 끄지 못해 따로 Kill 한다.
    /// 참조는 null 로 비우지 않는다: BT 는 pauseWhenDisabled 라 재사용 때 실행 중이던 노드의 OnUpdate 부터 이어 돌며
    /// 이 필드를 읽는다(Kill 된 시퀀스 = 비활성 → 노드가 정상 종료).
    /// </summary>
    protected override void OnBeforeRelease()
    {
        if (behaviorTree != null)
        {
            behaviorTree.sequence?.Kill();
        }
    }

}
