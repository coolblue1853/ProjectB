var pc = PlayerController.instance; string name = "__N__"; var sb = new System.Text.StringBuilder();
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude)) { if (sp.name != name) continue; var col = sp.GetComponent<Collider2D>(); var p = col != null ? (Vector3)col.bounds.center : sp.transform.position; pc.transform.position = new Vector3(p.x, p.y, 0); sb.Append("tp " + p + " "); break; }
PlayerHealthManager.Instance.nowHp = PlayerHealthManager.Instance.fullHP;
return sb.ToString();
