using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using DG.Tweening;
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
    public GameObject nowBox;


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
           LoadPrefab(obItemCheck.name, obItemCheck.equipArea);
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
            PlayerHealthManager.Instance.EquipmentActiveTrue(head.hp, head.armor);
        }
        else if (reciveEquipArea == "Chest" && chest == null)
        {
            chest = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(chest.hp, chest.armor);
        }
        else if (reciveEquipArea == "Leg" && leg == null)
        {
            leg = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(leg.hp, leg.armor);
        }
        else if (reciveEquipArea == "Hand" && hand == null)
        {
            hand = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(hand.hp, hand.armor);
        }
        else if (reciveEquipArea == "Necklace" && hand == null)
        {
            necklace = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(necklace.hp, necklace.armor);
        }
        else if (reciveEquipArea == "Ring" && hand == null)
        {
            ring = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(ring.hp, ring.armor);
        }

    }
    GameObject deletChile ;
    public void DeletPrefab(string reciveEquipArea, bool isFalse= true)
    {

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
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(head.hp, head.armor))
  .AppendCallback(() => Destroy(head.gameObject))
 .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
 .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if(head != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(head.hp, head.armor);
                    Destroy(head.gameObject);
                    head = null;
                }
    

            }

           

        }
         if (reciveEquipArea == "Chest")
        {
            if (isFalse == false)
            {

                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp, chest.armor))
.AppendCallback(() => Destroy(chest.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (chest != null)
                {

                    PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp, chest.armor);
                    Destroy(chest.gameObject);
                    chest = null;
                }

            }



        }

         if (reciveEquipArea == "Leg")
        {
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(leg.hp, leg.armor))
.AppendCallback(() => Destroy(leg.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (leg != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(leg.hp, leg.armor);
                    Destroy(leg.gameObject);
                    leg = null;
                }

            }


        }
         if (reciveEquipArea == "Hand")
        {
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
    .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(hand.hp, hand.armor))
    .AppendCallback(() => Destroy(hand.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (hand != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(hand.hp, hand.armor);
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
    .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(necklace.hp, necklace.armor))
    .AppendCallback(() => Destroy(necklace.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (necklace != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(necklace.hp, necklace.armor);
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
       .AppendCallback(() => PlayerHealthManager.Instance.EquipmentActiveFalse(ring.hp, ring.armor))
.AppendCallback(() => Destroy(ring.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (ring != null)
                {
                    PlayerHealthManager.Instance.EquipmentActiveFalse(ring.hp, ring.armor);
                    Destroy(ring.gameObject);
                    ring = null;
                }

            }




        }

    }


    private void Awake()
    {
        nowBox = this.gameObject;
    }


    public void LoadPrefab(string equipName, string equipArea)
    {
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
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public bool isSetArray =  false;
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