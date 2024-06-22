using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuff : MonoBehaviour
{
    public string buffType;
    public int buffTime;
    public int buffPower;
    public bool isStartIm = true; // 즉시 버프를 시작하는가
    // Start is called before the first frame update
    void Start()
    {
        if(isStartIm == true)
        {
            if (buffType == "AttSpeed")
                DatabaseManager.attackSpeedBuff += buffPower;
            if (buffType == "Speed")
                DatabaseManager.SpeedBuff += buffPower;
            if (buffType == "HitCount")
                DatabaseManager.hitCount += buffPower;
            if (buffType == "def")
                DatabaseManager.playerDef += buffPower;

            Invoke("DestoryBuff", buffTime);
        }

    }


    public void ActiveBuff()
    {
        if (buffType == "AttSpeed")
            DatabaseManager.attackSpeedBuff += buffPower;
        if (buffType == "Speed")
            DatabaseManager.SpeedBuff += buffPower;
        if (buffType == "HitCount")
            DatabaseManager.hitCount += buffPower;
        if (buffType == "def")
            DatabaseManager.playerDef += buffPower;

        Invoke("DestoryBuff", buffTime);
    }


    void DestoryBuff()
    {
        if (buffType == "AttSpeed")
            DatabaseManager.attackSpeedBuff -= buffPower;
        if (buffType == "Speed")
            DatabaseManager.SpeedBuff -= buffPower;
        if (buffType == "HitCount")
            DatabaseManager.hitCount -= buffPower;
        if (buffType == "def")
            DatabaseManager.playerDef -= buffPower;
        Destroy(this.gameObject);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
