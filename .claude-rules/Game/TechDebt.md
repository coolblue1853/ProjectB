# 버그 · 기술 부채 백로그

> 우선순위: P0 = 게임 진행 불가·데이터 손실, P1 = 핵심 수치/기능 오류, P2 = 잠재 크래시·누수, P3 = 정리.
> 수정하면 상태를 ✅로 바꾸고 커밋 해시와 **검증 방법**(에디터 플레이·프로브)을 적는다.

## P0

| ID | 내용 | 위치 | 상태 |
|---|---|---|---|
| B-001 | 적 사망 시 `ReleaseObject()`만 호출 → OnReleased 미발생 → **드롭·시체·리스폰 전부 안 됨** (b05df50a 회귀) | `EnemyHealth.cs:237-245, 298-306`, `EnemySpawner.OnEnemyReleased` | ✅ EnemyHealth.TryDie 단일 경로 (프로브: 처치→드롭·슬롯 비움·10초 후 리스폰) |
| B-002 | `ClearAllPools`의 `while(CountActive>0){Get();Release();}` 무한 루프 의심 → 포털 이동 시 멈춤 | `EnemySpawner.cs:285-297` | ✅ ClearAllPools 재작성: 대여 목록 추적 후 반환 (프로브: 포털 복귀 정상) |
| B-003 | 제작 시 표시명(itemNameT)으로 생성 → 키 불일치 NRE, 재료는 이미 차감 | `CraftingManager.cs:184` | ✅ 아이템 키로 생성 + 공간 확인 후 재료 차감 |
| B-004 | 인벤토리 수량 캐시 불일치 → 창고에 둔 재료로 공짜 제작 | `ItemDrag.cs:131-149`, `InventoryManager.UseEquipment/I2CMove` | ✅ 수량은 슬롯 직접 집계(CountItem), 캐시는 0.25s 재계산 |
| B-005 | 장비를 드래그로 해제하면 세이브 미갱신 → 재접속 시 아이템 복제 | `ItemDrag.cs:123-128` | ✅ 드래그 해제 시 SaveEquipItem(false)·장착 목록 해제 |
| B-006 | 창고 → 인벤토리 이동 중 인벤토리가 가득 차도 창고 수량 차감 → 소실 | `Chest.C2IMove` | ✅ CreatItem 성공 시에만 창고 차감 |
| B-007 | 시작 지점(originPos) 미할당 → SaveManager.Awake 예외 | `SaveManager.cs` | ✅ 0c338134 (플레이 모드 에러 0) |

> 인벤토리 신규 버그 B-008~B-023(창고→인벤 이중 차감, 나누기 소실, 목걸이 오타 NRE, 장비 드래그 교환, 티어·강화 소실, 강제종료 복제 등)은 `Game/InventoryRefactorDesign.md` 1절 표에 상세(파일:라인)가 있다. 처리하면 여기에도 ✅로 옮긴다.

| ID | 내용 | 상태 |
|---|---|---|
| B-008 | 창고→인벤(인벤 가득) 이중 차감·공짜 1개 (P0) | ✅ C2IMove 단일 경로 — 프로브 c2i_full_p1/p2 |
| B-009 | 나누기 시 빈칸 없으면 수량 소실 (P0) | ✅ 생성 성공 시에만 원본 감소 |
| B-010~015 | 칸 겹침 ✅, Neckles 오타 ✅, 장비 드래그 교환 ⏳, 페이지탭 드롭 ⏳, 티어·강화 소실 ✅(이동·나누기·해제·창고 경로에 ItemData 복사 전달, 스택 병합은 이름 비교 유지), 강제종료 복제 ✅(이동 시 양쪽 저장) (P1) | 🔶 |
| B-016~023 | 툴팁 부호, Resources.Load null, 외상 구매, 우선순위, int.Parse, 디버그 F키, 과도한 저장, name 가림 (P2~P3) | ⏳ |

## P1

