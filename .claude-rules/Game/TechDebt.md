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
| B-010~015 | 칸 겹침 ✅, Neckles 오타 ✅, 장비 드래그 교환 ✅·페이지탭 드롭 ✅(장비칸에서 시작한 드래그는 빈 인벤 칸만 허용, 차 있는 칸·창고 칸(옮기면 해제 없이 복제되던 경로)·페이지 탭·아무 칸도 아닌 곳(드래그 캔버스에 떠 있던 문제)이면 장비칸으로 되돌림 — `ItemDrag.DropFromEquipNotAllowed`, 프로브 `equip_drag_guard.cs`), 티어·강화 소실 ✅(이동·나누기·해제·창고 경로에 ItemData 복사 전달, 스택 병합은 이름 비교 유지), 강제종료 복제 ✅(이동 시 양쪽 저장) (P1) | ✅ |
| B-016 | 툴팁 음수 부호 중복 (P2) | ✅ D3 `TooltipBuilder`(부호 한 번) |
| B-017 | 툴팁 `Resources.Load` 매번·null NRE (P2) | ✅ `GearPrefabCache`(이름별 1회 로드, 없음도 기억) + `WeaponBlock/ArmorBlock/CheckSkillDetail` null 안전 — 인벤·창고·제작·상점 툴팁 |
| B-018 | 상점 외상 구매·가격 0 나누기·슬라이더 리스너 누적 (P2) | ✅ `ShopManager.MaxAffordable`, 구매 루프에서 돈 재확인·넣은 것만 차감, 슬라이더 4곳 Remove 후 Add |
| B-019 | `triggered \|\| (…) && 범위` 우선순위 (P2) | ✅ 인벤·나누기·창고 나누기·상점 슬라이더 8곳 괄호 |
| B-020 | 세이브 로드 `int.Parse` 크래시 (P2) | ✅ 인벤·창고는 SaveMapper(이전), 장비 `LoadEquipItem` TryParse. 시트에 없는 이름은 `ItemCheck.SetItem` 이 null 로 돌려주고 장비·인벤(`InventoryService.DrawSlot`)·창고 로드가 경고 후 버림(정책은 Q-427 임시 가정) |
| B-021 | 레거시 Input·디버그 F키 (P3) | ✅ 페이지 숫자키·F1~F6 → New Input System, F키·BuffTest·testFunc 디버그는 `UNITY_EDITOR \|\| DEVELOPMENT_BUILD` 전용, SaveManager Space 저장 제거, ItemDrag·ShopItemDrag 미사용 `Input.mousePosition` 제거. 숫자키 조건은 `nowBox != n`(기존 `inventoryUI[n].activeSelf == false`는 항상 거짓). 2~5페이지는 잠겨 있어(슬롯 없음) 실제 전환은 미검증 |
| B-022 | 획득·구매 1개마다 전체 저장 (P3) | ✅ `BeginSaveBatch/EndSaveBatch`(try/finally 로 짝 보장) — 상점 N개 구매·디버그 생성은 저장 1회. 일반 획득 1개 = 1회 저장은 유지(디바운스는 리팩터 Phase에서) |
| B-023 | `ItemCheck.name`이 `Object.name` 가림, 매 프레임 텍스트 갱신 (P3) | 🔶 Update는 값이 바뀔 때만 텍스트 갱신(표시 여부는 bool 비교). `name`은 `new`로 명시만 — 직렬화 필드라 이름 변경은 인벤 리팩터에서 |

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
| B-113 | 홀드 스킬이 다른 스킬 키를 떼도 종료됨 | `Skill.OnSkillReleased`·`EndHold`·`SkillAnimCheck`, `Weapon.ActiveWeaponSkill` | ✅ (2026-10-30) 해제 판정·쿨다운을 실제 발동 슬롯(`_activeSlot`)으로, 홀드 중 다른 스킬 발동 차단(`Weapon.ActiveWeaponSkill` + `Skill.IsHolding`), 선딜 중 해제 시 생성·데미지 지연 시퀀스 Kill. 프로브 `hold_skill.cs`(가상 키: 다른 키 눌렀다 떼도 유지·다른 스킬 미발동·자기 키 해제 시 종료+자기 슬롯 쿨)·선딜 해제 시 생성 0. 2차(10-30): `EndHold` 통합 — 시간 만료도 키 해제와 같은 정리(데미지 오브젝트 파괴·애니 정지·쿨), 프리팹 여러 개 홀드의 `SpawnSkills` 코루틴 중단, 시전 시작에 `damageObject` 초기화 + `SetSkillDmg` null 가드(바닥 못 찾은 GroundAttack). 프로브 `hold_skill_expire.cs`·`hold_skill_multi.cs`. 3차(10-30): 해제 라우팅 AttackManager 슬롯별 canceled 로 이전(Skill KeyAction 제거), 대시·피격 경직 시 홀드 종료(`Weapon.CancelHold`), OnDisable 도 `EndHold(false)`. 프로브 `hold_skill_cancel.cs` |
| B-114 | 무적·슈퍼아머·이동정지 bool을 여러 출처가 덮어씀 (대시 종료가 다른 무적 해제) | `PlayerController.cs:602`, `DamageObject.cs:183` | 🔶 무적·슈퍼아머 토큰화 ✅(PlayerStatus, 프로브 dash_overlap). 이동정지·사다리 잠금 ✅(Weapon/Skill 토큰, 프로브 lock_overlap), UI 잠금(Phase 4) ✅(2026-10-30: 레거시 쓰기 0 확인 → `DatabaseManager` 5개 bool facade를 읽기 전용으로 — 이제 쓰기는 토큰·`UiLock`만 컴파일됨. `UiLockTests` InvGroup+Craft 독립, 플레이 프로브 하나 닫아도 잠금 유지) |
| B-115 | 타일맵 타일 사이에 틈(세로·가로 선)이 보이는 고질 버그 (사용자 보고 2026-10-18) | 타일맵·아틀라스·카메라 | 🔶 1차 조치(2026-10-18, `fix_tile_bleed.cs`): LV1 시트 7장을 셀마다 4px extrude 패딩으로 재생성(rect 이동·spriteID 유지 → 타일 참조 그대로), 원본 해상도(TileMap1이 768→512로 축소 임포트되던 것 수정)·무압축·FullRect. 스크린샷상 틈 없음 — 카메라 이동 중 재발은 관찰 필요. 남은 후보: 아틀라스 패딩/블리딩 없음, 스프라이트 Mesh Type Tight, 필터 Bilinear·압축, 픽셀 퍼펙트 미스냅. 조치 후보: SpriteAtlas 패딩 4 + Extrude, Full Rect, Point·무압축, Pixel Perfect Camera Upscale RT 또는 Grid gap −0.001 |
| B-116 | 너클 계열 Sway Back/Hoof Step 사용 시 콘솔 에러 "No AnimCip : BackDash" — 플레이어 AnyPortrait에 BackDash 애니가 없음(스킬 skillAnim) | `Leather Knuckles`/`TuskKnuckles` 프리팹 RSkill | ✅ skillAnim 을 같은 무기 대시 애니(Dash)로 교체 (`fix_backdash_anim.cs`), 스킬 3종 발동 에러 0 |
| B-117 | ✅ 스포너 영역이 겹친 곳에서 몬스터가 **다른 스포너**의 트리거 경계를 넘으면 `OnTriggerExit2D`가 `DisaperByTime`을 불러 사라짐(깊은 숲 보스가 등장 직후 증발) → 자기 소속(`enemySpowner == this`)일 때만 사라지게 (2026-10-19) | `EnemySpawner.cs` |
| B-118 | ✅ Lv2 카메라 경계 4개가 90° 회전된 템플릿 복제라 Top/Bottom 이 세로 막대(x −300)가 돼 카메라가 x −311 에서 멈춤(보스 구역이 화면 밖) → 회전 초기화, `Lv2MapBuilder` 도 수정 (2026-10-19) | `Lv2MapBuilder.cs`, 씬 `Lv2_MagicCliffs/Cam`, `Lv1_Swamp/Cam` |
| B-119 | ✅ 깊은 숲 귀환 포털 `Portal_DeepToNormal` 이 원본 근교 맵의 비활성 포털을 이어받아 꺼져 있었음(TravelNow 직접 호출 테스트로는 안 보임) → 켬 (2026-10-19) | 씬 `Lv1_DeepForest/Portal`, `build_swamp.cs` |
| B-120 | 🔶 높이 4 발판 스포너 영역 바닥이 발판(−174)과 같아 발판에 선 플레이어를 못 잡음 → 늪지는 영역을 아래로 2 늘림. **Lv2 원본(L2_B_ShroomUp·L2_B_GoblinN)은 미수정** | 씬 `Lv2_MagicCliffs`, `Lv2MapBuilder.cs` |
| B-121 | ✅ 계보 무기 공용 판정 `WispLantern/WispTouch` 를 WispOrb·WispRing·FaeOrb·Slime/GelHopper 가 공유하는데 Wisp 전용 폴더에 있었다 → `DamageObject/Common/OrbTouch.prefab` 로 .meta 와 함께 이동(`AssetDatabase.MoveAsset`, GUID 7e1e7d1a… 유지, `move_orb_touch.cs`). 빌더 프로브 5개 경로 갱신. 검증 `orb_touch_refs.cs`: 참조 프리팹 5개 모두 ref=1·missing=0, 플레이 늪지에서 Ghostlight Ring·Fairy Dust Bolt 타격 확인. 남은 것: `build_wisp_lantern.cs` 가 공용 판정 값을 직접 덮어쓰므로 Wisp 밸런스 변경 시 여전히 전파 — 무기별 값은 오브 쪽에 둘 것 (2026-10-19) | `Resources/Weapon/DamageObject/Common/` |
| B-122 | 유도탄·공전 오브마다 Point Light2D 1개 → 연타 시 동시 8~12개 | HomingOrb/OrbitWisps 프리팹, 2층계 구름 정령(몹마다 Light2D, 밤 최대 ~17)·성벽 달빛 7 | ⏳ 프레임 저하 시 시전당 1개 또는 All In 1 글로우로 대체, Instantiate/Destroy 풀링은 함께 |
| B-123 | 보스·몹 판정 복제본(AncientEnt 뿌리 등)이 원본 개선을 따라가지 않음, 빌더 프로브가 스킬을 배열 순서로 지정 | `build_*_weapons*.cs`(rampart 포함), `build_ent_boss_roots.cs` | ⏳ Prefab Variant 화, 스킬은 자식 이름으로 찾기 |
| B-124 | ✅ 늪지 거대 슬라임 보스가 PassiveTerror 판정(갈퀴·불꽃) 그대로, 분열 패턴 없음 / 모기가 FlowerMole 유도탄 → `build_swamp_boss_attacks.cs`(엔트 방식: 복제 + OverrideController, 스포너 키 유지, BT damageOb GUID·fileID 교체): 점액 내려찍기 `GiantSlimeSlam`(64px×6@16ppu)·점액 탄 `GiantSlimeGelShot`(32px×3@12.5ppu), 2페이즈 `BossMinionCall` 늪 슬라임 2 분열(보스 풀 minion·minionDead·leaf), 모기는 `MosquitoBite`(ClowAttack 복제, 12 피해) + `EnemyLungeBite`(플레이어 쪽 최대 3.5유닛 0.18초 돌진). 아트 `gen_swamp_boss_fx.py`. 검증: 보스 두 판정 발동 스크린샷, `slime_boss_test.cs` 55% → 2페이즈·부하 2 → 처치 → 보상 16·부하 0, `mosquito_bite_test.cs` 간격 2.0 → 1.3 돌진, 에러 0. 남은 것: 돌진은 transform 이동이라 벽을 무시할 수 있음(기존 BT 이동과 같음), 모기 BT 감지 범위는 원거리용 그대로 (2026-10-19) | `Prefab/Enemy/EnemyDamge/Swamp/`, `GiantSlime_BD`, `SwampMosquito_BD`, `EnemyLungeBite.cs` |
| B-125 | ✅ 몹 직선 탄(EnemyDamageObject 비추적 발사)이 왼쪽을 보던 몹에서 localScale −x 그대로 회전돼 좌우 반전된 채 날아감(산적 궁수 화살이 거꾸로) → 발사 시 x 스케일 절댓값 (2026-10-19) | `EnemyDamageObject.cs` LaunchObject |
| B-126 | ✅ EnemyHealth.OnEnable 이 Kill 된 시퀀스에 `IsPlaying()` 을 불러 DOTween "This Tween has been killed" 경고가 풀 워밍업마다 수십 개 → `IsActive()` (2026-10-19) | `EnemyHealth.cs` OnEnable |
| B-127 | ✅ 보스 부하(SpawnMinion)가 보스가 죽어도 남고 드롭이 없었다(슬롯 밖이라 시간대 전환 DeCycleCheck 대상도 아님) → 스포너가 부하 목록을 들고 `DismissMinions()`(DisaperByTime). `BossMinionCall` 이 보스 `OnReleasedToPool`(처치·도트 사망·사라짐 공통)에서, `DeCycleCheck` 가 시간대 전환 때 부른다. 검증 `boss_minion_cleanup_test.cs`: 처치 2.5초 뒤 부하 2→0 (2026-10-19) | `EnemySpawner.SpawnMinion`, `BossMinionCall` |
| B-128 | ✅ 실제 장착 경로(`InventoryManager.UseEquipment` = 우클릭·Enter 장착)로 재현: **장비창을 연 상태에서는 누적 없음**(같은 세트 무기 4회 교체 → 스택 1·무기 1). **장비창이 닫힌 상태**(EquipBoxCheck.Update 가 안 돌아 ActivePrefab 미실행)에서 다시 장착하면 `equipWeapon==null` 분기로 LoadPrefab 이 반복돼 스택·무기 프리팹이 1→4 로 쌓였다(테스트 하네스가 이 경우). → `LoadPrefab` 이 활성화 전 대기 로드분을 파괴하고 세트 수를 되돌림(`ReleasePendingPrefab`), `DeletPrefab` 은 대기분 파괴 + 참조 끊기. 검증 `set_swap_ui_test.cs`(닫힘·열림 모두 스택 1·무기 1, 방어구 1) (2026-10-19). 남은 것: 닫힌 상태 무기 재장착 시 이전 아이템 UI 가 장비칸에 같이 남는 경로(InventoryManager 분기 조건) | `EquipBoxCheck`, `InventoryManager.UseEquipment` |
| B-129 | BackstabStep 이 플레이어 `localScale.x` 를 직접 뒤집어 방향을 바꿈 — 이동 입력이 있으면 다음 프레임 PlayerController 가 덮어씀 | `BackstabStep.cs`, `PlayerController.cs` 433/546 | ⏳ PlayerStateDesign FSM 의 방향 설정 API 로 교체 |
| B-130 | ✅ 근교 원본 지형 계열 맵의 맵 끝 낙사. 플레이 측정(끝에서 바깥 방향키 3초): **오른쪽 끝**은 샛길·일반 숲·산적 성채·깊은 숲·탑 근교 모두 낙사(마지막 발판 x −138.85 너머 2유닛 구덩이 깊이 10 → 그 끝이 낭떠러지), **왼쪽 끝(−436.8)** 은 깊은 숲·탑 근교 낙사 — 탑 근교에 넣었던 `EdgeWall_Left`(x −438)는 지형 끝과 1.2유닛 틈이 있어 그 사이로 떨어졌다. → `build_edge_walls.cs`(재실행 안전): `EdgeWall_Right`(x −138.85~−135.8, 구덩이까지 메움) 5맵, `EdgeWall_Left`(지형 끝에 붙임 −437.8~−436.8) 2맵, 높이 −40~90, Wall 레이어. 재측정: 오른쪽 −139.0 정지, 왼쪽 −436.6 정지. 샛길 계열 왼쪽·늪지·Lv2 양끝은 지형 벽이 있어 떨어지지 않음 (2026-10-19) | 씬 `Map/*/EdgeWall_*`, `Tools/probes/build_edge_walls.cs` |
| B-131 | 보스 조우 대사(`BossEncounterTalk`) 중에는 보스만 멈추고 주변 일반 몹은 계속 공격한다(대사창 UiLock 으로 플레이어는 묶임). 대화 중 보스가 도트로 경직되면 `EnemyHealth.EndStiffness` 가 BT 를 다시 켠다 | `BossEncounterTalk.cs`, `EnemyHealth.cs` 382 | ⏳ BT 정지를 토큰화(대화·경직 공용) 또는 대화 중 시간 정지 |
| B-132 | ✅ 테스트 생존 유지(`keep_hp.cs` 짧은 무적 토큰 연속)가 RuntimeMonitor 의 "Invincible 12초 초과" 이상 보고(에러)를 냈다(플래그 기준 감시). → 게임 코드는 그대로 두고 프로브만 정리: 8초(40틱)마다 3틱 토큰 갱신을 쉬어 무적이 약 0.3초 풀린다(HP 는 계속 채움 — 이 틈에 HP 보다 큰 한 방은 맞을 수 있음). 검증: keep_hp 200초 중 70초 넘게 플레이 → 콘솔 에러 0 (2026-10-19) | `Tools/probes/keep_hp.cs` |
| B-133 | B-128 의 근본 원인: `InventoryManager.UseEquipment` 의 "첫 장착" 분기가 `ActivePrefab` 을 부르지 않고 `EquipBoxCheck.Update`(장비창이 열려야 돎)에 맡겨, 장비창이 닫힌 채 장착하면 열 때까지 무기·장비 스탯이 적용되지 않는다(B-128 은 대기 로드분 파괴로 우회) | `InventoryManager.cs` 881·907, `EquipBoxCheck.cs` | ⏳ 두 분기에서 LoadPrefab 직후 ActivePrefab 호출 → ReleasePendingPrefab 제거 |
| B-134 | `BossLoot.Defeated`(탑 입구 개방)가 보상 파이프라인 안에서만 불린다 — 보상 에셋이 없거나 드롭 없는 반환이면 첫 처치 키·포털 개방도 없다. `TowerGatePortal._bossName` 은 프리팹 이름 문자열 | `BossReward.cs`, `EnemySpawner.cs` 301, `TowerGatePortal.cs` | ⏳ 보스 처치 이벤트를 보상과 분리, 이름은 BossReward 참조로 |
| B-135 | ✅ `TimeChange` 가 하루(totalTime)를 넘기면 경과 시간을 0 으로 되돌려, `AddTime` 으로 하루 끝을 넘긴 시간이 버려졌다 → 밤 전환(set_night·소크)이 낮으로 튐. `Mathf.Repeat` 로 넘친 만큼 남기고 저장값도 정규화 뒤 기록, 레거시 `Input.GetKeyDown(Alpha0)` 디버그 키를 `Keyboard.current` 로 교체 (2026-10-19 회귀 소크) | `TimeChange.cs` |
| B-136 | ✅ 몹 공격·이동 트윈 오류(DOTween 안전 모드가 경고로 삼킴, 소크 샛길 밤 86건): ① `EnemyAttackNode.CreatDamageOb` 가 판정 없는 소환형(CreepingTerror `DPAttackSpawon`)에 `EnemyDamageObject` 를 가정해 NRE → TryGetComponent 가드 ② `EnemyDamageObject.DestroyObject` 가 이전 시퀀스를 덮어써 파괴된 오브젝트 transform 접근 → Kill + `SetLink` ③ `EnemyPatrol`·`EnemyGoOrigin`·`EnemyGoRevers` DOMoveX 시퀀스에 `SetLink(enemyObject)` (2026-10-19) | `Assets/Script/BT/BD_/*`, `EnemyDamageObject.cs` |
| B-137 | ✅ ② 일반 숲은 보스 없는 구역인데 복제 원본의 야간 보스 스포너를 `SetActive(false)` 로만 꺼서, 포털·웨이포인트 진입 때 `PoolCheck.AbleSpwaner`(모든 자식 스포너 켬)가 다시 켜 PassiveTerror 가 나왔다 → 스포너 삭제(`fix_normal_forest_boss.cs`, `build_normal_forest.cs` 도 삭제로). 꺼 둘 스포너는 비활성 대신 삭제할 것 (2026-10-19) | 씬 `Lv1_NormalForest`, `PoolCheck.cs` |
| B-138 | ✅ `Rabbit_BD` GroundCheck 의 누락 스크립트 → 스폰마다 "referenced script is missing" 경고(샛길 낮 18건) → 제거(`fix_rabbit_missing_script.cs`). 쓰지 않는 `StoneRabbit`·`Wolf`(Prefab/Enemy) 도 누락 스크립트 다수 — 미사용이라 그대로 (2026-10-19) | `Prefab/Enemy_BD/Rabbit_BD.prefab` |
| B-139 | ✅ 빌드 프로브가 같은 세션에서 만든 오버라이드 컨트롤러를 프리팹에 넣으면, 에디터 메모리의 프리팹 Animator 참조가 깨진 채 남아(파일은 정상) 플레이 중 "Animator is not playing an AnimatorController" 경고(늪지 낮 197건). → `MonsterBuilder.Build` 끝에서 `Reimport`(SaveAssets + 컨트롤러·프리팹·시체 `ImportAsset(ForceUpdate)` + Refresh), Build 뒤에 프리팹을 다시 저장하는 프로브(bandit·deep_forest·outskirts·swamp_mobs, ent_boss_roots)는 시작 시각을 잡고 끝에서 `ReimportModifiedSince`(Prefab·Sprite/Enemy·Art/Monsters 의 그 뒤 저장분). 검증: build_swamp_mobs 재실행 직후 같은 세션 플레이 늪지 낮·밤 → 해당 경고 0, 에러 0 (2026-10-19) | `MonsterBuilder.cs`, `Tools/probes/build_*_mobs.cs` |
| B-140 | ✅ DOTween 기본 용량(200/50) 초과로 "Max Tweens reached" 경고 + 실행 중 재할당(몹 19마리 샛길) → `TweenCapacityBoot`(BeforeSceneLoad, 500/200) (2026-10-19) | `Combat/TweenCapacityBoot.cs` |
| B-141 | ✅ 풀 반환(비활성) 뒤에도 시퀀스가 계속 돌았다: `EnemyDamageObject` DestroyObject 시퀀스(다른 경로로 먼저 반환되면 holdingTime 뒤 재사용된 판정의 parent 를 끊고 다시 반환 시도), 몹 BT 노드의 `bt.sequence` DOMoveX(반환된 몹 transform 을 계속 움직임, `transform.DOKill` 은 중첩 트윈을 못 끔). → `PoolAble.ReleaseObject` 에 `virtual OnBeforeRelease` 훅(반환 한 곳), `EnemyHealth` 는 BT 시퀀스 Kill(참조는 유지 — BT 가 pauseWhenDisabled 라 재사용 때 노드 OnUpdate 가 이어 읽음), `EnemyDamageObject` 는 자기 시퀀스 Kill. 워밍업 반환도 ReleaseObject 로. `.active` 를 null 체크 없이 읽던 노드 3개(GoRevers·RunAway·RoundAttack) 가드. `PoolCheck` 자식 스포너·드롭매니저 TryGetComponent. 몹 자식으로 붙은 판정은 부모와 함께 꺼질 뿐 반환되지 않으므로 OnDisable 이 아닌 반환 훅에 둠. 검증 `b141_release_kill_test.cs`: 이동 중 반환 몹 11마리 시퀀스 0 활성, 붙은 판정 5개 2.5초 뒤 전부 풀 복귀, 재스폰 후 에러 0. 남은 것: `EnemyHealth.sequence`(public) 는 대입처 없는 죽은 필드 (2026-10-19) | `PoolAble.cs`, `EnemyHealth.cs`, `EnemyDamageObject.cs`, `PoolCheck.cs`, `BT/BD_/*` |
| B-142 | ✅ Slime Mace A(Gel Gather)·Primordial Gel A 가 쓰는 흡입 판정 **복제본**(`SlimeMaceAbsorbDark`·`PrimordialGelAbsorbDark`)의 `BeforeAbsorb` 클립 `GoNextAnim` 이벤트 수신자가 없어(원본의 누락 스크립트를 그대로 복사) 시전마다 "AnimationEvent has no receiver" 에러 1건. 이벤트는 컨트롤러 트리거 `Next`(→ AfterAbsorb)를 당기는 역할이었다 → 새 `AbsorbAnimRelay`(GoNextAnim = SetTrigger Next)를 누락 스크립트 자리에 넣고, 이벤트 없는 다른 누락 컴포넌트(AbsorbEffect 쪽)는 제거. 검증 `absorb_relay_test.cs`: 늪지에서 Gel Gather 시전 0.6초 뒤 AfterAbsorb 전환, 콘솔 에러 0 (2026-10-19). 잔여(원본 `GreatSword/AbsorbDark`, SacreGreatSword·HeartwoodGreatsword·Quakestone Maul·Gatekeeper's Oath 공용)도 복제본과 같은 패치(원본과 복제 전 파일이 이름 외 동일) — 누락 스크립트는 기능이 없어 원본 무기 영향 없음. 검증: 탑 근교에서 Quakestone Maul Tremor Pull·Gatekeeper's Oath Oath Bind·SacreGreatSword Gathering Nightmare 시전 → AfterAbsorb 전환, 콘솔 에러 0 | `Resources/Weapon/DamageObject/SlimeMace/`·`PrimordialGel/`, `Script/Weapon/AbsorbAnimRelay.cs` |
| B-143 | 맵을 씬 안에서 통째로 복제하는 빌더 방식 때문에 `SampleScene.unity` 가 구역마다 커진다(무너진 성벽 추가로 38.6MB → 44.6MB, +24만 줄) — 머지 충돌·에디터 로드 부담 | `Tools/probes/build_*` 맵 빌더, 씬 `Map/*` | ⏳ 맵 루트를 프리팹(샛길·근교·Lv2 원본 프리팹의 Variant)으로 빼고 씬에는 인스턴스만 |
| B-144 | 테스트 하네스 한계: ① `keep_hp.cs` 의 무적 쉬는 틈(B-132)에 HP 100 칭호 0 플레이어가 마법 절벽 밤 해골 한 방(70 × 부하 부족 1.5 = 105)에 죽어 마을로 귀환 → 소크 결과 `Lv2_MagicCliffs night mobs=0 pos y136`(구역 문제 아님, 에러 0). ② `*_weapon_test.cs` 로 무기를 장착한 직후 첫 스킬 발동이 가끔 무시됨(재발동하면 정상) | `Tools/probes/keep_hp.cs`, `zone_soak.sh`, `bandit/rampart_weapon_test.cs` | ⏳ 쉬는 틈에 최대 HP 임시 상향 또는 소크에서 몹 공격 끄기 / 장착 뒤 1프레임 대기 |
| B-145 | 에디터에서 씬을 저장하면 Quest Machine UI 프리팹 인스턴스의 RectTransform 오버라이드 23개가 레이아웃 재계산 값으로 바뀌어 매번 diff 에 섞인다(1층계 폴리싱에서는 ambience hunk 만 골라 커밋) | 씬 `SampleScene`, `Quest Machine.prefab` 인스턴스 | ⏳ 오버라이드를 프리팹 쪽 기본값으로 정리하거나 레이아웃 그룹 비활성 상태로 저장 |
| B-146 | 스킬 판정·투사체가 풀이 아니라 Instantiate/Destroy 라 `ProjectileFx` 트레일(TrailRenderer·Gradient·Curve)이 생성마다 할당된다(화살 비 6발 등) | `Skill.cs` 181~204, `ProjectileFx.cs` | ⏳ 판정 풀링 때 트레일을 프리팹에 굽고 재사용 |

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
| A-201 | 보스 5종(AncientEnt·BanditChief·GargoyleLord·Gatekeeper·GiantSlime)의 도트 밀도가 유닛당 18이라 PixelLook 24 격자에서 도트 크기가 1·1·2칸으로 고르지 않다. 24로 맞추면 크기 25% 축소 + 공격 판정 범위 재조정 필요(ArtStyle 해상도 규정은 24) | `reskin_mobs.cs`(보스 WorldPxPerUnit 18), 보스 공격 콜라이더 — **시도·되돌림(2026-10-19)**: `Tools/probes/boss_density24.cs`(시트 PPU×4/3·발 기준 피벗·몸 콜라이더×0.75)로 적용해 보니 문지기가 플레이어보다 조금 큰 정도(약 2.3유닛)로 줄어 보스 존재감이 무너져 되돌렸다. 해결은 **보스 시트를 더 큰 프레임(약 170px)으로 다시 그려** 24 밀도에서 지금 크기를 유지하는 것 — 이미지 작업(PixelLab 충전 후)으로 미룸 |
| A-202 | ✅ (2026-10-19) 타이틀 화면에서 좌우로 슬롯 선택(요약: 랭크·시각, 비어 있음/손상됨). 다른 슬롯을 고르면 `SaveSlots.SwitchAndRestart` — 씬 재로드 대신 **재시작**(빌드: 실행 파일 재실행 후 종료, 에디터: 플레이 종료). DatabaseManager 정적 상태 32개와 DDOL 싱글턴이 섞이지 않게 하려는 선택. 실행 중(게임 도중) 전환 메뉴는 없음 | TitleScreen, SaveSlots |

| B-213 | 픽셀 리그 주인공: 방어구 외형 교체 없음(`PixelRig.SetMeshImage` 무시) — CharacterPixelRules 6절·Q-420 레이어 설계 후 부위 스프라이트 교체로 구현 | `PixelRig.cs` |
| B-214 | 픽셀 리그 주인공: 무기 45종은 10-25 도트 복원·각도 자동으로 해결(weapon_lineup). 남은 것: 스킬 고유 동작은 기본 클립 매핑(`PixelRig.MapName`). 활 쏘기는 10-25 Shoot 로 해결 | `PixelRigBuilder.cs`, `PixelRig.cs` |
| B-215 | ✅ 픽셀 리그 주인공 피격(Hurt)·사망(무릎 꿇음 → 눕기) 연출 (10-25) | `PixelRig.cs` |
| B-216 | ⏳ Shift 키 겹침: 달리기 토글을 누를 때마다 아티팩트 발동(`ArtifactSystem` Shift 하드코딩). 키 배치 결정 대기 Q-422 (10-25) | `ArtifactSystem.cs`, `KeyAction.inputactions` |
| B-217 | ✅ 대시 잔상이 픽셀 리그에서 큰 파란 사각형으로 찍힘(FullRect 부위를 메시로 복사) → SpriteRenderer 복사 (10-26) | `DashAfterimage.cs` |
| B-218 | ✅ isAttack 이 없는 몹 애니메이터(토끼 등)를 맞힐 때마다 'Parameter isAttack does not exist' 경고 → 존재 여부를 Awake 에서 한 번 캐시 (10-26) | `EnemyHealth.cs` |
| B-219 | 인벤 드래그 임시 구멍(Phase 3c 전까지): ① 드래그 중인 아이템은 dragCanvas 아래라 그 사이 다른 경로가 `SaveInventory` 를 부르고 종료되면 세이브에서 빠짐 ② 페이지 탭 드롭 사전 검사 `CheckBoxCanCreat` 는 `inventoryArray`(폴링) 기준, 실제 배치는 모델 기준이라 거짓 음성 가능(실패 시 원위치라 소실은 없음) ③ `ShopItemDrag` 는 어디서도 안 쓰이는데 `DropFromEquipNotAllowed` 가드가 없음. 조치 후보: OnBeginDrag~OnEndDrag 를 `BeginSaveBatch/EndSaveBatch` 로 감싸기, 3c 에서 사전 검사 삭제, ShopItemDrag 삭제 | `ItemDrag.cs`, `ShopItemDrag.cs`, `InventoryService.PutInPage` |

## P3 (정리)

- 디버그 키 제거: SaveManager의 Space 키, TimeChange의 0 키, testFunc의 F1 키.
- 매 프레임 할당·조회 줄이기: Play 애니 재호출, LayerMask.GetMask, GetComponent, FindWithTag, Debug.Log.
- 오타 정리: Stemina, Posion, Destory, Weqpon, Spowner 등. 직렬화 필드는 `FormerlySerializedAs`로 이름을 바꾼다.
- 빈 클래스와 중복 코드 정리: ShopItemDrag, ShopCostUI, Equip/WeaponSmithUI, CameraResolution, HealthSystem 데모.
- 벤더 코드 수정분 분리: Behavior Designer BehaviorTree.cs.
- AnyPortrait Unity 6 경고: 새 버전으로 업데이트한다(캐시에 패키지 있음).
