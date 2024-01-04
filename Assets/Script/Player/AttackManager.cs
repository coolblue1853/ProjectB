using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackManager : MonoBehaviour
{
    Weapon equipWeapon;

    KeyAction action;
    InputAction attackAction;

    public string states = "";
    private void OnEnable()
    {

        attackAction.Enable();
    }
    private void OnDisable()
    {

        attackAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        attackAction = action.Player.Attack;
    }
    // Start is called before the first frame update
    void Start()
    {
        equipWeapon = transform.GetChild(0).GetComponent<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attackAction.triggered)
        {
            equipWeapon.MeleeAttack();
        }
    }
}
