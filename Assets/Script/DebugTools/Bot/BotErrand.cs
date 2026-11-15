using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 봇 마을 업무(B3): 귀환 판단 → (귀환 주문서 또는 포털 길찾기) → 마을에서 더 좋은 장비 장착 → 상점 NPC 로 걸어가 말 걸기 → 잡템·안 쓸 장비 판매 → 물약 충전 → 창 닫기.
/// 모든 조작은 진짜 키 입력(BotKey → PlayBot → BotInput)이다. 게임 함수(상점·인벤 판매/장착)는 부르지 않고, 열린 창·커서·가방 목록은 읽기만 한다.
/// 단계마다 제한 시간이 있고, 넘으면 업무를 접는다(HuntBrain 의 CloseUi 가 열린 창을 닫는다).
/// 판단 규칙은 BotErrandRules·BotMenuNav(순수). 설계는 PlayBotDesign.md 12절.
/// </summary>
public sealed class BotErrand
{
    public const string Town = "Town";
    public const float EvalInterval = 1f;
    public const float MinFieldSeconds = 90f;   // 마을에서 돌아온 뒤 이만큼은 사냥(포션이 바닥이면 예외)
    public const float AbortCooldown = 120f;
    public const float PhaseTimeout = 45f;
    public const float RecallHold = 3.6f;
    public const float RecallRetry = 25f;
    public const int MaxAttempts = 3;
    public const int FocusErrand = -4;

    private enum Phase { None, Equip, Walk, Talk, Sell, Buy, Close }

    private readonly IBotNavigator _nav;
    private readonly BotGear _gear = new BotGear();
    private readonly Dictionary<int, int> _attempts = new Dictionary<int, int>();
    private Phase _phase;
    private float _phaseSince;
    private float _nextKey;
    private float _nextEval;
    private float _lastVisitEnd = -1000f;
    private float _blockedUntil;
    private float _returnSince;
    private int _bagThreshold = -1;
    private int _potionThreshold;
    private int _junkPresses;
    private float _junkWaitUntil;
    private int _talkTries;
    private float _talkAt;
    private float _buyAt = -1f;
    private int _progressSeen;
    private ItemCheck _buyItem;
    private int _buyQty;
    private int _buyTries;
    private bool _buyConfirmed;
    private int _qtyLast = -1;
    private int _qtyStuck;
    private int _lastTarget = -1;
    private int _potionsBefore;
    private int _goldBefore;
    private ShopUI _shop;
    private string _shopMap;
    private float _shopRetryAt;
    private bool _recallActive;
    private float _recallStart;
    private float _recallBlock;
    private bool _recallPressed;
    private string _mapAtRecall;

    public bool Returning { get; private set; }
    public string Reason { get; private set; } = "";
    public int Trips { get; private set; }
    public int Sold { get; private set; }
    public int Equipped { get; private set; }
    public int PotionsBought { get; private set; }
    public int GoldGained { get; private set; }
    public int Aborts { get; private set; }
    public int Recalls { get; private set; }
    public string Phased => _phase.ToString();

    public BotErrand(IBotNavigator nav)
    {
        _nav = nav;
    }

    public void Begin(BotContext ctx)
    {
        Returning = false;
        _phase = Phase.None;
        _lastVisitEnd = ctx.Now - MinFieldSeconds * 0.5f; // 시작 직후 곧바로 돌아가지 않게
        _bagThreshold = -1;
        Trips = 0;
        Sold = 0;
        Equipped = 0;
        PotionsBought = 0;
        GoldGained = 0;
        Aborts = 0;
        Recalls = 0;
    }

    // ───── 귀환 판단 ─────

