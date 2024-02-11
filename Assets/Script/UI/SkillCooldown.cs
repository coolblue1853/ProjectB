using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImage; // ��Ÿ�� �̹���
    public float cooldownTime = 5f; // ��Ÿ�� �ð� (��)
    public bool isCooldown = false; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float cooldownTimer = 0f; // ��Ÿ�� Ÿ�̸�

    void Update()
    {
        // ��Ÿ�� ���̶��
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
            cooldownImage.fillAmount = 1-(cooldownTimer / cooldownTime); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.

            // ��Ÿ���� �Ϸ�Ǿ��� ��
            if (cooldownTimer <= 0)
            {
                isCooldown = false; // ��Ÿ�� ���¸� �����մϴ�.
                cooldownImage.fillAmount = 1; // �̹����� ���� ä��ϴ�.
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UseSkill();
        }
    }

    // ��ų�� ����� �� ȣ��Ǵ� �Լ�
    public void UseSkill()
    {
        Debug.Log("wkrehd");
        if (!isCooldown) // ��Ÿ�� ���� �ƴ϶��
        {
            isCooldown = true; // ��Ÿ���� �����մϴ�.
            cooldownTimer = cooldownTime; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            cooldownImage.fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
            // ��ų ��� ������ �߰��մϴ�.
        }
    }
}
