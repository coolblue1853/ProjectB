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

> **11-01 창고 재점검:** B-008은 개당 생성 루프를 `SlotContainer.Transfer` 수량 이동으로 통합. B-014는 창고 이름만 비교하던 생성·키보드 교환·창고 내부 드래그까지 ID/티어/강화 일치로 보완(아래 기존 완료 기록의 "스택 병합은 이름 비교 유지"는 이 경로에서 해소). B-015는 꺼내기 도중 인벤만 여러 번 저장하던 중간 상태를 없애고, 양쪽 모델·뷰 반영 후 `SaveInventoryAndChest` 1회로 저장. 창고 내부 드래그도 드롭 직후 양쪽 저장 요청. 신규 `ChestTransferTests` 10개 포함 Unity EditMode 460/460, 낮 플레이 데이터·즉시 저장값 대조 5,521개, 키보드/가상 패드 11/11 통과(11-02 독립 리뷰에서도 재확인). 초기 MCP 무응답에 따른 검증 보류는 해소했다. 실제 강제 종료·물리 입력·빌드 실행은 미검증이며(11-02 재확인: 가상 키보드 T/G/R·가상 패드 Select/R3/LB/Y/A/B 10/10 통과, 달리기 Shift 토글 걷기 2.2→달리기 5.7 유닛/초, 물리 장치·마우스 드래그는 ItemDrag 비활성이라 해당 없음/미검증), Windows64 빌드 시도는 IL2CPP 모듈 미설치로 실패했다. 상세 `Docs/Playtest/ChestReview.md`.

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
| B-216 | ✅ Shift 키 겹침: 달리기 토글을 누를 때마다 아티팩트 발동(`ArtifactSystem` Shift 하드코딩). 10-30 아티팩트를 입력 액션 Player/Artifact(기본 V)·ArtifactNext(Tab)로 옮기고 키 설정 화면에서 변경 가능(Q-422 임시 가정) | `ArtifactSystem.cs`, `KeyAction.inputactions` |
| B-217 | ✅ 대시 잔상이 픽셀 리그에서 큰 파란 사각형으로 찍힘(FullRect 부위를 메시로 복사) → SpriteRenderer 복사 (10-26) | `DashAfterimage.cs` |
| B-218 | ✅ isAttack 이 없는 몹 애니메이터(토끼 등)를 맞힐 때마다 'Parameter isAttack does not exist' 경고 → 존재 여부를 Awake 에서 한 번 캐시 (10-26) | `EnemyHealth.cs` |
| B-219 | 인벤 드래그 임시 구멍(Phase 3c 전까지): ① 드래그 중인 아이템은 dragCanvas 아래라 그 사이 다른 경로가 `SaveInventory` 를 부르고 종료되면 세이브에서 빠짐 ② 페이지 탭 드롭 사전 검사 `CheckBoxCanCreat` 는 `inventoryArray`(폴링) 기준, 실제 배치는 모델 기준이라 거짓 음성 가능(실패 시 원위치라 소실은 없음) ③ `ShopItemDrag` 는 어디서도 안 쓰이는데 `DropFromEquipNotAllowed` 가드가 없음. 조치 후보: OnBeginDrag~OnEndDrag 를 `BeginSaveBatch/EndSaveBatch` 로 감싸기, 3c 에서 사전 검사 삭제, ShopItemDrag 삭제. 🔶(10-30) ① 인벤·창고 모두 드래그 동안 `BeginSaveBatch` 로 미룸(`Chest.SaveChest`·`SaveInventoryAndChest` 도 배치 공유, 끝에 같은 시점 스캔 1회 저장), 파괴·씬 해제 중엔 저장 없이 풀고(`AbandonSaveBatch`) 페이지가 파괴됐으면 저장 건너뜀(`PagesAlive`) ✅ ② 인벤 쪽 `CheckBoxCanCreat` 삭제 ✅(창고 쪽은 Phase 4) ③ `ShopItemDrag` 삭제 ✅. 남은 것: OnEndDrag 가 오지 않고 오브젝트도 살아 있으면 배치가 풀리지 않음(감시·강제 해제 또는 프록시 드래그로 근본 해결, 3c) | `ItemDrag.cs`, `InventoryService.PutInPage` |
| B-220 | ⏳ 제작 창을 ESC 로 닫으면 같은 프레임에 일시정지 메뉴도 열린다(CraftingManager 가 먼저 닫아 isOpenUI 가 풀린 뒤 PauseMenu 가 ESC 를 받음). 10-30 제작 핀 프로브에서 발견. 조치 후보: PauseMenu 가 직전 프레임에 UI 잠금이 풀렸으면 무시 | `CraftingManager.cs`, `PauseMenu.cs` |
| B-221 | ⏳ 무기를 교체하면 `skillCooldown.ResetCoolTime()` 으로 스킬 쿨다운이 0 이 된다(기존 동작). 10-30 장비 프리셋(7/8 두 번)으로 쉽게 쿨 초기화 가능 — 인벤이 열려 있어도 시간이 흐르면 악용 여지. 조치 후보: 교체 시 쿨다운 유지 또는 새 무기 기준 재계산 | `InventoryManager.UseEquipment`, `GearPresets` |

## P3 (정리)

