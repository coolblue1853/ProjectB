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
`bash Tools/bot_run.sh <분> [goal=hunt] [맵=Lv1_NearForest_SideRoad] [순환 맵 목록|-]`(11.6): 세이브 백업(`Logs/playrec_bot/save_backup_*`) → 다음 기록 출처 bot → 플레이 → 시험 준비(`bot_prep_map.cs` 가 `MapInfo.GoToMap` — 봇 실행 전만, 실행 중엔 거절) → `PlayBot.Run(분+1)` → 분마다 상태 → 정지 → 플레이 끔 → 세이브 원복·md5 확인 → `playrec_summary.py`·`playrec_balance.py -o <세션>_balance.md` → 경로 출력.
### 10.6 사람 대 봇 (11-15, 요약기 같은 정의)
| 지표 | 사람 | 봇(5분, Lv8 세이브) | 메모 |
|---|---|---|---|
| 평타 간격 중앙 | 0.21초(131248) · 0.208(두 기록 합) | 0.22초 | 묶음 리듬 샘플 그대로 |
| 반응(적 6칸 진입 → 첫 공격) 중앙 | 0.71초(131248) · 0.76(합) | 2.16초 | 봇은 1.3 안에 들어가야 친다(사람은 대시·스킬로 먼저 친다). B2 에서 사거리·대시 반영 |
| 처치/분 | 9.8(Lv1→5) · 18.8(Lv5→8) · 합 14.7 | 13.7 | 봇 세이브가 Lv8(한두 대에 처치) |
| 점프/분 | 5.5(131248) · 필드 7.3(합) | 5.1 | 막힘 점프 포함 |

## 11. 봇 B2 — 이동 그래프·전투 (11-15)
### 11.1 이동 그래프 형식 (`BotNavGraph`, 순수)
- 파일: `Assets/Bot/Nav/<맵>.json`(JsonUtility). **Resources 밖** = 플레이어 빌드에 안 들어간다. 런타임은 에디터 플레이에서만 `System.IO` 로 읽고, 빌드·파일 없음이면 그래프 없이 직진(`DirectNavigator`).
- 노드 = 발판 구간 `Seg`(표면 꺾은선 px/py, `oneWay`, 양끝 `openL/openR` = 낭떠러지면 참·벽/턱이면 거짓). 간선 `Edge`: 종류 0 떨어지기 · 1 점프 · 2 높은 점프(2단, 1층계는 점프 1회라 안 씀) · 3 하향 점프 · 4 로프 · 5 같은 맵 포털. `ax0~ax1` = 출발 x 범위, `lx` = 도착 x, `dir` = 동작 중 좌우, `cost` = 동작 초.
- 다른 맵으로 가는 포털은 `portals`(toMap·도착 좌표·앉은 구간)에 따로 두고 `BotMapRoute.NextHop`(포털 BFS, 목적지가 Town 이 아니면 Town 경유 안 함)이 쓴다.
- 길찾기: 구간 다익스트라(구간마다 가장 싸게 도착한 x 하나만 들고 가는 근사), 비용 = 걷기 거리/8 + 동작 비용.

### 11.2 굽기 (`BotNavBuild` 순수 + `Assets/Editor/Dev/BotNavBaker.cs`)
- 메뉴 `Tools/Bot/Bake Nav Graphs` 또는 `bash Tools/u.sh cs Tools/probes/bot_nav_bake.cs`(플레이 아닐 때). `Map` 아래 맵마다 한 파일. **맵 지형을 바꾸면 다시 굽는다** — 맵 들어갈 때 `GraphNavigator.MatchesTerrain`(구간 몇 곳 레이캐스트 70%)이 어긋나면 경고 `[PB][Bot] 이동 그래프가 지형과 다름` 후 직진 이동.
- 고체: Ground/Wall 타일맵은 **타일 셀 가로 줄**로(꺼진 맵의 CompositeCollider 경로는 낡아 쓰면 안 된다), 타일맵 없는 콜라이더는 경로/상자. 단방향 = PlatformEffector 상자. 로프 = 사다리 상자. 포털 = 포털 컴포넌트(도착 맵·좌표).
- 런타임에 깔리는 타일 `EarlyForestSteps`(SideRoad·NormalForest 계단 2곳)는 굽기에서 같은 셀 계산으로 흉내 낸다(읽기 접근자 `StepPositions`·`AppliesTo` 추가, 동작 변화 없음).
- 표면 = 고체 가장자리를 0.25 간격으로 훑어 "아래는 고체·위는 빈칸"인 조각 → 이어 붙여 꺾은선 → 머리 위 공간(y+0.5·1.2) 막힌 곳에서 끊는다. 간선: 열린 끝 떨어지기, 겹치면 위 단방향으로 점프(≤2.3)·아래로 하향 점프, 틈(≤3.2) 건너 점프, 로프 위아래로 이어지는 구간, 같은 맵 포털.
- 1층계 결과: SideRoad 53구간·170간선·로프 16·포털 5, NormalForest 49·155·8·3, DeepForest 28·79·5·5 외 전 맵 12개.

### 11.3 간선 실행 (`GraphNavigator`, IBotNavigator)
- 진짜 입력만: 점프 = Jump 0.45초 누름(일찍 떼면 점프 컷), 점프 중 좌우는 떼고 이륙 후 `lx` 쪽. 하향 점프 = ↓(−0.5 아래) 누른 채 Jump. 로프 = 로프 x 로 걸어가 ↑/↓ **톡**(UPInventory/DownInventory 누른 순간 판정) 후 방향 유지, 닿지 않으면 점프해 잡기. 포털 = ↑ 톡.
- 이륙 범위는 벽/턱 쪽으로 0.25 안쪽, 이륙 1.5 안·간선 진행 중엔 **매 프레임 판단**(`Precise`, 10Hz 면 지나쳐 턱에 붙는다).
- 성공 = 0.3초 뒤 출발 구간 아닌 구간에 섬(중간 발판 착지도 성공, 다시 계획). 실패(시간 초과 3초·로프 4+|dy|/4초·1.6초 뒤 아직 출발 구간) → 그 간선 20초 금지 후 다시 계획. 1초마다 재계획.
- 간선 없이 로프에 매달림 → 목표 높이 쪽으로 오르내리고, 2.5초 넘으면 목표 쪽으로 뛰어내린다.
- `FaceAhead`: 막힌 끝(벽·턱) 0.3 안으로는 몸을 붙이지 않는다(직진·이륙 접근·돌아다님 모두). 돌아다님은 `BlockedAhead` 면 돌아선다.

