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
    public string type;
    public string description;
    public int price;
    public int weight;
    public string acqPath;
    public int maxStack;
    public int Buyprice;
    public int nowStack;
    public string effectOb;
    public string effectPow;
    public string equipArea;
    public TextMeshProUGUI stackText;
    public string tfName;
    public SpriteRenderer spriteRenderer;
    ItemData item;
    public string itemNameT;
    public int tear;
    public string rarity;
    public int upgrade;
    public string setName;

    public void SetItem(string itemName)
    {
        item = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = item.name;
        type = item.type;
        description = item.description;
        price = item.price;
        Buyprice = (int)(price * 1.5f);
        weight = item.weight;
        acqPath = item.acqPath;
        maxStack = item.maxStack;
        itemNameT = item.itemNameT;
        nowStack = 1;
        tfName = item.tfName;
        tear = item.tear;
        rarity = item.rarity;
        upgrade = item.upgrade;
        setName = item.setName;

        //Debug.Log(name);
        if (stackText != null)
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
        Invoke("DestroyItembyTime", 120);
    }

    void DestroyItembyTime()
    {
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
