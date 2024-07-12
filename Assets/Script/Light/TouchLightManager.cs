using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal; // Light2D 네임스페이스 추가
public class TouchLightManager : MonoBehaviour
{
     UnityEngine.Rendering.Universal.Light2D light;
    public float attPower = 1.2f; // 감쇠 보정치
    float maxAttenuation = 0.5f; // 최대감쇠
     float startAttenuation = 0.85f; // 감쇠 시작
     float endAttenuation = 0.35f; // 감쇠 끝 -> 0.9~ 1 -> 0 ~ 0.35 까지 총 0.45동안 감쇠가 되는것. 최대 감쇠가 
    float maxAttTime; // 최대 감쇠가 되는 시간
    public float nowAtt;// 현재 감쇠 정도
                        // Start is called before the first frame update

    float startTime;
    public float lightMaxTime = 60;// 최대 밝기가 유지되는 시간.
    public float lightTime = 180; // 밝기가 유지되는 전체 시간.
    public float lightMinus;
    public float nowLight = 1;
    private void Awake()
    {
        light = this.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    }
    void Start()
    {
        lightMinus = nowLight / (lightTime - lightMaxTime);
           startTime = Time.time;
           maxAttTime = ((1 - startAttenuation + endAttenuation) / 2) - (1-startAttenuation);
        // 인보크로 1초마다 감쇠 조정
    }
    void SetAttenuation()
    {
        float time = TimeChange.instance.value;
        if(time > endAttenuation && time < startAttenuation) // 해당 시간 내면 인보크 정지
        {
            isActiveAtt = false;
            return;
        }
        if(time >= startAttenuation) 
        {
            nowAtt =  (time - (startAttenuation))*2; //이게 아니라 늘다가 줄어드는 방식으로 감쇠가 진행 되어야 함. 
        }
        else if(time >=0 && time <= maxAttTime) // 중간값 까지는 계속해서 상성
        {
            nowAtt = (time + (1 - startAttenuation))*2; // 0.25 +(1- 0.85) -> 
        }
        else if(time > maxAttTime && time <= endAttenuation)
        {
            nowAtt = maxAttenuation - (time - maxAttTime) * 2;
        }

        light.intensity = nowLight * (1-(nowAtt)); // 나중에 저 1이 감쇠하는값임.

        Invoke("SetAttenuation", 1f);
    }
    bool isActiveAtt = false; // 감쇠가 진행되었는지 체크
    bool isMaxLightEnd = false;
    private void Update()
    {
        if (isActiveAtt == false) // 해당 시간 내면 인보크 정지
        {
            if (TimeChange.instance.value >= startAttenuation)
            {
                isActiveAtt = true;
                SetAttenuation();
            }
            else if (TimeChange.instance.value >= 0 && TimeChange.instance.value <= endAttenuation)
            {
                isActiveAtt = true;
                SetAttenuation();
            }
        }
        /*
        if (light.intensity != 1 && TimeChange.instance.value > endAttenuation && TimeChange.instance.value < startAttenuation)
        {
            light.intensity = 1;
        }
        */



        if(Time.time - startTime > lightMaxTime && isMaxLightEnd ==false) //
        {
            isMaxLightEnd = true;
            DarkerLight();
        }
        if (Time.time - startTime > lightTime) //
        {
            Destroy(this.gameObject);
        }
    }


    void DarkerLight() // 점점 밝기가 줄어들게
    {
        nowLight -= lightMinus;

        Invoke("DarkerLight", 1f);
    }
}
