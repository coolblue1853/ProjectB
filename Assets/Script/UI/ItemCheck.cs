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
    Item item;

    public bool isCraftItem = false;

    public void SetItem(string itemName)
    {

        item = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = item.name;
        type = item.type;
        description = item.description;
        price = item.price;
        weight = item.weight;
        acqPath = item.acqPath;
        maxStack = item.maxStack;
        nowStack = 1;
        if(stackText != null)
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
        if (this.nowStack == 0)
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isCraftItem == false)
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


        if(isCraftItem == true)
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
        
    }
   
    void LoadImage()
    {
        // �̹��� ������ ����� ���� ���
        string folderPath = "Assets/Sprite/Item/";

        // �̹��� ������ ��ü ���
        string imagePath = Path.Combine(folderPath, name + ".png");

        // �̹��� ������ �����ϴ��� Ȯ��
        if (File.Exists(imagePath))
        {
            // �̹��� ������ ����Ʈ �迭�� �о����
            byte[] imageData = File.ReadAllBytes(imagePath);

            // Texture2D�� �����ϰ� �̹��� �����͸� �ε�
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            // ���� GameObject�� Image ������Ʈ ��������
            Image imageComponent = GetComponent<Image>();

            // Image ������Ʈ�� sprite ����
            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            image = imageComponent;
        }
        else
        {
            Debug.LogError("�̹��� ������ �������� �ʽ��ϴ�: " + imagePath);
        }
    }
}
