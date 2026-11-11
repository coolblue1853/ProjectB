# 상태 (11-04 총괄 마감, 주간 34%)
- 오늘 완료: GUARD-BANNER·SAFE-ZONE(60c1f176), NPC_BLOCK·QUEST-UI(e4170c76), 설계 4종(Q-DEATH·EquipSlotExpansion·Floor1Completion §15·InventoryRefactorDesign §7), 낙하물·봇 점프·MAP-SCAN(8c3bdbbe), INV-EXP-1~3(14711f2d), TUTORIAL-HINT(df080212), MINIMAP 점(a0320663), SETTINGS-KEYS(기존 구현 확인 5ac9640d), CHIEF-1·2(9e8ca9d6)·SLOT-1(3dc2dfae), PV2 7항목 통과(54e2c7d4).
- 주의: 다른 프로세스가 같은 큐·state.md 를 병행 수정 중(INV-EXP·TUTORIAL·SETTINGS 가 세션 시작 전 이미 구현됨). 중복 확인 필요.
- 사용자 대기: Q-DEATH 승인(Lv1~10 EXP 손실 0, 보스 구간 7%) → DEATH-PENALTY-IMPL. Q-SLOT·Q-INV02 임시 가정 확인. Q13~18 주말.
- 이월: INV-EXP-4(통주 로그 필요, 봇 재측정 중단), SLOT-2~4, 대장 훈장·반지 아이콘(P3, 그래픽=사용자 검토), `PauseMenu` 설정 '뒤로' 행 겹침(P4). 미확인: 키 입력 경로, 무적 2초 만료 시각, 밀집 정량, 대장 실제 처치.
- 세이브: 최초 원본 Logs/pt_save/orig.
- 추가 완료(11-04 후반): DEATH-PENALTY-IMPL(Lv1~10 손실 0·보스 근처 7%·그 외 5%, 승인=사용자 진행 지시), PauseMenu 겹침, SLOT-2·3·4. 컴파일·EditMode 652/652 통과, UnityMCP 연결 실패로 플레이 미검증. 큐 남은 것: INV-EXP-4(보류). 다음: 에디터 연결 복구 후 PV3(슬롯 UI·해금·사망 페널티 플레이 확인).
- 11-04 마감(사용자 취침): 주간 36%로 마감, 남은 7%p 는 11-05(수→목, 누적 상한 62%)에 합산해 쓴다. 총괄 교대: 11-05 는 같은 총괄 세션 유지, 금요일(11-06)에 새로 판다. 내일 시작 작업: debt-sweep(TechDebt P1~P3 비그래픽 + PV2 미확인 보강), INV-EXP-4(보류), 사용자 답 대기 Q-SLOT·Q-INV02.
- 11-05 중간(주간 48%/5h 88%): 5시간 한도 때문에 중지, 리셋(14:50 KST) 후 재개. 오늘 완료: DEC-1105-A, 슬롯 재구현 R1~R4, AUTO-1~41(22·23 포함), 리뷰 수정 전부, INV-FILTER, PV7~PV9. 남은 큐: AUTO-42·44·46·47. 목표 62%까지 재개 필요. 세션 개설 한도(10개) 도달이라 서브에이전트(builder)로 처리 중.
- 11-05 오후(주간 49%/5h 95%): AUTO-42(Floor2Recalc.md)·44(PV9 4항목 통과, Loc [Sealed] P4 등록)·46(de1e8284 패드 LT)·47 완료. 큐 비었음. 5h 리셋(14:50 KST) 후 재개: 새 큐 배치 생성(정예 탄막 피격 PV, Loc [봉인] 수정, debt-sweep P1~P3) -> 주간 62%까지. 저녁에 MorningQuestions 작성.
- 11-05 저녁(주간 61%/5h 93%): 큐 AUTO-1~128 처리, EditMode 901/901. 남은 큐 AUTO-129·130(문서·프로브 정리). 5h 리셋(21:50 KST) 후 크론 틱이 재개, 큐 비면 새 배치. MorningQuestions 작성 완료. 금요일 총괄 교대.
- 11-09 REWORK-5 P0·P1 완료(주간 사용량 한도로 P2 이전 중지): `QuestService`·확장 열·게시판/촌장 11건 CSV 이행·`QuestMigration`, 컴파일 0·EditMode 1008/1008. 플레이 검증은 에디터에 씬이 없어 미검증(게시판 ↑ 대화·촌장 일 묻기·구 세이브 이행).
- 이어서: 플레이 검증(Town 로드 후 게시판 ↑, s_wood 수락 시 가방 보유량 반영) → P2(NpcQuestWindow 상세 패널·포기·입구 모드) → P3 HUD. 상세는 `Game/QuestSystemDesign.md` §7-2.

