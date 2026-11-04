# MAP-SCAN 맵 정적 스캔 (11-04, map-physics)

도구: `bash Tools/mapscan.sh [Run|Dbg]` (에디터가 꺼져 있을 때 배치모드, 켜져 있으면 메뉴 Tools/Dev/Map Scan) → `Logs/mapscan.txt`. 코드 `Assets/Editor/Dev/MapScan.cs`. 씬은 저장하지 않는다.
방식: 맵을 하나씩만 켜고 콜라이더·스폰점·포털 도착점을 물리 쿼리. 합성 콜라이더가 Outlines 형이라 OverlapPoint 는 못 쓰고 위쪽 레이 교차 홀짝으로 "지형 속" 판정.

## 결과 (1층계 전 맵)
- Missing 스크립트/참조: **0**. 경계 밖 오브젝트: **0**. 포털 도착점이 몹 근접/벽: 아래 항목 외 없음.
- 스폰점이 지형 속(표 `SpawnPointsFloor1.csv`): SideRoad·NormalForest·BanditFortress Stone `-711:-1.4`(지면 -0.6), DeepForest·TowerOutskirts Tree `-336:-1.4`(지면 0.4) → **수정함**(`-711:-0.2`, `-336:0.8`). 재스캔 0건.
- 씬 좌표 문제(11-04 SampleScene 수정 완료):
  - `Portal_ToNormalForest`/`Portal_ToSideRoad` y -0.41 → 5.52 (런타임 `AlignForestEntrance` 값과 동일, 충돌 없음).
  - 서쪽 고원 Breakable Props(Crate/Barrel, 4개 맵 복제 × 5 = 20개) y 20.4 → 22.4 (지면 윗면). 재스캔에서 Breakable 경고 사라짐.
  - SideRoad 계열 스포너 `Flowermole2`/`CreepingTerror2`/`Cutthroat4`/`Torch4`/`Knight4`/`Wisp4` 오브젝트(-598.4,15.6)는 지형 속이나 실제 스폰점은 `enemyPositon` 자식/표를 쓰므로 무해.
- 포털 `Lv1_NearForest/Portal/PotalLv1_NearForest2` destination 없음: 비활성(activeSelf=false)이고 어느 포털도 참조 안 함 → 미사용, 그대로 둠. (사용 중인 `Lv1_NearForest_SideRoad/Portal/PotalLv1_NearForest2` 는 destination 연결됨.)
- 동적 Rigidbody2D(gravity>0) 오브젝트: 마을 `Rock`(layer Enemy, gravity 3, 지면 위 2.8)만. 포털 31개는 gravity 0 이라 무해. 낙하물은 이 Rock 과 드롭 아이템(DropManager)뿐.
- 도달성(점프 단일 2.6 + 사다리): 1층계 일반 맵(NearForest 등) 발판은 전부 사다리로 도달 가능. 서쪽 샛길 단차 1칸 계단은 탭 점프(1.43)로 통과 가능.
- **Swamp/MistSwamp/DeepForestCore(마법 절벽 복제 지형)**: 2·3단 발판(Platform_8_*, Platform_12_95)은 사다리 바닥이 1단 발판 높이(-176.3)라 지면(-180)에서 단일 점프+몸 겹침으로만 잡힘. 1단 발판은 지면에서 4.0 위(더블점프 전제 설계, 1층계는 더블점프 미해금) → 사다리 없이 점프로는 불가. **권고(미수정)**: 사다리 바닥을 1단 발판 윗면 근처로 내리거나 지면(-180)까지 연장.
- BanditFortress·BrokenRampart 서쪽 절반(x<-515)은 포털이 없어 걸어서 못 가는 고립 지형(시드 포털 없음). **권고(미수정)**: 서쪽 구역 진입 포털 추가 또는 구역 제거/차단 중 택일(기획 결정).

## 원인 정리 (큐 항목)
- 계단 점프 불가: 게임 물리는 정상. 점프 높이 풀 2.87 / 탭 1.43(`JumpReach`, EditMode 테스트). 1칸 계단은 통과, 4칸 이상 발판은 사다리 전제(1층계 더블점프 없음). 봇(`ptbot_camp.cs`)에는 `ptbot_run.cs` 의 가변 점프 홀드(0.35초)와 사다리 로직이 없어 4칸 발판·구덩이 계단에서 막힘 → 봇 한계. ptbot_camp.cs 는 사용자 미커밋 변경이라 미수정.
- 떨어지는 오브젝트: 정적 스캔 범위에서 지형 속 스폰점 5건(수정)·묻힌 소품 5개(씬 보고)가 낙하/튕김 후보. 사용자 증상 원문은 OpenQuestions 에 없어 확정 못 함 → 플레이 확인 필요.

## 추가 점검 (11-04 map-physics 2차, 에디터 실측 — 위 "원인 정리" 정정)
도구: 도달 판정 점프를 실측 1.5 로 교정하고 계단 단차 검사(`StepTapFail` 0.95~1.62 / `StepTooHigh` 1.62 초과)를 추가했다.

**정정:** 위 "풀 2.87 / 탭 1.43" 은 스크립트 기본값 `jumpForce=13` 기준이다. 씬에 직렬화된 값은 **10**(2024-03 커밋)이라 실제 점프는 훨씬 낮다. `JumpReach`·`JumpReachTests` 도 13 을 가정해 실제와 다르다.

| 입력 (프로브 `_mp_jump.cs`, gravityScale 3) | 상승 높이 |
|---|---|
| 풀홀드 0.35초 이상 | 1.62 |
| 탭 0.1초 | 1.01 |
| 탭 0.05초 | 0.92 |

- **계단 점프**: 계단 지형은 막혀 있지 않다(프로브 `_mp_stair.cs`: SideRoad 계단을 탭·홀드 모두 통과, 35초에 x -745→-640). 그러나 1층계 단차는 대부분 1.00 이고 탭 높이가 0.92~1.01 이라 한계다. 봇(`ptbot_run.cs`)은 점프 키를 1틱(0.1초)만 눌러 탭 높이로 뛰어 자주 걸렸다 → **봇이 점프 키를 0.35초 유지하도록 수정**. 사람이 풀홀드하면 문제없음. 게임 쪽 수치(JumpCutMul 0.5→0.7 이면 탭 1.27)는 OpenQuestions Q-MP1 로 올림.
- **StepTapFail**(+1.00): 일반 숲·샛길·깊은 숲·탑 근교 등 맵마다 10~16곳. **StepTooHigh**: SideRoad 계열 x -636(+22)·-582(+7) — 사다리·발판 체인 구간.
- **떨어지는 오브젝트(수정함)**: `DropItemCheck.OnTriggerEnter2D` 가 Ground/Wall 트리거에 닿기만 해도 안착(중력 0)으로 처리해, 단차 옆면에 닿으면 허공에 멈췄다(재현: 일반 숲 x -268.3, 늪 x -356.8). 발밑에 비트리거 Ground 콜라이더가 있을 때만 안착하고 `OnTriggerStay2D` 에서도 재시도하게 고쳤다. 재현 프로브 `Tools/probes/_mp_drop.cs`(구역마다 약 20개 투하): 수정 전 허공 정지 3건 → 수정 후 0건(남은 2건은 발판 끝에서 아래 발판으로 떨어진 정상 낙하).
- 도달 재스캔(점프 1.5): 늪 계열 `Platform_8_*`·`12_95` 는 위 "도달성" 항목대로 더블 점프 전제(변동 없음). 그 외 새 이상 없음.
