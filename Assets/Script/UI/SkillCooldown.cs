using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{

    public Image[] backImage = new Image[4];
    public Image[] cooldownImage = new Image[4]; // ��Ÿ�� �̹���
    public float[] cooldownTime = new float[4]; // ��Ÿ�� �ð� (��)
    public bool[] isCooldown = new bool[4]; // ��Ÿ�� ������ ���� Ȯ���� ���� ����
    private float[] cooldownTimer = new float[4]; // ��Ÿ�� Ÿ�̸�
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
                cooldownTimer[i] -= Time.deltaTime; // ��Ÿ�� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
                cooldownImage[i].fillAmount = 1 - (cooldownTimer[i] / cooldownTime[i]); // �̹����� ������Ʈ�Ͽ� ��Ÿ���� ǥ���մϴ�.
                // ��Ÿ���� �Ϸ�Ǿ��� ��
                if (cooldownTimer[i] <= 0)
                {
                    isCooldown[i] = false; // ��Ÿ�� ���¸� �����մϴ�.
                    cooldownImage[i].fillAmount = 1; // �̹����� ���� ä��ϴ�.
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
    // ��ų�� ����� �� ȣ��Ǵ� �Լ�
    public void UseSkill(int skillNum,string skillName) //<< ������ ����
    {
        if (!isCooldown[skillNum]) // ��Ÿ�� ���� �ƴ϶��
        {
            float cooldownPower = 0;
            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
            {
                cooldownPower = DatabaseManager.skillCoolDown[skillName];
            }
            isCooldown[skillNum] = true; // ��Ÿ���� �����մϴ�.
            if (cooldownPower == 0) cooldownTimer[skillNum] = cooldownTime[skillNum]; // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            else cooldownTimer[skillNum] = cooldownTime[skillNum] * (1 - cooldownPower / 100); // ��Ÿ�� Ÿ�̸Ӹ� �ʱ�ȭ�մϴ�.
            cooldownImage[skillNum].fillAmount = 0; // �̹����� �ʱ�ȭ�մϴ�.
        }
    }
}
