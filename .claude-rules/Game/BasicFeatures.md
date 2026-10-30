# 기본 기능 점검 (메이플·던파·디아블로·몬헌 기준)

> 2026-10-30 조사. 사용자 방향: "메이플·던파·디아블로·몬헌 식에 필요한 게임 기본 기능들 — 퀵슬롯 같은 것도".
> 장르 기본인데 빠진 것을 우선순위로 정리하고, 위에서부터 같은 방식(설계 → 구현 → 테스트·프로브 → 커밋)으로 넣는다.
> 기획 판단은 `Docs/OpenQuestions.md` Q-430~ 에 임시 가정으로 남긴다.

## 1. 이미 있는 것

| 영역 | 기능 | 위치 |
|---|---|---|
| 조작 | 2단 점프·대시·사다리/로프·하향 점프, 입력 버퍼·코요테 | `Player/PlayerController.cs`, CombatSpec 1~2절 |
| 전투 | 무기 스킬 4슬롯(A/S/D/F) + 쿨다운 HUD, 평타 콤보, 아티팩트(Shift) | `UI/SkillCooldown.cs`, `Item/ArtifactSystem.cs` |
| 아이템 | 인벤토리(5페이지·정렬)·창고·상점·제작·강화 +0~+10·세트 2/5 | `UI/InventoryManager.cs`, `Inventory/InventoryService.cs`, ProgressionDesign |
| 툴팁 | 등급 색, 세트 진행도, 착용품 대비 ▲/▼ 비교 | `UI/TooltipBuilder.cs` |
| 소비 | 인벤에서 소비 키로 사용(HP·스태미나·포만감·횃불·방어 버프) | `InventoryManager.ActiveConsum` → `ItemCheck.ApplyConsumeEffect` |
| 줍기 | 닿으면 자동 줍기(인벤 가득이면 경고), 획득 로그 | `Enemy/DropItemCheck.cs`, `UI/PickupLog` |
| 진행 | 퀘스트 게시판, 도감(B), 층계 지도(웨이포인트), 탐험가 칭호, 보스 고유템 천장 | `Quest/`, `UI/Bestiary`, `UI/WaypointMap.cs` |
| 시스템 | 사망·마을 부활, 세이브 슬롯·백업, 일시정지·설정(음량·흔들림·번쩍임·언어) | `DB/SaveSlots.cs`, `UI/PauseMenu.cs`, `UI/GameSettings.cs` |
| 채집 | 돌·나무·약초 채집물(파괴 오브젝트) → 제작 재료 | EnemySpec, ItemSpec 5절 |

## 2. 기본인데 없는 것 (우선순위)

| 순위 | 기능 | 참고 게임 | 내용 | 상태 |
|---|---|---|---|---|
| 1 | **퀵슬롯 + 물약 쿨다운** | 메이플·디아 | 소비 아이템을 숫자 1~4 로 사용하는 HUD 4칸. 수량·0개 회색·공유 쿨다운 표시, ES3 저장 | ✅ 3절 |
| 2 | **줍기 범위(자석)·줍기 필터** | 디아·메이플 펫 | 가까운 드롭을 끌어와 줍기, 등급 필터(일반 재료 줍지 않기 등) | ✅ 4절 |
| 3 | 키 설정 화면 | 메이플 | 입력 액션 리바인딩(PerformInteractiveRebinding) + 저장(PlayerPrefs `SaveBindingOverridesAsJson`). Shift 겹침 Q-422 도 여기서 해소 | ✅ 5절 |
| 4 | 비교 패널(Shift 누르는 동안 착용품 툴팁 나란히) | 디아 | 툴팁 비교 수치는 있음. 옆 패널은 InventoryRefactorDesign 3.1 설계만 | ✅ 6절 |
| 5 | 제작 재료 추적(핀) | 몬헌 | 레시피를 고정하면 필요 재료 x/y 를 HUD 에 표시, 획득 시 갱신 | ✅ 7절 |
| 6 | 분해(장비 → 재료·강화석) | 디아·던파 | 쓰지 않는 장비 처리 경로. 지금은 판매뿐 | ✅ 8절 |
| 7 | 미니맵 | 메이플 | 맵 축소 + 플레이어·포털·보스 점. 맵 경계(`MapInfo`) 기준 | ✅ 9절 |
| 8 | 장비 프리셋(세트 저장·교체) | 던파 | 세트 파밍 게임의 장비 스왑 | ⏳ |
| 9 | 상점 되사기 | 디아 | 실수로 판 아이템 복구 | ⏳ |

의도적으로 넣지 않는 것: MP(스태미나 체계), 레벨·경험치(Q-103), 채널, 내구도·수리, HP/MP 자동 물약(메이플 펫 기능 — 퀵슬롯으로 대신).

