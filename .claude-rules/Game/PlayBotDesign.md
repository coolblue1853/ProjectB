# 플레이 기록기·봇 설계 (PlayBotDesign)

단계 계획·예산·합격 기준은 `Game/PlayBotPlan.md`. 이 문서는 **구현 형식**(R1 + R2 학습 확장)이다. 사람용 안내는 `Docs/Playtest/PlayRecGuide.md`.

## 1. 목적
- 사람이 직접 플레이한 기록(jsonl + mp4)을 남기고, 요약 통계와 학습 행으로 **사람처럼 노는 자동 플레이 봇**의 설계·학습 입력을 얻는다.
- 봇은 **진짜 입력만** 쓴다. 치트·순간이동·직접 위치 변경·내부 함수 직접 호출 금지. 기록기는 게임 상태를 바꾸지 않는다(읽기·구독, 게임 코드에는 한 줄 훅만).
- R2 까지 기록기와 요약기. R3-2(버릇 모델)·B0(입력 재생)·B1(봇 뼈대)은 11-15 — 9·10절.

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
| `Tools/playrec_fit.py` · `Tools/tests/test_playrec_fit.py` | 사람 기록 → 버릇 모델 `Assets/Resources/Bot/human_profile.json`(9절) |
| `Bot/BotInput.cs` · `BotKeyMap.cs` | 가상 장치 입력 계층(액션 이름 API, 리바인딩 추종)·경로 ↔ 키 변환(10절) |
| `Bot/BotProfile.cs` · `BotRules.cs` | 프로필 읽기·분위수 샘플링 · 평타 리듬(BotAttackRhythm)·막힘(BotStuck) 순수 로직 |
| `Bot/PlayBot.cs` · `BotTypes.cs` · `HuntBrain.cs` | B1 봇: 인지→판단(IBotBrain 10Hz)→입력, 이동(IBotNavigator) 확장 지점 |
| `Bot/BotReplay.cs` · `Tools/playrec_replay_cmp.py` | B0 입력 재생·비교 |
| `Tools/bot_run.sh` · `Tools/probes/bot_*.cs` · `SROptions.Bot.cs` | 봇 실행 스크립트·프로브·SRDebugger "Bot" 묶음 |
| `Assets/Editor/Tests/PlayBotTests.cs` | 봇 EditMode 시험(키 매핑·샘플링·리듬·막힘·목표·재생 변환·기록 폴더) |

- 자동 시작: `RuntimeInitializeOnLoadMethod`(AfterSceneLoad), `DontDestroyOnLoad`. 플레이 모드·빌드 모두 기본 켜짐. 끄려면 부트스트랩 전에 `PlayRecorder.Enabled = false`(실행 중에 바꾸면 기록만 멈춘다). 영상만 끄려면 `VideoEnabled = false`.
- 출력: 에디터 `<프로젝트>/Logs/playrec/<yyyyMMdd_HHmmss>.jsonl`(출처 human) · 봇·재생(`Source` bot·replay)은 `Logs/playrec_bot/`(`PlayRecWriter.DirNameFor` — 최근 30판 정리가 폴더별이라 봇이 사람 기록을 밀어내지 않는다). 출처 전환은 `PlayRecorder.Restart(source)`(현재 세션 닫고 새 세션), 플레이 시작 전 지정은 에디터 `SessionState` `PB.PlayRec.NextSource`(`bot_pending_source.cs`)·`.mp4`·`<세션>/mark_NN.png`(gitignore), 빌드 `Application.persistentDataPath/playrec/`.
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
| `warn` | `ty` Warning·Error·Exception·stuck·snapshot, `msg` `stk` `n` | 같은 메시지는 처음 5번 + 100번째마다. `stuck` 은 `map x y fsm mv`. 이동 입력을 눌러도 이동 잠금(`fl` 에 M)이거나 공격 모션 중이면 막힘으로 보지 않는다(R2.1, `PlayRecStuck.IsExcused`) |

