// 홀드 시간 만료: 키를 누른 채 만료 → 홀드 종료·데미지 오브젝트 파괴·자기 슬롯 쿨
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon; var hold = w.skill[2]; var cd = hold.skillCooldown;
cd.isCooldown[2] = false;
float ht = hold.holdingTime; hold.holdingTime = 0.6f; bool byTime = hold.holdDisEffectByTime; hold.holdDisEffectByTime = true;
float cdt = cd.cooldownTime[2]; cd.cooldownTime[2] = 30f;
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D));
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Debug.Log("[probe-exp] 0.3s holding=" + hold.IsHolding + " dmgObjs=" + UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None).Length + " prefabs=" + hold.skillprefab.Length), true);
DG.Tweening.DOVirtual.DelayedCall(3.0f, () => {
    Debug.Log("[probe-exp] t=" + Time.time + " ts=" + Time.timeScale + " 3.0s (key still down) holding=" + hold.IsHolding + " dmgObjs=" + UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None).Length + " cd=" + cd.isCooldown[2] + " skillWait=" + w.isSkillAttackWait + " moveLock=" + PlayerStatus.Has(StatusFlag.MoveLock));
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
    hold.holdingTime = ht; hold.holdDisEffectByTime = byTime; cd.cooldownTime[2] = cdt;
}, true);
return "ok";
