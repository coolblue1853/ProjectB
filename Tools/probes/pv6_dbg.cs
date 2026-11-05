var pc = PlayerController.instance; var phm = pc.GetComponentInChildren<PlayerHealthManager>(); var sb = new System.Text.StringBuilder();
sb.AppendLine("inv=" + DatabaseManager.isInvincibility + " pcTag=" + pc.tag + " phmTag=" + phm.tag + " phmPath=" + phm.transform.parent.name + " childIdx=" + phm.transform.GetSiblingIndex() + " hp=" + phm.nowHp + "/" + phm.fullHP + " status=" + PlayerStatus.Has(StatusFlag.Invincible));
foreach (var c in pc.GetComponentsInChildren<Collider2D>()) sb.AppendLine("col " + c.name + " tag=" + c.tag + " trig=" + c.isTrigger + " layer=" + c.gameObject.layer);
sb.AppendLine("pos=" + pc.transform.position + " noDmg=" + typeof(PlayerHealthManager).GetField("isNoDamge", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance)?.GetValue(phm));
return sb.ToString();
