var o = new System.Text.StringBuilder();
var dm = UnityEngine.Object.FindFirstObjectByType<DropManager>(FindObjectsInactive.Include);
if (dm == null) o.Append("no DropManager\n"); else {
 int m3=0, n=400; int baseV=-1;
 var pos = PlayerController.instance.transform.position;
 for (int i=0;i<n;i++){ int g = dm.DropGold("Rabbit_BD", false, false, pos); if (baseV<0||g<baseV) baseV=g; if (g>=3*baseV && g>0) m3++; }
 o.Append("DropGold called " + n + "\n");
}
var phm = UnityEngine.Object.FindFirstObjectByType<PlayerHealthManager>();
phm.nowHp = 5; AccessoryGimmick.LastStand.Reset();
phm.damage2Player(9999, 0f, 0f, Vector2.zero, 0f, false, 0f);
o.Append("after lethal#1 hp=" + phm.nowHp + " remaining=" + AccessoryGimmick.LastStand.Remaining(Time.time).ToString("F0") + "\n");
return o.ToString();