### 11.4 실패 처리
- `BotNavWatch`(순수): 목표까지 거리 진척이 3초 없으면 **Reroute**(현재 간선 금지 + 다시 계획), 다음 3초도 없으면 **Stuck** → `RecordStuck`: 경고 `[PB][Bot] 막힘 map x y` + `PlayRecorder.RequestBookmark()`(기록 `bm` 행 + 화면 PNG = 위치·스크린샷, 20초에 한 번). 간선 실행 중·공격 MoveLock(`M` 플래그) 중엔 보지 않는다.
- 금지 간선 때문에 길이 없으면 NoPath 막힘(20초에 한 번). 포털이 3+3초 안 타지면 막힘 + 맵 이동을 30초 쉼, 한 번의 맵 이동이 90초 넘으면 포기하고 30초 사냥.
- **B1 막힘 점프 27회 원인**(11-15 확인):
  1. **계단 모서리 고착(게임 쪽):** 1칸 턱 면에 몸을 붙이고 밀면 `MovementGrounded` 가 꺼지고, 코요테 시간 뒤 남은 점프 0 → `CanMovementJump` 거짓이라 Jump 입력이 무시된다. 공중 입력은 반대 방향을 막아 빠져나오지도 못한다. 봇은 점프만 눌러대다 막힘. 위치 SideRoad·NormalForest x≈−356.6, −388, −385. → 봇은 턱에 붙지 않게(FaceAhead·이륙 안쪽·점프 때 좌우 뗌·0.5~0.8초 뒤로 물러남) 우회. 게임 수정은 `TechDebt.md` 11-15 절(P2).
  2. **오탐:** 평타·스킬 MoveLock 중 제자리를 막힘으로 셌다 → 제외.

### 11.5 전투 (`HuntBrain` + `BotCombat.cs`)
- 사거리 `BotRange`: 무기 종류별 기본값(활 7 · 석궁 6 · 마도서/투창 5 · 채찍 2.6 · 창 2.3 … 검 1.6 · 단검/손톱 1.2 · 너클 1.1, 고정 1.3 폐지), 연속 3번 헛치면 0.2 줄임(최소 0.9). 평타는 사거리 안 + 같은 층(|dy|≤1.2).
- 스킬 `BotSkill.Pick`: HUD(`SkillCooldown` 아이콘·`WeaponMasteryService.IsSlotUnlocked`)로 쓸 수 있는 칸만, 사람 `press.Skill*` 횟수 비율로 고른다. 바라보고 max(사거리+0.5, 3) 안이면.
- 대시: |dx| 3.5~9 에서 1.2초 간격(사람이 대시로 먼저 붙는 것 흉내 — 반응 시간 단축의 주 원인).
- 회피 `BotAvoid`: 적이 예고·공격 상태로 가로 3.5·세로 1.5 안에 들어온 창마다 한 번, 사람 `avoid.r.*` 비율로 반응 종류(맞받아 공격·앞으로·물러남·점프·대시·무시), 지연 = `avoid.rt` 샘플, 0.35초 유지.
- 포션 `BotPotion`: 문턱 = `potion.hpPct` 샘플(30~85 자름). 퀵슬롯에서 이름으로 HP 포션 칸을 찾고, 눌렀는데 HP 가 안 오르면 그 칸 30초 제외.
- 맵 순환: `PlayBot.Run(분, goal, maps)` 기본 `Lv1_NearForest_SideRoad,Lv1_NormalForest,Lv1_DeepForest`, 맵마다 120초(적이 20초 없으면 일찍) 사냥 후 다음 맵(왕복). `-` = 맵 이동 없음.

### 11.6 실행·지표
`bash Tools/bot_run.sh <분> [goal=hunt] [시작 맵=Lv1_NearForest_SideRoad] [순환 맵 쉼표 목록|-]`. 상태 줄에 kills·deaths·mapMoves·stucks·reroutes·edgeFails·navJumps·downJumps·ropes·portals·skills·dashes·avoidMoves·avoidWindows·potions·range.

### 11.7 사람 대 봇 (11-15, `bot_run.sh 10`, Lv8 세이브, 세션 20261115_225213)
| 지표 | 사람 | B1(5분) | B2(10분) | 메모 |
|---|---|---|---|---|
| 반응(적 6칸 진입 → 첫 공격) 중앙 | 0.71초 | 2.16초 | **1.24초**(n=25, 무반응 9) | 합격선 1.4 통과. 앞선 10분 실행 1.18초(n=23) |
| 처치/분 | 14.7(합) | 13.7 | 13.1(135) | 앞선 실행 15.5 |
| 막힘(Stuck) | — | 막힘 점프 27 | 15(재계획 18·간선 실패 28·막힘 점프 1) → 수정 후 10분 8·8분 5·15분 13(NormalForest x≈380 후반 포함) | 앞선 실행 26. 남은 것 대부분 NormalForest x≈−356.6 계단 모서리(게임 버그) |
| 사망 | 0 | 0 | 0 | |
| 맵 이동 | — | 0 | 1(SideRoad → NormalForest) | 11-16 수정 후 15분: 3회(Side→Normal→Deep→Normal) 순환 확인. 10분 실행은 NormalForest 가 넓어 1회(포털까지 약 8분) |
| 로프·하향 점프·이동 점프 | — | — | 6 · 3 · 45 | |
| 스킬·대시 | 스킬 많이 씀 | 0 | 94(9.1/분) · 57 | |
| 회피(예고 → 첫 반응) | 사람 분포 | 없음 | 예고 73건, 반응 시간 중앙 0.20초, jump 21·none 19·fwd 18·back 12 | ForestHunter 사격은 96% 맞음(반응해도 못 피함) |
| 평타 간격 중앙 | 0.21초 | 0.22초 | 0.22초 | |
| 포션 | 문턱 사용 | 없음 | 0 | 수정 후 NormalForest·DeepForest 에서 3~10회 실사용, HP 117→213 처럼 회복 확인(11-16) |
- 사거리 수정(11-16): 구 무기 프리팹은 `Weapon._kind` 가 비어 1.3 이었다. `HuntBrain` 이 `WeaponMasteryService.KindOf(MasteryKey)` 로 보충(WeaponMoveProfiles 와 같은 규칙) → 1.9(대검). 15분 실행 끝무렵 DeepForest 에서 1.3 으로 다시 보인 구간은 원인 미확인.
- Lv3 세이브(level 3·exp 0 으로 고쳐 8분, 끝나고 원본 md5 복원): 최대 HP 116(Lv8 은 156), 사망 0, 162처치, 맵 이동 1, 막힘 4, 포션 1.
- 상태 줄: `avoidMoves`(뒤·앞·점프·대시 반응 횟수, 틱 아님) `avoidWindows`(예고 창 수)로 이름을 바로잡았다.

