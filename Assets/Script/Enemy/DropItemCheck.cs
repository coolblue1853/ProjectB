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
    [System.NonSerialized] public DropManager owner; // 이 드롭을 떨어뜨린 스포너 (공유 풀)

    public void SetItem(string itemName)
    {
        itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        name = itemData.name;
        Buyprice = (int)(itemData.price * 1.5f);
        nowStack = 1;
        LoadImage();
        ApplyRarityLook(itemData.rarity);
        Invoke("DestroyItembyTime", 120);
    }
    Rigidbody2D rigidbody;
    BoxCollider2D boxCollider2D;
    static Material rareMaterial, uniqueMaterial;
    Material defaultMaterial;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        defaultMaterial = GetComponent<SpriteRenderer>().sharedMaterial;
    }

    /// <summary>등급 연출: C 기본 / R 파란 1px 아웃라인 / U 금색 아웃라인+글로우 (공유 머티리얼, 개체별 변경 없음)</summary>
    void ApplyRarityLook(string rarity)
    {
        if (rareMaterial == null)
        {
            rareMaterial = Resources.Load<Material>("FX/M_Drop_R");
            uniqueMaterial = Resources.Load<Material>("FX/M_Drop_U");
        }
        var sr = GetComponent<SpriteRenderer>();
        switch (rarity)
        {
            case "R": sr.sharedMaterial = rareMaterial != null ? rareMaterial : defaultMaterial; break;
            case "U": sr.sharedMaterial = uniqueMaterial != null ? uniqueMaterial : defaultMaterial; break;
            default: sr.sharedMaterial = defaultMaterial; break;
        }
        DropLightPillar.Apply(gameObject, rarity);
    }
    private void OnEnable()
    {
        rigidbody.gravityScale = 1;
        isGround = false;
    }
    private void OnDisable()
    {
        CancelInvoke(); // 반환된 뒤 120초 소멸 타이머가 재사용된 아이템을 지우지 않도록
    }
    void DestroyItembyTime()
    {
        if (this.gameObject != null && gameObject.activeSelf != false)
        {
            ReleaseObject();
        }
    }

    bool isGround = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Ground"|| collision.transform.tag == "Wall" || collision.transform.tag == "InGroundPlayer")
        {
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.gravityScale = 0;
            //boxCollider2D.isTrigger = true;
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
                if (this.gameObject != null && gameObject.activeSelf != false)
                {
                    ReleaseObject();
                }
                string rarity = itemData != null ? itemData.rarity : "C";
                if (InventoryManager.instance.CreatItem(name, false, null, AddSource.Pickup))
                {
                    PickupLog.Report(name, 1, rarity);
                    PixelCrushers.MessageSystem.SendMessage(this, "Pickup", name); // 수집 퀘스트 (G11)
                }
                Sfx.Play(rarity == "U" ? SfxId.RareDrop : SfxId.Pickup);
            }
            else
            {
                PickupLog.ReportFull(); // 2초 스로틀 (이전엔 조용히 무시)
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
