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
    public GameObject rightHand; // 무기를 장착하는 소켓 오브젝트
    public GameObject leftHand;

    public PlayerBuff foodBuff;
    public GameObject BuffSlot;

    public GameObject[] skillBackGround = new GameObject[4];
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
    public void EquipMainWeaopon()
    {
        equipWeapon.pC = this.GetComponent<PlayerController>();
        equipWeapon.CheckSkill();
        equipWeapon.EquipWeqpon(portrait);
    }

    public void UnEquipMainWeaopon()
    {
        for (int i = 0; i < skillBackGround.Length; i++) skillBackGround[i].SetActive(false);
        SkillCooldown.instance.DeletSkillSprite();
        if (rightHand.transform.childCount != 0) Destroy(rightHand.transform.GetChild(0).gameObject);
        if (leftHand.transform.childCount != 0) Destroy(leftHand.transform.GetChild(0).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (DatabaseManager.isOpenUI == false && equipWeapon != null)
        {
            if (attackAction.triggered ) equipWeapon.MeleeAttack();
            else if (skillAAction.triggered && equipWeapon != null) equipWeapon.ActiveASkill();
            else if (skillSAction.triggered && equipWeapon != null) equipWeapon.ActiveBSkill();
            else if (skillDAction.triggered && equipWeapon != null) equipWeapon.ActiveCSkill();
            else if (skillFAction.triggered && equipWeapon != null) equipWeapon.ActiveDSkill();
        }
    }
}
