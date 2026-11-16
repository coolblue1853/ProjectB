# 주말 1 발견 사항 (11-15 사용자 플레이 39분 + 11-15~16 봇 실행)

출처: `Docs/Handoff/playtest_1115.md`(사용자 기록·피드백), `.claude-rules/Game/TechDebt.md` 11-15·11-16 봇 절, `Docs/Balance/PlayBalance_1115.md`, `Docs/Balance/BotBalance_1116.md`, 봇 보고서 `Logs/playrec_bot/*_report.md`.
사용자 피드백 7건(스태미너·기록 오류·막힘 판정·옛 장비·드롭률·몹 밸런스·밤 몹)은 11-15 에 처리 완료(`playtest_1115.md` 1~6 [x]). 아래는 남은 것만.

## 막힘
1. [x] (f79fea024, 진단·게임 쪽 1건 수정. 봇 막힘율은 그대로 → 후속 2건) SideRoad x≈-386~-401 계단·로프0 막힘. 진단(11-16): 사람은 같은 구간을 4~15초·점프 1~2회에 통과, 봇도 대부분 5~7초 → 지형은 통과 가능. 막힘 3종: ① 로프 밑에서 ↑ 가 안 먹음 = 게임 버그(공격 잠금 중 로프 트리거 진입 → isLadder Enter 놓침, 수정) ② 로프를 목표 발판 높이에서 못 내리고 끝까지 올라갔다 떨어져 다시 잡음 ③ 계단 위 몹(쥐 도둑) 접근 중 공격·회피 충돌. ②③ 봇 수정 3종(RopeLoose 같은높이 하차·RopeStep 발판 높이 하차·둘 병행)은 15분 막힘 9~15건으로 효과 없어 되돌림. 후속: 막힘 직전 간선 종류를 기록에 남겨(현재 tk 에 없음) 재조사.
   - 진척(11-16): 봇 tk 행에 `be`(간선 종류)·`bn`(목표 구간)·`bf`(Focus) 추가, `Tools/playrec_stuck_tk.py --edges` 로 막힘 직전 간선별 집계. 밤 15분(161911) 막힘 25건은 전부 SideRoad x≈-378~-399 — 집계 수치는 PlayBotDesign 15.5.

## 버그
2. [x] (e42c75923) 가방이 가득 차면 드롭 줍기가 안내 없이 거절된다(사람은 왜 안 주워지는지 모름). 메이플식 "가방이 가득 찼습니다" 안내 필요.
3. [x] (7fb5d5803, 실플레이 재현 안 됨·규칙 통합) 가방 열린 채 숫자 1~5 가 가방 페이지 넘기기와 퀵슬롯 사용을 동시에 한다.
4. [x] (f40a8451d) 상점 H(소비품 충전)가 물약뿐 아니라 응급식량·귀환 주문서까지 전부 채워 한 번에 약 2만 골드를 쓴다. 충전 대상·금액 확인창 필요.
5. [x] (c1fb27590) 옛 무기 프리팹의 `Weapon._kind` 가 비어 있다(봇은 `WeaponMasteryService.KindOf` 로 보충). 교체 순간 None 으로 보이는 경우도 있음 → 프리팹 값을 채우거나 런타임 보충을 게임 쪽에 둔다. 처리: 프리팹 53개 중 45개 비어 있었고(전부 ItemSheet WeaponType 있음) `Weapon.Kind` 가 `WeaponMasteryService.KindOf` 로 보충(DB 미로드면 None·캐시 안 함). 플레이로 PhantomScythe=Scythe 확인. 호출부 보충 코드 정리.
6. [x] (aed570f3e) 콘솔 error: EditMode 뒤 "Destroy may not be called from edit mode"(BurnStack 정리), Floor2Data 헤더 열 순서 error. 원인: Destroy = DotEffectBase 가 EditMode 에서 Destroy 호출(isPlaying 분기로 해결), 헤더 error = Floor2DataTests 가 일부러 넣은 b,a 머리글(실데이터 헤더는 일치, 시험은 logReject=false). EditMode 1421 통과·콘솔 error 0.
7. [x] 폐기 장비를 낀 옛 세이브 로드 → 플레이 프로브로 확인(코드 수정 없음): 가방 여유 시 가방 반환·칸 비움, 가방 가득이면 판매값/2 골드(Ring 250, HighArmor 2500, 추가슬롯 Shoes 150). 무기 칸이 비면 기본 무기 자동 장착(EnsureDefaultWeapon). 세이브 원복 md5 일치. 상점 H 문구 LocUi 이동은 588fd656e.

## 튜닝 (사용자 확인 후 CSV 반영)
8. `BotBalance_1116.md` 수정안: RatChief_BD contactDamage 19→15, StoneRabbit_BD 21→17, CreepingTerror_BD 21→16(표본 1), ForestSnail_BD maxHP 130→100, LeftMole_BD maxHP 24→18.
9. 밤 배율은 방향만 확인(같은 맵 받은피해/분 1.75배). 같은 몹 낮/밤 1.3배 실측은 아직(낮 Rabbit·밤 RatThief 로 몹이 달랐다).
10. Ghost 밤 EXP/분 1327→98~198 로 내려옴(n=8 저신뢰, dda411c32). Ghost EXP/처치 22.3 = CSV 16×1.39 로 밤 배율만으로 설명 안 됨(원인 미확인). 수정안 Ghost_BD contactDamage 18→14 는 "밤 몹은 더 세야" 요구와 충돌하므로 보류 권장. StoneRabbit 은 봇이 못 만나 미관측.
11. NormalForest 분당 EXP 가 봇 기준 사람의 0.4~0.5배. 동선 차이인지 밸런스인지 미판정.

## 그래픽
- 새 의뢰 없음(NormalForest 의 X 상자는 정식 `Crate` 그림이었다).
5. 밤(ns=1)에 SideRoad 몹이 다 사라지면 봇이 x≈-437 왼쪽 낭떠러지로 걸어 떨어졌다 복귀(`낙사 복귀`)를 8분 넘게 반복한다(Wander 가 구덩이 앞에서 못 돈다). 세션 143651·121217·123001·141245 에서 확인. 수정(11-16): 원인 = 구덩이는 바닥이 없어 `DropF=-1` 인데 Wander 가 `DropF>2` 만 낭떠러지로 봤다 → `DropF<0` 도 돌아선다. 밤 15분(161911)에서 `낙사 복귀` 0회(전 424회). 단 그 실행은 몹이 남아 있어 Wander 비중이 작았다.
