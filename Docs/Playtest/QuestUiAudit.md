# 퀘스트 UI 감사 (11-04)

## 1. 보드와 NPC 퀘스트가 갈라진 지점
| 영역 | NPC 퀘스트 | 게시대 현상 의뢰 |
|------|-----------|-----------------|
| 데이터 | `QuestsFloor1.csv` → `NpcQuestCatalog` | `Floor1Bounties.csv` → `BountyBoard.Defs` |
| 진행 저장 | `NpcQuestLog` (ES3) | `BountyBoard.State` (Offered/Active/Done/Progress) |
| 수락·보고 | NPC 대화(`NpcQuestGiver`, PixelCrushers) | 게시대 `BountyPanel` |
| 목록 UI | Q 창(`NpcQuestWindow`, J) — 예전엔 진행 중/완료 2탭, 현상 의뢰는 보이지 않음 | 게시대 앞 패널에서만 보임 |
| 머리 위 표시 | `NpcQuestGiver` ! ? | 없음 |
| 추적 HUD | `NpcQuestTrack` 최대 3개 | 연결 없음 |

결과: 플레이어는 "지금 받을 수 있는 일"을 한곳에서 볼 수 없고, 현상 의뢰는 게시대에 가야만 존재를 안다.

## 2. NPC_BLOCK 원인 (PT-BOT 통주 `Logs/ptbot/camp*_run.log`)
- 로그: `TALK ruen q=Q104 ok=True` → 직후 `TALK ruen q=Q105 ok=False` ×4 → `NPC_BLOCK`.
- 원인은 **게임 쪽** 상태 플래그였다. `NpcQuestGiver._offerShown` 은 한 번 true 가 되면 플레이어가 트리거(`OnTriggerExit2D`)를 벗어날 때까지 유지돼, 같은 NPC 의 **다른 퀘스트**(Q104 수락 직후 Q105)도 대화가 열리지 않았다(`Interact` 가 false). 봇은 NPC 위치로 순간이동해 트리거를 벗어나지 않아 계속 막혔고, 실제 플레이어도 보고 직후 다음 수락이 안 되는 같은 문제를 겪을 수 있었다.
- 수정: `_offerShown`(bool) → `_offerKey`("퀘스트Id:상태"). 같은 퀘스트·상태는 한 번만 묻고(거절 후 상점 NPC 가 막히지 않는 기존 규칙 유지), 다른 퀘스트로 바뀌면 곧바로 다시 묻는다.
- Q305 `BranchAntlerFox_BD` 스포너 없음: 몹은 `HuntingSpawnsFloor1.csv` 가 기존 스포너에 섞는 보조 몹이라(`Fairy1/2`, `Goblin1`) 스포너 `MonsterProfile` 에는 없다. 게임 문제가 아니라 **봇의 탐색 문제**였고 `ptbot_camp.cs` 가 `HuntingSpawnTable` 도 조회하도록 고쳤다.

## 3. 개편 내용 (Q 창)
- 탭 3개: **받을 수 있는 / 진행 중 / 완료**. ←/→ 로 순환(`QuestListRules.Step`).
- 받을 수 있는 탭: 수락 가능이 먼저, 레벨 부족(회색)이 뒤. 게시대에서 오늘 받을 수 있는 현상 의뢰(`QuestBountyLink`, Id 접두사 `bounty:`, 읽기 전용 어댑터)를 같이 표시.
- 진행 중 탭: 완료 가능이 맨 위, 진행 중 현상 의뢰 포함. 현상 의뢰는 Z 추적 대상에서 제외(수락·보고는 게시대에서만).
- 같은 구역끼리 묶음(`QuestListRules.GroupByZone`).
- 세이브 호환: 저장 필드 변경 없음. 현상 의뢰 진행 저장은 `BountyBoard` 가 그대로 맡는다.

## 4. 검증 상태
- Unity 배치 컴파일 통과(에러 0). 플레이 모드 화면 확인은 **미검증**(에디터 비실행).

## 5. 남긴 범위
- Quest Machine 게시판 체인(`QuestCatalog`, 5건 안팎) 통합 안 함 — 저널 API 가 달라 별도 어댑터 필요. 후순위.
- 추적 HUD 에서 현상 의뢰를 NPC 추적 한도(3개)와 통합하는 것은 다음 작업.
- 순수 규칙 EditMode 테스트: `Assets/Editor/Tests/QuestListTabsTests.cs`.

## 5. 플레이 검증 (11-04, 프로브 `Tools/probes/_mp_questui.cs`)
- Q 창: 3탭 제목(받을 수 있는 9 / 진행 중 N / 완료 3) 표시, 구역(숲 샛길·깊은 숲) 묶음, 상세·추적 표시 정상(스크린샷 확인).
- 루엔 연속 제안: 본 흐름 Q202 → Q104 → Q105 → Q106 이 NPC 곁을 떠나지 않고 차례로 대화 시작(TryTalk=True). 같은 상태 재대화는 묻지 않음.
- 미검증: 현상 의뢰(보드) 항목 표시(활성 현상 없음), 실제 대화창 선택지로 수락하는 조작.
