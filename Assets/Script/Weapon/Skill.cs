using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public GameObject skillPivot;

   public bool isButtonDownSkill;
    public float SkillCoolTime;
    private void Awake()
    {

        skillCooldown = GameObject.FindWithTag("Cooldown").GetComponent<SkillCooldown>();
    }
    public GameObject[] skillprefab;
    public SkillCooldown skillCooldown;
    // Start is called before the first frame update
    void Start()
    {
        skillCooldown.cooldownTime = SkillCoolTime;
    }
    public void ActiveLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldown == false)
        {

            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
            skillCooldown.UseSkill();
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
