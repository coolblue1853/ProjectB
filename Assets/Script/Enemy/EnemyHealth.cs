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
    // �˹鿡 ���� ����
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
            sequence.Kill(); // ����ݽ� ���� �ð� �ʱ�ȭ.
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
            // ���� ���� �˹��� ����
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
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

}
