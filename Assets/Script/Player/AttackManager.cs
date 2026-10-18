using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AnyPortrait;
public class AttackManager : MonoBehaviour
{
    public apPortrait portrait;
    public Weapon equipWeapon;
    [Tooltip("평타 선입력 시간(초). 후딜이 끝나기 직전에 누른 평타를 이어 준다 (W-15)")]
    [SerializeField] private float _attackBufferTime = 0.2f;
    InputBuffer _attackBuffer;
    InputBuffer attackBuffer => _attackBuffer ?? (_attackBuffer = new InputBuffer(_attackBufferTime));
    [Tooltip("스킬 선입력 시간(초). 후딜 중 누른 스킬을 이 시간 안에 쓸 수 있게 되면 발동 (W-09)")]
    [SerializeField] private float _skillBufferTime = 0.2f;
    InputBuffer[] skillBuffers;
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
        // 피격 경직 중에는 평타·스킬을 내지 않는다(슈퍼아머면 경직이 없어 그대로 가능, W-10). 입력은 버퍼에 남는다
        bool staggered = PlayerController.instance != null && PlayerController.instance.isAttacked;
        if (DatabaseManager.isOpenUI == false && equipWeapon != null)
        {
            // 평타 입력 0.1초 버퍼: 후딜이 끝나기 직전에 눌러도 콤보가 끊기지 않는다
            if (attackAction.triggered)
            {
                attackBuffer.Press(Time.time);
            }
            if (!staggered && equipWeapon.CanMeleeNow && attackBuffer.Consume(Time.time))
            {
                equipWeapon.MeleeAttack();
            }
            SkillTriggeredCheck(staggered);
        }
        else
        {
            attackBuffer.Clear();
            ClearSkillBuffers();
        }
    }

    void SkillTriggeredCheck(bool staggered)
    {
        if (skillBuffers == null || skillBuffers.Length != skillBackGround.Length)
        {
            skillBuffers = new InputBuffer[skillBackGround.Length];
            for (int i = 0; i < skillBuffers.Length; i++)
            {
                skillBuffers[i] = new InputBuffer(_skillBufferTime);
            }
        }
        for (int i = 0; i < skillBackGround.Length; i++)
        {
            Skill s = i < equipWeapon.skill.Length ? equipWeapon.skill[i] : null;
            if (s == null)
            {
                continue;
            }
            if (skillAction[i].triggered)
            {
                skillBuffers[i].Press(Time.time);
            }
            // 홀드 스킬은 키를 누르고 있는 동안만 의미가 있으므로 즉시 판정만 한다(버퍼로 늦게 시작하면 떼기를 놓친다)
            bool want = s.isHoldSkill ? skillAction[i].triggered : skillBuffers[i].Peek(Time.time);
            if (want && !staggered && s.CanActivate(i))
            {
                skillBuffers[i].Clear();
                equipWeapon.ActiveWeaponSkill(i);
            }
        }
    }

    void ClearSkillBuffers()
    {
        if (skillBuffers == null)
        {
            return;
        }
        foreach (InputBuffer b in skillBuffers)
        {
            b.Clear();
        }
    }
}
