using System;
using NUnit.Framework;

/// <summary>ReviewCode_1105 필수 1·2, 권장 5·6: ExtraEquip 로드 중 폐기 반환·읽기 실패 보호·가방 가득 보상</summary>
public class ExtraEquipLoadTests
{
    string[] _saved;
    int _savedMask;
    int _saves;
    int _money;

    [SetUp]
    public void SetUp()
    {
        _saved = null;
        _savedMask = -1;
        _saves = 0;
        _money = DatabaseManager.money;
        ExtraEquip.SaveSink = (ids, mask) => { _saved = ids; _savedMask = mask; _saves++; };
    }

    [TearDown]
    public void TearDown()
    {
        DatabaseManager.money = _money;
        ExtraEquip.ResetForTests();
    }

    static ItemData Make(string name, string area, int price = 100)
    {
        return new ItemData { name = name, equipArea = area, price = price };
    }

    static Func<ItemStack, ItemData> Resolver(params ItemData[] all)
    {
        return st =>
        {
            foreach (ItemData d in all)
            {
                if (d.name == st.Id)
                {
                    return d;
                }
            }
            return null;
        };
    }

    [Test]
    public void DeprecatedItem_ReturnedOnce_AndSlotClearedAndSaved()
    {
        int returned = 0;
        ExtraEquip.BagReturn = d => { returned++; return true; };
        int mask = SlotRules.AllExtraMask;
        var ids = new[] { "OldTitle", "", "", "" };
        ExtraEquip.LoadCore(mask, ids, Resolver(Make("OldTitle", "Etc")));
        Assert.AreEqual(1, returned);
        Assert.AreEqual(1, _saves, "반환 직후 저장해야 재로드 때 또 지급되지 않는다");
        Assert.AreEqual("", _saved[0]);
        Assert.AreEqual(mask, _savedMask);
        Assert.IsNull(ExtraEquip.Get(EquipSlot.Earring));

        // 저장된 값으로 다시 로드해도 재지급 없음
        ExtraEquip.LoadCore(_savedMask, _saved, Resolver(Make("OldTitle", "Etc")));
        Assert.AreEqual(1, returned);
        Assert.AreEqual(1, _saves);
    }

    [Test]
    public void DeprecatedItem_BagFull_PaysGold_AndStillSaves()
    {
        ExtraEquip.BagReturn = d => false;
        DatabaseManager.money = 0;
        ExtraEquip.LoadCore(SlotRules.AllExtraMask, new[] { "OldTitle", "", "", "" }, Resolver(Make("OldTitle", "Etc", 100)));
        Assert.AreEqual(50, DatabaseManager.money);
        Assert.AreEqual(1, _saves);
    }

    [Test]
    public void ValidItem_NoSaveOnLoad()
    {
        ExtraEquip.LoadCore(SlotRules.AllExtraMask, new[] { "Ring", "", "", "" }, Resolver(Make("Ring", "Earring")));
        Assert.AreEqual(0, _saves);
        Assert.IsNotNull(ExtraEquip.Get(EquipSlot.Earring));
    }

    [Test]
    public void ResolveThrows_KeepsMask_AndBlocksSave()
    {
        int mask = SlotRules.Bit(EquipSlot.Earring);
        ExtraEquip.LoadCore(mask, new[] { "Ring", "", "", "" }, st => throw new InvalidOperationException("boom"));
        Assert.IsTrue(ExtraEquip.SaveBlocked);
        Assert.IsTrue(SlotRules.IsUnlocked(mask, EquipSlot.Earring));
        Assert.IsTrue(ExtraEquip.IsUnlocked(EquipSlot.Earring), "해금 mask 를 0 으로 덮지 않는다");
        ExtraEquip.Unlock(EquipSlot.Stone);
        Assert.AreEqual(0, _saves, "읽기 실패 후에는 저장하지 않는다");
    }
}
