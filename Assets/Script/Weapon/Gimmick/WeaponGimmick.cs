using UnityEngine;

/// <summary>
/// 무기 계열(WeaponTypes.md). ItemSheet 맨 뒤 WeaponType 컬럼 문자열 = 열거형 이름.
/// 주의: int 로 직렬화될 수 있으니 기존 값의 순서를 바꾸지 않는다. 새 값은 반드시 맨 뒤에 추가한다.
/// </summary>
public enum WeaponKind
{
    None = 0,
    Sword, Greatsword, Scythe, Dagger, Knuckle, Claw, Spear, Adae, Staff,
    Axe, Mace, Whip, Flail, Dual, Tachi, Bow, Crossbow, Tome, Javelin, Bulwark,
}

/// <summary>판정 오브젝트가 적을 때리기 직전, 이 타격의 피해 배율을 바꾸는 계열 기믹</summary>
public interface IHitModifier
{
    float HitMultiplier(DamageObject hitbox, EnemyHealth enemy, Collider2D enemyCollider);
}

/// <summary>
/// 무기 계열 기믹 기반 클래스 (WeaponTypes.md 4절). 무기 프리팹 루트(Weapon 옆)에 붙는 작은 컴포넌트.
/// Weapon 이 평타 시작·판정 생성 때 부른다. 판정 프리팹은 공유되므로 기믹은 판정 인스턴스(생성본)만 고친다.
/// </summary>
public abstract class WeaponGimmick : MonoBehaviour, IHitModifier
{
    /// <summary>평타가 실제로 시작될 때(단계 step = 1..)</summary>
    public virtual void OnAttackStart(Weapon weapon, int step) { }

    /// <summary>평타 판정 인스턴스가 만들어진 직후 — 크기·경직·피해 배열을 이 인스턴스에 한해 고친다</summary>
    public virtual void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox) { }

    /// <summary>타격 직전 피해 배율(1 = 변화 없음). OnHitboxSpawned 에서 hitbox.AddHitModifier 로 등록한 기믹만 불린다</summary>
    public virtual float HitMultiplier(DamageObject hitbox, EnemyHealth enemy, Collider2D enemyCollider)
    {
        return 1f;
    }
}