    /// <summary>사냥 중 1초마다: 가방 여유·포션이 문턱 아래면 귀환을 켠다. 켜졌으면 true</summary>
    public bool Evaluate(BotContext ctx)
    {
        if (Returning || ctx.Maps == null || ctx.Now < _nextEval || ctx.Now < _blockedUntil)
        {
            return Returning;
        }
        _nextEval = ctx.Now + EvalInterval;
        if (_bagThreshold < 0)
        {
            _bagThreshold = BotErrandRules.ClampBagFree(ctx.Profile.Sample("return.bagFree", ctx.Rng, BotErrandRules.FallbackBagFree));
            _potionThreshold = BotErrandRules.ClampPotions(ctx.Profile.Sample("return.potions", ctx.Rng, BotErrandRules.FallbackPotions));
        }
        int bag = BagFree();
        int pots = HpPotions();
        if (bag < 0)
        {
            return false;
        }
        BotErrandRules.Reason why = BotErrandRules.WhyReturn(bag, pots, _bagThreshold, _potionThreshold);
        bool emergency = pots <= 0 && HasHpPotionSlot();
        if (why == BotErrandRules.Reason.None || (ctx.Now - _lastVisitEnd < MinFieldSeconds && !emergency))
        {
            return false;
        }
        Returning = true;
        _returnSince = ctx.Now;
        Reason = why + " bagFree=" + bag + "<=" + _bagThreshold + " potions=" + pots + "<=" + _potionThreshold;
        Debug.Log("[PB][Bot] 귀환 결정 " + Reason + " map=" + BgmDirector.CurrentMap);
        return true;
    }

    /// <summary>귀환 중 필드에서: 귀환 주문서가 있으면 진짜 퀵슬롯 키로 쓰고 제자리에서 기다린다. 쓰는 중이면 true</summary>
    public bool TryRecall(BotContext ctx, bool foeNear, ref BotIntent it)
    {
        if (_recallActive)
        {
            if (BgmDirector.CurrentMap != _mapAtRecall)
            {
                _recallActive = false;
                return false;
            }
            if (ctx.Now - _recallStart > RecallHold || foeNear)
            {
                _recallActive = false; // 안 먹혔다(맞음·시간): 걸어서 간다
                _recallBlock = ctx.Now + RecallRetry;
                return false;
            }
            it.Focus = BotIntent.FocusWander;
            it.Note = "recall";
            return true;
        }
        if (foeNear || ctx.Now < _recallBlock || ctx.Obs.UiOpen || !ctx.Obs.Grounded)
        {
            return false;
        }
        InventoryManager inv = InventoryManager.instance;
        if (inv == null)
        {
            return false;
        }
        for (int s = 0; s < QuickSlotModel.SlotCount; s++)
        {
            string id = QuickSlotBar.SlotItem(s);
            if (id == "Recall Scroll" && inv.CountItem(id) > 0)
            {
                it.Quick = s + 1;
                it.Focus = BotIntent.FocusWander;
                it.Note = "recall";
                _recallActive = true;
                _recallStart = ctx.Now;
                _mapAtRecall = BgmDirector.CurrentMap;
                Recalls++;
                return true;
            }
        }
        _recallBlock = ctx.Now + RecallRetry;
        return false;
    }

    /// <summary>길을 못 찾았거나 너무 오래 걸림: 접고 한동안 사냥</summary>
    public void Abort(BotContext ctx, string why)
    {
        if (Returning)
        {
            Aborts++;
            if (_phase != Phase.None)
            {
                GoldGained += DatabaseManager.money - _goldBefore; // 중단돼도 이미 한 판매·장착은 센다
                Sold += _gear.SellDone;
                Equipped += _gear.EquipDone;
            }
            Debug.LogWarning("[PB][Bot] 귀환 업무 중단 " + why + " phase=" + _phase + " map=" + BgmDirector.CurrentMap);
        }
        Returning = false;
        _phase = Phase.None;
        _recallActive = false;
        _blockedUntil = ctx.Now + AbortCooldown;
        _bagThreshold = -1;
    }

    public float ReturnSeconds(float now)
    {
        return Returning ? now - _returnSince : 0f;
    }

    // ───── 마을 업무 ─────

