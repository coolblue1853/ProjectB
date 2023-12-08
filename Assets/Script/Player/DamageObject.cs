using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageObject : MonoBehaviour
{
    public float holdingTime = 0;
    public float damage = 0;
    // Start is called before the first frame update
    void Start()
    {
        DestroyObject();
    }

    private void DestroyObject()
    {
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(holdingTime) 
        .AppendCallback(() => Destroy(this.gameObject));
    }
}
