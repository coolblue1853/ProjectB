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

    BTBrain brain;
    private void Start()
    {
        brain = transform.GetComponent<BTBrain>();
        nowHP = maxHP;
        hpBar.setHpBar(maxHP);
    }
    public void damage2Enemy(int damage, float stiffTime)
    {

        brain.isAttacked = true;
        if(isSuperArmor == false)
        {
            sequence.Kill(); // 재공격시 경직 시간 초기화.
            brain.KillAllTweensForObject();
            sequence = DOTween.Sequence()
            .AppendInterval(stiffTime)
            .OnComplete(() => EndStiffness());
        }

        brain.StopEvaluateCoroutine();
        ToggleObject();
        nowHP -= damage;
        hpBar.healthSystem.Damage(damage);
        if (nowHP <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void EndStiffness()
    {
        brain.restartEvaluate();
    }
    private void Update()
    {
        hpObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, hpHeight, 0));
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
}
