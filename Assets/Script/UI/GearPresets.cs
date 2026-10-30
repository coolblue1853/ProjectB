using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 장비 프리셋 실행·입력(BasicFeatures.md 11절): 인벤토리가 열려 있을 때 Shift+7/8/9 = 지금 장비를 프리셋 1~3 에 저장, 7/8/9 = 프리셋 장착.
/// 장착은 가방에서 같은 이름(티어·강화가 같은 것 우선)을 찾아 InventoryManager.EquipFromInventory(키보드 장착과 같은 경로)로 한다.
/// 가방·착용 어디에도 없는 장비 부위는 건너뛰고 알린다. 프리셋에 없는 부위는 그대로 둔다. 저장 ES3 gear.presets(슬롯 파일).
/// </summary>
public class GearPresets : MonoBehaviour
{
    public const string SaveKey = "gear.presets";
    private static readonly Key[] PresetKeys = { Key.Digit7, Key.Digit8, Key.Digit9 };

    private static GearPresets _instance;
    private readonly GearPresetModel _model = new GearPresetModel();

    public static GearPresetModel Model => _instance != null ? _instance._model : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[GearPresets]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<GearPresets>();
    }

    private void Awake()
    {
        try
        {
            _model.LoadFrom(ES3.KeyExists(SaveKey) ? ES3.Load<string[]>(SaveKey) : null);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "장비 프리셋 읽기 실패: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        InventoryManager inv = InventoryManager.instance;
        Keyboard kb = Keyboard.current;
        // 키보드 장착과 같은 조건: 인벤만 열려 있을 때(상점·창고 중엔 아님)
        if (kb == null || inv == null || inv.inventory == null || !inv.inventory.activeSelf || !string.IsNullOrEmpty(inv.state)
            || inv.chest != null || (inv.ShopGameObject != null && inv.ShopGameObject.activeSelf))
        {
            return;
        }
        for (int i = 0; i < PresetKeys.Length; i++)
        {
            if (!kb[PresetKeys[i]].wasPressedThisFrame)
            {
                continue;
            }
            if (kb.shiftKey.isPressed)
            {
                SaveCurrent(i);
            }
            else
            {
                Apply(i, out _);
            }
        }
    }

    /// <summary>지금 착용 장비를 preset 에 저장한다. 저장한 부위 수(실패면 -1)</summary>
    public static int SaveCurrent(int preset)
    {
        InventoryManager inv = InventoryManager.instance;
        if (_instance == null || inv == null)
        {
            return -1;
        }
        var entries = new Dictionary<string, GearPresetModel.Entry>();
        foreach (string area in GearPresetModel.Areas)
        {
            ItemCheck worn = inv.EquippedFor(area);
            if (worn != null && worn.itemData != null)
            {
                entries[area] = new GearPresetModel.Entry { Name = worn.name, Tier = worn.itemData.tear, Upgrade = worn.itemData.upgrade };
            }
        }
        _instance._model.Set(preset, entries);
        try
        {
            ES3.Save(SaveKey, _instance._model.ToSaveArray());
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "장비 프리셋 저장 실패: " + e.Message);
        }
        PickupLog.Report(Loc.F("Preset {0} saved ({1})", preset + 1, entries.Count), 1, "C");
        Sfx.Play(SfxId.UiMove, 0.8f);
        return entries.Count;
    }

    /// <summary>preset 을 장착한다. 바꾼 부위 수(빈 프리셋이면 0). missing = 가방에서 찾지 못한 부위 수</summary>
    public static int Apply(int preset, out int missing)
    {
        missing = 0;
        InventoryManager inv = InventoryManager.instance;
        if (_instance == null || inv == null || _instance._model.IsEmpty(preset))
        {
            PickupLog.Warn(Loc.F("Preset {0} is empty", preset + 1));
            return 0;
        }
        int changed = 0;
        inv.BeginSaveBatch();
        try
        {
            foreach (KeyValuePair<string, GearPresetModel.Entry> kv in _instance._model.Get(preset))
            {
                ItemCheck worn = inv.EquippedFor(kv.Key);
                if (worn != null && Same(worn, kv.Value))
                {
                    continue;
                }
                bool wornSameName = worn != null && worn.name == kv.Value.Name;
                ItemCheck found = FindInBag(inv, kv.Value);
                if (found == null)
                {
                    if (!wornSameName)
                    {
                        missing++; // 이름이 같은 장비(티어·강화만 다름)를 이미 착용 중이면 그대로 두고 경고하지 않는다
                    }
                    continue;
                }
                inv.EquipFromInventory(found);
                changed++;
            }
        }
        finally
        {
            inv.EndSaveBatch();
        }
        inv.DetailOff();
        if (missing > 0)
        {
            PickupLog.Warn(Loc.F("Preset {0}: {1} item(s) not in bag", preset + 1, missing));
        }
        PickupLog.Report(Loc.F("Preset {0} equipped ({1})", preset + 1, changed), 1, "R");
        Sfx.Play(SfxId.UiOpen, 0.7f);
        GameLog.Info(GameLog.Cat.Item, "장비 프리셋 " + (preset + 1) + " 장착: 교체 " + changed + ", 없음 " + missing);
        return changed;
    }

    private static bool Same(ItemCheck item, GearPresetModel.Entry e)
    {
        return item.name == e.Name && item.itemData != null && item.itemData.tear == e.Tier && item.itemData.upgrade == e.Upgrade;
    }

    /// <summary>가방에서 같은 장비(티어·강화까지 같은 것 우선, 없으면 이름만 같은 것)</summary>
    private static ItemCheck FindInBag(InventoryManager inv, GearPresetModel.Entry e)
    {
        ItemCheck byName = null;
        foreach (ItemCheck c in inv.EnumerateInventoryItems())
        {
            if (c.name != e.Name || c.itemData == null || c.itemData.type != "Equip")
            {
                continue;
            }
            if (Same(c, e))
            {
                return c;
            }
            if (byName == null)
            {
                byName = c;
            }
        }
        return byName;
    }
}
