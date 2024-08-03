using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Datas
{
    public int money;
    public int level = 0;
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
    private string fileName = "SaveFile.es3";
    public static SaveManager instance;
    public GameObject player;
    public GameObject originPos;
    public Datas datas;
    // Start is called before the first frame update]
    private void Awake()
    {
        instance = this;
        DataLoad();
        if(datas.playerPos == Vector2.zero)
        {
            datas.playerPos = originPos.transform.position;
        }
        if (datas.camPos == Vector2.zero)
        {
            datas.camPos = originPos.transform.position;
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DataSave();
        }
    }

    public void DataSave()
    {
        ES3.Save(keyName, datas);
    }
    public void DataLoad()
    {
        if (ES3.FileExists(fileName))
        {
            ES3.LoadInto(keyName, datas);
        }
        else
        {
            DataSave();
        }

    }
}
