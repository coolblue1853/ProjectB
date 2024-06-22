using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DropItemCheck : MonoBehaviour
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
    private void Start()
    {

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
        nowStack = 1;
        tfName = item.tfName;
        itemNameT = item.itemNameT;
        tear = item.tear;
        rarity = item.rarity;
        upgrade = item.upgrade;
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

    }
    bool isGround = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Ground")
        {
            Rigidbody2D rigidbody = this.GetComponent<Rigidbody2D>();
            rigidbody.velocity = Vector2.zero;
            rigidbody.gravityScale = 0;
            BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            Debug.Log("����2");
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
                Destroy(this.gameObject);
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
