using UnityEngine;

public class TimeChange : MonoBehaviour
{
    public float totalTime = 5 * 60; // 5���� �� ������ ��ȯ
    public float period = 5 * 60; // �ֱ⸦ ��ü �ð����� ����

    private float startTime;
    public LightColorController LightColorController;

    void Start()
    {
        startTime = Time.time;
        LightColorController = this.GetComponent<LightColorController>();
    }

    void Update()
    {
        float elapsedTime = Time.time - startTime;

        if (elapsedTime >= totalTime)
        {
            startTime = Time.time;
            elapsedTime = 0;
        }

        float progress = elapsedTime / totalTime;
        float value = Mathf.Lerp(0, 1, progress);
        LightColorController.time = value;
    }
}
