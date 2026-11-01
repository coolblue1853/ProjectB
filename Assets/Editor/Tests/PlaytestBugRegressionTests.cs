using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class PlaytestBugRegressionTests
{
    private GameObject _root;
    private PlayerHealthManager _health;
    private PlayerHealthManager _previousHealth;
    private SaveManager _previousSave;
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    [SetUp]
    public void SetUp()
    {
        _root = new GameObject("BugRegression");
        _root.SetActive(false);
        _previousHealth = PlayerHealthManager.Instance;
        _previousSave = SaveManager.instance;
        _health = _root.AddComponent<PlayerHealthManager>();
        typeof(PlayerHealthManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _health);
        _health.fullHP = _health.nowHp = 100;
        _health.fullStemina = 100;
        _health.fullFullness = 100;
        _health.steminaHealPoint = 10;
        _health.intervalTimeSteminaHeal = 1f;
        _health.intervalTimeHpHeal = 1f;
        SaveManager.instance = _root.AddComponent<SaveManager>();
        SaveManager.instance.datas = new Datas();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (string field in new[] { "sequence", "waitSequence", "hpSequence", "waitHpSequence" })
        {
            ((Tween)typeof(PlayerHealthManager).GetField(field, Private).GetValue(_health)).Kill();
        }
        Object.DestroyImmediate(_root);
        typeof(PlayerHealthManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _previousHealth);
        SaveManager.instance = _previousSave;
    }

    [Test]
    public void FoodRestartsStarvedRecovery_WithoutDuplicateTimers()
    {
        _health.nowStemina = 0;
        _health.nowFullness = 0;
        typeof(PlayerHealthManager).GetMethod("OnSequenceComplete", Private).Invoke(_health, null);
        Assert.AreEqual(0, _health.nowStemina);
        _health.FullnessUp(20);
        Tween timer = (Tween)typeof(PlayerHealthManager).GetField("waitSequence", Private).GetValue(_health);
        Assert.IsTrue(timer.IsActive());
        _health.FullnessUp(5);
        Assert.AreSame(timer, typeof(PlayerHealthManager).GetField("waitSequence", Private).GetValue(_health));
        timer.Complete(true);
        Assert.AreEqual(10, _health.nowStemina);
        Assert.AreEqual(24, _health.nowFullness);
    }

    [Test]
    public void FoodDoesNotSkipStaminaConsumptionDelay()
    {
        _health.waitTimeSteminaHeal = 5f;
        _health.nowStemina = 100;
        _health.SteminaDown(100);
        _health.FullnessUp(20);
        Assert.IsNull(typeof(PlayerHealthManager).GetField("waitSequence", Private).GetValue(_health));
        Assert.AreEqual(0, _health.nowStemina);
    }

    [Test]
    public void FullStaminaDoesNotSpendFullnessOnRecoveryTick()
    {
        _health.nowStemina = 100;
        _health.nowFullness = 20;
        typeof(PlayerHealthManager).GetMethod("OnSequenceComplete", Private).Invoke(_health, null);
        Assert.AreEqual(20, _health.nowFullness);
    }

    [Test]
    public void BackClosesOnlyTopOwner_AndDoesNotFallThroughToPause()
    {
        var frame = new UiBackFrame();
        frame.Begin(1, "InvGroup");
        Assert.IsFalse(frame.TryConsume(null));
        Assert.IsTrue(frame.TryConsume("InvGroup"));
        frame.Begin(1, null); // 최상위 창을 닫아도 같은 프레임 대상은 고정
        Assert.IsFalse(frame.TryConsume(null));
        Assert.IsFalse(frame.TryConsume("InvGroup"));
        frame.Begin(2, null);
        Assert.IsTrue(frame.TryConsume(null));
    }

    [Test]
    public void BackNestedOwnerConsumesBeforeUnderlyingWindow()
    {
        var frame = new UiBackFrame();
        frame.Begin(10, "KeyBind");
        Assert.IsFalse(frame.TryConsume("Pause"));
        Assert.IsTrue(frame.TryConsume("KeyBind"));
        Assert.IsFalse(frame.TryConsume("Pause"));
        frame.Begin(11, "Pause");
        Assert.IsTrue(frame.TryConsume("Pause"));
    }

    [Test]
    public void EquippedWeaponRefreshUpdatesDamageAndSaveImmediately()
    {
        var box = _root.AddComponent<EquipBoxCheck>();
        var weapon = _root.AddComponent<Weapon>();
        weapon.minDmg = 100;
        weapon.maxDmg = 200;
        typeof(Weapon).GetField("phm", Private).SetValue(weapon, _health);
        typeof(EquipBoxCheck).GetField("weapon", Private).SetValue(box, weapon);
        box.equipPrefab = _root;
        var itemObject = new GameObject("EquippedSword");
        itemObject.transform.SetParent(_root.transform);
        var item = itemObject.AddComponent<ItemCheck>();
        item.itemData = new ItemData { name = "TestSword", type = "Equip", equipArea = "Weapon", upgrade = 1 };
        box.RefreshEnhancement(item);
        Assert.AreEqual(104, weapon.damgeArray[0]);
        Assert.AreEqual(208, weapon.damgeArray[1]);
        Assert.AreEqual("1", SaveManager.instance.datas.equipGear[7, 2]);
        item.itemData.upgrade = 0;
        box.RefreshEnhancement(item);
        Assert.AreEqual(100, weapon.damgeArray[0]);
        Assert.AreEqual(200, weapon.damgeArray[1]);
        Assert.AreEqual("0", SaveManager.instance.datas.equipGear[7, 2]);
    }

    [Test]
    public void EquippedArmorRefreshUpdatesStatsAndSave_WithoutStackingSetOrHealing()
    {
        var box = _root.AddComponent<EquipBoxCheck>();
        var gear = _root.AddComponent<Equipment>();
        gear.hp = 100;
        gear.basicDmg = 100;
        gear.armor = 100;
        gear.appliedHp = 100;
        var list = (Equipment[])typeof(EquipBoxCheck).GetField("equipmentList", Private).GetValue(box);
        list[2] = gear;
        box.equipPrefab = _root;
        var itemObject = new GameObject("EquippedHat");
        itemObject.transform.SetParent(_root.transform);
        var item = itemObject.AddComponent<ItemCheck>();
        item.itemData = new ItemData { name = "TestHat", type = "Equip", equipArea = "Head", upgrade = 1 };
        object setSource = new object();
        int before = PlayerStats.GetInt(StatId.Def);
        PlayerStats.Add(new StatModifier(StatId.Def, ModType.Add, 10) { Source = setSource });
        try
        {
            box.RefreshEnhancement(item);
            Assert.AreEqual(105, gear.appliedHp);
            Assert.AreEqual(105, _health.fullHP);
            Assert.AreEqual(100, _health.nowHp);
            Assert.AreEqual(before + 10 + Mathf.RoundToInt(105 * Equipment.ArmorToDef), PlayerStats.GetInt(StatId.Def));
            box.RefreshEnhancement(item);
            Assert.AreEqual(105, _health.fullHP);
            Assert.AreEqual("1", SaveManager.instance.datas.equipGear[2, 2]);
            Assert.AreEqual(before + 10 + Mathf.RoundToInt(105 * Equipment.ArmorToDef), PlayerStats.GetInt(StatId.Def));
            item.itemData.upgrade = 0;
            box.RefreshEnhancement(item);
            Assert.AreEqual(100, _health.fullHP);
            Assert.AreEqual("0", SaveManager.instance.datas.equipGear[2, 2]);
        }
        finally
        {
            PlayerStats.RemoveBySource(gear);
            PlayerStats.RemoveBySource(setSource);
        }
    }
}