    /// <summary>마을 도착 뒤 한 틱. 업무 중이면 true(intent 를 채움), 아니면 false</summary>
    public bool Run(BotContext ctx, ref BotIntent it)
    {
        if (!Returning)
        {
            return false;
        }
        if (BgmDirector.CurrentMap != Town)
        {
            if (_phase != Phase.None)
            {
                Abort(ctx, "업무 중 마을을 벗어남"); // 사망·포털 등으로 밖에 나갔다: 다음 도착 때 중간 단계를 이어 하지 않는다
            }
            return false;
        }
        if (_phase == Phase.None)
        {
            Trips++;
            _goldBefore = DatabaseManager.money;
            _attempts.Clear();
            _gear.ResetCounts();
            _junkPresses = 0;
            _progressSeen = 0;
            _talkTries = 0;
            _buyAt = -1f;
            _buyItem = null;
            _buyConfirmed = false;
            _buyTries = 0;
            _qtyLast = -1;
            _qtyStuck = 0;
            Enter(Phase.Equip, ctx.Now);
            Debug.Log("[PB][Bot] 마을 도착 업무 시작 gold=" + _goldBefore);
        }
        it.Focus = FocusErrand;
        it.Note = "errand:" + _phase;
        if (ctx.Now - _phaseSince > PhaseTimeout)
        {
            Abort(ctx, "단계 시간 초과");
            return false;
        }
        int progress = _gear.SellDone + _gear.EquipDone;
        if (progress != _progressSeen)
        {
            _progressSeen = progress;
            _phaseSince = ctx.Now; // 팔거나 끼는 일이 진행되는 동안은 단계 제한 시간을 새로 센다(잡템 14개 판매 = 45초 넘음)
        }
        BotMenuView v = ReadView();
        switch (_phase)
        {
            case Phase.Equip: DoEquip(ctx, v, ref it); break;
            case Phase.Walk: DoWalk(ctx, v, ref it); break;
            case Phase.Talk: DoTalk(ctx, v, ref it); break;
            case Phase.Sell: DoSell(ctx, v, ref it); break;
            case Phase.Buy: DoBuy(ctx, v, ref it); break;
            case Phase.Close: DoClose(ctx, v, ref it); break;
        }
        return Returning;
    }

    private void Enter(Phase p, float now)
    {
        _phase = p;
        _phaseSince = now;
        Debug.Log("[PB][Bot] 업무 단계 " + p);
    }

    private void Finish(BotContext ctx)
    {
        GoldGained += DatabaseManager.money - _goldBefore;
        Sold += _gear.SellDone;
        Equipped += _gear.EquipDone;
        Returning = false;
        _phase = Phase.None;
        _lastVisitEnd = ctx.Now;
        _bagThreshold = -1;
        Debug.Log("[PB][Bot] 마을 업무 끝 sold=" + Sold + " equipped=" + Equipped + " potionsBought=" + PotionsBought + " gold=" + DatabaseManager.money);
    }

    /// <summary>키를 누른다(간격 안이면 건너뜀). 실제로 눌렀으면 true</summary>
    private bool Press(BotContext ctx, BotKey key, ref BotIntent it, float extraWait = 0f)
    {
        if (key == BotKey.None || ctx.Now < _nextKey)
        {
            return false;
        }
        it.Menu = key;
        float gap = ctx.Profile.Sample("menu.focusInterval", ctx.Rng, 0.25f);
        _nextKey = ctx.Now + Mathf.Clamp(gap, 0.22f, 0.9f) + extraWait;
        return true;
    }

    private void DoEquip(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        _gear.Build();
        BotGear.Plan target;
        if (!TryNextPlan(_gear.Equip, out target))
        {
            if (v.InvOpen)
            {
                Press(ctx, BotKey.Inventory, ref it); // 장착 끝: 가방 닫기
                return;
            }
            Enter(Phase.Walk, ctx.Now);
            return;
        }
        Navigate(ctx, v, target, ref it, true);
    }

    private void DoWalk(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        if (v.InvOpen || v.ShopOpen)
        {
            Press(ctx, BotKey.Inventory, ref it);
            return;
        }
        ShopUI shop = FindShop();
        if (shop == null)
        {
            Abort(ctx, "상점 NPC 없음");
            return;
        }
        if (shop.isShopClose)
        {
            it.MoveX = 0;
            Enter(Phase.Talk, ctx.Now);
            _talkAt = ctx.Now + 0.4f;
            return;
        }
        Vector3 p = shop.transform.position;
        _nav.Steer(ctx, p.x - ctx.Obs.X, p.y - ctx.Obs.Y, ref it);
    }

    private void DoTalk(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        if (v.ShopOpen)
        {
            Enter(Phase.Sell, ctx.Now);
            return;
        }
        if (v.NpcMenuOpen)
        {
            Press(ctx, BotMenuNav.ToNpcShop(v), ref it);
            return;
        }
        ShopUI shop = FindShop();
        if (shop != null && !shop.isShopClose)
        {
            Enter(Phase.Walk, ctx.Now); // 밀려났다
            return;
        }
        if (ctx.Now < _talkAt)
        {
            return;
        }
        if (_talkTries++ >= 6)
        {
            Abort(ctx, "NPC 말 걸기 실패");
            return;
        }
        _talkAt = ctx.Now + 1.4f;
        it.Menu = BotKey.Up; // NPC 상호작용 = ↑
    }

