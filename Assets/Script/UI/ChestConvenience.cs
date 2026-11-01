using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class Chest
{
    private bool _withdrawQuantity;
    private TextMeshProUGUI _storageHint;
    private float _nextStorageHint;
    private int _shownUsed = -1;
    private int _shownTotal = -1;
    public bool IsWithdrawQuantity => _withdrawQuantity;

    public int IndexOf(ItemCheck check)
    {
        if (check == null || check.transform.parent == null)
        {
            return -1;
        }
        Transform box = check.transform.parent;
        for (int p = 0; p < inventoryUI.Length; p++)
        {
            if (box.parent == inventoryUI[p].transform)
            {
                return p * 30 + box.GetSiblingIndex();
            }
        }
        return -1;
    }

    public SlotContainer ScanModel()
    {
        SlotContainer model = ViewScanner.BuildFrom(inventoryUI, 30, CsvItemCatalog.Shared);
        model.Unlocked = Mathf.Clamp(maxBoxNum, 0, model.Capacity);
        return model;
    }

    public void ApplyModel(SlotContainer model)
    {
        for (int p = 0; p < inventoryUI.Length; p++)
        {
            int slots = Mathf.Min(30, inventoryUI[p].transform.childCount);
            for (int s = 0; s < slots; s++)
            {
                DrawStorageSlot(inventoryUI[p].transform.GetChild(s), model[p * 30 + s], s, p);
            }
        }
        _nextStorageHint = 0f;
    }

    private void DrawStorageSlot(Transform box, ItemStack stack, int slot, int page)
    {
        ItemStack previous = ViewScanner.Read(box);
        if (previous.Id == stack.Id && previous.Count == stack.Count && previous.Tier == stack.Tier && previous.Upgrade == stack.Upgrade && previous.IsLocked == stack.IsLocked && previous.IsNew == stack.IsNew)
        {
            return;
        }
        // Destroy는 프레임 끝이므로 먼저 떼어 같은 프레임 스캔·저장에 잡히지 않게 한다.
        while (box.childCount > 0)
        {
            GameObject old = box.GetChild(0).gameObject;
            old.transform.SetParent(null, false);
            Destroy(old);
        }
        inventoryArray[slot, page] = stack.IsEmpty ? 0 : 1;
        if (stack.IsEmpty)
        {
            return;
        }
        GameObject go = Instantiate(itemPrefab, box.position, Quaternion.identity, box);
        go.transform.localScale = Vector3.one * 0.8f;
        ItemCheck check = go.GetComponent<ItemCheck>();
        check.SetItem(stack.Id);
        check.itemData.tear = stack.Tier;
        check.itemData.upgrade = stack.Upgrade;
        check.nowStack = stack.Count;
        check.itemData.isLocked = stack.IsLocked;
        check.itemData.isNew = stack.IsNew;
    }

    public int WithdrawSelected(int amount)
    {
        int index = nowBox * 30 + cusorCount[nowBox];
        ItemStack source = ScanModel()[index];
        int moved = InventoryManager.instance.Service.TransferChest(this, index, false, amount);
        FinishTransfer(moved, moved < Mathf.Min(amount, source.Count), false);
        return moved;
    }

    public int DepositSelected(ItemCheck selected)
    {
        if (selected == null)
        {
            return 0;
        }
        InventoryManager inv = InventoryManager.instance;
        int requested = selected.nowStack;
        int moved = inv.Service.TransferChest(this, inv.Service.IndexOf(selected), true, requested);
        FinishTransfer(moved, moved < requested, true);
        return moved;
    }

    /// <summary>null이면 모든 재료, ID가 있으면 같은 재료·소비품 전체 보관. 인스턴스 속성은 각각 보존.</summary>
    public int DepositAll(string id = null)
    {
        InventoryManager inv = InventoryManager.instance;
        SlotContainer before = inv.Service.ScanModel();
        int requested = 0;
        for (int i = 0; i < before.Unlocked; i++)
        {
            ItemStack stack = before[i];
            ItemDef def = stack.IsEmpty ? null : CsvItemCatalog.Shared.Get(stack.Id);
            if (id == null ? def?.Type == "Misc" : stack.Id == id && (def?.Type == "Misc" || def?.Type == "Consum"))
            {
                requested += stack.Count;
            }
        }
        int moved = inv.Service.DepositChest(this, id);
        FinishTransfer(moved, moved < requested, true);
        if (requested == 0)
        {
            PickupLog.Notice(id == null ? "보관할 재료가 없습니다" : "재료·소비품을 선택하세요");
        }
        return moved;
    }

    private void FinishTransfer(int moved, bool full, bool deposit)
    {
        if (moved > 0)
        {
            InventoryManager.instance.SaveInventoryAndChest(this);
            PickupLog.Notice((deposit ? "창고에 보관: " : "창고에서 꺼냄: ") + moved + "개");
        }
        if (full)
        {
            PickupLog.Warn(deposit ? "창고가 가득 찼습니다 — 남은 아이템은 가방에 있습니다" : "가방이 가득 찼습니다 — 남은 아이템은 창고에 있습니다");
        }
    }

    public void SortStorage()
    {
        SlotContainer model = ScanModel();
        model.Sort(new ChestItemComparer(CsvItemCatalog.Shared));
        ApplyModel(model);
        InventoryManager.instance.SaveInventoryAndChest(this);
        Sfx.Play(SfxId.UiMove);
    }

    private void UpdateConvenience()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.chest != this || !chestOb.activeInHierarchy)
        {
            return;
        }
        RefreshStorageHint();
        bool chestFocus = isCusorChest && inv.state == "InChestMove";
        bool ready = chestFocus ? state == "" || state == "C2IMove" : inv.state == "chestOpen" || inv.state == "I2CMove";
        if (!ready || inv.IsSaveBatchActive)
        {
            return;
        }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (chestFocus && ((kb != null && kb.rKey.wasPressedThisFrame) || (pad != null && pad.rightStickButton.wasPressedThisFrame)))
        {
            SortStorage();
        }
        else if ((kb != null && kb.tKey.wasPressedThisFrame) || (pad != null && pad.selectButton.wasPressedThisFrame))
        {
            DepositAll();
        }
        else if (!chestFocus && ((kb != null && kb.gKey.wasPressedThisFrame) || (pad != null && pad.rightTrigger.wasPressedThisFrame)))
        {
            ItemCheck selected = inv.SelectedInventoryItem;
            if (selected != null)
            {
                DepositAll(selected.itemData.name);
            }
        }
    }

    private void RefreshStorageHint()
    {
        if (Time.unscaledTime < _nextStorageHint)
        {
            return;
        }
        _nextStorageHint = Time.unscaledTime + 0.25f;
        if (_storageHint == null)
        {
            BuildStorageHint();
        }
        int used = 0, total = 0;
        for (int p = 0; p < inventoryUI.Length; p++)
        {
            int count = Mathf.Min(30, Mathf.Min(inventoryUI[p].transform.childCount, maxBoxNum - p * 30));
            for (int s = 0; s < count; s++)
            {
                total++;
                if (inventoryUI[p].transform.GetChild(s).childCount > 0)
                {
                    used++;
                }
            }
        }
        if (used != _shownUsed || total != _shownTotal)
        {
            _shownUsed = used;
            _shownTotal = total;
            _storageHint.text = "창고 " + used + " / " + total + "칸\nG/RT: 같은 종류   T/SELECT: 재료 전부\nR/R3: 정렬   꺼내기 모드 C/Y: 수량";
        }
    }

    private void BuildStorageHint()
    {
        GameObject go = new GameObject("ChestCapacityHint", typeof(RectTransform));
        // 씬의 창고 루트는 100×100 배치용 앵커다. 실제 배경 사각형 안쪽에 붙인다.
        Transform anchor = chestOb.transform;
        float largest = 0f;
        foreach (Transform child in chestOb.transform)
        {
            RectTransform candidate = child as RectTransform;
            if (candidate != null && child.GetComponent<UnityEngine.UI.Image>() != null && candidate.rect.width * candidate.rect.height > largest)
            {
                anchor = child;
                largest = candidate.rect.width * candidate.rect.height;
            }
        }
        go.transform.SetParent(anchor, false);
        _storageHint = go.AddComponent<TextMeshProUGUI>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            _storageHint.font = theme.pixelFont;
        }
        _storageHint.fontSize = 16;
        _storageHint.alignment = TextAlignmentOptions.BottomLeft;
        _storageHint.raycastTarget = false;
        RectTransform rect = _storageHint.rectTransform;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = new Vector2(-24, 66);
        ReserveStorageFooter();
    }

    private void ReserveStorageFooter()
    {
        // 마지막 슬롯 줄과 안내가 겹치지 않게 상단 위치를 유지하며 전체 페이지를 줄인다.
        const float scale = 0.84f;
        foreach (GameObject page in inventoryUI)
        {
            RectTransform pageRect = page.GetComponent<RectTransform>();
            pageRect.anchoredPosition += Vector2.up * pageRect.rect.height * (1f - scale) * (1f - pageRect.pivot.y);
            pageRect.localScale *= scale;
        }
        cusor.transform.localScale *= scale;
        changeCusor.transform.localScale *= scale;
        cusor.transform.position = inventoryUI[nowBox].transform.GetChild(cusorCount[nowBox]).position;
    }
}
