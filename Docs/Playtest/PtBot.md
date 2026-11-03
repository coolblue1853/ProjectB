# PT-BOT 플레이테스트 봇

에디터 플레이 모드에서 1층계를 자동으로 플레이하고 보고서를 만드는 봇. PV1 항목 검증용. 새 시스템이 아니라 기존 프로브(`_f1nav2_*` 구역 이동, `firsthour_talks` 대화, `f1fr_boss` 키 전투, `tp_spawner`, `soak.sh` 세이브 백업)를 묶은 것이다.

## 사용
```bash
bash Tools/ptbot.sh restore-orig             # 최초 원본 세이브 복원
bash Tools/ptbot.sh [key=value ...]          # 예: zones=Lv1_NearForest_SideRoad,Lv1_NormalForest zoneSec=120 timescale=3 quests=1
bash Tools/u.sh cs Tools/probes/ptbot_status.cs   # 진행 확인
bash Tools/u.sh cs Tools/probes/ptbot_stop.cs     # 중간 중지(다음 틱에 요약 작성)
```
- 옵션: `zones`(콤마, 구역=맵 오브젝트 이름) `zoneSec`(구역당 게임초, 120) `totalRealSec`(실시간 상한, 280) `timescale`(0.5~4, 기본 2, 코드가 4로 제한) `quests`(1=구역 입장 시 수락/완료 가능 NPC 대화) `boss`(1=보스·미니보스·정예 우선 도전, 0이면 보스/미니보스 무시) `god`(1=무적) `night`(1=밤) `givePotions`(5, 가방이 가득이면 잡템 한 칸 비움) `weapon`(이름|none) `lv`(시작 레벨 하한) `logEvery`(5) `stuckSec`(6) `potionPct`(0.45).
- 세이브 백업: 최초 원본 `Logs/pt_save/orig/`(있으면 덮어쓰지 않음) + 실행별 `Logs/pt_save/run_YYYYmmdd_HHMMSS/`. 종료 시 **이번 실행 시작 사본**으로 복원. 최초 원본으로 되돌리려면 `bash Tools/ptbot.sh restore-orig`. 점검 기록 `Docs/Playtest/SaveSlotCheck.md`.
- 자동 처리: 세이브 전부 백업 → 플레이 → 낮 고정 → 가상 키 입력(백그라운드 허용) → 봇 → 정지 → 세이브 복원. 입력 설정 복구가 필요하면 `input_bg_off.cs`.
- 산출: `Logs/ptbot/report.md`(보고서), `run.log`(원본), `summary.txt`, `console_errors.json`. 창 포커스는 건드리지 않는다. 최소화 시 게임 시간이 느리니 totalRealSec 를 넉넉히.

## 봇 동작
구역 이동(`MapInfo.GoToMap`, 몹이 없으면 활성 스포너 영역으로 걷고 8초 넘게 없으면 순간이동) → 가장 가까운 몹(Y 차 2배 가중) 접근 → 사거리 안에서 `Weapon.MeleeAttack`(콤보)+1.3초마다 스킬 순환, 몹이 위면 점프(C) → 피격(HP 감소) 시 1초 점프 회피 → HP < potionPct 이면 물약(Small→Medium→Large, 1초 쿨) → 사망 시 부활 후 구역 재입장.

## 보고서 형식 (`report.md`)
1. 설정 한 줄.
2. 요약: 종료 사유(`all-zones-done`/`stop-file`/`real-time-limit`), 실시간·게임시간, 최종 상태, 총계(처치·정예·보스·사망·막힘·물약·대화·회피·콘솔 에러).
3. 구역별 표: 게임초, 처치/정예/보스, 첫 처치 시각, 사망, 막힘, 물약, 대화, 레벨·EXP·골드 변화.
4. 이벤트(최대 40줄): `DEATH` `STUCK`(위치 불변 stuckSec 초, 교전 중 제외; 2단계에서 `UNSTUCK_TP`) `WAYPOINT_TP` `NO_POTION` `CONSOLE_ERROR` `TALK_*` `SKILL_ERR`.
5. 콘솔 에러: 봇이 `logMessageReceived` 로 잡은 Error/Exception(중복 제거) + 종료 시점 에디터 콘솔 에러 집계.

## 스모크 결과 (11-03)
숲 구역(Lv1_NearForest_SideRoad) 게임 100초(실 33초, ×3): 처치 20, 사망 0, 콘솔 에러 0. 물약 사용·퀘스트 NPC 대화(마을 3명, EXP +800) 확인.

## 남은 것 (PV1 전체 검증은 다음 작업)
- 보스·정예·미니보스 도전은 `boss=1` 로직만 있고 미검증(보스는 점프/후퇴 패턴 없음, f1fr_boss 의 공격 애니 후퇴 미이식).
- 몹 공격 탐지 점프 회피는 HP 감소 반응형(선제 회피 아님). 위층 발판·함정 길찾기 없음(막히면 순간이동으로 대체).
- 포털 이동 걷기 없음(구역은 GoToMap 순간이동). 퀘스트는 수락/완료 대화만, 사냥 목표 선택 없음.
- CF-A~D, L7 몹, 퀘스트 타입, 레시피 UI 전용 체크는 없음.
