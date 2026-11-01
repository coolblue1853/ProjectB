# 창고 수정 검증 기록 (2026-11-01 재개 완료)

이전 작업 트리의 창고 이동·일괄 보관·정렬·수량 선택·용량 표시를 이어 검토하고 Unity에서 검증했다. 커밋·push 없음.

## 작업 파일

- 신규 코드: `Assets/Script/UI/ChestConvenience.cs`, `Assets/Script/Inventory/Model/ChestItemComparer.cs`, `ChestTransferRules.cs` 및 각각의 `.meta`.
- 수정 코드: `Assets/Script/UI/Chest.cs`, `ChestDivideSlider.cs`, `InventoryManager.cs`, `ItemDrag.cs`, `TownServices.cs`, `Assets/Script/Inventory/InventoryService.cs`, `Model/SlotContainer.cs`.
- 신규 테스트: `Assets/Editor/Tests/ChestTransferTests.cs` 및 `.meta`(10개 테스트).
- 문서: `.claude-rules/Game/ItemSpec.md`, `BasicFeatures.md`, `InventoryRefactorDesign.md`, `TechDebt.md`, `Docs/Playtest/FirstHourGuide.md`, 이 기록.
- 공유 파일의 선행 강화·ESC 수정은 유지했다. 씬·폰트·서드파티·기존 아트 변경은 이번 작업 범위 밖이다.

## 동작과 수정

- B-008: 실제 들어간 수량만 차감한다. 가방/창고가 가득 차면 0개 또는 합칠 수 있는 수량만 이동하며 나머지는 원래 위치에 남는다.
- B-014: ID·티어·강화가 같은 스택만 합친다. 티어 3·강화 10 장비의 보관/꺼내기·정렬·페이지 이동에서 속성을 유지했다.
- B-015: 이동 후 인벤/창고 배열을 같은 `Datas`에 반영하고 한 번 저장한다. 기존 ES3 필드·배열 형식 변경 없음. 창고 내부 페이지 이동도 원본을 Destroy 전에 분리하고 즉시 저장한다.
- G/RT: 선택한 ID의 재료·소비품 모두 보관. T/SELECT: 재료 전부 보관. 창고 R/R3: 전체 병합·종류→등급→이름 정렬. Tab/LB 뒤 C/Y: 수량 선택, Enter/A 확정, ESC/B 취소.
- 낮 첫 캡처에서 마지막 슬롯 줄이 용량/조작 안내를 가렸다. 페이지와 커서를 84% 배율로 줄여 하단 안내 전용 66px를 확보했다. 수량 창 실행 버튼은 꺼내기/나누기로 구분한다. 새 그림·폰트는 만들지 않았다.

## 검증 수치

- Unity 최종 전체 EditMode **460/460**, 실패 0·건너뜀 0, 7.314초. job `a41e85af571f4fe0821ae68d97bef59e`. 신규 창고 테스트 10개 포함.
- 정지 확인 → Unity refresh/compile → 콘솔 오류 0. 최종 플레이 콘솔 오류 0. 대상 코드/문서 `git diff --check` 통과.
- 플레이 모델/뷰/디스크 검증 **5,521개 비교 통과**. 5개 저장 시점에서 ES3 `Datas.invenItem` 125칸과 `chestItem` 150칸의 4개 필드를 모두 즉시 대조했다.
- 빈칸 없이 Wood 58 + 창고 7: 2개 이동, 창고 잔여 5, 재시도 0, 양쪽 합 65. 가득 찬 창고는 이동 0·가방 7 유지.
- 동일 ID 15개 보관 후 다른 페이지의 덜 찬 스택 우선 병합. 재료 전부 8개 보관 시 소비품 유지. 소비품 5개 중 3개 꺼내기→잔여 2, 취소 시 무변경.
- 장비 왕복 티어 3·강화 10 유지. 정렬 후 장비/소비/재료 순서 및 Wood 합계 10 확인.
- 키보드 **T/G/R 3건**, Input System 가상 패드 **SELECT/R3/LB/Y/A/Y/B/RT 8건**, 합계 **11/11** 통과. 가상 장치는 제거하고 입력 설정은 시작값으로 복구했다.
- 레거시 드래그 핸들러/페이지 이동 보조 검증 **7/7**: 7+4 병합, 장비 속성 보존, 차 있는 칸 교환, 페이지 이동 직후 원본 비움/저장, 양쪽 저장, 배치 해제. 비활성 핸들러의 초기 참조를 프로브에서 주입해 로직을 호출한 결과이며 실제 마우스 입력 검증은 아니다.
- 플레이 정지 후 재진입해 가방 Wood 11(티어 2·강화 3), 창고 2페이지 녹슨 검 1(티어 3·강화 10)을 재로드했다. `reentry=True` 확인. 최초 조기 조회는 창고의 1초 지연 로드 전이었으며 로드 완료 후 재확인했다.

