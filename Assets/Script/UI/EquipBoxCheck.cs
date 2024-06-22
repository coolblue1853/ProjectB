using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using DG.Tweening;
using AnyPortrait;
public class EquipBoxCheck : MonoBehaviour, IPointerClickHandler
{
    public string equipArea = "";
    string reciveEquipArea;
    public GameObject equipPrefab;
    public AttackManager attackManager; // 무기 장착을 위한  AttackManager
    GameObject instantiatedPrefab;
    Weapon weapon;
    Equipment head;
    Equipment chest;
    Equipment leg;
    Equipment hand;
    Equipment necklace;
    Equipment ring;
    Equipment shoes;
    public GameObject nowBox;
    public Texture2D defaultOffset;
    public apPortrait mainCharacter;
    private void Start()
    {
    }
    public void EquipMainWeapon()
    {
       // weapon = equipPrefab.GetComponent<Weapon>();
        attackManager.equipWeapon = weapon;
        attackManager.EquipMainWeaopon();
    }
    public void ActivePrefab(string reciveEquipArea)
    {
        if(equipPrefab == null)
        {
            GameObject ob = transform.GetChild(0).gameObject;
            ItemCheck obItemCheck = ob.GetComponent<ItemCheck>();
           LoadPrefab(obItemCheck.name, obItemCheck.equipArea, obItemCheck.tfName);
        }
        if (reciveEquipArea == "Weapon" && weapon == null)
        {

             weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;
            attackManager.EquipMainWeaopon();

        }
        else if (reciveEquipArea == "Head" && head == null)
        {
            head = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(head.hp);
            SetEquipmentCiritcalRate(head.critical);
            SetEquipmentDropRate(head.dropRate);
        }
        else if (reciveEquipArea == "Chest" && chest == null)
        {
            chest = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(chest.hp);
            SetEquipmentCiritcalRate(chest.critical);
            SetEquipmentDropRate(chest.dropRate);

        }
        else if (reciveEquipArea == "Leg" && leg == null)
        {
            leg = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(leg.hp);
            SetEquipmentCiritcalRate(leg.critical);
            SetEquipmentDropRate(leg.dropRate);
        }
        else if (reciveEquipArea == "Hand" && hand == null)
        {
            hand = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(hand.hp);
            SetEquipmentCiritcalRate(hand.critical);
            SetEquipmentDropRate(hand.dropRate);
        }
        else if (reciveEquipArea == "Necklace" && hand == null)
        {
            necklace = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(necklace.hp);
            SetEquipmentCiritcalRate(necklace.critical);
            SetEquipmentDropRate(necklace.dropRate);
        }
        else if (reciveEquipArea == "Ring" && hand == null)
        {
            ring = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(ring.hp);
            SetEquipmentCiritcalRate(ring.critical);
            SetEquipmentDropRate(ring.dropRate);
        }
        else if (reciveEquipArea == "Shoes" && shoes == null)
        {
            shoes = equipPrefab.GetComponent<Equipment>();
            SetEquipmentHp(shoes.hp);
            SetEquipmentCiritcalRate(shoes.critical);
            SetEquipmentDropRate(shoes.dropRate);
        }

    }
    public void SetEquipmentDropRate(int num)
    {
        DatabaseManager.playerDropRate += num; 
    }
    public void SetEquipmentCiritcalRate(int num)
    {
        DatabaseManager.playerCritRate += num;
    }
    public void SetEquipmentHp(int num)
    {
        PlayerHealthManager.Instance.EquipmentActiveTrue(num);
    }
    public void DownEquipmentDropRate(int num)
    {
        DatabaseManager.playerDropRate -= num; // 위의 함수가 2번 발동해서 *2를 해줘야 한다
    }
    public void DownEquipmentCiritcalRate(int num)
    {
        DatabaseManager.playerCritRate -= num;
    }

    GameObject deletChile ;
    public void DeletPrefab(ItemCheck detail, string reciveEquipArea, bool isFalse= true)
    {
        DatabaseManager.MinusSetDict(detail.tfName, 1);
        if (reciveEquipArea == "Weapon")
        {
            if (isFalse == false)
            {
                attackManager.UnEquipMainWeaopon();
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => attackManager.equipWeapon = null)
  .AppendCallback(() => Destroy(weapon.gameObject))
 .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
 .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (weapon != null)
                {
                    attackManager.UnEquipMainWeaopon();

                    attackManager.equipWeapon = null;
       
                    Destroy(weapon.gameObject);
                    weapon = null;
                }


            }
        }
         if (reciveEquipArea == "Head")
        {
            mainCharacter.SetMeshImage("Hat", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(head.hp))
.AppendCallback(() => DownEquipmentDropRate(head.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(head.critical))
  .AppendCallback(() => Destroy(head.gameObject))
 .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
 .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if(head != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(head.hp);
                    DownEquipmentDropRate(head.dropRate);
                    DownEquipmentCiritcalRate(head.critical);
                    Destroy(head.gameObject);
                    head = null;
                }
    

            }

           

        }
         if (reciveEquipArea == "Chest")
        {
            mainCharacter.SetMeshImage("RArm", defaultOffset);
            mainCharacter.SetMeshImage("Rsholder", defaultOffset);
            mainCharacter.SetMeshImage("LArm", defaultOffset);
            mainCharacter.SetMeshImage("LSholder", defaultOffset);
            mainCharacter.SetMeshImage("UppderBody", defaultOffset);
            mainCharacter.SetMeshImage("DwonBody", defaultOffset);
            if (isFalse == false)
            {

                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp))
.AppendCallback(() => DownEquipmentDropRate(chest.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(chest.critical))
.AppendCallback(() => Destroy(chest.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (chest != null)
                {

                    PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp);
                    DownEquipmentDropRate(chest.dropRate);
                    DownEquipmentCiritcalRate(chest.critical);
                    Destroy(chest.gameObject);
                    chest = null;
                }

            }



        }

