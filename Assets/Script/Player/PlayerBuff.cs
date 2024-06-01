using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuff : MonoBehaviour
{
    public string buffType;
    public int buffTime;
    public int buffPower;
    // Start is called before the first frame update
    void Start()
    {
        if(buffType == "AttSpeed")
            DatabaseManager.attackSpeedBuff += buffPower;
        if (buffType == "Speed")
            DatabaseManager.SpeedBuff += buffPower;
        if(buffType == "HitCount")
            DatabaseManager.hitCount += buffPower;

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

        Destroy(this.gameObject);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
