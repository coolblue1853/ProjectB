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
    InputAction[] skillAction = new InputAction[4];
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
        for (int i = 0; i < skillBackGround.Length; i++) skillAction[i].Enable();
    }
    private void OnDisable()
    {
        attackAction.Disable();
        for (int i = 0; i < skillBackGround.Length; i++) skillAction[i].Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        attackAction = action.Player.Attack;
        skillAction[0] = action.Player.SkillA;
        skillAction[1] = action.Player.SkillS;
        skillAction[2] = action.Player.SkillD;
        skillAction[3] = action.Player.SkillF;
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
            SkillTriggeredCheck();
        }
    }

    void SkillTriggeredCheck()
    {
        for (int i = 0; i < skillBackGround.Length; i++) if (skillAction[i].triggered && equipWeapon != null) equipWeapon.ActiveWeaponSkill(i);
    }
}
