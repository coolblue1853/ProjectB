using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 장착한 방어구 → 던파식 장비 레이어(CelEquipmentPlan). 인벤토리 코드는 건드리지 않고 장비 칸(모자 2·갑옷 5·장갑 6·바지 8·신발 9)을
/// 0.5초마다 살펴, 장착 아이템 이름이 바뀌었을 때만 대응표(Resources/CelEquipMap.csv: 아이템 이름,레이어 id)로 레이어를 다시 합성한다.
/// PixelRigInstaller 가 픽셀 리그에 붙인다.
/// </summary>
public class CelEquipSync : MonoBehaviour
{
    private static readonly int[] ArmorBoxes = { 2, 5, 6, 8, 9 };
    private const float CheckInterval = 0.5f;
    private const string MapPath = "CelEquipMap";
    private const string LayerDir = "PixelRig/Equip/";

    /// <summary>
    /// 장비 외형 레이어 켜기. 10-28 사용자 "투구는 나중에, 동작부터 다지자" — 새 셀(기본 칼 3타 등)에 레이어가 맞지 않아
    /// 투구가 떴다 사라지고 흉갑 지우기 마스크가 다리를 지웠다. 마스터 모션이 확정되고 레이어를 다시 만들 때 true 로.
    /// </summary>
    public static bool VisualsEnabled = false;

    private static Dictionary<string, string> _map;
    private PixelRig _rig;
    private float _nextCheck;
    private string _lastKey = "";
    private readonly StringBuilder _key = new StringBuilder();
    private readonly List<CelEquipLayer> _layers = new List<CelEquipLayer>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _map = null;
    }

    private void Awake()
    {
        _rig = GetComponent<PixelRig>();
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextCheck || _rig == null || InventoryManager.instance == null)
        {
            return;
        }
        _nextCheck = Time.unscaledTime + CheckInterval;
        if (!VisualsEnabled)
        {
            if (_lastKey != "off")
            {
                _lastKey = "off";
                _rig.SetEquipmentLayers(null); // 맨몸
            }
            return;
        }
        GameObject[] boxes = InventoryManager.instance.equipBox;
        if (boxes == null)
        {
            return;
        }
        _key.Clear();
        foreach (int b in ArmorBoxes)
        {
            if (b < boxes.Length && boxes[b] != null && boxes[b].transform.childCount > 0
                && boxes[b].transform.GetChild(0).TryGetComponent(out ItemCheck item) && item.itemData != null)
            {
                _key.Append(item.itemData.name);
            }
            _key.Append('|');
        }
        string key = _key.ToString();
        if (key == _lastKey)
        {
            return;
        }
        _lastKey = key;
        Apply(key);
    }

    private void Apply(string key)
    {
        LoadMap();
        _layers.Clear();
        foreach (string itemName in key.Split('|'))
        {
            if (itemName.Length > 0 && _map.TryGetValue(itemName, out string id))
            {
                CelEquipLayer layer = Resources.Load<CelEquipLayer>(LayerDir + id);
                if (layer != null)
                {
                    _layers.Add(layer);
                }
            }
        }
        _rig.SetEquipmentLayers(_layers);
    }

    private static void LoadMap()
    {
        if (_map != null)
        {
            return;
        }
        _map = new Dictionary<string, string>();
        TextAsset csv = Resources.Load<TextAsset>(MapPath);
        if (csv == null)
        {
            return;
        }
        foreach (string line in csv.text.Split('\n'))
        {
            string l = line.Trim();
            if (l.Length == 0 || l.StartsWith("#") || l.StartsWith("itemName"))
            {
                continue;
            }
            string[] cols = l.Split(',');
            if (cols.Length >= 2)
            {
                _map[cols[0].Trim()] = cols[1].Trim();
            }
        }
    }
}
