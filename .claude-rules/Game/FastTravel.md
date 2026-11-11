# 빠른 이동: 수레(마차) 형제 NPC · 히든 포털 (REWORK-14, 설계만 — 구현 없음, 11-11)

근거: 사용자 11-08 요구, `FloorTransition.md`(포털·귀환·안전지대), `WorldSpec.md` §2, 코드 `Map/{Portal,MapInfo,PortalSeal,MapTransit}.cs`, `NpcQuest/NpcQuestLog.cs`(ES3 `npcq.state`), `Dialogue/NpcInteraction.cs`. 메이플 기준(클래식 택시·히든 포털)은 사이트 공개 정보 범위의 **임시 가정**이다(Q-FT1~5, `Docs/OpenQuestions.md`).

## 1. 한 줄 요약

각 지역에 마차 NPC(형제) 한 명. 걸어서 가 그 NPC 의 퀘스트를 깨면 그 지역 정류장이 열린다. 열린 뒤엔 **어느 마차 NPC 에게서든** 열린 정류장으로 골드를 내고 이동. 같은 맵 안 이동 보조는 히든 포털(별도 구조, §6).

## 2. 정류장(지역) 표

정류장 ID 는 맵 이름과 1:1(`MapInfo.GoToMap(mapName)` 재사용). 순번은 요금 계산용.

| 순번 | 정류장 ID(맵) | 층계 | 비고 |
|---:|---|---|---|
| 0 | `Town` | 1 | 항상 열림(기본 정류장) |
| 1 | `Lv1_NearForest_SideRoad` | 1 | 항상 열림(마을 바로 옆, 퀘스트 없이) |
| 2 | `Lv1_NormalForest` | 1 | 퀘스트 `FT_Q_Normal` |
| 3 | `Lv1_DeepForest` | 1 | `FT_Q_Deep` |
| 4 | `Lv1_Swamp` | 1 | `FT_Q_Swamp` |
| 5 | `Lv1_BanditFortress` | 1 | `FT_Q_Fort` |
| 6 | `Lv1_TowerOutskirts` | 1 | `FT_Q_Tower` |
| 7 | `Lv2_MagicCliffs` | 2 | `FT_Q_Cliffs`(2층계 진입=문지기 처치 후만 도달 가능) |
| 8 | `Lv2_BrokenRampart` | 2 | `FT_Q_Rampart` |

- 맵 이름은 씬 확인값이다. 씬에 없는 맵(MistSwamp 등)은 정류장에서 제외, 맵이 생기면 한 줄 추가.
- 마차 NPC 는 각 맵의 **입구 포털 근처 안전 지점**(도착 안전지대 반경 안)에 둔다. 몹 스폰 제외.

## 3. 열림 규칙

1. **퀘스트 완료 = 해금.** 각 마차 NPC 에게 `QuestsFloor1.csv` 형식 퀘스트 1건(`kind=side`). 내용은 "지역 몹 N 마리 처치" 또는 "수집 N 개 전달" 중 하나(지역 적정 레벨, 보상은 소량 골드).
2. 퀘스트는 **그 지역에 직접 걸어가야** 받을 수 있다(해금 전엔 다른 마차 NPC 목록에 잠금 줄로 이름만 노출, 선택 불가).
3. 올라가기 봉인과 충돌하지 않는다: 2층계 정류장은 `bossFirstKill.Gatekeeper_BD`(문지기 첫 처치)를 만족해 **걸어서 도착한 뒤** 퀘스트를 깨야 열린다. 마차 이동 자체가 봉인을 우회하는 일은 없다(해금 못 했으면 목록에 없음).
4. 한 번 열면 닫히지 않는다. 퀘스트 포기·리셋과 무관(해금 플래그를 퀘스트 상태와 분리 저장).
5. 임시 가정: 레벨 제한은 두지 않는다(퀘스트가 레벨 요구를 가짐 — `NpcQuestStatus.LevelLow` 재사용).

## 4. 이동·요금 규칙

- 출발지 = 마차 NPC 가 있는 맵(대화 → "이동" 선택). 목적지 = 열린 정류장 중 **현재 맵 제외**.
- 요금(임시 가정, 메이플 클래식 택시 '거리 비례 소액' 취향): `fee = 50 + 25 × |순번차|` 골드, 2층계 도착은 ×2. 예) 마을→늪(순번 4) 150, 마을→절벽(순번 7) 450.
  - 근거: 일반 몹 골드/킬 `레벨×(3~5)+2`(Lv5 약 9, Lv12 약 20) → 요금은 사냥 5~20회분. 도보 이동이 더 빠른 가까운 구간(순번차 1)도 75 로 부담 없음. 밸런스 조정 가능(§8 CSV).