- 디버그 키 제거: SaveManager의 Space 키, TimeChange의 0 키, testFunc의 F1 키.
- 매 프레임 할당·조회 줄이기: Play 애니 재호출, LayerMask.GetMask, GetComponent, FindWithTag, Debug.Log.
- 오타 정리: Stemina, Posion, Destory, Weqpon, Spowner 등. 직렬화 필드는 `FormerlySerializedAs`로 이름을 바꾼다.
- 빈 클래스와 중복 코드 정리: ShopItemDrag, ShopCostUI, Equip/WeaponSmithUI, CameraResolution, HealthSystem 데모.
- 벤더 코드 수정분 분리: Behavior Designer BehaviorTree.cs.
- AnyPortrait Unity 6 경고: 새 버전으로 업데이트한다(캐시에 패키지 있음).

## 11-01 리뷰 남은 것 (P3)

- **판단: 감지·스폰 영역 분리 보류.** `SpawnPointsFloor1.csv` 10행은 모두 Tree/Stone이고, 플레이 프로브에서도 대응 스포너 10개·프리팹 tree/Rock·그 외 대상 0개 확인. 정지 자원이므로 현재 `FitAreaToPoints`의 좌표 전체 + 좌우 12·위 15·아래 6 확장은 무해하다. `isInPlayer`와 자기 몹의 영역 이탈 판정이 같은 BoxCollider2D를 사용하므로, 몹에 CSV 좌표를 도입하기 전에 두 영역을 분리해야 한다. 지금 분리하면 기존 활성 범위·영역 이탈 반환까지 바뀌므로 적용하지 않는다.
- ✅ **CSV 가중치·발판 0개 처리:** `Start`에서 씬 발판 유무 검사보다 CSV 덮어쓰기를 먼저 실행한다. 원본 발판 루트가 없으면 스포너 아래에 생성하며, CSV에도 `spawnPointWeights`를 전달한다. `CsvSpawnOverrideTests` 2건(루트 null/빈 루트, 가중치 0|3|0|1 → x=-316 3슬롯·x=-705 1슬롯) 통과. 현재 CSV 자원은 가중치가 없고 씬 발판도 있으므로 기존 배치와 동일하다.
- ✅ **지역 배너 암전 대기:** `MapChanged` 발행은 그대로 유지하고 `ZoneBanner`만 한 프레임 대기 후 `ScreenFader.Busy`가 끝나면 표시한다. 새 요청은 이전 대기를 취소하며, 기다리던 맵이 비활성이면 표시하지 않는다. 대기·비활성 맵 회귀 테스트 통과. 실제 `Portal.Use` 재진입 추적 23표본 중 Busy=True 7표본은 alpha=0·표시 이력=0, 1.311초에 Busy=False·표시 이력=1, 1.499초 alpha=0.360. 다른 이벤트 구독자 변경 없음.
- ✅ **1시간 플레이 테스트용 SR 치트 4종:** 플레이 모드에서 실제 `SROptions.Current` 호출. Give Item: LeafAdae 1→2. Give Weapons: LeafAdae 2→3·FangDagger 0→1. Fill Quests: Q101 대화 0→1/1, Q102 처치 0→8/8, Q103 수집 1→3/3(GreenHurb=3), Q203 제작 0→1/1, Q204 방문 0→1/1, 모두 Ready. Unstuck: 6종 상태 토큰 합 6→0, Dump=(none), 피격·공격·사다리 플래그 false.
- **재개 검증(11-01):** EditMode 443/443 통과(실패·스킵 0), 컴파일/플레이/정지 콘솔 오류 각 0. 낮 실제 포털 연속 캡처 `Logs/playtest/p3_resume_portal_sheet.png`에서 암전 뒤 배너가 온전히 나타남. 재진입 장면의 배너 겹침·잘림, 캐릭터 끼임·튐·방향 이상 없음. 근거: `p3_resume_tests.json`, `p3_cheats.txt`, `p3_portal_trace.csv`, `p3_resume_spawners.txt`(모두 `Logs/playtest/`). 커밋하지 않음.
- **추가 발견(별도 보류):** 최초 맵 진입 때 보스 배너가 지역 배너 위에 겹치고 카메라가 플레이어를 벗어나는 장면 관찰(cameraY 2.90→57.86, playerY=0.26). `WarmUp`의 Get/Release가 보스를 활성화하고 `EnemyHealth.OnEnable`이 `BossIntro.Play`를 호출하는 경로가 원인으로 추정된다. 재진입에서는 발생하지 않았으며, 보스 등장 연출·워밍업 분리는 이번 P3 수정 범위 밖으로 남긴다.
- **미검증:** 1시간 연속 소크·빌드 실행·실제 SR 버튼 입력 경로(옵션 메서드는 직접 호출 검증), 몹 스포너에 CSV 덮어쓰기 도입 시 감지/이탈 범위. Fill Quests의 도감 수집 목표는 의도적으로 채우지 않는 기존 구현이며 이번 5종 목표 검증에 포함하지 않았다.
- **세이브 복원:** 시작 직전 `Logs/save-backup-p3-resume-20261101-214422/`에 3개 es3 백업 → 플레이 정지 후 3/3 MD5 일치 복원. SaveSlot3.es3 원본 75,451바이트·MD5 `1A521B8574EA743FA7B982B997487536` 유지, 에디터 슬롯 3·ES3 경로 SaveSlot3.es3 복구. 근거 `Logs/playtest/p3_resume_save_restore.json`.
- **독립 리뷰(p3_1101, 11-01):** 지정된 미커밋 6파일만 검토한 결과 신규 [필수] 결함 0건, 런타임·테스트 코드 수정 없음. CSV 경로의 가중치/루트 null 처리, 배너의 암전 대기/이전 요청 취소/비활성 맵 방어와 세이브 키 미변경을 확인했다. 기존 배너 이벤트 구독·트윈 코드는 이번 diff에서 변경하지 않았다. EditMode 443/443(실패·스킵 0), 컴파일 후·플레이·정지 콘솔 오류 각 0. CSV 10행/대응 스포너 10개/비자원 0개 재확인. SR 옵션 창과 핀 버튼에 UI pointerClick 이벤트를 전달해 Give Item +1, Give Weapons 각 +1, 퀘스트 5종 Ready, Unstuck 토큰 6→0 검증(물리 마우스/키보드 입력·화면 좌표 hit-test는 미검증). 낮 실제 Portal.Use 최초·재진입 각 16표본: Busy=True인 각 6표본 모두 배너 alpha=0·표시 이력=0, 재진입 1.205초 Busy=False·이력=1, 2.090초 alpha=1. 재진입 캡처 `p3review_reentry_11.png`에서 배너 겹침·잘림, 캐릭터 끼임·방향 이상 없음. 최초 진입은 기존 보스 연출 중첩·카메라 이탈 재현(playerY=0.26, cameraY 최대 61.32), 위 별도 부채 유지. 품질 의견: 보스 조우 전 큰 경고 연출은 초반 소박한 진행을 해치고, 상단 치트 버튼은 체력 HUD와 겹쳐 시인성을 낮춘다(기존 화면, 이번 범위 밖). 근거는 `Logs/playtest/p3review_*`. 세이브 3/3 MD5 일치·슬롯 3/SaveSlot3.es3 원상 복구, 플레이 정지. 1시간 연속 실행은 미검증.
- **리뷰 빌드 제한:** Windows64 개발 빌드 요청(`build-b31078565c`, 출력 예정 `Logs/Builds/p3review/ProjectB.exe`)은 접수됐으나 이후 unity-mcp 상태 조회 3회가 TimeoutError, execute_code 상태 조회도 실패했다. 빌드 취소 요청도 시도했으며 완료/취소 확정은 미검증이다. 빌드 실행 파일 생성은 확인되지 않았다. 컴퓨터 사용 도구의 Unity 읽기 접근은 자동 승인 검토에서 거절되어 UI 경유 진단을 중단했다. 빌드 요청 **전** 플레이 정지와 세이브 복원은 확인됐고, 그 뒤 새 플레이는 시작하지 않았다. 빌드 성공이나 최종 에디터 응답 정상화로 보고하지 않는다.
- 빌드 취소 응답은 `Unity plugin session 467cb400-e0b1-4d9d-9e70-8a415876dcf0 disconnected while awaiting command_result`로 실패했다.