## 화면 검토와 미검증

- `Logs/playtest/chest-resume/final-day.png`, `quantity-day.png`, `full-day.png`를 직접 확인했다. 창고 30칸·페이지 탭·사용/전체 수·하단 안내·수량 3/5·꺼내기 버튼·가득 참 경고에 겹침/잘림이 없다. 커서와 슬롯 위치도 맞는다. 화면 이동·전투 모션은 이번 확인 대상이 아니다.
- 기존 퀘스트 HUD가 인벤 위를 덮고, 상단 디버그 HUD와 제목이 조밀하거나 잘리는 현상은 별도 부채로 유지한다. 새 창고 안내와는 겹치지 않는다.
- **미검증:** 물리 키보드/게임패드, OS 마우스 드래그·화면 좌표 hit-test, 실제 프로세스 강제 종료/정전, 빌드 실행·장시간 소크. 강제 종료 대신 이동 직후 디스크 양쪽 값 대조와 정상 재진입을 확인했다.
- 기존 `Assets/Prefab/UI/Item.prefab`의 ItemDrag·2D 콜라이더는 비활성이다. 이번에 마우스 드래그 시스템을 새로 활성화하지 않았고, 핵심 창고 조작은 키보드/패드로 검증했다. 레거시 드래그 함수 보조 검증을 실제 마우스 사용 가능 판정으로 간주하지 않는다.

## 원본 복원·커밋 제외

- 재개 시작 시 ES3 3개를 `Logs/chest-resume-1101-save-backup/`에 백업했다. 이전 실행 당시 원본과 혼동하지 않고 이번 시작본을 복원 기준으로 삼는다.
- SaveFile MD5 `F1CBD3EDF7E845CACE5442F6641C5B2F`, SaveSlot2 `4683BD8B4FFE564E86535BDF7861504B`, **SaveSlot3 `F1A9AC9E7D4BE9716FB6ED03A0BD2C4F`**.
- 최종 플레이 정지(`idle`) 후 3개 원본을 복원했고 **MD5 3/3 일치**했다. 대조 결과를 `Logs/playtest/chest-resume/save-restore.json`에 남겼다. 슬롯 3·ES3 경로 SaveSlot3.es3 유지.
- **커밋 제외:** `Tools/probes/chest_resume_*`, 이전 `chest_runtime.cs`, `chest_status.sh`, `chest_stop.sh`, `chest_checks.sh`, `chest_offline_verify.py`, 모든 `Logs/`(세이브 백업·캡처·실험 스크립트 포함).

## 독립 리뷰 (2026-11-02, inv_chest)

### 범위·발견

- 구현자가 아닌 리뷰어로 미커밋 창고 코드 7개, 신규 코드/테스트 4개와 meta, 관련 설계 문서의 창고 변경만 검토했다. 공유 파일에 섞인 ESC·강화 및 씬·폰트·아트·서드파티 변경은 범위에서 제외했다. 앞 보고에 있는 `Docs/Playtest/FirstHourGuide.md`는 현재 diff가 없어 커밋 대상에서도 제외한다.
- **[필수/P2, 수정] `.claude-rules/Game/TechDebt.md:20`:** 초기 MCP 무응답의 “EditMode·플레이·낮 캡처 미검증, 완료 보류”가 최신 완료 기록과 충돌했다. 독립 재검증 결과와 실제 남은 제한으로 정정했다. 하단의 과거 화면 관찰은 당시 이력으로 유지하고 최신 창고 하단 안내 해소 기록을 적용한다.
- **런타임 [필수] 신규 발견 0건, 코드 수정 0개.** 실제 이동량 차감, 전체 열린 페이지 병합, ID/티어/강화 보존, Destroy 전 원본 분리, 저장 배치 종료 후 양쪽 배열 저장을 확인했다. 새 이벤트 구독·풀 반환·트윈은 없으며, 기존 수명주기 코드는 이번 변경과 구분했다. 신규 C# 4개 모두 UTF-8 BOM·CRLF 확인. 새 Update 경로의 용량 집계는 0.25초 간격이고 문자열 갱신은 수치가 달라질 때만 수행한다. ES3 키·필드명 변경 없음.
- **[참고/P3, 미수정] `Assets/Script/UI/ChestConvenience.cs:209`:** 한글·영문/패드 약어를 한 안내에 병기해 글자 크기 차이와 밀도가 느껴진다. 기존 퀘스트 HUD는 인벤 슬롯을 가리고 상단 디버그 줄/제목도 조밀하다(이번 코드 밖). 재료 일괄 보관·수량 선택·용량 안내는 클래식 메이플식 반복 정리에 맞고, 초반 장비·아이콘의 소박함을 유지한다. 새 유아틱한 그래픽이나 과장된 연출은 없다.