## 12. 봇 B3 — 마을 업무 (11-16, 축소판)
### 12.1 흐름 (`BotErrand` 가 HuntBrain 안에서 돈다)
1. **귀환 판단**(사냥 중 1초마다): 가방 빈칸 ≤ 문턱 또는 HP 물약 수(퀵슬롯 등록분) ≤ 문턱이면 마을로. 문턱은 사람 프로필 `return.bagFree`·`return.potions` 표본을 자른 값(빈칸 3~8, 포션 2~8 — 사람 중앙값 11칸·15개를 그대로 쓰면 1분마다 귀환). 마을에서 돌아온 뒤 90초는 사냥(포션 0 이면 예외). 문턱은 방문마다 다시 뽑는다.
2. **마을로**: 퀵슬롯의 귀환 주문서가 있으면 진짜 퀵슬롯 키로 쓰고 3.6초 제자리(맞으면 취소 → 걸어감). 없으면 `TravelTo("Town")`(포털 BFS). 길이 15초 넘게 없을 때만 접는다(2분 쿨다운).
3. **마을 업무**(단계마다 45초 제한, 넘으면 접음): 장착 → 걷기 → 말 걸기 → 판매 → 구매 → 닫기.
   - 장착: 같은 부위(무기·의상·반지·목걸이)에서 점수(무기 = 평균 피해, 방어구 = (기본 공격+방어)×강화 배율)가 착용품보다 높은 가방 장비. 가방을 `I` 로 열어 방향키로 칸까지 가서 Select ×2.
   - 걷기·말 걸기: `ShopUI` 위치로 걷고(`isShopClose` = 상호작용 범위), ↑ 로 말 걸고, NPC 선택지면 상점 줄로 ↑↓·Select.
   - 판매: 진열 → 가방으로 → 가방 페이지 고르기(boxChange) → 칸 → Select(상세) → Select(수량 창) → Select(확인). 잡템은 `J` 두 번(일괄 판매). 팔 장비 = 폐기 장비, 또는 같은 부위 착용품보다 점수가 높지 않은 것(중복 포함). 잠금·귀걸이 등 따로 다루는 부위·점수 모름은 안 판다.
   - 구매: `H`(상점 퀵슬롯 충전). **임시 규칙**: 골드 ≥ 2000 일 때만. H 는 퀵슬롯 소비품(음식·귀환 주문서 포함)을 가득 채워 골드 대부분(약 2만)을 쓴다 → 후속: 메뉴 구매(수량 입력)로 HP 물약만 골드 비율대로.
   - 닫기: `I`(전부 닫기) → 끝나면 일반 사냥 이동(`Travel` 이 첫 사냥 맵으로).
4. 키 입력은 `BotKey` → `PlayBot.PressMenu` → `BotInput`. 액션이 있는 키(↑↓←→·Z·ESC·I)는 `Press`, 액션 없는 H·J 는 `BotInput.PressKey`(장치 경로). **숫자 키는 쓰지 않는다**(퀵슬롯 사용과 겹쳐 비상식량이 소모됨). 가방 페이지는 칸 왼쪽 끝에서 ← 로 페이지 고르기에 들어가 ↑↓·Select.
5. 키 간격 = 프로필 `menu.focusInterval` 표본(0.22~0.9초). 상태 판단은 `BotMenuNav`(순수, EditMode 시험), 읽는 것은 `InventoryManager.state·nowBox·cusorCount·boxCusor`·`NpcMenu`·팝업 모달 여부뿐(쓰기 없음).

### 12.2 시험 방법
`PREP_BAG="<남길 물약> <남길 빈칸> <1=착용 무기를 Wood Spear 로>" bash Tools/bot_run.sh <분>` — 봇 시작 전 `bot_prep_bag.cs` 가 가방을 시험 상태로 만든다(봇 행동 아님, 세이브는 스크립트가 백업·원복·md5). 상태 줄에 `trips sold bought equipped gold+= weapon recalls errandAborts` 추가.

### 12.3 결과 (11-16)
| 실행 | 처치 | 왕복 | 판매 | 구매 | 장착 | 막힘 | 사망 |
|---|---|---|---|---|---|---|---|
| 3분 ×2(약화 무기 시험) | 29~31 | 1 | 10~11 | 98(HP 물약) | 2(무기·의상, 무기 PhantomScythe 복귀 확인) | 1~2 | 0 |
| **15분**(`PREP_BAG="3 3 1"`, 세션 20261116_013152) | 158(10.2/분) | 1 (마을 → SideRoad → NormalForest) | 11 | 97 | 2 | 6 | 0 |
- 사람 14.7/분 대비 처치율 낮음: 마을 업무 약 1분 + NormalForest 이동. 시험 가방이라 귀환은 첫 1회 뿐(구매 뒤 물약 99).
- 귀환 주문서 실사용 0회(시험 세이브에 없었음, 이번 방문에서 H 로 73개 구매 → 다음 방문부터 사용 경로. 미검증).

### 12.4 찾은 원인·수정
- **NormalForest x≈364~383 막힘 8건(`do=drop`)**: 가방 빈칸 0~3 에서 같은 드롭 발밑을 왔다 갔다(줍기 거절). `HuntBrain.DropOk`: 발밑에 1.5초 넘게 있으면 30초 무시 + 귀환 판단이 가방을 비운다.
- **DeepForest range 1.3 복귀**: 무기 교체 순간 `Weapon.Kind == None`이 한 틱 보이면 `BotRange.SetKind(None)` 이 기본 1.3 으로 되돌렸다(유력 — 직접 재현은 못 함). `None` 이면 갱신하지 않고, 30초에 한 번 경고 `무기 계열 None` 으로 상태를 남긴다.
- **Town 이동 그래프가 "지형과 다름"으로 버려짐**: 베이크가 꺼진 오브젝트("sample (n)")·꺼진 타일맵 콜라이더의 허공 발판 5개를 넣었다 → `BotNavBaker` 가 꺼진 오브젝트·꺼진 TilemapCollider2D 를 건너뜀(다른 맵 JSON 은 변화 없음), Town.json 재굽기(구간 1개).
- 마을 업무 버그: 키 간격에 막혀 안 눌린 틱도 시도로 세던 것, 눌리지 않았는데 기다림을 시작하던 것(H·J), 판매 직후 전환 상태에서 Back 을 눌러 상점이 닫히던 것.

