# 몬스터 스펙 (스탯 · AI · 스폰 · 드롭)

> 상태: 프리팹·씬 YAML 역산(2026-10-18). 프리팹은 `Assets/Prefab/Enemy_BD/*`에 있다.

## 1. 몬스터 목록

| 몬스터 | 시간대 | HP / Def | 성향 | 공격 (피해) | 추격 속도 (u / 0.1s) |
|---|---|---|---|---|---|
| Rabbit | 낮 | 50 / 1 | 비선공. 맞으면 도주 | 없음 | – |
| StoneRabbit | 낮 | 50 / **10** | 맞으면 반격 | Bite 근접 (20) | 0.25 |
| LeafMole | 낮 | 50 / 1 | 맞으면 반격 | 유도 잎 (30) | 0.15 |
| FlowerMole | 낮 | 50 / 1 | 선공 | 유도 꽃 (10) | 0.2 |
| RatThief | 밤 | 50 / 1 | 선공 | 찌르기 | 0.3 |
| RatChief | 밤 | 50 / 1 | 선공 | 찌르기 | 0.25 |
| Scarecrow | 밤 | 50 / 1 | 선공 | 화염 투사체 (10) | 0.15 |
| CreepingTerror | 밤 | 50 / 1 | 선공, 슈퍼아머 | 발밑 화염 소환 (10, 0.5s 지연) | 0.3 |
| **PassiveTerror (보스)** | 밤 | **1000** / 1 | 슈퍼아머, 거리 5~13 유지 | 원형 화염 3발, 할퀴기 (50) | 0.3 |
| Rock / Tree / GreenHurb | 상시 | 100 / 100 / 10 | 채집물 | – | – |

- 감지 범위는 전원 20×10 박스다.
- 피격 시 BT를 멈추고 흔들림 → 넉백 → 경직 후 다시 켠다. 슈퍼아머는 BT가 멈추지 않는다.
- HP바는 피격 후 2초간 표시한다. 보스는 화면 고정 HP바다.

## 2. AI (Behavior Designer)

```
Repeater → Selector
 ├ 전투: [피격 여부] → PlayerChecker(20x10) → 순찰 중단 → Selector
 │    ├ 벽 → 점프
 │    ├ 범위 이탈 → 빠져나감
 │    ├ 공격 범위 → Wait 0.5 → Attack(풀) → Wait
 │    └ 그 외 → Chase(0.1s마다 DOMoveX)
 ├ 벽 감지 → Jump(vy=10)
 ├ 낭떠러지 → 반대로 1~3u
 └ 순찰: Wait(2~3s) → Patrol(xMin~xMax, originMax 리쉬)   🐞 리쉬가 동작하지 않음
```

- Rabbit 도주 분기: 원점 복귀 → RunAway. RunAway는 끝나지 않는다(무한 Running).
- 보스 전용 노드: HP바 ON/OFF, 접촉 연출(1회), 플레이어 바라보기, 카메라 흔들림, 원형 탄막.

## 3. 스폰 — `EnemySpawner.cs`

- 스포너는 35개이고, 스포너마다 DropManager가 1개씩 있다.
- 플레이어가 스포너 영역 안에 있고 낮/밤 조건이 맞으면 1초 후부터 `cycle`초마다 빈 슬롯을 채운다.
- 기본 주기: 일반 10초, 채집물 3~30초, 보스 120초.
- 위치는 `enemyPositon` 자식 중 하나(랜덤 옵션)이고 x에 ±1 지터를 준다.
- 영역 이탈이나 시간대 불일치가 생기면 적이 1.5초 페이드아웃 후 풀에 반환된다. 이때 드롭은 없다.

| 맵 | 낮 | 밤 |
|---|---|---|
| Lv1_NearForest | Rabbit, StoneRabbit, LeafMole, 채집물 | RatThief, RatChief, Scarecrow |
| Lv1_NearForest_SideRoad | + FlowerMole, LeafMole | + CreepingTerror, **보스** |

## 4. 드롭 테이블 — `DropManager.cs`

- 항목마다 독립적으로 판정한다: `Random.value ≤ p × (1 + 드롭률/100)`
- 드롭 아이템은 위로 튀어오르고, 120초 뒤 사라진다. 땅에 닿은 뒤 인벤토리에 공간이 있으면 줍는다.

| 몬스터 | 재료 | 장비 |
|---|---|---|
| Rabbit | 가죽 10%, 작은 뼈 10%, 고기 15% | 0.1~0.2% (신발, 장갑, 하의, 녹슨 검, 나무 창, 네잎 반지) |
| StoneRabbit | 광석 15%, 둥근 뼈 10%, 고급 가죽 10%, 고기 15% | 모자/갑옷 0.2%, 토끼발 0.05% |
| LeafMole | 덩이줄기 15%, 잎 10%, 뿌리 10%, 약초 0.5% | Leaf 장비 0.1~0.2% |
| FlowerMole | 덩이줄기 20%, 잎/꽃잎/뿌리 10%, 약초 1% | Leaf 장비·반지 0.05~0.2% |
| RatThief | 쥐가죽 20%, 장물 10%, 쥐고기 10% | Rat 장비 0.2%, 쌍단검 0.1%, 반지 0.05% |
| RatChief | 쥐고기 15%, 전리품 10%, 질긴 가죽 10% | 독검 0.1%, 장신구 0.03~0.07% |
| Scarecrow | 검은 액체 15%, 가지 10%, 천 10% | Scare 장비 0.1~0.2% |
| CreepingTerror | 검은 액체 20%, 가지 10%, 천 10% | Scare 장비 0.1~0.2% |
| 보스 | – | Scare 방어구 각 20%, 무기 10%, 목걸이 5%, ScareSide 5% |
| 채집물 | 광석 / 나무 / 약초 100% | – |

## 4.1 정예·보스 보상 — 상세는 `ProgressionDesign.md` 1·2절

| 구분 | 판정 | 확정 보상 |
|---|---|---|
| 정예(중간보스) | 일반 스폰 시 `EnemySpawner.eliteChance`(4%, 정예 전용 스포너는 0) → HP×4·방어×1.5·크기×1.25·공격×1.5(몬스터에 붙은 공격만), 금빛 틴트·★ | 강화석 1~2 + 스포너 드롭표 장비 1 |
| 보스 | `isBossMob` + `Resources/Boss/<프리팹>.asset` | 세트 부위 1 + 강화석 3~6, 고유템 8%·12회 천장, 첫 처치 강화석 +5 |

- 공격 예고: `isAttack` 애니가 켜질 때 노란 ! + 붉은 깜빡임(`EnemyTelegraph`).
- Lv2 공격력은 `L2_*` 공격 프리팹(Lv2ContentPlan 3.3 구현 메모).

## 5. 확장 계획 (보유 에셋 활용)

`Game/AssetInventory.md`의 도트 몬스터 팩(Luiz Melo 시리즈, RPG Monster Pack)으로 **Lv2 사냥터 + 신규 세트 2종**을 만든다.
상세 기획은 확정 후 이 문서에 추가한다.
