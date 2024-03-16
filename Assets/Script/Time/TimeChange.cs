using UnityEngine;

public class TimeChange : MonoBehaviour
{
    public float totalTime = 5 * 60; // 5분을 초 단위로 변환
    public float period = 5 * 60; // 주기를 전체 시간으로 설정

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
