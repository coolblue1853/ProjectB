using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImageA; // 쿨타임 이미지
    public float cooldownTimeA = 0f; // 쿨타임 시간 (초)
    public bool isCooldownA = false; // 쿨타임 중인지 여부 확인을 위한 변수
    private float cooldownTimerA = 0f; // 쿨타임 타이머

    public Image cooldownImageB; // 쿨타임 이미지
    public float cooldownTimeB = 0f; // 쿨타임 시간 (초)
    public bool isCooldownB = false; // 쿨타임 중인지 여부 확인을 위한 변수
    private float cooldownTimerB = 0f; // 쿨타임 타이머

    public Image cooldownImageC; // 쿨타임 이미지
    public float cooldownTimeC = 0f; // 쿨타임 시간 (초)
    public bool isCooldownC = false; // 쿨타임 중인지 여부 확인을 위한 변수
    private float cooldownTimerC = 0f; // 쿨타임 타이머

    public Image cooldownImageD; // 쿨타임 이미지
    public float cooldownTimeD = 0f; // 쿨타임 시간 (초)
    public bool isCooldownD = false; // 쿨타임 중인지 여부 확인을 위한 변수
    private float cooldownTimerD = 0f; // 쿨타임 타이머

    

    static public SkillCooldown instance;

   

    public void ResetCoolTime()
    {
        cooldownImageA.sprite = null;
        cooldownImageB.sprite = null;
        cooldownImageC.sprite = null;
        cooldownImageD.sprite = null;
        cooldownTimeA = 0;
        isCooldownA = false;
        cooldownTimerA = 0;
        cooldownTimeB = 0;
        isCooldownB = false;
        cooldownTimerB = 0;
        cooldownTimeC = 0;
        isCooldownC = false;
        cooldownTimerC = 0;
        cooldownTimeD = 0;
        isCooldownD = false;
        cooldownTimerD = 0;
    }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        //    ItemSheet.Data.Load();
        // UnityGoogleSheet.LoadAllData();
    }
    void Update()
    {
        // 쿨타임 중이라면
        if (isCooldownA && cooldownImageA != null)
        {
            cooldownTimerA -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.

            cooldownImageA.fillAmount = 1-(cooldownTimerA / cooldownTimeA); // 이미지를 업데이트하여 쿨타임을 표시합니다.

            // 쿨타임이 완료되었을 때
            if (cooldownTimerA <= 0)
            {
                isCooldownA = false; // 쿨타임 상태를 해제합니다.
                cooldownImageA.fillAmount = 1; // 이미지를 가득 채웁니다.
            }
        }
        // 쿨타임 중이라면
        if (isCooldownB && cooldownImageB != null)
        {
            cooldownTimerB -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
            cooldownImageB.fillAmount = 1 - (cooldownTimerB / cooldownTimeB); // 이미지를 업데이트하여 쿨타임을 표시합니다.

            // 쿨타임이 완료되었을 때
            if (cooldownTimerB <= 0)
            {
                isCooldownB = false; // 쿨타임 상태를 해제합니다.
                cooldownImageB.fillAmount = 1; // 이미지를 가득 채웁니다.
            }
        }

        if (isCooldownC && cooldownImageC != null)
        {
            cooldownTimerC -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
            cooldownImageC.fillAmount = 1 - (cooldownTimerC / cooldownTimeC); // 이미지를 업데이트하여 쿨타임을 표시합니다.

            // 쿨타임이 완료되었을 때
            if (cooldownTimerC <= 0)
            {
                isCooldownC = false; // 쿨타임 상태를 해제합니다.
                cooldownImageC.fillAmount = 1; // 이미지를 가득 채웁니다.
            }
        }

        if (isCooldownD && cooldownImageD != null)
        {
            cooldownTimerD -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
            cooldownImageD.fillAmount = 1 - (cooldownTimerD / cooldownTimeD); // 이미지를 업데이트하여 쿨타임을 표시합니다.

            // 쿨타임이 완료되었을 때
            if (cooldownTimerD <= 0)
            {
                isCooldownD = false; // 쿨타임 상태를 해제합니다.
                cooldownImageD.fillAmount = 1; // 이미지를 가득 채웁니다.
            }
        }
    }
    public void DeletLeftSkill()
    {
        cooldownImageA.sprite = null;
        cooldownImageB.sprite = null;
        //  cooldownImageA.fillAmount = 1;
    }
    public void DeletRightSkill()
    {
        cooldownImageC.sprite = null;
        cooldownImageD.sprite = null;
    }
    // 스킬을 사용할 때 호출되는 함수
    public void UseSkillA(string skillName)
    {
        if (!isCooldownA) // 쿨타임 중이 아니라면
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }

            isCooldownA = true; // 쿨타임을 시작합니다.
            if(cooldownPower == 0)
            {
                cooldownTimerA = cooldownTimeA; // 쿨타임 타이머를 초기화합니다.
            }
            else
            {
         
                cooldownTimerA = cooldownTimeA*(1- cooldownPower/100); // 쿨타임 타이머를 초기화합니다.
                Debug.Log( (cooldownTimeA * (1 - cooldownPower / 100)));
            }

            cooldownImageA.fillAmount = 0; // 이미지를 초기화합니다.
            // 스킬 사용 로직을 추가합니다.
        }
    }
    public void UseSkillB(string skillName)
    {
        if (!isCooldownB) // 쿨타임 중이 아니라면
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownB = true; // 쿨타임을 시작합니다.
            if (cooldownPower == 0)
            {
                cooldownTimerB = cooldownTimeB; // 쿨타임 타이머를 초기화합니다.
            }
            else
            {
                cooldownTimerB = cooldownTimeB * (1 - cooldownPower / 100); // 쿨타임 타이머를 초기화합니다.
            }
            cooldownImageB.fillAmount = 0; // 이미지를 초기화합니다.
            // 스킬 사용 로직을 추가합니다.
        }
    }
    public void UseSkillC(string skillName)
    {
        if (!isCooldownC) // 쿨타임 중이 아니라면
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownC = true; // 쿨타임을 시작합니다.
            if (cooldownPower == 0)
            {
                cooldownTimerC = cooldownTimeC; // 쿨타임 타이머를 초기화합니다.
            }
            else
            {
                cooldownTimerC = cooldownTimeC * (1 - cooldownPower / 100); // 쿨타임 타이머를 초기화합니다.
            }

            cooldownImageC.fillAmount = 0; // 이미지를 초기화합니다.
            // 스킬 사용 로직을 추가합니다.
        }
    }
    public void UseSkillD(string skillName)
    {
        if (!isCooldownD) // 쿨타임 중이 아니라면
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownD = true; // 쿨타임을 시작합니다.
            if (cooldownPower == 0)
            {
                cooldownTimerD = cooldownTimeD; // 쿨타임 타이머를 초기화합니다.
            }
            else
            {
                cooldownTimerD = cooldownTimeD * (1 - cooldownPower / 100); // 쿨타임 타이머를 초기화합니다.
            }
            cooldownImageD.fillAmount = 0; // 이미지를 초기화합니다.
            // 스킬 사용 로직을 추가합니다.
        }
    }
}
