using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가방 장비 점검(읽기 전용): 같은 부위 착용품과 점수를 견줘 장착 후보·판매 후보를 만든다.
/// 점수: 무기 = 평균 피해(강화 반영), 방어구·장신구 = (기본 공격 + 방어) × 강화 배율. 프리팹이 없으면 -1(모름 → 장착도 판매도 안 한다).
/// 장착은 InventoryManager.SetEquipBox 가 다루는 부위(무기·의상·반지·목걸이)만. 규칙은 BotErrandRules(순수).
/// </summary>
public sealed class BotGear
{
    public struct Plan
    {
        public int Index;
        public string Id;
    }

    private readonly List<Plan> _equip = new List<Plan>(4);
    private readonly List<Plan> _sell = new List<Plan>(8);
    private readonly Dictionary<string, float> _scoreCache = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _bestScore = new Dictionary<string, float>(4);

    public const float BuildInterval = 0.3f;
    private float _nextBuild;

    public List<Plan> Equip => _equip;
    public List<Plan> Sell => _sell;
    /// <summary>직전 계획에 있던 칸이 이번엔 비었거나 다른 아이템 = 장착·판매가 일어났다</summary>
    public int EquipDone { get; private set; }
    public int SellDone { get; private set; }

    public void ResetCounts()
    {
        EquipDone = 0;
        SellDone = 0;
        _nextBuild = 0f;
        _equip.Clear();
        _sell.Clear();
    }

    /// <summary>가방을 다시 훑어 계획을 새로 만든다(직전 계획 칸의 변화는 장착·판매 수로 센다)</summary>
    public void Build()
    {
        if (Time.unscaledTime < _nextBuild)
        {
            return; // 매 틱(10Hz) 가방 전체를 훑지 않는다
        }
        _nextBuild = Time.unscaledTime + BuildInterval;
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.Service == null)
        {
            return;
        }
        SlotContainer m = inv.Service.ScanModel();
        int eg = CountGone(_equip, m);
        int sg = CountGone(_sell, m);
        if (eg + sg > 0)
        {
            Debug.Log("[PB][Bot] 업무 결과 장착+" + eg + " 판매+" + sg);
        }
        EquipDone += eg;
        SellDone += sg;
        _equip.Clear();
        _sell.Clear();
        _bestScore.Clear();
        for (int i = 0; i < m.Unlocked && i < m.Capacity; i++)
        {
            ItemStack st = m[i];
            ItemDef def = st.IsEmpty ? null : CsvItemCatalog.Shared.Get(st.Id);
            if (def == null || def.Type != "Equip" || string.IsNullOrEmpty(def.EquipArea))
            {
                continue;
            }
            string area = def.EquipArea;
            bool areaOk = BotErrandRules.IsEquippableArea(area);
            ItemCheck worn = areaOk ? inv.EquippedFor(area) : null;
            bool empty = worn == null;
            float wornScore = empty ? 0f : Score(worn.name, area, worn.itemData != null ? worn.itemData.upgrade : 0);
            float score = def.Retired || !areaOk ? -1f : Score(st.Id, area, st.Upgrade);
            if (areaOk && !def.Retired && BotErrandRules.IsUpgrade(score, wornScore, empty))
            {
                float cur;
                if (!_bestScore.TryGetValue(area, out cur) || score > cur)
                {
                    _bestScore[area] = score;
                    SetEquip(area, i, st.Id);
                }
                continue;
            }
            if (ItemTradeRules.CanSell(st.Id) && BotErrandRules.ShouldSell(st.IsLocked, def.Retired, areaOk, score, wornScore, empty))
            {
                _sell.Add(new Plan { Index = i, Id = st.Id });
            }
        }
    }

    private static int CountGone(List<Plan> prev, SlotContainer m)
    {
        int n = 0;
        for (int i = 0; i < prev.Count; i++)
        {
            ItemStack now = m[prev[i].Index];
            if (now.IsEmpty || now.Id != prev[i].Id)
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>부위마다 가장 좋은 후보 하나만 둔다(교체하면 다시 계획을 짠다)</summary>
    private void SetEquip(string area, int index, string id)
    {
        for (int k = 0; k < _equip.Count; k++)
        {
            ItemDef d = CsvItemCatalog.Shared.Get(_equip[k].Id);
            if (d != null && d.EquipArea == area)
            {
                _equip[k] = new Plan { Index = index, Id = id };
                return;
            }
        }
        _equip.Add(new Plan { Index = index, Id = id });
    }

    private float Score(string id, string area, int upgrade)
    {
        string key = area + "/" + id + "/" + upgrade;
        float s;
        if (_scoreCache.TryGetValue(key, out s))
        {
            return s;
        }
        s = -1f;
        GameObject prefab = GearPrefabCache.Get(area, id);
        if (area == "Weapon")
        {
            Weapon w = GearPrefabCache.Comp<Weapon>(prefab);
            if (w != null)
            {
                int min, max;
                TooltipBuilder.WeaponDamage(w, upgrade, out min, out max);
                s = (min + max) * 0.5f;
            }
        }
        else
        {
            Equipment e = GearPrefabCache.Comp<Equipment>(prefab);
            if (e != null)
            {
                s = (e.basicDmg + e.armor) * EnhanceRules.GearMultiplier(upgrade);
            }
        }
        _scoreCache[key] = s;
        return s;
    }
}
