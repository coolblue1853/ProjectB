// PV6: 분리 생성 공격 정예 ×1.5(EnemyAttackScale) + CounterBuff 실컴포넌트 → Logs/pv6/scale.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder();
var phm = pc.GetComponentInChildren<PlayerHealthManager>();
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 1; GameObject host = null; EnemyHealth foe = null; int[] drops = new int[3]; int k = 0;
Func<bool, int, int> dummy = null;
UnityEditor.EditorApplication.CallbackFunction upd = null;
System.Action<GameObject> fire = (owner) => {
  var g = new GameObject("pv6_atk"); g.layer = LayerMask.NameToLayer("Enemy"); g.transform.position = phm.transform.position;
  var rb = g.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; var bc = g.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = new Vector2(4,4);
  var d = g.AddComponent<EnemyDamageObject>(); d.damage = 300; d.isDestroyByTime = false; d.enemyOb = owner; d.player = pc.gameObject; d.isNockBackChangeDir = false;
  typeof(EnemyDamageObject).GetMethod("OnTriggerEnter2D", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(d, new object[]{ pc.GetComponent<Collider2D>() }); UnityEngine.Object.Destroy(g, 0.5f); };
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
  try {
  if (st == 0) { MapInfo.GoToMap("Lv1_NearForest", pc.gameObject); until = now + 5; st = 1; return; }
  if (st == 1) { foe = UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).FirstOrDefault(e => !e.IsElite && !e.isBossMob); sb.AppendLine("foe=" + (foe? foe.name : "null")); if (foe == null) { st = 9; return; } phm.fullHP = 9999; phm.nowHp = 9999; k = 0; st = 2; return; }
  if (st == 2) { int b = phm.nowHp; fire(foe.gameObject); until = now + 0.4; drops[0] = b; st = 3; return; }
  if (st == 3) { drops[1] = drops[0] - phm.nowHp; sb.AppendLine("일반 몹 소유 피해=" + drops[1]); phm.fullHP = 9999; phm.nowHp = 9999; foe.PromoteElite(); until = now + 3; st = 4; return; }
  if (st == 4) { drops[0] = phm.nowHp; fire(foe.gameObject); until = now + 0.4; st = 5; return; }
  if (st == 5) { drops[2] = drops[0] - phm.nowHp; sb.AppendLine("정예 소유 피해=" + drops[2] + " IsElite=" + foe.IsElite + " 비율=" + (drops[1]>0 ? (drops[2]/(float)drops[1]).ToString("F2") : "n/a")); phm.fullHP = 9999; phm.nowHp = 9999;
     var cb = new GameObject("pv6_cb").AddComponent<CounterBuff>(); sb.AppendLine("CounterBuff before=" + cb.Active + " mul=" + cb.HitMultiplier(null,null,null)); cb.NotifyBlocked(); sb.AppendLine("armed=" + cb.Active + " mul=" + cb.HitMultiplier(null,null,null)); until = now + 3.2; host = cb.gameObject; st = 6; return; }
  if (st == 6) { var cb = host.GetComponent<CounterBuff>(); sb.AppendLine("3.2초 후 Active=" + cb.Active + " mul=" + cb.HitMultiplier(null,null,null)); UnityEngine.Object.Destroy(host); st = 9; return; }
  } catch (System.Exception e) { sb.AppendLine("ERR " + e); st = 9; }
  if (st == 9) { System.IO.Directory.CreateDirectory("Logs/pv6"); System.IO.File.WriteAllText("Logs/pv6/scale.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