    private void DoSell(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        if (!v.ShopOpen)
        {
            Enter(Phase.Close, ctx.Now);
            return;
        }
        if (v.Popup)
        {
            Press(ctx, BotKey.Select, ref it);
            return;
        }
        bool browse = string.IsNullOrEmpty(v.State) || v.State == "ShopCusorOn" || v.State == "detail";
        if (_junkPresses < 2 && browse && ctx.Now >= _junkWaitUntil && HasJunk())
        {
            if (Press(ctx, BotKey.Junk, ref it, 0.6f)) // 같은 구성으로 두 번 눌러야 판다(8초 안)
            {
                _junkPresses++;
                _junkWaitUntil = ctx.Now + 0.9f;
            }
            return;
        }
        if (ctx.Now < _junkWaitUntil)
        {
            return;
        }
        _gear.Build();
        BotGear.Plan target;
        if (!TryNextPlan(_gear.Sell, out target))
        {
            Enter(Phase.Buy, ctx.Now);
            return;
        }
        Navigate(ctx, v, target, ref it, false);
    }

    private void DoBuy(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        if (!v.ShopOpen)
        {
            Debug.Log("[PB][Bot] 구매 단계: 상점이 닫혀 있다 state=" + v.State + " npcMenu=" + v.NpcMenuOpen);
            Enter(Phase.Close, ctx.Now);
            return;
        }
        if (QuantityPopup.IsOpen && _buyItem != null)
        {
            DoBuyQuantity(ctx, ref it);
            return;
        }
        if (v.Popup || v.State == "Sell" || v.State == "Buy" || v.State == "BuyDetail")
        {
            return; // 판매·구매 직후 창이 닫히는 사이: 기다린다(여기서 Back 을 누르면 상점이 닫힌다)
        }
        if (_buyConfirmed)
        {
            if (ctx.Now >= _buyAt)
            {
                int gained = HpPotions() - _potionsBefore;
                if (gained > 0)
                {
                    PotionsBought += gained;
                }
                Debug.Log("[PB][Bot] 물약 구매 " + _buyItem.name + " x" + gained + " gold=" + DatabaseManager.money);
                Enter(Phase.Close, ctx.Now);
            }
            return;
        }
        if (v.State != "ShopCusorOn")
        {
            // 판매를 마친 커서는 가방 칸(State ""): ← 로 왼쪽 끝을 지나 페이지 고르기(boxChange)로 들어가고, boxChange 에서 ← 를 한 번 더 누르면 상점 진열로 돌아온다(Back 은 상점을 닫는다)
            bool left = string.IsNullOrEmpty(v.State) || v.State == "boxChange";
            Press(ctx, left ? BotKey.Left : BotKey.Back, ref it);
            return;
        }
        if (_buyItem == null && !PickPotion(ctx, out _buyItem, out _buyQty))
        {
            Debug.Log("[PB][Bot] 구매 단계: 살 물약 없음/수량 0 gold=" + DatabaseManager.money);
            Enter(Phase.Close, ctx.Now);
            return;
        }
        ItemCheck cur = ActiveShopItem();
        if (cur == null || cur.transform.parent == null || _buyTries > 60)
        {
            Debug.Log("[PB][Bot] 구매 단계: 진열 칸 이동 실패 tries=" + _buyTries + " cur=" + (cur != null));
            Enter(Phase.Close, ctx.Now);
            return;
        }
        BotKey key = BotErrandRules.ToStockCell(cur.transform.parent.GetSiblingIndex(), cur.transform.GetSiblingIndex(),
            _buyItem.transform.parent.GetSiblingIndex(), _buyItem.transform.GetSiblingIndex());
        if (key == BotKey.Select && cur != _buyItem)
        {
            key = BotKey.None;
        }
        if (Press(ctx, key, ref it, key == BotKey.Select ? 0.3f : 0f))
        {
            _buyTries++; // 실제로 누른 키만 센다
        }
    }

