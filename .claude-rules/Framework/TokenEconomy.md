# 절약 대원칙 (11-02 사용자 확정, 상시 로드 — 2KB 이내 유지)

목표: Pro 요금제로 주 6~7일, 하루 8시간 개발. **모든 결정에서 토큰 절약이 먼저다.**

## 1. 상시 로드 최소화
- 상시 로드 = `CLAUDE.md` + `AGENTS.md`(짧은 인덱스) + 이 문서 + `CodeStyle.md` + `MEMORY.md`. 늘리지 않는다.
- 상세는 온디맨드: 작업에 필요한 문서만 Read. 전체 인덱스는 `Framework/DocIndexFull.md`.
- 큰 파일은 `grep -n`으로 위치 찾고 그 줄 주변만 읽는다. 읽은 파일 재독 금지. `*.unity`/`*.prefab`/로그/서드파티 통독 금지.
- 명령 출력은 `| head`·`| tail`로 자른다. 콘솔은 error만.

## 2. 세션 = 담당자 (한 세션을 길게 끌지 않는다)
- 컨텍스트가 길수록 매 턴 비용이 커진다. 세션 하나는 **한 담당 업무 단위**로 끝낸다.
- 담당자 예: 모션·이펙트 / 1층계 콘텐츠·밸런스 / 코드·버그 / 문서·기획. 총괄(메인 세션)은 얇게 유지한다.
- 새 담당 세션은 총괄이 `start_session`으로 연다. 브리프에 넣을 것: 목표, 읽을 문서 2~3개, 끝났다는 기준, "보고 5줄 이내".
- 끝나면 결과를 `Docs/Handoff/state.md`에 3~5줄로 남기고 세션을 닫는다. 다음 세션은 이 파일 + 브리프만 읽고 시작.
- 컨텍스트 40% 넘거나 주제가 바뀌면 `/clear` 또는 새 세션. 5분 넘게 쉴 때(캐시 식음)도 마찬가지.
- 같은 파일을 건드리는 세션은 동시에 두 개 열지 않는다.

## 3. 모델·에이전트
- 기본 Sonnet, 노력 medium. Opus는 모션·그래픽 판정/큰 설계/어려운 버그만, 끝나면 복귀. 단순 조회는 Haiku.
- 서브에이전트: 구현 builder(sonnet), 검증 reviewer-code(haiku). 단순 탐색·읽기에 쓰지 않는다(약 7배 비용).
- MCP는 필요할 때만: pixellab은 평소 off(`python Tools/mcp_toggle.py on|off`), retro-diffusion 제거.

## 4. 출력
- 간결 모드(`.claude/output-styles/gando.md`) + caveman 플러그인. 완료 보고 1~2줄, 중간 보고 없음.
- 판단이 필요한 질문은 `Docs/OpenQuestions.md`에 쌓고 계속한다.

## 5. 측정
- 주간 사용량은 `mcp__ccd_session_mgmt__get_usage`로 본다. 큰 작업 전후 %를 `Docs/AutomationLog.md`에 한 줄 기록해 "1시간당 몇 %"를 추정한다.
- 목표 소모율: 하루 주간 한도의 약 14%(7일) 이하.
