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
| `Game/GameOverview.md` | 게임 개요, 핵심 루프, 문서 지도 |
| `Game/CombatSpec.md` | 이동·입력·스탯·**데미지 공식**·콤보·스킬·버프 |
| `Game/ItemSpec.md` | ItemSheet/SetItem CSV 스키마, 슬롯, **세트 규칙**, 인벤토리·제작·상점·세이브 |
| `Game/EnemySpec.md` | 몬스터 스탯, BT 구조, 스폰, **드롭 테이블** |
| `Game/WorldSpec.md` | 낮/밤, 맵·포털, 카메라 |
| `Game/AssetInventory.md` | 보유 에셋스토어 86종 분류와 도입 현황 |
| `Game/TechDebt.md` | 버그·기술 부채 백로그 (P0~P3, 수정 시 갱신) |

## 운영

| 파일 | 설명 |
|------|------|
| `Docs/OpenQuestions.md` *(저장소 루트)* | 사용자 확인이 필요한 기획 질문과 임시 가정 |
