# 플레이 기록기·봇 설계 (PlayBotDesign)

단계 계획·예산·합격 기준은 `Game/PlayBotPlan.md`. 이 문서는 **구현 형식**(R1 + R2 학습 확장)이다. 사람용 안내는 `Docs/Playtest/PlayRecGuide.md`.

## 1. 목적
- 사람이 직접 플레이한 기록(jsonl + mp4)을 남기고, 요약 통계와 학습 행으로 **사람처럼 노는 자동 플레이 봇**의 설계·학습 입력을 얻는다.
- 봇은 **진짜 입력만** 쓴다. 치트·순간이동·직접 위치 변경·내부 함수 직접 호출 금지. 기록기는 게임 상태를 바꾸지 않는다(읽기·구독, 게임 코드에는 한 줄 훅만).
- R2 까지 기록기와 요약기다. 봇은 아직 없다.

## 2. 구성 (`Assets/Script/DebugTools/`)

| 파일 | 역할 |
|---|---|
| `PlayRecorder.cs` | 생명주기·스케줄(20Hz tk, 10Hz st, 1Hz cx/pf/vf)·정적 창구(`Event`·`Dealt`·`HitBy`·`HitMiss`·`HitAttempt`·`Guarded`·`Log`). `Enabled`·`VideoEnabled`·`Source` 스위치 |
| `PlayRecWriter.cs` | 세션 파일 열기·닫기·1초 Flush·한 줄 조립(`Gate` 잠금 + `Begin/Commit/Discard`)·오래된 세션 정리(최근 30판) |
| `PlayRecInput.cs` | InputAction 구독 → `in` 행, 누르고 있는 액션 비트(`HeldMask`)·이동 축, `Pressed` 이벤트 |
| `PlayRecHooks.cs` | 게임 이벤트 구독(`ev`), 결정 → `dec`, 피격 시도 → `hitat` |
| `PlayRecTick.cs` | `tk`·`gd`·`cx`·`tkh` 직렬화 |
| `PlayRecSignals.cs` | 회피 창(`PlayRecThreat`) → `av`, 사람 신호(`PlayRecHuman`) → `hs` |
| `PlayRecVideo.cs` | 화면 → 640x360 → AsyncGPUReadback → 파이프 스레드 → ffmpeg(libx264) mp4 |
| `PlayRecBookmark.cs` | F8 / 패드 SELECT+R3 → `bm` + 원본 해상도 PNG |
| `PlayRecPerf.cs` · `PlayRecMeta.cs` · `PlayRecWarn.cs` | `pf`·`hitch` / `meta` / `warn`(경고·예외·막힘 묶음) |
| `PlayRecGrid.cs` · `PlayRecHuman.cs` · `PlayRecThreat.cs` | 순수 로직(격자 좌표·해시, 사람 신호 판정, 회피 창 판정). EditMode 시험 대상 |
| `PlayObservation.cs` + `PlayObsEnemies/Terrain/Context/Ui.cs` | 게임 상태 **관측**(읽기 전용). 봇이 그대로 재사용한다 |
| `PlayRecSnapshot.cs` · `PlayRecJson.cs` | `st` 직렬화 · JSON 조립 + `PlayRecStuck`(막힘) |
| `Assets/Editor/Tests/PlayRecorderTests.cs` · `PlayRecR2Tests.cs` | EditMode 시험 |
| `Tools/playrec_summary.py` | jsonl → markdown 요약(150줄 이내) + `--frames` |

- 자동 시작: `RuntimeInitializeOnLoadMethod`(AfterSceneLoad), `DontDestroyOnLoad`. 플레이 모드·빌드 모두 기본 켜짐. 끄려면 부트스트랩 전에 `PlayRecorder.Enabled = false`(실행 중에 바꾸면 기록만 멈춘다). 영상만 끄려면 `VideoEnabled = false`.
- 출력: 에디터 `<프로젝트>/Logs/playrec/<yyyyMMdd_HHmmss>.jsonl`·`.mp4`·`<세션>/mark_NN.png`(gitignore), 빌드 `Application.persistentDataPath/playrec/`.
- 종료: 플레이 정지·앱 종료·도메인 리로드(`AssemblyReloadEvents.beforeAssemblyReload`) 어디서든 `Shutdown` 한 번 — 마지막 `vf`(end) → ffmpeg 표준입력 닫고 대기(최대 약 8초, 넘으면 Kill) → 파일 닫기.
- GameLog 연결: `GameLog.Info` → `PlayRecorder.Log`(1순위 이벤트 경로). `Warn`/`Anomaly` 는 `logMessageReceivedThreaded`.