## 3. 퀵슬롯 (1순위, 2026-10-30)

- 코드: `Assets/Script/UI/QuickSlot/QuickSlotBar.cs`(HUD·입력) + `QuickSlotModel.cs`(순수 모델, 테스트 대상) + `Assets/Script/Item/ConsumeCooldown.cs`(공유 쿨다운).
- 입력: `KeyAction.inputactions` Player 맵 `Quick1~Quick4` = 숫자 1~4(게임패드 미지정).
- 등록: 인벤토리가 열려 있을 때 ① 커서가 소비 아이템 위에 있으면 **Shift+1~4** 로 그 칸에 등록(인벤에서 숫자만 누르면 기존 페이지 전환), ② 인벤 칸의 소비 아이템을 HUD 칸으로 드래그(창고·장비칸에서 시작한 드래그는 제외, 마우스 드래그는 미검증). 소비(Consum)가 아닌 아이템은 거절. 같은 아이템이 다른 칸에 있으면 그 칸은 비운다.
- 사용: 인벤이 닫혀 있고 UI 잠금이 없을 때 1~4. 인벤에서 같은 이름의 첫 스택을 `InventoryManager.TryConsume` 으로 소비(→ `InventoryService.RemoveAt` → 저장 → `ApplyConsumeEffect`). 수량을 직접 깎지 않는다.
- 쿨다운: 효과에 `hp` 가 있으면 물약 그룹 6초(공유), 그 외 1초(연타 낭비 방지). 인벤의 소비 키도 같은 쿨다운을 탄다(우회 방지). Q-430.
- 표시: 화면 하단 가운데 4칸, 아이콘·수량(0 이면 회색)·키 번호·원형 쿨다운. 인벤토리가 열려 있으면 보이고(드래그 대상), 다른 UI(상점·대화·일시정지)가 열리면 숨긴다.
- 저장: ES3 키 `quickslot.items`(string[4], 빈칸 ""), 세이브 슬롯 파일별. 기존 키는 건드리지 않는다. 읽기 실패 시 빈 칸으로 시작(경고 로그).
- 검증: EditMode `QuickSlotModelTests`, 프로브 `Tools/probes/quickslot_probe.cs`(등록 → 사용 → 수량·쿨다운·세이브 키 확인).

## 4. 줍기 범위·필터 (2순위, 2026-10-30)

- 코드: `Assets/Script/Item/PickupRules.cs`(순수 규칙·수치, 테스트 `PickupRulesTests`) + `Enemy/DropItemCheck.Update`(자석 이동) + `UI/GameSettings.Pickup` + 일시정지 설정 행 "PICKUP".
- 자석: 바닥에 닿은 드롭이 플레이어 2.2유닛 안이면 끌려오기 시작(가속 30/s², 최고 14/s), 4.5유닛 밖으로 멀어지면 멈춤. 실제 줍기는 기존 접촉 경로(`DropItemCheck.OnTriggerStay2D` → `CreatItem`) 그대로. 인벤에 자리가 없는 아이템은 끌어오지 않는다(0.5초마다 재검사).
- 필터: 설정 "PICKUP" — 전부(기본) / 일반 재료 제외(type Misc + rarity C). 장비·소비·R/U 는 항상 줍는다. 거른 아이템은 끌려오지도, 주워지지도 않고 바닥에 남는다(120초 뒤 소멸). 저장은 ES3 `settings.pickup`(int, 공용 파일 `SaveSlots.SharedFile`). Q-431.
- 검증: 프로브 `Tools/probes/pickup_magnet_probe.cs`(전부: 나무 12→13 자석 줍기 / 필터: 나무 2.1유닛에 있어도 안 주움, 물약은 끌려와 주움 / 전부로 되돌리면 남은 나무 줍기).

## 5. 키 설정 (3순위, 2026-10-30)