## 2026-11-01 플레이테스트 버그 3건

| 내용 | 원인·수정 | 검증 |
|---|---|---|
| 음식 후 스태미너 자연 회복 미재개 | 허기 부족 시 DOTween 회복 체인이 끝났지만 음식에서 재예약하지 않음. `FullnessHeal`에서 끊긴 회복만 재예약, 대기 중 중복 금지, 만충 허기 소모 방지 | ✅ 신규 회귀 7개 포함 EditMode 460/460·아래 실제 플레이 검증 (미커밋) |
| ESC로 창 닫기와 일시정지 동시 열림 | 개별 UI와 PauseMenu가 같은 입력을 따로 판단. 프레임 시작 최상위 UI 고정 + 단일 소비(`UiBackFrame`), 키 설정도 UiLock 참여, 대화·게시판 취소 연결 | ✅ 신규 회귀 7개 포함 EditMode 460/460·아래 실제 플레이 검증 (미커밋) |
| 착용 장비 강화 불가 | 강화 수집이 가방만 순회. 착용 장비 추가·표시, 실착 프리팹 스탯과 equipGear 동기화. HP 차이만 적용, 세트 출처 보존 | ✅ 신규 회귀 7개 포함 EditMode 460/460·아래 실제 플레이 검증 (미커밋) |

### 세 버그 최종 검증 (11-01)

