using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 진행 맥락 관측(읽기 전용, 봇 재사용, 1초마다): 활성 퀘스트 진행도·가방 빈칸·퀵슬롯 소비 아이템 수(포션)·다음 레벨까지 EXP%·장비·낮/밤·버프.
/// 값이 바뀌었는지는 Signature 로 본다(바뀔 때만 k:"cx" 를 쓴다). 게임 상태를 바꾸지 않는다.
/// </summary>
public sealed class PlayObsContext
{
    public const int MaxQuests = 6;
    public const int MaxBuffs = 6;

    public readonly string[] QuestIds = new string[MaxQuests];
    public readonly int[] QuestProg = new int[MaxQuests];
    public readonly int[] QuestNeed = new int[MaxQuests];
    public int QuestCount;
    /// <summary>가방 빈칸 수(-1 = 가방 없음)</summary>
    public int BagFree = -1;
    /// <summary>퀵슬롯 4칸에 등록된 소비 아이템의 보유 수 합(-1 = 퀵슬롯 없음)</summary>
    public int Potions = -1;
    public int ExpPercent;
    public int Level;
    public int Gold;
    /// <summary>장비 칸(InventoryManager.equipItemCheck 순서)의 아이템 id, 빈칸 null</summary>
    public readonly string[] Equip = new string[10];
    /// <summary>1 밤, 0 낮, -1 시간 시스템 없음</summary>
    public int Night = -1;
    public readonly string[] Buffs = new string[MaxBuffs];
    public int BuffCount;
    public int Signature;

    private readonly List<PlayerBuff> _buffScan = new List<PlayerBuff>(8);

    public void Observe()
    {
        Level = LevelSystem.Level;
        Gold = DatabaseManager.money;
        int need = LevelRules.ExpToNext(Level);
        ExpPercent = need > 0 ? Mathf.Clamp(Mathf.FloorToInt(LevelSystem.Exp * 100f / need), 0, 100) : 0;
        ObserveQuests();
        ObserveBag();
        TimeChange tc = TimeChange.instance;
        Night = tc == null ? -1 : (tc.isDaytime ? 0 : 1);
        ObserveBuffs();
        Signature = ComputeSignature();
    }

    private void ObserveQuests()
    {
        QuestCount = 0;
        if (!NpcQuestLog.Loaded)
        {
            return;
        }
        List<NpcQuestDef> active = NpcQuestLog.ActiveQuests(); // 1초에 한 번이라 목록 할당은 허용
        for (int i = 0; i < active.Count && QuestCount < MaxQuests; i++)
        {
            NpcQuestDef d = active[i];
            QuestIds[QuestCount] = d.Id;
            QuestProg[QuestCount] = NpcQuestLog.Progress(d);
            QuestNeed[QuestCount] = d.Count;
            QuestCount++;
        }
    }

    private void ObserveBag()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.inventoryArray == null)
        {
            BagFree = -1;
            Potions = -1;
            for (int i = 0; i < Equip.Length; i++)
            {
                Equip[i] = null;
            }
            return;
        }
        int per = inv.maxHor * inv.maxVer;
        int free = 0;
        int pages = inv.inventoryArray.GetLength(1);
        int slots = inv.inventoryArray.GetLength(0);
        for (int j = 0; j < pages; j++)
        {
            for (int i = 0; i < per && i < slots; i++)
            {
                if (inv.maxBoxNum > j * per + i && inv.inventoryArray[i, j] == 0)
                {
                    free++;
                }
            }
        }
        BagFree = free;
        int potions = 0;
        for (int s = 0; s < QuickSlotModel.SlotCount; s++)
        {
            string id = QuickSlotBar.SlotItem(s);
            if (id != null)
            {
                potions += inv.CountItem(id);
            }
        }
        Potions = potions;
        for (int i = 0; i < Equip.Length; i++)
        {
            ItemCheck it = i < inv.equipItemCheck.Length ? inv.equipItemCheck[i] : null;
            Equip[i] = it != null ? it.name : null;
        }
    }

    private void ObserveBuffs()
    {
        BuffCount = 0;
        if (FoodBuff.Kind != FoodBuffKind.None)
        {
            Buffs[BuffCount++] = "food:" + FoodBuff.Kind;
        }
        _buffScan.Clear();
        _buffScan.AddRange(Object.FindObjectsByType<PlayerBuff>());
        for (int i = 0; i < _buffScan.Count && BuffCount < MaxBuffs; i++)
        {
            PlayerBuff b = _buffScan[i];
            if (b != null && b.isActiveAndEnabled && !string.IsNullOrEmpty(b.buffType))
            {
                Buffs[BuffCount++] = b.buffType;
            }
        }
        _buffScan.Clear();
    }

    private int ComputeSignature()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + Level;
            h = h * 31 + ExpPercent;
            h = h * 31 + BagFree;
            h = h * 31 + Potions;
            h = h * 31 + Night;
            h = h * 31 + Gold;
            for (int i = 0; i < QuestCount; i++)
            {
                h = h * 31 + (QuestIds[i] != null ? QuestIds[i].GetHashCode() : 0);
                h = h * 31 + QuestProg[i];
            }
            h = h * 31 + QuestCount;
            for (int i = 0; i < Equip.Length; i++)
            {
                h = h * 31 + (Equip[i] != null ? Equip[i].GetHashCode() : 0);
            }
            for (int i = 0; i < BuffCount; i++)
            {
                h = h * 31 + Buffs[i].GetHashCode();
            }
            return h * 31 + BuffCount;
        }
    }
}
