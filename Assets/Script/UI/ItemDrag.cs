using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler

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
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
         currentParent = transform.parent;

        if (currentParent != null)
        {
            // 부모의 부모(Transform) 찾기
            Transform grandParent = currentParent.parent.parent;
             siblingParentIndex = transform.parent.parent.GetSiblingIndex();
             siblingIndex = transform.parent.GetSiblingIndex();

            if (grandParent != null)
            {
                // 부모의 부모의 자식으로 현재 객체 추가
                transform.SetParent(grandParent);


                InventoryManager.instance.TestDelet(siblingParentIndex, siblingIndex);
            }
            else
            {
                Debug.LogWarning("부모의 부모가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("부모가 없습니다.");
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
            if(changeParent.childCount == 0)
            {
                transform.SetParent(changeParent);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.transform.position = changeParent.transform.position;

                int siblingParentIndex = changeParent.transform.parent.GetSiblingIndex();
                int siblingIndex = changeParent.transform.GetSiblingIndex();

                InventoryManager.instance.MoveItem(siblingIndex, siblingParentIndex);
            }
            else
            {
                GameObject changeItem = changeParent.GetChild(0).gameObject;
                changeItem.transform.SetParent(currentParent);
                changeItem.transform.position = currentParent.transform.position;

                this.transform.SetParent(changeParent);
                this.transform.position = changeParent.transform.position;
                InventoryManager.instance.ResetArray(siblingParentIndex, siblingIndex);
            }

        }
        else
        {
            transform.SetParent(currentParent);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = currentParent.transform.position;
        }


        if(changeBox != null)
        {
            int siblingParentIndex = changeBox.transform.GetSiblingIndex();

            if (InventoryManager.instance.nowBox != siblingParentIndex)
            {
                if (InventoryManager.instance.CheckBoxCanCreat(siblingParentIndex))
                {
                    InventoryManager.instance.CreatItemSelected(itemCheck.name, siblingParentIndex);
                    Destroy(this.gameObject);
                }
            }
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
