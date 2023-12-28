using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Weapon : MonoBehaviour
{
    public int maxComboCount = 0 ;
    public int nowConboCount = 0;
    public float maxComboTime = 0; // �޺� �ִ� �����ð�
    public float attckSpeed = 0;   // ���� �ֱ�, ª������ �� ������ ���� ����.

    public bool isAttackWait = false;
    public bool isWeaponStopMove = false;
    public float time;
    public GameObject attackPivot;

    public GameObject[] attackPrefab;
    private void Start()
    {

    }
    public void MeleeAttack()
    {
        time = 0;
        if (nowConboCount == 0 )
        {
            nowConboCount++;
            //������ ��ȯ
            GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation,this.transform);
            CheckAttackWait();
        }
        else
        {
            if (nowConboCount < maxComboCount && time <= maxComboTime && isAttackWait == true)
            {
                nowConboCount++;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
                CheckAttackWait();
            }
            else if (nowConboCount >= maxComboCount && time <= maxComboTime && isAttackWait == true) // �޺��� �ʱ�ȭ �� �ٽ� ī��Ʈ 1�� ������.
            {
                nowConboCount = 1;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
                CheckAttackWait();
            }
        }

    }
    private void CheckAttackWait()
    {
        if(isWeaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = true;
        }
        isAttackWait = false;
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(attckSpeed) // ������ ������ ���� �ֱ⸸ŭ ���.
        .AppendCallback(() => DatabaseManager.weaponStopMove = false)
        .AppendCallback(() => isAttackWait = true);
    }

    private void Update()
    {
        if(nowConboCount!= 0 && time < maxComboTime)
        {
            time += Time.deltaTime;
        }
        else if(time > maxComboTime && time != 0)
        {
            nowConboCount = 0;
               time = 0;
        }
    }
}