- **전체 EditMode:** 460/460 통과, 실패 0·스킵 0. 신규 `PlaytestBugRegressionTests` 7개: 허기 0 후 회복 재예약·중복 예약 방지, 소비 직후 대기 보존, 만충 허기 소모 방지, 최상위 창 소비·같은 프레임 일시정지 낙하 방지, 중첩 UI 소비, 착용 방어구/무기 스탯·저장 갱신 및 하락·반복 적용.
- **컴파일·플레이 콘솔 오류:** 각 0. `git diff --check` 통과. reviewer-code/reviewer-arch 체크리스트로 새 필드·ES3 호환·null 경로·UI 수명·갱신 비용을 검토했다. 세트/출혈/쿨다운은 장비 강화로 변하지 않아 해제·재적용하지 않고 기존 출처를 보존한다.
- **자연 회복:** 실제 대기 2초, 주기 0.1초·틱당 4. 허기/스태미너 `0/0` → 대기 종료 뒤 `0/0` → `Rabbit Meat Stew` 실제 인벤 소비 성공 `100/0` → 자연 회복 관찰 `96/16`. 값 직접 회복이나 콜백 강제 완료 없이 측정했다(`stamina.txt`).
- **ESC 실제 입력:** 테스트용 Input System Keyboard를 런타임에 추가하고 게임 내부 이벤트만 큐잉했다(OS 키·마우스 주입 없음). 22+10=32/32 통과. 강화·인벤·상점·제작·지도·도감·NPC 퀘스트·대화·창고·일시정지·키 설정 및 인벤 상세·제작 선택·Quest Machine 게시판. 창고 취소는 창고만 닫고 인벤 유지 → 다음 ESC로 인벤 닫기 → 다음 ESC로 일시정지 열기. 중첩 키 설정 취소는 일시정지 유지. 추가 장치와 복제한 입력 설정은 프로브 종료 때 제거·복원했다(`escape.txt`, `escape-details.txt`).
- **착용 강화:** 무기·장갑 모두 +4→+5 확정 성공(기존 찬스타임 사용), 가방으로 옮기지 않고 강화석 각각 3·골드 각각 400 차감, ES3를 다시 읽어 해당 `equipGear` 강화값 5 확인(`enhance.txt`). 녹슨 검 최소 데미지 6→6은 반올림 결과이며 최대 데미지는 12→13으로 즉시 변경됐다. RabbitArmor 2부위 활성 상태에서 장갑 강화: 부위 수 2→2, 방어 15→16, 최대 HP 336→351, 현재 HP 336→336(무료 회복 없음, `active-set.txt`). 강화 창 목록에서 착용 무기·장갑 포함 확인, 모자를 더 착용한 최종 화면에서도 세 장비가 `[착용]`으로 표시된다.
- **낮 화면 눈 검토:** `Logs/playtest/bugs1101/`의 강화·인벤·상점·제작·창고·대화·게시판·일시정지·회복 캡처를 직접 확인했다. 새 `[착용]` 행과 강화 정보에 겹침·잘림 없음. NPC/캐릭터 끼임·튐·방향 이상은 정지 화면에서 관찰되지 않았다. 전투 연속 모션은 이번 수정 범위가 아니며 검증하지 않았다.
- **기존 화면 문제(별도 부채):** 퀘스트 추적 HUD가 인벤·제작 창 위에 겹치며, 우상단 Quest Machine 추적 제목 일부가 화면 밖으로 잘린다. 창고 하단 편의 안내 일부가 슬롯/화면 경계와 겹치고 좌상단 아티팩트 HUD도 창고 슬롯 위에 남는다. 치트 줄은 체력 HUD와 겹친다. 이번 세 버그 수정으로 UI 정렬·폰트·씬을 바꾸지는 않았다. 후속 작업은 기존 HUD의 UI 표시 정책/캔버스 정렬/안전 영역을 정리할 것.
- **미검증:** 실제 게임패드·빌드 실행·+11~+15 정밀 단조의 직접 플레이·모든 강화 실패 확률의 장시간 실측·장시간 전투 소크. 강화 하락/반복 적용의 수치·저장은 EditMode 회귀로 확인했으며 실제 플레이에서는 성공 경로를 확인했다.
- **세이브 보호:** 시작 전 `Logs/playtest/bugs1101-save-backup/`에 es3 3개 백업, 최종 플레이 정지(`idle`) 뒤 3/3 MD5 일치 복원. `SaveFile.es3`: `F1CBD3EDF7E845CACE5442F6641C5B2F`, `SaveSlot2.es3`: `4683BD8B4FFE564E86535BDF7861504B`, 사용자 진행 `SaveSlot3.es3`: `F1A9AC9E7D4BE9716FB6ED03A0BD2C4F`. 슬롯 선택 3·ES3 경로 SaveSlot3.es3 유지. 근거 `save-restore.json`.

### 이번 작업 파일 (기존 미커밋 변경은 유지)

- 신규: `Assets/Script/Player/State/UiBackFrame.cs`, `Assets/Editor/Tests/PlaytestBugRegressionTests.cs`, 두 파일의 Unity `.meta`.
- 수정(회복·강화): `Assets/Script/Player/PlayerHealthManager.cs`, `Assets/Script/UI/EquipBoxCheck.cs`, `Assets/Script/Item/EnhanceService.cs`, `Assets/Script/Item/EnhanceWindow.cs`.
- 수정(ESC): `Assets/Script/Player/State/UiLock.cs`, `Assets/Script/UI/PauseMenu.cs`, `Assets/Script/UI/InventoryManager.cs`, `Assets/Script/UI/Chest.cs`, `Assets/Script/UI/CraftUI/CraftingManager.cs`, `Assets/Script/UI/WaypointMap.cs`, `Assets/Script/UI/Bestiary/BestiaryWindow.cs`, `Assets/Script/UI/KeyBind/KeyBindWindow.cs`, `Assets/Script/NpcQuest/NpcQuestWindow.cs`, `Assets/Script/Dialogue/PixelDialogueUI.cs`, `Assets/Script/Quest/QuestBoard.cs`.
- 수정(문서): `.claude-rules/Game/BasicFeatures.md`, `ItemSpec.md`, `TechDebt.md`, `ProgressionDesign.md`, `Docs/Playtest/FirstHourGuide.md` 조작표.
- **커밋 제외:** 이번 임시 `Tools/probes/bugs1101_*.cs`, `Tools/probes/bugs1101_*.sh` 전체. 로그·캡처·편집 보조 스크립트·세이브 백업은 `Logs/playtest/bugs1101*` 아래(커밋 제외). 커밋·push·stash·reset·브랜치 전환 없음.

### 독립 리뷰 fix_1101b (2026-11-01)

