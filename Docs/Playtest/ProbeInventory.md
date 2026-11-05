# 프로브 정리 목록 (AUTO-112, 11-05)

미추적 `Tools/probes/_*.cs` 170개 전수(`git ls-files --others --exclude-standard Tools/probes | grep '/_'`). 대부분 `execute_code` 일회용 스니펫이다. 이 문서는 **목록만**이며 삭제·이동은 하지 않았다. 판정은 파일명·첫 줄·문서 언급(`Docs`·`.claude-rules`·`Tools` grep -w) 기준 추정이고 사용자 확인 전 삭제 금지.

- 문서에 파일명이 직접 나온 것: `_b`·`_am`(오탐: 단어 일치), `_dash0`(queue.md), `_f1_flow`·`_f1_goto`(queue.md), `_f1c2_pace`(ClassicMapleDesign), `_f1nav_go`·`_f1nav_portal`(Floor1NavReview), `_f1r_esc`(TechDebt), `_pt_use`(`pt_run.sh` 호출). 나머지는 결과가 로그(`Logs/`)나 대화에만 있어 기록 문서 없음.
- 별도: 비`_` 미추적 프로브 273개(`firsthour_*`·`p3_*`·`r3_*`·`chest_*` 등)는 이 목록 범위 밖(후속 정리 후보).
- 판정: **유지**=재사용 가치(여러 문서·스크립트가 참조 또는 범용 상태 점검), **통합**=변형 묶음, 대표 1개만 남기고 나머지는 삭제 후보, **삭제**=일회성.

## 1. 묶음별 표

