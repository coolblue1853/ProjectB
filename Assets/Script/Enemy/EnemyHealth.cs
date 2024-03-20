using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using DamageNumbersPro;
public class EnemyHealth : MonoBehaviour
{
    public DamageNumber damageNumber;
    public int enemyDef = 0;
    Animator anim;
    public Sequence sequence;
    public int maxHP = 100;
    private int nowHP = 0;
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
    private void Start()
    {
        anim = transform.GetComponent<Animator>();
        dropManager = transform.GetComponent<DropManager>();
           enemyFSM = transform.GetComponent<EnemyFSM>();
        rb = transform. GetComponent<Rigidbody2D>();
       // brain = transform.GetComponent<BTBrain>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
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
    int outDmg;
    int SetDmg(int[] damage, bool isSkill)
    {
        int baseDmg = Random.Range(damage[0], damage[1]);
        int checkCrit = Random.Range(1, 101);

        if(checkCrit <= damage[2]) // 치명타 성공시
        {// 기본 치피는 20
            outDmg = (baseDmg + damage[9]) * (1 + ((20 + damage[3]) / 100)) * (1 + ((damage[4]) / 100));  // 기본뎀 * 치뎀 * 뎀증
        }
        else
        {
            outDmg = (baseDmg + damage[9]) * (1 + ((damage[4]) / 100));  // 기본뎀 * 뎀증
        }

        if (isSkill == true)
        {
            outDmg *= 1 + ((damage[6]) / 100);
        }

        return outDmg;
    }
    int dmgByDmg;
    int finDmg;
    public void damage2Enemy(int[] damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange, bool isSkill)
    {
        int dmg = SetDmg(damage, isSkill); // 방어력 적용전 데미지;
        if(damage[5] !=0 && enemyDef != 0)
        {
             dmgByDmg = (dmg / (dmg + (enemyDef * (damage[5] / 100)))); // 방무 적용후
             finDmg = dmgByDmg * (1 + ((damage[7]) / 100)); // 최종뎀 추가
        }
        else
        {
            finDmg = dmg * (1 + ((damage[7]) / 100)); // 최종뎀 추가
        }

        int addDmg = 0;
        if (damage[9] != 0)
        {
            addDmg = finDmg  * (((damage[7]) / 100)); // 추뎀 계산
        } 

        ToggleObject();
        nowHP -= finDmg;
        hpBar.healthSystem.Damage(finDmg);
       damageNumber.Spawn(transform.position + Vector3.up, finDmg);
        if (addDmg != 0)
        {
            nowHP -= addDmg;
            hpBar.healthSystem.Damage(addDmg);
           damageNumber.Spawn(transform.position, addDmg);
        }


        if (nowHP <= 0)
        {

            dropManager.DropItems(transform.position    );
            Destroy(this.gameObject);
        }

        if(isSuperArmor == false)
        {
            sequence.Kill(); // 재공격시 경직 시간 초기화.

            if (anim != null)
            {
                anim.SetBool("isHit", true);
            }
            if (enemyFSM != null)
            {

                if(stiffTime == 0)
                {
                    notStiff = true;
                }
                enemyFSM.KillBrainSequence(notStiff);
            }
            isAttackGround = true;
                sequence = DOTween.Sequence()
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
            dropManager.DropItems(transform.position);
            Destroy(this.gameObject);
        }


    }
   public bool isAttackGround;
    private void EndStiffness()
    {
        if(this != null && enemyFSM != null)
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

           enemyFSM.ReActiveBrainSequence();


        }

    }
    private void Update()
    {
        hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
    }


    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x, bool isDirChange)
    {

        if (rb != null)
        {
            if(x> transform.position.x && isDirChange == true)
            {
                knockbackDir.x = -knockbackDir.x;
            }
            // 힘을 가해 넉백을 적용
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
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

    public void CreatPoisonPrefab(int poisonDamage, float damageInterval,int damageCount)
    {
        GameObject poisonObject = Instantiate(poisonPrefab,transform.position, transform.rotation, this.transform);
        PosionAttack posionAttack = poisonObject.GetComponent<PosionAttack>();
        posionAttack.enemyHealth = this.GetComponent<EnemyHealth>();
        posionAttack.ActivePoison(poisonDamage, damageInterval, damageCount);

    }

}
