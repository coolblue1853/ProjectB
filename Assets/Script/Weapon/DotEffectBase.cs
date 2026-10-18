using System.Collections;
using UnityEngine;
using DamageNumbersPro;

/// <summary>
/// 지속 피해(독·출혈) 공통 베이스. 몬스터의 자식으로 생성되어 즉시 1회 + interval마다 피해를 준다(총 count회).
/// 몬스터가 죽거나 풀로 반환되면(OnDisable) 즉시 종료·파괴된다 — 반환 후 Invoke가 재사용 개체를 때리던 문제 수정(B-206).
/// </summary>
public abstract class DotEffectBase : MonoBehaviour
{
    public DamageNumber damageNumber;
    public EnemyHealth enemyHealth;

    private Coroutine routine;
    private bool ended;

    /// <summary>기존 프리팹·호출부 호환을 위해 이름 유지</summary>
    public void ActivePoison(int baseDamage, float interval, int count)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(Run(baseDamage, Mathf.Max(0.05f, interval), count));
    }

    protected virtual int TickDamage(int baseDamage) => baseDamage;
    protected virtual void OnBegin() { }
    protected virtual void OnTick() { }
    protected virtual void OnEnd() { }

    private IEnumerator Run(int baseDamage, float interval, int count)
    {
        OnBegin();
        var wait = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            if (enemyHealth == null || !enemyHealth.isActiveAndEnabled || enemyHealth.nowHP <= 0)
            {
                break;
            }
            int damage = TickDamage(baseDamage);
            enemyHealth.onlyDamage2Enemy(damage);
            if (damageNumber != null)
            {
                damageNumber.Spawn(transform.position + Vector3.up, damage);
            }
            OnTick();
            yield return wait;
        }
        End();
    }

    private void End()
    {
        if (ended)
        {
            return;
        }
        ended = true;
        OnEnd();
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        End();
    }
}
