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
