using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeNextAnim : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetComponent<Animator>();
    }
    public void GoNextAnim()
    {
        anim.SetBool("Next", true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
