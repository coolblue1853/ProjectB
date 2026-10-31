// 몸박·레벨 차 피해 검증(플레이). 가장 가까운 일반 몹 위에 서서 HP 변화를 기록한다. 결과는 wtype_log.cs 로 읽는다
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_wt", log);
var pc = PlayerController.instance; var ph = PlayerHealthManager.Instance;
EnemyHealth target = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (!e.IsMonster || e.isBossMob || e.ContactDamage <= 0) continue; float dd = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (dd < best) { best = dd; target = e; } }
if (target == null) return "no target";
System.Action<float, System.Action> at = (t, a) => DG.Tweening.DOVirtual.DelayedCall(t, () => { try { a(); } catch (System.Exception ex) { log.Append("EX " + ex.Message + "; "); } }, false);
float t0 = Time.time; int lastHp = 0; Vector3 lastPos = Vector3.zero;
log.Append(target.name + " lv" + target.Level + " contact" + target.ContactDamage + " elite" + target.IsElite + " playerLv" + LevelSystem.Level + " def" + ph.Def + " | ");
// 다른 몹은 치우고(소음 제거), god 프로브의 HP 보충은 끈다
System.AppDomain.CurrentDomain.SetData("pb_nogod", true);
foreach (var e2 in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active)) { if (e2 != target && e2.IsMonster && !e2.isBossMob) e2.ForceEnemyRelease(); }
// 몹을 멈춰 세워(경직 BT 끄기 대신 매 프레임 위치 고정) 그 위에 선다
var home = target.transform.position;
System.Action place = () => { if (target != null) { pc.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 0.1f, pc.transform.position.z); Physics2D.SyncTransforms(); } };
// A) 3.6초 겹쳐 서 있기 -> HP 변화 시각(0.9초 간격)
at(0.1f, () => { ph.nowHp = ph.fullHP; lastHp = ph.nowHp; place(); log.Append("A start hp" + lastHp + "; "); });
for (int i = 0; i < 40; i++) at(0.2f + i * 0.09f, () => { place(); if (ph.nowHp != lastHp) { log.Append("A hp" + lastHp + "->" + ph.nowHp + " (-" + (lastHp - ph.nowHp) + ") @" + (Time.time - t0).ToString("F2") + "; "); lastHp = ph.nowHp; } });
// B) 대시 무적 중에는 피해가 없다
at(4.2f, () => { ph.nowHp = ph.fullHP; lastHp = ph.nowHp; PlayerStatus.AcquireFor(StatusFlag.Invincible, 3f, "ContactTest"); log.Append("B invincible hp" + lastHp + " flag" + DatabaseManager.isInvincibility + "; "); });
for (int i = 0; i < 15; i++) at(4.3f + i * 0.09f, () => { place(); if (ph.nowHp != lastHp) { log.Append("B hit!" + lastHp + "->" + ph.nowHp + " flag" + DatabaseManager.isInvincibility + " @" + (Time.time - t0).ToString("F2") + "; "); lastHp = ph.nowHp; } });
at(5.8f, () => { log.Append("B after hp" + ph.nowHp + " (loss " + (lastHp - ph.nowHp) + "); "); });
// C) 정예는 x1.5
at(6.0f, () => { int before = target.ContactDamage; target.PromoteElite(); log.Append("C contact " + before + " -> " + target.ContactDamage + " elite" + target.IsElite + "; "); ph.nowHp = ph.fullHP; lastHp = ph.nowHp; });
for (int i = 0; i < 18; i++) at(6.2f + i * 0.09f, () => { place(); if (ph.nowHp != lastHp) { log.Append("C hp" + lastHp + "->" + ph.nowHp + " (-" + (lastHp - ph.nowHp) + ") @" + (Time.time - t0).ToString("F2") + "; "); lastHp = ph.nowHp; } });
// D) 레벨 차 피해 감소: 같은 무기 배열로 몹을 때려 HP 변화 비교
at(8.2f, () => {
    target.nowHP = target.maxHP; var w = new int[] { 100, 100, 0, 0, 0, 0, 0, 0, 0, 0 };
    int h0 = target.nowHP; target.damage2Enemy(w, 0f, 0f, Vector2.right, 0f, false, false, 0f, 1f); int dealt = h0 - target.nowHP;
    float mul = DamageCalculator.LevelGapMul(target.Level, LevelSystem.Level);
    var calc = DamageCalculator.Calculate(w, DamageCalculator.PlayerOffense.FromDatabase(), target.enemyDef, false, false, 1f, 0f, 99.9f);
    log.Append("D mobLv" + target.Level + " playerLv" + LevelSystem.Level + " mul" + mul.ToString("F2") + " dealt" + dealt + " noGap" + calc.damage + "; ");
});
at(8.6f, () => { log.Append("DONE"); });
return "scheduled contact vs " + target.name;
