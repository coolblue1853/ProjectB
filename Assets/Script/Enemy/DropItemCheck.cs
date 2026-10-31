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
        goldAmount = 0;
        LoadImage();
        ApplyRarityLook(itemData.rarity);
        Invoke("DestroyItembyTime", 120);
    }
    /// <summary>0 보다 크면 골드 더미(itemData 없음). 줍으면 DatabaseManager.money 가 오른다 (ClassicMapleDesign 14절)</summary>
    [System.NonSerialized] public int goldAmount;
    public static int GoldPickedCount, GoldPickedTotal; // 검증용 누적(프로브가 읽음)
    bool IsGold => goldAmount > 0;

    /// <summary>골드 더미로 설정: 금액에 따라 스프라이트 3단계</summary>
    public void SetGold(int amount)
    {
        itemData = null;
        goldAmount = amount;
        name = "Gold";
        Buyprice = 0;
        nowStack = 1;
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = GoldSprites.Get(GoldDropRules.Tier(amount));
        sr.sharedMaterial = defaultMaterial != null ? defaultMaterial : sr.sharedMaterial;
        spriteRenderer = sr;
        DropLightPillar.Apply(gameObject, "C");
        Invoke("DestroyItembyTime", 120);
    }

    void PickUpGold()
    {
        int amount = goldAmount;
        goldAmount = 0;
        ReleaseObject();
        DatabaseManager.money += amount;
        GoldPickedCount++;
        GoldPickedTotal += amount;
        PickupLog.ReportGold(amount);
        Sfx.Play(SfxId.Pickup);
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
        _magnet = false;
        _magnetSpeed = 0f;
        _roomCheckAt = 0f;
    }

    // ── 줍기 자석 (BasicFeatures.md 4절): 바닥에 닿은 드롭이 가까우면 플레이어 쪽으로 끌려온다. 실제 줍기는 기존 접촉 경로 ──
    private bool _magnet;
    private float _magnetSpeed;
    private float _roomCheckAt;
    private const float RoomCheckInterval = 0.5f;

    private void Update()
    {
        if (!isGround || (itemData == null && !IsGold) || PlayerController.instance == null || InventoryManager.instance == null)
        {
            return;
        }
        if (!IsGold && !PickupRules.ShouldPickUp(itemData.type, itemData.rarity, GameSettings.Pickup))
        {
            _magnet = false;
            return;
        }
        Vector2 pos = transform.position;
        Vector2 target = PlayerController.instance.transform.position;
        bool wasMagnet = _magnet;
        _magnet = PickupRules.UpdateMagnet(_magnet, (target - pos).sqrMagnitude);
        if (_magnet && !wasMagnet && !IsGold)
        {
            // 인벤이 가득 차서 못 주울 아이템은 끌어오지 않는다(발밑에 쌓여 경고만 반복). 검사는 0.5초에 한 번
            // 타이머는 실제로 HasRoomFor 를 불렀을 때만 민다(범위 안에 머무는 동안에도 0.5초마다 재검사)
            if (Time.time < _roomCheckAt)
            {
                _magnet = false;
            }
            else if (!InventoryManager.instance.HasRoomFor(name))
            {
                _roomCheckAt = Time.time + RoomCheckInterval;
                _magnet = false;
            }
        }
        if (!_magnet)
        {
            if (wasMagnet)
            {
                // 끌려오다 놓치면(대시·포털) 공중에 멈추지 않게 다시 떨어뜨린다
                rigidbody.gravityScale = 1;
                isGround = false;
            }
            _magnetSpeed = 0f;
            return;
        }
        _magnetSpeed = PickupRules.Accelerate(_magnetSpeed, Time.deltaTime);
        Vector2 next = Vector2.MoveTowards(pos, target, _magnetSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }
    private void OnDisable()
    {
        goldAmount = 0;
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
            if (IsGold)
            {
                PickUpGold(); // 골드는 인벤 공간·줍기 필터와 무관
                return;
            }
            if (itemData != null && !PickupRules.ShouldPickUp(itemData.type, itemData.rarity, GameSettings.Pickup))
            {
                return; // 줍기 필터로 거른 아이템(설정 PICKUP)
            }
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
