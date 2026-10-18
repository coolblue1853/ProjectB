# 자동 실행 로그

예약 작업 `projectb-auto-improve`가 실행될 때마다 한 줄씩 추가한다.
주간 사용량 %는 `get_usage`의 "Weekly · all models" 값이다. 리셋은 **매주 일요일 21:00 (KST)** 이다.

## 예산 규칙

- 일요일 21:00 리셋 이후 평일(월~금)에 ProjectB가 쓸 수 있는 양은 **주간 사용량의 10%** 다.
- 하루 상한은 약 **2%**. 실행 1회 상한은 약 **0.6%**(초과가 예상되면 작업을 쪼갠다).
- 주말에는 자동 실행하지 않는다(Q-201 답변 전까지의 임시 가정).
- 아래 표에서 이번 주 `증가분` 합계가 10%에 도달하면 그 주에는 더 실행하지 않는다.
- `증가분`은 같은 시간대에 다른 작업이 쓴 양도 포함하므로 보수적으로 잡힌 값이다.

## 기록

| 날짜 시각 | 시작 % | 종료 % | 증가분 | 작업 | 커밋 |
|---|---|---|---|---|---|
| 2026-10-18 (수동 세션) | 69 | – | – | Unity 6 업그레이드, 문서 체계, B-001/B-002 등 | 4795fc21..b48b2ab0 |

### 2026-10-18 (토) 자율 작업 세션
- 09:00~21:00 무제한 예산 사용(사용자 허용). 커밋 90여 개: Unity 6 업그레이드, P0~P1 버그, 타격감/FX(도트 베이커), Lv2, 강화·정예·보스 보상·퀘스트·도감·아티팩트·층계 지도·상승 부하 등. 상세는 `.claude-rules/Game/QualityRoadmap.md` 현황 요약.
- 21:00 리셋 이후 규칙: 평일 주간 사용량 10% 이내. 세션 모델은 사용자가 메뉴에서 변경(세션 자체 전환 불가).

### 2026-10-18 (일) 20:00~21:00 Opus 5.5 이어서
- 커밋: 59214408(인벤토리 Phase 2 SaveMapper·ViewScanner), 92aefed4(에셋 도입 계획 W0~W5), a6b4eb9e·80c43582·644eab18(Phase 3a: InventoryService, AddSource, 로드 경로·차감 저장), 936404a2(B-115 타일맵 틈 1차 조치).
- 에셋 다운로드(사용자 허락, 캐시 `%APPDATA%/Unity/Asset Store-5.x`, 미임포트): I2 Localization 2MB, Smart Lighting 2D 17MB, SPUM 4MB, Corgi Engine 301MB, 8-Bit Sfx 11MB, Fantasy Music Suite 2 79MB, Free Fantasy Adventure Music 75MB, Free Frost Mage SFX 30MB, SoundBits 599MB, Pro Sound Collection 2.0GB, Horror Bundle SFX 2.4GB, Ultimate Game Music Collection 4.5GB, Game Music Treasury 4.1GB, SAGA Complete Collection(진행 중), Ultimate SFX & Music Bundle(확인 필요). "2D Pixel - RPG Monster Pack"은 구매 목록에 없음(보유: RPG Monsters Icon, Monsters_Creatures_Fantasy).
- 무기 공격·콤보 전수 감사 백그라운드 에이전트 → `.claude-rules/Game/WeaponReview.md`.
- 21:00 리셋 이후: 평일만, 주간 10% 이내. 일요일 밤은 추가 작업 없음.
- 21:00 이후(사용자 지시로 계속): PC 재시작 후 Unity·MCP 재기동. 커밋 2715aa69(대화 시스템 W1·SPUM NPC), 4d39c706(W-07 판정 구간), 63f368a0(구역 BGM·덕킹·사운드 카탈로그), 61f66d46(플레이어 피격·회복 숫자), 02a09395(보스 등장 카메라). 주간 사용량 1%(22:05).

### 2026-10-19 (월) 00:00~ 밤샘 자율 작업 (사용자: "자고 올 테니 묻지 말고 계속")
- 커밋: ac151c5b(②구역 일반 숲·코드 도트 몹 4종), e4a1d461(SPUM 제거·장로 NPC 도트), b42dd5d8(무기 P2), 98ea5b85·76ec3ef5·8dda3545·26dc0d4d(계보 무기 Fang Dagger·Tusk Knuckles·Wisp Lantern·Shell Shield), c3b5f554(B-116), 4eb30c5d(계보 세트 Pack Hunter·Night Lantern).
- ChatGPT 로그인 없음 → 몹·아이콘은 `Tools/pixelfx` 코드 도트로 생성(사용자 지시 '도트 필수' 준수).
- 주간 사용량 3%(00:58).

### 2026-10-19 (월) 1층계 정리·회귀 검증 — 전 구역 낮·밤 소크
- 도구: `bash Tools/zone_soak.sh 15` — 맵마다 시간대를 맞추고 `MapInfo.GoToMap` → 그 시간대 일반 스포너 중 슬롯이 가장 많은 곳의 스폰 지점으로 이동(스포너는 플레이어가 영역 안에 있어야 스폰) → 게임 시간 15초 → 활성 몹 수·콘솔 에러/경고(아틀라스 경고 제외)·스크린샷(`Logs/zone_soak/`, 커밋 안 함). 생존은 `keep_hp`, 세이브 백업·복원(종료 시 trap, md5 일치 확인).
- 1·2차 실행에서 나온 문제와 수정: 밤 전환이 낮으로 튐(B-135) · 샛길 밤 몹 공격 트윈 오류 86건(B-136) · 일반 숲 밤에 PassiveTerror 등장(B-137) · 토끼 누락 스크립트 경고 18건(B-138) · 늪지 낮 Animator 경고 197건(에디터 메모리 참조 깨짐, 재임포트, B-139) · DOTween 용량 경고(B-140) · 깊은 숲 밤 파괴된 transform 트윈(B-136).
- 최종(3차, 수정 반영 후) 결과 — **에러 0, 경고 0, 감시기 이상 0**:

| 맵 | 낮: 위치 / 몹(보스) | 밤: 위치 / 몹(보스) | 에러 | 경고 |
|---|---|---|---|---|
| Town | 입구 / 0 | 입구 / 0 | 0·0 | 0·0 |
| Lv1_NearForest_SideRoad | Rabbit / 19 | CreepingTerror2 / 15 (1, PassiveTerror) | 0·0 | 0·0 |
| Lv1_NormalForest | Flowermole2 / 7 | CreepingTerror2 / 14 (0) | 0·0 | 0·0 |
| Lv1_DeepForest | Goblin1 / 11 (1, 고대숲의 엔트) | Fairy2 / 12 | 0·0 | 0·0 |
| Lv1_BanditFortress | Cutthroat1 / 17 (1, 산적 대장) | Torch4 / 5 | 0·0 | 0·0 |
| Lv1_Swamp | Frog1 / 6 | Slime1 / 5 (1, 거대 슬라임) | 0·0 | 0·0 |
| Lv1_TowerOutskirts | GolemSmall1 / 17 (1, 탑의 문지기) | RuinSpirit2 / 9 | 0·0 | 0·0 |
| Lv2_MagicCliffs | L2_B_Goblin / 6 | L2_B_SkelN / 4 | 0·0 | 0·0 |

- 같은 날: Q-414 지름길 봉인, B-130 맵 끝 벽(플레이 측정 7맵 양끝), B-132 테스트 무적 감시 정리, EditMode 테스트 92/92 통과.
