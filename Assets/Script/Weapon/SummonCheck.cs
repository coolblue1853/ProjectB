using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class SummonCheck : MonoBehaviour
{
    public float[] summonPosition;
    public float[] time;
    public GameObject checkObject;
    GameObject[] checkBox;
    // Start is called before the first frame update
    void Start()
    {
        for(int i =0; i < summonPosition.Length; i++)
        {
            GameObject r = Instantiate(checkObject, this.transform.position, this.transform.rotation, this.transform);
            Rigidbody2D rb = r.GetComponent<Rigidbody2D>();

            rb.velocity = new Vector2(transform.localScale.x * (summonPosition[i]/ time[i]), 0f);



            Sequence dashSequence = DOTween.Sequence()
            .AppendInterval(time[i]) // 2초 대기
            .OnComplete(() => rb.gravityScale = 3f)
            .OnComplete(() => rb.velocity = new Vector2(0f, 0f));
        //    Sequence sequence = DOTween.Sequence()
         //   .Append(r.transform.DOMoveX(newPos, 1f).SetEase(Ease.Linear));// 원점으로 이동합니다.

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
