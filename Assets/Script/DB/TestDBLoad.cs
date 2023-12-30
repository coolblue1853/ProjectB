using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;
public class TestDBLoad : MonoBehaviour
{
    static public TestDBLoad instance;
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


    public void LoadETCItemData()
    {
        UnityGoogleSheet.LoadFromGoogle<int, TestSheet.Data>((list, map) => {
            // list에는 로드한 데이터가 들어 있음

            if (list.Count > 0)
            {
                // 첫 번째 요소의 intValue 값을 가져옴
             //   int firstIntValue = list[0].intValue;
         //       Debug.Log("첫 번째 intValue: " + firstIntValue);
            }
            else
            {
                Debug.Log("데이터가 없습니다.");
            }
        }, true);
    }

}
