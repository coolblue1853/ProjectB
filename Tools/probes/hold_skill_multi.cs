// 프리팹 여러 개 홀드 스킬: 첫 생성 뒤, 순차 생성(SpawnSkills) 도중 키를 떼면 이후 생성이 멈추는지
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon; var hold = w.skill[2]; var cd = hold.skillCooldown;
cd.isCooldown[2] = false;
var pf = hold.skillprefab; var pv = hold.skillPivot; var iv = hold.interval;
hold.skillprefab = new GameObject[] { pf[0], pf[0], pf[0], pf[0] };
hold.skillPivot = new GameObject[] { pv[0], pv[0], pv[0], pv[0] };
hold.interval = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D));
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => { Debug.Log("[probe-multi] before release dmgObjs=" + UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None).Length + " holding=" + hold.IsHolding); UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); }, true);
DG.Tweening.DOVirtual.DelayedCall(6.0f, () => {
    Debug.Log("[probe-multi] 6s after dmgObjs=" + UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None).Length + " holding=" + hold.IsHolding);
    hold.skillprefab = pf; hold.skillPivot = pv; hold.interval = iv;
}, true);
return "ok";
