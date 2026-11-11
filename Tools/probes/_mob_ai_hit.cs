var go = GameObject.Find("SporeShroom_BD"); if (go == null) return "gone";
go.GetComponent<EnemyHealth>().Damage(1);
return "hit";
