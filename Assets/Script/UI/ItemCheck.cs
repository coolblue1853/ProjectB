using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using TMPro;
public class ItemCheck : MonoBehaviour
{
    public Image image;
    public ItemData itemData;
    public int nowStack;
    public int buyprice;
    public int needCount;
    /// <summary>아이템 시트 이름. Object.name(게임오브젝트 이름)을 의도적으로 가린다 — 직렬화 필드라 이름 변경은 인벤 리팩터에서(B-023)</summary>
    public new string name;

    public TextMeshProUGUI stackText;
    public GameObject defBuffObject;
    public bool isCraftItem = false;
    public bool isShopItem = false;
    private void Start()
    {
        if (isShopItem)
            SetItem(this.transform.name);
    }
    public void SetItem(string itemName, ItemData item = null)
    {
        if(item != null)
            itemData = item;
        else
            itemData = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(itemName));
        if (itemData == null) // 시트에 없는 이름(손상·구버전 세이브). 호출 측이 itemData null 을 보고 정리한다(B-020)
        {
            Debug.LogWarning("[PB][Item] 아이템 시트에 없는 이름: " + itemName);
            return;
        }

        _shownStack = _shownHave = _shownNeed = int.MinValue; // 재사용 시 표시 캐시 무효화
        buyprice = (int)(itemData.price * 1.5f);
        nowStack = 1;
        name = itemData.name;
        LoadImage();
    }
    public AttackManager attackManager;
    /// <summary>소비 아이템 효과만 적용한다(수량은 건드리지 않음)</summary>
    public void ApplyConsumeEffect()
    {
        string[] effect = itemData.effectOb.Split("/");
        string[] effectPower = itemData.effectPow.Split("/");

        StringBuilder logBuilder = new StringBuilder(); // StringBuilder 인스턴스 생성

        for (int i = 0; i < effect.Length; i++)
        {
            string[] effectStr = effect[i].Split();
            string[] effectPowerDetail = effectPower[i].Split("_");

            if (effectStr[0] == "stemina")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.SteminaUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"Stemina increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "fullness")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.FullnessUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"Fullness increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "hp")
            {
                if (effectStr[1] == "+")
                {
                    PlayerHealthManager.Instance.HpUp(int.Parse(effectPowerDetail[0]));
                    logBuilder.AppendLine($"HP increased by {effectPowerDetail[0]}");
                }
            }
            else if (effectStr[0] == "light")
            {
                if (effectStr[1] == "+")
                {
                    PlayerController.instance.TouchLightOn(float.Parse(effectPowerDetail[0]), float.Parse(effectPowerDetail[1]), float.Parse(effectPowerDetail[2]));
                    logBuilder.AppendLine($"Light activated with parameters {effectPowerDetail[0]}, {effectPowerDetail[1]}, {effectPowerDetail[2]}");
                }
            }
            else if (FoodBuff.TryParseKind(effectStr[0], out FoodBuffKind foodKind) && effectStr[1] == "+")
            {
                // 요리 버프(탐험가 요리): 음식 버프는 1종만 — 기존 방어 음식 버프도 지운다
                AttackManager cook = GameObject.FindWithTag("Player").GetComponent<AttackManager>();
                if (cook.foodBuff != null)
                {
                    cook.foodBuff.DestoryBuff();
                }
                FoodBuff.Apply(foodKind, int.Parse(effectPowerDetail[0]), int.Parse(effectPowerDetail[1]));
                logBuilder.AppendLine($"Food buff {foodKind} +{effectPowerDetail[0]} for {effectPowerDetail[1]}s");
            }
            else if (effectStr[0] == "def")
            {
                FoodBuff.Clear();
                AttackManager player = GameObject.FindWithTag("Player").GetComponent<AttackManager>();
                if (player.foodBuff != null) // 이미 버프면
                {
                    player.foodBuff.DestoryBuff();
                }
                GameObject buff = Instantiate(defBuffObject, Vector2.zero, Quaternion.identity, player.BuffSlot.transform);
                PlayerBuff playerBuff = buff.GetComponent<PlayerBuff>();
                player.foodBuff = playerBuff;

                if (effectStr[1] == "+")
                {
                    playerBuff.buffPower = int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                    logBuilder.AppendLine($"Defense buff activated with power {effectPowerDetail[0]} and duration {effectPowerDetail[1]}");
                }
                else
                {
                    playerBuff.buffPower = -int.Parse(effectPowerDetail[0]);
                    playerBuff.buffTime = int.Parse(effectPowerDetail[1]);
                    playerBuff.ActiveBuff();
                    logBuilder.AppendLine($"Defense debuff activated with power {effectPowerDetail[0]} and duration {effectPowerDetail[1]}");
                }
            }
        }

    }


    public TextMeshProUGUI Cost;
    int _shownStack = int.MinValue; // 마지막으로 그린 수량(바뀔 때만 텍스트·SetActive 갱신, B-023)
    int _shownHave = int.MinValue;
    int _shownNeed = int.MinValue;

    void Update()
    {
        if (stackText == null || itemData == null)
        {
            return;
        }
        if (isCraftItem == false && isShopItem == false)
        {
            if (nowStack != _shownStack)
            {
                _shownStack = nowStack;
                stackText.text = nowStack.ToString();
            }
            bool showCount = nowStack != 1;
            if (stackText.gameObject.activeSelf != showCount) // 다른 코드가 켜 두는 경우가 있어 표시 여부는 매번 비교(할당 없음)
            {
                stackText.gameObject.SetActive(showCount);
            }
        }
        else if (isCraftItem == true)
        {
            int have = HaveCount();
            if (have != _shownHave || needCount != _shownNeed)
            {
                _shownHave = have;
                _shownNeed = needCount;
                stackText.text = have + "/" + needCount;
            }
        }
        else if (isShopItem == true)
        {
            if (Cost != null && Cost.text == "")
            {
                Cost.text = buyprice.ToString();
            }
            int have = HaveCount();
            if (have != _shownHave)
            {
                _shownHave = have;
                stackText.text = have.ToString();
            }
        }
    }

    /// <summary>인벤토리에 가진 개수(표시용 캐시)</summary>
    int HaveCount()
    {
        return DatabaseManager.inventoryItemStack.TryGetValue(itemData.name, out int n) ? n : 0;
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