### 12.5 남은 것
- SideRoad x≈-386 `do=drop` 진척 없음 16건(15분 실행 마을 복귀 직후): 닿지 않는 드롭(1칸 턱 위 발판?)으로 추정 — 미조사.
- NormalForest → DeepForest `맵 이동 경로 없음` 4건(막힘 직후 간선 금지) 재계획 대기.
- 구매를 H 대신 메뉴 수량 입력으로(골드 비율), 퀘스트 따라가기(B3 나머지), 귀환 주문서 실사용 확인.

## 13. 봇 B3 보완 + B4 자동 보고 (11-16)
### 13.1 B3 보완
- **구매 = 메뉴 직접 구매**: H(일괄 충전)는 응급식량·귀환 주문서까지 채워 골드가 -2만씩 빠졌다 → 제거(`RefillMinGold` 2000 임시 규칙도 삭제). `BotErrand.DoBuy`: 판매를 마친 가방 커서에서 ←(boxChange 를 지나 ← 한 번 더 = 상점 진열 복귀. **Back 은 상점을 닫는다**) → 진열에서 HP 물약(퀵슬롯에 등록된 종류, 없으면 Medium) 칸까지 ↑↓←→ → Select ×2(상세 → 수량 창) → 수량 창에서 ↑↓(±1)·←→(±10)로 목표 수량 → Select. 수량 = 가진 골드의 25~35%로 살 수 있는 수를 스택 한도(99) 남은 수로 자른 값(`BotErrandRules.PotionBuyAmount`·`ToQuantity`·`ToStockCell`, EditMode 시험). 창이 최대값에 막혀 안 늘면 2번 뒤 확정.
- **단계 시간 제한**: 판매·장착이 하나씩 진행되는 동안(`SellDone+EquipDone` 증가)은 45초 제한을 새로 센다. 옛 장비·잡템 14개 판매가 45초를 넘겨 구매 전에 업무가 접히던 버그(30분 첫 실행, 세션 022417). 중단돼도 이미 한 판매·장착은 센다.
- **닿지 않는 드롭**: `HuntBrain` 이 같은 자리 드롭을 10초(`DropChaseSeconds`) 넘게 쫓으면 그 좌표(반경 1.5)를 60초 무시 목록에 넣는다(경고 `닿지 않는 드롭 … 무시`). 발밑 가방-가득 규칙은 그대로. 마지막 10초 값은 30분 재실행 미검증(20초 값으로는 경고 32건 남음).
- **맵 이동 경로 없음 원인**: 이동 그래프는 NormalForest 전 구간에서 포털(seg 46)까지 길이 있다(오프라인 BFS 로 확인). 실패는 길 부재가 아니라 걷기 실패다. 경고에 `이유=`(`GraphNavigator.TravelFail`)를 달았다. 막힘 북마크(NormalForest x≈-137, y 11.3, 1칸 턱 앞) PNG 에는 오른쪽에 나무 상자가 보인다 → 길을 가로막는 오브젝트 가능성. **미해결**(TechDebt).
- **귀환 주문서**: 메뉴 구매는 물약만 산다. 시험은 `PREP_RECALL=<개>`(`bot_prep_recall.cs` 가 가방에 넣고 퀵슬롯 3번 등록). **실사용 확인**(세션 035806): t=70 `퀵슬롯 3 사용: Recall Scroll` → t=94 상점 앞 도착, recalls=1·왕복 1·구매 96.

### 13.2 B4 자동 보고서
`bash Tools/bot_run.sh <분>` 한 번 → 끝나면 `Logs/playrec_bot/<세션>_{summary,balance,report}.md`. `Tools/playrec_report.py`(300줄 이내): ①한눈에(왕복·판매·구매·장착·막힘·사망·처치/분) ②막힘(위치·북마크 PNG) ③예외·error 상위 ④사망 ⑤봇 경고 상위 ⑥사람 대비(11-15 사람 기록 2개를 `playrec_balance` 로 돌린 `human_1115_balance.md` 와 맵·낮밤별 처치/분·EXP/분·받은피해/분, 몹별 처치시간·피해/처치를 비교, 2배 초과 ▲·절반 미만 ▼) ⑦밸런스 표. 최종 상태 줄은 `--status` 로 받는다.
- **손 시험 기록 분리**: 에이전트가 손으로 돌리는 플레이는 `Tools/playtest.sh` 가 시작 전 기록 출처를 `test` 로 걸어 `Logs/playrec_test/` 로 간다(`PlayRecWriter.DirNameFor("test")`). 사람 `Logs/playrec/`·봇 `Logs/playrec_bot/` 와 섞이지 않는다.
- **`PREP_TIME=day|night`**(11-16): 낮/밤 고정(`bot_prep_time.cs`, 런타임 `TimeChange` 만, 세이브 불변). 밤은 `_lockedTimeValue` 를 밤 구간 중앙으로. 낮밤 밸런스 비교용(`Docs/Balance/BotBalance_1116.md`).
- 시험 준비 환경변수: `PREP_LEVEL="<Lv> <골드>"`(`bot_prep_level.cs`), `PREP_BAG`, `PREP_RECALL`. 모두 봇 시작 전 시험용, 세이브는 스크립트가 백업·원복·md5 확인.

### 13.3 합격 실행 (Lv3·골드 300, 마을 시작, 30분, `PREP_BAG="6 8 1"`)
세션 20261116_032517: 처치 479(15.7/분)·왕복 3·판매 19·구매 96(Small HP Potion, 골드 약 2400)·장착 2·업무 중단 0·막힘 11·진척없음 경고 32·사망 1(DeepForest, Lv3)·골드 +15400·세이브 원복 md5 일치. 수정 전 같은 조건(세션 022417): 단계 시간 초과로 업무 중단·구매 0.
- 퀘스트 따라가기는 범위 밖(TechDebt).

## 14. 봇 막힘 정리 (11-16)
- **NormalForest x≈-137 "나무 상자" 막힘 = 상자 탓이 아니다.** 상자는 `Crate`(`Assets/Art/Props/Crate.png`, 정식 그림) `BreakableProp` — 콜라이더가 트리거(통과 가능, 공격으로 부숨). 기록 tk 행에서 봇은 그 자리를 3.24/초로 계속 걸었다. 막힘 경고는 `BotNavWatch` 가 "거리 지표(|목표x-내x|)가 3초 줄지 않음"으로 판정한 오탐(계획 목표가 바뀌거나 지나치면 지표가 늘어난다) → 간선 금지 → NoPath 막힘으로 이어졌다.
  - 수정: `BotNavWatch.Check(now, metric, x)` — 마지막 진척 뒤 1.5 넘게 움직였으면 진척. 제자리 정지는 그대로 Reroute → Stuck.
