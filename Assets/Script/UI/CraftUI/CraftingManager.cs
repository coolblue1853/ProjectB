using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CraftingManager : MonoBehaviour
{

    public GameObject[] LV1;
    CraftItemCheck nowCraftItem;
    NeedItem nowNeedItem;
    public TextMeshProUGUI name;
    public TextMeshProUGUI type;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public TextMeshProUGUI weight;
    public TextMeshProUGUI acqPath;
    public TextMeshProUGUI effectOb;
    public TextMeshProUGUI effectPow;
    public TextMeshProUGUI equipArea;
    public Image image;

    public GameObject needMaterail;
    public GameObject itemPrefab; // �����Ǵ� Box �ν��Ͻ�
                                  // Start is called before the first frame update

    int lV = 0;
    int line = 0;
    int childCount = 0;
    public GameObject CraftUI;
    public GameObject cusor;
    void Start()
    {
        lV = 1;
        line = 0;
        childCount = 0;
        nowCraftItem = LV1[0].transform.GetChild(0).GetComponent<CraftItemCheck>();
        nowNeedItem = LV1[0].transform.GetChild(0).GetComponent<NeedItem>();
        SetNeedItem();
       // Invoke("SetDetail", 1f); // �ϴ� 1�ʵڿ� �ϵ��� �ؼ� ������ üũ�� �ߴµ� �̰Ŵ� Ȯ���ؾ��ҵ�


    }

    void SetDetail()
    {
        name.text = nowCraftItem.name;
        type.text = nowCraftItem.type;
        description.text = nowCraftItem.description;
        price.text =(nowCraftItem.price).ToString();
        weight.text = nowCraftItem.weight.ToString();
        acqPath.text = nowCraftItem.acqPath;
        image.sprite = nowCraftItem.image.sprite;
        if (type.text == "Consum")
        {
          //  effectOb.text = nowCraftItem.effectOb;
         //   effectPow.text = nowCraftItem.effectPow.ToString();
        }
        if (type.text == "Equip")
        {
          //  equipArea.text = nowCraftItem.equipArea;
        }

    }

    public void Craft()
    {
        if(NeedCountCheck() == true)
        {
            Debug.Log("���� ����");
            for (int i = 0; i < nowNeedItem.needItem.Count; i++)
            {
                InventoryManager.instance.DeletItemByName(needItemList[i].name, needItemList[i].needCount);
            }
            InventoryManager.instance.CreatItem(name.text);
        }
        else
        {
            Debug.Log("���� ����");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(CraftUI.activeSelf == true)
        {
            MoveCusor();
        }



        if (name.text == "")
        {
            SetDetail();
        }

    }


    void MoveCusor()
    {
        if(lV == 1)
        {
            cusor.transform.position = LV1[line].transform.GetChild(childCount).position;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (LV1[line].transform.childCount > childCount + 1)
                {
                    childCount += 1;
                    DeleteAllChildren(needMaterail);
                    nowCraftItem = LV1[line].transform.GetChild(childCount).GetComponent<CraftItemCheck>();
                    nowNeedItem = LV1[line].transform.GetChild(childCount).GetComponent<NeedItem>();
                    SetNeedItem();
                    SetDetail();
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (childCount > 0)
                {
                    childCount -= 1;
                    DeleteAllChildren(needMaterail);
                    nowCraftItem = LV1[line].transform.GetChild(childCount).GetComponent<CraftItemCheck>();
                    nowNeedItem = LV1[line].transform.GetChild(childCount).GetComponent<NeedItem>();
                    SetNeedItem();
                    SetDetail();
                }
            }
        }


    }

    //DatabaseManager.inventoryItemStack[name]+"/" + needCount
    bool NeedCountCheck()
    {
        for (int i = 0; i < nowNeedItem.needItem.Count; i++)
        {
            if(DatabaseManager.inventoryItemStack.ContainsKey(needItemList[i].name) == true)
            {
                if (DatabaseManager.inventoryItemStack[needItemList[i].name] < needItemList[i].needCount)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


        }

        return true;
    }

    ItemCheck[] needItemList;
    void SetNeedItem()
    {
        needItemList = new ItemCheck[nowNeedItem.needItem.Count];
        for (int i =0; i < nowNeedItem.needItem.Count; i++)
        {
            GameObject item = Instantiate(itemPrefab, needMaterail.transform.position, Quaternion.identity, needMaterail.transform);
            ItemCheck itemCheck = item.GetComponent<ItemCheck>();
            needItemList[i] = itemCheck;
            itemCheck.SetItem(nowNeedItem.needItem[i]);
            itemCheck.needCount = nowNeedItem.needItemAmount[i];
        }
    }


    void DeleteAllChildren(GameObject parent)
    {
        // �θ� GameObject�� �ڽ� GameObject�� ��� �����ͼ� �迭�� ����
        GameObject[] children = new GameObject[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children[i] = parent.transform.GetChild(i).gameObject;
        }

        // �迭�� ����� ��� �ڽ� GameObject�� ����
        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }
}
