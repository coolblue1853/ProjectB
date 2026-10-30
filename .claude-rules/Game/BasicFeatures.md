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
| 3 | 키 설정 화면 | 메이플 | 입력 액션 리바인딩(PerformInteractiveRebinding) + 저장(PlayerPrefs `SaveBindingOverridesAsJson`). Shift 겹침 Q-422 도 여기서 해소 | ⏳ |
| 4 | 비교 패널(Shift 누르는 동안 착용품 툴팁 나란히) | 디아 | 툴팁 비교 수치는 있음. 옆 패널은 InventoryRefactorDesign 3.1 설계만 | ⏳ |
| 5 | 제작 재료 추적(핀) | 몬헌 | 레시피를 고정하면 필요 재료 x/y 를 HUD 에 표시, 획득 시 갱신 | ⏳ |
| 6 | 분해(장비 → 재료·강화석) | 디아·던파 | 쓰지 않는 장비 처리 경로. 지금은 판매뿐 | ⏳ |
| 7 | 미니맵 | 메이플 | 맵 축소 + 플레이어·포털·보스 점. 맵 경계(`MapInfo`) 기준 | ⏳ |
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
