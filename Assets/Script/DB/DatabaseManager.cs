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
            // list���� �ε��� �����Ͱ� ��� ����
            etcItemDataList = list;

            if (etcItemDataList.Count > 0)
            {
                Debug.Log("�����͸� ���������� �ҷ��Խ��ϴ�.");
            }
            else
            {
                Debug.Log("�����Ͱ� �����ϴ�.");
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
            Debug.Log("�����͸� �ҷ����� ���߰ų� ��ȿ���� ���� �ε����Դϴ�.");
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
            // list���� �ε��� �����Ͱ� ��� ����

            if (list.Count > 0)
            {
                // ù ��° ����� intValue ���� ������
                int firstIntValue = list[itemNum].intValue;
                Debug.Log("ù ��° intValue: " + firstIntValue);
            }
            else
            {
                Debug.Log("�����Ͱ� �����ϴ�.");
            }
        }, true);
    }
    */

}
