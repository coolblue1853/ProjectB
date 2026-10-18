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
        anim = transform.GetComponent<Animator>();
        dropManager = transform.GetComponent<DropManager>();
        rb = transform.GetComponent<Rigidbody2D>();
        behaviorTree = transform.GetComponent<BehaviorTree>();


    }
    DG.Tweening.Sequence wait;
    private void OnEnable()
    {
        ///isReleased = false;
        nowHP = maxHP;
        if (wait != null&& wait.IsPlaying() == true)
            wait.Kill();
        wait = DOTween.Sequence()
        .AppendInterval(0.3f)
        .AppendCallback(() => canDisapear = true);
        SpriteRenderer img = this.GetComponent<SpriteRenderer>();
        img.DOFade(1, 0);
        deadOnec = false;
        nowHP = maxHP;
        stiffShakeTime = 0;
        bleedStacks = 0;
        isBleeding = false;
        if (behaviorTree != null)
            behaviorTree.enabled = true; // 경직 중 반환되어 꺼진 채로 재사용되는 것 방지
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

        shakeTime = shaking;
        var result = DamageCalculator.Calculate(damage, DamageCalculator.PlayerOffense.FromDatabase(), enemyDef,
            isBleeding, isSkill, dmgRatio, UnityEngine.Random.value, UnityEngine.Random.Range(0f, 100f));
        isCrit = result.isCrit;
        finDmg = result.damage;
        addDmg = result.extraDamage;

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
        }
        if ((int)addDmg != 0) // 추가 데미지 판정
        {
           Damage((int)addDmg);
            HitFeedback.SpawnNumber(damageNumber, transform.position + Vector3.up, (int)addDmg, HitFeedback.NumberKind.Extra);
        }


        bool killed = TryDie();
        HitFeedback.OnEnemyHit(this, isCrit, isSkill, killed); // 플래시·히트스톱·흔들림
        if (killed)
        {
            return; // 사망 후에는 피격 연출(넉백·경직)을 걸지 않는다
        }

        if(isSuperArmor == false)
        {

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

    private void EndStiffness()
    {
        behaviorTree.enabled = true;
        isStun = false;
        if (this != null && behaviorTree != null)
        {
            if (anim != null)
            {
                anim.SetBool("isHit", false);
                if ( anim.GetBool("isAttack"))
                {
                    anim.SetBool("isAttack", false);
                }
            }

            isAttackGround = false;


        }

    }
    private void Update()
    {
        if(isBossMob == false && hpObject!= null)
        {
            hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
        }
        if(stiffShakeTime > 0)
        {
            stiffShakeTime -= Time.deltaTime;
        }
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
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
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

    public void CreatPoisonPrefab(int poisonDamage, float damageInterval,int damageCount)
    {
        if (this.gameObject != null)
        {
            GameObject poisonObject = Instantiate(poisonPrefab, transform.position, transform.rotation, this.transform);
            PosionAttack posionAttack = poisonObject.GetComponent<PosionAttack>();
            posionAttack.enemyHealth = this.GetComponent<EnemyHealth>();
            posionAttack.ActivePoison(poisonDamage, damageInterval, damageCount);
        }
    }
    public void CreatBleedingPrefab(int bleedingDamage, float bleedingDamageInterval, int bleedingDamageCount)
    {
        if(this.gameObject != null)
        {
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
        // 반환된 뒤에 지연 콜백이 재사용된 개체를 건드리지 않도록 정리
        disappearSequence?.Kill();
        striffSequence?.Kill();
        onlyShakeSeq?.Kill();
        wait?.Kill();
        transform.DOKill();
    }

}
