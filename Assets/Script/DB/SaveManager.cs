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
    public int bagUnlocked; // 가방 해금 칸 수(0 = 키 없음 → 시작 40칸, BagExpansionRules.Clamp)
    public int[,] inventoryFlags; // bit 0: 잠금, bit 1: 아직 보지 않은 획득
    public int[,] chestFlags;
    public int[] equipmentFlags = new int[10];
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
        WeaponMasteryService.Save(); // 무기 숙련(신규 키, 바뀐 때만)
        TypeMasteryService.Save(); // 타입 숙련(신규 키)
        SkillTreeService.Save(); // 스킬트리(신규 키)
    }
    public void DataLoad()
    {
        bool hadFile = ES3.FileExists();
        if (hadFile) // 슬롯 파일(SaveSlots 가 기본 경로를 정한다)
        {
            ES3.LoadInto(keyName, datas);
        }
        else
        {
            DataSave();
        }
        LevelSystem.LoadFrom(datas); // 레벨·EXP (세이브에 없으면 Lv1)
        WeaponMasteryService.Load(hadFile, datas); // 무기 숙련(구세이브면 인벤 무기 만렙으로 1회 마이그레이션)
        TypeMasteryService.Load(hadFile); // 타입 숙련(weaponMastery 에서 타입별 최대로 1회 이행)
        SkillTreeService.Load(); // 스킬트리(레벨·타입 숙련 로드 뒤)
    }
}
