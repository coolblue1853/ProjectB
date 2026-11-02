# 다른 계정에서 이어 작업하기 (11-02 인계)

Claude Code 계정을 바꿔도 이 저장소와 이 폴더만 있으면 그대로 이어서 할 수 있다.

## 1. 새 계정에서 처음 할 일

1. 같은 PC(같은 Windows 사용자)라면 메모리 폴더 `C:\Users\user\.claude\projects\C--Users-user-ProjectB\memory\` 가 그대로 남아 있다. 확인만 한다.
2. 다른 PC·다른 Windows 사용자라면 `Docs/Handoff/memory/*.md` 를 위 경로(새 PC 의 같은 위치, 폴더 이름은 저장소 경로에 따라 다름)에 복사한다. `MEMORY.md` 가 색인이다.
3. 첫 메시지로 이렇게 시킨다:
   > `Docs/Handoff/README.md` 읽고 이어서 작업해. 사용량 상한은 ○○%.
4. 프로젝트 규칙은 `.claude/CLAUDE.md` → `.claude-rules/AGENTS.md`(인덱스) 가 자동으로 읽힌다. Codex 규칙은 루트 `AGENTS.md`.

## 2. 일하는 방식 (사용자가 정한 것)

- 한국어, 짧게. 진행 여부를 묻지 말고 알아서 이어 간다. 기획 질문은 `Docs/OpenQuestions.md` 에 임시 가정과 함께 쌓는다.
- **Codex 우선, Codex 가 한도(LIMIT)면 Claude builder 에이전트로 바로 넘긴다.** 감시에 `bash Tools/codex/usage.sh --limit` 포함(11-02 Codex 대기로 5시간 날린 적 있음).
  - Codex: `bash Tools/codex/run.sh <이름> Logs/codex/<이름>.md` (지시서는 파일 경로로). 큐 `bash Tools/codex/queue_wait.sh 이름…` — 멈출 때는 queue_wait·run.sh·codex.exe exec 를 모두 꺼야 한다(하나만 끄면 다음 작업이 뜬다).
  - Claude: builder 에이전트(sonnet)에 "지시서 `Logs/codex/<이름>.md` 를 그대로 수행" 으로 맡기고, 보고를 보고 커밋.
- 커밋은 기능 단위, `(추가|수정|삭제|문서) 요약` + bullet + 검증/미검증 + `Co-Authored-By`. 커밋할 파일만 골라 add(씬·폰트·ES3Defaults·M_PixelSlash.mat·ProjectSettings·Docs/ArtCompare 삭제분은 사용자 작업 — 넣지 않는다).
- **SaveSlot3.es3 는 사용자 진행분.** 플레이 테스트 전 백업, 끝나면 md5 그대로 복원.
- 사용자가 플레이 중이라고 하면 Unity 플레이 종료·컴파일 금지(Codex·에이전트 전부 멈춤).
- 그래픽은 사용자 검토 후 적용. 전투 = 완전 횡스크롤이라 "메이플 + 던파 적당히"(CombatSpec §9). 인벤·상점·창고 = 메이플식 기능 최대(임시 UI 허용). 지금 목표 = **1층계 고도화**.

## 3. 11-01~02 한 일 (커밋)

- 새 주인공(던전 슬래셔풍 ds03) 걷기·달리기·공격 셀, 방향 기준 5장(`Docs/ArtCompare/CharacterGen/ds01/dirs/`, 3차가 기준·4차 후보, 소품 불일치는 보류).
- SR Debugger 치트(Unstuck 포함), 스태미너·ESC·착용 장비 강화 버그, 창고·인벤·상점(메이플식), HudPolicy(큰 창이면 HUD 숨김), 던파식 전투(캔슬·경직 감쇠·쿨다운 HUD), UI 2차(조작 안내 F10·퀘스트 탭/추적·HUD 숫자·레벨업 배너), 포털 라벨·낙사 복구·구덩이 포털·층계 지도, 1층계 콘텐츠(사냥 의뢰·숨은 상자·도감 보상·NPC 힌트·희귀 군락·밤 습격·보스 재처치 감소 — `Docs/Playtest/Floor1ContentList.md` 에서 켜고 끔), 몹 배치·밸런스(처치 3~6타, Lv15 약 4시간, 골드 드롭 40%), 첫 경험 안내(퀘스트 ?·첫 행동 힌트·사망 안내), 보스 HP·피해·예고, 통주 점검.
- 자세한 수치: `Docs/Playtest/Floor1Balance.md`, `Floor1Fullrun.md`, `Floor1NavReview.md`, 리뷰 남은 것 `.claude-rules/Game/TechDebt.md` 끝부분.

## 4. 다음 할 일 (순서대로)

1. **장비 HP 기준 정리(Q-F1H)** — 지시서 `Docs/Handoff/briefs/f1_hpbal.md` 를 `Logs/codex/` 로 복사해 실행. 늑대 세트만 껴도 HP 124→434 라 보스 피해 기준이 맞지 않는다.
2. OpenQuestions 사용자 판단 대기: Q-F1B(1→2층계 몹 HP 단차), Q-F1E(샛길 초반 물약비 부담), Q-F1H.
3. 미검증 확인: 나머지 보스 3종 실측, 첫 행동 힌트 실제 트리거, 대화 무적·루엔 의뢰 순서 인게임, 창고 마우스 드래그, 실제 키보드·패드 입력.
4. 그래픽(사용자 검토 필요): 새 주인공 로프 공격·사망 모션 흰 칼, 신규 몹·보스 그림(현재 코드 도트 임시), ds03 방향 그림 소품 통일.
5. PixelLab 남은 생성 약 143회(11-25 리셋). Codex 주간 한도는 11-05 무렵 풀림.

## 5. 파일 위치

- 지시서 원본: `Logs/codex/*.md`(Logs 는 커밋 안 됨 — 필요한 것만 `Docs/Handoff/briefs/` 에 복사해 둠).
- 메모리 사본: `Docs/Handoff/memory/` (11-02 기준).
- 자동화 기록: `Docs/AutomationLog.md`.