## 11-11 총괄 상태
- 슬롯 개편 v2 완료(10f676151·64af17e48): 시작 5칸·홀수 층계 해금(`SlotUnlocks.csv`)·구 세이브 이행·펫 자동 물약·룬/성물 뼈대. 컴파일 0·EditMode 1021 통과·장비창 스크린샷 확인.
- 남음: 5·7층계 해금 스토리 호출 지점, 펫 자동 물약 실제 발동·펫 줄 키보드·패드 확인, 장비창 툴팁 겹침, 개발 세이브 정리(가방 Wolf 시험 아이템), 콘솔 `Destroy may not be called from edit mode` 출처 미확인.
- BUG-U9: 방어 수정만(재현 못 함). REWORK-0 1차 표 U24~U42 완료(P0 2건: U32 `↑` 중복 반응, U34 낙사 복귀) — 다음은 ③ REWORK-8 키 충돌, P0 수정.
- 11-11 REWORK-5 P2~P4 완료: 퀘스트 창 재작성(3탭·상세·보상·포기 확인·마우스/키/패드, `QuestWindowText/View`), 추적 HUD 단일화(최대 5·`…외 n건`·접기 `quest.hudFold`), 입구 모드(`QuestEntrance` 게시판·푯말, `BountyPanel`→`BountyGraduation`). 컴파일 0·EditMode 1038 통과·플레이로 창/입구 텍스트·스크린샷 확인. 미검증: 실제 마우스 클릭·패드 입력, 푯말 ↑ 실입력. P5(QuestCatalog 삭제·objective2)는 이행 동등성 테스트 기준값이라 보류.
- 11-11 REWORK-1·2 S1: U32·U34 플레이 검증됨(probe). 인벤 재설계 §9 설계 확정. 공용 `QuantityPopup`(구매·판매·나누기·꺼내기·버리기, ↑↓±1·←→±10·숫자·최대·길게 누르기, 키·패·마우스)+버리기 U24(Delete/상세창 RT/인벤 밖 드래그). 컴파일 0·EditMode 1044 통과·키 경로 플레이+스크린샷 확인. 미검증: 마우스 클릭·패드·상점 구매/판매·창고 꺼내기 실화면·드래그 버리기. 다음 S2(우클릭 U25·호버 툴팁 U26·상점 마우스 U27).
- 11-11 REWORK-2 S2: 우클릭 사용/장착/판매(U25)·호버 툴팁(U26, `HoverTip`)·상점 마우스(U27: 호버·클릭·재클릭 구매·휠·X). 핵심 발견: Item.prefab 의 ItemDrag 가 오래전부터 비활성이라 마우스가 죽어 있었음 → 활성화(콜라이더는 꺼 둠, 칸끼리 드래그 교환은 미동작). 컴파일 0·EditMode 1049 통과·프로브+스크린샷 확인. 미검증: 실마우스·패드. 다음 S3(화면 버튼·ConfirmPopup).
- 11-11 REWORK-1 S3·S5: 칸 드래그 교환 복구(레이캐스트 드롭: 이동·교환·합치기)·`InventoryToolbar` 화면 버튼(정렬·쌓기·필터·잠금·가방 확장, 키·패드 병행, 쌓기 T/LT+LB)·S2 리뷰 지적 반영. 컴파일 0·EditMode 1051 통과·probe+스크린샷. 미검증: 실마우스·패드·창고 열림 버튼. 남음: ConfirmPopup(U40)·이름 검색(S4).
- 11-11 WSKILL-V2-2 P3~P6 완료(1e0dcddb1·fe82ac803): 타입·공용 트리 효과 연결, 스킬창 K/LT+Y(마우스·키·패드), 툴팁·토스트·Loc. 컴파일 0·EditMode 1055 통과·probe wskill_p3/p5 확인. 미검증: 실마우스·패드 입력.
- 남음: P7 수치 조정(체감 플레이 후), 생활 탭 노드, 타입별 극의, 전투 중 창 차단, 최대 HP 장비 변경 시 재계산(권고).

