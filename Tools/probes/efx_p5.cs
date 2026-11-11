// REWORK-12 P5: Gatekeeper's Oath 장착 → Gate Cleave 변형(스케일·관통)·피해/쿨 Mod·처치 시 Stone Sentinels. 로그 "efx5"
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx5", log);
var pc = PlayerController.instance;
var db = DatabaseManager.instance;
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
System.Func<float, int, EnemyHealth> spawn = (dx, hp) => {
    float gy = GroundProbe.Y(pc.transform.position + Vector3.right * dx);
    var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + dx, gy + 0.6f, 0f), Quaternion.identity);
    var e = go.GetComponent<EnemyHealth>(); e.maxHP = hp; e.nowHP = hp; return e;
};
System.Func<string> objs = () => { var l = new System.Collections.Generic.List<string>(); foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) l.Add(o.skillName + "|scale" + o.transform.localScale.x.ToString("F2") + "|max" + o.maxDamagedEnemies + "|dmg" + (o.damageArr != null && o.damageArr.Length > 0 ? o.damageArr[0] : -1)); return string.Join(" ; ", l.ToArray()); };
System.Action clearObjs = () => { foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject); };
System.Action<int> cast = (i) => { var w = Weapon.Equipped; PlayerHealthManager.Instance.nowStemina = 999; w.skill[i].skillCooldown.ResetCoolTime(); w.skill[i].isButtonDownSkill = true; log.Append("block=" + w.skill[i].BlockReason(i) + " "); try { w.ActiveWeaponSkill(i); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); } };
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => {
    var d = db.LoadItemData(db.FindItemDataIndex("GatekeepersOath"));
    box.LoadPrefab("GatekeepersOath", "Weapon", d.tfName);
}, false);
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => { try { var d = db.LoadItemData(db.FindItemDataIndex("GatekeepersOath"));
    amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
    SkillEffectService.AttachIds("Item:Weapon", d.effectIds, "U"); // 슬롯 아이템 없이 장착했으므로 직접 부착
    log.Append("effectIds=" + d.effectIds + " active=" + SkillEffectService.ActiveCount + " kind=" + Weapon.Equipped.Kind + "\n");
    for (int i = 0; i < Weapon.Equipped.skill.Length; i++) if (Weapon.Equipped.skill[i] != null) log.Append("skill" + i + "=" + Weapon.Equipped.skill[i].skillName + " ");
    log.Append("\n");
} catch (System.Exception ex) { log.Append("EX0 " + ex + "\n"); } }, false);
int gc = 1; // Gate Cleave 슬롯(프리팹 순서: Oath Bind, Gate Cleave, Stone Sentinels)
DG.Tweening.DOVirtual.DelayedCall(1.2f, () => { clearObjs(); var m = SkillEffectService.Query("Gate_Cleave", Weapon.Equipped.Kind); log.Append("## mods dmg=" + m.damagePct + " cd=" + m.cooldownPct + " rules=" + (m.rules == null ? 0 : m.rules.Length) + "\n"); cast(gc); }, false);
System.Func<string> variantName = () => { var sk = Weapon.Equipped.skill[gc]; var mi = typeof(Skill).GetMethod("Variant", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); var r = (GameObject)mi.Invoke(sk, new object[] { sk.skillprefab[0] }); return r.name + "|scale" + r.transform.localScale.x.ToString("F2") + "|max" + r.GetComponent<DamageObject>().maxDamagedEnemies; };
DG.Tweening.DOVirtual.DelayedCall(2.0f, () => { log.Append("## variant with effect: " + variantName() + "\n"); SkillEffectService.ClearAll(); }, false);
DG.Tweening.DOVirtual.DelayedCall(2.6f, () => { log.Append("## variant no effect: " + variantName() + "\n"); var d = db.LoadItemData(db.FindItemDataIndex("GatekeepersOath")); SkillEffectService.AttachIds("Item:Weapon", d.effectIds, "U"); }, false);
DG.Tweening.DOVirtual.DelayedCall(4.0f, () => { clearObjs(); var e = spawn(3f, 1); log.Append("## KillCast before=" + objs() + "\n"); e.damage2Enemy(new int[] { 5, 5, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, true, 0f, 1f); }, false);
DG.Tweening.DOVirtual.DelayedCall(4.2f, () => { log.Append("## KillCast after: " + objs() + "\n"); clearObjs(); SkillEffectService.ClearAll(); log.Append("DONE"); }, false);
return "scheduled";
