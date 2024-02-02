using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackManager : MonoBehaviour
{
  public Weapon equipWeapon;
    public Weapon equipSideWeapon;
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

    }

    // Update is called once per frame
    void Update()
    {
        if (attackAction.triggered && equipWeapon != null)
        {
            equipWeapon.MeleeAttack();
        }
        else if (attackAction.triggered && equipSideWeapon != null)
        {
            equipSideWeapon.MeleeAttack();
        }
    }
}
