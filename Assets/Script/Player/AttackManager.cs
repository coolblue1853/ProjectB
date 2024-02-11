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
    InputAction skillAAction;
    InputAction skillSAction;

    public string states = "";
    private void OnEnable()
    {
        attackAction.Enable();
        skillAAction.Enable();
        skillSAction.Enable();
    }
    private void OnDisable()
    {
        attackAction.Disable();
        skillAAction.Disable();
        skillSAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        attackAction = action.Player.Attack;
        skillAAction = action.Player.SkillA;
        skillSAction = action.Player.SkillS;
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

        else if (skillAAction.triggered && equipWeapon != null)
        {
            equipWeapon.ActiveLeftSkill();
        }
        else if (skillSAction.triggered && equipWeapon != null)
        {
            equipWeapon.ActiveRightSkill();
        }
        
    }
}
