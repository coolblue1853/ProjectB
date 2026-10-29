// B-113 2.4-6: 홀드 중 대시(가상 Z)·피격 경직(damage2Player)으로 홀드가 끝나는지. 먼저 equip_weapon(CliffBlade) + input_bg_on, 세이브 백업·복원
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon; var hold = w.skill[2]; var cd = hold.skillCooldown;
var kb = UnityEngine.InputSystem.Keyboard.current;
cd.isCooldown[2] = false;
float cdt = cd.cooldownTime[2]; cd.cooldownTime[2] = 0.5f;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D));
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => {
    Debug.Log("[probe-cancel] A holding=" + hold.IsHolding);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D, UnityEngine.InputSystem.Key.Z));
}, true);
DG.Tweening.DOVirtual.DelayedCall(1.6f, () => {
    Debug.Log("[probe-cancel] A after dash (D still down) holding=" + hold.IsHolding + " stamina=" + PlayerHealthManager.Instance.nowStemina + " cd=" + cd.isCooldown[2] + " skillWait=" + w.isSkillAttackWait + " dmgObjs=" + UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None).Length);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
}, true);
DG.Tweening.DOVirtual.DelayedCall(3.5f, () => {
    cd.isCooldown[2] = false;
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D));
}, true);
DG.Tweening.DOVirtual.DelayedCall(3.9f, () => {
    Debug.Log("[probe-cancel] B holding=" + hold.IsHolding + " superArmor=" + DatabaseManager.isSuperArmor + " invincible=" + DatabaseManager.isInvincibility);
    PlayerHealthManager.Instance.damage2Player(1, 0.3f, 1f, Vector2.left, PlayerController.instance.transform.position.x + 1f, false);
}, true);
DG.Tweening.DOVirtual.DelayedCall(4.4f, () => {
    Debug.Log("[probe-cancel] B after hit (D still down) holding=" + hold.IsHolding + " attacked=" + PlayerController.instance.isAttacked + " cd=" + cd.isCooldown[2]);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
    cd.cooldownTime[2] = cdt;
}, true);
return "ok";
