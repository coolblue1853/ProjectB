# 총괄 교대 프롬프트 (매일 새 최상위 세션 첫 메시지로 붙여넣기)

사용자가 하루를 끝내면, 다음 날 아침 **새 최상위 세션**(사이드바 새 세션)을 열고 아래를 붙여넣는다. 날짜·누적 상한은 `AutoRun.md` 주간 배분표에서 고쳐 쓴다.
총괄이 다른 세션이 연 자식 세션이면 `start_session` 이 거부된다(사용자 직접 요청 턴만 허용). 그래서 반드시 사용자가 연 최상위 세션이어야 한다.

## 템플릿
```
.claude-rules/Framework/AutoRun.md, Dispatch.md, TokenEconomy.md 읽고 오늘(MM-DD) 총괄을 맡아. 인수인계는 Docs/Handoff/state.md 와 queue.md, 결정은 Docs/OpenQuestions.md 의 최신 [결정 MM-DD] 섹션(사용자가 직접 답한 것이니 그대로 따름). 오늘 주간 누적 상한 NN%, 하드스톱 98%. 하위 세션은 네가 start_session 으로 열어.
```

## 전날 마감 때 총괄이 할 일
1. `Docs/MorningQuestions.md` 에 다음 날 아침 질문(길고 자세히) 작성.
2. `Docs/Handoff/state.md` 3~5줄 갱신(완료·미검증·이월·진행 중인 builder/세션).
3. 아침 답변은 `Docs/OpenQuestions.md` 에 `[결정 MM-DD]` 로 기록 후 질문 파일은 `Docs/Handoff/archive/` 로 보관.
4. 끝난 세션은 보관(archive).

## 11-04 인수 메모
- 총괄 11-04(`local_0f1e9374…`, 자식 세션)는 하위 세션을 못 열어 builder 에이전트로 처리. 새 최상위 총괄이 열리면 이 세션은 보관.
- 완료: GUARD-BANNER·SAFE-ZONE(커밋 60c1f176, 플레이 모드 미검증, 배치모드 컴파일·EditMode 46 통과).
- 진행 불명: NPC_BLOCK+QUEST-UI builder가 `Assets/Script/NpcQuest/*.cs` 4개를 수정한 채 미커밋. 새 총괄이 diff 확인 후 완료·커밋 또는 이어서 처리.
- 유니티 에디터가 꺼져 있었음(UnityMCP ECONNREFUSED) → 플레이 검증 필요.
