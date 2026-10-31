# Project B — Codex 작업 지침

Claude Code 가 지시를 쓰고, Codex 가 구현·검증을 맡는다(10-31 사용자 결정, Claude 사용량 절약). 보고는 **한글**로 한다.

## 먼저 읽기

- 규칙 인덱스: `.claude-rules/AGENTS.md`. 작업과 관련된 문서를 찾아 **읽고** 시작한다.
- 항상 지킬 것: `.claude-rules/Framework/CodeStyle.md`, `.claude-rules/Framework/EditorAutomation.md`, `.claude-rules/Framework/ContextEconomy.md`.
- 모션·이펙트 작업이면 `.claude-rules/Game/MotionFxChecklist.md` 필독. 기본 3타·스미어 v7은 덮어쓰지 않는다.

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
