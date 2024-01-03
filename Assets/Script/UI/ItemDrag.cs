using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler

{
    public static Vector2 DefaultPos;
    Transform currentParent;
    Transform changeParent;
    Transform changeBox;
    ItemCheck itemCheck;
    int siblingParentIndex;
    int siblingIndex;

    private void Start()
    {
        itemCheck = this.transform.GetComponent<ItemCheck>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log(this.name); // ������â ����.
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.instance.ChangeCusor(this.gameObject.transform.parent.gameObject);
        }

    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
         currentParent = transform.parent;

        if (currentParent != null)
        {
            // �θ��� �θ�(Transform) ã��
            Transform grandParent = currentParent.parent.parent;
             siblingParentIndex = transform.parent.parent.GetSiblingIndex();
             siblingIndex = transform.parent.GetSiblingIndex();

            if (grandParent != null)
            {
                // �θ��� �θ��� �ڽ����� ���� ��ü �߰�
                transform.SetParent(grandParent);
                InventoryManager.instance.TestDelet(siblingParentIndex, siblingIndex);
            }
            else
            {
                Debug.LogWarning("�θ��� �θ� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("�θ� �����ϴ�.");
        }
        DefaultPos = this.transform.position;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos = eventData.position;
        this.transform.position = currentPos;


    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if(changeParent != null)
        {
            InventoryManager.instance.ChangeCusor(changeParent.gameObject);
            if (changeParent.childCount == 0)
            {
                transform.SetParent(changeParent);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.transform.position = changeParent.transform.position;

                int siblingParentIndex = changeParent.transform.parent.GetSiblingIndex();
                int siblingIndex = changeParent.transform.GetSiblingIndex();
                //InventoryManager.instance.TestDelet(siblingParentIndex, siblingIndex);
                InventoryManager.instance.MoveItem(siblingIndex, siblingParentIndex);
            }
            else
            {
                GameObject changeItem = changeParent.GetChild(0).gameObject;
                ItemCheck itemC = changeItem.GetComponent<ItemCheck>();
                ItemCheck itemB = this.GetComponent<ItemCheck>();
                if (itemC.name == itemB.name && (itemC.nowStack != itemC.maxStack && itemB.nowStack != itemB.maxStack))
                {

                    if (itemC.nowStack + itemB.nowStack <= itemC.maxStack)
                    {
                        Debug.Log("������1");
                        itemC.nowStack += itemB.nowStack;
                        changeItem.transform.SetParent(changeParent);
                        changeItem.transform.position = changeParent.transform.position;
                        Destroy(itemB.gameObject);
                    }
                    else
                    {
                        Debug.Log("������2");
                        itemB.nowStack -= (itemC.maxStack - itemC.nowStack);
                        itemC.nowStack = itemC.maxStack;
                        changeItem.transform.SetParent(changeParent);
                        changeItem.transform.position = changeParent.transform.position;
                        this.transform.SetParent(currentParent);
                        this.transform.position = currentParent.transform.position;
                    }




                }
                else
                {
    
                    changeItem.transform.SetParent(currentParent);
                    changeItem.transform.position = currentParent.transform.position;

                    this.transform.SetParent(changeParent);
                    this.transform.position = changeParent.transform.position;

                }



                InventoryManager.instance.MoveItem(siblingIndex, siblingParentIndex);

                // InventoryManager.instance.ExchangeItem(currentParent.gameObject, changeParent.gameObject);
            }


        }
       else if(changeBox != null && changeBox.transform.GetSiblingIndex() != InventoryManager.instance.nowBox)
        {
            int siblingParentIndex = changeBox.transform.GetSiblingIndex();
            if (InventoryManager.instance.nowBox != siblingParentIndex)
            {
                if (InventoryManager.instance.CheckBoxCanCreat(siblingParentIndex))
                {
                    InventoryManager.instance.CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack);
                    Destroy(this.gameObject);
                }
            }
            InventoryManager.instance.ChangeCusor(currentParent.gameObject);
        }
        else
        {
            transform.SetParent(currentParent);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = currentParent.transform.position;
            InventoryManager.instance.ChangeCusor(currentParent.gameObject);
        }






    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.transform.tag == "Box")
        {
            changeParent = collision.transform;
        }
        if (collision.transform.tag == "UpperBox")
        {
            changeBox = collision.transform;
        }


    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Box")
        {
            changeParent = null;
        }
        if (collision.transform.tag == "UpperBox")
        {
            changeBox = null;
        }
    }


}
