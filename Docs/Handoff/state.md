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

## 11-11 잔여 후속 A~D (builder)
- (A) `ReleaseCheatGate`: 복원값을 EditorPrefs 에 보관, 빌드 종료 감시(update)·에디터 로드 보정으로 실패/취소 빌드도 SR Debugger 복원. 테스트 `ReleaseCheatGateTests`. (B) 창고 R3 정렬이 LT+R3 에 반응하던 것 `PadChordRules.SortAlone` 으로 차단, InputAudit §14 갱신. (C) `InputDeviceHint`+`TalkNpc` 프롬프트 패드 시 "D-PAD " 접두(아이콘 그림 없음). (D) Q201·Q401·Q404 대사 열 이행.
- 컴파일 0·EditMode 1186 통과·probe `_u31_hint` 확인. 미검증: 실제 릴리스 빌드 실패 시나리오, 실패드.

## 11-11 ES3 호환 점검 (reviewer-arch, 10f676151^..HEAD)
- 새 키: ft.stops·ft.last, features.unlocked, feature.pet.auto/hpPct/mpPct, quest.hudFold, npcq.tracked. 기존 키 이름·타입 변경·삭제 없음. 구세이브 로드 예외·유실 위험 높음 0.
- 중간 1: 구세이브 `equip.extraUnlock` 비트가 새 해금 규칙과 어긋나도 `rawMask & 31` 로 그대로 열린 채 남음(의도된 관대 이행 — 빼앗지 않음). 필요하면 SlotUnlocks.csv 기준 재계산 보정.
- 낮음: 구세이브에서 전투 액션을 B·M·N 에 지정했다면 새 창 열기 키와 충돌(Q 만 `DropNewlyReservedKeys` 가 되돌림). MonsterSheet 18→26열은 `c.Length > 25` 가드로 안전.
- 주간 사용량 54~55%, 사용자 상한 60%.

## 11-11 저녁 1층계 결정 (사용자 "추천대로 알아서")
- 맵 2장: 별도 프리팹 백업 + SampleScene 디스크 상태 그대로 커밋(builder 종료 후). 소실 방지 우선.
- EXP 기준=CSV(문서 정정). 인장=3곳(늪·성채·탑) 확정. 일괄 판매=J키 유지+상점 창 버튼 추가. Q704=대화 1회로 축소. 문지기 a=1 보너스 강화석 +2.
- 보고 NPC: 늪·성채·탑 입구 안내판(QuestEntrance)에서 수락·보고 허용(eve 13건 중 구역 밖 6건 포함).
- 밤 의존: Q104·Q106·Q205·Q403·Q311·Q411 설명에 "밤에만" 표기 + Q창·HUD "밤까지 약 N분" 안내. Q404 의존은 유지.
- 곁가지 EXP: 늪(Q406~409)·탑(Q605~610)을 메인의 2배 이내로 감액, 핀 테스트 값 동반 갱신. 탑 근교 무기: Q605~610 중 한 곳 보상에 무기 1종.
- 후속 P2: Q410 Spirit Koi pity, Q602 보상 EXP +800, 밤 몹 1종 추가, 튜토리얼 "게시판" 문구.

## 11-11 밤 범위 조정 (사용자: 그래픽 작업용 사용량 남기기, 60% 상한 유지)
- **이번 주(60% 안)**: 1층계 초반(마을·샛길·일반 숲, 첫 1시간)만. 밤 의존 표기·"밤까지 N분" 안내(Q104·Q106·Q205), 튜토리얼 "게시판" 문구, 마차 해금 안내 문구, 상점 일괄 판매 버튼, 초반 구간 완주 프로브, 정예 배치(진행 중 builder).
- **다음 주로 미룸**: 늪·성채·탑 입구 보고 안내판, 늪·탑 곁가지 EXP 감액(+핀 테스트), 탑 근교 무기, Q704 축소, 문지기 강화석 보너스, 도감 B08~B13, 사냥 의뢰 6건, Q410 pity, Q603/Q602 EXP, 밤 몹 추가, 몹 6종 행동, 임시 아이콘 폴백, 깊은 숲 이후 완주 프로브. 결정은 위 '저녁 결정'대로 확정(재질문 없음).

