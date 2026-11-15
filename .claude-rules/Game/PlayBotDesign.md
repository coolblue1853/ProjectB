# 플레이 기록기·봇 설계 (PlayBotDesign)

단계 계획·예산·합격 기준은 `Game/PlayBotPlan.md`. 이 문서는 **구현 형식**(R1 산출물)이다.

## 1. 목적
- 사람이 직접 플레이한 기록(jsonl)을 남기고, 요약 통계로 **사람처럼 노는 자동 플레이 봇**의 설계 입력(공격 간격·반응 시간·점프 빈도·방향 전환·UI 체류)을 얻는다.
- 봇은 **진짜 입력만** 쓴다. 치트·순간이동·직접 위치 변경·내부 함수 직접 호출 금지. 게임 로직 상태를 바꾸는 코드는 기록기에 두지 않는다(읽기·구독만).
- 이번(R1)은 기록기와 요약기까지다. 봇은 만들지 않는다.

## 2. 구성

| 파일 | 역할 |
|---|---|
| `Assets/Script/DebugTools/PlayRecorder.cs` | 부트스트랩·파일·입력·이벤트 구독·경고·막힘. `Enabled`·`Source` 정적 스위치 |
| `Assets/Script/DebugTools/PlayObservation.cs` | 게임 상태 **관측**(읽기 전용, 할당 없음). 봇이 그대로 재사용한다 |
| `Assets/Script/DebugTools/PlayRecSnapshot.cs` | 관측 → `k:"st"` JSON 직렬화 |
| `Assets/Script/DebugTools/PlayRecJson.cs` | JSON 한 줄 조립(이스케이프·무할당 숫자) + `PlayRecStuck`(막힘 판정). 순수 함수 |
| `Assets/Editor/Tests/PlayRecorderTests.cs` | 직렬화·이스케이프·막힘 판정 EditMode 시험 |
| `Tools/playrec_summary.py` | jsonl → markdown 요약(표준 라이브러리만) |

- 자동 시작: `RuntimeInitializeOnLoadMethod`(AfterSceneLoad), `DontDestroyOnLoad`. 플레이 모드·빌드 모두 기본 켜짐. 끄려면 `PlayRecorder.Enabled = false`(부트스트랩 전) — 실행 중에 바꾸면 기록만 멈춘다.
- 출력: 에디터 `<프로젝트>/Logs/playrec/yyyyMMdd_HHmmss.jsonl`(gitignore), 빌드 `Application.persistentDataPath/playrec/`. 1초마다 Flush, `OnApplicationQuit`에 닫는다.
- 비용: 스냅샷 1회(관측+직렬화) 약 2µs, 이벤트 한 줄 약 0.2µs(에디터 측정). 포털·NPC·드롭 캐시는 3초·0.75초마다 `FindObjectsByType` 한 번.
- GameLog 연결: `GameLog.Info` 가 `PlayRecorder.Log` 로도 보낸다(1순위 이벤트 경로). `Warn`/`Anomaly` 는 `logMessageReceivedThreaded` 로 잡힌다.

## 3. 로그 형식
한 줄 = JSON 한 객체. 공통: `t`(Time.time, 소수 3자리) · `k`(종류) · `src`(`human`|`bot`).

| k | 필드 | 설명 |
|---|---|---|
| `in` | `a` 맵/액션, `ph` S·P·C(시작·수행·취소), `dev` kb·pad·mouse·other, `v` 또는 `x`,`y` | 모든 InputAction. 같은 이름 중복 인스턴스는 같은 프레임·같은 값 1회로 줄임. 포인터 Vector2(마우스 이동)는 제외. 축 Performed 는 변화량 0.25 이상·0 교차만 |
| `st` | `map` `x` `y` `vx` `vy` `gr`(접지) `rope` `lad` `face`(±1) `fsm` `fl`(I무적 S슈아 M이동잠금 L로프잠금 U입력잠금 D대시잠금) `hp` `hpm` `mp`(스태미나) `mpm` `lv` `exp` `gold` `ui`[창] `en`[{id,dx,dy,hp%}x3] `pt` `npc` `drop`([dx,dy], 없으면 생략) | 10Hz. 플레이어 없으면 `pl:0`+`ui` 만 |
| `ev` | `e` 이름, `a`, `v`, `b` | 아래 이벤트 표 |
| `log` | `cat`(GameLog.Cat), `msg`(300자) | GameLog.Info 미러 |
| `warn` | `ty` Warning·Error·Exception·stuck·snapshot, `msg`(200자), `stk`(스택 첫 줄), `n`(누적) | 같은 메시지는 처음 5번 + 100번째마다. `stuck` 은 `map x y fsm mv` |