`ev` 이름: `dmg`(a 대상, `id` 몹 인스턴스, v 피해, b basic/skill[+crit], `kb` 넉백 힘(슈퍼아머 0), `cb` 콤보 단계, `hc` 히트 카운터) · `hurt`(v) · `kill`(R2.1: a 종 id(처치 메시지 id), `mid` MonsterSheet id, `id` 인스턴스(=`dmg`·`hitat` 의 id), `exp` 받은 EXP(밤·정예 배율 반영), `gold` 떨어진 골드, `el` 정예 1/0, `map`, `night` 처치 시각 밤 1/0, `ns` 밤 스폰 몹 1/0 — 옛 기록은 a 만 있다) · `pick` · `lvup` · `death` · `revive` · `map` · `qacc`/`qdone` · `npc` · `buy`(a,v) · `sell`(a,v) · `equip`(a, b 슬롯) · `skill`(a, v 슬롯) · `use` · `drop`(a,v 버림) · `quick`(a 아이템, v 슬롯) · `invest`(a 노드, v 레벨, b 트리).

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
- **인코딩(R2.1):** jsonl 은 UTF-8(BOM 없음)이고 한글이 그대로 들어간다. 11-15 에 보고된 U+FFFD 깨짐은 기록 결함이 아니라 윈도우 콘솔(cp949)에서 읽은 표시 깨짐이었다(두 세션 파일 strict UTF-8 디코드 통과, EF BF BD 0개). 파이썬 도구는 출력을 UTF-8 로 고정한다. 터미널에서 직접 볼 때는 `python -X utf8`.
- 도트 피해 처치는 `kill` 에서 빠진다(`EnemyHealth.Killed` 는 직격만). `dmg` 에 스킬명 없음(`skill` 이벤트와 시각으로 맞춘다).
- 자체 커서만 쓰는 창(EventSystem 선택을 안 쓰는 창)은 `fo` 경로가 없다(창 이름만 `ui`).
- R1 의 `EquipmentModel` 장착 훅은 그 클래스가 쓰이지 않아 발생하지 않는다 → 장착은 `InventoryManager.UseEquipment` 훅(인벤에서 꺼낸 장착만, 로드·기본 무기 제외).

## 6. 요약 스크립트
```
python Tools/playrec_summary.py Logs/playrec/<세션>.jsonl [출력.md] [--frames]
```
기본 출력 `<세션>_summary.md`(150줄 이내): 메타·tk Hz·맵 체류, 영상(크기·프레임·버림), 북마크(영상 시각), 입력 상위, 전투 리듬(평타 간격·반응 시간·점프/대시/방향 전환), 전투 결과(처치·피해·치명·넉백·콤보·사망), 회피(피격 시도 결과, 예고 → 반응 종류·시간·종별 맞은 비율, 종별 신호·상태 분포), UI(창 열기·체류, 상점에서 본 품목, 결정 목록과 골드·스탯 변화), 사람 신호·막힘, 성능(프레임·끊김·GC), 경고. `--frames` 는 북마크·막힘·사망마다 `ffmpeg -ss` 로 한 장씩 `<세션>_frames/` 에 뽑는다(t → u 는 tk, u → 영상 초는 vf 보간).

## 7. 봇 계획 메모
1. 가상 `Keyboard`/`Gamepad` 를 `InputSystem.AddDevice` 로 만든다 — 구현·키보드 주입 결론은 10절. (`input_bg_on` 프로브는 설정 에셋을 메모리에서 바꾼다 — 봇은 쓰지 않는다)
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
- R3-2·B0·B1(11-15): EditMode 전체 1365 통과, 컴파일 오류 0, 콘솔 error 0. `test_playrec_fit` 16개 통과. 세이브 백업 → 원복 md5 일치(매 실행).
  - B0 재생(`20261115_131248` t 0~150, 마을 시작): 원본 808 · 재생 795 사건, 순서 일치(LCS) 95.3%, 앞 사건과의 간격 오차 중앙 0.008초·p90 0.037초. 남는 차이는 게임 상태 차(창이 열려 Player 맵 꺼짐 → Move 218/194, PortalOn 62/80, Select 85/79).
  - B1 `bot_run.sh 5`(Lv1_NearForest_SideRoad, 세이브 Lv8, 리뷰 반영 뒤 최종 `20261115_185253`): 처치 73(13.7/분), 받은 피해 8회 합 8, 사망 0, 막힘 점프 27·반대 걷기 1, 진척 없음 경고 1(못 닿는 목표 10초 무시 추가 전 17), src=bot 14210줄. transform·rigidbody·치트 대입 grep 0. B0 재생은 리뷰 반영(정상 종료 때 사람 출처 복귀·neg 만 있는 축 건너뜀) 전 코드로 측정.