    /// <summary>수량 창: 위·아래(±1)·좌우(±10)로 목표 수량에 맞춘 뒤 확정</summary>
    private void DoBuyQuantity(BotContext ctx, ref BotIntent it)
    {
        int cur = QuantityPopup.CurrentValue;
        BotKey key = BotErrandRules.ToQuantity(cur, _buyQty);
        if (ctx.Now < _nextKey)
        {
            return; // 키 간격 안: 값이 바뀔 시간을 준다(막힘 판정도 누른 뒤에만)
        }
        if (key != BotKey.Select && cur == _qtyLast && ++_qtyStuck >= 2)
        {
            key = BotKey.Select; // 창 최대값에 막혀 더 안 늘어난다
        }
        if (Press(ctx, key, ref it, key == BotKey.Select ? 0.4f : 0f))
        {
            _qtyStuck = cur == _qtyLast ? _qtyStuck : 0;
            _qtyLast = cur;
            if (key == BotKey.Select)
            {
                _buyConfirmed = true;
                _buyAt = ctx.Now + 1f;
                _potionsBefore = HpPotions();
            }
        }
    }

    /// <summary>진열 첫 페이지에서 살 HP 물약(퀵슬롯에 등록된 종류, 없으면 Medium)과 수량(가진 골드 25~35%)</summary>
    private bool PickPotion(BotContext ctx, out ItemCheck item, out int qty)
    {
        item = null;
        qty = 0;
        ItemCheck cur = ActiveShopItem();
        if (cur == null || cur.transform.parent == null || cur.transform.parent.parent == null)
        {
            return false;
        }
        string want = null;
        for (int s = 0; s < QuickSlotModel.SlotCount && want == null; s++)
        {
            string id = QuickSlotBar.SlotItem(s);
            want = BotPotion.IsHpPotion(id) ? id : null;
        }
        Transform page = cur.transform.parent.parent;
        ItemCheck fallback = null;
        for (int l = 0; l < page.childCount; l++)
        {
            Transform line = page.GetChild(l);
            for (int c = 0; c < line.childCount; c++)
            {
                ItemCheck ic = line.GetChild(c).GetComponent<ItemCheck>();
                if (ic == null || !BotPotion.IsHpPotion(ic.name))
                {
                    continue;
                }
                if (ic.name == want)
                {
                    item = ic;
                }
                else if (fallback == null || ic.name == "Medium HP Potion")
                {
                    fallback = ic;
                }
            }
        }
        item = item != null ? item : fallback;
        if (item == null)
        {
            return false;
        }
        float pct = Mathf.Lerp(0.25f, 0.35f, (float)ctx.Rng.NextDouble());
        qty = BotErrandRules.PotionBuyAmount(DatabaseManager.money, item.buyprice, InventoryManager.instance.CountItem(item.name), item.itemData != null ? item.itemData.maxStack : 99, pct);
        Debug.Log("[PB][Bot] 물약 구매 계획 " + item.name + " x" + qty + " gold=" + DatabaseManager.money + " price=" + item.buyprice);
        return qty > 0;
    }

