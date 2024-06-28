using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AnyPortrait;
public class AttackManager : MonoBehaviour
{
    public apPortrait portrait;
    public Weapon equipWeapon;
    KeyAction action;
    InputAction attackAction;
    InputAction skillAAction;
    InputAction skillSAction;
    InputAction skillDAction;
    InputAction skillFAction;
    public SkillCooldown skillCooldown;
    public Transform sword;
    public string states = "";
    public GameObject rightHand;
    public GameObject leftHand;

    public PlayerBuff foodBuff;

    public GameObject BuffSlot;
    private void OnEnable()
    {
        attackAction.Enable();
        skillAAction.Enable();
        skillSAction.Enable();
        skillDAction.Enable();
        skillFAction.Enable();
    }
    private void OnDisable()
    {
        attackAction.Disable();
        skillAAction.Disable();
        skillSAction.Disable();
        skillDAction.Disable();
        skillFAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        attackAction = action.Player.Attack;
        skillAAction = action.Player.SkillA;
        skillSAction = action.Player.SkillS;
        skillDAction = action.Player.SkillD;
        skillFAction = action.Player.SkillF;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void EquipMainWeaopon()
    {
        equipWeapon.pC = this.GetComponent<PlayerController>();
        equipWeapon.CheckSkill();
        equipWeapon.EquipWeqpon(portrait);
    }
    public void UnEquipMainWeaopon()
    {
        SkillCooldown.instance.DeletLeftSkill();
        SkillCooldown.instance.DeletRightSkill();
        if (rightHand.transform.childCount != 0)
        {
            Destroy(rightHand.transform.GetChild(0).gameObject);
        }
        if (leftHand.transform.childCount != 0)
        {
            Destroy(leftHand.transform.GetChild(0).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {/*
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 앞서 만든 두개의 본의 소켓을 가져옵니다.
            Transform socketR = portrait.GetBoneSocket("RightWeapon");
            // 검 (Weapon_Sword)을 오른손 본의 소켓의 자식으로 등록합니다.
            sword.parent = socketR;
            sword.localPosition = Vector3.zero;
            sword.localRotation = Quaternion.identity;

        }
        */

        if (DatabaseManager.isOpenUI == false)
        {
            if (attackAction.triggered && equipWeapon != null)
            {
                equipWeapon.MeleeAttack();
            }
      

            else if (skillAAction.triggered && equipWeapon != null)
            {
                equipWeapon.ActiveASkill();
            }
            else if (skillSAction.triggered && equipWeapon != null)
            {
                equipWeapon.ActiveBSkill();
            }
            else if (skillDAction.triggered && equipWeapon != null)
            {
                equipWeapon.ActiveCSkill();
            }
            else if (skillFAction.triggered && equipWeapon != null)
            {
                equipWeapon.ActiveDSkill();
            }
    
        }


    }
}
