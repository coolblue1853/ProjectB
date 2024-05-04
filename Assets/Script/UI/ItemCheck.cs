using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
public class ItemCheck : MonoBehaviour
{
    //�ڵ� / ����((������� �Ҹ�ǰ����)  / �̸� / ���� / ������ / ���� / ���� // ȹ����
    public string name;
    public string type;
    public string description;
    public int price;
    public int Buyprice;
    public int weight;
    public string acqPath;
    public int maxStack;
    public int nowStack;
    public string effectOb;
    public int effectPow;
    public string equipArea;
    public int needCount;
    public TextMeshProUGUI stackText;
    public Image image;

    public string tfName;
    ItemData item;

    public bool isCraftItem = false;
    public bool isShopItem = false;
    private void Start()
    {
        if (isShopItem)
        {
            SetItem(this.transform.name);
        }

    }
    public void SetItem(string itemName)
    {

        item = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = item.name;
        type = item.type;
        description = item.description;
        price = item.price;
        Buyprice = (int)(price * 1.5f);
        weight = item.weight;
        acqPath = item.acqPath;
        maxStack = item.maxStack;
        nowStack = 1;
        tfName = item.tfName;
        if (stackText != null)
        stackText.text = nowStack.ToString();
        if(type == "Consum")
        {
            effectOb = item.effectOb;
            effectPow = item.effectPow;
        }
        if (type == "Equip")
        {
            equipArea = item.equipArea;

        }
        LoadImage();
    }

    public void ConsumItemActive()
    {
        nowStack -= 1;
        string[] effect = effectOb.Split();
        if (effect[0] == "stemina")
        {
            if(effect[1] == "+")
            {
                PlayerHealthManager.Instance.SteminaUp(effectPow);
            }
        }
        if (effect[0] == "fullness")
        {
            if (effect[1] == "+")
            {
                PlayerHealthManager.Instance.FullnessUp(effectPow);
            }
        }
        if (effect[0] == "hp")
        {
            if (effect[1] == "+")
            {
                PlayerHealthManager.Instance.HpUp(effectPow);
            }
        }

        
        if (this.nowStack == 0)
        {
            Destroy(this.gameObject);
        }
    }


    public TextMeshProUGUI Cost;
    // Update is called once per frame
    void Update()
    {
        if(isCraftItem == false && isShopItem == false)
        {
            if (nowStack == 1)
            {
                stackText.text = "1";
                stackText.gameObject.SetActive(false);
            }
            else
            {
                stackText.text = nowStack.ToString();
                stackText.gameObject.SetActive(true);
            }
        }


       else  if(isCraftItem == true)
        {
            if (DatabaseManager.inventoryItemStack.ContainsKey(name) == false)
            {
                stackText.text = "0/" + needCount;
            }
            else
            {
                stackText.text = + DatabaseManager.inventoryItemStack[name]+"/" + needCount;
            }
        }

        else  if(isShopItem == true)
        {
            if (Cost.text == "")
            {
                Cost.text = Buyprice.ToString();
            }


                if (DatabaseManager.inventoryItemStack.ContainsKey(name) == false)
            {
                if(stackText.text != "0")
                     stackText.text = "0";
        
            }
            else
            {
                if(stackText.text != DatabaseManager.inventoryItemStack[name].ToString())
                     stackText.text = DatabaseManager.inventoryItemStack[name].ToString();
            }
        }
    }

    void LoadImage()
    {
        // ���ҽ� ���� ���� �ִ� �̹��� ������ ���
        string resourcePath = "Item/" + name;

        // ���ҽ��ε带 ���� �̹����� �����ɴϴ�.
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite != null)
        {
            // �̹����� ���������� �ҷ����� ���� ó��
            Image imageComponent = GetComponent<Image>();
            imageComponent.sprite = sprite;
            image = imageComponent;
        }
        else
        {
            Debug.LogError("�̹��� ������ �ε����� ���߽��ϴ�: " + resourcePath);
        }
    }

}