## 3. 로그 형식
한 줄 = JSON 한 객체. 공통: `t`(Time.time, 소수 3자리) · `k`(종류) · `src`(`human`|`bot`). 실제 시간이 필요한 행은 `u`(unscaledTime)를 따로 쓴다.

### 3.1 R1 행
| k | 필드 | 설명 |
|---|---|---|
| `in` | `a` 맵/액션, `ph` S·P·C, `dev` kb·pad·mouse·other, `v` 또는 `x`,`y` | 모든 InputAction. 같은 이름 중복 인스턴스는 같은 프레임·같은 값 1회. 마우스 이동 제외. 축 Performed 는 변화 0.25 이상·0 교차만 |
| `st` | `map` `x` `y` `vx` `vy` `gr` `rope` `lad` `face` `fsm` `fl`(I무적 S슈아 M이동잠금 L로프잠금 U입력잠금 D대시잠금) `hp` `hpm` `mp` `mpm` `lv` `exp` `gold` `ui`[창] `en`[{id,dx,dy,hp%}x3] `pt` `npc` `drop` | 10Hz(tk 2번에 1번). 플레이어 없으면 `pl:0`+`ui` |
| `ev` | `e` `a` `v` `b` | 아래 이벤트 목록 |
| `log` | `cat` `msg`(300자) | GameLog.Info 미러 |
| `warn` | `ty` Warning·Error·Exception·stuck·snapshot, `msg` `stk` `n` | 같은 메시지는 처음 5번 + 100번째마다. `stuck` 은 `map x y fsm mv` |

`ev` 이름: `dmg`(a 대상, v 피해, b basic/skill[+crit], `kb` 넉백 힘(슈퍼아머 0), `cb` 콤보 단계, `hc` 히트 카운터) · `hurt`(v) · `kill` · `pick` · `lvup` · `death` · `revive` · `map` · `qacc`/`qdone` · `npc` · `buy`(a,v) · `sell`(a,v) · `equip`(a, b 슬롯) · `skill`(a, v 슬롯) · `use` · `drop`(a,v 버림) · `quick`(a 아이템, v 슬롯) · `invest`(a 노드, v 레벨, b 트리).

### 3.2 `tk` — 20Hz 학습 행 (실제 시간 기준 0.05초마다)
| 필드 | 뜻 | 단위 | 없음 값 |
|---|---|---|---|
| `n` | 틱 번호(세션 내 단조 증가) | 정수 | — |
| `u` | 실제 시간 unscaledTime | 초 | — |
| `a` | 누르고 있는 액션 비트(비트 i = `tkh.bits[i]`) | 비트마스크 | 0 |
| `mx` `my` | `Player/Move` x, `UI/verticalCheck` y | -1~1 | 0 |
| `ui` | 열린 창 있음 | 0/1 | 0 |
| `pl` | 플레이어 없음 표시(이때 아래 필드 전부 생략) | 0 | 필드 자체가 없음 |
| `p` | [x, y, vx, vy] | 월드 유닛, 유닛/초 | — |
| `s` | 1 접지 · 2 로프 위 · 4 로프 근처 · 8 왼쪽 봄 · 16 피해 가능(무적 아님) | 비트 | 0 |
| `fsm` `fl` | FSM 상태 이름 · 잠금 플래그(st 와 같음) | 문자열 | "" |
| `hp` | [HP, 최대HP, 스태미나, 최대] | 정수 | 필드 생략 |
| `cd` | 스킬 슬롯 1~4 남은 재사용 대기 | 초(0.1) | 0 |
| `cb` `hc` | 무기 콤보 단계 · 히트 카운터 | 정수 | 0 |
| `gh` | 지형 격자 해시(같은 해시의 `gd` 행이 앞에 한 번 있다) | 16진 8자리 | 생략(미계산) |
| `ed` | [EdgeL, EdgeR, Fall, DropF] 발판 끝까지 왼/오 · 발밑 낙하 높이 · 바라보는 쪽 끝 너머 낙하 | 유닛(0.25 간격, 최대 8 / 30) | -1(공중·없음). 끝이 8 넘게 이어지면 8 |
| `en` | 가까운 순 최대 6: [종 id, dx, dy, vx, vy, 거리, HP%, 상태, 판정, 신호] | 유닛·유닛/초·% | 빈 배열 |
| `pj` | 가까운 몹 투사체 최대 4: [dx, dy, vx, vy] | 유닛·유닛/초 | 생략 |
| `dr` `pt` `npc` | 가장 가까운 드롭·포털·NPC [dx, dy] | 유닛 | 생략 |