- 상한 65%로 상향(사용자 11-11 밤): 범위는 1층계 초반 유지, 남으면 그래픽 방 몫으로 반환, 그 뒤 사용자 추가 지시.
- 65% 안에서 초반 작업 후 남으면 미룬 항목을 가까운 콘텐츠 순서(깊은 숲→늪→성채→탑→문지기)로 진행. 그래픽 몫 따로 확보하지 않음(사용자 11-11 밤 재지시).

## 11-11 R-6 정예 배치 + CI-P4·P7 (builder)
- R-6: `trait elite` 프로필(항상 정예, HP×4)·샛길 낮 `Rabbit2_2`/밤 `RatCheif` 에 쥐 두목 배치, Q106 `objective2 killElite RatChief_BD` + `NpcQuestLog` 두 번째 목표 판정(세이브 4번째 필드). 프로브 `_r6_elite`: 낮 샛길서 정예 쥐 두목 HP160 확인.
- CI-P4: `WeaponMoveProfiles` 전 타입 화이트리스트 + `Weapon.CheckAttackWait` 한 줄 훅(`LocksMove`). CI-P7: `AttackManager` 선행 소비(`Weapon.CanComboEarlyNow` 1줄). 프로브 `_ci_p4p7`: 12종 모드·간격(경량 0.75·표준 0.90·대검 1.0·지팡이 잠금 없음).
- 컴파일(내 변경) 0·EditMode 1193 통과. 미검증: 실 몹 전투로 Q106 완료 흐름, 실패드. 타인 수정 `InventoryConvenience.cs` 컴파일 에러는 내 변경 아님.

## 11-11 밤 미룬 항목 1~6 (builder)
- Q-CI11 기본 칼 원복(ActiveProfileOf null). 늪 곁가지 EXP 감액(Q406~409 ≤3,000, Q602 +800, 핀 갱신). 구역 보고 안내판 ZoneReportSign(깊은 숲·늪·요새·탑 입구, 이브 보고, ReportOnly 필터). 정령잉어 3→10%. 문지기 a=1 강화석 +2·입장 알림. Q704 는 이미 talk 1회(변경 없음). 깊은 숲 Q30x 는 ≤1,316 이라 감액 불필요.
- 컴파일 0·EditMode 1216 통과. 미검증: 안내판 실제 배치 위치·플레이 화면(UnityMCP 프로브 미실행).

## 11-11 밤 이월 2차 (builder)
- 탑 근교 무기: Q610 보상 강화석→FairysGrimoire(마도서). 도감 B08~B13: 새 metric `zone`(9열 zone=MonsterSheet zone, `BestiaryZoneRules`, 밤·보스·미니보스 제외 전종) 보상=구역 세트 부위 1. 사냥 의뢰 D18~D23(성채·탑·늪 미니보스·정예). Tough Rat Hide→Junk(참조 없음 확인, 핀 테스트 정정). `ItemIconLoader`: 아이콘 없으면 Error 대신 이름당 1회 Warning+기존 임시 그림 유지.
- 프로브 `_f1e_flow2`: Q1~Q7 62건 선행→수락→목표→보고→완료 bad=0(안내판 필터 일치 포함). 컴파일 0·EditMode 1220 통과. 미검증: 실제 플레이로 도감 전종 지급·안내판 상호작용.

