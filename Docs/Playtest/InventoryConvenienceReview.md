# 인벤토리 편의 보완 검증 (2026-11-02)

## 결과·조작

- 가방 L / 패드 LT+R3: 스택 단위 잠금·해제. 판매/분해/일괄 판매 보호, 창고·정렬·분할·장착·강화로 잠금 유지.
- 상점 J / LT+L3: 잠금 해제 Misc의 개수·총액·제작/의뢰 재료 포함 안내, 8초 안에 같은 키로 확인1회. 장비/소비품/진행 필수품 제외. 기존 되사기12건 재사용.
- 새 획득 점: 첫 페이지를 떠나거나 창을 닫을 때 해제. 직접 장착도 열람 처리. 기존 스택 합치기는 새 표시 OR.
- 정렬: 종류→등급→티어→강화→부위→이름. 요구 레벨 필드가 없어 기존 티어를 사용.
- Shift/LT 비교: 실제 교체 시 누적 세트 단계의 +획득/-해제 효과. 기존 제작 핀·펫 없는 2.2u 자동 자석 유지. Z는 대시 그대로.

## 검증

- EditMode 전체 470/470, 실패0·스킵0(기존460 + 신규10). 최종 job/시간은 아래 최종 확인에 기록.
- `invconv_runtime.cs` 25/25: ES3 추가 플래그3과 기존 강화3 동시 저장, 잠금 판매/분해 실행 거절, 재료 분할 잠금8개 보존, T2/+3 잠금 검 창고 왕복, 양쪽 디스크 플래그, 일괄 판매 확인 전 무변경, Wood4×200G=800G 지급·잠금Wood8 보존·소비품5/장비2 제외, 되사기 골드/수량 복원, 확인 중 잠금 시 판매 거절, 제작 핀12/15, 줍기2.2/4.5 경계.
- `invconv_pickup.cs` 6/6: 실제 15/15 가방에서 잠금Wood3은 추가 여유false, 실제 Player 콜라이더로 접촉 핸들러 호출 시 바닥 드롭 활성·수량3 보존, 잠금 해제 뒤 여유true, 실제 DropItemCheck.Update가 거리2.19u에서 자석 활성, 접촉 성공 뒤 Wood4·드롭 비활성(풀 반환1회). 자연 이동 입력 완주는 아니며 실제 런타임 경로의 직접 호출 검사다.
- 가상 Input System 3/3: L 잠금true→false, LT+R3 false→true·커서 같은 슬롯(정렬 미발생), LT+L3 확인 후 10,000→10,800G·Wood12→8. J의 첫 확인 화면도 캡처했다. 물리 장치 검증은 아니다.
- 추가 실행: WolfCap+WolfArmor 착용 후 StoneCap 비교에서 WolfArmor2 해제(이동속도8/치명8/피해3) 표시·장비 실교체 없음, 장착 잠금 ES3 equipmentFlags[7]=1, 가방 닫은 후 새 표시0. HUD 제작핀 Wood8/15 표시.
- 컴파일·플레이 종료 콘솔 오류0, `git diff --check` 통과. 처음 신규 파일이 아직 임포트되지 않은 컴파일 오류와 프로브 준비 상태/기존 핀 토글 때문에 실패한 실행은 최종 수치에서 제외했다.

## 낮 화면 직접 검토

`Logs/playtest/invconv/bag-day.png`, `bulk-confirm-day.png`, `compare-day.png`, `tracker-day.png`.

- 새 잠금L·초록점은 수량/아이콘과 구분된다. 판매 확인 개수·총액·재료 포함 안내가 창 위 전용 공간에 보이며 잘림 없음.
- 비교 패널의 하단 HUD 중첩과 미지원 Unicode minus 글리프를 발견해 보정했다. 최종은 하단 여유100 기준·ASCII -로 전체 효과3줄 표시, 점프/방향/이동 변화 없음.
- **남은 기존 화면 부채:** 상단 치트 UI가 원래 대형 장비 상세 이름을 가리고 긴 세트 설명이 큰 상세창에서 줄바꿈/경계 잘림을 보인다. 퀘스트 추적창은 가방 빈 슬롯 위에 겹친다. 장비 교체 후 기존 슬롯 아이콘 크기가 작은 경로도 보인다. 새 비교 패널 자체의 잘림은 해소했으며 기존 전체 상세 레이아웃 재작성은 하지 않았다.

