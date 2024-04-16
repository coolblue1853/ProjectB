using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DropItemCheck : MonoBehaviour
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
    public string effectOb;
    public int effectPow;
    public string equipArea;
    public TextMeshProUGUI stackText;
    public Image image;
    ItemData item;


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
        if (type == "Consum")
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

    // Update is called once per frame
    void Update()
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground")
        {
            Rigidbody2D rigidbody = this.GetComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            Debug.Log("생성2");
            this.gameObject.layer =1;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.tag == "Player")
        {
            if(InventoryManager.instance.CheckBoxCanCreatAll() == true)
            {
                InventoryManager.instance.CreatItem(name);
                Destroy(this.gameObject);
            }

        }
        else
        {

        }
    }

    void LoadImage()
    {

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

            // 현재 GameObject의 SpriteRenderer 컴포넌트 가져오기
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            // SpriteRenderer의 sprite 변경
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        }
        else
        {
            Debug.LogError("이미지 파일이 존재하지 않습니다: " + imagePath);
        }
    }
}