- `en` 상태: 0 대기 · 1 추적(ChaseLatched) · 2 예고 · 3 공격 · 4 피격(경직·스턴) · 5 사망. 우선순위 사망 > 피격 > 공격 > 예고 > 추적 > 대기.
  - 공격 = 그 몹이 소유한 `EnemyDamageObject` 판정이 살아 있음(근접 판정) 또는 같은 isAttack 구간에서 판정이 나온 뒤의 회복.
  - 예고 = Animator `isAttack` 켜짐(BT 공격 준비 0.5초~) 판정 전, `TempMobDash` 웅크림, `TempMobShooter` 조준.
  - 판정 = 근접 판정이 지금 살아 있음(0/1). 신호 = 그 몹이 가진 예고 신호 비트(1 isAttack 애니 · 2 돌진 · 4 사격). 0 이면 예고를 관측할 수 없는 종이다(5절 미지원).
  - 속도는 위치 차분(틱 간). 투사체 = 부모 없이 날아가는 `EnemyDamageObject`.
- 시간이 끊기면(에디터 명령·로딩 등 0.25초 넘는 정지) 밀린 틱을 몰아 쓰지 않고 다음 틱부터 다시 맞춘다 → `u` 간격으로 끊김을 알 수 있다.

### 3.3 그 밖의 R2 행
| k | 주기 | 필드 |
|---|---|---|
| `tkh` | 처음 + 새 액션 이름을 볼 때 | `bits`[액션 이름] — 입력 에셋 Player → UI → Portal 맵 순 |
| `gd` | 새 격자 해시마다 한 번 | `gh` `map` `ox` `oy`(원점 = 왼쪽 아래 칸의 정수 좌표) `w`=24 `h`=14 `c`(w*h 글자, 행 0 = 맨 위). 칸: 0 빈칸 · 1 바닥 · 2 발판(단방향) · 3 벽 · 4 로프/사다리 · 5 위험(독포자 구름). 원점 = 발 기준 왼쪽 12칸·아래 6칸 |
| `cx` | 1초마다 바뀔 때 | `lv` `expp`(%) `gold` `bag`(빈 칸) `pot`(퀵슬롯 아이템 수 합) `night` `q`[[id, 진행, 목표]≤6] `eq`[슬롯 10, 빈 슬롯 null] `buff`[food:종류, 버프 이름] |
| `fo` | 바뀔 때 | `p` 포커스 경로: `inv/<상태\|browse>/<페이지>/<커서 아이템\|->` · `shop/<진열 아이템>` · `ui/<부모>/<선택 오브젝트>` · ""(창 없음) |
| `dec` | 결정 다음 프레임 | `t0` `d`(buy·sell·equip·use·drop·quick·invest) `a` `v` `b` `g0`/`g1` 전후 골드 `s0`/`s1` 전후 스탯[기본피해, 방어, 치명률, 치명피해, 피해증가, 이속, 공속, 최대HP] `ui` 맨 위 창 `foc` 포커스 |
| `hitat` | 몹 공격이 플레이어에 닿을 때 | `a` 종 `id` `v` 피해 `r` hit·guard·parry·iframe(피격 후 무적)·immune(대시·스킬 무적)·miss(레벨 차 빗나감) |
| `av` | 예고 창이 닫힐 때 | `a` 종 `id` `t0` `dur` `dx0`(시작 dx) `res`(hit·guard·parry·immune·iframe·miss·none) `r` 첫 반응(jump·dash·back·fwd·guard·atk·none) `rt` 반응까지 초(-1 없음) `in` 창 안 입력 비트(1 점프 2 대시 4 후퇴 8 전진 16 방어 32 공격) `atk` 공격 상태를 봤나 `tele` 예고에서 열렸나 |
| `hs` | 판정 때 | `s` mash(같은 버튼 0.15초 안 3연타)·osc(2초 안 좌우 3번 뒤집기, 이동 폭 1.5 이하)·qclose(창 열고 1초 안 닫기)·idle(3초 이상 무입력, 끝날 때 dur)·pause `a` 액션/창 `u0` `dur` `n` |
| `pf` | 1초 | `u` `n` 프레임 수 `avg`/`max` ms `hitch`(50ms 초과 수) `gc`(GC 횟수) `heap` MB `alloc` KB `spike`(1MB 이상 할당) |
| `hitch` | 50ms 넘는 프레임마다 | `u` `ms` |
| `vf` | 1초 + 북마크 + 종료 | `f` 마지막 프레임 번호 `fu` 그 프레임 캡처 시각(u) `u` `drop` `on` `end` `err` — 영상 초 = f/15, `fu` 로 u ↔ 영상 시각 보간 |
| `bm` | 북마크 | `n` `u` `png`(세션/mark_NN.png) `map` `x` `y` `fsm` `foc`. 같은 순간 st·cx·gd·vf 를 강제로 함께 쓴다 |
| `meta` | 한 번(플레이어·맵 확인 뒤, 최대 10초) | `ver`=R2 `session` `git` `unity` `platform` `sw` `sh` `dev`(장치) `map` `lv` `gold` `hasPl` + cx 필드 `video`(640x360@15 또는 off) `videoErr` `tkHz` |

