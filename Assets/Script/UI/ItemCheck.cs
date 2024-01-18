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
    public TextMeshProUGUI stackText;
    public Image image;
    Item item;


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
        stackText.text = nowStack.ToString();
        LoadImage();
    }

    // Update is called once per frame
    void Update()
    {
        if(nowStack == 1)
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
