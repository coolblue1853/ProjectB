using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler

{
    public static Vector2 DefaultPos;
   public Transform currentParent;
    Transform changeParent;
    Transform changeBox;
   public  Transform equipBox;
    ItemCheck itemCheck;
    int siblingParentIndex;
    int siblingIndex;
    public GameObject dragCanvus;
    private bool _holdingSave; // 드래그 동안 저장을 미루는 중(B-219)


    private void Start()
    {
        itemCheck = this.transform.GetComponent<ItemCheck>();
        dragCanvus = GameObject.FindWithTag("dragCanvas");
        //GetEquipmentBox();
    }
    void GetEquipmentBox()
    {

        GameObject upper = GameObject.FindWithTag("Equipment");
        handBox = upper.transform.GetChild(0).gameObject;
        chestBox = upper.transform.GetChild(1).gameObject;
        shoesBox = upper.transform.GetChild(2).gameObject;
        necklesBox = upper.transform.GetChild(3).gameObject;
        headBox = upper.transform.GetChild(4).gameObject;
        ringBox = upper.transform.GetChild(5).gameObject;
        legBox = upper.transform.GetChild(6).gameObject;
        weaponBox = upper.transform.GetChild(7).gameObject;

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
        if (transform.parent != null && transform.parent.GetComponent<EquipBoxCheck>() != null
            && !InventoryManager.CanUnequip(GetComponent<ItemCheck>()))
        {
            eventData.pointerDrag = null; // 장착한 무기는 끌어서 뺄 수 없다(빈손 방지). 교체는 인벤토리 무기를 끌어다 놓으면 된다
            return;
        }
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
              //  InventoryManager.instance.TestDelet(siblingParentIndex, siblingIndex);
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
        // 끄는 동안 아이템은 슬롯 밖(드래그 캔버스)이라 스캔 저장에 안 잡힌다 → 놓을 때까지 저장을 미룬다(B-219)
        if (!_holdingSave)
        {
            _holdingSave = true;
            InventoryManager.instance.BeginSaveBatch();
        }
    }

    /// <summary>드래그 중 미뤄 둔 저장을 푼다(놓기·비활성 어느 쪽이든 한 번만). flush=false 면 저장 없이 푼다</summary>
    private void ReleaseSaveHold(bool flush = true)
    {
        if (!_holdingSave)
        {
            return;
        }
        _holdingSave = false;
        if (InventoryManager.instance == null)
        {
            return;
        }
        if (flush)
        {
            InventoryManager.instance.EndSaveBatch();
        }
        else
        {
            InventoryManager.instance.AbandonSaveBatch();
        }
    }

    /// <summary>
    /// 드래그 중 파괴·씬 해제·종료: 비활성화 도중에는 부모를 바꿀 수 없으므로 원위치는 하지 않는다.
    /// 씬이 내려가는 중이면 페이지가 이미 없을 수 있어 저장 없이 배치만 푼다(빈 인벤 저장 방지). dragCanvas 는 씬 루트라 창 닫기로는 오지 않는다.
    /// </summary>
    private void OnDisable()
    {
        ReleaseSaveHold(gameObject.scene.isLoaded);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos = eventData.position;
        this.transform.position = currentPos;


    }
    EquipBoxCheck equipBoxCheck;

    /// <summary>
    /// 장비칸에서 시작한 드래그는 빈 인벤 칸에만 놓을 수 있다(B-012/013).
    /// 차 있는 인벤 칸(교환 분기에 부위 검사가 없어 엉뚱한 아이템이 장비칸에 들어감)·창고 칸·페이지 탭(해제 없이 Destroy)은 막는다.
    /// </summary>
    bool DropFromEquipNotAllowed()
    {
        if (currentParent == null || currentParent.GetComponent<EquipBoxCheck>() == null)
        {
            return false;
        }
        if (changeParent != null)
        {
            return !(changeParent.parent.CompareTag("Inventory") && changeParent.childCount == 0);
        }
        // 페이지 탭이거나, 아무 칸도 아닌 곳(장비칸 트리거가 꺼져 있어 equipBox 가 null 이면 드래그 캔버스에 떠 있게 됨)
        return changeBox != null || equipBox == null;
    }

    /// <summary>드래그를 취소하고 시작한 칸으로 되돌린다(장비칸이면 EquipBoxCheck 가 다시 연결하지만 이미 연결돼 있어 중복 적용 없음)</summary>
    void ReturnToOrigin()
    {
        transform.SetParent(currentParent);
        transform.position = currentParent.position;
        equipBox = null;
        changeParent = null;
        changeBox = null;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        try
        {
            if (!TryDropOnQuickSlot(eventData.position))
            {
                EndDrag();
            }
        }
        finally
        {
            ReleaseSaveHold(); // 어느 분기로 끝나도 미뤄 둔 저장을 1회로 푼다
        }
    }

    /// <summary>퀵슬롯 칸 위에 놓으면 등록만 하고 아이템은 원래 칸으로 되돌린다(BasicFeatures.md 3절). 처리했으면 true</summary>
    private bool TryDropOnQuickSlot(Vector2 screenPos)
    {
        if (itemCheck == null || currentParent == null || currentParent.childCount > 0
            || currentParent.parent == null || !currentParent.parent.CompareTag("Inventory"))
        {
            return false; // 인벤 칸에서 시작한 드래그만(창고·장비칸 제외). 원래 칸이 채워졌으면 되돌릴 수 없으니 기존 경로로
        }
        if (!QuickSlotBar.TryAssignFromDrag(screenPos, itemCheck))
        {
            return false;
        }
        ReturnToOrigin();
        return true;
    }

    /// <summary>인벤 칸 → 인벤 칸 놓기는 모델 명령(MoveSlot)으로. 처리했으면 true</summary>
    private bool TryDropInventoryToInventory()
    {
        if (changeParent == null || currentParent == null || currentParent.parent == null
            || !currentParent.parent.CompareTag("Inventory") || !changeParent.parent.CompareTag("Inventory"))
        {
            return false;
        }
        if (currentParent.childCount > 0)
        {
            // 끄는 사이 원래 칸이 다른 아이템으로 채워졌다(획득 등). 되돌리면 한 칸에 둘이 되므로 옛 경로(직접 배치)로 넘긴다
            return false;
        }
        InventoryManager inv = InventoryManager.instance;
        if (inv.chest != null)
        {
            inv.chest.cusor.SetActive(false);
        }
        inv.cusor.SetActive(true);
        Transform target = changeParent;
        ReturnToOrigin(); // 먼저 원래 칸에 돌려놓아 모델이 보게 한 뒤 명령으로 옮긴다(이 오브젝트는 다시 그려지며 사라질 수 있음)
        inv.MoveInInventory(currentParent, target);
        inv.ChangeCusor(target.gameObject);
        return true;
    }

    /// <summary>인벤 칸 → 맞는 장비칸: 키보드 장착과 같은 경로(인벤 쪽은 TakeOut, 교체 장비는 PutIn). 처리했으면 true</summary>
    private bool TryDropInventoryToEquip()
    {
        if (equipBox == null || changeParent != null || changeBox != null || currentParent == null || currentParent.parent == null
            || itemCheck == null || itemCheck.itemData == null)
        {
            return false;
        }
        if (currentParent.parent.CompareTag("Chest"))
        {
            ReturnToOrigin(); // 창고 → 장비칸 직접 장착은 막는다(장비 저장 누락 경로). 인벤으로 옮긴 뒤 장착 — 창고 명령화(Phase 4) 때 다시 연다
            return true;
        }
        if (!currentParent.parent.CompareTag("Inventory") || currentParent.childCount > 0)
        {
            return false;
        }
        InventoryManager inv = InventoryManager.instance;
        GameObject target = inv.EquipBoxFor(itemCheck);
        if (target == null || target.transform != equipBox)
        {
            return false; // 부위가 다르거나 보조 칸 등 — 옛 경로
        }
        ReturnToOrigin(); // 원래 칸에 돌려놓은 뒤 명령으로 꺼낸다
        inv.EquipFromInventory(itemCheck);
        return true;
    }

    /// <summary>장비칸 → 빈 인벤 칸: 인벤 쪽은 PutIn(오브젝트 그대로), 장비 쪽은 해제 처리. 처리했으면 true</summary>
    private bool TryDropEquipToInventory()
    {
        if (changeParent == null || changeParent.parent == null || currentParent == null || !changeParent.parent.CompareTag("Inventory") || changeParent.childCount > 0)
        {
            return false;
        }
        EquipBoxCheck from = currentParent.GetComponent<EquipBoxCheck>();
        if (from == null)
        {
            return false;
        }
        InventoryManager inv = InventoryManager.instance;
        ItemCheck item = GetComponent<ItemCheck>();
        int index = inv.Service.IndexOfBox(changeParent);
        if (!inv.Service.PutIn(index, item))
        {
            ReturnToOrigin(); // 잠긴 칸 등 — 장착 그대로
            return true;
        }
        from.DeletPrefab(item, item.itemData.equipArea);
        // 드래그 해제도 세이브·장착 목록에 반영 (안 하면 재접속 시 장착+인벤토리 양쪽에 생겨 복제됨)
        from.SaveEquipItem(item, false);
        inv.ForgetEquippedItem(item);
        if (inv.chest != null)
        {
            inv.chest.cusor.SetActive(false);
        }
        inv.cusor.SetActive(true);
        inv.ChangeCusor(changeParent.gameObject);
        inv.SaveInventory();
        equipBox = null;
        changeParent = null;
        return true;
    }

    /// <summary>같은 아이템·티어·강화이고 대상이 덜 찼으면 합칠 수 있다(인벤 모델 CanStackWith 와 같은 기준)</summary>
    private static bool CanMerge(ItemCheck b, ItemCheck c)
    {
        return b != null && c != null && b.itemData != null && c.itemData != null
            && b.itemData.name == c.itemData.name && b.itemData.tear == c.itemData.tear && b.itemData.upgrade == c.itemData.upgrade
            && c.nowStack < c.itemData.maxStack;
    }

    /// <summary>
    /// 창고 칸 ↔ 인벤 칸 놓기(3c-3): 인벤 쪽은 모델 명령(TakeOut/PutIn/RemoveAt/SetCount), 창고 쪽은 아직 오브젝트 직접(Phase 4).
    /// 빈칸=이동, 합칠 수 있으면 합치고 넘침은 원래 칸, 아니면 교환. 끝나면 인벤·창고 한 번에 저장. 처리했으면 true
    /// </summary>
    private bool TryDropAcrossContainers()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv.chest == null || changeParent == null || changeParent.parent == null || currentParent == null || currentParent.parent == null)
        {
            return false;
        }
        bool fromInv = currentParent.parent.CompareTag("Inventory"), fromChest = currentParent.parent.CompareTag("Chest");
        bool toInv = changeParent.parent.CompareTag("Inventory"), toChest = changeParent.parent.CompareTag("Chest");
        if (!((fromInv && toChest) || (fromChest && toInv)) || currentParent.childCount > 0)
        {
            return false;
        }
        Transform origin = currentParent, target = changeParent;
        ItemCheck b = itemCheck;
        ReturnToOrigin(); // 먼저 원래 칸으로(모델이 보는 상태와 같게) 돌려놓고 명령으로 옮긴다
        ItemCheck c = target.childCount > 0 ? target.GetChild(0).GetComponent<ItemCheck>() : null;
        InventoryService svc = inv.Service;
        if (c == null)
        {
            if (fromInv)
            {
                svc.TakeOut(b, target);
            }
            else
            {
                svc.PutIn(svc.IndexOfBox(target), b);
            }
        }
        else if (CanMerge(b, c))
        {
            int put = Mathf.Min(b.nowStack, c.itemData.maxStack - c.nowStack);
            if (fromInv)
            {
                if (svc.RemoveAt(b, put))
                {
                    c.nowStack += put;
                }
            }
            else if (svc.SetCount(c, c.nowStack + put))
            {
                b.nowStack -= put;
                if (b.nowStack <= 0)
                {
                    b.transform.SetParent(null, false);
                    Destroy(b.gameObject);
                }
            }
        }
        else if (fromInv)
        {
            int originIndex = svc.IndexOfBox(origin);
            if (svc.TakeOut(b, target) && !svc.PutIn(originIndex, c))
            {
                svc.PutIn(originIndex, b); // 교환 실패 → 되돌림
            }
        }
        else
        {
            int targetIndex = svc.IndexOfBox(target);
            if (svc.TakeOut(c, origin) && !svc.PutIn(targetIndex, b))
            {
                svc.PutIn(targetIndex, c);
            }
        }
        if (toInv)
        {
            inv.chest.cusor.SetActive(false);
            inv.cusor.SetActive(true);
            inv.ChangeCusor(target.gameObject);
        }
        else
        {
            inv.cusor.SetActive(false);
            inv.chest.cusor.SetActive(true);
            inv.chest.ChangeCusor(target.gameObject);
        }
        inv.SaveInventoryAndChest();
        return true;
    }

    private void EndDrag()
    {
        if (DropFromEquipNotAllowed())
        {
            ReturnToOrigin();
            return;
        }
        if (TryDropInventoryToInventory() || TryDropEquipToInventory() || TryDropInventoryToEquip() || TryDropAcrossContainers())
        {
            return;
        }
        if(changeParent != null)
        {

            
            if (changeParent.parent.tag == "Inventory")
            {
                if (InventoryManager.instance.chest != null)
                {
                    InventoryManager.instance.chest.cusor.SetActive(false);
                }
                InventoryManager.instance.cusor.SetActive(true);
                InventoryManager.instance.ChangeCusor(changeParent.gameObject);

                if(currentParent.GetComponent<EquipBoxCheck>() != null)
                {
                    EquipBoxCheck nowEquipBox = currentParent.GetComponent<EquipBoxCheck>();
                    ItemCheck item = this.GetComponent<ItemCheck>();
                    nowEquipBox.DeletPrefab(item,item.itemData.equipArea);
                    // 드래그 해제도 세이브·장착 목록에 반영 (안 하면 재접속 시 장착+인벤토리 양쪽에 생겨 복제됨)
                    nowEquipBox.SaveEquipItem(item, false);
                    InventoryManager.instance.ForgetEquippedItem(item);
                    InventoryManager.instance.SaveInventory();
                }

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

                if (itemB.itemData.equipArea == itemC.itemData.equipArea || equipBox == null)
                {
                    if (itemC.name == itemB.name && (itemC.nowStack != itemC.itemData.maxStack && itemB.nowStack != itemB.itemData.maxStack))
                    {

                        if (itemC.nowStack + itemB.nowStack <= itemC.itemData.maxStack)
                        {
                            itemC.nowStack += itemB.nowStack;
                            changeItem.transform.SetParent(changeParent);
                            changeItem.transform.position = changeParent.transform.position;
                            Destroy(itemB.gameObject);
                        }
                        else
                        {
                            itemB.nowStack -= (itemC.itemData.maxStack - itemC.nowStack);
                            itemC.nowStack = itemC.itemData.maxStack;
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
                else
                {
                    transform.SetParent(equipBox);
                    this.transform.position = equipBox.transform.position;
                }


            }

            equipBox = null;
        }
       else if(changeBox != null )
        {

            Debug.Log("Check0");
            equipBox = null;
            if (changeBox.parent.tag == "Inventory")
            {
                int siblingParentIndex = changeBox.transform.GetSiblingIndex();
                if (InventoryManager.instance.nowBox != siblingParentIndex)
                {
                    AddSource? from = currentParent.parent.CompareTag("Chest") ? AddSource.Chest : (AddSource?)null;
                    if (InventoryManager.instance.CreatItemSelected(itemCheck.itemData.name, siblingParentIndex, itemCheck.nowStack, itemCheck.itemData, from))
                    {
                        InventoryManager.instance.CommitDragToPage(gameObject);
                    }
                    else
                    {
                        // 대상 페이지에 빈칸이 없으면 원래 칸으로 되돌린다(드래그 캔버스에 남지 않게)
                        transform.SetParent(currentParent);
                        transform.position = currentParent.position;
                    }
                }
                else
                {
                    transform.SetParent(currentParent);
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
                    if (InventoryManager.instance.chest.CheckBoxCanCreat(siblingParentIndex)
                        && InventoryManager.instance.chest.CreatItemSelected(itemCheck.itemData.name, siblingParentIndex, itemCheck.nowStack, itemCheck.itemData))
                    {
                        InventoryManager.instance.CommitDragToPage(gameObject);
                    }
                    else
                    {
                        // 대상 창고 페이지에 빈칸이 없으면 원래 칸으로 되돌린다
                        transform.SetParent(currentParent);
                        transform.position = currentParent.position;
                    }
                }
                else
                {
                    transform.SetParent(currentParent);
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
            if (this.itemCheck.itemData.equipArea == equipBoxCheck.equipArea)
            {

                if (equipBox.childCount == 0)
                {
                    transform.SetParent(equipBox);
                    this.transform.position = equipBox.transform.position;
                    currentParent = equipBox;
                    equipBoxCheck.LoadPrefab(itemCheck.name, itemCheck.itemData.equipArea, itemCheck.itemData.tfName);
                }
                else
                {
                    ItemCheck beforeItem = InventoryManager.instance.CheckNowEquipment(itemCheck.itemData.equipArea);
                    // 아이템을 해체한는 부분
                    DetechItem(itemCheck.itemData.equipArea);
                    equipBoxCheck.DeletPrefab(beforeItem, beforeItem.itemData.equipArea);

                    transform.SetParent(equipBox);
                    this.transform.position = equipBox.transform.position;
                    equipBoxCheck.LoadPrefab(itemCheck.name, itemCheck.itemData.equipArea, itemCheck.itemData.tfName);
                    equipBoxCheck.ActivePrefab(itemCheck.itemData.equipArea);
                    equipBoxCheck.SaveEquipItem(itemCheck, true);
                    InventoryManager.instance.CheckNowEquipItem(itemCheck.itemData.equipArea);
                }

            }
            else
            {

                if (currentParent.parent.tag == "Inventory")
                {
                    transform.SetParent(currentParent);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.ChangeCusor(currentParent.gameObject);
                    equipBox = null;
                }
                else if (currentParent.parent.tag == "Chest")
                {
                    transform.SetParent(currentParent);
                    this.transform.position = currentParent.transform.position;
                    InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
                    equipBox = null;
                }
                else
                {
                    transform.SetParent(equipBox);
                    this.transform.position = equipBox.transform.position;

                }
            }
        }
        else
        {

            if (currentParent.parent.tag == "Inventory")
            {
                transform.SetParent(currentParent);
                this.transform.position = currentParent.transform.position;
                InventoryManager.instance.ChangeCusor(currentParent.gameObject);
            }
            else if (currentParent.parent.tag == "Chest")
            {
                transform.SetParent(currentParent);
                this.transform.position = currentParent.transform.position;
                InventoryManager.instance.chest.ChangeCusor(currentParent.gameObject);
            }
            equipBox = null;
        }

    }

     GameObject weaponBox;
     GameObject headBox;
     GameObject chestBox;
    GameObject ringBox;
    GameObject necklesBox;
    GameObject sideWeaponBox;
    GameObject legBox;
    GameObject shoesBox;
    GameObject handBox;

    public void DetechItem(string equipArea)
    {
        if (equipArea == "Weapon")
        {
            if(equipBox.name == "SideWeapon")
            {
                GameObject moveItem = sideWeaponBox.transform.GetChild(0).gameObject;
                moveItem.transform.SetParent(currentParent);
                moveItem.transform.position = currentParent.transform.position;
            }
            else
            {
                GameObject moveItem = weaponBox.transform.GetChild(0).gameObject;
                moveItem.transform.SetParent(currentParent);
                moveItem.transform.position = currentParent.transform.position;

            }

        }
        else if (equipArea == "Head")
        {
            GameObject moveItem = headBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;

        }
        else if (equipArea == "Chest")
        {
            GameObject moveItem = chestBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;
        }
        else if (equipArea == "Hand")
        {
            GameObject moveItem = handBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;
        }
        else if (equipArea == "Necklace")
        {
            GameObject moveItem = necklesBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;
        }
        else if (equipArea == "Ring")
        {
            GameObject moveItem = ringBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(currentParent);
            moveItem.transform.position = currentParent.transform.position;
        }
        else if (equipArea == "Leg")
        {
            GameObject moveItem = legBox.transform.GetChild(0).gameObject;
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
        if (collision.transform.tag == "EquipBox"&& InventoryManager.instance.ShopGameObject.activeSelf == false)
        {/*
            equipBoxCheck = collision.GetComponent<EquipBoxCheck>();
            if (this.itemCheck.equipArea == equipBoxCheck.equipArea)
            {
                equipBox = collision.transform;
                if (equipBox == null)
                {

                }
            }
            */

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

    private void Update()
    {
        
    }
}
