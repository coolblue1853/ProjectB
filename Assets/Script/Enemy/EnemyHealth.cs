using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 100;
    private int nowHP = 0;
    public HealthBarCut hpBar;
    public GameObject hpObject;
    public float hpHeight;

    public GameObject hpCanvas;
    private Coroutine toggleCoroutine;

    private void Start()
    {
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
    }
    public void damage2Enemy(int damage)
    {
        ToggleObject();
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);
        if (nowHP <= 0)
        {
            Destroy(this.gameObject);
        }
    }
    private void Update()
    {
        hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
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
