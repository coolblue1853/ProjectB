# CLAUDE.md

Project B 작업 지침. 모든 대화와 문서는 **한글**로 쓴다.

## 🔴 규칙 문서는 온디맨드 — 작업 전 반드시 Read

`.claude-rules/`의 문서는 자동 로드되지 않는다. 상시 로드되는 것은 아래 `@` 임포트뿐이다.

- 코드를 고치거나 시스템 동작을 설명하기 전에, `AGENTS.md` 인덱스에서 해당 문서를 찾아 **Read로 읽고** 작업한다.
- 인덱스 한 줄만 보고 추측하지 않는다. 여러 시스템이 얽히면 관련 문서를 전부 읽는다.

## 문서 관리

- 기획·설계 문서는 `.claude-rules/`의 `Framework/`(범용 규약) 또는 `Game/`(게임 고유)에 둔다. 새 문서를 만들면 `AGENTS.md`에 한 줄 추가한다.
- 인덱스 항목은 **백틱**으로 적는다. 마크다운 링크는 쓰지 않는다(링크는 전이 임포트되어 매 세션 로드된다).
- `.claude/rules/` 폴더는 만들지 않는다(디렉터리 전체가 자동 로드된다).
- 결정 사항은 그때그때 해당 문서에 기록한다. 대화는 압축되면 사라진다.
- 기획 질문은 바로 묻지 않고 `Docs/OpenQuestions.md`에 **임시 가정**과 함께 쌓은 뒤, 질문이 필요 없는 작업을 계속한다.

## 작업 방식

- **커밋:** 기능 단위로 커밋하고 push한다. 메시지는 `(추가)/(수정)/(삭제)/(문서) 요약` + 본문 bullet 형식으로 쓴다.
- **검증:** 버그 수정과 기능은 **Unity 에디터에서 실제로 확인**한다(unity-mcp: 콘솔 로그, 플레이 모드, 스크린샷). 확인하지 못했으면 커밋 메시지와 문서에 "미검증"으로 적는다.
- **사용량 절약(10-30 사용자):** Claude 사용량을 아끼려고, GPT(Claude in Chrome 의 ChatGPT 탭, Plus)로 해도 품질 차이가 없는 일은 GPT 에 맡긴다 — 기획서·수치표·리서치·레퍼런스 조사·이미지 리뷰·채점·초안 코드. Claude 는 Unity 통합·검증·커밋·판단만. 아끼는 순서 PixelLab > Claude > GPT.
- **참조 프로젝트:** `C:\Users\user\Downloads\PurpleCow_MadDucks-main`은 **읽기 전용**이다. 절대 수정·생성·커밋하지 않는다.

@../.claude-rules/AGENTS.md

@../.claude-rules/Framework/ContextEconomy.md
@../.claude-rules/Framework/CodeStyle.md