## 11-11 REWORK-12 P4~P8 (builder)
- P4 Rule 2차(Chain·OnKillCast·Proc·CooldownRefund, `SkillRuleEvents`)·P5 Replace 변형(`SkillVariants`, GateCleave_Wide)·P6 툴팁 특수 효과 구획+평타 Basic 피해·P7 세트 11단계 데이터+`EffectScore`·점검 표·P8 트리×장비 쿨 곱 결합. 컴파일 0·EditMode 1070 통과·probe efx_p4b/p5/p6/p7/p8.
- 버그 2건 수정: `LoadItemData`(P3)·`LoadSetsData`(P7) 가 effectIds 미복사 → 세트 단계 효과가 실제로 안 걸렸음.
- 남음: 신규 장신구·팔찌(Teleport 쿨 훅 필요), U 등급 승격 Q-EFX7, 변형 판정 실시전(숙련 잠금으로 프로브 불가)·실마우스/패드·체감 플레이, 세트 창 별도 화면·효과 상세 토글·교체 미리보기 효과 줄, Gatekeeper 고유 보너스(슈퍼아머 이벤트), Crit 트리거.

## 11-11 REWORK-3·BUG-U23 (builder)
- 행동 속성 8열(move·edge·chase·chaseRange·visionY·giveUp·aggro·attack)을 MonsterSheet 맨 뒤에 추가, `MobBehaviorRules`+BT 훅 5곳. 일반 몹 배정은 `MobRules.md`, 빈 값=기존 동작.
- BUG-U23: 소형 몹 본체 콜라이더 월드 하한 1.2x1.1(토끼·유령·돌토끼·허수아비·버섯·두더지 확대). 컴파일 0·EditMode 1080 통과·play probe(후공 맞은 뒤 추격, 제자리, 히트박스) 확인.
- 남음: edge=fall 배정·정예 속성, 실전 체감, 스킬 종류별(AoE·화살) 실판정 확인, 후공 8초·hop 높이 튜닝.

## 11-11 REWORK-7·10 레벨 디자인 참고 (builder)
- `Game/LevelDesignRef.md` 신규: 메랜 DB 몹 749종·맵 7곳(WebFetch, Chrome 미연결) 레벨 구간 HP·EXP·맵당 개체 수 표 → 우리 1~2층 대조, 튀는 구간 L1~L7.
- 주요: 2층 EXP 2.8배 도약, 1층 후반 HP 정체(EXP/HP 증가), DeathBringer HP 2500 이상치, 층계 경계 HP 8배.
- CSV 변경 없음(Unity MCP ECONNREFUSED, EditMode 불가). 사이트에 맵 크기·발판·리젠 없음 → Q-LVREF.

## REWORK-6 퀘스트 콘텐츠 (11-11)
- `Game/QuestDesign.md` 신규: 메이플 퀘스트 구성 조사(표본 4건)·유형 3종·대사 단계·1/2층계 메인 줄기·사이드 사슬·재분류표.
- `QuestsFloor1.csv` `kind` 빈칸 60행 채움(main 7·side 8·reward 45, 이행분 포함 71건 = 10/8/53). 다른 값 불변.
- 코드 변경 없음. Unity MCP 연결 실패로 EditMode 미실행(미검증). 질문 Q-QD1~4 는 OpenQuestions.

## 11-11 REWORK-15·14 (builder)
- REWORK-15: `MonsterSheet.csv` HP 22종 조정(1층 Lv11~15 76~110 → 200~350, 2층 일반 몹 완화·상향, DeathBringer 2500 → 700·EXP 566), 정예 3종 HP 기반 x4. probe `_bal_ttk` 처치 1.5~2s → 3.5~7s(1층 후반). `MiniBossDataTests` x10 → x4~x10. EditMode 1096 통과. 상세 `LevelDesignRef.md` §7.
- REWORK-14: `Game/FastTravel.md` 신규(설계만): 정류장 9곳·퀘스트 해금·요금·ES3 `ft.stops`·3경로·히든 포털·P0~P5. 코드 없음.
- 남음: 실플레이 체감·보스 HP·초반 장비 스탯 상한 점검(Q-BAL1), 마차 구현 P0~.

## 11-11 REWORK-11 요리 일괄 (builder)
- `Game/Cooking.md` 신규(진단·설계). Town x=-406 마을 요리대(쿨 0)·요리 X → `QuantityPopup` 일괄(최대 10, 한 판 등급 전체 적용, 가방 부족 시 부분 지급)·선택 단계 마우스 버튼·MiniGameSpot ↑ 중재·대장장이 쌓이는 제작물 일괄(상한 99).
- 컴파일 0·EditMode 1107 통과(신규 11)·play probe(요리대 존재·팝업·2개분 소모·탄 요리 2) 확인. 미검증: 실마우스·패드, 완벽 등급 일괄, 대장장이 UI 실조작. Loc 6키 I2 동기화.

