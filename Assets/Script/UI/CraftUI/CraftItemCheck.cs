using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;


public class CraftItemCheck : MonoBehaviour
{
    public TextMeshProUGUI stackText;
    public Image image;
    public ItemData itemData;
    public int nowStack;
    public int Buyprice;
    public int needCount;
    public string name;
    public CraftingManager craftingManager;

    public void OnPointerEnter()
    {
        craftingManager.MoveCusorByMouse(this.transform.parent.GetSiblingIndex(), this.transform.GetSiblingIndex());
    }



    private void Awake()
    {
        SetItem(this.transform.name); 
    }

    private void Start()
    {


    }

    public void SetItem(string itemName)
    {
        itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = itemData.name;
        nowStack = 0;

     if(image != null)
         LoadImage();
    }

    // Update is called once per frame
    void Update()
    {
        stackText.text = nowStack.ToString();   
        stackText.gameObject.SetActive(true);
        if (DatabaseManager.inventoryItemStack.ContainsKey(this.transform.name) == true)
        {
            nowStack = DatabaseManager.inventoryItemStack[itemData.name];
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
