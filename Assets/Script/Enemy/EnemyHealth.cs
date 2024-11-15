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
    public event System.Action<int,int,Vector3, bool, bool> OnReleasedToPool; // �̺�Ʈ ����
    public EnemySpawner enemySpowner;
    public bool isBleeding = false;
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
    // �˹鿡 ���� ����
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


    /*  0~ 9������ �������
   public int minDmg=0;        // �ּҴ� 0
    public int maxDmg=0;       // �ִ��  1
    public int critPer = 0;   //  ġ�� Ȯ��% 2 
    public int critDmg = 0;   //  ġ������% 3
    public int incDmg = 0;   // ����������% 4
    public int ignDef = 0; // ���� ����% 5
    public int skillDmg = 0; // ��ų ���ݷ�% 6
    public int finDmg = 0; // ������%   7
    public int addDmg = 0; // �߰���%   8
    public int ArmAtp = 0; // �� �⺻�� 9  
     */
    bool isCrit = false;
    float outDmg;
    float SetDmg(int[] damage, bool isSkill)
    {
        // Ư�� ���� ������ ����
        // ������ ����


        isCrit = false;
        float baseDmg = UnityEngine.Random.Range(damage[0]+DatabaseManager.addbasicDmg, damage[1]+DatabaseManager.addbasicDmg);
        int checkCrit = UnityEngine.Random.Range(1, 101);

        if(checkCrit <= damage[2]+ DatabaseManager.playerCritRate) // ġ��Ÿ ������
        {// �⺻ ġ�Ǵ� 20
            isCrit = true;
            if (isBleeding)
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3] + DatabaseManager.playerCritDmgRate) / 100)) * (1 + ((damage[4]+DatabaseManager.bleedingAddDmg + DatabaseManager.incDmg) / 100));  // �⺻�� * ġ�� * ����
            }
            else
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3] + DatabaseManager.playerCritDmgRate) / 100)) * (1 + ((damage[4] + DatabaseManager.incDmg) / 100));  // �⺻�� * ġ�� * ����
            }

        }
        else
        {
            if (isBleeding)
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]+DatabaseManager.bleedingAddDmg) / 100));  // �⺻�� * ����
            }
            else
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]) / 100));  // �⺻�� * ����
            }

        }

        if (isSkill == true)
        {
            outDmg *= 1 + ((damage[6]) / 100);
        }

        return outDmg;
    }
    float dmgByDmg;
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
        float dmg = SetDmg(damage, isSkill); // ���� ������ ������;

        if (damage[5] !=0 && enemyDef != 0)
        {
             dmgByDmg = dmg * (dmg / (dmg + (enemyDef * (damage[5] / 100)))); // �湫 ������
            finDmg = dmgByDmg * (1 + ((damage[7]) / 100)) * dmgRatio; // ������ �߰� * ���
        }
        else
        {
            dmgByDmg = dmg * (dmg / (dmg + enemyDef)); // �湫 ������

            finDmg = dmgByDmg * (1 + ((damage[7]) / 100)) * dmgRatio; // ������ �߰� * ���

        }

         addDmg = 0;
        if (damage[8] != 0)
        {
            float addFloat = damage[8];
            addDmg = finDmg  * ((addFloat / 100.0f)); // �ߵ� ���

        }

       if(isBossMob == false)
        {
            ToggleObject();

        }

       // ����������
        Damage((int)finDmg);
        if (isCrit) damageNumber.Spawn(transform.position + Vector3.up, (int)finDmg + "!");
        else damageNumber.Spawn(transform.position + Vector3.up, (int)finDmg);


        if (isSuperArmor == true)
        {
            transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
        }
        if ((int)addDmg != 0) // �߰� ������ ����
        {
           Damage((int)addDmg);
            damageNumber.Spawn(transform.position + Vector3.up, (int)addDmg);
        }


        if (nowHP <= 0 && deadOnec == false)
        {
            deadOnec = true;
            if (pool.CountInactive == 0)
            {
                Destroy(gameObject);

            }
            else
            {
                ReleaseObject();
            }
        }

        if(isSuperArmor == false)
        {

            if (anim != null)
            {
                anim.SetBool("isHit", true);
            }
            if (behaviorTree != null) // ����enemyFSM ����
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
                striffSequence.Kill(); // ����ݽ� ���� �ð� �ʱ�ȭ.
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
                onlyShakeSeq.Kill(); // ����ݽ� ���� �ð� �ʱ�ȭ.
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
        if (nowHP <= 0 && deadOnec == false)
        {
            deadOnec = true;
            if (pool.CountInactive == 0)
            {
                Destroy(gameObject);

            }
            else
            {
                ReleaseObject();
            }
        }



    }
    public void ReleaseEnemy(bool isDrop = true, bool isItemDrop = true)
    {
        canDisapear = false;
        OnReleasedToPool?.Invoke(enemyNum, Mathf.Abs(nowHP),this.transform.position, isDrop, isItemDrop); // 4��° ��Ҵ� deadbody�� �����Ұ����� ��������


        foreach (Transform damagedBar in hpBar.damagedBars)
        {
            if (damagedBar != null)
                Destroy(damagedBar.gameObject);
        }
        hpBar.damagedBars.Clear(); // ����Ʈ �ʱ�ȭ
        if (pool.CountInactive == 0)
        {
            Destroy(gameObject);

        }
        else
        {
            ReleaseObject();
        }
    }
    public bool isAttackGround;

    public void ResetBoolParameters()
    {
        // �ִϸ����Ϳ� ��ϵ� ��� �Ķ���� ��������
        AnimatorControllerParameter[] parameters = anim.parameters;

        // ��� �Ҹ��� �Ķ������ ���� false�� ����
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

    public float maxXSpeed =10f; // x�� �˹� �ӵ� ����
    public float maxYSpeed =10f; //y �� �˹� �ӵ� ����
    public bool isStun;
    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x, bool isDirChange)
    {
        isStun = true;


        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            if (x> transform.position.x && isDirChange == true)
            {
                knockbackDir.x = -knockbackDir.x;
            }
            // ���� ���� �˹��� ����
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
            //    transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
            Vector2 currentVelocity = rb.velocity;


            // x �ӵ��� �ִ밪�� �ʰ��ϴ��� Ȯ���ϰ�, �ʰ��ϸ� �ִ밪���� ����
            if (Mathf.Abs(currentVelocity.x) > maxXSpeed)
            {
                currentVelocity.x = Mathf.Sign(currentVelocity.x) * maxXSpeed;
            }

            // y �ӵ��� �ִ밪�� �ʰ��ϴ��� Ȯ���ϰ�, �ʰ��ϸ� �ִ밪���� ����
            if (Mathf.Abs(currentVelocity.y) > maxYSpeed)
            {
                currentVelocity.y = Mathf.Sign(currentVelocity.y) * maxYSpeed;
            }

            // ���� �ӵ� ����
            rb.velocity = currentVelocity;
        }

    }
    private void ToggleObject()
    {
        // ������ ���� ���� �ڷ�ƾ�� �ִٸ� ����
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }
        if(this.gameObject != null&& this.gameObject.activeSelf != false)
        {
            // ���ο� �ڷ�ƾ ����
            toggleCoroutine = StartCoroutine(ToggleObjectRoutine());
        }

    }

    private IEnumerator ToggleObjectRoutine()
    {
        // ������Ʈ�� 2�� ���� �ѱ�
        hpCanvas.SetActive(true);

        // 2�� ���
        yield return new WaitForSeconds(2f);

        // ������Ʈ�� ���ֱ�
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
        // GetActiveTasks �޼��尡 null�� ��ȯ�� �� �����Ƿ� ���� ó��
        List<BehaviorDesigner.Runtime.Tasks.Task> activeTasks = behaviorTree.GetActiveTasks();
        if (activeTasks != null)
        {
            // Behavior Tree���� ���� ���� ��� Task�� ã�Ƽ� ����
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
    public void ForceEnemyRelease()
    {
        ReleaseObject();
    }
    public void DisaperByTime()
    {
        if(this.gameObject.activeSelf != false)
        {
            SpriteRenderer img = this.GetComponent<SpriteRenderer>();
            DG.Tweening.Sequence disSequence = DOTween.Sequence()
            .AppendCallback(() => img.DOFade(0, 1.5f))
            .AppendInterval(1.5f)
           .OnComplete(() => ReleaseEnemy(false, false));
        }

    }

}