## 9. 버릇 모델 `human_profile.json` (R3-2)
`python Tools/playrec_fit.py <기록.jsonl>... [-o 경로]` (기본 `Assets/Resources/Bot/human_profile.json`). 원본은 읽기만 한다. 11-15 학습 자료 = `20261115_131248` + `20261115_133116`(39분, 필드 28분, 처치 574).

| 필드 | 형식 |
|---|---|
| `version` `minN`(=5) `sources` `minutes` `fieldMinutes` `kills` | 머리 |
| `dists[]` | `{key, n, unit, p10, p25, p50, p75, p90}` — n < minN 이면 백분위 null(추측 기본값 없음) |
| `ratios[]` | `{key, n(분모), k(분자), value}` — n < minN 이면 value null |
| `rates[]` | `{key, n(사건 수), minutes, value(분당)}` — n < minN 또는 1분 미만이면 null |

| key | 정의 |
|---|---|
| `react.engage` | 적이 \|dx\|≤6 안에 새로 들어온 순간 → 다음 평타·스킬(5초 안). summary 와 같은 정의(접근 시간 포함) |
| `react.idle` | 예고 창(`av`)이 열릴 때 아무 키도 안 누르고 있었으면 → 첫 입력(3초 안). 순수 반응 시간 — 봇 반응 지연에 쓴다 |
| `press.<액션>` | 같은 액션 S → C 누름 시간(Player 맵은 접두 생략, `UI/Select` `UI/Back` `UI/OpenInventory` `Portal/PortalOn`) |
| `attack.interval` | 평타 S 간격(2초 이하) |
| `attack.burstWithin` · `burstLen` · `burstGap` | 연타 묶음(간격 0.35초 이하로 이어짐 — 분포가 0.1~0.3 / 0.4+ 로 갈림): 묶음 안 간격·묶음 길이(타수)·묶음 사이 간격(10초 이하) |
| `move.dirChange` | 좌우 이동 방향이 뒤집힌 시각 사이 간격 |
| `avoid.rt` · `avoid.r.*` · `avoid.res.*` | `av` 첫 반응까지 초(반응 있음만) · 첫 반응 종류 비율 · 결과 비율 |
| `jump.ctx.*` | 점프 직전 상황(우선순위): enemy_near(2.5 안 산 적) · rope · edge(바라보는 쪽 발판 끝 1.0 안) · moving · standing |
| `target.nearest` · `lowestHp` · `dropFirst` | 처치 직후 다음 첫 타격 대상을 처치 순간 후보(산 적 2+)와 절대 위치로 대조: 최근접이었나 · (HP 가 서로 다를 때) HP 최저였나 · (드롭이 있을 때) 다음 타격 전에 주웠나. 옛 기록엔 dmg 인스턴스 id 가 없어 같은 종 최근접으로 맞은 몹을 잡는다(근사) |
| `potion.hpPct` | 포션 `use` 직전 HP% |
| `return.bagFree` · `return.potions` | 다른 맵 → 마을 이동 직전 cx 의 가방 빈 칸·포션 수 |
| `hunt.segment` | 마을 밖 전투 이벤트(dmg·kill·hurt)가 15초 안으로 이어진 구간 길이 |
| `rest.idle` | `hs` idle(3초 이상 무입력) 길이 |
| `menu.focusInterval` | 같은 창 안 포커스 이동(`fo`) 간격(5초 이하) |
| rates `jump.perMinute` · `jump.fieldPerMinute` · `kill.perMinute` | 분당 빈도 |

11-15 값(중앙): 평타 누름 0.113초, 묶음 안 간격 0.202초, 묶음 4타, 묶음 사이 0.90초, react.idle 0.081초(p75 0.32), react.engage 0.76초, 방향 전환 1.21초, 포션 HP 66%(n=11), 귀환 직전 가방 빈 칸 11·포션 15(n=5), 사냥 구간 58초(n=18), 목표 최근접 49%·HP 최저 12%·드롭 먼저 27%, 필드 점프 7.3/분. null(n<5): press.Artifact·ArtifactNext·Guard·QuestWindow·Quick3·WorldMap.