- 코드: `Assets/Script/UI/KeyBind/InputBindings.cs`(저장·적용·겹침 처리) + `InputBindingsRunner.cs` + `KeyBindWindow.cs`(창), 테스트 `InputBindingsTests`.
- 진입: 일시정지 > 설정 > 키 설정. 위/아래 행, 좌/우 열(키보드·게임패드), 선택/ENTER 로 새 키 입력 대기(ESC 취소). 마지막 행 "기본값으로"·"뒤로".
- 대상: Player 맵 14개 — 점프·대시·달리기·공격·스킬 1~4·퀵슬롯 1~4·아티팩트·다음 아티팩트. 고정 키(이동 화살표·스틱, ESC·ENTER, 패드 Start, 인벤 열기 I·제작 K·포털 ↑, 하드코딩 B 도감·M 지도)와 겹치면 지정 거절. 인벤 안에서만 쓰는 UI 키(Z/X 등)와의 겹침은 허용(Q-432). 다른 대상 액션과 겹치면 **서로 바꾼다**(메이플식).
- 적용: 게임 곳곳의 `new KeyAction()` 인스턴스(20여 곳)를 고치지 않고 `InputSystem.onActionChange` 로 처음 켜질 때 잡아 다음 프레임에 오버라이드를 입힌다. 저장하면 살아 있는 인스턴스 전부에 다시 입힌다. 편집은 항상 비활성인 마스터 인스턴스에서 한다.
- 저장: ES3 `settings.bindings`(오버라이드 JSON, 공용 파일 `SaveSlots.SharedFile` — 세이브 슬롯과 무관). 읽기 실패 시 기본 키.
- 기본 키 변경(Q-422 해소): 아티팩트 Shift → **V**(패드 오른쪽 스틱 누름), 다음 아티팩트 Tab(패드 Select) — 둘 다 하드코딩에서 입력 액션으로. 퀵슬롯 패드: 1 = 왼쪽 스틱 누름, 2~4 미지정(설정에서 지정). HUD 의 키 이름(아티팩트·퀵슬롯)은 설정을 따른다.
- 대기 취소: 키보드 열은 ESC, 게임패드 열은 Start, 8초 지나면 자동 취소.
- 주의: 오버라이드 JSON 은 바인딩 id 로 맞춘다. inputactions 에서 바인딩을 지웠다 다시 만들면(id 재생성) 저장된 키 설정이 조용히 무시된다 — 기존 바인딩은 경로만 고칠 것.
- 검증: EditMode `InputBindingsTests`, 프로브 `Tools/probes/keybind_probe.cs`(퀵슬롯1 → 7 재지정 후 실제 7로 사용, V 아티팩트 발동, 기본값 복원, ESC 두 번으로 창·일시정지 닫기).
- 남은 한계: 인벤 등 UI 맵 키(Z/X/I/화살표)와 하드코딩 키(B 도감, M 지도, 인벤 숫자 페이지)는 재지정 대상이 아니다.

## 6. 비교 패널 (4순위, 2026-10-30)

- 코드: `Assets/Script/UI/ComparePanel.cs`(코드 생성 HUD, 정렬 60) + `InventoryManager.EquippedFor(부위)`.
- 동작: 인벤에서 장비 상세 창(state "detail")을 보는 중 Shift(게임패드 LT)를 누르고 있으면 같은 부위 착용품의 이름·부위·등급·스탯 패널을 띄운다. 상세 창 왼쪽에 자리가 있으면 왼쪽, 없으면 오른쪽 아래(상세 창 바닥 기준 — 오른쪽 위는 무기 스킬 상세 창 자리). 상세 창의 ▲/▼ 수치 비교는 그대로.
- 비용: 착용품 조회·문구 생성은 상세 아이템이 바뀌거나 Shift 를 누른 순간에만 한다.
- 한계: Shift·LT 는 하드코딩(키 설정 대상 아님, Q-432). 세트 진행도·무기 스킬 줄은 아직 없다.
- 검증: 프로브 `Tools/probes/compare_probe.cs`(녹슨 검 착용 + 인벤 녹슨 검 상세 → Shift 표시·떼면 숨김), 스크린샷.

## 7. 제작 재료 추적 — 핀 (5순위, 2026-10-30)

- 코드: `Assets/Script/Item/CraftPinModel.cs`(순수, 테스트 `CraftPinModelTests`) + `Assets/Script/UI/CraftPinHud.cs`(HUD·저장) + `CraftingManager.CheckPinInput`.
- 조작: 제작 창(K)에서 레시피에 커서를 두고 **P**(패드 R3)로 고정, 같은 레시피에서 다시 누르면 해제. 한 번에 하나만 고정(Q-433).
- 표시: 오른쪽 위 칩 — "제작 핀  결과" + 재료마다 보유/필요(갖추면 초록). UI 가 열려 있으면 숨김. 0.5초마다 표시 캐시(inventoryItemStack)를 읽고 바뀐 때만 글자를 다시 만든다.
- 알림: 재료가 다 모이는 순간 획득 로그 "제작 가능: 결과"(U 강조) 한 번. 고정할 때 이미 모여 있으면 알리지 않는다.
- 저장: ES3 `craftpin`(세이브 슬롯 파일, "결과|재료*수|…"). 레시피 데이터가 제작 창 씬 오브젝트(NeedItem)에 있어서 고정 시점의 재료 목록을 그대로 저장한다.
- 같은 결과의 다른 재료 목록에서 P 는 교체, 같은 레시피는 해제. 제작 창에서 고정한 레시피에 커서가 오면 재료 목록을 조용히 다시 맞춘다(레시피 데이터 변경 대비). 부트 직후 1.5초는 알림 없이 기준만 잡는다(DB·인벤 로드 순서).
- 키 전제: 제작 창의 P/R3 는 인벤토리 정렬(R/R3)과 같은 하드코딩 UI 키다. 인벤과 제작 창은 동시에 열리지 않는다. P 는 키 설정에서 예약 키.
- 검증: 프로브 `Tools/probes/craftpin_probe.cs`(가상 키 K→P 로 토끼고기 스튜 고정 → 칩 1/1·0/2 → 재료 지급 후 2/2 초록·알림 → 해제), 스크린샷.

