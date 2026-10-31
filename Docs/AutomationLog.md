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

- 10-25 밤(사용자 취침, "멈추지 말고 계속"): 주간 사용량 상한 10%. 시작 시점 2%. 작업 사이마다 get_usage 확인, 9% 근처에서 정리·기록 후 중단.

- 10-26 새벽(사용자 취침 중 계속): 주인공 걷기·달리기 재작업(포즈 그림 교체)부터 타격·베기·찌르기 이펙트 v2, 발소리 동기화까지 커밋 약 35개. 주간 사용량 2%→3%.

- 10-26 오전(사용자 부재): 셀 방식 이동·점프 완성(측면 통일, 대시·피격·사망 셀), 던파식 장비 레이어 실험. 주간 사용량 7% — 상한 10% 앞에서 장비 구조 결정(Q-425) 대기로 멈춤. PixelLab 이번 셀 작업 누계 28회분(남은 1789).

- 10-26 낮: 던파식 장비 레이어 구현(철 투구·흉갑 셀 전체, 부위 분리 생성 0, 인벤토리 동기화). 주간 사용량 8% — 상한 10% 앞이라 정리하고 멈춤. PixelLab 누계 이번 셀·장비 작업 73회분(남은 1749 추정).

## 10-31 Codex 분담 (사용자 결정)

- Claude 주간 90%에서 11-01 20:00까지 버티려고 구현·검증을 Codex CLI(ChatGPT 로그인, 로컬, gpt-6.1-sol)로 넘긴다.
- Claude: 지시서 작성(짧게) → `bash Tools/codex/run.sh <이름> <지시서> [강도]` 백그라운드 → `Logs/codex/<이름>.last.md` 보고만 읽고 diff 요약 확인 → 커밋·push.
- 추론 강도 기본 auto: usage.sh 가 남은 양·리셋 시점으로 고른다(리셋 6시간 안·30% 이상 남음 → max, 여유 → xhigh, 페이스 초과 → medium, 90% 이상 → low).
- 샌드박스: workspace-write + 네트워크 + 세이브 폴더. 샌드박스 전체 해제는 쓰지 않는다.
- Codex 규칙은 저장소 루트 `AGENTS.md`(커밋 금지·제외 파일·검증 절차). 남은 사용량 `bash Tools/codex/usage.sh`.
- Unity 는 하나라 Codex 구현 작업은 한 번에 하나씩.
- **언제 쓰나(사용자 10-31):** Claude 주간 사용량이 많이 찼거나(85%↑) 사용자가 정해 준 하루 한도에 다가오면 이 방식으로 바꾼다. 평소에도 Codex 여유가 크면 우선 사용.
- **리뷰도 Codex:** 구현 실행 뒤 `bash Tools/codex/review.sh <이름>` 이 별도 Codex 실행으로 리뷰·품질 확인·[필수] 수정. 묶음 실행은 `bash Logs/codex/queue.sh <이름…>`(작업→리뷰 반복).
- Codex 리셋이 가까우면(6시간 안, 30%↑ 남음) 강도 max 로 몰아 쓴다(usage.sh 자동).
- **실측(10-31 review1):** 리뷰+수정 1회 = Codex 5시간 창 19%·주간 2%, 약 30분. 품질은 sonnet builder 이상(놓친 실제 버그 10건 발견·수정). 샌드박스 안 u.sh·테스트·플레이 프로브·스크린샷 모두 동작.
- Claude 는 큐를 길게 묶어 백그라운드로 돌리고 끝날 때만 깨어난다(대기 중 짧은 폴링 금지).
- **한도 대체(사용자 10-31):** Codex 5시간 85%↑ 또는 주간 95%↑ 이면 run.sh 가 75 로 끝나고 큐가 멈춘다(`LIMIT`). 그동안 Claude 가 직접 작업하고, 한도가 풀리면(5시간 리셋) 다시 Codex 큐로 넘긴다.
- **강도 기본 medium**(사용자 10-31: 1회 5시간 19% 는 높다). 5시간 창 30% 미만·주간 여유 2배면 high, 리셋 6시간 안·30%↑ 남으면 max.
- **크레딧 주의(10-31):** 이 계정은 팀 워크스페이스라 사용률(5시간·주간)과 별개로 **멤버 크레딧**이 있다. 하루 약 10회(250만~300만 토큰)에 `workspace_member_credits_depleted` 로 소진됐다. 프로브를 수십 번 도는 실측·플레이 점검이 가장 비싸다. run.sh 는 'out of credits' 도 LIMIT 로 본다. 충전은 워크스페이스 관리자만 가능.