| ID | 내용 | 위치 | 상태 |
|---|---|---|---|
| B-101 | `% / 100` 정수 나눗셈 → 공속·이속·받피증·치피·증뎀·스킬뎀·방무·최종뎀이 100% 미만이면 무효 | 다수 (CombatSpec 4절) | ✅ StatMath/DamageCalculator로 /100f 통일 — EditMode 테스트 10/10 |
| B-102 | 방무 공식 반대 의미: `def × 방무/100` → `def × (1 - 방무/100)` | `EnemyHealth.cs:190-199` | ✅ 실효 방어 = def×(1-방무%) — EditMode 테스트 10/10 |
| B-103 | `Random.Range(int,int)` 최대값 미포함 | `EnemyHealth.cs:139` | ✅ Lerp(min,max,roll) 양끝 포함 — EditMode 테스트 10/10 |
| B-104 | 비치명타 경로에 장비 증뎀 누락 | `EnemyHealth.cs:159,163` | ✅ 장비 증뎀 항상 적용 — EditMode 테스트 10/10 |
| B-105 | `nomalizedHP = 1/fullHP` 정수 → 0 | `PlayerHealthManager.cs:149+` | ✅ 죽은 정규화 계산 제거 |
| B-106 | 장비 해제 시 `HpDamage(fullHP - nowHp)`로 오히려 회복·이상 동작 | `PlayerHealthManager.cs:164-166` | ✅ 최대 HP 감소 시 초과분만 깎음 |
| B-107 | 같은 이름 출혈 장비 2개 장착 시 Dictionary.Add 예외 | `EquipBoxCheck.cs:167` | ✅ 인스턴스별 출혈 키 |
| B-108 | 중복 재료 레시피 판정 오류(Ring×6) / 제작 시 공간 미확인 | `CraftingManager.cs:641-661` | ✅ 재료 합산 판정 + 공간 확인, DeletItemByName 일괄 차감 |
| B-109 | 풀 재사용 시 HP바 이벤트 중복 구독 → 데미지바 여러 개 | `EnemyHealthBar.cs:24-25` | ✅ 재구독 전 해제 |
| B-110 | 순찰 리쉬 무효(originPosition을 매번 덮어씀) | `EnemyPatrol.cs:21` | ✅ 리쉬 기준 = 스폰 위치(스포너가 지정) — 거리 제한 효과는 추가 관찰 필요 |
| B-111 | SideRoad 포털 연결 끊김 / openMap null NRE | 씬 `Portal` | ✅ Lv1을 SideRoad(확장판)로 교체, 이동 전용 포털 null-safe (Q-104) |
| B-112 | 세트 키가 빈 문자열 `""`로 쌓임, MinusSetDict KeyNotFound | `EquipBoxCheck.cs:246`, `DatabaseManager.cs:71` | ✅ 빈 tfName 무시, MinusSetDict TryGetValue |
| B-113 | 홀드 스킬이 다른 스킬 키를 떼도 종료됨 | `Skill.cs:96-100, 297` | 🔶 1차: 자기 키 해제만 처리·null 가드. 나머지 원인(동시 발동, 슬롯/skillNum)은 PlayerStateDesign Phase 0 |
| B-114 | 무적·슈퍼아머·이동정지 bool을 여러 출처가 덮어씀 (대시 종료가 다른 무적 해제) | `PlayerController.cs:602`, `DamageObject.cs:183` | 🔶 무적·슈퍼아머 토큰화 ✅(PlayerStatus, 프로브 dash_overlap). 이동정지·사다리 잠금 ✅(Weapon/Skill 토큰, 프로브 lock_overlap), UI 잠금(Phase 4) ⏳ |
| B-115 | 타일맵 타일 사이에 틈(세로·가로 선)이 보이는 고질 버그 (사용자 보고 2026-10-18) | 타일맵·아틀라스·카메라 | 🔶 1차 조치(2026-10-18, `fix_tile_bleed.cs`): LV1 시트 7장을 셀마다 4px extrude 패딩으로 재생성(rect 이동·spriteID 유지 → 타일 참조 그대로), 원본 해상도(TileMap1이 768→512로 축소 임포트되던 것 수정)·무압축·FullRect. 스크린샷상 틈 없음 — 카메라 이동 중 재발은 관찰 필요. 남은 후보: 아틀라스 패딩/블리딩 없음, 스프라이트 Mesh Type Tight, 필터 Bilinear·압축, 픽셀 퍼펙트 미스냅. 조치 후보: SpriteAtlas 패딩 4 + Extrude, Full Rect, Point·무압축, Pixel Perfect Camera Upscale RT 또는 Grid gap −0.001 |
| B-116 | 너클 계열 Sway Back/Hoof Step 사용 시 콘솔 에러 "No AnimCip : BackDash" — 플레이어 AnyPortrait에 BackDash 애니가 없음(스킬 skillAnim) | `Leather Knuckles`/`TuskKnuckles` 프리팹 RSkill | ✅ skillAnim 을 같은 무기 대시 애니(Dash)로 교체 (`fix_backdash_anim.cs`), 스킬 3종 발동 에러 0 |
| B-117 | ✅ 스포너 영역이 겹친 곳에서 몬스터가 **다른 스포너**의 트리거 경계를 넘으면 `OnTriggerExit2D`가 `DisaperByTime`을 불러 사라짐(깊은 숲 보스가 등장 직후 증발) → 자기 소속(`enemySpowner == this`)일 때만 사라지게 (2026-10-19) | `EnemySpawner.cs` |
| B-118 | ✅ Lv2 카메라 경계 4개가 90° 회전된 템플릿 복제라 Top/Bottom 이 세로 막대(x −300)가 돼 카메라가 x −311 에서 멈춤(보스 구역이 화면 밖) → 회전 초기화, `Lv2MapBuilder` 도 수정 (2026-10-19) | `Lv2MapBuilder.cs`, 씬 `Lv2_MagicCliffs/Cam`, `Lv1_Swamp/Cam` |
| B-119 | ✅ 깊은 숲 귀환 포털 `Portal_DeepToNormal` 이 원본 근교 맵의 비활성 포털을 이어받아 꺼져 있었음(TravelNow 직접 호출 테스트로는 안 보임) → 켬 (2026-10-19) | 씬 `Lv1_DeepForest/Portal`, `build_swamp.cs` |
| B-120 | 🔶 높이 4 발판 스포너 영역 바닥이 발판(−174)과 같아 발판에 선 플레이어를 못 잡음 → 늪지는 영역을 아래로 2 늘림. **Lv2 원본(L2_B_ShroomUp·L2_B_GoblinN)은 미수정** | 씬 `Lv2_MagicCliffs`, `Lv2MapBuilder.cs` |

