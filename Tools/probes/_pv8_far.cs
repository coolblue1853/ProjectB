var pc = PlayerController.instance; var b = pc.transform.position; string s = "";
foreach (var e in EnemyHealth.Active) if (e.isBossMob) { pc.transform.position = new Vector3(e.transform.position.x - 45f, pc.transform.position.y, pc.transform.position.z); s += "dist=" + Vector2.Distance(pc.transform.position, e.transform.position).ToString("F0"); }
bool far = RecallCast.Begin("귀환 주문서"); s += " far Begin=" + far + " casting=" + RecallCast.IsCasting; if (far) RecallCast.Cancel("t");
pc.transform.position = b; return s;
