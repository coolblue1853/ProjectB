using UnityEngine;

public class TimeChange : MonoBehaviour
{
    public float totalTime; // 5분을 초 단위로 변환
    public float period; // 주기를 전체 시간으로 설정

    private float startTime;
    public LightColorController LightColorController;

    public bool isTest = false;
    public float time;
    public float startNighttime;
    public float endNighttime;
    public float value;
    float progress;
    public bool isDaytime = false;

    void Start()
    {
        startTime = Time.time -SaveManager.instance.datas.time;
        LightColorController = this.GetComponent<LightColorController>();

    }


    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current; // 디버그 키(0): 시간 60초 추가 — 레거시 Input 대신 New Input System
        if (keyboard != null && keyboard.digit0Key.wasPressedThisFrame)
        {
            AddTime(60); //시간 추가
        }

        if (isTest == false && totalTime > 0f)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= totalTime)
            {
                // 하루가 넘어가면 넘친 만큼을 남긴다. 0으로 되돌리면 AddTime 으로 하루 끝을 넘긴 시간이 버려져 밤 전환이 낮으로 튄다
                elapsedTime = Mathf.Repeat(elapsedTime, totalTime);
                startTime = Time.time - elapsedTime;
            }
            SaveManager.instance.datas.time = elapsedTime;

            progress = elapsedTime / totalTime;
            value = Mathf.Lerp(0, 1, progress);

            LightColorController.time = value;
            if ((value >= endNighttime || value <= startNighttime) && isDaytime == false)
            {
                isDaytime = true;
            }
            else if (value >= startNighttime && value <= endNighttime && isDaytime == true)
            {
                isDaytime = false;
            }
        }
        else
        {
            LightColorController.time = time;
            if ((time >= endNighttime || time <= startNighttime) && isDaytime == false)
            {
                isDaytime = true;
            }
            else if (time >= startNighttime && time <= endNighttime && isDaytime == true)
            {
                isDaytime = false;
            }
        }
    }

    // 원하는 만큼 시간을 즉시 추가하는 함수
    public void AddTime(float additionalTime)
    {
        // 시작 시간을 변경하고 추가적인 시간을 더합니다.
        startTime -= additionalTime;
    }

    static public TimeChange instance;

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
}
