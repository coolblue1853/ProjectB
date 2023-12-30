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
            // list���� �ε��� �����Ͱ� ��� ����

            if (list.Count > 0)
            {
                // ù ��° ����� intValue ���� ������
             //   int firstIntValue = list[0].intValue;
         //       Debug.Log("ù ��° intValue: " + firstIntValue);
            }
            else
            {
                Debug.Log("�����Ͱ� �����ϴ�.");
            }
        }, true);
    }

}
