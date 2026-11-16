# 주말 1 발견 사항 (11-15 사용자 플레이 39분 + 11-15~16 봇 실행)

출처: `Docs/Handoff/playtest_1115.md`(사용자 기록·피드백), `.claude-rules/Game/TechDebt.md` 11-15·11-16 봇 절, `Docs/Balance/PlayBalance_1115.md`, `Docs/Balance/BotBalance_1116.md`, 봇 보고서 `Logs/playrec_bot/*_report.md`.
사용자 피드백 7건(스태미너·기록 오류·막힘 판정·옛 장비·드롭률·몹 밸런스·밤 몹)은 11-15 에 처리 완료(`playtest_1115.md` 1~6 [x]). 아래는 남은 것만.

## 막힘
1. SideRoad x≈-386~-401, y 0.3~1.9 계단·로프0(seg24↔25) 구간에서 봇이 막힌다(분당 0.4~0.55). 원인 후보: 위 발판 몹 접근 중 공중 공격 분기, 로프 위·아래 점프·낙하 반복. 11-16 수정(eec28cf89) 후에도 개선 확인 안 됨. 사람도 막히는 지형인지 미확인 → 막힘 직전 tk 행을 간선 종류별로 집계해 지형 문제/봇 문제를 가른다.

## 버그
2. 가방이 가득 차면 드롭 줍기가 안내 없이 거절된다(사람은 왜 안 주워지는지 모름). 메이플식 "가방이 가득 찼습니다" 안내 필요.
3. 가방 열린 채 숫자 1~5 가 가방 페이지 넘기기와 퀵슬롯 사용을 동시에 한다.
4. 상점 H(소비품 충전)가 물약뿐 아니라 응급식량·귀환 주문서까지 전부 채워 한 번에 약 2만 골드를 쓴다. 충전 대상·금액 확인창 필요.
5. 옛 무기 프리팹의 `Weapon._kind` 가 비어 있다(봇은 `WeaponMasteryService.KindOf` 로 보충). 교체 순간 None 으로 보이는 경우도 있음 → 프리팹 값을 채우거나 런타임 보충을 게임 쪽에 둔다.
6. 콘솔 error: EditMode 뒤 "Destroy may not be called from edit mode"(BurnStack 정리), Floor2Data 헤더 열 순서 error. 원인 미확인.
7. (미검증) 폐기 장비를 낀 옛 세이브 로드 → 가방 반환/골드 환산 경로를 플레이로 확인 못 함.

## 튜닝 (사용자 확인 후 CSV 반영)
8. `BotBalance_1116.md` 수정안: RatChief_BD contactDamage 19→15, StoneRabbit_BD 21→17, CreepingTerror_BD 21→16(표본 1), ForestSnail_BD maxHP 130→100, LeftMole_BD maxHP 24→18.
9. 밤 배율은 방향만 확인(같은 맵 받은피해/분 1.75배). 같은 몹 낮/밤 1.3배 실측은 아직(낮 Rabbit·밤 RatThief 로 몹이 달랐다).
10. Ghost count×0.6·StoneRabbit EXP14 실측 → 11-16 2회차 봇 실행 중(`BotBalance_1116.md` 2회차 절).
11. NormalForest 분당 EXP 가 봇 기준 사람의 0.4~0.5배. 동선 차이인지 밸런스인지 미판정.

## 그래픽
- 새 의뢰 없음(NormalForest 의 X 상자는 정식 `Crate` 그림이었다).
