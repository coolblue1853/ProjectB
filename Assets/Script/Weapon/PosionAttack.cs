using DarkTonic.MasterAudio;

/// <summary>독: 방어 무시 고정 피해. 장비 독 피해(addPoisonDmg)가 매 틱 더해진다. 여러 개가 중첩된다.</summary>
public class PosionAttack : DotEffectBase
{
    public bool isBleeding = false; // 직렬화 호환용 (미사용)

    protected override int TickDamage(int baseDamage) => baseDamage + DatabaseManager.addPoisonDmg;

    protected override void OnTick()
    {
        if (!MasterAudio.IsSoundGroupPlaying("Poison"))
        {
            MasterAudio.PlaySound("Poison");
        }
    }
}
