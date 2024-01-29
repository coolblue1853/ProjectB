using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
public class EquipBoxCheck : MonoBehaviour, IPointerClickHandler
{
    public string equipArea = "";
    string reciveEquipArea;
    public GameObject equipPrefab;
    public AttackManager attackManager; // 무기 장착을 위한  AttackManager
    GameObject instantiatedPrefab;
    Weapon weapon;
    Weapon sideWeapon;
    Equipment head;
    Equipment chest;
    Equipment leg;
    Equipment hand;
    Equipment necklace;
    Equipment ring;
    public bool isSideWeaponBox = false;
    public void ActivePrefab(string reciveEquipArea)
    {
        if(equipPrefab == null)
        {
            GameObject ob = transform.GetChild(0).gameObject;
            ItemCheck obItemCheck = ob.GetComponent<ItemCheck>();
           LoadPrefab(obItemCheck.name, obItemCheck.equipArea);
        }
        if (reciveEquipArea == "Weapon" && weapon == null && isSideWeaponBox == false)
        {
             weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;

        }
        if (reciveEquipArea == "Weapon" && sideWeapon == null && isSideWeaponBox == true)
        {
            sideWeapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipSideWeapon = sideWeapon;

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
    public void DeletPrefab(string reciveEquipArea)
    {
        if (reciveEquipArea == "Weapon")
        {
            if(sideWeapon == false)
            {
                attackManager.equipWeapon = null;

                Destroy(weapon.gameObject);
                weapon = null;
            }
            else
            {
                attackManager.equipSideWeapon = null;

                Destroy(sideWeapon.gameObject);
                sideWeapon = null;
            }

        }
        else if (reciveEquipArea == "Head")
        {
            //  head = equipPrefab.GetComponent<Equipment>();

            PlayerHealthManager.Instance.EquipmentActiveFalse(head.hp, head.armor);
            Destroy(head.gameObject);
            head = null;
        }
        else if (reciveEquipArea == "Chest")
        {
            PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp, chest.armor);
            Destroy(chest.gameObject);
            chest = null;
        }
        else if (reciveEquipArea == "Leg")
        {
            PlayerHealthManager.Instance.EquipmentActiveFalse(leg.hp, leg.armor);
            Destroy(leg.gameObject);
            leg = null;
        }
        else if (reciveEquipArea == "Hand")
        {
            PlayerHealthManager.Instance.EquipmentActiveFalse(hand.hp, hand.armor);
            Destroy(hand.gameObject);
            hand = null;
        }
        else if (reciveEquipArea == "Necklace")
        {
            PlayerHealthManager.Instance.EquipmentActiveFalse(necklace.hp, necklace.armor);
            Destroy(necklace.gameObject);
            necklace = null;
        }
        else if (reciveEquipArea == "Ring")
        {
            PlayerHealthManager.Instance.EquipmentActiveFalse(ring.hp, ring.armor);
            Destroy(ring.gameObject);
            ring = null;
        }
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

    bool isSetArray =  false;
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
            DeletPrefab(reciveEquipArea);
        }
    }   
}