    private ItemCheck ActiveShopItem()
    {
        ShopManager[] all = Object.FindObjectsByType<ShopManager>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].ShopUI != null && all[i].ShopUI.activeSelf)
            {
                return all[i].nowShopItem;
            }
        }
        return null;
    }

    private void DoClose(BotContext ctx, BotMenuView v, ref BotIntent it)
    {
        if (!v.InvOpen && !v.ShopOpen && !v.NpcMenuOpen && !v.Popup)
        {
            Finish(ctx);
            return;
        }
        Press(ctx, v.Popup || v.NpcMenuOpen ? BotKey.Back : BotKey.Inventory, ref it);
    }

    /// <summary>목표 칸까지 키를 한 번씩 눌러 간다. 목표 칸을 확정하는 Select 마다 시도 횟수를 센다(MaxAttempts 넘으면 그 칸 제외)</summary>
    private void Navigate(BotContext ctx, BotMenuView v, BotGear.Plan target, ref BotIntent it, bool equip)
    {
        if (!equip && !v.ShopOpen)
        {
            return;
        }
        if (equip && !v.InvOpen && !v.Popup)
        {
            Press(ctx, BotKey.Inventory, ref it, 0.3f);
            return;
        }
        if (target.Index != _lastTarget)
        {
            _lastTarget = target.Index;
            Debug.Log("[PB][Bot] 업무 목표 " + (equip ? "장착 " : "판매 ") + target.Id + " idx=" + target.Index);
        }
        BotKey key = BotMenuNav.ToItem(v, BotMenuNav.PageOf(target.Index), BotMenuNav.CellOf(target.Index));
        if (key == BotKey.Select && string.IsNullOrEmpty(v.State) && !v.Popup && ctx.Now >= _nextKey) // 실제로 누르는 틱만 시도로 센다
        {
            int n;
            _attempts.TryGetValue(target.Index, out n);
            _attempts[target.Index] = n + 1;
        }
        Press(ctx, key, ref it, key == BotKey.Select ? 0.15f : 0f);
    }

    /// <summary>계획에서 시도 횟수를 넘지 않은 첫 칸</summary>
    private bool TryNextPlan(List<BotGear.Plan> plans, out BotGear.Plan target)
    {
        target = default(BotGear.Plan);
        for (int i = 0; i < plans.Count; i++)
        {
            int n;
            _attempts.TryGetValue(plans[i].Index, out n);
            if (n < MaxAttempts)
            {
                target = plans[i];
                return true;
            }
        }
        return false;
    }

    // ───── 관측(읽기 전용) ─────

    private BotMenuView ReadView()
    {
        InventoryManager inv = InventoryManager.instance;
        BotMenuView v = default(BotMenuView);
        v.NpcMenuOpen = NpcMenu.IsOpen;
        v.NpcCursor = NpcMenu.Cursor;
        v.NpcRow = NpcMenu.RowOf(NpcMenuRules.Kind.Shop);
        v.Popup = QuantityPopup.IsBlocking || ConfirmPopup.IsBlocking;
        if (inv == null)
        {
            return v;
        }
        v.InvOpen = inv.inventory != null && inv.inventory.activeSelf;
        v.ShopOpen = inv.ShopGameObject != null && inv.ShopGameObject.activeSelf;
        v.State = inv.state;
        v.Page = inv.nowBox;
        v.Cell = inv.cusorCount != null && inv.nowBox >= 0 && inv.nowBox < inv.cusorCount.Length ? inv.cusorCount[inv.nowBox] : 0;
        v.BoxCursor = inv.boxCusor;
        return v;
    }

    private ShopUI FindShop()
    {
        string map = BgmDirector.CurrentMap;
        if (_shopMap == map && (_shop != null || Time.time < _shopRetryAt))
        {
            return _shop; // 못 찾은 맵도 1초는 다시 찾지 않는다
        }
        _shopRetryAt = Time.time + 1f;
        _shopMap = map;
        _shop = null;
        ShopUI[] all = Object.FindObjectsByType<ShopUI>(FindObjectsSortMode.None);
        Vector3 me = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
        float best = float.MaxValue;
        for (int i = 0; i < all.Length; i++)
        {
            float d = Mathf.Abs(all[i].transform.position.x - me.x);
            if (all[i].isActiveAndEnabled && d < best)
            {
                best = d;
                _shop = all[i];
            }
        }
        return _shop;
    }

    private static int BagFree()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.Service == null)
        {
            return -1;
        }
        SlotContainer m = inv.Service.ScanModel();
        int free = 0;
        for (int i = 0; i < m.Unlocked && i < m.Capacity; i++)
        {
            if (m[i].IsEmpty)
            {
                free++;
            }
        }
        return free;
    }

    private static bool HasHpPotionSlot()
    {
        for (int s = 0; s < QuickSlotModel.SlotCount; s++)
        {
            if (BotPotion.IsHpPotion(QuickSlotBar.SlotItem(s)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>퀵슬롯에 등록된 HP 물약의 보유 수(포션 칸이 없으면 가방 전체의 물약)</summary>
    private static int HpPotions()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null)
        {
            return 0;
        }
        int n = 0;
        for (int s = 0; s < QuickSlotModel.SlotCount; s++)
        {
            string id = QuickSlotBar.SlotItem(s);
            if (BotPotion.IsHpPotion(id))
            {
                n += inv.CountItem(id);
            }
        }
        return n;
    }

    private static bool HasJunk()
    {
        InventoryManager inv = InventoryManager.instance;
        return inv != null && inv.Service != null && BulkSaleRules.Build(inv.Service.ScanModel(), CsvItemCatalog.Shared).Count > 0;
    }
}