`ev` 이름: `dmg`(a 대상, v 피해, b basic/skill[+crit]) · `hurt`(v) · `kill`(a 몹 id) · `pick`(a 아이템) · `lvup`(v 레벨) · `death` · `revive`(a town/here) · `map`(a 맵) · `qacc`/`qdone`(a 퀘스트 id) · `npc`(a 이름) · `buy`(a,v 수량) · `sell`(a,v) · `equip`(a 아이템 id, b 슬롯) · `skill`(a 이름, v 슬롯) · `use`(a 소비 아이템).

- 막힘: 이동 입력(`Player/Move` |값|>0.3)을 2초 넘게 누르는데 위치 변화 < 0.2. UI 창이 열렸거나 로프 위·대시 중이면 제외. 한 번 알린 뒤 움직이거나 입력을 떼야 다시 무장한다.
- 열린 창 판정: `UiLock.Held`(창마다 소유자 토큰). 대화·퀘스트 창이 UiLock 을 안 쓰면 `ui` 에 안 보인다(R2 에서 보강).
- 알려진 한계: `dmg` 의 스킬명 없음(`skill` 이벤트와 시각으로 맞춘다). 도트 피해 처치는 `kill` 에서 빠진다(`EnemyHealth.Killed` 는 직격만).

## 4. 요약 스크립트
```
python Tools/playrec_summary.py Logs/playrec/<이름>.jsonl [출력.md]
```
기본 출력 `Logs/playrec/<이름>_summary.md`(약 100줄 이내). 담는 것: 총 시간·맵별 체류, 액션별 입력 횟수·평균 누름·장치, 평타 간격(중앙값·p10·p90), 반응 시간(적 |dx|≤6 진입 → 첫 공격/스킬 입력, 5초 안), 점프·대시·방향 전환 빈도(분당), 처치/분·피해·사망, UI 창 열기와 체류, 막힘·경고·예외(중복 묶음). 잘린 마지막 줄은 건너뛴다.

## 5. 2단계: 봇 계획 메모
1. 가상 `Keyboard`(필요하면 `Gamepad`)를 `InputSystem.AddDevice<Keyboard>()` 로 만들고, `InputSystem.QueueStateEvent(kb, new KeyboardState(...))` 로 키를 누르고 뗀다. 게임은 실제 키 입력과 구분할 수 없다. 에디터 비포커스 입력은 `Framework/EditorAutomation.md` 의 `input_bg_on` 방식을 참고.
2. 관측은 `PlayObservation` 을 그대로 쓴다. 정책은 요약 통계(간격 분포·반응 시간)로 샘플링한다. 적 반응 지연 = 반응 시간 분포, 평타 간격 = 간격 분포에서 뽑는다.
3. 봇도 `PlayRecorder.Source = "bot"` 로 같은 jsonl 을 남긴다. 같은 요약기로 사람과 봇을 비교해 분포가 어긋나는 항목(점프 빈도·방향 전환 등)을 조정한다.
4. 금지 재확인: 위치·HP·아이템을 코드로 바꾸지 않는다. 막힘이 나면 봇 입력 정책의 문제로 보고하고 게임 코드는 건드리지 않는다.
5. 필요한 보강(R2): 고정 틱 행, 지형 격자, 적 상태·공격 예고, UI 포커스 경로 → `PlayBotPlan.md`.

## 6. 검증 기록 (11-15)
- EditMode 전체 통과(1296), 컴파일 오류 0. 플레이 모드 약 140초(게임 시간): st 9.8Hz, log 미러·warn 정상, 키 주입(D·RightArrow)으로 `in` 확인. 콘솔 error 0.
- 미검증: 전투 중 `dmg`/`hurt`/`kill` 이벤트와 `stuck` 실전 발생(시험은 순수 로직 단위), 빌드 경로(`persistentDataPath`).
