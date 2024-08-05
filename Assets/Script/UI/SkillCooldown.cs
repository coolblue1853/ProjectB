using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{

    public Image[] backImage = new Image[4];
    public Image[] cooldownImage = new Image[4]; // 쿨타임 이미지
    public float[] cooldownTime = new float[4]; // 쿨타임 시간 (초)
    public bool[] isCooldown = new bool[4]; // 쿨타임 중인지 여부 확인을 위한 변수
    private float[] cooldownTimer = new float[4]; // 쿨타임 타이머
    static public SkillCooldown instance;
    public void ResetCoolTime()
    {
        for(int i =0; i < backImage.Length; i++)
        {
            cooldownImage[i].sprite = null;
            cooldownTime[i] = 0;
            isCooldown[i] = false;
            cooldownTimer[i] = 0;
        }
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
    }
    void Update()
    {
        CheckCoolTime();
    }
    void CheckCoolTime()
    {
        for(int i =0; i < cooldownImage.Length; i++)
        {
            if (isCooldown[i] && cooldownImage[i] != null)
            {
                cooldownTimer[i] -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
                cooldownImage[i].fillAmount = 1 - (cooldownTimer[i] / cooldownTime[i]); // 이미지를 업데이트하여 쿨타임을 표시합니다.
                // 쿨타임이 완료되었을 때
                if (cooldownTimer[i] <= 0)
                {
                    isCooldown[i] = false; // 쿨타임 상태를 해제합니다.
                    cooldownImage[i].fillAmount = 1; // 이미지를 가득 채웁니다.
                }
            }
        }
    }

    public void DeletSkillSprite()
    {
        for (int i = 0; i < cooldownImage.Length; i++)
        {
            cooldownImage[i].sprite = null;
        }
    }
    // 스킬을 사용할 때 호출되는 함수
    public void UseSkill(int skillNum,string skillName) //<< 포폴에 넣자
    {
        if (!isCooldown[skillNum]) // 쿨타임 중이 아니라면
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldown[skillNum] = true; // 쿨타임을 시작합니다.
            if (cooldownPower == 0) cooldownTimer[skillNum] = cooldownTime[skillNum]; // 쿨타임 타이머를 초기화합니다.
            else cooldownTimer[skillNum] = cooldownTime[skillNum] * (1 - cooldownPower / 100); // 쿨타임 타이머를 초기화합니다.
            cooldownImage[skillNum].fillAmount = 0; // 이미지를 초기화합니다.
        }
    }
}
