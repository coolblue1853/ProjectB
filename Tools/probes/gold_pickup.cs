// 바닥 골드 더미로 플레이어를 순서대로 옮겨 줍기 → 돈 증가량 확인
var pc = PlayerController.instance;
int before = DatabaseManager.money, piles = 0, sum = 0;
foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude)) if (d.goldAmount > 0) { piles++; sum += d.goldAmount; }
return "piles=" + piles + " sum=" + sum + " money=" + before + " pickedCount=" + DropItemCheck.GoldPickedCount + " pickedTotal=" + DropItemCheck.GoldPickedTotal + " droppedTotal=" + DropManager.GoldDroppedTotal;
