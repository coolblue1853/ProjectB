using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImage; // 쿨타임 이미지
    public float cooldownTime = 5f; // 쿨타임 시간 (초)
    public bool isCooldown = false; // 쿨타임 중인지 여부 확인을 위한 변수
    private float cooldownTimer = 0f; // 쿨타임 타이머

    void Update()
    {
        // 쿨타임 중이라면
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
            cooldownImage.fillAmount = 1-(cooldownTimer / cooldownTime); // 이미지를 업데이트하여 쿨타임을 표시합니다.

            // 쿨타임이 완료되었을 때
            if (cooldownTimer <= 0)
            {
                isCooldown = false; // 쿨타임 상태를 해제합니다.
                cooldownImage.fillAmount = 1; // 이미지를 가득 채웁니다.
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UseSkill();
        }
    }

    // 스킬을 사용할 때 호출되는 함수
    public void UseSkill()
    {
        Debug.Log("wkrehd");
        if (!isCooldown) // 쿨타임 중이 아니라면
        {
            isCooldown = true; // 쿨타임을 시작합니다.
            cooldownTimer = cooldownTime; // 쿨타임 타이머를 초기화합니다.
            cooldownImage.fillAmount = 0; // 이미지를 초기화합니다.
            // 스킬 사용 로직을 추가합니다.
        }
    }
}
