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
    public void ActivePrefab(string reciveEquipArea)
    {
        if (reciveEquipArea == "Weapon")
        {
            Weapon weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;

        }
        else
        {
            Equipment equipment = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveTrue(equipment.hp, equipment.armor);
        }

    }
    public void DeletPrefab(string reciveEquipArea)
    {
        if (reciveEquipArea == "Weapon")
        {
            Weapon weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = null;
            Destroy(instantiatedPrefab.gameObject);
        }
        else
        {
            Equipment equipment = equipPrefab.GetComponent<Equipment>();
            PlayerHealthManager.Instance.EquipmentActiveFalse(equipment.hp, equipment.armor);
            Destroy(instantiatedPrefab.gameObject);
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