| 묶음(파일) | 용도 1줄 | 결과 기록 문서 | 판정 |
|---|---|---|---|
| `_am` | 활성 맵·근처 포털 나열 | 없음 | 통합(`_f1nav2_mapinfos`로) |
| `_b` | 현상금 가용/진행 수 | 없음 | 삭제 후보 |
| `_c` | ControlsWindow 토글 | 없음 | 삭제 후보 |
| `_day` | 낮/밤 상태 반환 | `Floor1HuntingAudit.md` 등 | 유지(1줄 범용) |
| `_night` | 밤으로 전환 | 없음 | 유지(`_day`와 한 쌍) |
| `_m` | 1줄 임시 | 없음 | 삭제 후보 |
| `_trace` | 1줄 임시 | 없음 | 삭제 후보 |
| `_tree`, `_tree2` | 씬 트랜스폼 트리 덤프 | 없음 | 통합(`_tree` 유지) |
| `_intr` | 포털 암전 중 웨이포인트 인터럽트 확인 | 없음 | 삭제 후보 |
| `_dash0`~`_dash8` | 여우 돌진(TempMobDash) 검증 9단계(스폰·밤 전환·타이밍) | `queue.md`(`_dash0`) | 통합: `_dash0` 유지, 1~8 삭제 후보 |
| `_dashcap` | 여우 돌진 연속 캡처 | 없음 | 유지(캡처 템플릿) |
| `_foxp` | 여우 CycleCheck 스폰 검증 | 없음 | 삭제 후보 |
| `_dnf_air`, `_dnf_cancel`, `_dnf_cap`, `_dnf_chain`, `_dnf_hud`, `_dnf_hud2`, `_dnf_hud3`, `_dnf_q`, `_dnf_spawn`, `_dnf_wpn` | 전투 조작감(CF) 공중평타·캔슬·콤보·HUD 확인 | `CombatFeelPlan.md`(간접) | 통합: `_dnf_chain`·`_dnf_cancel` 유지, 나머지 삭제 후보 |
| `_dnf_go` | 낮으로 맞추고 이동 | 없음 | 통합(`_day`로) |
| `_dnf_keyA` | 가상 키 입력(Input System) | 없음 | 유지(입력 주입 템플릿) |
| `_f1_caches`, `_f1_free`, `_f1_inv`, `_f1_kiosk`, `_f1_maps`, `_f1_near`, `_f1_tokiosk`, `_f1_tre`, `_f1_up` | 1층계 캐시·인벤·키오스크·맵 단발 조회 | 없음 | 삭제 후보(`_f1_maps` 유지) |
| `_f1_flow`, `_f1_goto` | 1층계 흐름·맵 이동 | `queue.md` | 유지 |
| `_f1c2_boss`, `_f1c2_raid`, `_f1c2_raidstate` | 보스·습격 상태 점검 | 없음 | 통합(`_f1c2_raid` 유지) |
| `_f1c2_bounty`, `_f1c2_bounty2` | 현상금 흐름 | 없음 | 통합(`_f1c2_bounty2` 유지) |
| `_f1c2_chestkey`, `_f1c2_chests` | 상자·열쇠 | 없음 | 통합(`_f1c2_chests` 유지) |
| `_f1c2_natural`, `_f1c2_rare`, `_f1c2_t01`, `_f1c2_tonight` | 드롭/희귀/자연 스폰 단발 | 없음 | 삭제 후보 |
| `_f1c2_pace` | 레벨 도달 경험치 페이스 | `ClassicMapleDesign.md` | 유지 |
| `_f1c2_portals`, `_f1c2_road`, `_f1c2_roadtrip`, `_f1c2_reach`, `_f1c2_scan`, `_f1c2_shot` | 포털·샛길·도달성·스캔·캡처 | 없음 | 통합(`_f1c2_reach`·`_f1c2_scan` 유지) |
| `_f1nav_go`, `_f1nav_portal` | 1층계 내비 검증 1차 | `Floor1NavReview.md` | 유지 |
| `_f1nav2_walk`, `_f1nav2_reach`, `_f1nav2_portalwalk`, `_f1nav2_fall`, `_f1nav2_pit`, `_f1nav2_wall`, `_f1nav2_burst`, `_f1nav2_profile`(+`_run`), `_f1nav2_ascii`(+`_run`), `_f1nav2_goto`(+`_run`), `_f1nav2_dry`, `_f1nav2_mapinfos`, `_f1nav2_sanity`, `_f1nav2_ui`, `_f1nav2_where` | 1층계 실보행·낙사·벽·도달성 내비 2차 | `Floor1NavReview.md`(1차) | 통합: `_f1nav2_walk`·`_f1nav2_reach`·`_f1nav2_where` 유지, `_run` 래퍼는 삭제 후보 |
| `_f1nm_dbg`, `_f1nm_goto`(+`_core`·`_mist`), `_f1nm_mobs`, `_f1nm_ray`, `_f1nm_shot`·`shotc`·`shotm`, `_f1nm_state`, `_f1nm_tp`·`tp1`·`tpx`, `_f1nm_trip`·`trip1~4` | 일반 숲 이동·몹·포털 왕복·캡처 | 없음 | 통합: `_f1nm_state`·`_f1nm_mobs`·`_f1nm_trip` 유지 |
| `_f1r_esc` | 입력 설정 복제로 Esc 동작 확인 | `TechDebt.md` | 유지 |
| `_fd`, `_fd2`, `_fd3`, `_fd4` | 맵 이동/포털 페이드 단일 캡처 | 없음 | 통합(`_fd3` 유지) |
| `_portalcap` | 샛길 포털 Use 경로 연속 캡처 | 없음 | 유지 |
| `_g` | 활공 검증 드라이버(short/hold/updraft) | 없음 | 유지 |
| `_ground`, `_ground2`, `_ground3` | 자원 좌표 지면·포털 거리 | 없음 | 통합(`_ground3` 유지) |
| `_res`, `_res2`, `_rescap`, `_rescap2` | 자원(나무·광석) 스폰·생존 캡처 | 없음 | 통합(`_res2`·`_rescap2` 유지) |
| `_t`, `_t2`~`_t10` | 자원/이동/페이드 번호 임시 | 없음 | 삭제 후보(`_t6`·`_t8` 유지 검토) |
| `_pt_die`, `_pt_goto`, `_pt_list`, `_pt_use` | PtBot 사망·이동·목록·사용 | `pt_run.sh`, `PtBot.md` | 유지 |
| `_pv2t` | NpcQuestWindow 탭 전환 | 없음 | 삭제 후보 |
| `_pv7_dbg`, `_pv7_go` | 1줄 임시 | 없음 | 삭제 후보 |
| `_pv9_a`, `_pv9_b`, `_pv9_bg`, `_pv9_en`, `_pv9_nui`, `_pv9_nui2`, `_pv9_sites`, `_pv9_trk` | 둥지 사냥 UI·추적 조회 | `FloorCoreContent.md`(간접) | 통합(`_pv9_nui` 유지) |
| `_pv9_ndrive`, `_pv9_nsetup` | 둥지 추적 검증 준비·구동 | `FloorCoreContent.md` | 유지 |
| `_uf2_bars`, `_uf2_c`, `_uf2_cap_l`, `_uf2_cap_run`, `_uf2_l`, `_uf2_l2`, `_uf2_n`, `_uf2_q` | UI 2차: 상태 바·퀘스트·캡처 | 없음 | 통합(`_uf2_bars`·`_uf2_cap_run` 유지) |
| `_uf_cap`, `_uf_cap_run`, `_uf_close_inv`, `_uf_open_inv`, `_uf_day`, `_uf_lock_on`, `_uf_lock_run`, `_uf_setup`, `_uf_state`, `_uf_t`, `_uf_unl` | UI 1차: 인벤 토글·UiLock·캡처·상태 | 없음 | 통합(`_uf_cap_run`·`_uf_state` 유지) |
| `_uf_qm`, `_uf_qm2`~`_uf_qm4`, `_uf_qmadd`, `_uf_qt` | 퀘스트 HUD(QuestMachine) 배치 조사 | 없음 | 통합(`_uf_qm4` 유지) |
| `_uf_shopcap` | 상점 창 캡처 | 없음 | 유지 |
| `_uf_sr`, `_uf_sr2`, `_uf_sr3` | SRDebugger 고정 UI 루트 조사 | 없음 | 삭제 후보 |

## 2. 집계(추정)

- 유지: 약 30개. 통합 후 삭제 후보: 약 90개. 삭제 후보(일회성): 약 50개.
- 효과: 대표 파일만 남기면 약 60~70개 수준으로 줄어든다. 실행은 사용자 확인 후 별도 큐로(이번엔 파일 삭제·이동 없음).

## 3. 사용법 메모

- `Tools/playtest.sh [대기초] [이름]`: 씬 로드→콘솔 비움→Play→대기→스크린샷→콘솔 요약→Stop 한 사이클.
- `_run`·`_cap_run` 접미 파일은 같은 이름 본체를 감싼 래퍼이므로 본체와 함께 두거나 함께 지운다.
- `__FROM__`·`MODE`·`DELAY` 같은 자리표시자가 있는 파일(`_f1nm_trip*`, `_fd3`, `_uf_lock_on`)은 호출 스크립트가 치환해야 동작한다.
