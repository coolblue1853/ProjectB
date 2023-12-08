using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    Weapon equipWeapon;

    // Start is called before the first frame update
    void Start()
    {
        equipWeapon = transform.GetChild(0).GetComponent<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            equipWeapon.MeleeAttack();
        }
    }
}
