using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Datas
{
    public int money;
    public int level = 0;
    public int exp = 0; // 현재 레벨에서 쌓은 EXP (LevelSystem)
    public string mapName; // 같은 좌표를 공유하는 사냥터 구분. 구 세이브는 활성 맵·높이로 추정한다.
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
        // AfterSceneLoad의 카메라 경계 계산도 저장 위치를 기준으로 하도록 Start보다 먼저 배치한다.
        if (player != null)
        {
            player.transform.position = datas.playerPos;
        }
        RestoreMap();
    }

    private void OnEnable()
    {
        Portal.MapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        Portal.MapChanged -= OnMapChanged;
    }

    private void OnMapChanged(GameObject map)
    {
        if (map == null)
        {
            return;
        }
        datas.mapName = map.name;
        SavePos();
    }

    private void RestoreMap()
    {
        GameObject root = GameObject.Find("Map");
        if (root == null)
        {
            return;
        }
        Transform target = string.IsNullOrEmpty(datas.mapName) ? null : root.transform.Find(datas.mapName);
        if (target == null)
        {
            float nearest = float.MaxValue;
            foreach (Transform map in root.transform)
            {
                float distance = Mathf.Abs(map.position.y - datas.playerPos.y);
                if (map.gameObject.activeSelf && map.GetComponent<MapInfo>() != null && distance < nearest)
                {
                    nearest = distance;
                    target = map;
                }
            }
        }
        if (target == null)
        {
            return;
        }
        datas.mapName = target.name;
        foreach (Transform map in root.transform)
        {
            if (map.GetComponent<MapInfo>() != null)
            {
                map.gameObject.SetActive(map == target);
            }
        }
        PoolCheck pools = target.GetComponent<PoolCheck>();
        if (pools != null)
        {
            pools.AbleSpwaner();
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

    public void SaveProgress()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.SaveInventoryAndChest();
        }
        if (player != null)
        {
            datas.playerPos = player.transform.position;
        }
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
