using UnityEngine;

/// <summary>
/// Phase 2 브리지 (InventoryRefactorDesign 4절): 현재 UI 슬롯 GameObject(페이지 → 박스 → ItemCheck)를 읽어 SlotContainer 를 만든다.
/// 진실은 아직 UI 이고, 저장·비교는 이 모델을 거친다.
/// </summary>
public static class ViewScanner
{
    public static SlotContainer BuildFrom(GameObject[] pages, int perPage, IItemCatalog catalog)
    {
        int pageCount = pages != null ? pages.Length : 0;
        var c = new SlotContainer(catalog, pageCount * perPage, pageCount * perPage);
        for (int p = 0; p < pageCount; p++)
        {
            if (pages[p] == null)
            {
                continue;
            }
            Transform page = pages[p].transform;
            int n = Mathf.Min(page.childCount, perPage);
            for (int s = 0; s < n; s++)
            {
                ItemStack st = Read(page.GetChild(s));
                if (!st.IsEmpty)
                {
                    c.PutAt(p * perPage + s, st);
                }
            }
        }
        return c;
    }

    /// <summary>슬롯 박스 하나의 내용. 첫 자식 ItemCheck 만 본다(기존 저장 코드와 동일)</summary>
    public static ItemStack Read(Transform box)
    {
        if (box == null || box.childCount == 0)
        {
            return ItemStack.Empty;
        }
        ItemCheck item = box.GetChild(0).GetComponent<ItemCheck>();
        if (item == null || item.itemData == null)
        {
            return ItemStack.Empty;
        }
        return new ItemStack(item.itemData.name, item.nowStack, item.itemData.tear, item.itemData.upgrade);
    }
}