## P2

| ID | 내용 | 위치 |
|---|---|---|
| B-201 | ✅ DisaperByTime 재진입 → 이중 Release 예외 (PoolAble 이중 반환 가드 + 시퀀스 OnDisable Kill) | `EnemyHealth.cs` |
| B-202 | ✅ `CountInactive==0`이면 Destroy하는 패턴 → 풀 붕괴 (B-001의 실제 원인) | EnemyHealth, DeadBody, DropItemCheck |
| B-203 | ✅ DropItemCheck의 120초 Invoke 미취소 → 재사용 아이템 조기 소멸 | `DropItemCheck.cs:26` |
| B-204 | ✅ AttackSpawon이 풀 오브젝트 Destroy / Start에서만 예약 | `AttackSpawon.cs` |
| B-205 | DOTween 시퀀스 미정리 → 파괴 후 콜백 MissingReference | `DamageObject.cs` 다수 |
| B-206 | ✅ 독·출혈이 Destroy 후 Invoke 재호출 | `PosionAttack.cs`, `BleedingAttack.cs` |
| B-207 | 각종 null 체크 누락(GetOb, bt.sequence, skill[0], enemyHealth) | 다수 |
| B-208 | 리스너 중복 등록(sellSlider) / 0 나누기(buyprice) / 연산자 우선순위 | ShopManager, DivideSlider |
| B-209 | CSV 파서: 빈 줄·따옴표 미처리, placeholder 행 로드 | `ItemDatabase.cs` |
| B-210 | `new SetItem()`(MonoBehaviour new) | DatabaseManager, InventoryManager |
| B-211 | 코드 생성 머티리얼이 `Shader.Find`(Sprites/Default, Legacy Particles/Additive)에 의존 → 빌드에서 셰이더가 빠질 수 있음. Always Included 등록 또는 Resources 머티리얼로 교체 필요 | `EmitParticles`, `DropLightPillar` |
| B-212 | ✅ 방어구 `armor`가 `playerDef`에 더해지지 않던 문제 → `EquipBoxCheck.SetEquipment`에서 `armor × 강화배율 × 0.5(Equipment.ArmorToDef)` 적용. 비율은 몬스터 공격력 조정(Lv2 튜닝)과 함께 재검토 | `EquipBoxCheck.cs`, `Equipment.cs` |

## P3 (정리)

- 디버그 키 제거: SaveManager의 Space 키, TimeChange의 0 키, testFunc의 F1 키.
- 매 프레임 할당·조회 줄이기: Play 애니 재호출, LayerMask.GetMask, GetComponent, FindWithTag, Debug.Log.
- 오타 정리: Stemina, Posion, Destory, Weqpon, Spowner 등. 직렬화 필드는 `FormerlySerializedAs`로 이름을 바꾼다.
- 빈 클래스와 중복 코드 정리: ShopItemDrag, ShopCostUI, Equip/WeaponSmithUI, CameraResolution, HealthSystem 데모.
- 벤더 코드 수정분 분리: Behavior Designer BehaviorTree.cs.
- AnyPortrait Unity 6 경고: 새 버전으로 업데이트한다(캐시에 패키지 있음).
