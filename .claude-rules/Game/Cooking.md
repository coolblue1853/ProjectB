# 요리 · 일괄 제작 (REWORK-11)

## 1. 현재 구조 진단 (11-11)
- 요리 = `CookingStove`(MiniGameSpot 상속) 위의 미니게임 1판. 흐름: ↑ 시작 → Select(좌우로 가진 재료로 가능한 레시피 고르기) → X(패드 West) 시작(재료 소모) → 굽기(12초·뒤집기 3회) / 끓이기(18초·화력 유지) → 결과 등급(완벽 2·좋음/보통 1·실패=탄 요리 1) → `GiveReward`.
- 레시피 6종은 `CookingRules.Recipes`(코드 상수). CSV 아님(`SelfCraftRecipes.csv` 는 대장장이 소모품 제작이고 별개).
- 화로 2곳(숲 샛길·늪)뿐, 마을엔 없었음. 화로 쿨타임 90초. 1회 1개라 재료를 쌓아 두어도 일괄 처리 수단이 없었음.
- 입력 갭: 선택 단계가 키·패드 전용(마우스 없음). ↑ 가 NPC·포털과 겹쳐도 중재 없음.

## 2. 설계 결정
| 항목 | 결정 | 이유 |
|---|---|---|
| 마을 요리대 | `Town` x=-406(루엔 -402.5·세나 -409.5 사이), 지면 스냅. 같은 `CookingStove`(`_kitchen`=true). 쿨타임 0 | 야영 화로 90초 쿨은 모험 중 제한용. 마을은 일괄 요리가 대신 |
| 일괄 수량 | 선택 단계에서 X → 가능 횟수 ≥2 면 공용 `QuantityPopup`(1~max), 1이면 바로 시작. max = min(재료 보유/필요, **10**) | 메이플 제작 수량 선택과 동일 |
| 진행 시간 | 일괄이어도 **미니게임 1판**(굽기 12초/끓이기 18초). 진행 바는 기존 HUD, 제목에 `x N` 표시. 건너뛰기 없음 | 수량이 시간 이득이 되어 일괄의 동기가 됨 |
| 등급 | 한 판의 등급을 N개 전부에 적용: 완벽 2N · 좋음/보통 N · 실패 N개의 탄 요리 | 실패 위험이 커져 숙련 보상. 임시(Q-CK1) |
| 중단(ESC) | 시작 뒤 중단이면 N회분 재료 손실(기존 규칙 확장) | |
| 가방 부족 | 들어가는 만큼만 지급하고 "가방 가득" 알림 1회, 못 받은 개수 표시 | 결과 전량 소실 방지 |
| 퀘스트 | 제작 메시지 `Craft` 를 N번 보냄(제작 목표 N개 진행) | |
| 대장장이 일괄 제작 | `CraftingManager.Craft()` 가 쌓이는(maxStack>1) 결과물이고 2회분 이상이면 같은 `QuantityPopup`(상한 99, 재료·골드 한도). 장비(maxStack 1)는 기존 1개 제작 | 같은 UI 패턴 통일 |
| ↑ 중재 | `MiniGameSpot.UpdateIdle` 이 `InteractArbiter.Contend` 를 쓴다(모든 미니게임 지점 공통) | NPC 와 겹쳐도 가까운 하나만 |
| 입력 3경로 | 키: ←→ 고르기·X 시작·수량 창 ↑↓←→숫자·Z. 패드: 십자·West·A. 마우스: 선택 단계 ◀ ▶ [요리] 버튼 + 수량 창 버튼·휠 | |
| 그림 | 임시: 화로 도트 재사용. 요리대 전용 픽셀은 PixelLab 방 몫 | |

## 3. 코드 지도
- 순수 규칙: `BatchRules.MaxCount`(재료·골드·상한) · `CookingRules.MaxCount/ResultCount(grade,batch)/PlaySeconds/KitchenCooldown` · `CookingSession.Count`.
- 배치: `CookingStovePlacer`(Town 요리대). 화면: `CookingUI`(버튼·x N·최대 수). 일괄 제작: `CraftingManager.CraftCount`.
- 테스트: `BatchRulesTests`(11건). ES3 키·CSV 변경 없음.

## 4. 후속
- 레시피 CSV 화 완료(11-11): `Resources/CookingRecipes.csv`(id,method,rarity,ingredients=`재료*개수;…`) → `CookingRecipeCsv.Load/Parse` → `CookingRules.Recipes`. 파일이 없거나 유효 줄 0 이면 코드 `DefaultRecipes` 폴백, 잘못된 줄은 경고 후 건너뜀. 테스트 `CookingRecipeCsvTests`.
- 생활 탭 트리(V.3b) 연동·요리대 전용 도트·요리 버프 시간 연계는 미구현.
- 실마우스·실패드 입력은 미검증(probe 는 API 호출).
- 검증 완료(11-14, play probe): 완벽 등급 일괄(3회분 → 구운 토끼 6개, 손실 0), 대장장이 쌓이는 제작물 2회분 이상 → `QuantityPopup` 열림·확정 → 지급. 콘솔 에러 0. 코드·데이터 추가 작업 없음. 남음: 실마우스·실패드, 요리대 도트, 생활 탭 연동.