## 11-11 밤 마감 (주간 약 61%, 상한 65%)
- 완료·push: 1층 초반(밤 안내 QuestNightHint·낮 쥐 도둑·튜토리얼/마차 문구·기본 칼 3타 복원), 정예 쥐 두목 R-6, CI-P4·P7, 상점 일괄 판매 버튼, 깊은 숲·늪 EXP 감액·Q410 10%·Q602 +800·문지기 보너스·구역 보고 안내판 ZoneReportSign, 도감 B08~B13(zone metric)·사냥 의뢰 D18~D23·Q610 마도서, ItemIconLoader 폴백, 몹 사격(숲 사냥꾼·유적 파수꾼), SampleScene 커밋(맵 2장 보존).
- 리뷰 필수 1건 수정: 문지기 보너스 0일 때 강화석 +1 이 붙던 버그(BossReward). 도감 종 캐시는 빈 목록 미캐시. EditMode 1226 통과.
- 다음 주 후보: 밤 몹 추가(그래픽 필요), 덩굴 요정 속박, 감시병 장전 튜닝, ZoneReportSign Active 탭 노출 정리(권장), TempMobShooter 정적 풀 씬 전환 초기화, 실제 플레이 검증 전부(`UserPlaytest_1111.md`·`Weekend1Guide.md`).

## 11-13 한도 계획 (주간 63%, 11-16 04:00Z 재설정)
- 모델 분담: 판단·분류=Opus 5.5 중간(재설정 전 1회 완료, 재설정 후 주말 개선 재료 분류 1회), 구현=Sonnet builder, 리뷰=haiku. 새 세션 시 state.md 하단+queue.md '11-13 이월'(NEXT-*)만 읽고 시작.
- 63→약 85%: Opus 검토 후속(NEXT-10) + 프로브·스크린샷 검증(NEXT-15) + 큐 코드 항목. 85→약 97%: 재설정 전 소진(가치 순 builder). 재설정 후 0→60%: 주말 발견 분류 4%·막힘 수정 ~35%·여유 ~20%, 40%는 그래픽 몫.
- 하지 않음: 주말 중 SampleScene 수정, 밤 몹 추가(그래픽), 발견 사항 전 새 기능 튜닝.

