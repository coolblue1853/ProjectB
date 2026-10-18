// 에너지 셰이더 A/B: __KEY__ = "on|0.2" 형식 (셰이더 사용 여부|정지 시각). 무기는 미리 equip_weapon 으로 장착
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
if (w == null) return "no weapon";
var parts = "__KEY__".Split('|');
w.useEnergyShader = parts[0] == "on";
foreach (var sk in w.skill) if (sk != null) sk.castVfx = VfxId.None; // 스프라이트만 비교
w.ActiveWeaponSkill(int.Parse(parts[2]));
DG.Tweening.DOVirtual.DelayedCall(float.Parse(parts[1]), () => Time.timeScale = 0f, false);
return "cast skill" + parts[2] + " shader=" + w.useEnergyShader + " color=" + w.energyColor;
