# 에디터 자동화 (unity-mcp)

## 원칙

- 🔴 **Unity 창을 앞으로 가져오지 않는다.** 사용자가 같은 PC로 다른 작업을 한다. 포커스 강탈, 대화상자를 띄우는 메뉴 실행, 마우스·키 입력 주입은 금지한다.
- 대화상자가 필요한 작업은 조용한 API로 대체한다: `AssetDatabase.ImportPackage(path, false)`, `EditorWindow.Close()`.
- Game 뷰는 "Play Unfocused"로 둔다(`Tools/probes/play_unfocused.cs`). `PlayerSettings.runInBackground = true`.
- 🔴 **포커스 가드 상시 실행:** `powershell -WindowStyle Hidden -File Tools/focus_guard.ps1` (뮤텍스로 중복 방지). 사용자 클릭/Alt/Tab 없이 Unity 창이 앞으로 오면 직전 창으로 되돌리고 Unity를 비활성 최소화한다. 기록은 `Logs/focus_guard.log`, 끄려면 `Logs/focus_guard.off`. 셸의 cwd를 Unity가 옮길 폴더 안에 두지 말 것(“Moving file failed” 모달이 뜸).
- 🔴 **Unity 창은 최소화해 둔다:** `powershell -File Tools/minimize_unity.ps1`(포커스 없이 최소화). 컴파일·도메인 리로드 진행 팝업이 앞으로 튀어나와 키 입력을 가로채는 것을 막는다. 최소화 상태에서도 플레이·스크린샷이 된다(게임 시간은 느리게 흐름 → 대기 시간을 넉넉히).
- 🔴 **unity-mcp 서버의 포커스 넛지를 끈다.** 서버는 Unity가 백그라운드에서 느려지면 창을 앞으로 당기는 기능이 있다(테스트·새로고침 중). 반드시 `Tools/start_mcp_server.ps1`로 띄운다. 이 스크립트는 `UNITY_MCP_DISABLE_FOCUS_NUDGE=true`를 설정하고 숨김 창으로 실행한다.

## 구성

| 구성 요소 | 내용 |
|---|---|
| 패키지 | `com.coplaydev.unity-mcp` |
| 자동 시작 | `Assets/Editor/Dev/McpAutoStart.cs` — 브리지 자동 시작, 텔레메트리 끔 |
| 서버 | HTTP `http://127.0.0.1:8080/mcp`. `powershell -File Tools/start_mcp_server.ps1`(숨김, 넛지·텔레메트리 끔) |
| Claude 등록 | `claude mcp add --scope local`(저장소에 흔적 없음). 새 세션부터 `mcp__UnityMCP__*` 도구가 바로 보인다 |
| Bash 클라이언트 | `bash Tools/u.sh ...` (세션에 MCP가 없을 때) |

## 자주 쓰는 명령

```bash
bash Tools/u.sh call refresh_unity '{"mode":"force","scope":"all","compile":"request","wait_for_ready":true}'   # 컴파일
bash Tools/u.sh call read_console '{"action":"get","types":["error"],"count":"30","format":"plain"}'           # 에러
bash Tools/u.sh cs Tools/probes/state.cs                                                                       # C# 프로브 실행
bash Tools/playtest.sh 10 이름                                                                                  # 플레이 → 스크린샷 → 콘솔 → 정지
```

- 프로브(`Tools/probes/*.cs`)는 메서드 본문이다. 마지막에 `return 문자열;`로 결과를 돌려준다.
  - CodeDom(C# 6) 컴파일러이므로 `UnityEngine.Object`를 명시하고, 최신 문법은 쓰지 않는다.
- 플레이 중 검증: `manage_editor play` → 프로브로 상태를 조작·관찰(예: 적에게 데미지를 주고 드롭 확인) → `stop`.
- 스크린샷은 `Assets/Screenshots/`에 쌓인다(gitignore). 사본은 `Logs/playtest/`에 둔다.

## 디버그 로그 · 자동 장시간 테스트

- `GameLog.Info(Cat, msg)` — `PB_DEBUG` 심볼(Standalone 스크립팅 정의)일 때만 컴파일된다. 형식 `[PB][Cat] ...`. `Warn`은 항상 남고, `Anomaly`는 에러로 남아 자동 테스트에서 실패로 잡힌다.
- `RuntimeMonitor`(에디터·개발 빌드 자동 생성): 5초마다 `[PB][Monitor]`로 fps·최악 프레임·프레임당 GC·트윈 수·몬스터 수를 요약한다. 즉시 이상 보고: NaN·추락, 8초 넘게 유지되는 잠금 토큰, 몬스터 120마리 초과, 트윈 증가 추세.
  - ⚠️ 프로브 실행(`execute_code`)은 C#을 컴파일하느라 에디터를 약 1초 멈춘다. 그래서 감시기의 1초대 스파이크는 도구 착시일 수 있다. 실제 원인은 `Tools/probes/spike_top.cs`(프로파일러 최악 프레임 self time)로 확인한다.
  - 에디터에서는 프레임당 GC가 약 10KB 잡히는데 대부분 에디터 몫이다. 게임 코드의 할당은 `Tools/probes/gc_top.cs`(에디터 샘플 제외)로 본다.
- `bash Tools/soak.sh [사이클] [무기]`: 마을↔사냥터 왕복, 무작위 스포너 사냥, 밤 전환, 보스 지역을 반복한다. 끝나면 `Logs/soak/`에 에러, `[PB]` 경고, 모니터 요약을 집계한다. 세이브는 자동으로 백업·복원한다.

## 가상 키 입력 테스트 (2026-10-18)

- 에디터가 최소화·비포커스면 Input System이 키보드를 비활성화한다. `Tools/probes/input_bg_on.cs`(백그라운드 입력 허용 + 키보드 재활성) → `press_key.cs`(`__KEY__`=Key 이름, 0.1초 뒤 뗌) → 끝나면 `input_bg_off.cs`로 복구.
- 스크린샷에 오버레이 UI가 빠질 때가 있다(타이밍). 창을 연 뒤 2~3초 기다려 다시 찍는다.

## 셸 heredoc 백슬래시 함정 (2026-10-18)

- 이 환경의 Bash 도구는 heredoc 본문의 `\`를 한 번 풀어 `\`로 넘긴다. 파이썬으로 C# 문자열 이스케이프(`""`, `'
'`)를 쓸 때는 백슬래시를 `chr(92)`로 만들어 조합한다. 깨지면 "Newline in constant" 컴파일 오류가 난다.

## 열린 씬 파일을 밖에서 바꾸면 모달 (2026-10-19)

- 에디터가 연 `SampleScene.unity` 를 git 으로 되돌리거나 패치하면 다음 새로고침 때 "Reload / Ignore" 모달이 떠 refresh 가 멈추고 포커스 가드가 계속 창을 되돌린다. `powershell -File Tools/win_ui.ps1 dialogs` 로 확인 → `win_ui.ps1 click "ProjectB - SampleScene" "Reload"`(UIA 호출, 마우스 주입 아님).
- 씬 저장(`EditorSceneManager.SaveScene`)은 Quest Machine UI 프리팹의 RectTransform 오버라이드(레이아웃 재계산 값)를 같이 바꾼다. 의도한 변경만 커밋하려면 `git diff -U0` 에서 해당 hunk 만 골라 `git apply --unidiff-zero` 후 위 Reload.
