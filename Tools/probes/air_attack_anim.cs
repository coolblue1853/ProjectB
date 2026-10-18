// W-12: 공중 평타 종료 후 Jump 애니(isJumpAnim) 복귀 → 착지 시 해제. 먼저 지팡이 장착. 결과 pb_w12
var pc = PlayerController.instance;
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var fj = typeof(PlayerController).GetField("isJumpAnim", B); var fa = typeof(PlayerController).GetField("isAttackAnim", B);
var rb = pc.GetComponent<Rigidbody2D>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_w12", log);
pc.transform.position += new Vector3(0f, 25f, 0f); rb.linearVelocity = new Vector2(0f, 1f);
DG.Tweening.DOVirtual.DelayedCall(0.1f, () => Weapon.Equipped.MeleeAttack(), true);
for (int k = 1; k <= 12; k++) { float tt = 0.25f * k; DG.Tweening.DOVirtual.DelayedCall(0.1f + tt, () => log.Append(tt.ToString("F1") + ":g" + (pc.isGround?1:0) + "a" + ((bool)fa.GetValue(pc)?1:0) + "j" + ((bool)fj.GetValue(pc)?1:0) + "vy" + rb.linearVelocity.y.ToString("F1") + " "), true); }
return "atk=" + Weapon.Equipped.attckSpeed[0];
