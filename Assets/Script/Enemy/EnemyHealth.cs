using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;
using DamageNumbersPro;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
public class EnemyHealth : MonoBehaviour
{
    public DamageNumber damageNumber;
    public int enemyDef = 0;
    Animator anim;
    public DG.Tweening.Sequence sequence;
    public int maxHP = 100;
    public int nowHP = 100;
    public HealthBarCut hpBar;
    public GameObject hpObject;
    public float hpHeight;
    public bool isSuperArmor;
    public GameObject hpCanvas;
    private Coroutine toggleCoroutine;
     Rigidbody2D rb;
    // �˹鿡 ���� ����
    Vector2 knockbackDirection = new Vector2(0f, 1f);
    public DropManager dropManager;
    EnemyFSM enemyFSM;
    BehaviorTree behaviorTree;

    public GameObject deadBody;
    public GameObject enemySensor;
    public bool isBossMob = false;

    private void Start()
    {

        anim = transform.GetComponent<Animator>();
        dropManager = transform.GetComponent<DropManager>();
       //    enemyFSM = transform.GetComponent<EnemyFSM>();
        rb = transform. GetComponent<Rigidbody2D>();
        // brain = transform.GetComponent<BTBrain>();
        behaviorTree = transform.GetComponent<BehaviorTree>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
        
        if (enemySensor != null)
        {
            if (checkPlayer.isPreemptive == true && enemySensor.activeSelf == false)
            {
                enemySensor.SetActive(true);
            }
        }
     
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
    float outDmg;
    float SetDmg(int[] damage, bool isSkill)
    {
        float baseDmg = Random.Range(damage[0], damage[1]);
        int checkCrit = Random.Range(1, 101);

        if(checkCrit <= damage[2]) // ġ��Ÿ ������
        {// �⺻ ġ�Ǵ� 20
            outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3]) / 100)) * (1 + ((damage[4]) / 100));  // �⺻�� * ġ�� * ����
        }
        else
        {
            outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]) / 100));  // �⺻�� * ����
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
    public void damage2Enemy(int[] damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange, bool isSkill, float shaking)
    {

        shakeTime = shaking;
        float dmg = SetDmg(damage, isSkill); // ���� ������ ������;

        if (damage[5] !=0 && enemyDef != 0)
        {
             dmgByDmg = dmg * (dmg / (dmg + (enemyDef * (damage[5] / 100)))); // �湫 ������
             finDmg = dmgByDmg * (1 + ((damage[7]) / 100)); // ������ �߰�
        }
        else
        {
            dmgByDmg = dmg * (dmg / (dmg + enemyDef)); // �湫 ������

            finDmg = dmgByDmg * (1 + ((damage[7]) / 100)); // ������ �߰�

        }

         addDmg = 0;
        if (damage[8] != 0)
        {
            float addFloat = damage[8];
            addDmg = finDmg  * ((addFloat / 100.0f)); // �ߵ� ���

        }
       // shakeTime = finDmg / maxHP* ShakeCorrection ;
       if(isBossMob == false)
        {
            ToggleObject();

        }

        nowHP -= (int)finDmg;
        hpBar.healthSystem.Damage((int)finDmg);
        damageNumber.Spawn(transform.position + Vector3.up, (int)finDmg);


        if (isSuperArmor == true)
        {
            transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
        }
        if ((int)addDmg != 0)
        {
            nowHP -= (int)addDmg;
            hpBar.healthSystem.Damage((int)addDmg);
            damageNumber.Spawn(transform.position + Vector3.up, (int)addDmg);
        }


        if (nowHP <= 0)
        {
            GameObject dB = Instantiate(deadBody, transform.transform.position, transform.transform.rotation);
            DeadBody dBB = dB.transform.GetComponent<DeadBody>();
            dBB.parentEnemy = this.transform.gameObject;
            dBB.Force2DeadBody(Mathf.Abs(nowHP));
            dropManager.DropItems(transform.position);
            Destroy(this.gameObject);
        }

        if(isSuperArmor == false)
        {
            striffSequence.Kill(); // ����ݽ� ���� �ð� �ʱ�ȭ.

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
               // StopAllActions();
                behaviorTree.enabled = false;

                //  enemyFSM.KillBrainSequence(notStiff);
            }


            isAttackGround = true;
            striffSequence = DOTween.Sequence()
            .AppendCallback(()=>transform.DOShakePosition(shakeTime, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false))
            .AppendInterval(shakeTime)
            .AppendCallback(() => KnockbackActive(force, knockbackDir, x, isDirChange))
            .AppendInterval(stiffTime)
            .OnComplete(() => EndStiffness());
        }




    }

    public void onlyDamage2Enemy(int damage)
    {
        ToggleObject();
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);

        if (nowHP <= 0)
        {
            GameObject dB = Instantiate(deadBody, transform.transform.position, transform.transform.rotation);
            dropManager.DropItems(transform.position);
            Destroy(this.gameObject);
        }


    }
   public bool isAttackGround;
    public CheckPlayer checkPlayer;
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
            }

            isAttackGround = false;

            if (enemyFSM.state.Contains("Hit"))
            {
                enemyFSM.StateChanger("Hit");
            }


            if (checkPlayer.isPreemptive == false && enemySensor.activeSelf == false)
            {
                ResetBoolParameters();
                enemySensor.SetActive(true);
               // enemyFSM.StateChanger("Chase");
                enemyFSM.ReActiveBrainSequence();
            }
            else if (enemyFSM.state.Contains("Chase")) // ���ݹ޾����� ���ʰ� �ƴϰ�, �񼱰� ���Ͷ�� �ٽ� �� �߰��� �����Ѵ�.
            {
               Debug.Log("11111111111111");
              // enemyFSM.StateChanger("Chase");
               enemyFSM.ReActiveBrainSequence();
            }
            else
            {
                enemyFSM.ReActiveBrainSequence();
            }



        }

    }
    private void Update()
    {
        if(isBossMob == false)
        {
            hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
        }

    }

    public float maxXSpeed =10f; // x�� �˹� �ӵ� ����
    public float maxYSpeed =10f; //y �� �˹� �ӵ� ����
    public bool isStun;
    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x, bool isDirChange)
    {
        isStun = true;
        Debug.Log("�˹�ߵ�");

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

        // ���ο� �ڷ�ƾ ����
        toggleCoroutine = StartCoroutine(ToggleObjectRoutine());
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

    public void CreatPoisonPrefab(int poisonDamage, float damageInterval,int damageCount)
    {
        GameObject poisonObject = Instantiate(poisonPrefab,transform.position, transform.rotation, this.transform);
        PosionAttack posionAttack = poisonObject.GetComponent<PosionAttack>();
        posionAttack.enemyHealth = this.GetComponent<EnemyHealth>();
        posionAttack.ActivePoison(poisonDamage, damageInterval, damageCount);

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

    public void DisaperByTime()
    {
        SpriteRenderer img = this.GetComponent<SpriteRenderer>();
        DG.Tweening.Sequence disSequence = DOTween.Sequence()
 .AppendCallback(() => img.DOFade(0, 1.5f))
 .AppendInterval(1.5f)
.OnComplete(() => Destroy(this.gameObject));
    }

}