## 10. 봇(B0·B1)
### 10.1 입력 경로와 키보드 주입 결론
- **결론: 키보드 주입은 된다.** 가상 `Keyboard`("BotKeyboard")로 이동·평타·점프·창 키가 모두 액션을 일으킨다(11-15 플레이 모드 확인, 에디터 최소화·비포커스).
- 이전 "키보드 주입 안 됨"의 원인(둘 다):
  1. **검증 방법 착시:** 프로브(`u.sh cs`)는 에디터 업데이트에서 돈다. 거기서 읽는 `kb.xKey.isPressed` 는 에디터 상태 버퍼라 플레이어에서 눌려 있어도 false 가 나온다. 실제로는 캐릭터가 움직이고 있었다. 반영 확인은 플레이어 위치·fsm·기록기 `in` 행으로 한다.
  2. **설정에 따른 이벤트 경로:** 입력 시스템 기본값(`editorInputBehaviorInPlayMode = PointersAndKeyboardsRespectGameViewFocus`)이면 게임 뷰 비포커스 때 키보드·포인터 *이벤트*(`QueueStateEvent`)는 플레이어 업데이트에서 빠져 에디터 업데이트로 간다(`InputManager.OnUpdate`). 가상 키보드도 `Keyboard` 라 같은 규칙에 걸린다. 패드는 예외라 "패드로는 된다"가 나왔다. 기본값으로 바꿔 확인: QueueStateEvent 키보드 = 안 움직임, 아래 Direct = 움직임.
