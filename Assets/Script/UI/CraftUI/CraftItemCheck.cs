using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;


public class CraftItemCheck : MonoBehaviour
{

    public string name;
    public string type;
    public string description;
    public int price;
    public int weight;
    public string acqPath;
    public int maxStack;
    public int nowStack;
    public string effectOb;
    public string effectPow;
    public string equipArea;
    public TextMeshProUGUI stackText;
    public Image image;
    ItemData item;
    public string tfName;
    public string itemNameT;
    public CraftingManager craftingManager;
    public int tear;
    public string rarity;
    public int upgrade;
    public void OnPointerEnter()
    {
        Debug.Log(this.transform.name);
        
        craftingManager.MoveCusorByMouse(this.transform.parent.GetSiblingIndex(), this.transform.GetSiblingIndex());
    }



    private void Awake()
    {

    }

    private void Start()
    {
        SetItem(this.transform.name);

    }

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
        nowStack = 0;
        tfName = item.tfName;
        itemNameT = item.itemNameT;
        tear = item.tear;
        rarity = item.rarity;
        upgrade = item.upgrade;
        if(stackText != null)
        stackText.text = nowStack.ToString();
        if (type == "Consum")
        {
            effectOb = item.effectOb;
            effectPow = item.effectPow;
        }
        if (type == "Equip")
        {
            equipArea = item.equipArea;
        }
        if(image != null)
        LoadImage();

    }

    /*
    public void ConsumItemActive()
    {
        nowStack -= 1;
        string[] effect = effectOb.Split();
        if (effect[0] == "stemina")
        {
            if (effect[1] == "+")
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
    */
    // Update is called once per frame
    void Update()
    {
        stackText.text = nowStack.ToString();   
        stackText.gameObject.SetActive(true);
        if (DatabaseManager.inventoryItemStack.ContainsKey(this.transform.name) == true)
        {
            nowStack = DatabaseManager.inventoryItemStack[name];
        }
    }



    void LoadImage()
    {
        // 리소스 폴더 내에 있는 이미지 파일의 경로
        string resourcePath = "Item/" + name;

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
