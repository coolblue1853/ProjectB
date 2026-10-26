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

    [Tooltip("켜면 시간이 흐르지 않고 항상 낮(_lockedTimeValue)으로 고정된다. 끄면 기존 낮/밤 순환으로 돌아간다(2026-10-26 사용자 요청).")]
    [SerializeField] private bool _lockDaytime = true;
    [Tooltip("낮 고정 시 쓰는 시간값(0~1). 밤 구간(startNighttime~endNighttime) 밖이어야 한다.")]
    [SerializeField, Range(0f, 1f)] private float _lockedTimeValue = 0.25f;

    /// <summary>낮 고정 여부. 프로브(set_night 등)가 밤을 시험할 때 끈다.</summary>
    public bool LockDaytime
    {
        get { return _lockDaytime; }
        set { _lockDaytime = value; }
    }

    void Start()
    {
        startTime = Time.time -SaveManager.instance.datas.time;
        LightColorController = this.GetComponent<LightColorController>();
        if (_lockDaytime)
        {
            ApplyTime(_lockedTimeValue); // 첫 프레임부터 낮으로 — 스포너·BGM 이 Update 순서와 상관없이 낮을 읽게
        }

    }


    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current; // 디버그 키(0): 시간 60초 추가 — 레거시 Input 대신 New Input System
        if (keyboard != null && keyboard.digit0Key.wasPressedThisFrame)
        {
            AddTime(60); //시간 추가
        }

        if (_lockDaytime)
        {
            // 낮 고정: 경과 시간도 고정값에 맞춰 둔다 — 플래그를 끄면 이 시각부터 이어서 흐른다(고정 중 AddTime·isTest 는 무시됨)
            float lockedElapsed = _lockedTimeValue * totalTime;
            startTime = Time.time - lockedElapsed;
            if (SaveManager.instance != null)
            {
                SaveManager.instance.datas.time = lockedElapsed;
            }
            value = _lockedTimeValue;
            ApplyTime(value);
            return;
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
            ApplyTime(value);
        }
        else
        {
            ApplyTime(time);
        }
    }

    /// <summary>조명에 시간값을 넘기고 낮/밤 여부를 갱신한다.</summary>
    private void ApplyTime(float t)
    {
        LightColorController.time = t;
        isDaytime = t >= endNighttime || t <= startNighttime;
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
