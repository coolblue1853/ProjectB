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
| `Game/TowerWorld.md` | **TO THE BEYOND** 세계관·스토리(브로치·아버지)·7층계 테마·1층계 구역(얕은 숲/요정여왕/깊은 숲/골렘)·기획 용어↔구현 매핑(형체 코어·층계 결정·힘의 편린·아티팩트·브로치 해방), 주인공 도트 리디자인 방향 |
| `Game/LineageGear.md` | 몬스터 계보 장비 설계안: ④늪지·⑤산적 성채·⑥탑 근교 몹→계보 무기·세트, 공통 규칙, 필요한 신규 시스템(LifeSteal 등) |
| `Game/GameOverview.md` | 게임 개요, 핵심 루프, 문서 지도 |
| `Game/CombatSpec.md` | 이동·입력·스탯·**데미지 공식**·콤보·스킬·버프 |
| `Game/ItemSpec.md` | ItemSheet/SetItem CSV 스키마, 슬롯, **세트 규칙**, 인벤토리·제작·상점·세이브 |
| `Game/EnemySpec.md` | 몬스터 스탯, BT 구조, 스폰, **드롭 테이블** |
| `Game/WorldSpec.md` | 낮/밤, 맵·포털, 카메라 |
| `Game/ArtStyle.md` | **도트 아트 스타일 가이드**(사용자 레퍼런스: 인디 액션풍 저해상도, 채도 낮은 바탕+포인트 색, 부분 외곽선), 핀터레스트→PixelLab/GPT→격자 복원 파이프라인 |
| `Game/CharacterPixelRules.md` | **캐릭터 도트 생성 규칙**(주인공·NPC): **§0 확정 레시피(원본 편집으로 컨셉 교체·세대별 진행·합격 문구 모음)**, 2등신 데포르메 앵커·색감, 판정 지표(IDEAL/OK), PixelLab Pro Flash+style_image 파이프라인, 방어구 레이어 대비, 실패 사례·이력. 스킬 `.claude/skills/pixel-character/` 가 실행 |
| `Game/AssetInventory.md` | 보유 에셋스토어 86종 분류와 도입 현황 |
| `Game/AssetIntegrationPlan.md` | 사용자 지정 필수 에셋(대화·I2·Feel·ProCamera 등) 실사용 계획, 평가 대상(Smart Lighting·SPUM·RPG Monster·Corgi), 사운드 카탈로그, 웨이브 W0~W5 |
| `Game/TechDebt.md` | 버그·기술 부채 백로그 (P0~P3, 수정 시 갱신) |
| `Game/QualityRoadmap.md` | **게임 방향(무기 스킬 + 세트 강화)** 과 퀄리티업 로드맵 A~F |
| `Game/MotionSet.md` | **주인공 마스터 모션 목록(던파식 고정 31개)**: 스컬 머리별 공격 형태 → 모션 계열, 이동 12 + 공격 19 표(있음/없음), 방어구 비용, 스컬 문법 이펙트 목록(PixelLab용) |
| `Game/JobCoverage.md` | 직업 200종(근접·원거리·마법·기술·변신) × 마스터 모션 38개 대응표, 결론(✔191 △9 ✖0, 전신 변신은 전용 스프라이트) |
| `Game/SkulReference.md` | **전투 모션·이펙트 레퍼런스 = 게임 스컬**: 스컬 전투 표현 문법(평타 리듬·전진·초승달 베기·히트스톱·스파크·파편), 우리 평타와 차이, 적용 순서 |
| `Game/MotionPipeline.md` | **모션 제작 절차(사용자 확정)**: 레퍼런스 몸 전체 → 관절 읽기 → 졸라맨 3칸 GIF(레퍼런스·지금·수정안)로 승인 → 고정 길이 스켈레톤 보간 → PixelLab 1회, 자세 체크리스트(오다리 금지·반대손 가드·벰 vs 내려침) |
| `Game/MotionRootCause.md` | 공격 모션이 이상하게 나오는 원인 검토(뒤로 물러남·투구 출렁임·완급·콤보 연결 등 수치 근거)와 생성 0 수정안, 추가 성공 기준 |
| `Game/LimbusReference.md` | **이펙트·동작 레퍼런스 = 림버스 컴퍼니(10-28)**: 참고 전용 규칙(저장소 밖 `LimbusRef`), 어드레서블 캐시 위치·Unity 버전, 캐릭터 구조(Spine 대기 + 스프라이트 리그 스킬), 우리 에디터에서 번들 재생·캡처 방법, 관찰 |
| `Game/MotionCoverage.md` | **무기 174 × 직업 309 엄격 재판정(10-29)**: 무기별 기본 동작·직업별 판정(✔/기술/✖), 지금 30개 → 72%, 줄·사슬 그리기·후보 동작(악기·줄 휘두르기·춤·중화기·총기 보류) 단계별 효과, 최소 모션 곡선. 생성 `Tools/motioncov/coverage.py` |
| `Game/SkulMotionMap.md` | 마스터 모션 38 × 스컬 참고 애니 대응표(묶음/애니/프레임 수), 로프는 스컬에 없음 |
| `Game/SkulAssetLibrary.md` | **스컬 원본 추출 라이브러리(참고 전용, 저장소 밖 `SkulRef/lib`)**: 사용 규칙, 분류 체계(캐릭터 머리·적·보스 / 이펙트 22종 / 배경 / UI / 아이템), 머리 124벌 프레임 수 통계, 리틀 본 평타 프레임 분석, 작업 기준 |
| `Game/GameFeelPlan.md` | 타격감(A2~A6) 구현 계획: `HitFeedback` 파사드, All In 1 셰이더 키워드, 히트스톱·흔들림·숫자 수치, 임포트 목록, 검증 |
| `Game/InventoryRefactorDesign.md` | 인벤토리 리팩터 설계(D2~D6): 현 구조·잔존 버그, InventoryModel/세트 평가기/세이브 매핑, 툴팁·정렬·픽업 로그, 단계별 이행·테스트 |
| `Game/WeaponReview.md` | 무기 평타·콤보·스킬 감사: 무기 18종 흐름표, 문제 W-01~36(파일:줄), 핵앤슬 제안, 수정 순서 |
| `Game/PixelRigDesign.md` | **새 주인공 픽셀 리그**(셀 대신 Unity 리깅 + 픽셀 격자): 부위 분할·뼈 계층·PPU 36 크기, 클립 키프레임 표(팔·칼날 각도), 기존 애니 이름 매핑, `CharacterRig` 창구(AnyPortrait 래퍼/픽셀 리그), 검증 프로브 |
| `Game/PlayerStateDesign.md` | 플레이어 FSM·상태 토큰(무적/슈퍼아머/이동정지)·입력 버퍼·Modifier 스탯 재설계와 단계별 마이그레이션 |
| `Game/ProgressionDesign.md` | 성장 루프(메이플식 진행·디아식 콘텐츠): 정예(중간보스) 승격·보스 확정 드롭·고유템 천장·강화 +0~+10 표와 보너스, 단계 P1~P4 |
| `Game/Lv2ContentPlan.md` | Lv2 "마법 절벽" 기획(E4): 신규 에셋 프레임·PPU 분석, 맵 레이아웃·스포너·포털, 몬스터 5+엘리트+보스 스탯·BT·드롭, Cliff 세트(ItemSheet/SetItem 행·스킬·레시피), 빌더 단계별 구현·프로브 |
| `Game/CelEquipmentPlan.md` | **셀 애니 × 장비 외형** 계획: 던파식 부위 레이어 방향, 페이퍼돌·앵커·팔레트 스왑·PixelLab transfer_outfit 조사, A/B/C안과 생성량 추정 |
| `Game/Floor2Plan.md` | **2층계 Undercloud** 설계: 구역 5개(절벽·무너진 성벽·석상 정원·기사단 성채·용의 둥지) 낮/밤 몹·보스, 상승 부하 25~60, 계보 무기·세트 표, 탐험가 칭호 2, **수치 연속성 구역 단계 T 공식**(Q-409 정리안) |

## 운영

| 파일 | 설명 |
|------|------|
| `Docs/OpenQuestions.md` *(저장소 루트)* | 사용자 확인이 필요한 기획 질문과 임시 가정 |
| `Docs/AutomationLog.md` *(저장소 루트)* | 예약 자동 실행 기록과 사용량 예산 규칙 |