## 파일 목록

신규(각 .meta 포함):
- `Assets/Script/Inventory/Model/ItemFlagSave.cs`
- `Assets/Script/Inventory/Model/BulkSaleRules.cs`
- `Assets/Script/UI/InventoryConvenience.cs`
- `Assets/Script/UI/ItemStatusBadge.cs`
- `Assets/Script/UI/SetSwapPreview.cs`
- `Assets/Editor/Tests/InventoryConvenienceTests.cs`
- 이 검증 문서 `Docs/Playtest/InventoryConvenienceReview.md`(.meta 없음)

수정:
- `Assets/Script/DB/ItemDatabase.cs`, `SaveManager.cs`
- `Assets/Script/Inventory/InventoryService.cs`, `ViewScanner.cs`
- `Assets/Script/Inventory/Model/ItemStack.cs`, `SlotContainer.cs`, `SaveMapper.cs`, `ItemSortComparer.cs`
- `Assets/Script/Item/DismantleService.cs`
- `Assets/Script/Enemy/DropItemCheck.cs`
- `Assets/Script/UI/Chest.cs`, `ChestConvenience.cs`, `ComparePanel.cs`, `DismantleInput.cs`, `EquipBoxCheck.cs`, `InventoryManager.cs`, `ItemCheck.cs`
- `.claude-rules/Game/BasicFeatures.md`, `InventoryRefactorDesign.md`, `ItemSpec.md`, `TechDebt.md`
- `Docs/OpenQuestions.md`, `Docs/Playtest/FirstHourGuide.md`

선행 창고 변경을 보존하고 필요한 플래그 연결만 추가했다. 다른 작업의 씬·폰트·서드파티·렌더링·모션·아트 변경은 본 작업 범위가 아니다. 공유 작업 트리의 선행 변경은 별도로 확인할 것. 커밋·push 없음.

## 미검증·한계

- 물리 키보드/패드, 마우스 드래그 hit-test, 빌드, 실제 강제 종료·정전, 장시간 경제/전투 소크, 모든 해상도.
- 일괄 판매는 제작/의뢰 재료를 자동 보존하지 않는다(확인 안내+잠금). 되사기는 기존 최근12스택까지. 잠금은 소비/제작/의뢰/강화 재료 사용을 막지 않는다.
- ES3 Load/Save와 레거시 배열 매핑을 검사했으며 실제 강제 종료를 통한 원자성 검사는 하지 않았다.
- 한 번에 제작핀1개, 창고 그림/기존 마우스 비활성 상태 유지. 스프라이트·모션·이펙트 제작 없음.

## 종료·세이브·일회성 파일

- 시작 백업 `Logs/save-backup-invconv-20261102-023830/`의 ES3 3개를 플레이 idle에서 복원. MD5 SaveFile=`F1CBD3EDF7E845CACE5442F6641C5B2F`, SaveSlot2=`4683BD8B4FFE564E86535BDF7861504B`, **SaveSlot3=`F1A9AC9E7D4BE9716FB6ED03A0BD2C4F`**. 일치 증거 `Logs/playtest/invconv/save-restore.json`.
- 입력 설정(배경동작2/에디터입력2) 복구·가상 키보드/패드 제거. 선택 슬롯3 유지.
- **커밋 제외:** 모든 `Tools/probes/invconv_*`(C#/shell), `Logs/` 전체(일회성 Python·결과·캡처·세이브 백업 포함). 기존 `chest_resume_*` 등 다른 일회성 프로브도 이번 기능 커밋에 넣지 않는다.

## 최종 확인

- 안내 문구 캐시까지 반영한 최종 전체 EditMode **470/470**(실패0·스킵0, 7.3679332초), job `12f69f1400f640c589361d6be2b143ab`.
- 최종 캐시 반영 뒤 플레이31/31·가상 입력3/3 재확인, 낮 가방/판매 확인/세트 비교/추적 HUD 캡처 재검토. 종료 idle·입력 설정 복원·ES3 3/3 원본 MD5 일치 재복원. 코드/구조 리뷰 체크리스트로 null·이벤트·풀 반환·저장 호환·매 프레임 문자열 할당을 점검했다.