- **SideRoad x≈-313 `do=drop` 진척 없음**: 드롭이 위 발판(dy +5.6, 크레이트 드롭)이고 로프 간선이 금지돼 NoPath → 봇이 제자리에 17초 서 있었다(`Steer` 거짓 반환을 무시). 수정: `HuntBrain.SteerDrop` — 이동 거짓이 1.5초(`DropNoPathSeconds`, 순수 `BotStall`) 이어지면 그 드롭 60초 무시(경고 `길 없는 드롭`).
  - **드롭 무시 10초(`DropChaseSeconds`) 검증**: 같은 좌표가 안정되면 10초 뒤 무시된다(11-16 로그 410~434초). 단 근처 드롭 둘이 번갈아 가장 가깝게 잡히면 식별이 바뀌어 센 시간이 리셋된다 → 길 없음 규칙이 먼저 끊는다.
- **리포트 메타 시작 Lv·골드**: 첫 meta 는 Play 직후(준비 전)라 Lv8·골드 0. `PlayBot.Run` 이 `PlayRecorder.WriteStartMeta` 로 meta 를 한 번 더 쓰고 리포트·요약은 마지막 meta 를 읽는다.
- **시험 기록 분리 규칙**: `Tools/u.sh` 가 실행될 때마다 `Logs/.agent_active` 를 갱신하고, 그 파일이 30분 안이면 미리 정한 출처가 없는 Play 는 `test`(`Logs/playrec_test/`)로 간다(`PlayRecAgentFlag`). 사람이 에디터에서 직접 Play 하면(에이전트 활동 30분 뒤) 그대로 `Logs/playrec/`. MCP 도구를 `u.sh` 없이 직접 부르는 에이전트는 먼저 `bash Tools/u.sh list` 로 플래그를 갱신한다.
- **검증**(Lv3·골드 300, `PREP_BAG="6 8 1"`, 15분, 세션 20261116_043358): 처치 228(14.8/분)·사망 0·막힘 6·진척없음 1·왕복 2·판매 19·구매 94. 이전 30분(032517) = 막힘 11·진척없음 32·사망 1 → 15분 환산 5.5·16. 진척없음 16→1, 막힘은 0.37→0.39/분으로 같다. 메타 시작 Lv3·골드 300 확인.
- **남은 막힘**: SideRoad x≈-386~-400, y 0.3~1.9(계단 구간 seg 24·27·28) 5건 + NormalForest 시작부 1건. 이 구간은 이동 그래프 계단 간선 실행이 막힌다. 후속.

## 15. 봇 퀘스트 따라가기 + 계단 막힘 조사 (11-16)
### 15.1 퀘스트 따라가기 (`BotQuest` + `BotQuestRules` 순수, `HuntBrain` 이 부른다)
- **읽기만**: `QuestService.StatusOf`·`NpcQuestCatalog.All`·`NpcQuestTrack.Tracked`(추적 중 우선). 완료·수락 함수(`QuestService.Report/Accept`)는 부르지 않는다.
- **사냥 방향**: 진행 중 처치 퀘스트(추적 → 표 순서) 한 건의 목표 몹 id 로 `HuntMap`(그 몹이 나오는 맵 — 시작 때 `EnemySpawner.enemySlot` 프리팹 이름 + `HuntingSpawnTable` 프로필을 맵별로 모음)과 `IsTarget` 을 정한다. `HuntBrain.EffMaps` 가 순회 목록 대신 그 맵 하나를 쓰고(`ctx.Maps` 가 비면 이동 없음 그대로), `FindTarget` 은 목표 몹이 보이면 다른 몹을 뺀다(없으면 아무거나).
- **마을·NPC 일**(`BotQuestRules.PickJob`): 보고 가능(Ready) 퀘스트(지금 맵 → 추적) > 진행 중 처치가 2건 미만일 때 받을 수 있는 처치·"수락한 NPC 와 대화"(메인 → 지금 맵). 수집·제작·방문·게시판·현상은 받지 않는다. NPC 맵 표 `NpcMap`(elder·ruen·sena·merchant·smith = Town, eve = NormalForest).
- **진짜 입력**: `TravelTo`(포털)·`Steer` 로 NPC 앞까지 → ↑ → NPC 선택지에서 "퀘스트 이야기" 줄로 ↑↓ + Select(`ToNpcQuest`) → 대화 중 Select 반복(타이핑 끝·다음 줄·선택지 첫 줄 "수락한다"). 상태가 Done(보고)/Available 아님(수락)이 되면 끝. 150초(이동)·40초(대화) 제한, 실패 2번이면 그 퀘스트 10분 접음, 중단 뒤 120초 쉼. 마을 귀환 업무(BotErrand)가 진행 중이면 양보.
- 상태 줄 추가: `questsDone`(시작 대비 완료 수)·`questAccepted`·`questReported`·`questAborts`·`huntMap`·`quest`. 보고서 1절 "퀘스트" 줄.
- **검증**(Lv3·골드 300, `PREP_BAG="6 8 1"`): 8분 세션 050002 — Q106 보고 1건 완료(진짜 입력, 보상 EXP 600). 20분 세션 051218 — 처치 408(19.9/분)·사망 0·왕복 4·판매 22·구매 94·퀘스트 완료 1·받음 0·막힘 11·업무 중단 2·세이브 원복 md5 일치.
- **한계(미검증)**: 퀘스트 받기(수락 대화) 경로는 실제로 한 번도 실행되지 않았다(시험 세이브에 이미 진행 중 퀘스트가 많아 `PickJob` 이 받을 일을 못 골랐다). 사냥 방향은 SideRoad 한 맵에 머물렀다. 밤 전용 몹(허수아비)·NormalForest 몹 퀘스트로 맵을 옮기는 경로도 이번 실행에서는 안 탔다.

### 15.2 SideRoad 계단 막힘 조사 (x≈-386~-400, y 0.3~1.9)
- 8분 세션 050002 막힘 4건 tk 행 분석: ① x≈-385.5~-386, y 1.26~3.9 = 계단 27→28 위에 선 쥐 도둑·쥐 대장(`RatThief_BD`·`RatChief_BD`, dy +0.94)에게 접근하다가, 공중에서 사거리 안(|dy|≤1.2)이라 공격 분기가 `MoveX=0` 으로 입력을 끊어 점프가 제자리로 떨어지기를 반복(공격 중 접근 이동 충돌). ② x≈-399.4 = 로프 0(seg24↔25) 위·아래에서 점프·낙하 반복(y 0.3~11).
- 시도: 근접 사거리에서 한 칸 위 몹은 올라가서 싸우고 공중에서는 이동을 끊지 않게 하는 `StepFoe` 규칙 → 20분 실행에서 막힘 11(0.55/분, 이전 15분 6건 0.39/분) 로 **개선 확인 못 해 되돌렸다**(커밋에 없음).
- **미해결**: 로프 0 상·하단 이탈, 위 발판 몹 접근 시 공격-이동 충돌. 다음 조사 후보: ① 로프 간선 위 끝에서 `Jump`/`↑` 이탈 규칙 ② 점프 간선 진행 중 `Fight` 공격 분기가 이동 입력을 끊지 않게(간선 중 플래그).

