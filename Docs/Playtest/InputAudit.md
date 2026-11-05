# 키보드·게임패드 조작 점검표 (AUTO-40, 11-05)

방식: `KeyAction.inputactions` 바인딩 + 코드 하드코딩(`kb.xKey`·`pad.xxx`·`new InputAction`)을 grep 으로 대조. **수정 안 함**, 실기 패드 검증 없음(미검증).
읽은 곳: `Assets/InputSystem/KeyAction.inputactions`, `BasicFeatures.md` §5·§13·§14, `Floor1PlayVerify.md` PV7, `ShieldGuard.cs:34-46`, `BestiaryWindow.cs`, `WaypointMap.cs`, `MinimapHud.cs`, `NpcQuestWindow.cs`, `CraftingManager.cs:750-760`.

## 1. 전투·이동 (Player 맵)

| 액션 | 키보드 | 패드 | 비고 |
|---|---|---|---|
| 이동 | ← → | 왼쪽 스틱 | 십자는 UI 맵만. 이동에 십자 없음(§3 항목 P3) |
| 점프 | C | A(South) | |
| 대시 | Z | B(East) | |
| 달리기 | Shift | Y(North) | |
| 평타 | X | X(West) | |
| 스킬 A/S/D/F | A/S/D/F | LB / **LT** / RB / **RT** | LT·RT 는 하드코딩 키와 겹침(§3) |
| 퀵슬롯 1~4 | 1~4 | L3 / 십자← / 십자→ / **없음** | 4 패드 미지정 |
| 아티팩트 / 다음 | V / Tab | R3 / Select | R3 겹침(§3) |
| 방어(방패) | G | LT | 하드코딩(`new InputAction`), 키 설정 대상 아님. **SkillS 와 같은 LT** |
| 인벤 | I | Start | |
| 제작 | K | 십자↓ | |
| 포털·NPC 대화 | ↑ | 십자↑ (UI/Up) | 포털은 `Portal/PortalOn` 키보드 ↑만, 패드 바인딩 없음 |
| 일시정지 | Esc | Start | 인벤 열기와 같은 Start (열림 상태로 분기) |
| 상호작용(채집·미니게임) | ↑ | 십자↑ | UI/Up |
| 도감 | B | **없음** | `BestiaryWindow` 키보드 전용. 안 탭 전환 Tab 도 패드 없음 |
| 지도(워프) | M | **없음** | `WaypointMap` 키보드 전용 |
| 미니맵 토글 | N | **없음** | `MinimapHud` 키보드 전용 |
| 퀘스트 창 | J | R3 | R3 겹침 |

## 2. UI 안 (인벤·상점·창고·제작)

| 액션 | 키보드 | 패드 |
|---|---|---|
| 선택 / 확인 | Z / Enter | A |
| 교체(Change) | X | X |
| 나누기 | C | Y |
| 뒤로 | Esc | B |
| 페이지 | Q / E | LB / RB |
| 창고 이동 활성 | Tab | LB |
| 소비 사용(ActiveConsum) | 없음 | LT |
| 제작 진입 | K | 십자↓ |
| 정렬 R / 분해 D / 보기필터 G / 핀 P | 키 | R3 / L3 / LT+R3 / R3 |
| 프리셋·되사기 | 숫자·U·] | Select·RT·LT+Y |
| 상점 O·H | O·H | **확인 안 됨**(`ShopConvenience.cs:77-84` 패드 분기 있음, 매핑 미확인) |
| 가방 확장 N | N | LT+X |

UI 탐색은 방향키/십자/스틱 → UI 맵 4방향 + `verticalCheck`·`horizontalCheck`. 대부분 창은 패드로 탐색 가능. 도감·지도는 열 수조차 없음(§3).

## 3. 문제 항목 (미지정·충돌·탐색 불가)

| # | 구분 | 내용 | 제안 키(1줄) |
|---|---|---|---|
| P1 | 충돌 | **LT = SkillS + 방패 방어 동시 발동.** `ShieldGuard` 가 `<Gamepad>/leftTrigger` 를 하드코딩하고 Player/SkillS 도 LT. PV7 "방패 패드 입력 못 함"의 유력 원인(확정은 실기 확인 필요) | 방패 장착 중에는 SkillS 의 패드 바인딩을 끄고 LT = 방어 전용(방패 없을 때만 SkillS) |
| P2 | 충돌 | **R3 = 아티팩트 + 퀘스트 창(J) + 제작 핀(P)** (+ 인벤 정렬 R, LT+R3 필터). Player 맵 R3 아티팩트와 퀘스트 토글이 같은 프레임에 둘 다 발동 | 퀘스트 창 = **Select 길게(0.8초)** 또는 LT+Start 로 이동 |
| P3 | 미지정 | 퀵슬롯 4 패드 없음 | 십자↑은 대화 예약 → **L3 길게** 또는 LT+십자← 조합 |
| P4 | 미지정 | 도감 B 패드 없음 | **LT+Select** (인벤 안에서 Select 는 프리셋이라 필드 한정) |
| P5 | 미지정 | 지도 M 패드 없음 | **LT+Start** |
| P6 | 미지정 | 미니맵 N 패드 없음(UI 비필수) | **LT+R3**는 인벤에서 필터와 겹침 → 필드 한정 **LT+L3** |
| P7 | 미지정 | 포털 ↑ 패드 바인딩 없음(Portal/PortalOn) | 십자↑ (UI/Up 과 같이 사용 가능한지 확인) |
| P8 | 탐색 | 도감 안 Tab(구역 탭) 패드 없음 | LB/RB 로 탭 전환 |
| P9 | 일관성 | Select 가 필드에선 다음 아티팩트, 인벤에선 프리셋 — 동일 버튼 이중 의미 | 문서화만(이미 UI 잠금으로 분기) |
| P10 | 이동 | 십자로 이동 불가(스틱만) | 이동 바인딩에 십자 ←→ 추가(퀵슬롯 2·3 와 겹침 → 이동 우선 시 퀵슬롯 재배치 필요) |
| P11 | 설정 | 방어 G·도감 B·지도 M·미니맵 N 은 키 설정 재지정 대상 아님 | 후순위 |

우선순위: P1(전투 직결) > P2 > P3 > P4·P5 > 나머지.

## 4. 판단 필요 → OpenQuestions
`Q-INPUT` 임시 가정 등록(방패 방어 패드 충돌 해결안 선택).
