using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 장착한 방어구 → 던파식 장비 레이어(CelEquipmentPlan). 인벤토리 코드는 건드리지 않고 장비 칸(의상 5; 모자·장갑·바지·신발은 v2 에서 폐지 — 몸+옷 한 조각 방식)을
/// 0.5초마다 살펴, 장착 아이템 이름이 바뀌었을 때만 대응표(Resources/CelEquipMap.csv: 아이템 이름,레이어 id)로 레이어를 다시 합성한다.
/// PixelRigInstaller 가 픽셀 리그에 붙인다.
/// </summary>
// [v2 11-11] 외형은 의상 한 벌 + 무기뿐이다: 의상 칸(5)만 합성한다(모자·장갑·신발 칸 합성과 *_glove/*_shoes 임시 규칙, CelComposer.AlignBoots 삭제).
// 기본 동작은 의상, 공격 동작은 무기가 정하며 둘은 독립(모션 소유 원칙).
public class CelEquipSync : MonoBehaviour
{
    private static readonly int[] ArmorBoxes = { 5 };
    /// <summary>빈 칸에 보이는 기본 의상 레이어(옷 한 벌=base_coat_top 은 목 아래 전부). ArmorBoxes 와 같은 순서. 속옷 몸(기반)은 겉으로 드러나지 않는다(11-09 사용자).</summary>
    private static readonly string[] DefaultLayers = { "base_coat_top" };
    private const float CheckInterval = 0.5f;
    private const string MapPath = "CelEquipMap";
    private const string LayerDir = "PixelRig/Equip/";

    /// <summary>
    /// 장비 외형 레이어 켜기. 10-28 사용자 "투구는 나중에, 동작부터 다지자" — 새 셀(기본 칼 3타 등)에 레이어가 맞지 않아
    /// 투구가 떴다 사라지고 흉갑 지우기 마스크가 다리를 지웠다. 마스터 모션이 확정되고 레이어를 다시 만들 때 true 로.
    /// </summary>
    public static bool VisualsEnabled = true; // 11-09: 레이어가 리그 그림(forDS)과 맞을 때만 합성되므로 켠다

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
        string[] names = key.Split('|');
        for (int i = 0; i < ArmorBoxes.Length && i < names.Length; i++)
        {
            string itemName = names[i];
            string id = null;
            if (itemName.Length == 0)
            {
                id = DefaultLayers[i];
            }
            if (id != null || (itemName.Length > 0 && _map.TryGetValue(itemName, out id)))
            {
                CelEquipLayer layer = Resources.Load<CelEquipLayer>(LayerDir + id);
                if (layer == null || !layer.forDS)
                {
                    layer = Resources.Load<CelEquipLayer>(LayerDir + DefaultLayers[i]); // 새 몸용 그림이 없는 장비는 기본 의상으로 대신
                }
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