### 15.3 계단 막힘 수정·퀘스트 받기 검증 (11-16 후속)
- **수정**: `HuntBrain.Fight` — 공중(`!Grounded`)이고 몹이 다른 발판(|dy|>0.5)이면 공격 분기를 건너뛰고 접근(`Steer`)을 이어 간다(점프가 제자리로 떨어지던 것 방지). `GraphNavigator.RepeatBan` — 같은 간선을 30초 안에 5번째 시작하면 15초 금지(`EdgeRepeatBans`).
- **퀘스트 받기 검증**: `PREP_NOQUEST=1`(새 프로브 `bot_prep_noquest` — 진행 중 퀘스트 전부 포기, 10건 → 0) + Lv3·골드 300. 10분 세션 114550: **받음 1** (NPC 대화 진짜 입력으로 수락 경로 실행 확인). 완료 0·보고 0(10분 안에 못 끝남).
- **10분 지표**(114550): 처치 189(18.3/분)·사망 0·막힘 5(0.5/분)·진척없음 12(do=drop)·왕복 2·세이브 md5 일치. 막힘 위치 x≈-378~-401(계단·로프0 근처 그대로). 이전 20분 11건(0.55/분)·15분 6건(0.39/분)과 비교해 **뚜렷한 개선 없음**(표본 작음). 막힘 #3 y=7.2 는 로프 위.
- **미해결**: SideRoad x≈-380~-401 막힘이 0.5/분 수준으로 남음. 반복 금지는 대안 경로가 없으면 NoPath 로 가므로 근본 해결이 아니다. 다음: 막힘 직전 tk 행으로 간선 종류별 실패 집계. 코드 리뷰어 미실행(예산).

### 15.4 계단·로프0 막힘 진단 (11-16 후속)
- **도구**: `Tools/playrec_stuck_tk.py <jsonl> [--sec 5 --xmin -410 --xmax -370]` 막힘 경고 직전 tk 를 상태(접지·로프·상승)·mx/my·점프 입력·x/y 범위로 집계. `--pass` 는 구간 통과 시간·점프 수(사람 기록과 비교). 간선 종류는 tk 에 없어 입력·속도로 추정한다.
- **지형은 문제 아님**: 사람 기록(20261115_13*) x -401→-386 통과 2.9~14.7초·점프 0~6. 봇 대부분 5~7초. 로프1(x=-393.6) 트리거 y 7.9~15.0, 위 발판 y≈12.7(로프가 발판을 지나 더 이어짐), 로프0(x=-399.4) y 0.58~7.65.
- **게임 버그 1건 수정(f79fea024)**: `PlayerController.isLadder` 는 `OnTriggerEnter2D` 에서만 켜진다. 공격·스킬 `LadderLock` 중에 로프 트리거에 들어오면 Enter 를 놓쳐, 잠금이 풀려도 트리거 안에 서서 ↑ 가 안 먹는다(발판 y=7.2 가 로프1 아래 끝 7.9 와 겹침 → 같은 자리 점프 반복). `OnTriggerStay2D` 로 복구. 프로브 `rope_stay_enter`/`rope_stay_read` 로 수정 전 False · 후 True 확인.
- **남은 봇 원인**: ② 로프를 오르다 목표 발판 높이를 지나 로프 끝(15)·점프 최고점(18.5)까지 갔다 떨어져 재잡기(RopeStep 이 위 끝만 기다림, RopeLoose 는 목표가 같은 높이여도 `dy>-0.5` 라 위로) ③ 계단 위 쥐 도둑 접근 중 회피(Back)·공격 분기와 충돌.
- **시도·실패**: RopeLoose 같은 높이면 옆 점프 하차 / RopeStep 에서 FeetY≥목표Y-0.15 면 옆 점프 하차 / 둘 병행. 15분(낮 고정 `PREP_TIME=day`, Lv3, 처치 400~460)에서 막힘 11·9·15건(분당 0.6~1.0)으로 기준(11건 0.73/분)과 차이 없어 **모두 되돌렸다**. 규칙만으로 부족 — 막힘 순간의 현재 간선·목표를 기록(예: tk 에 `ed2` 간선 종류·`tg` 목표 uid)해 다시 본다.
- **별건(미수정)**: 밤에 SideRoad 몹이 전부 사라지면 Wander 가 x≈-437 왼쪽 구덩이로 걸어 떨어져 `낙사 복귀` 를 8분 반복. 막힘 기준 비교는 `PREP_TIME=day` 로 해야 한다. 밤 비교 시 이 시간이 처치율을 깎는다. → 15.5 에서 수정.

### 15.5 간선 기록 필드 + 구덩이 반복 수정 (11-16 후속)
- **tk 새 필드(src=bot 만, 사람 형식 불변)**: `be` 따라가는 간선(walk·drop(낙하 간선)·jump·jumphigh·down·rope·portal·wander·none), `bn` 길찾기 목표 구간(없으면 -1), `bf` Focus(몹 Uid=인스턴스 ID라 음수일 수 있음 · 0 없음 · -1 배회 · -2 드롭 · -3 창). `BotTrace`(정적, 마지막 Decide 값)를 `PlayBot.Decide` 가 비우고 `GraphNavigator.SteerTo`·`HuntBrain.Wander` 가 채우며 `PlayRecTick.WriteBotTrace` 가 쓴다.
- **집계**: `python -X utf8 Tools/playrec_stuck_tk.py <jsonl...> --edges [--sec 5]` — 막힘 경고 직전 구간에서 가장 오래 따라간 간선·Focus 분류별 건수.
- **구덩이 반복 원인·수정**: 구덩이 너머는 바닥이 없어 `Terrain.DropF = -1` 인데 Wander 의 돌아서기 조건이 `DropF > 2` 뿐이었다. `DropF < 0` 도 돌아서게 고침(`HuntBrain.Wander`).
- **검증**: 밤 15분 161911 — `낙사 복귀` 0회(수정 전 143651 에서 424회), 처치 224(14.6/분)·사망 0, 막힘 25. 막힘 직전 간선: rope 13·walk 11·jump 1(walk/드롭 Focus 9). 막힘 전부 SideRoad x≈-378~-399. 낮 15분 163816 — 처치 457·사망 0·막힘 9(walk 4·rope 3·wander 1·jump 1, Focus 전부 몹)·낙사 복귀 0. EditMode 1421 통과·콘솔 error 0. 한계: 밤 실행은 몹이 남아 Wander 비중이 작아 구덩이 상황을 충분히 만들지 못했다(재현 조건 미확인).

