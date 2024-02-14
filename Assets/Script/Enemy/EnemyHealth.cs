using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class EnemyHealth : MonoBehaviour
{
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
        dropManager = transform.GetComponent<DropManager>();
           enemyFSM = transform.GetComponent<EnemyFSM>();
        rb = transform. GetComponent<Rigidbody2D>();
       // brain = transform.GetComponent<BTBrain>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
    }
    bool notStiff;
    public void damage2Enemy(int damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange)
    {
        ToggleObject();
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);

        if (nowHP <= 0)
        {

            dropManager.DropItems(transform.position    );
            Destroy(this.gameObject);
        }

        if(isSuperArmor == false)
        {
            sequence.Kill(); // 재공격시 경직 시간 초기화.
            if(enemyFSM != null)
            {

                if(stiffTime == 0)
                {
                    notStiff = true;
                }
                enemyFSM.KillBrainSequence(notStiff);
            }

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
    private void EndStiffness()
    {
        if(this != null)
        {
            Debug.Log("EndStiff");
            enemyFSM.StateChanger("Hit");
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