- 이 프로젝트 설정 에셋 `Assets/InputSystem/InputSystem.inputsettings.asset` 은 이미 `m_BackgroundBehavior: 2`(IgnoreFocus)·`m_EditorInputBehaviorInPlayMode: 2`(AllDeviceInputAlwaysGoesToGameView)로 커밋돼 있다(6088747d3) → QueueStateEvent 키보드도 된다. 설정 변경은 필요 없다. 다른 설정의 프로젝트·빌드에서 이벤트 경로를 쓰려면 이 두 값을 위처럼 둔다(방법만 기록, 이번에 바꾸지 않음).
- **BotInput 기본 = Direct:** `InputState.Change(봇키보드, 상태)` 를 `InputSystem.onAfterUpdate`(Dynamic 업데이트 직후)에 쓴다. 이벤트 경로·포커스 규칙을 타지 않고, 액션은 상태 변화 감시로 똑같이 발동하며, 그 프레임 `Update` 들에서 `WasPressedThisFrame` 이 참이다(프레임 중간에 쓰면 먼저 돈 스크립트가 놓친다). `KeyboardDirect = false` 면 QueueStateEvent. 패드 경로(`Route.Pad`)는 QueueStateEvent.
- 패드 바인딩: 전투 액션(Move·Jump·Dash·Run·Attack·Skill 4)은 모두 패드 칸이 있다(시험 `EveryPlayerCombatAction_HasPadBinding`). 키보드 전용: Bestiary·WorldMap·Minimap·Guard·SkillWindow·QuestWindow(패드는 고정 제스처), Quick4·ArtifactNext 패드 빈 칸.
### 10.2 BotInput API
`Press(action, duration=-1)`(음수면 `press.<action>` 분포 샘플) · `Hold` · `Release` · `Stick(x, y)`(x = Move, y = `UI/verticalCheck`; 키보드는 ±0.5 넘으면 화살표) · `SetPath(path, down)`(재생용 저수준) · `ReleaseAll` · `Tick(now)`(소유자 Update). 액션 이름은 `Attack` 처럼 쓰면 Player 맵, `UI/Back` 처럼 맵을 붙여도 된다. 키 경로는 `InputBindings.Master`(저장된 리바인딩 반영)의 effectivePath 를 읽고 `InputBindings.Version` 이 바뀌면 다시 읽는다. Press/Hold/Release 시작은 0~25ms 흔들림. 같은 키를 여러 액션이 쓰면(예 Z = Dash·UI/Select) 하나라도 눌려 있으면 누른 상태.
### 10.3 B0 `BotReplay`
`BotReplay.Play(jsonl, from, sec)`(프로브 `bot_replay.cs`): 구간의 `in` 행을 키보드 경로 단계로 바꿔(버튼 S/C, 1D 축은 값 부호로 양쪽 키, Debug Menu·키보드 칸 없는 액션 제외, 같은 키는 마지막 단계가 이김) **실제 시각** 기준으로 재생한다. in 행엔 t 만 있어 u = 앞 tk 행 u + (t − t_tk)(0~0.05). 일시정지(timeScale 0) 중 t 가 멈추므로 게임 시간으로 재생하면 멈춘다(첫 시도에서 확인). 같은 키를 한 프레임에 눌렀다 떼면 상쇄되므로 다음 프레임으로 미룬다. 비교 `python Tools/playrec_replay_cmp.py <원본> <시작> <길이> <재생>`.
### 10.4 B1 `PlayBot`
- `PlayBot.Run(minutes, goal)`(실제 시간 분, goal = hunt) / `Stop(resumeHuman)` / `Status()`. 프로브 `bot_start`·`bot_status`·`bot_stop`, SRDebugger "Bot" 묶음(Bot Hunt 5 min · Bot Stop · Bot Status). 실행 중 `PlayRecorder.Source = "bot"`(`Restart`), 끝나면 사람 기록으로 되돌린다(`bot_run.sh` 는 곧 플레이를 끄므로 되돌리지 않음).
- 루프: 매 프레임 `BotInput.Tick`·평타 리듬, 10Hz(게임 시간) 판단. 인지 = 봇 전용 `PlayObservation`(기록기와 같은 관측). 목표(Focus)가 바뀌면 `react.idle` 샘플만큼 늦게 반영한다.
- `HuntBrain`(IBotBrain): 사람 비율대로 목표(HP 최저 12% · 최근접 49% · 나머지 두 번째로 가까운 것, 같은 층 |dy|≤2 우선) → 걸어가서 \|dx\|≤1.3·\|dy\|≤1.2 면 평타(`BotAttackRhythm`: 묶음 길이·안 간격·사이 간격 샘플). 처치 직후 드롭이 있으면 `target.dropFirst` 확률로 먼저 줍기(접촉 자동 줍기). 적이 없으면 드롭 → 없으면 발판 끝에서 돌아서며 돌아다님. 창이 1.5초 넘게 열려 있으면 `UI/Back`.
- `DirectNavigator`(IBotNavigator): 목표 쪽 직진, 목표가 1.5~4 위에 있고 가까우면 점프. 막힘(`BotStuck`): 0.8초 제자리 → 점프, 2초 → 1.5초 반대로. 목표 진척(가까워짐·HP 감소·처치)이 3초 없으면 `[PB][Bot] 진척 없음` 경고(3초마다) + 그 목표를 10초 무시.
- 확장 지점: B2 = `IBotNavigator` 교체(발판·로프·포털 그래프, 회피·스킬은 HuntBrain 확장), B3 = `IBotBrain` 교체(귀환·판매·퀘스트, `return.*`·`potion.hpPct` 사용).
### 10.5 실행
`bash Tools/bot_run.sh <분> [goal=hunt] [맵=Lv1_NearForest_SideRoad]`: 세이브 백업(`Logs/playrec_bot/save_backup_*`) → 다음 기록 출처 bot → 플레이 → 시험 준비(`bot_prep_map.cs` 가 `MapInfo.GoToMap` — 봇 실행 전만, 실행 중엔 거절) → `PlayBot.Run(분+1)` → 분마다 상태 → 정지 → 플레이 끔 → 세이브 원복·md5 확인 → `playrec_summary.py`·`playrec_balance.py -o <세션>_balance.md` → 경로 출력.
### 10.6 사람 대 봇 (11-15, 요약기 같은 정의)
| 지표 | 사람 | 봇(5분, Lv8 세이브) | 메모 |
|---|---|---|---|
| 평타 간격 중앙 | 0.21초(131248) · 0.208(두 기록 합) | 0.22초 | 묶음 리듬 샘플 그대로 |
| 반응(적 6칸 진입 → 첫 공격) 중앙 | 0.71초(131248) · 0.76(합) | 2.16초 | 봇은 1.3 안에 들어가야 친다(사람은 대시·스킬로 먼저 친다). B2 에서 사거리·대시 반영 |
| 처치/분 | 9.8(Lv1→5) · 18.8(Lv5→8) · 합 14.7 | 13.7 | 봇 세이브가 Lv8(한두 대에 처치) |
| 점프/분 | 5.5(131248) · 필드 7.3(합) | 5.1 | 막힘 점프 포함 |
