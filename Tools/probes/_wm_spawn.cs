var pc = PlayerController.instance;
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
float gy = GroundProbe.Y(pc.transform.position + Vector3.right * 3f);
var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + 3f, gy + 0.6f, 0f), Quaternion.identity);
var e = go.GetComponent<EnemyHealth>();
var rb = go.GetComponent<Rigidbody2D>();
e.maxHP = 100000; e.nowHP = 100000;
return "spawned " + go.name + " at " + go.transform.position + " pc=" + pc.transform.position + " gy=" + gy + " grav=" + (rb != null ? rb.gravityScale.ToString() : "null") + " bt=" + e.IsMonster + " superArmor=" + e.isSuperArmor + " def=" + e.enemyDef + " resist=" + e.StaggerResist;
