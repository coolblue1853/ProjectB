# Project B 규칙 인덱스

- 모든 대화·문서는 **한글**로 쓴다.
- 이 파일은 **인덱스 전용**이다. 항목마다 "어느 문서를 읽을지" 판단할 한 줄만 적고, 항목은 **백틱**으로 적는다(링크 금지).
- 문서는 `Framework/`(범용 규약) 또는 `Game/`(게임 고유)에 둔다. 한 문서는 500줄 이내를 권장한다.

## Project Overview

**Project B** — 메이플식 횡스크롤 2D 액션 + 던파·디아식 장비 세트 파밍 RPG (개인 프로젝트).

- **Unity:** 6000.4.0f1 / URP 2D / PC(키보드·게임패드)
- **데이터:** 로컬 CSV(`Assets/Resources`) + Easy Save 3. 서버 없음.
- **에디터 자동화:** unity-mcp(`Tools/u.sh`, `Tools/playtest.sh`, `Tools/probes/*.cs`)

## 프레임워크 규약

| 파일 | 설명 |
|------|------|
| `Framework/ContextEconomy.md` | 컨텍스트 절약 — 읽기 범위 절제 *(상시 로드)* |
| `Framework/CodeStyle.md` | 코드 스타일, 커밋 규칙, Unity 규약 *(상시 로드)* |
| `Framework/EditorAutomation.md` | unity-mcp로 컴파일·플레이·스크린샷·프로브 실행, 포커스 금지 원칙 |

## 게임 기획 / 스펙

| 파일 | 설명 |
|------|------|
| `Game/TowerWorld.md` | **TO THE BEYOND** 세계관·스토리(브로치·아버지)·7층계 테마·1층계 구역(얕은 숲/요정여왕/깊은 숲/골렘)·기획 용어↔구현 매핑(형체 코어·층계 결정·힘의 편린·아티팩트·브로치 해방) |
| `Game/GameOverview.md` | 게임 개요, 핵심 루프, 문서 지도 |
| `Game/CombatSpec.md` | 이동·입력·스탯·**데미지 공식**·콤보·스킬·버프 |
| `Game/ItemSpec.md` | ItemSheet/SetItem CSV 스키마, 슬롯, **세트 규칙**, 인벤토리·제작·상점·세이브 |
| `Game/EnemySpec.md` | 몬스터 스탯, BT 구조, 스폰, **드롭 테이블** |
| `Game/WorldSpec.md` | 낮/밤, 맵·포털, 카메라 |
| `Game/AssetInventory.md` | 보유 에셋스토어 86종 분류와 도입 현황 |
| `Game/AssetIntegrationPlan.md` | 사용자 지정 필수 에셋(대화·I2·Feel·ProCamera 등) 실사용 계획, 평가 대상(Smart Lighting·SPUM·RPG Monster·Corgi), 사운드 카탈로그, 웨이브 W0~W5 |
| `Game/TechDebt.md` | 버그·기술 부채 백로그 (P0~P3, 수정 시 갱신) |
| `Game/QualityRoadmap.md` | **게임 방향(무기 스킬 + 세트 강화)** 과 퀄리티업 로드맵 A~F |
| `Game/GameFeelPlan.md` | 타격감(A2~A6) 구현 계획: `HitFeedback` 파사드, All In 1 셰이더 키워드, 히트스톱·흔들림·숫자 수치, 임포트 목록, 검증 |
| `Game/InventoryRefactorDesign.md` | 인벤토리 리팩터 설계(D2~D6): 현 구조·잔존 버그, InventoryModel/세트 평가기/세이브 매핑, 툴팁·정렬·픽업 로그, 단계별 이행·테스트 |
| `Game/WeaponReview.md` | 무기 평타·콤보·스킬 감사: 무기 18종 흐름표, 문제 W-01~36(파일:줄), 핵앤슬 제안, 수정 순서 |
| `Game/PlayerStateDesign.md` | 플레이어 FSM·상태 토큰(무적/슈퍼아머/이동정지)·입력 버퍼·Modifier 스탯 재설계와 단계별 마이그레이션 |
| `Game/ProgressionDesign.md` | 성장 루프(메이플식 진행·디아식 콘텐츠): 정예(중간보스) 승격·보스 확정 드롭·고유템 천장·강화 +0~+10 표와 보너스, 단계 P1~P4 |
| `Game/Lv2ContentPlan.md` | Lv2 "마법 절벽" 기획(E4): 신규 에셋 프레임·PPU 분석, 맵 레이아웃·스포너·포털, 몬스터 5+엘리트+보스 스탯·BT·드롭, Cliff 세트(ItemSheet/SetItem 행·스킬·레시피), 빌더 단계별 구현·프로브 |

## 운영

| 파일 | 설명 |
|------|------|
| `Docs/OpenQuestions.md` *(저장소 루트)* | 사용자 확인이 필요한 기획 질문과 임시 가정 |
| `Docs/AutomationLog.md` *(저장소 루트)* | 예약 자동 실행 기록과 사용량 예산 규칙 |
