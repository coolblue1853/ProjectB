using UnityEngine;

public class Fixed : MonoBehaviour
{
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Start()
    {
        SetResolution(); // �ʱ⿡ ���� �ػ� ����
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void Update()
    {
        // ���� �ػ󵵿� ���� �ػ󵵸� ��
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            SetResolution(); // �ػ󵵰� ����� ��� �ػ� ����
            lastScreenWidth = Screen.width; // ���� �ػ� ���� ����
            lastScreenHeight = Screen.height;
        }
    }

    /* �ػ� �����ϴ� �Լ� */
    public void SetResolution()
    {
        int setWidth = 1920; // ����� ���� �ʺ�
        int setHeight = 1080; // ����� ���� ����

        int deviceWidth = Screen.width; // ��� �ʺ� ����
        int deviceHeight = Screen.height; // ��� ���� ����

        // ��ü ȭ�� ���°� �ƴ� ���� SetResolution�� ȣ���Ͽ� â ��带 ����
        if (!Screen.fullScreen)
        {
            // ���� â ��带 �����ϸ鼭 �ػ� ����
            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false);
        }

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // ����� �ػ� ������ �� ū ���
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // ���ο� �ʺ� ���
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���ο� Rect ����
        }
        else // ������ �ػ� ������ �� ū ���
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // ���ο� ���� ���
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���ο� Rect ����
        }
    }
}