- 골드 부족: 줄 비활성 + "골드가 부족합니다" 문구. 이동 중 취소 없음(암전 직후 즉시 이동).
- 이동 처리: `MapInfo.GoToMap(destMap, player)` 재사용(암전·스포너 정리·카메라 스냅·`ArrivalSafeZone` 등록·이동 1초 뒤 `SavePos`). 도착 위치 = 목적지 맵의 마차 NPC 옆(정류장 앵커, §8 `FastTravelStops.csv` 의 x,y).
- **금지 상황:** 보스 30유닛 안(`RecallRules.BlockedByBoss` 와 같은 조건), 전투 직후 3초(`PlayerStatus` 피격 플래그), 사망·대화 외 UI 열림. 거부 시 사유 문구.
- 귀환 주문서·마을 귀환과 역할 분리: 주문서=마을로만(소모품), 마차=정류장 간 왕복(골드). 마을 마차 NPC 는 마을 정류장 겸 허브.

## 5. 3경로 입력(마우스·키보드·패드)

모든 UI 는 세 경로 동시 설계(`Docs/Playtest/InputAudit.md` 키 충돌 확인 필수).

| 동작 | 마우스 | 키보드 | 패드 |
|---|---|---|---|
| NPC 대화 시작 | NPC 클릭 | ↑(NPC 근처, 기존 상호작용 키 — 포털 ↑와 겹치지 않게 NPC 앞은 포털 반경 밖) | 기존 상호작용 버튼 |
| 정류장 선택 | 줄 클릭 | ↑↓ 이동, Enter 확정 | D패드/스틱 이동, A 확정 |
| 취소 | 바깥 클릭·✕ | Esc | B |
| 잠금 줄 | 회색, 클릭 시 "○○의 마차 퀘스트를 먼저 깨세요" | 같음 | 같음 |

- 창 = `NpcMenu` 에 "이동" 항목 추가(새 창 아님, 구현 단계 P1 에서 `NpcInteraction.Run` 에 `travel` 콜백 추가 대신 별도 `Action`).
- 줄 표시: 정류장 이름 · 요금 · (골드 부족 시 빨간 요금). 현재 맵은 목록에서 숨김.

## 6. 히든 포털 (같은 맵 내 이동 보조, REWORK-10 연계)

메이플: 맵 안 숨은 포털이 같은 맵 다른 발판·구석으로 보내 줌(보통 눈에 안 띄고 ↑ 키로 사용). 우리 `Portal` 은 `openMap` 이 비면 같은 맵 안 위치 이동이므로 **새 코드 없이 데이터만으로 가능**하다.

- 구조: 같은 맵 안 양방향 한 쌍(A↔B). 맵당 0~2쌍. 사냥터 긴 맵(가로 스크롤)의 양 끝·고발판 연결용.
- 노출: 평소 표식 없음(반투명 약한 입자 1개 정도), 가까이(반경 2) 가면 ↑ 힌트. 하이라이트는 `PortalLabel` 숨김 모드.
- 위치 규칙(임시 가정): 막다른 고발판↔지상 복귀, 맵 양 끝 ↔ 중앙 포털 근처. 보스방·보스 영역 포털 금지(체력 리셋 악용, `EnemySpec` §보스 영역 규칙과 같은 이유).
- 요금·잠금 없음, 마차와 무관. 도착 시 기존 포털과 같은 무적 2초.
- 데이터: `HiddenPortals.csv`(`map,id,ax,ay,bx,by,note`) → 로드 때 `Portal` 컴포넌트 두 개를 생성(`PortalFactory`). 실제 좌표는 REWORK-10 맵 점검(`MapReview.md`)이 정한다.

## 7. 세이브(ES3, 추가만)

| 키 | 형태 | 비고 |
|---|---|---|
| `ft.stops` | 문자열 `맵ID;맵ID;…`(열린 정류장, Town·SideRoad 는 암묵 열림이라 저장 안 함) | 신규. 퀘스트 완료 시 추가. 순서 무관, 중복 제거 |
| `ft.last` | 문자열(마지막 이용 정류장) | 선택. 목록 기본 선택 위치용. 없어도 무방 |

- 기존 키(`npcq.state`, `bossFirstKill.*`, `explorer.*`) 불변. 구세이브: `ft.stops` 없음 = 암묵 열림만 → 정상 동작. 문제: 이미 먼 지역을 걸어서 지나간 구세이브는 해금 퀘스트를 다시 해야 함(임시 가정: 허용. 대안: 해당 맵 퀘스트를 `Done` 으로 판정된 `npcq.state` 가 있으면 자동 해금 — 구현 쉬움, 권장).
- 해금 판정은 `ft.stops` 한 곳(`FastTravelState`)에서만 읽고 쓴다. 퀘스트 완료 훅이 `Unlock(mapId)` 호출.

## 8. 데이터 파일(구현 시)