## 11-11 REWORK-8 키 정리 (builder)
- `InputAudit.md` §14: 키보드 전체 키 표·충돌 7건 판정. 실충돌 K(스킬창 vs 제작) -> 제작 기본 키 Y(inputactions), 디버그 0키 시간 치트 `#if` 게이트.
- U39: 가방 툴바 필터·확장에 G·N 표기 + 하단 안내 2줄(분해·버리기·비교·프리셋·패드). `ControlsGuide` 행 추가(퀘스트 H·Del, 방어 G, 확장 N·쌓기 T, 페이지, 버리기).
- 컴파일 0·EditMode 1107 통과. 미검증: 인벤 툴바 화면 실확인·패드. 남음: U40 ConfirmPopup, U37 하드코딩 키 설정 이관, U38 null 체크.

## 11-11 REWORK-8 후속 U37·U38·U40 (builder)
- U40: `ConfirmPopup`(UI/ConfirmPopup.cs, 기본 아니오, 마우스·←→·Z/Enter/A·Esc/B) 신규, 퀘스트 포기(창 안 확인 제거)·가방 확장(구 N 두 번)·장비 분해(구 D 두 번)에 적용. `DismantleInput.Request`·`BagExpansion.TryBuy/Describe` 프로브 창구는 유지.
- U37: 도감·지도·미니맵·방어·스킬창·퀘스트 6키를 키 설정 재지정 대상으로(`Hotkeys`, inputactions 액션 6 추가, 충돌 규칙은 InputAudit §14). F10 안내 창 넘침 수정(패널 1000·글자 21). U38: null 체크 누락 없음 확인.
- 컴파일 0·EditMode 1114 통과(신규 7)·play probe(확인창 기본 아니오·←·Esc→아니오·Enter→예, B→U 재지정 후 U 로 도감 열림, 키 설정·F10 화면 확인). 미검증: 실마우스·패드, 퀘스트 포기·분해·가방 확장 실조작.

## 11-11 REWORK-4 전투 입력 CI-P0~P3·P5·P6 (builder, Weapon.cs 미수정)
- 신규 `AttackMoveRules`·`WeaponMoveProfiles`(전 타입 표+`EnabledKinds` 화이트리스트=롤백 한 줄)·`DirectionBuffer`·`StickDigital`. 한손검·검방·단검·쌍검 Lunge, 대검·둔기·방패·활류 InPlace(PlayerController), 방향 버퍼 0.08·점프 `TryConsumeJump` 일원화·스킬 버퍼 0.15·홀드 연타·A+X 동시(점프 먼저).
- 컴파일 0·EditMode 1138 통과(신규 24)·play probe `_ci_move`(A·B·C)·`_ci_input`(E·F·H) 통과. 미검증: 실패드·마우스 체감, 프로브 D·G·I 전수(P4·P7 필요). Q-CI9·10 기록.
- 남음: CI-P4·P7(Weapon.cs 모션 방 커밋 뒤), 창·낫·클로·너클·채찍·철퇴·도끼·태도·지팡이 모드 적용, P8 문서.

## 11-11 EarlyHour P0·P1 + FastTravel P0~P3·P5 일부 (builder)
- EarlyHour: Q105·106·107·201·206·207 데이터 조정, Q109·Q110 신규(`EarlyHourPlan` §10 정정: RatChief 미배치→killElite 보류, Q107 선행 Q103, Q110 선행 Q108). 핀 테스트 값만 갱신.
- FastTravel: 정류장 CSV·상태(ES3 ft.stops/ft.last)·마차 NPC 9곳·NpcMenu 행 모드·요금/금지 규칙·FT_Q 7건. 컴파일 0·EditMode 1146 통과·play probe(NPC 배치·목적지 9줄·확인→75G 차감 이동) 확인.
- 미검증: 실패드·실마우스, 맵별 NPC 위치 캡처, 슬롯 전환 ES3. 남음: P4 히든 포털, 재진입 가드.


