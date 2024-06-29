using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImageA; // ��Ÿ�� �̹���
    public float cooldownTimeA = 0f; // ��Ÿ�� �ð� (��)
    public bool isCooldownA = false; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float cooldownTimerA = 0f; // ��Ÿ�� Ÿ�̸�

    public Image cooldownImageB; // ��Ÿ�� �̹���
    public float cooldownTimeB = 0f; // ��Ÿ�� �ð� (��)
    public bool isCooldownB = false; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float cooldownTimerB = 0f; // ��Ÿ�� Ÿ�̸�

    public Image cooldownImageC; // ��Ÿ�� �̹���
    public float cooldownTimeC = 0f; // ��Ÿ�� �ð� (��)
    public bool isCooldownC = false; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float cooldownTimerC = 0f; // ��Ÿ�� Ÿ�̸�

    public Image cooldownImageD; // ��Ÿ�� �̹���
    public float cooldownTimeD = 0f; // ��Ÿ�� �ð� (��)
    public bool isCooldownD = false; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float cooldownTimerD = 0f; // ��Ÿ�� Ÿ�̸�

    

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
        // ��Ÿ�� ���̶��
        if (isCooldownA && cooldownImageA != null)
        {
            cooldownTimerA -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.

            cooldownImageA.fillAmount = 1-(cooldownTimerA / cooldownTimeA); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.

            // ��Ÿ���� �Ϸ�Ǿ��� ��
            if (cooldownTimerA <= 0)
            {
                isCooldownA = false; // ��Ÿ�� ���¸� �����մϴ�.
                cooldownImageA.fillAmount = 1; // �̹����� ���� ä��ϴ�.
            }
        }
        // ��Ÿ�� ���̶��
        if (isCooldownB && cooldownImageB != null)
        {
            cooldownTimerB -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
            cooldownImageB.fillAmount = 1 - (cooldownTimerB / cooldownTimeB); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.

            // ��Ÿ���� �Ϸ�Ǿ��� ��
            if (cooldownTimerB <= 0)
            {
                isCooldownB = false; // ��Ÿ�� ���¸� �����մϴ�.
                cooldownImageB.fillAmount = 1; // �̹����� ���� ä��ϴ�.
            }
        }

        if (isCooldownC && cooldownImageC != null)
        {
            cooldownTimerC -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
            cooldownImageC.fillAmount = 1 - (cooldownTimerC / cooldownTimeC); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.

            // ��Ÿ���� �Ϸ�Ǿ��� ��
            if (cooldownTimerC <= 0)
            {
                isCooldownC = false; // ��Ÿ�� ���¸� �����մϴ�.
                cooldownImageC.fillAmount = 1; // �̹����� ���� ä��ϴ�.
            }
        }

        if (isCooldownD && cooldownImageD != null)
        {
            cooldownTimerD -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
            cooldownImageD.fillAmount = 1 - (cooldownTimerD / cooldownTimeD); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.

            // ��Ÿ���� �Ϸ�Ǿ��� ��
            if (cooldownTimerD <= 0)
            {
                isCooldownD = false; // ��Ÿ�� ���¸� �����մϴ�.
                cooldownImageD.fillAmount = 1; // �̹����� ���� ä��ϴ�.
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
    // ��ų�� ����� �� ȣ��Ǵ� �Լ�
    public void UseSkillA(string skillName)
    {
        if (!isCooldownA) // ��Ÿ�� ���� �ƴ϶��
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }

            isCooldownA = true; // ��Ÿ���� �����մϴ�.
            if(cooldownPower == 0)
            {
                cooldownTimerA = cooldownTimeA; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            else
            {
         
                cooldownTimerA = cooldownTimeA*(1- cooldownPower/100); // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
                Debug.Log( (cooldownTimeA * (1 - cooldownPower / 100)));
            }

            cooldownImageA.fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
            // ��ų ��� ������ �߰��մϴ�.
        }
    }
    public void UseSkillB(string skillName)
    {
        if (!isCooldownB) // ��Ÿ�� ���� �ƴ϶��
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownB = true; // ��Ÿ���� �����մϴ�.
            if (cooldownPower == 0)
            {
                cooldownTimerB = cooldownTimeB; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            else
            {
                cooldownTimerB = cooldownTimeB * (1 - cooldownPower / 100); // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            cooldownImageB.fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
            // ��ų ��� ������ �߰��մϴ�.
        }
    }
    public void UseSkillC(string skillName)
    {
        if (!isCooldownC) // ��Ÿ�� ���� �ƴ϶��
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownC = true; // ��Ÿ���� �����մϴ�.
            if (cooldownPower == 0)
            {
                cooldownTimerC = cooldownTimeC; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            else
            {
                cooldownTimerC = cooldownTimeC * (1 - cooldownPower / 100); // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }

            cooldownImageC.fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
            // ��ų ��� ������ �߰��մϴ�.
        }
    }
    public void UseSkillD(string skillName)
    {
        if (!isCooldownD) // ��Ÿ�� ���� �ƴ϶��
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldownD = true; // ��Ÿ���� �����մϴ�.
            if (cooldownPower == 0)
            {
                cooldownTimerD = cooldownTimeD; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            else
            {
                cooldownTimerD = cooldownTimeD * (1 - cooldownPower / 100); // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            }
            cooldownImageD.fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
            // ��ų ��� ������ �߰��մϴ�.
        }
    }
}