### 15.6 로프 막힘 원인 확정·수정 (11-16)
- **원인**: tk 기록(세션 161911 t≈206)에서 로프 위 끝을 벗어난 순간 `Grounded` 가 잠깐 참이라 `RopeStep` 의 "로프 밑이 머리보다 높다 → 점프" 가 발동 → 로프 끝(피벗 y≈8.5)에서 수직 점프(최고점 11.2) → 떨어지며 ↑ 톡으로 로프 재잡기 → 반복. 발판(seg 25, 발 6.4)은 로프 끝(7.65) 아래라 가만히 있으면 내려앉는다. 로프 끝에서 점프 입력 발생 24건(밤)·38건(낮) 확인. 이전 규칙(RopeLoose·발판 높이 하차)은 로프 위에서만 동작해 이 지점을 못 건드렸다.
- **수정**: `GraphNavigator.RopeStep` — 로프를 탄 뒤(`_ropeCounted`) 로프를 벗어났고 발이 목표 발판 높이(`RopeArrived`, ±0.6)면 점프·↑ 없이 둔다. 순수 `RopeArrived` EditMode 테스트 추가.
- **전→후(15분)**: 밤 161911→165926 막힘 25→11(rope 13→1, walk 11→7), 처치 224→317. 낮 163816→171529 막힘 9→1(walk 1), 처치 457→579. 사망 0·세이브 md5 일치·EditMode 1422 통과·콘솔 error 0.
- **남은 walk 막힘**: 밤 7건(Focus 몹 5) — 미조사.

### 15.7 밤 walk 막힘 7건 진단 (11-16, 코드 변경 없음)
- 대상: 밤 165926(막힘 11, 직전 5초 간선 walk 7). tk 의 be·bn·bf·적 상태를 막힘별로 풀어 봄. 전부 SideRoad x≈-400~-375 계단·로프0 근처, 원인은 한 가지가 아니다.
- ① 계단 점프 튕김(386·426·451·486·746, 5건): 계단 한 칸(≈1.0 높이)을 점프로 올라 y=3.0 까지 갔다가 아래 칸(y=1.3)으로 되떨어지고, 같은 칸에서 vx=0 으로 1초쯤 벽에 밀며 서 있다 다시 점프. 3초간 순이동 1.5 미만이라 BotNavWatch 가 막힘으로 센다(실제로는 올라가는 중, 한 계단 4~7초).
- ② 몹 되돌림(217·510·766, 3건): 목표 쥐 도둑이 봇 뒤·아래 칸(dx≈-2, dy≈-2)으로 돌아 들어오면 `airOtherFloor` 접근이 아래쪽으로 향해 오르던 점프를 되돌린다. 처치 직후 Focus 가 몹 4마리로 0.5초마다 바뀌며 bn 도 24~28 로 바뀜(처치로 인한 재선택). 같은 층(|dy|≤2) 우선 규칙은 계단 칸 높이 차(≈1~2) 때문에 잘 안 걸러 준다.
- ③ 별건: 850 은 y=14 위 발판에서 mx 0 으로 5초 서 있음(Focus 배회, 몹 근접 dy≈0) — 공격 안 하는 정지, 미조사.
- 결정: 한 가지 수정으로 줄지 않아(①은 계단 본질, ②는 사냥 재선택) 봇 코드는 고치지 않음. 후속 후보: 계단 칸을 한 구간으로 보고 목표 몹과의 세로 차 판정을 경로 기준(seg)으로, 막힘 판정에 "점프 간선 수행 중 위로 진척" 반영. 실효 막힘 손실은 밤 15분 처치 317(21/분)·사망 0 이라 작음.

### 15.8 막힘 판정 계단 오탐 수정 (11-16)
- `BotNavWatch.Check(now, metric, x, y, climbing)` 추가: 점프·로프 간선을 최근 5초 안에 수행했고 최근 5초 y 기록의 바닥(가장 낮은 y)이 처음보다 0.5 이상 올랐으면 진척. 순수 함수 `ClimbRise`. 제자리 점프(바닥 안 오름)·같은 높이 정지는 막힘 그대로.
- 검증: 165926 tk 로 15.7 의 5건(386·426·451·486·746)은 rise 1~2 로 제외, 나머지 6건(217·329·510·663·766·850)은 유지(파이썬 재적용). EditMode 1426 통과. 밤 15분 재실행(174527): 막힘 10, 사망 0, 처치 410. 그중 450·817 은 rise>0 인데도 찍힘 — Watch 가 아닌 NoPath(`RecordStuck` 500행) 경로로 추정, 미조사.

