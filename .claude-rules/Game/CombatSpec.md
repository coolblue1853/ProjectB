# 전투 스펙 (플레이어 · 데미지 · 스킬)

> 상태: 코드 역산(2026-10-18). 🐞 표시는 `Game/TechDebt.md`에 등록된 버그다. 수치가 [Inspector]로 표시된 항목은 프리팹/씬 값이다.

## 1. 이동 — `Script/Player/PlayerController.cs`

| 항목 | 값 | 비고 |
|---|---|---|
| 걷기 | moveSpeed 10 | |
| 달리기 | runSpeed 10 (기본값이 걷기와 같음 → [Inspector] 확인 필요) | Run 토글(LShift), 주기마다 스태미나 소모 |
| 속도 버프 | `speed × (1 + SpeedBuff/100)` | 🐞 정수 나눗셈 |
| 점프 | jumpForce 13, 2단 점프 | velocity.y를 직접 대입 |
| 중력 | gravityScale 3 (대시·로프 중 0) | |
| 대시 | 속도 20, 0.2초, 쿨 2초, 무적, 스태미나 소모 | `nowStemina > dashStemina`일 때만 |
| 하향 점프 | ↓ + 점프 → 원웨이 플랫폼 통과 | |
| 사다리/로프 | ↑/↓로 붙음, 사다리 x로 스냅 | |
| 벽 점프 | 비활성(주석) | |
| 캐릭터 스케일 | 1.5, 좌우 반전은 localScale.x | |

## 2. 입력 — `InputSystem/KeyAction.inputactions`

| 동작 | 키보드 | 게임패드 |
|---|---|---|
| 이동 | ←/→ | 왼쪽 스틱 |
| 점프 | C | South |
| 대시 | Z | East |
| 달리기(토글) | LShift | North |
| 평타 | X | West |
| 스킬 A/S/D/F | A/S/D/F | LB/LT/RB/RT |
| 상호작용·사다리 | ↑/↓ | D-pad |

## 3. 플레이어 스탯 — `PlayerHealthManager.cs`, `DatabaseManager`(static)

| 스탯 | 기본 | 설명 |
|---|---|---|
| HP / 스태미나 | [Inspector] | 장비 hp는 최대치와 현재치를 함께 올린다 |
| 포만감 | 100 | 자연 회복의 재료 |
| 방어(Def) | 장비 합 | 받는 피해 = `floor(d² / (d + Def))` |
| 받는 피해 증가 | 0% | `d × (1 + incIncomingDmg/100)` 🐞 |

**자연 회복:**
- 마지막 피격 후 대기 시간이 지나면 주기마다 회복한다.
- HP 회복 1회에 포만감 3, 스태미나 회복 1회에 포만감 1을 소모한다. 포만감이 부족하면 회복이 멈춘다.

**피격:**
- 무적 시간 동안 알파가 깜빡인다.
- 넉백 후 경직된다. 슈퍼아머면 넉백 10%, 경직 없음.

## 4. 데미지 공식 (플레이어 → 적) — `EnemyHealth.cs`

무기 damage 배열(int[10]): `[0]min [1]max [2]치명% [3]치피% [4]증뎀% [5]방무% [6]스킬뎀% [7]최종뎀% [8]추가타% [9]armorAtp`

```
base   = Random(min + addBasicDmg, max + addBasicDmg)            🐞 max 미포함
crit   = rand(1..100) ≤ 치명% + 플레이어 치명
critM  = crit ? 1 + (20 + 치피% + 플레이어 치피)/100 : 1           (기본 치피 20%)
inc    = 1 + (무기 증뎀 + 장비 증뎀 + [출혈 대상이면 출혈 증뎀])/100   🐞 비치명 경로에 장비 증뎀 누락
dmg    = (base + armorAtp) × critM × inc
skill  = 스킬 공격이면 × (1 + 스킬뎀/100)
def    = 적 방어 × (1 - 방무/100)                                 🐞 현재 코드는 `× 방무/100`(반대 의미)
dmg    = dmg² / (dmg + def)
final  = dmg × (1 + 최종뎀/100) × dmgRatio
추가타 = final × 추가타%/100 (별도 숫자로 표시)
```

> 🐞 모든 `x/100`이 정수 나눗셈이라 100% 미만 보정은 0이 된다. 수정 후 위 식이 **목표 사양**이다.

**상태이상:**

| 종류 | 동작 |
|---|---|
| 독 | 주기마다 `poisonDamage + addPoisonDmg`를 N회. 방어를 무시하는 고정 피해. 걸 때마다 중첩(인스턴스 추가) |
| 출혈 | 독과 같은 구조. 걸려 있는 동안 출혈 증뎀이 적용된다. 장비 출혈은 공격 오브젝트 생성 시 확률 판정 |

속성(element)은 없다.

## 5. 평타 콤보 — `Weapon.cs`

- X를 누를 때마다 콤보 단계가 1 → maxComboCount로 오르고 순환한다. maxComboTime 동안 입력이 없으면 리셋된다.
- 단계별 데이터는 배열로 정의한다: `attckSpeed[]`, `delayTime[]`, `attackAnimName[]`, `attackPrefab[]`, `weaponSound[]`
- 다음 입력까지 대기: `(attackSpeed + delay) / (1 + 공속/100)` 🐞
- **캔슬 규칙:**
  - 평타는 스킬 후딜 중에 캔슬 가능 옵션(isSkillCancel)이 있다.
  - 스킬은 평타 대기 중에 캔슬 가능 옵션(isCancleAttack)이 있다.

## 6. 스킬 — `Skill.cs`, `SkillCooldown.cs`, `DamageObject.cs`

- **발동 조건:** 쿨다운이 아님, 스태미나 > 소모량, 캔슬 조건 충족
- **발동 방식:**

  | 방식 | 동작 |
  |---|---|
  | Normal | 일반 발동 |
  | Round | 부채꼴로 bulletCount발 |
  | Ground | 바닥 레이캐스트 위치에 생성 |
  | 다단 | interval[]마다 프리팹을 순차 생성 |

- **선딜/홀드:** 선딜 동안 지연 이펙트를 표시한다. 홀드 스킬은 키를 떼면 즉시 종료하고 쿨다운을 시작한다.
- **쿨다운:** `cooldown × (1 - 장비 쿨감[스킬]/100)` (float, 정상)
- **장비 보정:** 다단 수 +skillHitCount, 탄 수 +bulletCount
- **DamageObject 옵션:** 다단히트 주기, 최대 타격 수, 발사체, 유도, 흡입, 힐 장판, 지속 버프, 대시 공격

## 7. 버프 · 소환

- **버프** — `PlayerBuff.cs`: `AttSpeed / Speed / HitCount / def` 문자열로 대상을 고르고, 지속 시간 뒤 원복한다. 🐞 DamageObject는 대문자 "Def"를 써서 표기가 다르다.
- **소환** — `SummonSkill.cs`, `SummonAttack.cs`: 플레이어 앞에 N기를 소환한다. 소환물은 주기 공격 후 사라진다.

## 8. 개선 방향 (MadDucks 참고)

| 항목 | 내용 |
|---|---|
| CharacterStats(Modifier) | static 필드를 대체한다. 장비 = Source, 세트 = GroupID, 버프 = ID로 관리해 착용·해제·만료 버그를 구조적으로 없앤다. |
| 데미지 레이어 문서화 | 위 4절을 `StatStackSystem`처럼 합/곱 분류표로 고정한다. |
| 샌드박스 씬 | 허수아비 + DPS 측정으로 장비·세트 조합을 검증한다. |
