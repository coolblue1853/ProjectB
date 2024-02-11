using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public GameObject skillPivot;

   public bool isButtonDownSkill;

    public GameObject[] skillprefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void ActiveLeft()
    {

        if (isButtonDownSkill)
        {

            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
        }

    }
    public void ActiveRight()
    {

        if (isButtonDownSkill)
        {

            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
