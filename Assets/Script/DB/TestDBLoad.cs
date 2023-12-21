using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;
public class TestDBLoad : MonoBehaviour
{
    void Awake()
    {
        UnityGoogleSheet.LoadAllData();
        // UnityGoogleSheet.Load<DefaultTable.Data.Load>(); it's same!
        // or call DefaultTable.Data.Load(); it's same!
    }

    void Start()
    {
        UnityGoogleSheet.LoadFromGoogle<int, TestSheet.Data>((list, map) => {
            list.ForEach(x => {
                Debug.Log(x.intValue);
            });
        }, true);
        /*
        foreach (var value in DefaultTable.Data.DataList)
        {
            Debug.Log(value.index + "," + value.intValue + "," + value.strValue);
        }
        var dataFromMap = DefaultTable.Data.DataMap[0];
        Debug.Log("dataFromMap : " + dataFromMap.index + ", " + dataFromMap.intValue + "," + dataFromMap.strValue);
        */
    }
}
