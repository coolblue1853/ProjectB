using UnityEngine;

public class TimeChange : MonoBehaviour
{
    public float totalTime ; // 5분을 초 단위로 변환
    public float period ; // 주기를 전체 시간으로 설정

    private float startTime;
    public LightColorController LightColorController;

    public bool isTest = false;
    public float time;
    public float startDaytime;
    public float endDaytime;

    public bool isDaytime = false;
    void Start()
    {
        startTime = Time.time;
        LightColorController = this.GetComponent<LightColorController>();
    }

    void Update()
    {
        
        if (isTest == false)
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
            if (value >= endDaytime && isDaytime == true)
            {
                isDaytime = false;

            }
            else if (value >= startDaytime && value <= endDaytime && isDaytime == false)
            {
                isDaytime = true;

            }

        }
        else
        {
            LightColorController.time = time;
            if (time >= endDaytime && isDaytime == true)
            {
                isDaytime = false;

            }
            else if (time >= startDaytime && time <= endDaytime && isDaytime == false)
            {
                isDaytime = true;

            }
        }

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

