# 에디터 자동화 (unity-mcp)

## 원칙

- 🔴 **Unity 창을 앞으로 가져오지 않는다.** 사용자가 같은 PC로 다른 작업을 한다. 포커스 강탈, 대화상자를 띄우는 메뉴 실행, 마우스·키 입력 주입은 금지한다.
- 대화상자가 필요한 작업은 조용한 API로 대체한다: `AssetDatabase.ImportPackage(path, false)`, `EditorWindow.Close()`.
- Game 뷰는 "Play Unfocused"로 둔다(`Tools/probes/play_unfocused.cs`). `PlayerSettings.runInBackground = true`.
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
