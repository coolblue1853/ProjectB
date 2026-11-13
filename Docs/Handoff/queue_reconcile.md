# 큐 완료 여부 대조표 (queue_reconcile)

- 기준: `Docs/Handoff/queue.md` 의 체크 안 된 `- [ ]` 줄 53개(같은 ID 중복 줄 포함)를 git log·코드·문서로 대조. 기준일 2026-11-13.
- 상태: **완료 / 부분 / 미착수 / 그래픽 / 사용자 필요**. 그래픽 항목은 상태만 적고 건드리지 않았다.
- 증거: 커밋 해시 또는 파일 경로. `state:N` 은 `Docs/Handoff/state.md` 줄 번호.
- 1층 영향: 플레이어가 1층계에서 체감하는 정도(상/중/하).
- 비용: 남은 일을 끝내는 데 드는 주간 사용량 **추정** %(대략치).
- 한계: Unity MCP 연결 실패(ECONNREFUSED) 상태라 플레이 검증은 문서 기록만 믿었다. 코드 재검증은 하지 않았다.

## 대조표

| 항목 | 상태 | 증거 | 남은 일 | 1층 영향 | 비용(주간 %) |
|------|------|------|---------|----------|--------------|
| REWORK-0 관례 이탈 점검 | 부분 | e02f712e3(1차 점검), dc25967e4(U32·U34 수정), `Docs/Playtest/ConventionAudit.md`, state:18 | U24~U42 1차 표 완료. 11-13 U15 퀵슬롯 자동 등록·대화 Space/클릭 진행 수정, U14 구현 확인. 남음: 2차 점검 5종(장비 창 마우스·창고 문구 한글화·낙사·카메라·포털), U6/U8b 는 REWORK-5 | 상 | 6% |
| BUG-U9 허공 LV 표시 | 사용자 필요 | 6f395c13b·748a3c232(방어·동기화 수정), state:18 "재현 못 함" | 사용자 재현 또는 계측 로그 | 하 | 0% |
| REWORK-8 키 배치·NPC 정리 | 부분 | 4596422c5(K→Y·키 표), f9c176eb4(ConfirmPopup·U37·U38·U40), 32a9523d3(NPC 선택지·Q), state:55-64 | 실마우스·패드 검증, 하단 안내 일치 확인 | 상 | 3% |
| REWORK-5 퀘스트 통합 | 부분 | 90d33fdff(P0·P1), 18ff75578(설계), d78e6dd34(P2~P4), cda9e80bc(P5), `Game/QuestSystemDesign.md` §7 | P5 코드 완료(11-14). 남음: 씬 QM HUD·QuestGiver 제거(씬 허가 필요), 게시판 입구·구 세이브 이행·패드 플레이 검증 | 상 | 5% |
| REWORK-1 인벤·상점·창고 재설계 | 부분 | 12e8b4fe0(S3·S5 툴바·가방 확장 버튼), state:20-22(S1~S5) | 구현 대부분 완료. 가상 입력(키·패드) 검증 통과(11-14, InputAudit §14), 실마우스·실패드 손맛만 남음(UserPlaytest_1111) | 상 | 3% |
| REWORK-2 인벤·상점 재구현·수량 창 | 부분 | 3af1354b5(우클릭·툴팁·상점 마우스), 52aeac8a3(U14), `Assets/Script/UI/QuantityPopup.cs` | 가상 입력 통과(11-14), 실마우스·실패드 손맛만 남음 | 상 | 1% |
| INV-BAG-EXP 가방 확장 | 부분 | `Assets/Script/Inventory/BagExpansion.cs`·`Model/BagExpansionRules.cs`, `UI/InventoryToolbar.cs`(확장 버튼 12e8b4fe0), f9c176eb4(확장 확인창) | 가상 입력(N·LT+X)·구매·확인창 통과(11-14, 한도 시 X 새는 버그 수정), 가격·최대 칸 실확인과 유저 안내 문구 남음 | 중 | 2% |
| REWORK-12 장비·세트 효과 | 부분 | 8dd03b9de(P8 쿨 곱), e9b535d4f(P7 세트 11단계·EffectScore), 906467331(P6 툴팁), 083c27d48(장신구 2종·환급 내부 쿨), 세트 창(11-14, SetOverview·SetOverviewPopup), `Assets/Script/Combat/Effects/SkillVariants.cs`, state:26-30 | 효과 상세 토글, 세트 창 문구 Loc 등록, Q-EFX7, 변형 실플레이, Gatekeeper 보너스, 팔찌 획득처(3층계) | 상 | 5% |
| REWORK-3 몹 행동 속성 | 부분 | e6b524591(속성 8열·BT 훅), `Assets/Script/Enemy/MobBehaviorRules.cs`, state:31-32 | 낙하 후 정리 해결(5a8d91dd8), 남음: 후공 8초·hop 높이 튜닝 | 상 | 3% |
| BUG-U23 작은 몹 판정 | 완료 | e6b524591(콜라이더 하한 1.2x1.1), state:33(EditMode 1080·play probe) | 없음(체감 확인은 REWORK-3) | 중 | 0% |
| REWORK-7 레벨 디자인 참고 | 완료 | c6e859c7d·bdf9ffce9, `.claude-rules/Game/LevelDesignRef.md`, Q-LVREF(`Docs/OpenQuestions.md`:689) | 산출물 완료. 수정안 반영 여부는 REWORK-15 와 묶어 사용자 판단 | 중 | 0% |
| REWORK-10 맵 구조·몹 배치 | 부분 | 14671055e·a87ebe053·c0a426259, 11-13 7314bda68(깊은 숲 2층 몹 혼입 제거·Lv4~9 EXP), `Docs/Playtest/MapReview.md` 작성 | 남음: 지형 3벌 복제(씬)·비활성 8맵 폭/층 실측·늪 포인트 몰림·리젠 차등·막다른 맵 히든 포털(MapReview §4). 타일맵 그림은 그래픽 | 상 | 4% |
| REWORK-6 퀘스트 콘텐츠 | 부분 | 87678eb17(QuestDesign.md·kind 재분류), 1bbd3932b(U7), 07bc7372d, state:41-45·89 | 퀘스트 대사 나머지 단계(1층계 main·side 완료 11-13, 남음: Q101 코드 대사 유지·reward 계열 코드 대사 유지·Q 창 노출 Q-QD2·2층계) | 하 | 2% |
| REWORK-13 미스(회피) 시스템 | 완료 | 5d9ff15fe(미스 시스템), `Assets/Script/Combat/MissRules.cs`(PlayerAttackMissMax 60) | 11-14 검증(MISS 표시·상한 프로브·테스트 1255). 남음: 명중/회피 장비 스탯 공급원 | 중 | 2% |
| REWORK-15 몹 HP 곡선 | 완료 | 11cd4320a(MonsterSheet HP 22종·정예 x4), probe `_bal_ttk`, state:47 | 체감 플레이 확인은 사용자 몫 | 상 | 0% |
| REWORK-14 빠른 이동(마차) | 부분 | 6dcbcf134(FastTravel P0~P3·P5 일부), 265047bfe(히든 포털 P4), `Assets/Script/FastTravel/*.cs` 5개, `.claude-rules/Game/FastTravel.md` | 재진입 가드 완료(11-14). 남음: 정류장 9곳 실플레이·실패드/마우스 사용자 확인, 요금 튜닝, 마차꾼 도트(그래픽 의뢰) | 하 | 1% |
| REWORK-11 요리 일괄 | 완료(코드) | 52b67c97f(요리대·일괄), 84b8be51d, `.claude-rules/Game/Cooking.md`, state:51-54 | 실마우스·패드, 요리대 도트(그래픽 방), 생활 탭 연동 — 완벽 일괄·대장장이 일괄은 11-14 probe 검증 | 하 | 0% |
| REWORK-4 전투 입력 재설계 | 부분 | f6e8739f3(무기별 이동 모드), `Assets/Script/Combat/AttackMoveRules.cs`·`WeaponMoveProfiles`·`DirectionBuffer`, state:65-69 | CI-P4·P7(Weapon.cs, 모션 방 커밋 뒤), 나머지 무기 모드, 실패드 검증 | 상 | 6% |
| REWORK-9 초반 1시간 시간표 | 부분 | 736bcfcd7·b447d20f7·e612bbafd(설계), `.claude-rules/Game/EarlyHourPlan.md`, state:88 | 설계 완료. R-1~14 CSV 조정 미착수, killElite 보류(R-5·R-6), 실플레이 검증 시나리오 | 상 | 8% |
| 펫(자동 물약) | 부분 | 10f676151(`Assets/Script/Features/PetFeature.cs`), 4e3baebf4(범위 추가), state:16-17 | 자동 물약 실발동, 펫 줄 키보드·패드 확인. 짝수 층계 해금이라 1층 영향 없음 | 하 | 1% |
| 공통 규칙: 3경로(마우스·키보드·패드) | 부분 | 4596422c5(InputAudit §14), f9c176eb4, NEXT-8(실패드 미검증) | 각 UI 실패드·실마우스 검증 | 상 | 2% |
| G1 묶음 부위별 방어구 교체 | 그래픽 | 1a84bfc28·c54c956ef·6f01d0909 | 건드리지 않음 | - | - |
| G1-1 영역 지도 | 그래픽 | 6f01d0909 | 건드리지 않음 | - | - |
| G1-2 잎사귀 세트 자르기 | 그래픽 | `Docs/ArtCompare/Motion/equip/leaf_set/` | 건드리지 않음 | - | - |
| G1-3 SetEquipmentLayers·CelEquipMap | 그래픽 | 6f01d0909 | 건드리지 않음 | - | - |
| G1-4 조합 자동 검사 | 그래픽 | 없음 | 건드리지 않음 | - | - |
| G1-5 인게임 확인 | 그래픽 | 없음 | 건드리지 않음 | - | - |
| G1-6 속옷 몸 1차 | 그래픽 | 1a84bfc28(속옷 몸), c54c956ef(목도리 이동) | 건드리지 않음 | - | - |
| G2 나머지 모션 확장 | 그래픽 | f1e00b127(걷기·달리기·점프 속옷 몸 23장) | 건드리지 않음 | - | - |
| G3 여유 그래픽 묶음 | 그래픽 | 143c4e066(세부 단계 문서) | 건드리지 않음 | - | - |
| 사용자 검토 필요 줄(그래픽 큐 제외) | 그래픽 | queue.md 41행 | 건드리지 않음 | - | - |
| INV-EXP-4 시작 칸·가격 보정 | 사용자 필요 | 103e8179f(AUTO-4 경제 표), state:1 "통주 로그 필요" | PtBot 로그 확보 후 가격표 조정 | 하 | 0% |
| SLOT-R5 반지2(Ring2) 로더 | 완료 11-14 | `EquipSlot.Ring2=15`·비트 32(Belt 가 14·16 선점), `SlotRules.Extras` 끝 추가·AllExtraMask 63, 구 세이브 테스트 | 로더만. 해금 CSV·UI·아이템은 필요 시 별도 | - | - |
| DUR-REVIEW 내구도 | 사용자 필요 | `.claude-rules/Game/Durability.md`(추천 보류), 0ba158c84 | PT-BOT 로그에서 골드 과잉 확인 시 재검토. 코드 없음 | 하 | 0% |
| AUTO-129 일회성 프로브 삭제 | 미착수 | `Tools/probes/_*.cs` 335개 잔존, `Docs/Playtest/ProbeInventory.md` 존재 | 삭제 조건 충족분만 선별 삭제 후 문서 갱신 | 하 | 1% |
| AUTO-130 11차 정적 리뷰 | 미착수 | `Docs/Playtest/ReviewCode_1105d.md` 없음 | 리뷰 문서 신규 작성 | 하 | 2% |
| NEXT-1 밤 몹 추가 | 그래픽 | queue.md 313행 | 그래픽 방 작업 | - | - |
| NEXT-2 덩굴 요정 속박 | 부분 | `Assets/Script/Combat/Rules/Floor2MobBehaviorRules.cs`(VineRange 3·VineCooldown 8, 정원사 골렘) | 덩굴 요정 대상 연결 여부 확인, 플레이 확인 | 하 | 1% |
| NEXT-3 감시병 장전 튜닝 | 부분 | `Assets/Script/Combat/Rules/AmmoMagazine.cs`(3발·재장전 1.2s), 2f6fa20ca(원거리 몹) | 실측 튜닝 | 하 | 1% |
| NEXT-4 ZoneReportSign 탭 노출 | 완료 | 09ef24e74(Ready만 노출), db6f66b4f(완료 기록) | 없음 | 하 | 0% |
| NEXT-5 몹 사격 시야 판정 | 부분 | aa2eb25b6, 9680fc538 | 플레이 검증(NEXT-14 와 같이 미검증) | 중 | 1% |
| NEXT-6 TempMobShooter 정적 풀 초기화 | 완료 | db6f66b4f(11-13 완료 기록) | 없음 | 하 | 0% |
| NEXT-7 실플레이 검증 전부 | 사용자 필요 | `Docs/Playtest/UserPlaytest_1111.md`, `Docs/Playtest/Weekend1Guide.md` 존재 | 사용자 실플레이 | 상 | 0% |
| NEXT-8 실패드 검증 CI-P4·P7 | 사용자 필요 | `Docs/OpenQuestions.md`:746 Q-CI12 | 실제 패드 필요 | 상 | 0% |
| NEXT-9 주인공 리그 변경 확인 | 그래픽 | 38ba36c6f | 그래픽 방 확인 | - | - |
| NEXT-10 Opus 검토 반영 | 완료 | f504f0f62(B09~B11 제외·회귀), 472d59929(Q605~610 EXP 감액), 48b9b97f4·09ef24e74(안내판 수락·보고), 35205d219(도감 캐시) | B08·B10 종 목록 제한 여부 1건 확인 | 하 | 0% |
| NEXT-11 낮 쥐 도둑 그림 | 그래픽 | queue.md 323행 | 그래픽 방 의뢰 | - | - |
| NEXT-12 주인공 리그 변경(38ba36c6f) | 그래픽 | queue.md 324행 | 그래픽 방 확인, 되돌리기 금지 | - | - |
| NEXT-13 기본 칼 3타 보존 | 완료 | db6f66b4f(11-13 완료 기록, Q-CI14) | 없음 | 중 | 0% |
| NEXT-14 몹 사격 시야 판정 코드 | 완료 | aa2eb25b6, 9680fc538, db6f66b4f(플레이 미검증 기록) | 플레이 검증은 NEXT-5 와 묶음 | 중 | 0% |
| NEXT-15 안내판·쥐 스폰 스크린샷 검증 | 미착수 | Unity MCP ECONNREFUSED 로 수행 불가 | Unity 연결 복구 후 spartan:unity-render | 중 | 2% |

