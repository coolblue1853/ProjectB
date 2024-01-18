using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
public class ItemCheck : MonoBehaviour
{
    //코드 / 종류((장비인지 소모품인지)  / 이름 / 설명 / 보유수 / 가격 / 무게 // 획득방법
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
        // 이미지 파일이 저장된 폴더 경로
        string folderPath = "Assets/Sprite/Item/";

        // 이미지 파일의 전체 경로
        string imagePath = Path.Combine(folderPath, name + ".png");

        // 이미지 파일이 존재하는지 확인
        if (File.Exists(imagePath))
        {
            // 이미지 파일을 바이트 배열로 읽어오기
            byte[] imageData = File.ReadAllBytes(imagePath);

            // Texture2D를 생성하고 이미지 데이터를 로드
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            // 현재 GameObject의 Image 컴포넌트 가져오기
            Image imageComponent = GetComponent<Image>();

            // Image 컴포넌트의 sprite 변경
            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            image = imageComponent;
        }
        else
        {
            Debug.LogError("이미지 파일이 존재하지 않습니다: " + imagePath);
        }
    }
}
