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