## 8. 분해 (6순위, 2026-10-30)

- 코드: `Assets/Script/Item/DismantleRules.cs`(순수, 테스트 `DismantleRulesTests`) + `DismantleService.cs`(실행) + `Assets/Script/UI/DismantleInput.cs`(입력·확인·알림).
- 조작: 인벤토리에서 커서가 장비 위일 때 **D**(패드 L3)를 2.5초 안에 두 번. 첫 번째는 "한 번 더 D: 이름 분해" 경고만. 착용칸·창고 장비는 대상 아님(착용 해제 후). 상세 창이 떠 있으면 닫는다.
- 결과(Q-434): C → 광석 1 / R → 광석 1 + 층계 결정 1 / U → 층계 결정 3. 광석은 티어 0 이면 T0_Ore, 1 이상이면 T1_Ore. 강화 2단계마다 층계 결정 +1(강화 비용 일부 환급). 받은 재료는 획득 로그에 표시.
- 패드 L3 는 Player/Quick1 기본 키와 같지만, 인벤이 열려 있으면 퀵슬롯은 Shift+키로만 등록하므로 겹치지 않는다. 미지 등급(레전더리 등)은 U 이상으로 친다.
- 한계: 가방이 꽉 찼으면 장비를 빼서 생길 칸을 미리 쓰지 못해 "가득 참"으로 실패한다(Q-434).
- 안전: 재료를 먼저 넣어 보고(InventoryService.Add, AddSource.Dismantle) 하나라도 자리가 없으면 넣은 것을 되돌리고 "인벤토리 가득 참". 그다음 장비를 `RemoveAt` 으로 뺀다. 총량 보존.
- 검증: 프로브 `Tools/probes/dismantle_probe.cs` + `DismantleInput.Request` 직접 호출(StoneCap R +4 → 장비 −1·T0 광석 +1·층계 결정 +3, 첫 요청은 확인만, 착용 무기는 거절), EditMode 3개. 가상 키 D 입력은 그 시점 에디터가 포커스 상태라(실제 키보드 상태가 덮음) 확인하지 못함 — 키 판정은 P(제작 핀)와 같은 방식.

## 9. 미니맵 (7순위, 2026-10-30)

- 코드: `Assets/Script/UI/Minimap/MinimapHud.cs`(HUD·굽기) + `MinimapMath.cs`(순수, 테스트 `MinimapMathTests`).
- 표시: 오른쪽 위 패널 — 맵 이름(MapInfo.displayName) + 지면 실루엣 + 점(플레이어 노랑·포털 하늘·NPC/퀘스트 게시판 초록·보스 빨강). 제작 핀 칩은 미니맵 아래로 밀린다(`MinimapHud.OccupiedHeight`). UI 가 열리면 숨김.
- 지면: 지금 맵의 태그 Ground 콜라이더(크기 0.5 이상)들로 영역을 정하고(흩어진 장식 타일이 영역을 늘리지 않게), 타일맵 지면은 칠한 칸, 그 밖의 발판은 경계 사각형을 1유닛=1픽셀(최대 512)로 굽는다. 맵이 바뀔 때만(Portal.MapChanged, 웨이포인트·부활 포함) 다시 굽는다.
- 갱신: 플레이어 점 매 프레임, 보스 점 1초마다(EnemyHealth.Active 의 살아 있는 isBossMob 중 영역 안 — 스포너가 몹을 씬 최상위에 만들어서), 포털·NPC 는 맵 변경 시(꺼진 포털·NPC 제외).
- 비용: 타일 순회는 타일맵 경계와 영역이 겹치는 칸만(40만 칸 넘으면 건너뜀). 텍스처가 512 로 잘려도 화면 비율은 월드 크기대로.
- 조작: **N** 으로 켜고 끔(키 설정 예약 키), ES3 `settings.minimap`(공용 파일). 기본 켬(Q-435).
- 검증: 프로브 `Tools/probes/minimap_probe.cs`(마을 75×17 영역·점 5, 외곽 샛길 618 유닛 → 512×64 텍스처·점 6, 맵 이름 한국어), 스크린샷 2장, 보스 스폰(PassiveTerror) 후 점 6→7.
