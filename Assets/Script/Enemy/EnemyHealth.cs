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
    public BTBrain brain;

     EnemyFSM enemyFSM;
    private void Start()
    {
        enemyFSM = transform.GetComponent<EnemyFSM>();
        rb = transform. GetComponent<Rigidbody2D>();
       // brain = transform.GetComponent<BTBrain>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
    }
    public void damage2Enemy(int damage, float stiffTime, float force, Vector2 knockbackDir, float x)
    {
        if (nowHP <= 0)
        {
            Destroy(this.gameObject);
        }
            brain.isAttacked = true;
        if(isSuperArmor == false)
        {
            sequence.Kill(); // ����ݽ� ���� �ð� �ʱ�ȭ.
            brain.KillAllTweensForObject();
            sequence = DOTween.Sequence()
            .AppendCallback(() => KnockbackActive(force, knockbackDir, x))
            .AppendInterval(stiffTime)
            .OnComplete(() => EndStiffness());
        }

        brain.StopEvaluateCoroutine();
        ToggleObject();
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);

    }

    private void EndStiffness()
    {
        if(this != null)
        {
            enemyFSM.StateChanger("Hit");
        }

    }
    private void Update()
    {
        hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
    }


    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x)
    {

        if (rb != null)
        {
            if(x> transform.position.x)
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
}
