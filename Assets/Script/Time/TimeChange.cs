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
        startTime = Time.time;
        LightColorController = this.GetComponent<LightColorController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AddTime(60); //시간 추가
        }
        
        if (isTest == false)
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime >= totalTime)
            {
                startTime = Time.time;
                elapsedTime = 0;
            }

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