- 회피 창(`av`): 몹이 예고 또는 공격 상태로 바뀌고 가로 8·세로 4 안이면 연다(같은 몹은 창 하나, `tele` 1 = 예고에서 열림). 예고·공격 상태가 0.6초 넘게 안 보이거나 3초가 지나면 닫는다. 창이 열리기 0.3초 안쪽 앞의 같은 몹 피격 시도도 붙인다(훅은 매 프레임, 관측은 20Hz). 결과는 그 몹의 실제 `hitat`(가장 나쁜 쪽: hit > guard > parry > immune > iframe > miss)이고 없으면 none.

## 4. 영상·북마크
- 영상: 게임 최종 화면(UI 포함)을 프레임 끝에 `ScreenCapture.CaptureScreenshotIntoRenderTexture` → 640x360 Blit → `AsyncGPUReadback`. 메인 스레드는 요청만 하고, 파이프 쓰기는 전용 스레드 + 큐(8). 큐가 차거나 읽기가 4개 넘게 밀리면 그 프레임을 버리고 `drop` 을 센다.
- ffmpeg: `-f rawvideo -pix_fmt rgba -s 640x360 -r 15 -i - -c:v libx264 -preset veryfast -crf 30 -pix_fmt yuv420p -g 30 -movflags +frag_keyframe+empty_moov+default_base_moof`. 조각 mp4 라 비정상 종료에도 앞부분은 재생된다. D3D 는 readback 이 위 행부터라 뒤집지 않는다(OpenGL 이면 vflip).
- ffmpeg 찾기: PATH(프로세스·사용자·시스템) → `%LOCALAPPDATA%\Microsoft\WinGet\Packages\*FFmpeg*\**\bin\ffmpeg.exe`. 없거나 실패하면 영상만 끄고(`meta.videoErr`, `vf.err`) 기록은 계속.
- 크기: 11-15 실측 3세션(마을·필드·전투 섞음, 6~14분): mp4 약 8~10KB/초 → **30분 약 15~19MB**. jsonl 약 9~11KB/초 → 30분 약 17~20MB(tk 가 대부분). 북마크 PNG(3840x2160 원본)는 장당 약 0.3~0.5MB.
- 북마크: **F8**(키보드) 또는 **패드 SELECT + R3 동시**(`PadFieldInputRules.IsSelectR3Chord`). 0.5초 안 연속은 무시. 원본 해상도 PNG 를 `Logs/playrec/<세션>/mark_NN.png` 에 비동기로 저장하고 "북마크 저장 #n" 알림(`PickupLog.Notice`)을 띄운다(PNG 는 알림이 뜬 그 프레임이라 알림 글자가 찍힌다).
  - 키 충돌: F8 은 기존 바인딩 없음. 패드 SELECT 짧게=가방·길게=퀘스트 창, R3=유물 — 동시 누름은 둘 다 취소하고 북마크만 한다(필드). 이 조합은 기록기가 켜져 있을 때만 쓴다(`PadFieldInput.SelectR3ChordEnabled`, 꺼져 있으면 예전 동작 그대로). 가방이 열린 상태에서는 가방 코드가 SELECT(프리셋 넘김)·R3(정렬)를 원시 버튼으로 직접 읽으므로 같이 먹을 수 있다 → 필드에서 누르도록 안내(`Docs/Playtest/InputAudit.md`).

