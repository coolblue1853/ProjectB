using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class SummonAttack : MonoBehaviour
{
    public int[] damgeArray = new int[10];
    public GameObject attackPrefab;
    public GameObject attackPivot;
   // public int attackCount =0;
   public float firstIntervel = 0;
    public float attackIntervel =0;
    public float holdingTime = 0;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetComponent<Animator>();
        Invoke("SummonAttackPrefab", firstIntervel);
        Invoke("DestroyObjet", holdingTime);

    }


    void DestroyObjet()
    {
        Destroy(this.gameObject);
    }

    void SummonAttackPrefab()
    {
        if (this.gameObject != null)
        {
            anim.SetBool("Attack", true);
            GameObject damageObject = Instantiate(attackPrefab, attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            Sequence seq = DOTween.Sequence()
            .AppendInterval(attackIntervel/2)
            .AppendCallback(() => anim.SetBool("Attack",false))
            .AppendInterval(attackIntervel / 2)
           .AppendCallback(() => SummonAttackPrefab());
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
