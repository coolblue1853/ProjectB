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
    public bool isBleeding = false;
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
    // 넉백에 사용될 방향
    Vector2 knockbackDirection = new Vector2(0f, 1f);
    public DropManager dropManager;
    EnemyFSM enemyFSM;
    BehaviorTree behaviorTree;

    public GameObject deadBody;
    public GameObject enemySensor;
    public bool isBossMob = false;
    private void Awake()
    {
        anim = transform.GetComponent<Animator>();
        dropManager = transform.GetComponent<DropManager>();
        //    enemyFSM = transform.GetComponent<EnemyFSM>();
        rb = transform.GetComponent<Rigidbody2D>();
        // brain = transform.GetComponent<BTBrain>();
        behaviorTree = transform.GetComponent<BehaviorTree>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);

    }
    private void Start()
    {



        if (enemySensor != null)
        {
            if (checkPlayer.isPreemptive == true && enemySensor.activeSelf == false)
            {
                enemySensor.SetActive(true);
            }
        }
     
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
    float outDmg;
    float SetDmg(int[] damage, bool isSkill)
    {
        // 특정 조건 만족시 뎀증
        // 출혈시 뎀증


        isCrit = false;
        float baseDmg = Random.Range(damage[0]+DatabaseManager.addbasicDmg, damage[1]+DatabaseManager.addbasicDmg);
        int checkCrit = Random.Range(1, 101);

        if(checkCrit <= damage[2]+ DatabaseManager.playerCritRate) // 치명타 성공시
        {// 기본 치피는 20
            isCrit = true;
            if (isBleeding)
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3] + DatabaseManager.playerCritDmgRate) / 100)) * (1 + ((damage[4]+DatabaseManager.bleedingAddDmg + DatabaseManager.incDmg) / 100));  // 기본뎀 * 치뎀 * 뎀증
            }
            else
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3] + DatabaseManager.playerCritDmgRate) / 100)) * (1 + ((damage[4] + DatabaseManager.incDmg) / 100));  // 기본뎀 * 치뎀 * 뎀증
            }

        }
        else
        {
            if (isBleeding)
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]+DatabaseManager.bleedingAddDmg) / 100));  // 기본뎀 * 뎀증
            }
            else
            {
                outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]) / 100));  // 기본뎀 * 뎀증
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
    {    // Null 체크 및 디버그 로그 추가
        if (damage == null)
        {
            Debug.LogError("damage array is null");
            return;
        }

        if (hpBar == null)
        {
            Debug.LogError("hpBar is null");
            return;
        }

        if (damageNumber == null)
        {
            Debug.LogError("damageNumber is null");
            return;
        }

        if (dropManager == null)
        {
            Debug.LogError("dropManager is null");
            return;
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D (rb) is null");
            return;
        }


        shakeTime = shaking;
        float dmg = SetDmg(damage, isSkill); // 방어력 적용전 데미지;

        if (damage[5] !=0 && enemyDef != 0)
        {
             dmgByDmg = dmg * (dmg / (dmg + (enemyDef * (damage[5] / 100)))); // 방무 적용후
            finDmg = dmgByDmg * (1 + ((damage[7]) / 100)) * dmgRatio; // 최종뎀 추가 * 계수
        }
        else
        {
            dmgByDmg = dmg * (dmg / (dmg + enemyDef)); // 방무 미적용

            finDmg = dmgByDmg * (1 + ((damage[7]) / 100)) * dmgRatio; // 최종뎀 추가 * 계수

        }

         addDmg = 0;
        if (damage[8] != 0)
        {
            float addFloat = damage[8];
            addDmg = finDmg  * ((addFloat / 100.0f)); // 추뎀 계산

        }
       // shakeTime = finDmg / maxHP* ShakeCorrection ;
       if(isBossMob == false)
        {
            ToggleObject();

        }

       if(hpBar != null)
        {
            hpBar.healthSystem.Damage((int)finDmg);
            nowHP -= (int)finDmg;
            if (isCrit) damageNumber.Spawn(transform.position + Vector3.up, (int)finDmg+"!");
            else damageNumber.Spawn(transform.position + Vector3.up, (int)finDmg);

        }



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


        if (nowHP <= 0 && deadOnec == false)
        {
            deadOnec = true;
            if(deadBody != null)
            {
                GameObject dB = Instantiate(deadBody, transform.transform.position, transform.transform.rotation);
                DeadBody dBB = dB.transform.GetComponent<DeadBody>();
                dBB.parentEnemy = this.transform.gameObject;
                dBB.Force2DeadBody(Mathf.Abs(nowHP));

            }

            dropManager.DropItems(transform.position);
            Destroy(this.gameObject);
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
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);

        if (nowHP <= 0 && deadOnec == false)
        {
            deadOnec = true;

            if(deadBody!= null)
            {
                GameObject dB = Instantiate(deadBody, transform.transform.position, transform.transform.rotation);
                DeadBody dBB = dB.transform.GetComponent<DeadBody>();
                dBB.parentEnemy = this.transform.gameObject;
                dBB.Force2DeadBody(Mathf.Abs(nowHP));
            }

            dropManager.DropItems(transform.position);
            Destroy(this.gameObject);
        }



    }
    public bool isAttackGround;
    public CheckPlayer checkPlayer;
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

            if (enemyFSM.state.Contains("Hit"))
            {
                enemyFSM.StateChanger("Hit");
            }
        }

    }
    private void Update()
    {
        if(isBossMob == false)
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
            rb.velocity = Vector2.zero;
            if (x> transform.position.x && isDirChange == true)
            {
                knockbackDir.x = -knockbackDir.x;
            }
            // 힘을 가해 넉백을 적용
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
            //    transform.DOShakePosition(0.15f, strength: new Vector3(0.04f, 0.04f, 0), vibrato: 30, randomness: 90, fadeOut: false);
            Vector2 currentVelocity = rb.velocity;


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
            rb.velocity = currentVelocity;
        }

    }
    private void ToggleObject()
    {
        // 이전에 실행 중인 코루틴이 있다면 중지
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }

        // 새로운 코루틴 시작
        toggleCoroutine = StartCoroutine(ToggleObjectRoutine());
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

    public void DisaperByTime()
    {
        SpriteRenderer img = this.GetComponent<SpriteRenderer>();
        DG.Tweening.Sequence disSequence = DOTween.Sequence()
 .AppendCallback(() => img.DOFade(0, 1.5f))
 .AppendInterval(1.5f)
.OnComplete(() => Destroy(this.gameObject));
    }

}