## 5. 미지원·한계
- **예고 신호 없는 몹**(`en` 신호 0 → 예고 상태가 나오지 않고 `av` 창이 열리지 않음): 프리팹 정적 점검(11-15, `Assets/**/*.prefab` 의 EnemyHealth + Animator `isAttack` + MonsterSheet `trait` dash/shoot/rune) 결과.
  - 신호 없음: `Rabbit_BD`(attack=contact, isAttack 없음), 옛 프리팹 `Wolf`·`Wolf_Fix`·`GreenHurb`·`Rock`·`tree`·`EnemySample*`(BT·애니 없음).
  - 프리팹 이름이 시트 id 와 달라 정적으로 못 본 행(변종·엘리트, 쓰는 프리팹의 신호를 따른다): `ForestSpore_BD`·`DeepSpore_BD`·`SwampSpore_BD`·`DeepGoblin_BD`·`MossShellBeetle_BD`·`SporeBackToad_BD`·`AlphaWolf_BD`·`OldBear_BD`·`OldCroc_BD`·`BanditLieutenant_BD`·`ElderGolem_BD`·`VineFairy_BD`·`ForestOgre_BD`·`BanditSentry_BD`·`RuinWarden_BD` → 미확인(첫 실전 기록의 `en` 신호 열로 확인).
  - 나머지 `*_BD` 프리팹 44개는 `isAttack` 이 있다. 단 **BT 가 `isAttack` 과 공격 판정을 같은 프레임에 켜는 몹**(실측: `RatChief_BD`·`RatThief_BD` — SetBool 직후 EnemyAttackNode)은 예고 구간이 0 이라 `en` 상태가 추적 → 공격으로 바로 간다. 이런 몹은 `av` 창이 공격 시작에서 열리고 `tele:0` 으로 남는다(반응 시간은 "공격 시작 → 첫 입력").
- 접촉 피해(`ContactDamage`)는 예고·공격 상태가 없다. 피격 결과는 `hitat` 으로 남지만 `av` 창은 열리지 않는다.
- 도트 피해 처치는 `kill` 에서 빠진다(`EnemyHealth.Killed` 는 직격만). `dmg` 에 스킬명 없음(`skill` 이벤트와 시각으로 맞춘다).
- 자체 커서만 쓰는 창(EventSystem 선택을 안 쓰는 창)은 `fo` 경로가 없다(창 이름만 `ui`).
- R1 의 `EquipmentModel` 장착 훅은 그 클래스가 쓰이지 않아 발생하지 않는다 → 장착은 `InventoryManager.UseEquipment` 훅(인벤에서 꺼낸 장착만, 로드·기본 무기 제외).

