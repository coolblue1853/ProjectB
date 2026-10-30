// 공격 셀 ↔ 궤적 이펙트 타이밍 확인(플레이 중, WeaponFx.StrikeDelayFor): 애니마다 Play → 바로 ApplyCel(리플렉션) → 실제 셀 이름 + 궤적 지연(초).
// FrameStartTime 이 고른 셀과 화면 셀이 같아야 한다(10-30 모션 리뷰: 예전엔 MapName 셀 기준이라 어긋남)
var pc = PlayerController.instance;
var rig = pc.Rig as PixelRig;
if (rig == null) return "no PixelRig";
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var sr = typeof(PixelRig).GetField("_cel", BF).GetValue(rig) as SpriteRenderer;
var clips = typeof(PixelRig).GetField("_celClips", BF).GetValue(rig) as System.Collections.IEnumerable;
var cur = typeof(PixelRig).GetField("_current", BF);
var apply = typeof(PixelRig).GetMethod("ApplyCel", BF);
string[] anims = { "Stab", "SwingDown", "SwingUp", "GreatUpper", "GreatVashDown", "GreatDownAttack", "GreatSideAttack", "StrongStab", "upperCut", "LeftHandStab", "StabLR", "Lunge", "Punch" };
var sb = new System.Text.StringBuilder();
foreach (string a in anims)
{
    rig.Play(a);
    apply.Invoke(rig, new object[] { cur.GetValue(rig) });
    string shown = "?";
    foreach (CelClip c in clips) { if (c != null && System.Array.IndexOf(c.sprites, sr.sprite) >= 0) { shown = c.name; break; } }
    sb.Append(a + "→" + shown + " map=" + PixelRig.MapName(a) + " fx=" + WeaponFx.StrikeDelayFor(a, rig).ToString("F2") + "\n");
}
rig.Play("Idle");
return sb.ToString();
