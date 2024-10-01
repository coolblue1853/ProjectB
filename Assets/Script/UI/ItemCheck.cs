using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
public class ItemCheck : MonoBehaviour
{
    public Image image;
    public ItemData itemData;
    public int nowStack;
    public int Buyprice;
    public int needCount;
    public string name;
    /*
    //코드 / 종류((장비인지 소모품인지)  / 이름 / 설명 / 보유수 / 가격 / 무게 // 획득방법

    public string type;
    public string description;
    public int price;

    public int weight;
    public string acqPath;
    public int maxStack;

    public string effectOb;
    public string effectPow;
    public string equipArea;

    public string setName;
    public string itemNameT;
    public string tfName;
    public int tear;
    public string rarity;
    public int upgrade;

    */
    public TextMeshProUGUI stackText;
    public GameObject defBuffObject;
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
        itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        Buyprice = (int)(itemData.price * 1.5f);
        nowStack = 1;
        name = itemData.name;
        /*

        type = item.type;
        description = item.description;
        price = item.price;
        weight = item.weight;
        acqPath = item.acqPath;
        maxStack = item.maxStack;
        itemNameT = item.itemNameT;
        tfName = item.tfName;
        tear = item.tear;
        rarity = item.rarity;
        upgrade = item.upgrade;
        setName = item.setName;

        //Debug.Log(name);
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
        */
        LoadImage();
    }
    public AttackManager att;
    public void ConsumItemActive()
    {
        nowStack -= 1;
        string[] effect = itemData.effectOb.Split("/");
        string[] effectPower = itemData.effectPow.Split("/");

        for (int i =0; i < effect.Length; i++)
        {
            string[] effectStr = effect[i].Split();
            string[] effectPowerDetail = effectPower[i].Split("_");
            if (effectStr[0] == "stemina")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.SteminaUp(int.Parse(effectPowerDetail[0]));
                }
            }
            if (effectStr[0] == "fullness")
            {

                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.FullnessUp(int.Parse(effectPowerDetail[0]));
                }
            }
            if (effectStr[0] == "hp")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.HpUp(int.Parse(effectPowerDetail[0]));
                }
            }
            if (effectStr[0] == "light")
            {
                if (effectStr[1] == "+")
                {
                    PlayerController.instance.TouchLightOn(float.Parse(effectPowerDetail[0]), float.Parse(effectPowerDetail[1]), float.Parse(effectPowerDetail[2]));
                }
            }
                if (effectStr[0] == "def")
            {
                if (effectStr[1] == "+")
                {
                    AttackManager  player = GameObject.FindWithTag("Player").GetComponent<AttackManager>();
                    if(player.foodBuff != null) // 이미 버프면
                    {
                        player.foodBuff.DestoryBuff();
                    }
                    GameObject buff = Instantiate(defBuffObject, Vector2.zero, Quaternion.identity, player.BuffSlot.transform);
                    PlayerBuff playerBuff = buff.GetComponent<PlayerBuff>();
                    player.foodBuff = playerBuff;
                    playerBuff.buffPower = int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                    // PlayerHealthManager.Instance.HpUp(int.Parse(effectPowerDetail[0]));
                }
                else
                {
                    AttackManager player = GameObject.FindWithTag("Player").GetComponent<AttackManager>();
                    if (player.foodBuff != null) // 이미 버프면
                    {
                        player.foodBuff.DestoryBuff();
                    }
                    GameObject buff = Instantiate(defBuffObject, Vector2.zero, Quaternion.identity, player.BuffSlot.transform);
                    PlayerBuff playerBuff = buff.GetComponent<PlayerBuff>();
                    player.foodBuff = playerBuff;
                    playerBuff.buffPower = -int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                }
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
        if(isCraftItem == false && isShopItem == false && stackText != null)
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


       else  if(isCraftItem == true && stackText != null)
        {
            if (DatabaseManager.inventoryItemStack.ContainsKey(itemData.name) == false)
            {
                stackText.text = "0/" + needCount;
            }
            else
            {
                stackText.text = + DatabaseManager.inventoryItemStack[itemData.name] +"/" + needCount;
            }
        }

        else  if(isShopItem == true && stackText != null)
        {
            if (Cost.text == "")
            {
                Cost.text = Buyprice.ToString();
            }


                if (DatabaseManager.inventoryItemStack.ContainsKey(itemData.name) == false)
            {
                if(stackText.text != "0")
                     stackText.text = "0";
        
            }
            else
            {
                if(stackText.text != DatabaseManager.inventoryItemStack[itemData.name].ToString())
                     stackText.text = DatabaseManager.inventoryItemStack[itemData.name].ToString();
            }
        }
    }

    void LoadImage()
    {
        // 리소스 폴더 내에 있는 이미지 파일의 경로
        string resourcePath = "Item/" + itemData.name;

        // 리소스로드를 통해 이미지를 가져옵니다.
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite != null)
        {
           
            // 이미지를 성공적으로 불러왔을 때의 처리
            Image imageComponent = GetComponent<Image>();
            imageComponent.sprite = sprite;
            image = imageComponent;
        }
        else
        {
            Debug.LogError("이미지 파일을 로드하지 못했습니다: " + resourcePath);
        }
    }

}
