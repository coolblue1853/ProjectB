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

- **레벨 성장(10-31):** Lv1~70 자동 성장 — 최대 HP +8/레벨(`fullHP` 증가분), 공격(`BasicDmg`)·방어(`Def`) 레벨당 +0.5(정수 누적: 2레벨마다 +1, PlayerStats Add 모디파이어 Id "Level"). 시작 기준은 지금 기본값(HP 200·방어 1·공격 0 + 장비) 그대로 두고 증가분만 얹는다. 규칙 `LevelRules`, 상태 `LevelSystem`.

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
final  = dmg × (1 + 최종뎀/100) × dmgRatio × 레벨차배율   (레벨차: 몹Lv − 플레이어Lv ≥ 4 이면 초과 1당 −3%, 최대 −30%. +3=0%, +4=−3%, +13=−30%. 몹 Lv 미지정=1. MISS 없음. `DamageCalculator.LevelGapMul`)
추가타 = final × 추가타%/100 (별도 숫자로 표시)
```

> ✅ 위 식이 현재 구현이다: `Script/Combat/DamageCalculator.cs` (순수 함수, 테스트 `Assets/Editor/Tests/DamageCalculatorTests.cs`). % 계산은 `StatMath`를 쓴다.

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
- **활공 (10-30):** 2층계(Lv2_*) 첫 진입 시 해금(ES3 `glide.unlocked`). 공중 하강 중 점프 키 0.2초 홀드 → 낙하 −1.3 u/s·수평 85%·최대 12초(착지 초기화, 상승 부하 부족 시 −10~20%), 짧은 누름은 기존 2단 점프. 상세 `Game/FloorCoreContent.md` 2절.
- **평타 중 이동 (10-26):** 무기 프리팹의 `isWeaponStopMove`가 켜져 있으면 평타 후딜 동안 MoveLock으로 멈춘다. 꺼진 무기는 후딜 동안 `_attackMoveSpeedMul`(기본 1) 배율로 걷거나 달린다(`Weapon.AttackMoveSpeedMul` → `PlayerController.Move`).
  - 녹슨 검(`RustedSword`, id 41)만 `isWeaponStopMove 0` + `_attackMoveSpeedMul 0.6`(걷기 3.0 → 1.8). 다른 한손검·단검·대검·둔기·창은 기존 설정 그대로다. 기존에 이미 정지 0이던 무기는 배율 1이라 속도 변화가 없다.
  - 배율은 `isAttackWait == false` 구간 전체(선딜 `delayTime` 포함, 공중 평타 포함)에 걸린다. 달리기 중이면 달리기 속도 × 0.6이고 달리기 스태미나 소모는 그대로다.
  - 스킬의 이동 정지는 `Skill.isWeaponStopMove`가 따로 정하므로 녹슨 검 스킬은 여전히 멈춘다.
- **공격 중 방향 잠금 (10-30, 던파식):** 평타·스킬은 공격 모션(`PlayerController.isAttackAnim`) 내내 시작 때 방향을 본다. 그동안 좌우 입력은 이동(평타 중 배율 규칙 그대로)만 하고 뒤돌지 않으며(로프 위 공격 포함), 모션이 끝난 다음 프레임에 `Move`가 입력 방향으로 돌려놓는다. 그래서 뒤로 걸으면서 앞 방향키+공격으로 잠깐 앞을 보고 치고 다시 뒤로 갈 수 있다(공격 중 몸은 뒤로 가도 얼굴은 앞).
  - 시작 방향: 평타(`Weapon.CreatAttackPrefab`, 데이터 검증 뒤)와 스킬(`Skill.ActiveSkill`, `CanActivate` 통과 뒤·오브젝트 생성 전)이 `PlayerController.FaceInputForAttack()`을 먼저 부른다. 좌우 입력이 있으면 그 방향, 없으면 현재 방향으로 시작한다(앞 공격이 잠근 방향이어도 새 공격은 입력 방향). 쿨·스태미나로 발동하지 않은 스킬 키는 돌리지 않는다. 대시·벽점프 중에는 돌지 않는다.
  - 공격 중 대시: `Dash()`가 시작할 때 입력 방향으로 돌고 남은 모션의 잠금을 푼다(잠긴 방향으로 거꾸로 대시하지 않게).
  - 예외: 무기 `_turnDuringAttack`, 스킬 `_turnDuringCast`(기본 꺼짐)를 켜면 예전처럼 시작 `_turnLockTime`(0.2초)만 고정한다. 현재 켠 무기·스킬은 없다.

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

### 임시 스킬 시전 FX 슬롯 (11-01)

- 첫 1시간 테스트용 그림 연결: `Assets/Art/Temp/FX/<스킬ID>.png` 또는 `<스킬ID>_f<N>.png`(1~64프레임 가로 시트). ID는 `Skill.skillName` 공백을 `_`로 치환하며 기존 철자·작은따옴표를 유지한다.
- `TempSkillFxImporter`가 정지 상태에서 Resources 카탈로그에 텍스처 참조를 등록한다. `TempSkillFxLibrary`는 36 PPU·중앙 피벗 클립을 만들고 기존 `Vfx` 풀로 1회 재생한다(단일 0.2초, 시트 20fps).
- 리뷰 보강(11-01): 임시 FX도 Vfx 공통 프레임당 6개 생성 제한을 따른다. 런타임 카탈로그의 null 항목·중복 ID·1~64 범위 밖 프레임 수도 기존 FX로 폴백한다.
- PNG는 시전 첫 피벗의 `castVfx`만 대체한다. 누락·중복·잘못된 시트는 기존 시전 효과로 폴백한다. 공격 판정·투사체·소환수·장판·기본 3타·스미어 v7은 기존 연결을 유지한다.
- 그림 제작·시각 판정은 Claude 담당. 전체 조사표·제작 주문: `Docs/Playtest/TempFxSlots.md`.

- **버프** — `PlayerBuff.cs`: `AttSpeed / Speed / HitCount / def` 문자열로 대상을 고르고, 지속 시간 뒤 원복한다. 🐞 DamageObject는 대문자 "Def"를 써서 표기가 다르다.
- **소환** — `SummonSkill.cs`, `SummonAttack.cs`: 플레이어 앞에 N기를 소환한다. 소환물은 주기 공격 후 사라진다.

## 8. 개선 방향 (MadDucks 참고)

| 항목 | 내용 |
|---|---|
| CharacterStats(Modifier) | static 필드를 대체한다. 장비 = Source, 세트 = GroupID, 버프 = ID로 관리해 착용·해제·만료 버그를 구조적으로 없앤다. |
| 데미지 레이어 문서화 | 위 4절을 `StatStackSystem`처럼 합/곱 분류표로 고정한다. |
| 샌드박스 씬 | 허수아비 + DPS 측정으로 장비·세트 조합을 검증한다. |

## 9. 던파식 전투 보강 (11-02)

> **원칙(사용자 11-02):** 우리는 완전 횡스크롤(메이플 뷰, 깊이축 없음)이다. 전투는 **메이플 + 던파 적당히**. 깊이축 이동·라인 회피·잡기·긴 띄우기 콤보처럼 벨트스크롤을 전제한 던파 요소는 넣지 않는다. 아래 공중 콤보·다운은 "이미 떠 있는 몹"에만 걸리는 짧은 보정으로 유지하고, 지상 평타가 몹을 띄우지는 않는다. 아티팩트 범위 피해가 전원 띄움→다운이 되는 것은 과하므로 다음 전투 손질 때 띄움 없이 넉백만으로 줄인다(임시 결정).

수치는 `CombatRules`(순수 함수, `CombatRulesTests`). 그림·모션·이펙트는 새로 만들지 않았다. 기본 3타·스미어 v7·셀 주인공(`_celArtWeapon`) 경로는 그대로.

| 항목 | 규칙 |
|---|---|
| 평타→스킬 캔슬 | 평타 후딜 중 `max(후딜×0.5, 판정 시점+0.05초)` 이후 스킬 키로 후딜을 끊는다(`Weapon.CanSkillCancelMelee`, 끊으면 `CancelMeleeRecovery`: 잠금 해제 + 콤보 유예 재시작). 타격 전에는 못 끊는다. 스킬→스킬은 기존대로 불가(스킬 `isCancleAttack` 켠 것만). 스킬→평타도 기존대로 불가(`isSkillCancel`만) |
| 입력 버퍼 | 경직(피격) 중 눌러 둔 평타·스킬 입력은 버린다. 스킬이 나가면 평타 선입력도 버린다. 활 차지 릴리스 버퍼는 그대로 |
| 경직 연쇄 | 1.5초 안에 이어 맞으면 경직 시간 ×1 / .75 / .5 / .35(하한). 저항: 보스 60%·trait `sturdy` 50%(이끼 껍질 딱정벌레)·정예 30%(최대값 하나). 슈퍼아머 몹은 경직 없음 |
| 슈퍼아머 표시 | 슈퍼아머 몹을 때리면 `ARMOR`(방어) 문자 팝업(0.5초에 한 번) |
| 공중 콤보 | 중력 있는 몹이 땅에서 떨어진 채 맞으면: 중력 ×0.35(타마다 +0.1, 7타쯤 1.0) · 수평 넉백 ×0.6 · 위로 4 u/s(타마다 −0.7)로 살짝 띄움. 중력 0 몹·슈퍼아머는 제외 |
| 띄움·다운 | 넉백 위쪽 속도 ≥3 이면 '띄움'. 경직이 끝나도 착지할 때까지 행동 트리를 안 켜고, 착지하면 다운 0.8초(누움) 뒤 복귀(안전 한도 3초). 다운 중 넉백은 수평·절반 |
| 다운 공격 | 다운된 적을 ↓ 를 누른 채 공격(기존 ↓ 키 활용, 새 키 없음)하면 피해 ×1.3, 다운 +0.3초 |
| 스킬 쿨다운 HUD | A·S·D·F 칸 위에 남은 초(10초 미만 소수 한 자리). 큰 창이 열리면(`HudPolicy.Hidden`) 숨김 |
| 스태미나 부족 | 스킬 키를 눌렀는데 스태미나가 모자라면 그 칸에 붉은 `STA` + 아래 중앙 `[키] Not enough stamina`(0.6초 제한, 1.1초 표시). MP 는 이 게임에 없다 |
| 히트 카운터 | 왼쪽 가운데 아래 작게 `N HIT` + 누적 피해 + 2초 유지 게이지(10 / 30 히트에서 색 변화). 맞으면 0. `HudPolicy.Hidden` 일 때 숨김. 우상단 처치 콤보(KillCombo)와 별개 |

- 아티팩트 범위 피해(`ArtifactSystem` 별폭발)는 11-02 띄움 없이 수평 넉백만으로 줄였다(`CombatRules.AreaBurstKnockDir`).
- 구현: `CombatRules`·`HitComboCounter`(순수), `EnemyAirState`(몹별 중력·착지·다운), `CombatHud`(히트·안내), `SkillCooldown` 라벨, `EnemyHealth.ApplyAirAndStagger/EndStiffness`.