- `FastTravelStops.csv`: `id(맵명),order,floor,displayName,anchorX,anchorY,questId,npcName` — 정류장·요금 순번·NPC 위치. 컬럼 추가는 맨 뒤.
- 요금 상수는 코드 상수 대신 `FastTravelRules`(순수 C#, EditMode 테스트 가능): `Fee(fromOrder, toOrder, floor)`.
- `HiddenPortals.csv`: §6.

## 9. 구현 단계

| 단계 | 내용 | 검증 |
|---|---|---|
| P0 | 순수 규칙: `FastTravelRules.Fee`·`FastTravelState`(열림 집합, ES3 직렬화) + EditMode 테스트(요금·해금·구세이브 자동 해금) | EditMode |
| P1 | `FastTravelStops.csv` 로더, 마차 NPC 프리팹(`TalkNpc` 기반) 마을·SideRoad 2곳 먼저, `NpcMenu` "이동" 줄(마우스·키보드·패드) | play probe: 대화→이동→골드 차감→도착 |
| P2 | 퀘스트 `FT_Q_*` 8건 CSV 추가, 완료 훅→`Unlock`, 잠금 줄 표시 | probe: 퀘스트 완료 후 줄 활성 |
| P3 | 나머지 지역 NPC 배치(안전지대 좌표 확인), 금지 상황(보스 근처·전투 직후) | 맵별 캡처(낮 시간대) |
| P4 | 히든 포털 데이터 `HiddenPortals.csv` + 생성 코드, 맵 점검 결과 좌표 반영 | probe: 맵 내 이동·무적 |
| P5 | 구세이브 자동 해금, 실마우스·패드 입력 점검, 요금 튜닝 | 사용자 확인 |

## 10. 질문·임시 가정(OpenQuestions 등록)

- Q-FT1 해금 퀘스트 종류(처치/수집/방문)와 NPC 가 형제로 보이는 외형 — 임시: 처치/수집, 외형은 그래픽 방.
- Q-FT2 요금 공식 `50+25×순번차`(2층계 ×2) — 임시, 튜닝 가능.
- Q-FT3 이동 금지 상황 — 임시: 보스 30유닛·전투 직후 3초.
- Q-FT4 구세이브 자동 해금 — 임시: 자동 해금 채택.
- Q-FT5 히든 포털 맵당 개수(0~2)·표식 강도 — 임시.

## 11. 구현 상태 (11-11, builder)
- 완료: P0(`FastTravelRules`·`FastTravelState`·EditMode `FastTravelTests` 8건), P1(`FastTravelStops.csv` 로더·`FastTravelNpc`·`FastTravelFlow`, `NpcMenu` 행 모드·"마차로 이동"·`ConfirmPopup`), P2(`FT_Q_*` 7건 `QuestsFloor1.csv`, giver `coach_*`, 완료→`FastTravelHost` 가 `NpcQuestLog.Changed` 로 AutoUnlock), P3 일부(보스 30유닛·피격 3초·사망 금지), P5 일부(구세이브 자동 해금).
- 설계 정정: 해금 퀘스트는 표대로 **7건**(Normal·Deep·Swamp·Fort·Tower·Cliffs·Rampart, 설계 본문 "8건"은 오기). 퀘스트는 EXP 0·골드만(EXP 합 핀 테스트 유지), 처치 5마리, 선행 없음(걸어가야 받음).
- 마차 NPC 는 `FastTravelStops.csv` 의 `npcName`(=NPC id)·anchorX/Y 로 부트 때 배치(`NpcQuestPlacer.CreateCoaches`, 상인 도트 + 색조 임시). anchor 는 각 맵 입구 포털 +4 유닛, Y 는 지면 스냅. 도착 위치는 `GoToMap` 기본(입구 포털) 그대로.
- ↑ 상호작용은 기존 `TalkNpc` 의 `InteractArbiter.Contend` 를 그대로 사용(`NpcInteraction.Run` 이 `FastTravelNpc` 를 보고 Travel 항목 추가). 3경로 입력은 `NpcMenu`(↑↓·Enter·A·마우스·Esc·B)·`ConfirmPopup`(←→·Z·A) 공용.
- 미구현: P4 히든 포털, 마차 NPC 실좌표 맵별 캡처(낮), 실패드·실마우스 확인, 이동 중 재진입 가드(리뷰 권장), ES3 `ft.*` 슬롯 전환 실기.

## 12. P4 히든 포털 구현 (11-11 builder)
- 데이터 `Resources/HiddenPortals.csv`(`map,id,ax,ay,bx,by,note`) → `HiddenPortalTable`(파싱·검증: 두 끝 6유닛 이상, id 중복 제거) → `HiddenPortals.Boot` 가 부트 때 맵마다 `Portal` 두 개를 만든다(씬 수정 없음, `openMap` 비어 같은 맵 이동·도착 무적 2초 그대로).
- 높이는 맵이 처음 켜질 때 지면+1.1 로 맞춘다(`HiddenPortalSnap`). 표식: `PortalLabel` 숨김 모드(평소 옅은 "·", 반경 2 안이면 "↑ 건너편").
- 입력: 키보드·패드 ↑ = `Portal.Update`(`InteractArbiter.Contend`), 마우스 = 범위 안에서 포털 자리 좌클릭(`HiddenPortalClick`).
- 배치(임시, Q-FT5): `Lv1_NearForest_SideRoad`·`Lv1_NormalForest` 각 1쌍 `(-230, 높은 막다른 발판) <-> (-355, 길)`. 보스방 금지는 수동(보스 맵 줄을 CSV 에 넣지 않는다). 미검증: 일반 숲 실좌표 지면, 실패드·실마우스.
