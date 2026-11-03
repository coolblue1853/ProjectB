// PV1 CF-D 몸박 검증: 일반 몹과 겹쳐 서서 HP 감소 측정 + 평타 중 유예. 결과 Logs/pv1/contact.log
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/contact.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
var pc = PlayerController.instance; var hm = PlayerHealthManager.Instance;
Time.timeScale = 1f; Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
string zone = "Lv1_NearForest_SideRoad";
MapInfo.GoToMap(zone, pc.gameObject);
EnemyHealth mob = null; float t0 = Time.time; int ph = 0; float phT = 0; int hp0 = 0, drops = 0, prevHp = 0; EnemyHealth eliteMob = null;
var drops2 = new System.Collections.Generic.List<int>();
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        float t = Time.time - t0;
        if (ph == 0)
        {   // 일반 몹 찾기
            if (t < 2f) { return; }
            if (t < 2.2f) { var mr = GameObject.Find("Map"); var mp0 = mr.transform.Find(zone); foreach (var sp in mp0.GetComponentsInChildren<EnemySpawner>(false)) { if (sp.isActiveAndEnabled && sp.enemyPositon != null) { var col = sp.GetComponent<Collider2D>(); var c = col != null ? (Vector3)col.bounds.center : sp.transform.position; pc.transform.position = new Vector3(c.x, c.y, 0f); break; } } return; }
            if (t < 7f) { return; }
            foreach (var e in EnemyHealth.Active) { if (e != null && e.gameObject.activeInHierarchy && e.IsMonster && !e.IsElite && !e.isBossMob && e.CanContact && e.ContactDamage > 0 && !e.isSuperArmor) { mob = e; break; } }
            if (mob == null) { if (t > 20f) { L("FAIL 일반 몹 못 찾음 active=" + EnemyHealth.Active.Count); UnityEditor.EditorApplication.update -= tick; } return; }
            L("MOB " + mob.name + " contactDamage=" + mob.ContactDamage + " scaled(expect)=" + ContactRules.ScaledDamage(mob.ContactDamage, false, false) + " fullHP=" + hm.fullHP + " enabled=" + ContactRules.contactEnabled);
            // 물약/무적 정리
            ph = 1; phT = Time.time; hp0 = hm.nowHp; prevHp = hp0; drops2.Clear(); return;
        }
        if (mob == null || !mob.gameObject.activeInHierarchy) { L("FAIL 몹 사라짐 ph=" + ph); UnityEditor.EditorApplication.update -= tick; return; }
        Vector3 mp = mob.transform.position; pc.transform.position = new Vector3(mp.x, mp.y + 0.05f, 0f);
        if (hm.nowHp < prevHp) { drops2.Add(prevHp - hm.nowHp); L("  hit @" + (Time.time - phT).ToString("F2", ic) + " ph=" + ph + " hp " + prevHp + "->" + hm.nowHp); }
        prevHp = hm.nowHp;
        if (hm.nowHp < hm.fullHP * 0.5f) { hm.nowHp = hm.fullHP; prevHp = hm.nowHp; }
        if (ph == 1 && Time.time - phT > 1.6f)
        {   // 가만히: 몸박 피해
            L("PH1 (공격 안 함, 1.6초 겹침): 피해 횟수=" + drops2.Count + " 값=" + string.Join(",", drops2.ConvertAll(x => x.ToString()).ToArray()) + " mobStillAlive=" + (mob.nowHP > 0) + " armor/def=" + hm.armorAtp);
            drops2.Clear(); ph = 2; phT = Time.time; return;
        }
        if (ph == 2)
        {   // 평타 연타 중에는 유예
            var w = Weapon.Equipped; if (w != null && w.CanMeleeNow) { w.MeleeAttack(); }
            if (Time.time - phT > 2.5f) { L("PH2 (평타 연타 2.5초 겹침): 피해 횟수=" + drops2.Count + " 값=" + string.Join(",", drops2.ConvertAll(x => x.ToString()).ToArray()) + " mobHP=" + mob.nowHP + " isAttackAnimating=" + pc.IsAttackAnimating); ph = 3; }
            return;
        }
        if (ph == 3) { L("DONE"); UnityEditor.EditorApplication.update -= tick; }
    }
    catch (System.Exception ex) { L("EXCEPTION " + ex.Message + "\n" + ex.StackTrace); UnityEditor.EditorApplication.update -= tick; }
};
UnityEditor.EditorApplication.update += tick;
return "pv1_contact started";