         if (reciveEquipArea == "Leg")
        {
            mainCharacter.SetMeshImage("RDownLeg", defaultOffset);
            mainCharacter.SetMeshImage("RUpperLeg", defaultOffset);
            mainCharacter.SetMeshImage("LDownLeg", defaultOffset);
            mainCharacter.SetMeshImage("LUpperLeg", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(leg.hp))
.AppendCallback(() => DownEquipmentDropRate(leg.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(leg.critical))
.AppendCallback(() => Destroy(leg.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (leg != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(leg.hp);
                    DownEquipmentDropRate(leg.dropRate);
                    DownEquipmentCiritcalRate(leg.critical);
                    Destroy(leg.gameObject);
                    leg = null;
                }

            }


        }
         if (reciveEquipArea == "Hand")
        {
            mainCharacter.SetMeshImage("RHand", defaultOffset);
            mainCharacter.SetMeshImage("LHand", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
    .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(hand.hp))
    .AppendCallback(() => DownEquipmentDropRate(hand.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(hand.critical))
    .AppendCallback(() => Destroy(hand.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (hand != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(hand.hp);
                    DownEquipmentDropRate(hand.dropRate);
                    DownEquipmentCiritcalRate(hand.critical);
                    Destroy(hand.gameObject);
                    hand = null;
                }

            }


        }
         if (reciveEquipArea == "Necklace")
        {
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
    .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(necklace.hp))
    .AppendCallback(() => DownEquipmentDropRate(necklace.dropRate))
    .AppendCallback(() => DownEquipmentCiritcalRate(necklace.critical))
    .AppendCallback(() => Destroy(necklace.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (necklace != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(necklace.hp);
                    DownEquipmentDropRate(necklace.dropRate);
                    DownEquipmentCiritcalRate(necklace.critical);
                    Destroy(necklace.gameObject);
                    necklace = null;
                }

            }

        }
         if (reciveEquipArea == "Ring")
        {
            if(isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
       .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(ring.hp))
       .AppendCallback(() => DownEquipmentDropRate(ring.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(ring.critical))
.AppendCallback(() => Destroy(ring.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (ring != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(ring.hp);
                    DownEquipmentDropRate(ring.dropRate);
                    DownEquipmentCiritcalRate(ring.critical);
                    Destroy(ring.gameObject);
                    ring = null;
                }

            }
        }
        if (reciveEquipArea == "Shoes")
        {
            if (isFalse == false)
            {
                mainCharacter.SetMeshImage("RFoot", defaultOffset);
                mainCharacter.SetMeshImage("LFoot", defaultOffset);
                Sequence waitSequence = DOTween.Sequence()
       .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(shoes.hp))
       .AppendCallback(() => DownEquipmentDropRate(shoes.dropRate))
.AppendCallback(() => DownEquipmentCiritcalRate(shoes.critical))
.AppendCallback(() => Destroy(shoes.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (shoes != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(shoes.hp);
                    DownEquipmentDropRate(shoes.dropRate);
                    DownEquipmentCiritcalRate(shoes.critical);
                    Destroy(shoes.gameObject);
                    shoes = null;
                }

            }
        }
    }


    private void Awake()
    {
        nowBox = this.gameObject;
    }


    public void LoadPrefab(string equipName, string equipArea, string tfName)
    {
        DatabaseManager.PlusSetDict(tfName,1);


        reciveEquipArea = equipArea;
        if (equipArea == "Weapon")
        {

            string folderPath = "Weapon/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }

      else if (equipArea == "Head")
        {
            string folderPath = "Head/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("Hat", sprite);
            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Chest")
        {
            string folderPath = "Chest/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath+ tfName);

            if(sprite != null)
            {
                mainCharacter.SetMeshImage("RArm", sprite);
                mainCharacter.SetMeshImage("Rsholder", sprite);
                mainCharacter.SetMeshImage("LArm", sprite);
                mainCharacter.SetMeshImage("LSholder", sprite);
                mainCharacter.SetMeshImage("UppderBody", sprite);
                mainCharacter.SetMeshImage("DwonBody", sprite);
            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Leg")
        {
            string folderPath = "Leg/";
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RDownLeg", sprite);
                mainCharacter.SetMeshImage("RUpperLeg", sprite);
                mainCharacter.SetMeshImage("LDownLeg", sprite);
                mainCharacter.SetMeshImage("LUpperLeg", sprite);
            }
            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Hand")
        {
            string folderPath = "Hand/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RHand", sprite);
                mainCharacter.SetMeshImage("LHand", sprite);

            }
            // 
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Necklace")
        {
            string folderPath = "Necklace/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Ring")
        {
            string folderPath = "Ring/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Shoes")
        {
            string folderPath = "Shoes/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RFoot", sprite);
                mainCharacter.SetMeshImage("LFoot", sprite);

            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public bool isSetArray =  false;
    bool isFalse = false;
    public int siblingIndex;
    private void Update()
    {
        if (this.transform.childCount > 0 && isSetArray == false)
        {
            isSetArray = true;
            ActivePrefab(reciveEquipArea);
        }
        else if (this.transform.childCount <= 0 && isSetArray == true)
        {

            isSetArray = false;
         
        }
    }   
}