- **범위·판정:** 위 `이번 작업 파일`의 신규 4파일(코드 2 + meta 2), 수정 코드 15파일·문서 5파일에서 이 작업 변경만 검토했다. `Chest.cs`·`InventoryManager.cs`·공유 문서에 섞인 선행 창고 편의 변경은 별도 작업으로 구분했다. 신규 **[필수] 결함 0건**, 런타임·테스트 코드 수정 0파일. null 경로, 입력 단일 소비, 구독/트윈 수명, 프레임별 할당, 장비 모디파이어 출처, HP 차이 적용, 기존 `equipGear` 키·배열 호환을 확인했다. 신규 C# 2파일은 UTF-8 BOM·CRLF(단독 LF 0개)다.
- **자동 검증:** 전체 EditMode 460/460(실패 0·스킵 0, 7.315초, job `bc84775505c84bd4bef128fcb56ecfae`). 플레이 정지 확인 후 force refresh/compile 요청이 접수됐고, 컴파일 후·플레이·정지 콘솔 오류 각 0. 대상 코드 `git diff --check` 통과. 코드 변경이 없어 새 회귀 테스트는 추가하지 않았다.
- **입력 재검증:** 별도 Input System 가상 Keyboard로 ESC 기본 22/22 + 상세/제작 선택/게시판 10/10 = **32/32**. 가상 Gamepad 및 실제 게임 UI 경로 추가 검증 **12/12**: Start로 중첩 키 설정만 닫기·다음 Start로 일시정지 닫기, B로 강화 창 닫기·정밀 단조 취소 시 무소모, 착용 무기 +10→+11·장갑 +14→+15. 단조는 `Tick/Hit/Execute` 직접 호출 없이 Z로 시작하고 X 입력 이벤트 3회로 마쳤다. 비용·ES3 저장값·현재 HP 미회복을 확인했다. 고급 성공 RNG만 0으로 고정했으므로 확률 실측은 아니다. 가상 장치·복제 입력 설정·RNG는 종료 때 제거/복원했다.
- **강화 수치:** 무기 +10→+11 데미지 8~16, ES3 강화 11·결정 5·골드 1,000 차감. 장갑 +14→+15 최대 HP 306→311·현재 HP 306→306, ES3 강화 15·결정 8·골드 6,000 차감. 기존 장비를 착용한 채 진행했다. +12~+14 도달 전체 경로는 이번 직접 플레이에 포함하지 않았다.
- **자연 회복:** 허기 0에서 실제 대기 종료 후 음식 소비. 스태미너 0→16·허기 100→96, 별도 HP 검증 281→291·허기 100→94. 회복 콜백을 직접 실행하지 않았다. 테스트 재료 과다 지급으로 음식 추가가 거절된 초기 보조 실행은 인벤 공간을 확보한 뒤 재실행했으며, 최종 로그의 추가/소비 성공을 확인했다.
- **화면·품질 [참고/P2, 미수정]:** 낮 캡처 `forge-Weapon10.png`, `forge-Hand14.png`, `inventory.png`, `chest.png`, `hp-recovered.png` 직접 검토. 새 `[착용]` 목록의 겹침·잘림은 없었다. 기존 TechDebt의 HUD/퀘스트 중첩·우상단 제목 잘림·창고 하단 안내 잘림을 재확인했다. 소박한 장갑·녹슨 검을 그대로 강화하는 흐름은 초반 장비 축적 감각과 맞는다. 정밀 단조의 숫자와 한글 글자 크기가 다르고 안내가 조밀해 읽기 어렵다(`Assets/Script/Item/ForgeHammerView.cs:97`, 기존 코드·이번 diff 밖). 이번 변경에서 유아틱한 새 요소나 과장된 초반 외형은 추가되지 않았다. 캐릭터는 해당 정지 캡처에서 끼임·방향 이상을 관찰하지 않았으며 연속 이동/전투는 미검증이다.
- **남은 미검증:** 물리 게임패드·OS 실제 키 조작, 빌드 실행, +12~+14 도달 전체 UI 경로, 고급 실패/9점 환불의 이번 직접 플레이, 실패 확률 장시간 실측·장시간 전투 소크. 가상 입력을 물리 장치 검증으로 간주하지 않는다. 같은 에디터의 선행 빌드 요청이 MCP 시간 초과·연결 해제를 일으킨 기록(위 `리뷰 빌드 제한`)이 있어 이번 빌드는 재요청하지 않았다.
- **종료·세이브:** 최종 `idle`, 슬롯 3·`SaveSlot3.es3` 유지. 시작 전 `Logs/playtest/fix1101b-review-save-backup/` 백업에서 es3 **3/3 원본 MD5 일치** 복원(SaveSlot3 `F1A9AC9E7D4BE9716FB6ED03A0BD2C4F`). 근거 `Logs/playtest/fix1101b-review/save-restore.json`, 입력/회복 텍스트와 낮 PNG. 커밋·push 없음.
- **커밋 범위:** 위 `이번 작업 파일`의 24파일(신규 4·수정 20) 내 해당 작업 hunk만. 이번 리뷰가 추가 수정한 파일은 `TechDebt.md` 1개다. 특히 창고 선행 변경이 섞인 파일 전체를 일괄 stage하지 않는다. `Tools/probes/fix1101b_review_*`, `bugs1101_*`, 모든 Logs/세이브 백업은 커밋 제외. 기타 씬·폰트·서드파티·렌더링 설정·아트 삭제 등 기존 미커밋 변경은 이번 범위 밖이다.
### 창고 재개 완료 (2026-11-01)