### 직접 검증

- 시작 전 ES3 3개 백업: `Logs/inv_chest_review/save_backup/`. 포커스 가드·최소화·Play Unfocused 적용. 초기 PowerShell 실행 정책 제한은 프로세스 한정 Bypass로 처리했다.
- idle 확인 → refresh/compile 요청 → 콘솔 오류 0. 전체 EditMode **460/460**, 실패 0·스킵 0, **10.895초**, job `1e532b55d6ba49d4a94398bc43e0a49b`.
- 낮 플레이 **5,521개 비교 통과**: 5개 저장 시점의 가방/창고 4필드를 ES3 디스크값과 비교. 가방 합칠 자리 2개만 있으면 2개 이동·창고 5개 잔류·재시도 0, 가득 찬 창고는 이동 0. 다른 페이지 스택 우선 병합, 동일 ID 15개 보관, 재료 8개 보관 시 소비품 보존, 티어3/+10 장비 왕복, 소비품 3/5 꺼내기와 취소, 정렬 후 총량 보존 확인.
- 프레임을 거친 게임 내부 Input System 입력 **11/11 통과**: T/G/R 3개와 가상 패드 SELECT/R3/LB/Y/A/Y/B/RT 8개. RT의 속성 보존 보관 및 가득 참 원본 보존도 통과. OS 입력 주입 없음.
- 기존 프로브는 마을 활성화와 수량 슬라이더 Start 완료를 전제로 했다. 준비 전 첫 실행들은 비활성 UI/같은 호출 내 최초 활성화로 실패했고, 이 결과는 게임 결함 판정에 사용하지 않았다. 실제 프레임 입력으로 초기화한 뒤 동일 모델 프로브와 입력 검증이 통과했다. 이전 실행의 증거 파일은 덮어쓰지 않고 이번 전용 경로에 저장했다.
- 캡처 `Logs/playtest/inv-chest-review/quantity-day.png`, `final-day.png`, `full-day.png` 3장을 직접 눈으로 확인. 하단 안내·수량 3/5·꺼내기 버튼·가득 참 경고의 겹침/잘림 없음, 커서 정렬 정상. 기존 퀘스트 HUD 중첩은 남는다. 캐릭터는 UI에 가려 끼임·방향/이동 품질 판정 대상이 아니다.
- 플레이 콘솔 오류 **0**. 정지 후 `playing=False`, 슬롯 **3**, 경로 **SaveSlot3.es3**. 정지 확인 뒤 원본을 다시 복원하여 **MD5 3/3 일치**(`save-restore.json`).
- Windows64 개발 빌드 **시도 후 실패**: job `build-000f925681`, **2.137초**, 오류 **1**, 경고 **0**, 출력 0MB. 원인: 현재 선택된 **IL2CPP 모듈 미설치**. 동일 원인 콘솔 로그 2개가 남았으며 런타임 오류와 구분한다. 설정 변경·모듈 설치·실행은 하지 않았다.
- 대상 코드·문서 `git diff --check` 통과. 커밋·push 없음.

### 남은 미검증·커밋 범위

- 물리 키보드/패드, OS 마우스 드래그와 좌표 hit-test, 실제 강제 종료/정전, 성공한 빌드와 빌드 실행, 장시간 소크는 미검증. 실제 강제 종료는 사용 중인 에디터를 종료하므로 즉시 저장값 대조로 대체했다. 기존 드래그 핸들러 비활성 상태 유지.
- 신규(각 meta 포함): `Assets/Script/UI/ChestConvenience.cs`, `Assets/Script/Inventory/Model/ChestItemComparer.cs`, `ChestTransferRules.cs`, `Assets/Editor/Tests/ChestTransferTests.cs`.
- 수정: `Assets/Script/Inventory/InventoryService.cs`, `Model/SlotContainer.cs`, `Assets/Script/UI/Chest.cs`, `ChestDivideSlider.cs`, `InventoryManager.cs`, `ItemDrag.cs`, `TownServices.cs`.
- 문서: `.claude-rules/Game/ItemSpec.md`, `BasicFeatures.md`, `InventoryRefactorDesign.md`, `TechDebt.md`, `Docs/Playtest/ChestReview.md`. 공유 문서·InventoryManager·Chest에는 다른 작업 변경도 섞여 있으므로 창고 hunk만 커밋 대상으로 삼는다.
- 커밋 제외: `Tools/probes/chest_*`의 모든 일회성 프로브, 이번 검증 스크립트/백업/캡처/빌드 산출물을 포함한 `Logs/` 전체와 범위 밖 변경.