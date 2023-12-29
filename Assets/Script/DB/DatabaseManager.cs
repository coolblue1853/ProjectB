using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;
public class DatabaseManager : MonoBehaviour
{
    public static bool weaponStopMove = false;


    private List<TestSheet.Data> etcItemDataList;

    static public DatabaseManager instance;
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
        UnityGoogleSheet.LoadAllData();
        LoadAllETCItemData();
    }

    public void LoadAllETCItemData()
    {
        UnityGoogleSheet.LoadFromGoogle<int, TestSheet.Data>((list, map) => {
            // list에는 로드한 데이터가 들어 있음
            etcItemDataList = list;

            if (etcItemDataList.Count > 0)
            {
                Debug.Log("데이터를 성공적으로 불러왔습니다.");
            }
            else
            {
                Debug.Log("데이터가 없습니다.");
            }
        }, true);
    }

    public void LoadETCItemData(int itemNum)
    {
        if (etcItemDataList != null && itemNum >= 0 && itemNum < etcItemDataList.Count)
        {
            int intValue = etcItemDataList[itemNum].intValue;
            Debug.Log("intValue: " + intValue);
        }
        else
        {
            Debug.Log("데이터를 불러오지 못했거나 유효하지 않은 인덱스입니다.");
        }
    }
    void Start()
    {





        /*
       UnityGoogleSheet.LoadFromGoogle<int, TestSheet.Data>((list, map) => {
           list.ForEach(x => {
               Debug.Log(x.intValue);
           });
       }, true);

       foreach (var value in DefaultTable.Data.DataList)
       {
           Debug.Log(value.index + "," + value.intValue + "," + value.strValue);
       }
       var dataFromMap = DefaultTable.Data.DataMap[0];
       Debug.Log("dataFromMap : " + dataFromMap.index + ", " + dataFromMap.intValue + "," + dataFromMap.strValue);
       */
    }

    /*
    public void LoadETCItemData(int itemNum)
    {
        UnityGoogleSheet.LoadFromGoogle<int, TestSheet.Data>((list, map) => {
            // list에는 로드한 데이터가 들어 있음

            if (list.Count > 0)
            {
                // 첫 번째 요소의 intValue 값을 가져옴
                int firstIntValue = list[itemNum].intValue;
                Debug.Log("첫 번째 intValue: " + firstIntValue);
            }
            else
            {
                Debug.Log("데이터가 없습니다.");
            }
        }, true);
    }
    */

}