- B-008/B-014/B-015 창고 경로를 모델 이동·속성별 병합·인벤/창고 동일 Datas 즉시 저장으로 보완. 같은 종류/재료 전부 보관·전체 병합 정렬·꺼낼 수량·용량/가득 참 안내 구현. 창고 페이지 이동의 즉시 저장도 추가했다.
- 전체 EditMode 460/460(신규 창고 10개 포함), 실패/스킵 0. 플레이 모델·양쪽 디스크 5,521개 비교, 키보드/가상 패드 11/11, 레거시 핸들러/페이지 이동 보조 검사 7/7. 재진입 후 가방 Wood 11 T2 +3·창고 2페이지 녹슨 검 1 T3 +10 유지. 컴파일/최종 플레이 콘솔 오류 0.
- 낮 캡처로 확인한 창고 하단 겹침은 페이지/커서 84% 배율·66px 안내 영역으로 해소. 수량 실행 버튼은 꺼내기/나누기를 구분한다. 기존 퀘스트 HUD의 인벤 중첩과 상단 디버그 UI는 별도 부채로 유지한다.
- 미검증: 물리 장치·마우스 hit-test·실제 강제 종료/정전·빌드·장시간 소크. 기존 Item.prefab의 ItemDrag/2D 콜라이더 비활성은 유지하며 레거시 핸들러 보조 검사를 마우스 지원 판정으로 보지 않는다.
- 플레이 정지 idle·세이브 재개 시작본 3개 원본 복원 후 MD5 3/3 일치, SaveSlot3 원본 MD5 `F1A9AC9E7D4BE9716FB6ED03A0BD2C4F`. 상세/작업 파일/커밋 제외 프로브는 `Docs/Playtest/ChestReview.md`, 증거는 `Logs/playtest/chest-resume/`. 커밋·push 없음.


### 인벤토리 편의 보완 (2026-11-02)

- 잠금·미열람 플래그를 기존 ES3 배열 변경 없이 별도 Datas 필드로 저장. 잠금 판매/분해/일괄 판매를 실행 시 검사하고 창고/분할/정렬/장비로 상태를 보존한다. 안내 문구와 슬롯 표시는 상태가 달라질 때만 재생성한다.
- 재료 일괄 판매는 기존 되사기/저장 배치를 재사용, 실제 교체 세트 효과는 기존 SetBonusEvaluator.Diff로 계산한다. 새 책임은 별도6클래스(테스트 포함)에 두었다.
- 잠금 스택으로 획득 실패할 때 바닥 아이템을 먼저 풀에 반환하던 경로를 수정했다. 실제 Add 성공 후 ReleaseObject1회. 최종 검증/파일 목록/미검증/세이브 복원은 `Docs/Playtest/InventoryConvenienceReview.md`.
- 남은 P2 UI: 원래 대형 장비 상세의 긴 설명 줄바꿈·상단 치트 UI 중첩, 퀘스트HUD가 가방 슬롯에 겹침, 장비 교체 후 슬롯 아이콘이 작아지는 기존 경로. 새 비교 패널의 하단HUD 중첩/미지원 minus 글리프는 수정·낮 재캡처 확인.
- 사용자 정책은 Q-INV01: 제작·의뢰 재료를 별도 잡템 분류로 자동 보존하지 않고 잠금으로 보호, 기존 되사기12건 유지. 물리 장치·빌드·강제 종료·장시간 소크는 미검증.
- 상점 강화(11-02): 진열 MinFloor 는 현재 전부 1이라 층계 확장 경로(CSV 행 추가→상점 재오픈 시 재구성)는 EditMode 로만 검증했고 인게임 층계 상승은 미검증. 되사기 선택 패드 조합(LT+Y)·판매 필터 패드(SELECT)·물약 충전(LT+X)은 실제 장치 미검증. 큰 진열(줄 3개 이상)은 RowGap 110 고정이라 화면 밖 가능성.

### HUD 겹침 정책 (2026-11-02)

- `Assets/Script/UI/HudPolicy.cs` 한 곳에 정리: `HudPolicy.Hidden`(= UiLock 잡힘 = `isOpenUI`)이면 퀘스트 추적(NpcQuestTracker)·Quest Machine 추적·아티팩트 칩·SR 치트 줄을 숨기고, AscentHud·CraftPinHud·MinimapHud 도 같은 값을 본다. 상주 `HudPolicyDriver` 가 아티팩트·SR 캔버스를 끄고 켜며, QM HUD 는 하위 Canvas 를 얹어 끈다.
- 평소 배치: 우측 추적 열(1080 기준 폭 380, 중앙선 위 125)에 NpcQuestTracker(내용 높이에 맞춰 최대 190) → 그 아래 Quest Machine 추적(고정 폭 줄바꿈, 폰트 26/24 환산 = QM 캔버스 기준 14/13). 잘리던 QM 제목 해결. SR 치트 줄은 `PinnedUIRoot.Options` 를 `ignoreLayout` 으로 우하단 두 줄에 옮김(체력 HUD 와 겹침 해결). 서드파티 소스 수정 없음(런타임 조정).
- 첫 판매 토스트: 창이 열려 있으면 PickupLog 줄을 오른쪽 아래로 옮겨 되사기·장비 목록과 겹치지 않음.
- 검증: EditMode 483/483(신규 HudPolicyTests 4), 콘솔 오류 0, 낮 캡처 `Logs/playtest/uifunc/`(before `base_*`, after `after_*`·`final_hud`·`shop_toast_after`)에서 인벤·상점·Craft·InvGroup·Pause·NpcQuest 잠금 시 숨김·해제 시 복귀 수치 확인. 미검증: 물리 해상도 변경, 비 16:9 화면, 퀘스트 창(NpcQuest)·창고 실제 열기 캡처.
- 남음: 창고 하단 슬롯과 좌하단 스킬 아이콘이 큰 창에 가려지는 것은 그대로(스킬 HUD 는 숨기지 않음).

