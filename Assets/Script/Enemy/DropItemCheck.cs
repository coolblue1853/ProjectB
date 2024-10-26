using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DropItemCheck : PoolAble
{
    //코드 / 종류((장비인지 소모품인지)  / 이름 / 설명 / 보유수 / 가격 / 무게 // 획득방법
    public string name;
    public Image image;
    public ItemData itemData;
    public SpriteRenderer spriteRenderer;
    public int nowStack;
    public int Buyprice;
    public int needCount;

    public void SetItem(string itemName)
    {
        itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = itemData.name;
        Buyprice = (int)(itemData.price * 1.5f);
        nowStack = 1;
        LoadImage();
        Invoke("DestroyItembyTime", 120);
    }

    void DestroyItembyTime()
    {
        if(this.gameObject != null)
            ReleaseObject();
    }

    bool isGround = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Ground"|| collision.transform.tag == "Wall")
        {
            Rigidbody2D rigidbody = this.GetComponent<Rigidbody2D>();
            rigidbody.velocity = Vector2.zero;
            rigidbody.gravityScale = 0;
            BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;

            this.gameObject.layer = 1;
            Invoke("GroundCheckOn", 0.1f);
        }
    }
    void GroundCheckOn()
    {
        isGround = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && isGround == true)
        {
            if(InventoryManager.instance.CheckBoxCanCreatAll() == true || InventoryManager.instance.OnlyCheckStack(name) == true)
            {
                InventoryManager.instance.CreatItem(name);
                ReleaseObject();
            }
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
            SpriteRenderer imageComponent = GetComponent<SpriteRenderer>();
            imageComponent.sprite = sprite;
            spriteRenderer = imageComponent;
        }
        else
        {
            Debug.LogError("이미지 파일을 로드하지 못했습니다: " + resourcePath);
        }
    }

}