- NEXT-15 완료(11-13): 안내판 4곳(깊은 숲·늪·산적 요새·탑 근교) 낮 프롬프트·입구 창 스샷 `Docs/Playtest/NEXT15/`, 콘솔 에러 0. 깊은 숲은 수락 가능 퀘스트 0이라 창 안 열림(정상).
- 낮 쥐 도둑: Lv1_NearForest_SideRoad 낮 스폰 RatThief_BD 4마리 확인·스샷(`SideRoad_day_rat.png`). 그림은 아직 토끼 계열(NEXT-11 의뢰 유지).
- 관찰(미수정, 경미): 맵 이동 직후 맵 이름 라벨이 이전 맵 것으로 남음(산적 요새 스샷 "탑 아래 마을") — 갱신 지연 가능, 플레이 확인 필요.
- REWORK-8 U38 완료(11-13): Keyboard/Gamepad/Mouse .current 전수 정적 스캔(변수 파생 포함) 누락 0 -> 코드 수정 없음, 소스 스캔 회귀 테스트 `InputNullGuardTests` 추가.
- REWORK-3 데이터(11-13): MonsterSheet edge=fall = 토끼·돌토끼·늪 개구리·포자등 두꺼비, 정예 6종(쥐 두목·미니보스 5) giveUp 30·turn 유지. 수치 튜닝 없음. 컴파일 0·EditMode 1232/1232·play probe(edge 로딩) 확인. 미검증: 실제 낙하·낙하 후 갇힘.
- 남음: fall 몹 낙하 후 복귀·갇힘 처리(TechDebt), 후공 8초·hop 높이 튜닝, 실마우스·패드.
- REWORK-0(11-13): U15 소비 아이템 획득 시 빈 퀵슬롯 자동 등록(패드 경로)·안내 문구, 대화 Space/클릭 진행·선택지 마우스. EditMode 1234/1234 통과, 퀵슬롯 플레이 프로브 확인, 대화 마우스는 미검증.
- 남음: 2차 점검 5종(장비 창 마우스·창고 문구 한글화·낙사·카메라·포털), 패드 수동 퀵슬롯 교체 버튼은 실패드 확인 필요.
- 주의: 플레이 프로브가 세이브 가방에 Potion·Food 1개씩 넣었을 수 있음(퀵슬롯 칸은 원복).
- REWORK-6 대사(11-13): QuestsFloor1.csv 빈 main·side 6건(Q304·Q501·Q503·Q504·Q602·Q704) 대사 열 이행, 보상·EXP 불변. QuestVoiceCsvTests 케이스 6 추가, EditMode 1240/1240.
- 남음: Q101 튜토리얼 코드 대사 유지, reward 계열 코드 대사(CSV 이행은 선택), 2층계 QuestsFloor2·사이드 ㉣㉤.
- 주의: 대사는 새로 쓴 초안(이브 관찰 반말·일사 존댓말·루엔 반말·장로 하게체), 인게임 눈 확인 미검증.
- 11-13 저녁 사용자 정정: 주간 98%까지 사용(그래픽 작업 없음), 5시간 97% 넘으면 resetsAt 까지 대기 후 재개. 판단 많은 REWORK(12·10·5·6·0·3 설계)는 builder opus, 구조 변경은 reviewer-arch. 재설정(11-16 13:07 KST) 뒤 60% 계획은 그대로.
- REWORK-10(11-13): `Docs/Playtest/MapReview.md` 1층계 9맵 표. 1층 맵이 지형 3벌 복제(숲 A·B, 늪 C)임을 발견(씬 작업 필요, 미적용).
- 적용 7314bda68: 깊은 숲·심부 Goblin1 의 2층 CliffGoblin(Lv16·EXP 240) 혼입 제거(여우/숲고블린 전량), Lv4~9 몹 EXP 상향(처치 수 Lv4 171→133, Lv6~8 ~270→~183), 미니보스 x20. EditMode 1240/1240.
- 남음: 비활성 8맵 폭·층 실측 프로브, 늪 포인트 한 점 8마리, 리젠 10초 차등, 막다른 맵 히든 포털, EarlyHourPlan 시간표 재측정(사냥 EXP 증가 반영).
- REWORK-5 P5(11-14, cda9e80bc): 구 QuestCatalog 삭제(LegacySnapshot 고정표), QM 추적 HUD 항상 숨김·배치 코드 제거, QuestBoard RequireComponent 제거, objective2 창 두 줄. EditMode 1239/1239, 프로브 q_rabbits 수락→보고 확인.
- 남음(REWORK-5): 씬 QM HUD·게시판 QuestGiver 제거(씬 허가 필요), 게시판 ↑ 입구·구 세이브 이행·패드 플레이 검증.
- 주의: QuestMigration·Quest Machine 참조는 구 저장 이행용으로 유지(qm.* 키 삭제 금지).
- REWORK-12 장신구(11-14, 083c27d48): 방랑자의 팔찌(tag:Dash 쿨 -20%, 3층계 해금이라 획득처 없음)·사냥꾼 부적(처치 시 전 스킬 쿨 -0.5초·내부 쿨 1초, AlphaWolf 10%). 텔레포트는 Leaf Teleport 스킬이라 SkillTags 만 등록. EditMode 1243/1243, 프로브 확인.
- 남음(REWORK-12): 세트 창 화면(단계 표시·마우스/키보드/패드), 효과 상세 토글, SetSwapPreview 효과 줄, Q-EFX7·Q-EFX8, Gatekeeper 보너스, Bog5 Bleed.
- 주의: 주간 사용량 78% 도달로 세트 창은 착수 안 함(EquipEffectDesign P6 보류 그대로).
- REWORK-12 세트 창(11-14): 장비창 추가 칸 펫 줄 끝 "세트" 칸(커서 노드 23) 호버·키보드/패드 커서 → 착용 세트 요약 팝업(세트별 기존 툴팁 세트 단락·켜진 단계·다음 단계까지 부위 수). 순수 규칙 `SetOverview` + 테스트 7, 단계 상한 공용 `SetTierTable.MaxTier=11`(SetEffectText 0~7 루프 버그 함께 수정). EditMode 1250/1250, 플레이 프로브·스샷 확인.
- 남음(REWORK-12): 효과 상세 토글, SetSwapPreview 효과 줄, 세트 창 문구 Loc 등록(한글 직접), 세트 부위 수 표시가 착용 수보다 작게 나오는 경우("5/4", SetPartCount 가 부위 종류 수) 점검, Q-EFX7·8, Gatekeeper 보너스, Bog5 Bleed.
- 주의: SET 팝업은 호버·커서 중에만 보임. 정적 캐시 RepCache(세트 키→대표 아이템)는 찾은 것만 저장.
- SLOT-R5(11-14): Ring2=15·비트 32 로더 추가(Belt 가 14·16 선점이라 번호 조정), 구 5칸 세이브 빈칸·잠금 테스트, ExtraEquip StatSources 길이 버그 방지. EditMode 1252/1252.
- 남음(SLOT-R5): Ring2 해금 CSV 행·장비창 표시(DisplayOrder)·아이템은 미구현(필요 시).
- (11-14 입력 검증) 인벤·상점·창고·가방 확장을 가상 키보드·패드로 몰아 봄(`Tools/probes/_inputsweep.cs`, InputAudit §14): 대부분 통과.
- 수정: 확인창이 열려도 인벤이 ←→ 를 처리하던 것(`ConfirmPopup.IsBlocking`), 패드 LT+X 가 교체 모드로 새던 것(`ltChord`). EditMode 1252/1252.
- (11-14 F1 해소) 패드 Start=일시정지·창 닫기 전용, 패드 인벤=일시정지 메뉴 가방 항목(PauseMenu·InventoryManager.OpenFromField). 세트 노드 팝업 커서 유지(ExtraEquip 참고6). 가상 패드 통과, EditMode 1252/1252.
- 남음: 실마우스·실패드 손맛은 UserPlaytest_1111.

