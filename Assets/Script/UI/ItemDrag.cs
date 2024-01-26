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
    Transform equipBox;
    ItemCheck itemCheck;
    int siblingParentIndex;
    int siblingIndex;
    public GameObject dragCanvus;


    private void Start()
    {
        itemCheck = this.transform.GetComponent<ItemCheck>();
        dragCanvus = GameObject.FindWithTag("dragCanvas");
        GetEquipmentBox();
    }
    void GetEquipmentBox()
    {

        GameObject upper = GameObject.FindWithTag("Equipment");
        weaponBox = upper.transform.GetChild(0).gameObject;
        headBox = upper.transform.GetChild(1).gameObject;
        ChestBox = upper.transform.GetChild(2).gameObject;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log(this.name); // 나누기창 띄우기.
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(this.gameObject.transform.parent.parent != null)
            {
                if (this.gameObject.transform.parent.parent.gameObject.transform.tag == "Inventory")
                {
                    InventoryManager.instance.ChangeCusor(this.gameObject.transform.parent.gameObject);
                }
                else if (this.gameObject.transform.parent.parent.gameObject.transform.tag == "Chest")
                {
                    InventoryManager.instance.chest.ChangeCusor(this.gameObject.transform.parent.gameObject);
                }
            }


        }

    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if(InventoryManager.instance.chest != null && this.gameObject.transform.parent.parent.gameObject.transform.tag == "Chest")
        {
            InventoryManager.instance.chest.DragReset();
        }
        InventoryManager.instance.DragReset();
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
                transform.SetParent(dragCanvus.transform);
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
    EquipBoxCheck equipBoxCheck;
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if(changeParent != null)
        {
            equipBox = null;
            if (changeParent.parent.tag == "Inventory")
            {
               if(InventoryManager.instance.chest != null)
                {
                    InventoryManager.instance.chest.cusor.SetActive(false);
                }
                InventoryManager.instance.cusor.SetActive(true);
                InventoryManager.instance.ChangeCusor(changeParent.gameObject);
            }
            else if (changeParent.parent.tag == "Chest")
            {
                InventoryManager.instance.cusor.SetActive(false);
                InventoryManager.instance.chest.cusor.SetActive(true);
                InventoryManager.instance.chest.ChangeCusor(changeParent.gameObject);

            }

            if (changeParent.childCount == 0)
            {
                transform.SetParent(changeParent);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.transform.position = changeParent.transform.position;

                int siblingParentIndex = changeParent.transform.parent.GetSiblingIndex();
                int siblingIndex = changeParent.transform.GetSiblingIndex();
                //InventoryManager.instance.TestDelet(siblingParentIndex, siblingIndex);
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
                        itemC.nowStack += itemB.nowStack;
                        changeItem.transform.SetParent(changeParent);
                        changeItem.transform.position = changeParent.transform.position;
                        Destroy(itemB.gameObject);
                    }
                    else
                    {
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
            }


        }
       else if(changeBox != null )
        {
            equipBox = null;
            if (changeBox.parent.tag == "Inventory")
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
                else
                {
                    transform.SetParent(currentParent);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.ChangeCusor(currentParent.gameObject);
                }
                if (currentParent.parent.tag == "Inventory")
                {
                    InventoryManager.instance.ChangeCusor(currentParent.gameObject);
                }
                else if (currentParent.parent.tag == "Chest")
                {
                    InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
                }

            }
            else if (changeBox.parent.tag == "Chest")
            {
                int siblingParentIndex = changeBox.transform.GetSiblingIndex();
                if (InventoryManager.instance.chest.nowBox != siblingParentIndex)
                {
                    if (InventoryManager.instance.chest.CheckBoxCanCreat(siblingParentIndex))
                    {
                        InventoryManager.instance.chest.CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack);
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    transform.SetParent(currentParent);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
                }

                if (currentParent.parent.tag == "Inventory")
                {
                    InventoryManager.instance.ChangeCusor(currentParent.gameObject);
                }
                else if (currentParent.parent.tag == "Chest")
                {
                    InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
                }
            }
        }
        else if(equipBox != null )
        {

            equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();
            if(this.itemCheck.equipArea == equipBoxCheck.equipArea)
            {
                Debug.Log("Check0");
                if (equipBox.childCount == 0)
                {
                    Debug.Log("Check1");
                    transform.SetParent(equipBox);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = equipBox.transform.position;
                    equipBoxCheck.LoadPrefab(itemCheck.name, itemCheck.equipArea);
                }
                else
                {
                    Debug.Log("Check2");
                    // 아이템을 해체한는 부분
                    DetechItem(itemCheck.equipArea);
                    equipBoxCheck.DeletPrefab(itemCheck.equipArea);

                    transform.SetParent(equipBox);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = equipBox.transform.position;
                    equipBoxCheck.LoadPrefab(itemCheck.name, itemCheck.equipArea);
                    equipBoxCheck.ActivePrefab(itemCheck.equipArea);
                }

            }
            else
            {
                Debug.Log("Check3");
                if (currentParent.parent.tag == "Inventory")
                {
                    transform.SetParent(currentParent);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.ChangeCusor(currentParent.gameObject);
                }
                else if (currentParent.parent.tag == "Chest")
                {
                    transform.SetParent(currentParent);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
                }
                else
                {
                    transform.SetParent(equipBox);
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    this.transform.position = equipBox.transform.position;

                }
            }
        }
        else
        {
            Debug.Log("Check");
            if (currentParent.parent.tag == "Inventory")
            {
                transform.SetParent(currentParent);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.transform.position = currentParent.transform.position;
                InventoryManager.instance.ChangeCusor(currentParent.gameObject);
            }
            else if (currentParent.parent.tag == "Chest")
            {
                transform.SetParent(currentParent);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.transform.position = currentParent.transform.position;
                InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
            }

        }

    }

     GameObject weaponBox;
     GameObject headBox;
     GameObject ChestBox;

    public void DetechItem(string equipArea)
    {
        if (equipArea == "Weapon")
        {
            GameObject moveItem = weaponBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
        }
        else if (equipArea == "Head")
        {
            GameObject moveItem = headBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
        }
        else if (equipArea == "Chest")
        {
            GameObject moveItem = ChestBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;
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
        if (collision.transform.tag == "EquipBox")
        {
            if(equipBox == null)
            {
                equipBox = collision.transform;
            }



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
        if (collision.transform.tag == "EquipBox")
        {
           // equipBox = null;
        }
    }


}
