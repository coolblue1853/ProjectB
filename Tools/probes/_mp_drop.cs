// 낙하물(드롭) 안착 점검: 구역마다 지면 위 여러 x 에서 골드를 떨어뜨려 3초 뒤 gravityScale==0(착지)·지면 위인지 확인 → drop.txt
var pc = PlayerController.instance; DropManager dm = null;
string[] zones = { "Lv1_NearForest","Lv1_NearForest_SideRoad","Lv1_NormalForest","Lv1_DeepForest","Lv1_Swamp","Lv1_BanditFortress","Lv1_TowerOutskirts","Lv1_MistSwamp","Lv1_DeepForestCore","Town" };
int mask = LayerMask.GetMask("Ground");
var sb = new System.Text.StringBuilder(); int zi = -1, st = 0; double until = 0; var drops = new System.Collections.Generic.List<(GameObject go, Vector2 from)>();
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
    PlayerStatus.AcquireFor(StatusFlag.Invincible, 30f, "mp");
    if (st == 0) { zi++; if (zi >= zones.Length) { System.IO.File.WriteAllText("Logs/playtest/mapphys/drop.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
        MapInfo.GoToMap(zones[zi], pc.gameObject); until = now + 3; st = 1; return; }
    if (st == 1) {
        var map = GameObject.Find("Map").transform.Find(zones[zi]); Bounds gb = new Bounds(); bool any = false;
        foreach (var c in map.GetComponentsInChildren<Collider2D>(false)) { if (c.isTrigger || ((1 << c.gameObject.layer) & mask) == 0) continue; if (!any) { gb = c.bounds; any = true; } else gb.Encapsulate(c.bounds); }
        dm = map.GetComponentInChildren<DropManager>(true); drops.Clear(); int n = 0; if (dm == null) { sb.AppendLine("== " + zones[zi] + " no DropManager"); st = 0; until = now + 0.3; return; }
        for (int i = 0; i < 24; i++) {
            float x = Mathf.Lerp(gb.min.x + 2, gb.max.x - 2, (i + 0.5f) / 24f);
            var h = Physics2D.Raycast(new Vector2(x, gb.max.y + 3), Vector2.down, gb.size.y + 10, mask);
            if (h.collider == null) continue;
            var go = dm.GetGo("Gold"); if (go == null) continue; var p = new Vector3(x, h.point.y + 2.5f, 0);
            go.transform.position = p; var rb = go.GetComponent<Rigidbody2D>(); rb.gravityScale = 1; rb.linearVelocity = Vector2.zero;
            go.GetComponent<DropItemCheck>().SetGold(10); rb.AddForce(new Vector2(UnityEngine.Random.Range(-0.6f, 0.6f), 1f) * dm.impulseForce, ForceMode2D.Impulse);
            drops.Add((go, new Vector2(x, h.point.y))); n++;
        }
        sb.AppendLine("== " + zones[zi] + " dropped " + n); until = now + 4; st = 2; return; }
    if (st == 2) {
        int bad = 0;
        foreach (var d in drops) { var go = d.go; var rb = go.GetComponent<Rigidbody2D>(); float gy = go.transform.position.y - d.from.y;
            bool landed = rb.gravityScale == 0; if (!go.activeSelf || !landed || gy < -0.5f || gy > 2.0f) { bad++; sb.AppendLine("  BAD x=" + d.from.x.ToString("F1") + " groundY=" + d.from.y.ToString("F1") + " now=" + go.transform.position.ToString("F1") + " gs=" + rb.gravityScale + " active=" + go.activeSelf); } }
        sb.AppendLine("  bad=" + bad + "/" + drops.Count); foreach (var d in drops) if (d.go.activeSelf) d.go.GetComponent<DropItemCheck>().ReleaseObject(); st = 0; until = now + 0.3; }
};
UnityEditor.EditorApplication.update += upd; return "started";