## 11-11 슬롯 훅 확인 · 히든 포털 P4 · 인벤 S4 검색 (builder)
- 슬롯 훅: `BossLoot` 첫 처치가 `grantsRank`→`boss:N` 호출 중(1~3층계·bestiary:B04 연결 확인). 4~7층계(룬·허리띠·성물·마법석)는 보스 에셋 없음 = 콘텐츠 대기(EquipSlotExpansion §10).
- 히든 포털: `HiddenPortals.csv`+`HiddenPortals.cs`(숲 샛길·일반 숲 각 1쌍, 지면 스냅·마우스 클릭·숨김 라벨). S4 검색: `InventorySearch`(/·Ctrl+F·한글 IME·Esc, 패드 LT+RB 첫 글자, 버튼 "검색 /"), 입력 중 단축키 차단(IsBlocking).
- 컴파일 0·EditMode 1154 통과(신규 8)·play probe 이동·검색 확인. 미검증: 실패드·실마우스·실제 IME, 일반 숲 히든 포털 실좌표. Q-FT5 임시 가정 유지.

## 11-11 WSKILL-V2-2 P7 · 퀘스트 대사 CSV (builder)
- P7: 정수 스탯 `ToWholePoints`(0.5→1,1,2,2,3)·극의 전 타입 `UltimateMul`·`CombatState` 5초 전투 중 K 창 차단·장비 HP 변화 시 트리 HP% 재계산. `SkillTreeP7Tests` + probe `wskill_p7`.
- 대사: 로더·열(offer/progress/done)·CSV 우선 폴백은 이미 있었음 → 6건(Q204·Q301·Q403·Q502·Q601·Q604) 새 대사 이행 + `QuestVoiceCsvTests`(형식 상한·폴백). Q102 빈 값 핀·QuestCatalog 기준값 불변.
- 컴파일 0·EditMode 1177 통과·play probe 확인. 미검증: 실 NPC 대화창 화면, 생활 노드(훅 없음).

## 11-11 총괄 저녁 정리 (주간 51%, 사용자 상한 60%)
- 큐 완료: ⓪ 슬롯 v2 · ③ REWORK-8(키 표·U37·U39·U40) · ④ REWORK-5 P2~P4 · ⑤ REWORK-1·2 S1~S5(검색 포함) · ⑥ WSKILL-V2-2 P3~P7 · ⑦ REWORK-12 P4~P8 · ⑧ REWORK-3·U23 · ⑨ REWORK-7·10 · ⑩ REWORK-6 설계·kind·대사 6건 · ⑪ REWORK-13·15·14(P0~P4) · ⑫ REWORK-11. 초반 1시간 P0·P1 데이터 적용.
- 설계서만: REWORK-4(CI-P0~P3·P5·P6 구현 완료, P4·P7 은 모션 방 `Weapon.cs` 정리 후), REWORK-9(`EarlyHourPlan.md`, R-5·R-6 killElite 보류).
- 남은 것: 4~7층계 보스 콘텐츠(해금 훅은 연결됨), 요리 레시피 CSV화, 퀘스트 대사 나머지, 리뷰 권장(NpcQuestWindow 629줄 분할 등), U9 재현, REWORK-4 P4·P7, 플레이 미검증 전체(`Docs/Playtest/UserPlaytest_1111.md`).
- Spartan 설치(11-11): 점검 스크립트·git 가드 훅 적용, 훅 실차단은 새 세션에서 확인 필요.

## 11-11 밤 정리 세션
- (A) NpcQuestWindow partial 분할(441+187줄)·만료 추적 id Prune·Redraw Query 캐시·Tracker.OnDisable HUD 숨김·빈 입구 차단 커밋. (B) `CookingRecipes.csv`+`CookingRecipeCsv` 로더(폴백 DefaultRecipes). EditMode 1184 통과.
- (C) U9: 풀 반환·사망·맵 전환 뒤 고아 라벨 0건, 원인 미확정. 기존 방어가 라벨을 영영 못 켜던 결함을 라벨-바 매 프레임 동기로 수정. 프로브 `Tools/probes/_u9_*.cs`(미추적).

## 11-11 점검 U29~U42 잔여 (builder)
- U33: `Assets/Editor/ReleaseCheatGate`(릴리스 빌드서 SR Debugger 끔·빌드 뒤 복원). U36: 패드 LT+R3 미니맵(`PadFieldInput.LtR3`, 아티팩트 R3 와 분리, InputAudit §14 등록). U31: `TalkNpc` 범위 최소 2.2.
- U29·U30 은 이미 구현(UiLock.TryBack·PauseMenu) 확인만, U35·U42 는 코드 확인만(시각 미검증). 컴파일 0·EditMode 1184 통과·probe `_u36_pad` 확인.
- 미검증: 실제 릴리스 빌드, 실패드, NPC 범위 실플레이.