## 16. 봇 퀘스트 수락 검증 + Lv 사냥터 + 스킬 투자 (11-16, B5-1)
- **퀘스트 수락(진짜 입력)**: 15절 코드가 그대로 동작함을 처음 확인. `PREP_NOQUEST=1`(진행 퀘스트 전부 포기)·`PREP_LEVEL="1 300"` 마을 시작 30분(세션 20261116_220002): 수락 이벤트 3건(Q106 t=194·Q104 t=334·Q105 t=339, 마을 왕복 3회), 대사 단계 넘기기·선택지 "수락한다"까지 Select 입력만 사용. 8분 시험(세션 214424)도 Q106 수락 확인.
- **Lv 사냥터**: `BotQuestRules.LevelMap(Lv, maps)` — Lv1~3 샛길·4~6 일반 숲·7+ 깊은 숲(MapReview 권장 Lv 에서 경계만 한 단계 안전하게). `HuntBrain.EffMaps`: 처치 퀘스트 목표 맵 > Lv 맵 > 맵 목록 순회. `PlayBot.Run` 에서 맵 인자를 비우면(기본) `ctx.LevelMaps=true`, 맵을 직접 주면 기존 순회 그대로. 30분 실행은 수락한 처치 퀘스트(Q104·Q105·Q106) 목표 몹이 전부 샛길이라 Lv6 까지 샛길에 머물렀다(퀘스트 우선이 맞게 동작). Lv 맵 전환 자체는 EditMode 시험(`PlayBotSkillTests`)만 확인 — 퀘스트 없는 상태 실플레이 미검증.
- **스킬 투자(진짜 입력)**: `BotSkillInvest`(HuntBrain 이 errand·quest 다음, CloseUi 앞에서 부름) + 순수 `BotSkillRules`. 포인트(`SkillTreeService.Available` 읽기)가 있고 전투 중·다른 창 없음이면 `SkillWindow` 단축키 → 탭 → (→) 줄 ↑↓ → `UI/Enter` 확인 → 목표 없으면 Back. 우선순위: 타입 트리 P1 5·A1 3·M1 3·P2·A2·M2…, 공용 C5·C6·C3·C1·C2 를 3·6·10 라운드로, 마지막 C4·C7. `BotKey.SkillWindow`·`Confirm` 추가(스킬창은 `UI/Enter` 를 쓰고 UI/Select 와 키가 다르다), `SkillTreeWindow.CurrentTab·CursorIndex` 읽기 접근자 추가. 이벤트 `invest`(기록기). 30분: 투자 8건·중단 0, Lv 1→6.
- **30분 수치**(Lv1·골드 300 시작, 마을 출발): 처치 590(19.3/분)·사망 0·Lv6·수락 3·완료 0·스킬 투자 8·막힘 6·진척없음 17·귀환 업무 중단 6·맵 방문 Town→샛길→Town→샛길→Town→샛길(마을 3회는 전부 퀘스트 수락). 세이브 원복 md5 일치·콘솔 error 0·EditMode 1431 통과.
- **미해결 — 완료 0**: t≈955 에 샛길 안쪽 포털(x≈-236)로 넘어간 뒤 아래 지형(y -2.7~-0.7, x -250~-200)에서 이동 그래프가 마을 포털까지 길을 못 찾아(`귀환 업무 중단 … 포털 쪽 걷기 실패 nav=44>24` 5회) 마지막 14분 처치가 멈추고(590 고정) 완료 가능(Q104·Q106 6/6) 퀘스트를 보고하러 못 갔다. PlayBotPlan B5 6번(NoPath 막힘)과 같은 원인 계열. 다음: 안쪽 포털로 들어간 경위(드롭 추격 중 의도 없는 접촉인지)와 그 구역의 그래프 연결 조사.

### 16.1 안쪽 구덩이·퀘스트 따라가기 보강 (11-17)
- **구덩이 원인 확정(그래프 문제)**: SideRoad x≈-257~-139·y≈-3.6~-1.6 지형(구간 44·46~50)은 구덩이다. 나가는 길이 ①안쪽 포털 x=-236(→x=-728 같은 맵) ②로프 x=-229.5(y0 0.93~12.7) 둘뿐인데, 베이커가 로프 아래 끝이 구간 표면에서 2.53 높다는 이유로(허용 2.4) 로프 간선을 만들지 않았다. 사람 기록(133116)은 같은 로프를 점프로 잡아 올라갔다(x=-229.5·y 2.1). 포털 간선 47→1 이 있어도 1~22 서쪽 섬은 마을 포털(구간 24)과 이어지지 않아 `nav=44>24` 로 길이 없었다.
- **수정 1**: `BotNavBuild.RopeReachBelow = 2.6`(표면→로프 y0 허용). 영향 그래프는 같은 로프를 쓰는 4맵(DeepForest·NearForest·SideRoad·BrokenRampart)뿐이고 재굽기 후 44→24 도달 가능(구간 51 만 도달 불가). 15.8 의 NoPath 막힘(450·817)은 계단 x≈-385 라 별개 원인(미해결).
- **수정 2(홈 복귀)**: `HuntBrain.Wander` — 적이 안 보인 지 6초 넘고 마지막 지상 전투 자리에서 6 이상 멀면 그래프 길찾기로 그 자리로 돌아간다(2 안이면 도착, 히스테리시스). 몹 없는 구덩이·서쪽 끝에 갇혀 처치가 멈추던 막힘(11-17 런 3~5: 600 에서 정지)을 푼다. 첫 구현은 6 한 값이라 경계에서 떨었다.
- **수정 3(사냥 퀘스트 선택)**: `BotQuestRules.PickHunt(q, night)` — 첫 목표가 찬(정예만 남은) 퀘스트·낮의 밤 전용(허수아비) 퀘스트를 뒤로 민다. 11-16 의 완료 0 원인: Q106 이 정예 쥐 두목(비정예 두목은 안 센다) 때문에 영원히 Active 라 PickHunt 가 계속 Q106 을 골라 Q104·Q105 를 사냥하지 않았다.
- **수정 4(스포너 찾아가기)**: `HuntBrain.SeekQuestMob` + `BotQuest.TryFindSpawn` — 목표 몹이 10초 안 보이면 그 몹을 내는 스포너(스캔 때 위치 저장) 쪽으로 걷는다. 이끼껍질 딱정벌레 스포너(Mole3)는 SideRoad 안쪽이라 토끼·쥐만 잡던 문제.
- **30분 런(Lv1·300G·퀘스트 없음·마을 시작, 세션 20261117_173003)**: 수락 3(Q106·Q104·Q105)·**완료 2**(Q104·Q106, 퀘스트 보고 2)·Lv6·처치 428(14/분)·사망 0·귀환 업무 중단 1(`nav=36>24`)·막힘 13·세이브 원복 md5 일치·콘솔 error 0·EditMode 1432 통과. 같은 코드 이전 런들: 수정 전 600 에서 정지(런 3~5), 홈 복귀만 넣은 런 7 은 700 처치.
- **미해결**: ①Q105 5/5(t=818) 보고를 못 했다 — 보고 job 이 `phase=Go` 150초 시간 초과 2회(몹이 곁에 있으면 양보 + 맵 순회·귀환이 앞서 감). ②귀환 중단 1·`맵 이동 경로 없음 SideRoad→NormalForest` 반복(`nav=28>40`·`36>24`, 구간 간선 금지 만료 추정) ③정예 쥐 두목(Q106 둘째 목표)은 이번엔 센 듯하나 확인 못 함 ④구덩이 로프 오르기·포털은 이번 런에서 직접 안 탔다(그래프 연결만 확인, 11-17 런 8 는 구덩이 진입 없음) ⑤Lv4 이상 일반 숲 전환은 런 8 에서 실제로 일어났다(t=1370 SideRoad→NormalForest, 퀘스트 없음 때가 아니라 Q105 완료 뒤).
- 리뷰 권장 미반영: 밤 전용 목록 하드코딩(`Scarecrow_BD`)·점수 상수화·seek 도달 타임아웃.