- (11-14 리뷰1114 반영 a90550f04) Ring2←Ring 허용 SlotRules.Accepts·환급 키/쿨 캐시·RepCache 무효화·롤백 주의 문서. EditMode 1253/1253.
- 남음: 없음(권장 1~3·참고 4 완료).

- (11-14 패드 인벤 단축) 필드 Select 짧게=인벤, 다음 아티팩트=R3 길게, R3 짧게=사용. PadFieldInput.R3 추가, InputAudit·ControlsGuide 갱신. 가상 패드 프로브 검증, 실패드는 UserPlaytest_1111.
- 남음: 실패드 손맛 확인.

- (11-14 REWORK-3 P2 5a8d91dd8) edge=fall 몹 낙하 후: 킬플레인 아래 제거(드롭·EXP 없음)→리스폰, 아래 발판 착지 시 기준점 이동. EditMode 1255/1255, 플레이 프로브 검증.
- 남음: 후공 8초·hop 높이 튜닝.

- (11-14 REWORK-13 미스 검증) 코드는 5d9ff15fe 에 이미 구현. EditMode 1255/1255, 플레이 프로브(Lv99 몹 20타 15 미스, MISS 글자 양방향, 에러 0)로 확인.
- 남음: 명중/회피 장비 스탯 공급원(스탯 도입 때).
- (11-14) TechDebt 슬롯 보스 킬플레인 낙하 → 스폰 위치 복귀(a8239e57f). EditMode 1257/1257.
- (11-14) REWORK-14 재진입 가드 마무리, 남음: 사용자 실기(패드·마우스·낮 캡처)·요금 튜닝·마차꾼 도트.
- (11-14 REWORK-11 마무리) 코드·데이터는 52b67c97f 등으로 이미 완료. play probe로 완벽 등급 일괄(3회분 → 6개)·대장장이 일괄 수량 창 확인, EditMode 1257/1257.
- 남음: 실마우스·실패드 입력, 요리대 전용 도트(PixelLab 방), 생활 탭 트리 연동.
- (11-14 펫 자동 물약) 코드는 10f676151·4e3baebf4 에서 완료. play probe: Tick 으로 HP 20% 에서 Potion 1개 소모, HP 27 → 139/139. EditMode 1257/1257, 콘솔 에러 0.
- 남음: 펫 줄 키보드·패드 실확인, 펫 전용 도트(플레이스홀더).
- (11-14 REWORK-0 2차 5종) 3149f3793: 장비칸 착용품 우클릭·더블클릭 해제, 씬 영문 버튼(close·divide·Sell·Buy) 한글화·번역 누락 1줄, 낙사 복귀 프로브 확인. EditMode 1258/1258, 콘솔 에러 0.
- 남음: 실마우스 해제·카메라 맵 끝·포털 패드 체감(`UserPlaytest_1111.md`), 패드 수동 퀵슬롯 교체 사용자 확인.
- (11-14 REWORK-12 마무리) 효과 상세 토글(Tab/패드 Y, 호버 툴팁·장비 상세창·세트 창 공용 `EffectDetailMode`)·세트 창 문구 Loc 등록(LocUi 13종+I2). EditMode 1261/1261. 미검증: 실입력(키·패드) 인게임.
- 남음: SetSwapPreview 효과 줄·Loc, 변형 실플레이, Q-EFX7.
- (11-14 NEXT-5·14) 몹 사격 시야 판정 플레이 프로브 확인: 탁 트임 차단 false, 지형 벽 사이 차단 true. 코드 변경 없음.
- 남음: NEXT-2 덩굴 요정 속박 미연결(`CanVineBind` 호출처 없음, slow 감속만) — TechDebt 기록, 신규 스킬 구현 필요.
- (11-14 입력 표시 대조) ControlsGuide·인벤 툴바 안내·메뉴 SELECT 문구를 InputAudit 과 일치시킴, 가방 확장 안내(125칸·650~22000G). EditMode 1261/1261, 컴파일 0. 미검증: 화면 캡처·실패드.
- 남음: 상점 O·H 패드 매핑 확인, UI 안 Y(나누기) vs 효과 상세 Y 충돌 판단.
- 11-14 08:55 총괄 정지(주간 89%): 남은 큐는 사용자 확인(UserPlaytest_1111)·사용자 결정(AUTO-129 프로브 삭제·씬 허가)·주말 발견 의존 튜닝(REWORK-9 R-1~14·NEXT-3·후공/hop·마차 요금·리젠 차등)·그래픽·2층계(덩굴 속박)뿐이라 사용량 채우기 작업 안 함. 다음: 11-16 13:07 예약 — Weekend1_Findings 분류부터.
- (11-14 REWORK-10 맵 설계) `Docs/Playtest/MapLayoutSpec.md`: 1층계 맵 9장 고유 지형 설계안·씬 체크리스트. 메랜 DB 접속 실패(WebFetch ECONNRESET·Chrome 미연결)라 메이플 맵 실측 표는 전부 미확인.
- 남음: 보존 2장 확정(사용자), 메랜 맵 구조 채우기, 씬 이전(사용자 허가 후), 새 타일 T1~T5 그래픽 방 의뢰(graphics_requests.md).
- (11-14 REWORK-10 씬) BanditFortress 고유 지형 반영: 안뜰·목책·망루·옥상 4단, 사다리 7, 스폰 9포인트×4, 포털 왕복·스폰 발판 위 확인(플레이 프로브). 히든 포털 bf_ledge 미적용.
- 남음: 고유 지형 4장(NormalForest·DeepForestCore·MistSwamp·TowerOutskirts)·Swamp 분산, T2 석재 그림은 그래픽 방.
- (11-14 REWORK-10 씬) DeepForestCore 고유 지형 "뿌리 동굴" 반영: U자 바닥·양쪽 뿌리 둔덕·T2/T3 뿌리 발판, 로프 6, 스폰 낮/밤 각 22슬롯, 포털 왕복·스폰 발판 위 확인. 히든 포털 미적용.
- 남음: 고유 지형 3장(NormalForest·MistSwamp·TowerOutskirts)·Swamp 분산, 뿌리 T4·석재 T2 그림은 그래픽 방.
- 11-14 총괄 정지(주간 97%): 메이플 참고 자료 Docs/Reference/Maple*(0f206324c, maplestory.io GMS v83 — 개선 재료, 그대로 넣지 않음). 맵 고유 지형 2/5 반영: BanditFortress f74a25756·DeepForestCore 0a640975c(둘 다 실입력 사다리·점프 미검증, 씬 변경은 해당 맵 하위만 확인). 남음: TowerOutskirts·MistSwamp·NormalForest(같은 절차, MapLayoutSpec §5), 퀘스트 보드 U6(놓친 사용자 제보), 퀘스트 한글 본문 나머지(maple_maps_dump fetchko 재실행). 11-16 13:07 예약 작업과 함께.
- (11-14 REWORK-10 씬) TowerOutskirts 고유 지형 "무너진 계단 폐허" 반영: 폭 -640~-137 확장·고지대·큰 층 4·무너진 계단 3줄·사다리 11·탑 입구 최상단, 스폰 낮/밤 각 46슬롯, 포털 왕복·스폰 발판 위 확인. 실입력 사다리·점프 미검증.
- 남음: 고유 지형 2장(NormalForest·MistSwamp)·Swamp 분산, 석재 T2·T3·뿌리 T4 그림은 그래픽 방.
- (11-14 REWORK-10 씬) MistSwamp 고유 지형 "안개 속 좁은 섬·썩은 통나무 순환로" 반영: 폭 -422~277 확장·물웅덩이 17·좁은 섬 8·중앙 큰 섬·통나무 징검 29·윗길 14·로프 10, 스폰 낮/밤 각 42슬롯, 포털 왕복·스폰 발판 위 확인. MapInfo 카메라 벽 필터(같은 높이대 맵 복귀 경계 버그). 실입력 로프·점프 미검증.
- 남음: 고유 지형 1장(NormalForest)·Swamp 분산, 석재 T2·T3·뿌리 T4·통나무/수면 T5 그림은 그래픽 방.
- (11-14 REWORK-10 씬) NormalForest 고유 지형 "나무 사이 오르막" 반영: 폭 -437~391·왼쪽 길 유지·1칸 계단 3줄·큰 층 4(능선 24.4)·윗발판 12·사다리 8·깊은 숲 포털 능선 끝, 스폰 낮/밤 각 53슬롯, 포털 왕복·카메라 경계·스폰 발판 위 확인. PitReturn 목록에서 일반 숲 제외.
- 남음: 지형 복제 해소 완료(5장). Swamp 분산, PitReturn GroundY 맵 필터(리뷰 권장), 고유 타일 T1~T5 그림은 그래픽 방.
- (11-14 맵 사이 이동 통합 확인) 포털 MapChanged 를 출발 맵을 끈 뒤로 이동(Portal.MoveMaps), PitReturn 목록에서 성채 제외. 포털 31개·마차 10곳 전수 도착 점검 통과(MapReview §5). 실입력 사다리·점프는 UserPlaytest_1111 에 맵별 기록.
- 남음: Swamp 포인트 분산, PitReturn.GroundY 맵 필터(근본 수정으로 영향 줄어 낮은 우선순위), 고유 타일 T1~T5 그림은 그래픽 방.
- (11-14 U6·U6a 218285b1f) 퀘스트 창 전체 탭(입구 기본, 완료 가능>진행 중>수락 가능>레벨 부족>완료), 줄마다 아이콘(보상→유형 대체), 상세 의뢰인·보고 NPC, 경험치·골드 한글. EditMode 1268/1268, 캡처 quest_board_1114.png.
- 남음: 전체 탭 키보드·패드 실입력 플레이 확인(UserPlaytest), 보상 아이템 없는 퀘스트 아이콘이 유형 대체라 같은 그림이 반복될 수 있음.
