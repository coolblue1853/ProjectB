# 마을 정비 검증 — 2026-11-01

## 변경 파일

- 수정: `Assets/Script/UI/CraftUI/ShopStockExtender.cs`, `ShopManager.cs` — 기존 슬롯을 보급6종·기본 장비8종으로 재구성, 페이지/가격 안내, 열 때 소형 물약부터 선택.
- 신규: `Assets/Script/UI/TownServices.cs`, `TownServiceLabel.cs`, `TownSavePoint.cs` 및 `.meta` — 마을 이름표9곳, 창고 간격 조정, 기존 도트 색조를 쓴 기록지기1명과 SaveProgress 연결.
- 신규: `Assets/Editor/Tests/TownShopTests.cs`, `TownStockTests.cs` 및 `.meta` — 품목/일반 등급/아이콘/장비 프리팹/매매 차익/초반 예산/계보 장비 공급 경로 검사.
- 수정 문서: `.claude-rules/Game/ClassicMapleDesign.md` §19, `Docs/Playtest/FirstHourChecklist.md`, `Docs/Playtest/TempAssets.md`. 본 문서 신규.
- ItemSheet 가격·능력치·퀘스트 보상은 유지. 구매/매각 1.5배/1배 규칙과 §12 물약25/45/70G·귀환120G를 그대로 적용한다. 기존 미진열 기본 장비를 진열한 변경이다.

## 숫자와 기능

- 런타임: 상점2페이지·품목14종·이름표9곳·기록지기1명. 소25/중45/대70/귀환120/식량225/횃불75G, 검450/너클750/창750G, 모자·갑옷·장갑·바지750/신발450G.
- 실제 BuyItem 경로6건(물약3·귀환·검·모자): 각각 수량+1, 차감25/45/70/120/450/750G 일치. 메서드 호출 프로브이며 모든 품목을 하드웨어로 구매한 검사는 아니다.
- 검 판매 수량1→0·골드+300G, Buyback.TryBuyBackLast로 수량1 복구·골드 원상복구. `town1_validation_root.txt`.
- 기존 제작 창: 무기=True, 방어구=True(6페이지), 강화=True. 제작 결과·강화 성공까지 이번에 반복하지 않았다.
- 이동한 창고에서 Input System 가상 ↑ 입력 → chestOpen·창고UI=True. 기존 보관/꺼내기 검증 로그는 `town1_validation.txt`(소형물약6→0→6).
- 기록지기 SaveNow=True, 디스크와 현재 골드4540G·좌표(-405.8,136.36) 일치. 상태/위치 주입을 쓴 저장 스모크이며 손으로 걷는 전체 동선은 아니다.
- E 가상 키 → 기본 장비 페이지 활성·선택 RustedSword, 다시 상점 열면 Small HP Potion. UI 캡처에서 아이콘·가격·페이지 문구 확인.
- 낮 화면: `Logs/playtest/town1_west_final.png`, `town1_center_final.png`, `town1_east_final.png`, `town1_shop_supplies.png`, `town1_shop_equipment.png`. 기존 도트 재사용·이름표 간격·안내/머리 위 대화 표시 확인. 화면 우측 퀘스트 추적기가 상점 가방 일부와 겹치는 기존 표시 문제는 남아 있다.
- 리뷰 기준: reviewer-code/reviewer-arch 체크리스트로 세이브 필드 호환, 입력 수명, 런타임 전용 배치, 기존 씬/프리팹 참조, 매 프레임 검색/할당을 점검했다. 코드에는 새 세이브 키·필드·전투 동작 변경이 없다.

## 한계·미검증

- 사람60분 연속 플레이·실제 시간당 골드/물약 소비·하드웨어 키/패드·제작 및 강화 결과의 이번 재실행·신규 기록지기 정식 아트는 미검증이다.
- 시작 시 선택돼 있던 슬롯2는 가방0칸으로 로드돼 기존 ResetBoxOrigin 경로에서 상점 확인이 막혔다. 슬롯3(가방15칸)으로 바꿔 검증했고 기존 슬롯2 데이터는 수정하지 않았다. 슬롯2의 원인은 이번 범위 밖이다.
- 원래 있던 씬·폰트·ES3Defaults·M_PixelSlash 및 다른 작업 변경은 유지했다. 커밋·push 없음.
- 커밋 제외: `Tools/probes/town1*.cs`, `Tools/probes/town1_call.sh`, `Logs/` 전체(스크린샷·입력/검증 로그·세이브 백업). 기존 firsthour 프로브도 이번 커밋 대상으로 넣지 않는다.

## 최종 종료 확인

- EditMode **414/414**, 실패0·스킵0, 0.910초. 컴파일/플레이 종료 후 콘솔 오류0, 플레이 idle.
- `TalkNpc.cs`도 수정: 마을 서비스 안내가 있는 NPC는 기존 UP TALK 표시를 감춰 안내가 중복되거나 방향 반전으로 뒤집히지 않게 했다. 대화 입력은 그대로다.
- 세이브 시작 백업은 본 실행의 `Logs/playtest/town1_save_backup`을 기준으로 복원했다. SaveFile `F1CBD3EDF7E845CACE5442F6641C5B2F`, SaveSlot2 `74DDA8E69AFECBE7FD3E8A01FEECFC5F`, SaveSlot3 `19F8B41DA36D93C79D675FB3CFD1325C`.
- 입력 설정2/2·선택 슬롯2 복구·Town1Keyboard 제거. `town1_restore_root.txt`에서 최종 MD53/3 일치.

## 독립 리뷰 후속

