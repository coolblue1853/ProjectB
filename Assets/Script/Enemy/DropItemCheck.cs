using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DropItemCheck : PoolAble
{
    //�ڵ� / ����((������� �Ҹ�ǰ����)  / �̸� / ���� / ������ / ���� / ���� // ȹ����
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
        // ���ҽ� ���� ���� �ִ� �̹��� ������ ���
        string resourcePath = "Item/" + name;

        // ���ҽ��ε带 ���� �̹����� �����ɴϴ�.
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite != null)
        {
            // �̹����� ���������� �ҷ����� ���� ó��
            SpriteRenderer imageComponent = GetComponent<SpriteRenderer>();
            imageComponent.sprite = sprite;
            spriteRenderer = imageComponent;
        }
        else
        {
            Debug.LogError("�̹��� ������ �ε����� ���߽��ϴ�: " + resourcePath);
        }
    }

}