## 미착수·부분 중 1층계 플레이 영향 큰 순 Top 10

1. REWORK-9 초반 1시간 시간표: CSV 조정 R-1~14 미착수 (상, 추정 8%)
2. REWORK-0 관례 이탈: 2차 점검과 나머지 수정 (상, 6%)
3. REWORK-8 키·NPC: U38 null 체크, 실마우스·패드 검증 (상, 3%)
4. REWORK-3 몹 행동 속성: edge=fall 배정, 정예 속성, 튜닝 (상, 3%)
5. REWORK-10 맵 구조: MapReview 표·데이터 수정 완료(11-13), 남음 = 씬 지형 3벌 복제 해소·비활성 맵 실측 (상, 4%)
6. REWORK-1 인벤·상점·창고: 실마우스·패드 검증 (상, 3%)
7. REWORK-5 퀘스트 통합: P5 정리와 플레이 검증 (상, 5%)
8. REWORK-6 퀘스트 콘텐츠: 대사 나머지 단계 (상, 5%)
9. REWORK-4 전투 입력: CI-P4·P7 은 커밋 완료(af9aac2fd, 11-11), 남은 것은 실패드 검증뿐 (상, 2%)
10. REWORK-12 장비·세트 효과: 효과 상세 토글, 변형 실플레이, Q-EFX7 (상, 5%)

참고: NEXT-7·NEXT-8(실플레이·실패드 검증, 상)은 사용자 필요 항목이라 Top 10 에서 뺐다. 부분·미착수 비용 합계 추정은 약 76%로, 주간 목표(60%)를 넘는다.

## 총괄 정정·주의 (11-13)
- REWORK-4 CI-P4·P7 은 이미 구현·커밋됨(af9aac2fd). 위 표의 '커밋 대기'는 낡은 기록.
- Unity MCP 직접 연결은 ECONNREFUSED 이므로 `bash Tools/u.sh call <tool> '<json>'`·`Tools/compile_errs.sh`·`Tools/run_tests_wait.sh` 를 쓴다. 에디터가 꺼져 있으면 직접 기동.
- 사용자가 주말에 플레이하는 동안은 코드·씬 변경과 컴파일(도메인 리로드)을 하지 않는다. 그 시간대는 문서·분류만.
- 비용 합계(~76%)가 한도를 넘으므로 Top 10 을 위에서부터 자르고, 항목마다 커밋한다.
