// 시험 준비(봇 행동 아님, 봇 실행 전에만): 귀환 주문서 __N__ 개를 가방에 넣고 퀵슬롯 3번에 등록한다(귀환 주문서 실사용 확인용). 세이브는 실행 스크립트가 백업·원복한다
if (PlayBot.IsRunning) return "bot running - refused";
var inv = InventoryManager.instance;
if (inv == null) return "no inventory";
int have = inv.CountItem("Recall Scroll");
if (have < __N__) inv.Service.Add("Recall Scroll", __N__ - have, AddSource.Other);
bool ok = QuickSlotBar.Model != null && QuickSlotBar.Model.Assign(2, "Recall Scroll");
return "recall=" + inv.CountItem("Recall Scroll") + " assigned=" + ok + " slot3=" + QuickSlotBar.SlotItem(2);