## 2026-11-02 던파식 전투 보강 남은 것 (P3)
- 아티팩트 범위 피해(`ArtifactSystem.cs:226`)가 위쪽 넉백이라 전원 띄움·다운이 된다 — 의도 확인.
- `EnemyAirState` 착지 판정이 `GroundProbe`와 별개 레이캐스트(통합 여지). 다운 공격·캔슬은 순수 규칙만 단위 테스트, 통합은 플레이 프로브로만 검증.

## 2026-11-02 1층계 보강 남은 것 (P3)
- (해결 11-02 2차) 숨은 상자 위치.
- (해결 11-02 2차: 추적 HUD·미니맵) 하루 경계는 밤→낮 전환 수(세션 간 `f1.bounty.Day` 저장).

- 11-02 2차: 숨은 상자 위치 ✅(고지대 도달성은 BFS 로 걷기·점프 도달 가능한 자리만 고름), 사냥 의뢰 추적 HUD·미니맵 ✅, 일반 숲 도착 ✅ `RoadLink`(길 → 깊은 숲 포털 추가). ⚠ 기존 Pixel Crushers 퀘스트 HUD("아버지의 흔적·땔감")가 우측 추적 HUD 와 같은 자리에 겹침(기존 문제, 2차에서 건드리지 않음). ⚠ 낮 고정이 기본이라 밤 습격·하루 경계는 낮/밤 순환을 켜야 동작. ⚠ 왼쪽 산 구간은 이제 어디서도 도착하지 않음(샛길 쪽 산은 기존 포털로 그대로). 미검증: 습격 몹 BT 의 실전 처치(프로브는 ReleaseEnemy 로 대체), 실제 전투 중 습격 난이도.
- 11-02 이동 점검: 마을 가장자리 무한 낙하 ✅ `FallRescue`. 복제 지형 구덩이 갇힘 ✅ `PitReturn`. ⚠ 일반 숲 도착(x -722, 깊은 숲에서)이 왼쪽 산 발판 구간 — 방향키로는 길 쪽에 못 감(의도 확인 필요). 복제 구역 도착 위치 자체를 길 쪽으로 옮기는 것은 씬 수정이라 보류.

- **회귀 점검(11-02, f1_regress):** `zone_soak.sh 15` 8구역 낮·밤 16회 전부 콘솔 에러 0·anomaly 0(경고 1 = 마을 낮, 기존). 새 창 ESC 프로브 `Tools/probes/_f1r_esc.cs`: 사냥 의뢰·조작 안내·퀘스트 창·층계 지도·상점·인벤 열기→HudPolicy.Hidden=True, ESC 는 맨 위 창만(조작 안내 위 퀘스트 → 퀘스트만 닫힘) 닫고 일시정지 미진입, 닫으면 HUD 복귀 13/13 PASS. 우측 추적 열 겹침: 코드·실측상 NPC 추적 상자(중앙선 위 125~56)와 QM 패널(위 48에서 시작) 세로로 쌓임 확인(QM 패널 anchored y=-273.2, canvas 600). 사냥 의뢰 추적 HUD 는 NPC 추적 상자에 포함. 미검증: 실제 QM 퀘스트(아버지의 흔적)가 활성인 상태의 눈 캡처(QM 퀘스트는 대화 수락이라 프로브로 못 켰다). EditMode 530/530. 코드 변경 없음.

## 11-02 통주 점검·리뷰 남은 것 (코드 수정 없음, `Docs/Playtest/Floor1Fullrun.md`)