- 필수 수정: ShopManager 상세 가격을 구매가로 통일, Q/E 페이지 전환 시 이전 구매/소비품/장비/스킬 상세 해제. TownServiceLabel 안내 문자열 캐시와 비활성 서비스 이름표 숨김·복원. TownStockTests에 이름표 활성화 회귀 검사 추가.
- 검증과 한계의 최신 기록은 ClassicMapleDesign.md §19.2 참조. 전체 415/415, 제작2종 결과/재료 보존 및 강화+1·강화석1개/50G 차감 확인. 정상 재실행 오류0, 세이브3/3 MD5 복원.
- 커밋 대상에는 본 문서와 앞 변경 파일 목록의 TownShopTests.cs 및 .meta도 포함한다(앞 최종 채팅 보고에서 누락되었으나 town1 검증 문서에 포함된 파일).
## 추가 독립 확인 (2026-11-01, town1_review)

리뷰 중 다른 실행이 같은 파일과 Unity 플레이 상태를 변경했다. 코드 수정은 해당 실행이 반영한 최신본을 유지했으며, 아래 결과는 별도 프로브와 테스트 job으로 재확인했다. 이 실행의 제품 코드 추가 수정은 없다.

| 발견 위치 | 심각도 | 문제·현재 상태 |
|---|---|---|
| ShopManager.cs:157,174,194 | 필수/P2 | 상세 가격이 기준 판매가여서 실제 구매가와 달랐음. buyprice 수정 반영 확인: 물약25G·녹슨 검450G 일치. |
| ShopManager.cs:473~479 | 필수/P2 | 페이지 전환 때 이전 스킬 상세가 남을 수 있었음. DetailOff 해제 반영 확인, 가상 패드 RB/LB 양방향에서 상세·스킬 false. |
| TownServiceLabel.cs:42~43 | 필수/P2 | 대상과 형제인 이름표가 비활성 대상을 계속 표시했음. 대상 활성 상태 동기화 반영 확인: 창고 숨김1/1·복원1/1. |
| TownServiceLabel.cs:19,56 | 필수/규약 | LateUpdate 근접 전환에서 안내 문자열 생성. Init 캐시 수정 반영 확인. |
| TownStockTests.cs:83 | 필수/검증 | 초안 테스트가 TMP Init의 renderer.material EditMode 오류로 실패. 렌더 초기화 대신 가시성 필드 구성으로 수정된 테스트 통과 확인. |

- 직접 실행한 전체 EditMode: **415/415**, 실패0·스킵0, **0.67554초**, job `0b7397584cde4135bf650d09874cee06`. 최초 실패와 플레이 상태 경합으로 시작이 거절된 테스트는 통과 근거에서 제외했다.
- 실제 제작 API: RustedSword **0→1**, T0_Ore5/Wood10/Small Bone10 차감. LegArmor **0→1**, T0_Ore5/Wood10/Rabbit Hide15 차감. 강화 API: RustedSword **+0→+1**, Success·골드50·강화석1 차감. 재료·돈 주입 검사이며 경제/실제 UI 입력 전체 검증은 아니다(`town1_review_craft.txt`).
- 가상 Gamepad RB/LB **2/2** 페이지 전환, 재방문 최초 Small HP Potion. 구매 상세 물약25G·녹슨 검450G 일치. 가상 패드는 finally에서 제거했다(`town1_review_ui.txt`). 하드웨어 패드는 미검증이다.
- 이름표 **9개**, 기록지기 **1개**. 창고 비활성/재활성 이름표 검사 **2/2**. SaveNow 근접 true·5유닛 원거리 false. 정상 지면 플레이어(-405.8,136.36)에서 저장 true·디스크 Datas.playerPos(-405.8,136.36) 일치.
- 낮 화면 `Logs/playtest/town1_review_keeper_standing.png` 육안 확인. NPC 중심으로 플레이어를 직접 옮겨 지면 아래로 떨어졌던 초기 캡처(`town1_review_keeper.png`)는 증거에서 제외한다. SaveData 오기 프로브 컴파일 실패도 검증 수치에서 제외한다.
- 품질 의견/미수정: 작은 일반 장비·절제된 채도는 소박한 초반에 맞는다. 상인·세나·기록지기의 같은 실루엣은 직업 식별이 약하고, 기록지기와 플레이어가 포개지면 이름표가 플레이어 머리에 겹친다. 새 그림 제작은 하지 않았다. 퀘스트 HUD와 상점 가방의 기존 겹침은 범위 밖이다.
- 남은 미검증: 사람60분 수입·물약 소비, 하드웨어 키보드/패드, UI 입력부터 제작·강화 결과까지 전 과정, 슬롯2 가방0칸의 원인. 이번 실행의 숫자로 이 항목들을 대체하지 않는다.
- 종료 **idle**, 콘솔 오류 **0**, 선택 슬롯 **2** 복원, 임시 패드 **0**. 시작 백업 `Logs/playtest/town1_review_backup`으로 세이브 **3/3 MD5 일치**(`town1_review_restore.json`). 슬롯2의 이번 시작 MD5는 `60CDEDFC265168487F85D591DC0338E5`로, 앞 보고의 백업과 다르므로 이 실행 시작본을 보존했다.
- 커밋 대상: ShopManager.cs, ShopStockExtender.cs, TalkNpc.cs; TownServices.cs/TownServiceLabel.cs/TownSavePoint.cs와 .meta; TownShopTests.cs/TownStockTests.cs와 .meta; ClassicMapleDesign.md, FirstHourChecklist.md, TempAssets.md, Town1Audit.md. 다른 작업 파일은 제외한다.
- 커밋 제외: `Tools/probes/town1_review*`, 기존 `town1*`/`town1review*` 일회성 프로브, `Logs/` 전체. 커밋·push 없음.
