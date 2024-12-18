using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BoxCheck : MonoBehaviour, IPointerClickHandler
{
    public bool isInventoryBox = true;
    public int siblingIndex;
    public int grandSiblingIndex;
    public int arrayCheck;
    Chest chestComponent;
    InventoryAlpha boxCheck;
    public void OnPointerClick(PointerEventData eventData)
    {
        /*
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isInventoryBox)
            {
                InventoryManager.instance.ChangeCusor(this.gameObject);
            }
            else
            {
                chestComponent.ChangeCusor(this.gameObject);
            }

        }
        */
    }
   public  bool isSetArray = false;
    void ChildCheck()
    {
        if (this.transform.childCount > 0 && isSetArray == false)
        {
            isSetArray = true;
            InventoryManager.instance.inventoryArray[siblingIndex, grandSiblingIndex] = 1;
            arrayCheck = 1;
        }
        else if (this.transform.childCount <= 0 && isSetArray == true)
        {
            isSetArray = false;
            InventoryManager.instance.inventoryArray[siblingIndex, grandSiblingIndex] = 0;
            arrayCheck = 0;

        }
    }
    void ChestChildCheck()
    {
        chestComponent = FindParentWithChestScript(transform);

        if (this.transform.childCount > 0 && isSetArray == false)
        {
            isSetArray = true;
            chestComponent.inventoryArray[siblingIndex, grandSiblingIndex] = 1;
            arrayCheck = 1;
        }
        else if (this.transform.childCount <= 0 && isSetArray == true)
        {
            isSetArray = false;
            chestComponent.inventoryArray[siblingIndex, grandSiblingIndex] = 0;
            arrayCheck = 0;

        }
    }
    Transform parentTransform;

    Chest FindParentWithChestScript(Transform currentTransform)
    {
        // 현재 Transform이 null이거나 Chest 스크립트가 있다면 반환
        if (currentTransform == null)
        {
            return null;
        }

        Chest chestComponent = currentTransform.GetComponent<Chest>();
        if (chestComponent != null)
        {
            // Chest 스크립트가 있는 경우 해당 오브젝트 반환
            return chestComponent;
        }
        else
        {
            // Chest 스크립트가 없으면 부모로 올라가는 재귀 호출
            return FindParentWithChestScript(currentTransform.parent);
        }
    }
    void CheckArray()
    {

        if (parentTransform == null)
        {
            parentTransform = transform.parent;

            if (parentTransform != null)
            {
                // 자신이 부모의 몇 번째 자식인지 확인
                siblingIndex = transform.GetSiblingIndex();
            }
            if (parentTransform != null)
            {
                // 부모의 부모 가져오기
                Transform grandparentTransform = parentTransform.parent;

                if (grandparentTransform != null)
                {
                    boxCheck = this.transform.parent.GetComponent<InventoryAlpha>();
                    grandSiblingIndex = int.Parse(parentTransform.name);
                    // 자신이 부모의 부모의 몇 번째 자식인지 확인
                    // grandSiblingIndex = parentTransform.GetSiblingIndex();
                }
            }
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        CheckArray();
    }

    bool onceCheck = false;
    // Update is called once per frame
    void Update()
    {
        if (isInventoryBox == true)
        {
            ChildCheck();
        }
        else if (isInventoryBox == false)
        {
            ChestChildCheck();
        }

    }

}