- (해결 11-02, Floor1Balance §7-1) (P1) 엔트 보스 몸박/스윙이 Lv9 최대 HP(164)의 34~57%라 회피 없으면 3방 사망. 문서 모델은 HP 200+보너스로 가정 → 밸런스 재검토 필요(보스 HP 하향 효과는 있으나 피해량 미조정).
- (해결 11-02: PixelDialogueUI 무적 토큰, 끝난 뒤 0.5초 — 플레이 모드 확인: 대화 중 Invincible·CanBeHit=false, 종료 0.5초 뒤 해제) (P2) NPC 대화 중에도 몹이 플레이어를 때려 사망(이브 대화, 일반 숲 입구). 대화 중 무적 또는 몹 정지 검토.
- (해결 11-02: NpcQuestLog.BestFor 본 흐름 우선 — 플레이 모드 확인: Q104 완료 뒤 Q105·Q202 동시 수락 가능일 때 Q202 가 잡힘) (P3) 루엔이 Q105·Q202 동시 수락 가능 상태일 때 Q105가 먼저 잡힘 — 안내 필요 여부 확인.
- (P3) EnemyTelegraph: 바닥 띠 0.7초 vs 공격 시작 0.5초, `isBoss` Start 단발 캡처, `!` 마크 트윈 OnDisable Kill 누락. BossReward.RewardSummary 영문/한글 혼용. BossPhase.Thresholds 가변 배열 공개. BossPortrait 초상 없으면 페이즈 선도 생략.
- (확인 11-02, 버그 아님, Floor1Balance §7-2) 받는 피해가 문서 계산보다 약 25% 큼 = 깊은 숲 상승 부하(×1.4). SaveSlot2 Lv4 HP 434 = 성장 124 + 늑대 장비 260 + 세트 50.
- (11-03) 별폭발(StarBurst) 아티팩트 적중 확인: 허수아비 2.0/3.5m 소환 후 발동 → HP 100000→99848/99909 감소(반경 4). 수정 불필요. 프로브 `Tools/probes/_sb_test.cs`.
- (11-03 PV1) (P3) 구역 인장 3종 아이콘 없음: `Resources/Item/ZoneSeal_Swamp|Fortress|Tower` 이미지 파일 없어 드롭·인벤 표시 때 Error "이미지 파일을 로드하지 못했습니다". 아이콘 제작 필요(그래픽 사용자 검토 후 적용). 확인: 둥지 정예 처치 드롭 시 콘솔.
- (11-03 PV1) (P3) 방어구 제작창 정적 페이지 0~1 칸이 창 열기 전 itemData 가 비어 있음(지연 로드, 열면 정상 — 동작에는 문제 없음). PV1 로 확인한 CF-A~D·L1·L5·L7·미니보스·둥지는 모두 통과, 상세 `Docs/Playtest/Floor1PlayVerify.md`.

- (11-04 arrival-safe) ✅ 포털 도착 안전: `ArrivalSafeZone`(Portal.OnEnable 이 목적지 등록, 반경 14×9) 안에는 `EnemySpawner.CycleCheck` 가 스폰 안 함(풀에서 꺼내기 전 걸러 누수 없음). 도착 무적 1→2초, `TravelNow`(사망 귀환)에도 적용. `BossIntro` 배너·카메라 연출은 맵 이동 중(`isUsePortal`)이거나 플레이어와 20유닛 밖이면 생략(마을 출입마다 뜨던 '탑의 수호자' 배너, GUARD-BANNER). 초반 3맵(NearForest/SideRoad/NormalForest) 스폰 수 ×0.65·한 점 최대 2슬롯. 미검증: 플레이 모드 실측. 한계: 추적 정예·밤 습격·보스 부하 스폰은 안전지대 미적용. 테스트 `ArrivalSafeZoneTests`.
- (11-04 PV2) (P3) 산적 대장 보상 아이콘 없음: `Resources/Item/BanditPlunderMedal`·`ChiefsLastStandRing` 이미지·프리팹 없어 장착·드롭 때 Error. 아이콘 제작 필요(그래픽 사용자 검토).
- (11-04 PV2) (해결 11-04, PV3 검증) (P4) 일시정지 > 설정 화면 마지막 행 "뒤로" 가 하단 안내문(위/아래·좌우 조절·ESC)과 겹침(행 7개). `PauseMenu` 패널 높이 또는 행 간격 조정 필요.

- (11-04 SLOT-2) 신규 장비 4칸 패널(`ExtraEquipPanel`)은 코드 생성 임시 UI. 위치(인벤창 하단 중앙)·화면 겹침 미검증, 게임패드/키보드 커서 이동 없음(마우스 호버·클릭만), 아이콘/자물쇠 아트는 사용자 검토 후속.
- (11-04 SLOT-4) `ExtraEquipFx` 오라 링(LineRenderer 단색 펄스)·칭호 이름표(TMP, 머리 위 2.3유닛)는 임시 코드 연출. 위치·크기·가독성 화면 미검증, 아트 교체는 사용자 검토 후속. 신규 슬롯 아이템의 실제 효과(오라 이동속도 등)·부적/망토 첫 아이템·SLOT-5는 미구현. 기본 SLOT-3 은 탐험가 rank=층계 번호 가정.
- (11-04 PV3) (P3) `ExtraEquip.cs` ~~`ExtraEquipPanel`(신규 슬롯 4칸) 이 인벤 아이템 칸을 가림~~ → **해결(11-04)**: 장비 그리드 아래 빈 띠(290×66)로 이동·축소. 플레이 스샷 확인.
- (11-04 PV3) (P3) ~~`ExtraEquipFx.TitleTag` 영문 이름표가 한글 이름표와 겹침~~ → **해결(11-04)**: TitleHeight 2.3→2.85. 플레이 스샷 확인.
- (11-04 PV3) 설정 '뒤로' 겹침(P4)은 해결 확인(통과).
- (11-05 PV4) (P2) `SampleScene` 직렬화 `PlayerController.jumpForce=10` 이 코드 기본 13 을 덮어씀. `JumpReachTests`(13 가정)와 실제 불일치 → 탭 점프 상승 1.1~1.2, 풀홀드 1.62 라 1.6 계단은 탭으로 못 오름. 13 복원 또는 계단 하향 결정 필요(`Docs/Playtest/Floor1PlayVerify.md` PV4 #7).
- (11-05 PV4) (P4) 퀘스트 창 구역 접기는 Z·패드 A 만 동작(UI.Select 에 Enter 바인딩 없음). 문구/바인딩 정리.
- (11-05 SLOT-R) 신규 4슬롯 패드 커서 이동·드래그 장착은 여전히 미구현·미확인. 폐기 장신구 반환 시 가방 가득이면 판매값/2 골드(임시).
