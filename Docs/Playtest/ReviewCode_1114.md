# 11-14 11차 큐 코드 커밋 정적 리뷰 (AUTO-130, 코드 수정 없음)

- 범위: `b5fcd77d0..HEAD` 코드 커밋. 481a51c1(SLOT-R5 Ring2)·e657c6e9(세트 창)·7e7acfe2(SetOverview)·083c27d4(장신구·처치 환급 내부 쿨)·cda9e80b(REWORK-5 P5)·7314bda6·43910716(CSV 값만)·c489893d(퀘스트 CSV)·26e4f58c(퀵슬롯 자동·대화 입력)·0b2f3f22(테스트만). 형식은 `ReviewCode_1105c.md`.
- 검증: reviewer-arch(Opus)가 `git show`·호출부 grep 으로만 봤다. 플레이·EditMode 재실행 없음. **전부 미검증**(코드 읽기로 추정).

## [필수]

(없음)

## [권장]

1. `Assets/Script/UI/ExtraEquip.cs:293` + `Assets/Script/Inventory/Model/EquipmentModel.cs:77` — 로드 때 `equipArea`(또는 `TryGetSlot`)가 칸과 정확히 같아야 통과한다. Ring2 칸에 `equipArea=Ring` 반지를 끼우면 다음 로드에서 폐기 장신구로 보고 가방으로 돌려보낸다. 지금은 Ring2 아이템·UI가 없어 잠재 문제다. 대안: `SlotRules.Accepts(slot, want)` 한 곳에서 Ring2←Ring 을 허용하고 두 로더가 같이 쓴다. SLOT-R 후속.
2. `Assets/Script/Combat/Effects/SkillRuleEvents.cs:148` — 처치마다 스킬×규칙 수만큼 `"Refund:" + r.id + "#" + r.entry` 문자열 생성·`float.TryParse(p3)`. 다수 처치 때 GC. 대안: `EffectEntry` 로드 때 키와 p3 float 캐시.
3. `Assets/Script/Inventory/Model/SlotRules.cs:17` — `AllExtraMask` 31→63. 구 빌드로 되돌리면 비트 32를 버려 Ring2 아이템이 사라진다. 지금은 Ring2 아이템이 없어 영향 없음. 대안: 롤백 정책 문서에 한 줄.

## [참고]

4. `Assets/Script/UI/SetOverviewPopup.cs` `RepCache` — static 가변 캐시, 지우는 곳 없음. DB 재로드 시 옛 `ItemData` 참조가 남는다. 대안: DB 로드 시점에 `Clear` 또는 인스턴스 필드로.
5. `SetOverviewPopup.BuildText` — `InventoryManager.instance.SetEffectText`·`DatabaseManager.setsEffectStack` 직접 참조(UI끼리 결합). 순수 규칙(SetOverview)은 분리돼 수용 가능.
6. `ExtraSlotBox.OnPointerExit`(`ExtraEquip.cs:759`) — 마우스가 SET 칸을 벗어나면 키보드·패드 커서가 SET 노드에 있어도 팝업을 닫는다(혼용 입력 때만). 대안: 커서 노드가 SetNode 면 유지.
7. `Assets/Script/Dialogue/PixelDialogueUI.cs:168` — 마지막 대사를 Space 로 닫는 같은 프레임에 점프가 들어가는지는 UiLock 해제 순서에 달림. Z 확정도 같은 구조라 회귀는 아님. 대안: 해제를 다음 프레임으로 미루거나 플레이 1회 확인(`UserPlaytest_1111.md` 항목 있음).
8. `Assets/Script/UI/QuickSlot/QuickSlotBar.cs:78-88` — 구독 OnEnable/OnDisable 짝 정상. 막대가 꺼진 동안 얻은 물약은 자동 등록 안 됨. 필요하면 Awake/OnDestroy 로.
9. `Assets/Script/Quest/QuestBoard.cs` — `RequireComponent(QuestGiver)` 만 제거, 씬 컴포넌트는 남아 직렬화 안전. 이상 없음.

## 확인 결과(문제 없음)

- Ring2: `Extras` 끝에 추가해 ES3 키(`equip.extra`·`equip.extraUnlock`)·순서 유지. 짧은 구 배열은 `i < ids.Length && i < Items.Length` 로 방어. 관련 배열은 모두 `Extras.Length` 기준, `_labels[i]==null` 가드.
- 세트 창 3경로: 마우스 PointerEnter/Exit/Click, 키보드·패드 `EquipNavRows` 노드 23·`TipForNode`. OnDisable 에서 닫음. Update 할당 없음.
- `SkillRuleEvents.RefundThisKill`: Hook·ClearCooldowns·OnKilled 마다 Clear, 누수 없음.
- 퀵슬롯 `AutoSlotFor`: 등록된 id·찬 칸 보존, Load·창고·해제 경로 제외.
- `NpcQuestTracker` 문자열 재조립은 `_dirty`·주기 갱신 뒤에서만. CSV 커밋은 값만, 컬럼 추가 없음.
