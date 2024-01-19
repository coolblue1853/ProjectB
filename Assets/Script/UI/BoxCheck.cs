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

    }
    bool isSetArray = false;
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
        // ���� Transform�� null�̰ų� Chest ��ũ��Ʈ�� �ִٸ� ��ȯ
        if (currentTransform == null)
        {
            return null;
        }

        Chest chestComponent = currentTransform.GetComponent<Chest>();
        if (chestComponent != null)
        {
            // Chest ��ũ��Ʈ�� �ִ� ��� �ش� ������Ʈ ��ȯ
            return chestComponent;
        }
        else
        {
            // Chest ��ũ��Ʈ�� ������ �θ�� �ö󰡴� ��� ȣ��
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
                // �ڽ��� �θ��� �� ��° �ڽ����� Ȯ��
                siblingIndex = transform.GetSiblingIndex();
            }
            if (parentTransform != null)
            {
                // �θ��� �θ� ��������
                Transform grandparentTransform = parentTransform.parent;

                if (grandparentTransform != null)
                {
                    boxCheck = this.transform.parent.GetComponent<InventoryAlpha>();
                    grandSiblingIndex = boxCheck.siblingIndex;
                    // �ڽ��� �θ��� �θ��� �� ��° �ڽ����� Ȯ��
                    //     grandSiblingIndex = parentTransform.GetSiblingIndex();
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