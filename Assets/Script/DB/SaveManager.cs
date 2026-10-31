using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Datas
{
    public int money;
    public int level = 0;
    public int exp = 0; // 현재 레벨에서 쌓은 EXP (LevelSystem)
    public Vector2 playerPos;
    public Vector2 camPos;
    public float time;
    public string[,,] invenItem;
    public string[,,] chestItem;
    public string[,] equipGear = new string[10, 3]; // 장비아이템, [이름,티어,강화]
}

public class SaveManager : MonoBehaviour
{
    private string keyName = "Datas";
    public static SaveManager instance;
    public GameObject player;
    public GameObject originPos;
    public Datas datas;
    // Start is called before the first frame update]
    private void Awake()
    {
        instance = this;
        DataLoad();
        // 시작 지점이 지정되지 않았으면 씬에 배치된 플레이어 위치를 시작 지점으로 쓴다.
        Vector2 origin = originPos != null ? (Vector2)originPos.transform.position
                       : player != null ? (Vector2)player.transform.position : Vector2.zero;
        if (datas.playerPos == Vector2.zero)
        {
            datas.playerPos = origin;
        }
        if (datas.camPos == Vector2.zero)
        {
            datas.camPos = origin;
        }

    }
    void Start()
    {

    }
    public void SavePos() 
    {
        datas.playerPos = player.transform.position;
        DataSave();
    }
    public void SaveTime(float time)
    {
        datas.time = time;
        DataSave();
    }
    public void DataSave()
    {
        LevelSystem.WriteTo(datas);
        GameLog.Info(GameLog.Cat.Save, "저장 money=" + datas.money + " pos=" + datas.playerPos);
        ES3.Save(keyName, datas);
    }
    public void DataLoad()
    {
        if (ES3.FileExists()) // 슬롯 파일(SaveSlots 가 기본 경로를 정한다)
        {
            ES3.LoadInto(keyName, datas);
        }
        else
        {
            DataSave();
        }
        LevelSystem.LoadFrom(datas); // 레벨·EXP (세이브에 없으면 Lv1)
    }
}
