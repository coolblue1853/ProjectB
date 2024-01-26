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
    Equipment head;
    Equipment chest;


    public void ActivePrefab(string reciveEquipArea)
    {
        if (reciveEquipArea == "Weapon" && weapon == null)
        {
             weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;

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

        ActiveBoolCheck(reciveEquipArea);
    }
    public void DeletPrefab(string reciveEquipArea)
    {
        if (reciveEquipArea == "Weapon")
        {
            // weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = null;

            Destroy(weapon.gameObject);
            //    Destroy(instantiatedPrefab.gameObject);
            weapon = null;
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
            //  chest = equipPrefab.GetComponent<Equipment>();

            PlayerHealthManager.Instance.EquipmentActiveFalse(chest.hp, chest.armor);
            Destroy(chest.gameObject);
            chest = null;
        }
        DeletBoolCheck(reciveEquipArea);
    }


    public void ActiveBoolCheck(string reciveEquipArea)
    {
        if (reciveEquipArea == "Head")
        {

        }
        else if (reciveEquipArea == "Head")
        {

        }
        else if (reciveEquipArea == "Chest")
        {

        }
    }
    public void DeletBoolCheck(string reciveEquipArea)
    {
        if (reciveEquipArea == "Head")
        {

        }
        else if (reciveEquipArea == "Head")
        {

        }
        else if (reciveEquipArea == "Chest")
        {

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

        if (equipArea == "Head")
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
        if (equipArea == "Chest")
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