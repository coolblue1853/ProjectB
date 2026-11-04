# MAP-SCAN 맵 정적 스캔 (11-04, map-physics)

도구: `bash Tools/mapscan.sh [Run|Dbg]` (에디터가 꺼져 있을 때 배치모드, 켜져 있으면 메뉴 Tools/Dev/Map Scan) → `Logs/mapscan.txt`. 코드 `Assets/Editor/Dev/MapScan.cs`. 씬은 저장하지 않는다.
방식: 맵을 하나씩만 켜고 콜라이더·스폰점·포털 도착점을 물리 쿼리. 합성 콜라이더가 Outlines 형이라 OverlapPoint 는 못 쓰고 위쪽 레이 교차 홀짝으로 "지형 속" 판정.

## 결과 (1층계 전 맵)
- Missing 스크립트/참조: **0**. 경계 밖 오브젝트: **0**. 포털 도착점이 몹 근접/벽: 아래 항목 외 없음.
- 스폰점이 지형 속(표 `SpawnPointsFloor1.csv`): SideRoad·NormalForest·BanditFortress Stone `-711:-1.4`(지면 -0.6), DeepForest·TowerOutskirts Tree `-336:-1.4`(지면 0.4) → **수정함**(`-711:-0.2`, `-336:0.8`). 재스캔 0건.
- 씬 좌표 문제(코드 보정 있어 현재 증상 없음, 씬 수정은 SampleScene 이 총괄 관리라 보고만):
  - `Portal_ToNormalForest`/`Portal_ToSideRoad` y=-0.41 이 지면(4.4) 아래 4.8 묻힘 → `Portal.AlignForestEntrance` 가 런타임에 y=5.52 로 보정.
  - 서쪽 고원 Breakable Props(Crate/Barrel 5개, x -551~-515, y 20.4)가 지면 윗면(22.4)보다 2 아래 묻힘 → 씬 수정 필요(y +2). SideRoad·NormalForest·BanditFortress·BrokenRampart 공통(복제 지형).
  - SideRoad 계열 스포너 `Flowermole2`/`CreepingTerror2`/`Cutthroat4`/`Torch4`/`Knight4`/`Wisp4` 오브젝트(-598.4,15.6)는 지형 속이나 실제 스폰점은 `enemyPositon` 자식/표를 쓰므로 무해.
- 포털 `PotalLv1_NearForest2`(Lv1_NearForest) destination 없음(미사용 포털로 추정, 확인 필요).
- 동적 Rigidbody2D(gravity>0) 오브젝트: 마을 `Rock`(layer Enemy, gravity 3, 지면 위 2.8)만. 포털 31개는 gravity 0 이라 무해. 낙하물은 이 Rock 과 드롭 아이템(DropManager)뿐.
- 도달성(점프 단일 2.6 + 사다리): 1층계 일반 맵(NearForest 등) 발판은 전부 사다리로 도달 가능. 서쪽 샛길 단차 1칸 계단은 탭 점프(1.43)로 통과 가능.
- **Swamp/MistSwamp/DeepForestCore(마법 절벽 복제 지형)**: 2·3단 발판(Platform_8_*, Platform_12_95)은 사다리 바닥이 1단 발판 높이(-176.3)라 지면(-180)에서 단일 점프+몸 겹침으로만 잡힘. 1단 발판은 지면에서 4.0 위(더블점프 전제 설계, 1층계는 더블점프 미해금) → 사다리 없이 점프로는 불가.
- BanditFortress·BrokenRampart 서쪽 절반(x<-515)은 포털이 없어 걸어서 못 가는 고립 지형(시드 포털 없음).

## 원인 정리 (큐 항목)
- 계단 점프 불가: 게임 물리는 정상. 점프 높이 풀 2.87 / 탭 1.43(`JumpReach`, EditMode 테스트). 1칸 계단은 통과, 4칸 이상 발판은 사다리 전제(1층계 더블점프 없음). 봇(`ptbot_camp.cs`)에는 `ptbot_run.cs` 의 가변 점프 홀드(0.35초)와 사다리 로직이 없어 4칸 발판·구덩이 계단에서 막힘 → 봇 한계. ptbot_camp.cs 는 사용자 미커밋 변경이라 미수정.
- 떨어지는 오브젝트: 정적 스캔 범위에서 지형 속 스폰점 5건(수정)·묻힌 소품 5개(씬 보고)가 낙하/튕김 후보. 사용자 증상 원문은 OpenQuestions 에 없어 확정 못 함 → 플레이 확인 필요.