## 6. 요약 스크립트
```
python Tools/playrec_summary.py Logs/playrec/<세션>.jsonl [출력.md] [--frames]
```
기본 출력 `<세션>_summary.md`(150줄 이내): 메타·tk Hz·맵 체류, 영상(크기·프레임·버림), 북마크(영상 시각), 입력 상위, 전투 리듬(평타 간격·반응 시간·점프/대시/방향 전환), 전투 결과(처치·피해·치명·넉백·콤보·사망), 회피(피격 시도 결과, 예고 → 반응 종류·시간·종별 맞은 비율, 종별 신호·상태 분포), UI(창 열기·체류, 상점에서 본 품목, 결정 목록과 골드·스탯 변화), 사람 신호·막힘, 성능(프레임·끊김·GC), 경고. `--frames` 는 북마크·막힘·사망마다 `ffmpeg -ss` 로 한 장씩 `<세션>_frames/` 에 뽑는다(t → u 는 tk, u → 영상 초는 vf 보간).

## 7. 봇 계획 메모
1. 가상 `Keyboard`/`Gamepad` 를 `InputSystem.AddDevice` 로 만들고 `QueueStateEvent` 로 누르고 뗀다. 비포커스 입력은 `Framework/EditorAutomation.md` 의 `input_bg_on`.
2. 관측은 `PlayObservation` 그대로, 행동 = `tk.a` 비트 + `mx`/`my`. 학습 쌍 = (tk 관측 + 최근 gd, 다음 틱 a).
3. 봇도 `PlayRecorder.Source = "bot"` 로 같은 jsonl 을 남기고 같은 요약기로 사람과 비교한다.
4. 금지 재확인: 위치·HP·아이템을 코드로 바꾸지 않는다. 막힘은 봇 정책 문제로 보고하고 게임 코드는 건드리지 않는다.

## 8. 검증 기록
- R1(11-15): EditMode 1296 통과, st 9.8Hz, 키 주입 `in` 확인.
- R2(11-15): EditMode 전체 1317 통과(R2 순수 로직: 격자·사람 신호·회피 창·tk 직렬화·SELECT+R3 조합), 컴파일 오류 0, 콘솔 error 0. 플레이 모드 키 주입 4세션(합 약 30분 실제 시간).
  - tk: 끊김 제외 20.0Hz(전체 17~19Hz — `u.sh cs` 프로브마다 약 1.1초 에디터 정지가 끊김의 대부분). gd: 바닥 1·발판 2·로프 4 확인(Lv2_MagicCliffs). 로프 오르기 `s`&2·fsm moveUp 확인.
  - 적 상태: RatChief/RatThief 대기·추적·공격·피격 관측, 판정(hitbox) 1 은 공격 상태와 일치. 예고(2)는 이 두 종에서 0(BT 구조, 5절). `av` 6건(tele 0, 결과 hit·iframe·immune, 반응 jump·back·atk). `hitat` hit·iframe·immune.
  - 전투 이벤트: `hurt`·`death`·`revive`·`dmg`(kb·cb 포함) 확인. `dec` buy·sell·equip(스탯 전후 차이 기록) 확인. `fo` 가방 포커스 경로 확인. `bm` F8·패드 SELECT+R3(동시 누름, SELECT 누른 채 R3 둘 다) PNG 저장(방향 정상). SELECT 를 0.8초 넘게 누른 뒤 R3 를 누르면 가방이 함께 열린 사례 1회(키 주입 지연) — 가이드는 "동시에" 누르기로 안내. `hs` idle·mash 확인.
  - 영상: ffprobe 640x360·15fps, 길이 = 프레임/15 와 일치(예 421초 6314프레임, 버림 0). 플레이 정지 뒤 ffmpeg 프로세스 0. 영상 프레임 방향 정상. `summary.py --frames` 로 북마크·사망 프레임 추출 확인.
  - 성능(pf, 같은 장면 8초씩 번갈아): 기록기+영상 켬 평균 4.94ms vs 끔 4.73ms(+0.2ms, 에디터 3840x2160).
  - 미검증: `kill`(키 주입 공격이 처치까지 못 감), `stuck` 실전 발생(순수 로직 시험만), qclose·osc 사람 신호(주입 간격이 판정 밖), 상점 UI 를 통한 실제 구매(리플렉션으로 BuyItem/SellItem 호출 — 훅·dec 는 확인), 빌드 경로, 도메인 리로드 중 영상 종료(Stop(true)) 경로.
