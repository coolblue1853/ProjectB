using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using TMPro;
public class ItemCheck : MonoBehaviour
{
    public Image image;
    public ItemData itemData;
    public int nowStack;
    public int buyprice;
    public int needCount;
    public string name;

    public TextMeshProUGUI stackText;
    public GameObject defBuffObject;
    public bool isCraftItem = false;
    public bool isShopItem = false;
    private void Start()
    {
        if (isShopItem)
            SetItem(this.transform.name);
    }
    public void SetItem(string itemName, ItemData item = null)
    {
        if(item != null)
            itemData = item;
        else
            itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));

        buyprice = (int)(itemData.price * 1.5f);
        nowStack = 1;
        name = itemData.name;
        LoadImage();
    }
    public AttackManager attackManager;
    public void ConsumItemActive()
    {
        nowStack -= 1;
        string[] effect = itemData.effectOb.Split("/");
        string[] effectPower = itemData.effectPow.Split("/");

        StringBuilder logBuilder = new StringBuilder(); // StringBuilder �ν��Ͻ� ����

        for (int i = 0; i < effect.Length; i++)
        {
            string[] effectStr = effect[i].Split();
            string[] effectPowerDetail = effectPower[i].Split("_");

            if (effectStr[0] == "stemina")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.SteminaUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"Stemina increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "fullness")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.FullnessUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"Fullness increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "hp")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.HpUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"HP increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "light")
            {
                if (effectStr[1] == "+")
                {
                    PlayerController.instance.TouchLightOn(float.Parse(effectPowerDetail[0]), float.Parse(effectPowerDetail[1]), float.Parse(effectPowerDetail[2]));
                    logBuilder.AppendLine($"Light activated with parameters {effectPowerDetail[0]}, {effectPowerDetail[1]}, {effectPowerDetail[2]}");
                }
            }
            else if (effectStr[0] == "def")
            {
                AttackManager player = GameObject.FindWithTag("Player").GetComponent<AttackManager>();
                if (player.foodBuff != null) // �̹� ������
                {
                    player.foodBuff.DestoryBuff();
                }
                GameObject buff = Instantiate(defBuffObject, Vector2.zero, Quaternion.identity, player.BuffSlot.transform);
                PlayerBuff playerBuff = buff.GetComponent<PlayerBuff>();
                player.foodBuff = playerBuff;

                if (effectStr[1] == "+")
                {
                    playerBuff.buffPower = int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                    logBuilder.AppendLine($"Defense buff activated with power {effectPowerDetail[0]} and duration {effectPowerDetail[1]}");
                }
                else
                {
                    playerBuff.buffPower = -int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                    logBuilder.AppendLine($"Defense debuff activated with power {effectPowerDetail[0]} and duration {effectPowerDetail[1]}");
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
                Cost.text = buyprice.ToString();
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
        // ���ҽ� ���� ���� �ִ� �̹��� ������ ���
        string resourcePath = "Item/" + itemData.name;

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
