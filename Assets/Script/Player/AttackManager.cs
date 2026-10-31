using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AnyPortrait;
public class AttackManager : MonoBehaviour
{
    public apPortrait portrait;
    PlayerController _pc;
    /// <summary>활성 캐릭터 리그 — 무기 소켓을 여기서 얻는다</summary>
    CharacterRig Rig
    {
        get
        {
            if (_pc == null)
            {
                _pc = FindFirstObjectByType<PlayerController>();
            }
            return _pc != null ? _pc.Rig : PortraitRig.For(portrait);
        }
    }
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
    // 슬롯별 키 해제 핸들러. 같은 델리게이트로 구독·해제해야 하므로 Awake 에서 한 번 만들어 둔다 (B-113, 설계 2.4-1)
    private System.Action<InputAction.CallbackContext>[] _releaseHandlers;

    private void OnEnable()
    {
        attackAction.Enable();
        for (int i = 0; i < skillAction.Length; i++)
        {
            skillAction[i].Enable();
            skillAction[i].canceled += _releaseHandlers[i];
        }
    }
    private void OnDisable()
    {
        attackAction.Disable();
        for (int i = 0; i < skillAction.Length; i++)
        {
            skillAction[i].canceled -= _releaseHandlers[i];
            skillAction[i].Disable();
        }
    }

    /// <summary>스킬 키를 떼면 그 슬롯에만 해제를 보낸다 — 다른 키를 떼도 홀드가 끝나던 문제의 근본 수정 (B-113)</summary>
    private void OnSkillKeyReleased(int slot)
    {
        if (equipWeapon != null)
        {
            equipWeapon.ReleaseWeaponSkill(slot);
        }
    }

    /// <summary>피격 경직·대시 시작 등 외부에서 홀드 스킬을 끊을 때</summary>
    public void CancelHold()
    {
        if (equipWeapon != null)
        {
            equipWeapon.CancelHold();
        }
    }
    private void Awake()
    {
        action = new KeyAction();
        attackAction = action.Player.Attack;
        skillAction[0] = action.Player.SkillA;
        skillAction[1] = action.Player.SkillS;
        skillAction[2] = action.Player.SkillD;
        skillAction[3] = action.Player.SkillF;
        _releaseHandlers = new System.Action<InputAction.CallbackContext>[skillAction.Length];
        for (int i = 0; i < _releaseHandlers.Length; i++)
        {
            int slot = i;
            _releaseHandlers[i] = ctx => OnSkillKeyReleased(slot);
        }
    }
    public void EquipMainWeaopon()
    {
        equipWeapon.pC = this.GetComponent<PlayerController>();
        equipWeapon.CheckSkill();
        equipWeapon.EquipWeqpon(Rig);
    }

    public void UnEquipMainWeaopon()
    {
        for (int i = 0; i < skillBackGround.Length; i++) skillBackGround[i].SetActive(false);
        SkillCooldown.instance.DeletSkillSprite();
        if (rightHand.transform.childCount != 0) Destroy(rightHand.transform.GetChild(0).gameObject);
        if (leftHand.transform.childCount != 0) Destroy(leftHand.transform.GetChild(0).gameObject);
        // 픽셀 리그는 소켓이 따로 있다 — 거기 붙은 무기 그림도 지운다(PixelRigDesign.md 3절)
        ClearSocket(Rig != null ? Rig.GetBoneSocket("RightWeapon") : null, rightHand);
        ClearSocket(Rig != null ? Rig.GetBoneSocket("LeftWeapon") : null, leftHand);
    }

    void ClearSocket(Transform socket, GameObject legacy)
    {
        if (socket == null || (legacy != null && socket == legacy.transform))
        {
            return;
        }
        for (int i = socket.childCount - 1; i >= 0; i--)
        {
            Destroy(socket.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 피격 경직 중에는 평타·스킬을 내지 않는다(슈퍼아머면 경직이 없어 그대로 가능, W-10). 입력은 버퍼에 남는다
        bool staggered = PlayerController.instance != null && PlayerController.instance.isAttacked;
        if (DatabaseManager.isOpenUI == false && equipWeapon != null)
        {
            // 평타 입력 0.1초 버퍼: 후딜이 끝나기 직전에 눌러도 콤보가 끊기지 않는다
            ChargeShot charge = equipWeapon.Charge;
            if (charge != null)
            {
                // 차지 무기(활): 누르는 동안 충전, 떼는 순간 평타 입력으로 바꾼다
                if (attackAction.triggered)
                {
                    charge.BeginHold(Time.time);
                }
                if (charge.Holding && !attackAction.IsPressed())
                {
                    charge.EndHold(Time.time);
                    attackBuffer.Press(Time.time);
                }
            }
            else if (attackAction.triggered)
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
            if (equipWeapon != null && equipWeapon.Charge != null)
            {
                equipWeapon.Charge.CancelHold(); // UI 가 열리면 충전을 버린다
            }
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
