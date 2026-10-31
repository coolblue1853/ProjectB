# Project B — Codex 작업 지침

Claude Code 가 지시를 쓰고, Codex 가 구현·검증을 맡는다(10-31 사용자 결정, Claude 사용량 절약). 보고는 **한글**로 한다.

## 먼저 읽기

- 규칙 인덱스: `.claude-rules/AGENTS.md`. 작업과 관련된 문서를 찾아 **읽고** 시작한다.
- 항상 지킬 것: `.claude-rules/Framework/CodeStyle.md`, `.claude-rules/Framework/EditorAutomation.md`, `.claude-rules/Framework/ContextEconomy.md`.
- 모션·이펙트 작업이면 `.claude-rules/Game/MotionFxChecklist.md` 필독. 기본 3타·스미어 v7은 덮어쓰지 않는다.
- **사용자 취향·결정 메모리**(Claude 와 같은 맥락): `C:\Users\user\.claude\projects\C--Users-user-ProjectB\memory\MEMORY.md` 색인을 읽고, 작업과 관련된 항목 파일을 읽는다(읽기만, 수정 금지). 특히 `classic-maple-direction`, `game-pillars`, `early-game-modest`, `abyss-mood-not-childish`, `keep-basic-motion-fx`, `test-daytime`, `no-focus-steal`.
- 리뷰 기준: `.claude/agents/reviewer-code.md`, `.claude/agents/reviewer-arch.md` 체크리스트를 쓴다.

## 품질 기준 (속도보다 우선)

- 빨라도 품질이 낮으면 의미 없다(사용자 10-31). 추측으로 끝내지 말고 코드·데이터를 읽고 확인한다.
- 게임 감: 진행은 클래식 메이플(레벨·퀘스트·몸박·물약), 스킬은 장비에서, 공격은 답답하지 않게. 초반은 작고 소박하게, 유아틱 금지(메이드 인 어비스풍 몽환·섬뜩).
- 기존 코드 스타일·구조를 따르고 재사용한다. 새 시스템을 만들기 전에 비슷한 기존 것을 찾는다.
- 숫자로 검증한다. 확인 못 한 것은 반드시 "미검증"으로 적는다.

## 환경 메모

- uv 캐시는 `Logs/uv-cache`(run.sh 가 `UV_CACHE_DIR` 설정). PowerShell 로 한글 파일을 읽을 때 `-Encoding UTF8`.

## 하지 말 것

- **커밋·push 금지.** 커밋은 Claude 가 한다. `git stash`·`reset`·브랜치 전환 금지.
- 건드리지 말 것: `Assets/Scenes/*.unity`(필요한 변경이 아니면), 폰트 asset, `ES3Defaults.asset`, `M_PixelSlash.mat`, `Tools/keep_going.sh`, `Docs/ArtCompare/`, `.claude/`, 서드파티 폴더(Plugins 등).
- `C:\Users\user\Downloads\PurpleCow_MadDucks-main` 는 읽기 전용.
- PixelLab 등 유료 생성 도구 금지. 그림이 필요하면 코드로 절차 생성하거나 기존 스프라이트 재사용.
- Unity 창을 앞으로 가져오지 않는다.

## 검증 (필수)

Unity 에디터는 이미 켜져 있고 unity-mcp 로 조작한다.

1. 시작 전 세이브 백업: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/ProjectB/*.es3` 를 임시 폴더에 복사.
2. `bash Tools/u.sh call refresh_unity '{}'` 로 컴파일 → `bash Tools/u.sh call read_console '{"types":["error"]}'` 에러 0.
3. EditMode 테스트 전체 통과(`run_tests`). 새 규칙 로직에는 테스트를 추가한다(`Assets/Editor/Tests/`).
4. 플레이 확인은 프로브(`Tools/probes/*.cs`, `bash Tools/u.sh cs <파일>`)로 숫자를 찍는다. 화면 확인은 낮 시간대로 바꾼 뒤 스크린샷을 `Logs/playtest/` 에.
5. 끝나면 플레이 정지, 세이브 파일을 백업본으로 복원(md5 확인).

## 보고 형식 (마지막 메시지)

- 바뀐 파일 목록(신규/수정), 검증 숫자, **미검증**, 커밋에서 뺄 일회성 프로브.
- 설계 결정은 해당 `.claude-rules/Game/*.md` 문서에 직접 기록한다. 기획 질문은 묻지 말고 `Docs/OpenQuestions.md` 에 임시 가정과 함께 적는다.
