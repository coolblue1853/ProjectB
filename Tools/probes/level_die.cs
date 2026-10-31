// EXP 1000 으로 맞추고 강제 사망 → 5% 감소 확인
LevelSystem.SetState(LevelSystem.Level, 400);
int before = LevelSystem.Exp;
var phm = PlayerHealthManager.Instance;
phm.HpDamage(phm.nowHp);
return "before=" + before + " (사망은 다음 프레임에 처리)";
