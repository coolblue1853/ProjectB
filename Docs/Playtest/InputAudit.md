# 키보드·게임패드 조작 점검표 (AUTO-40, 11-05; 패드 상태 AUTO-59 갱신)

방식: `KeyAction.inputactions` 바인딩 + 코드 하드코딩(`kb.xKey`·`pad.xxx`·`new InputAction`)을 grep 으로 대조. 수정은 AUTO-46·49·50 에서 적용, 아래는 현재 상태. **실기 패드 검증 없음(미검증, 가상 패드 프로브만)**.
읽은 곳: `Assets/InputSystem/KeyAction.inputactions`, `BasicFeatures.md` §5·§13·§14·§16, `Floor1PlayVerify.md` PV7·PV9·PV10, `ShieldGuard.cs:34-46`, `BestiaryWindow.cs`, `WaypointMap.cs`, `MinimapHud.cs`, `NpcQuestWindow.cs`, `CraftingManager.cs:750-760`, `ControlsGuide.cs`.

## 1. 전투·이동 (Player 맵)

| 액션 | 키보드 | 패드 | 비고 |
|---|---|---|---|
| 이동 | ← → | 왼쪽 스틱 | 십자는 UI 맵만. 이동에 십자 없음(§3 P10) |
| 점프 | C | A(South) | |
| 대시 | Z | B(East) | |
| 달리기 | Shift | Y(North) | |
| 평타 | X | X(West) | |
| 스킬 A/S/D/F | A/S/D/F | LB / **LT** / RB / **RT** | LT 는 방패 방어와 공유(§3 P1, 해소 AUTO-46) |
| 퀵슬롯 1~4 | 1~4 | L3 짧게 / 십자← / 십자→ / **L3 길게** | 4 = L3 길게(AUTO-49). 설정에서 재지정 가능 |
| 아티팩트 | V | R3 | 키 설정 대상 |
| 다음 아티팩트 | Tab | Select 짧게 | 필드 한정 |
| 방어(방패) | G | LT | 하드코딩(`new InputAction`), 키 설정 대상 아님(§3 P11) |
| 퀘스트 창 | J | Select 길게(0.8초) | AUTO-49 (R3 에서 이동) |
| 인벤 | I | Start | |
| 제작 | K | 십자↓ | |
| 포털·NPC 대화·채집 | ↑ | 십자↑ (UI/Up, PortalOn 포함) | AUTO-49 에 PortalOn 십자↑ 추가 |
| 일시정지 | Esc | Start | 인벤 열기와 같은 Start (열림 상태로 분기) |
| 도감 | B | **LT+Select** | AUTO-49 (필드 한정) |
| 지도(워프) | M | **LT+Start** | AUTO-49. 가방 열기·일시정지와 동시 발동 안 함 |
| 미니맵 토글 | N | **없음** | `MinimapHud` 키보드 전용(§3 P6 보류) |

## 2. UI 안 (인벤·상점·창고·제작)

| 액션 | 키보드 | 패드 |
|---|---|---|
| 선택 / 확인 | Z / Enter | A |
| 교체(Change) | X | X |
| 나누기 | C | Y |
| 뒤로 | Esc | B |
| 페이지 | Q / E | LB / RB |
| 창고 이동 활성 | Tab | LB |
| 소비 사용(ActiveConsum) | 없음 | LT |
| 제작 진입 | K | 십자↓ |
| 정렬 R / 분해 D / 보기필터 G / 핀 P | 키 | R3 / L3 / LT+RT(AUTO-64 적용) / R3 |
| 프리셋·되사기 | 숫자·U·] | Select·RT·LT+Y |
| 상점 O·H | O·H | **확인 안 됨**(`ShopConvenience.cs:77-84` 패드 분기 있음, 매핑 미확인) |
| 가방 확장 N | N | LT+X |

UI 탐색은 방향키/십자/스틱 → UI 맵 4방향 + `verticalCheck`·`horizontalCheck`. 대부분 창은 패드로 탐색 가능. 도감·지도도 패드로 열 수 있다(AUTO-49).

## 3. 문제 항목 상태 (AUTO-59 갱신)

| # | 구분 | 내용 | 상태 | 근거 |
|---|---|---|---|---|
| P1 | 충돌 | LT = SkillS + 방패 방어 | ✅ 해소(AUTO-46). 방패 장착 중 LT 는 방어만. 방패 없을 때 SkillS 동시 발동 위험은 남음 | PV9 #4 통과(가상 패드). 실기 미검증 |
| P2 | 충돌 | R3 = 아티팩트 + 퀘스트 + 핀 | ✅ 해소(AUTO-49). 퀘스트 = Select 길게, R3 = 아티팩트 | 커밋 3e66bdef |
| P3 | 미지정 | 퀵슬롯 4 패드 없음 | ✅ 해소(AUTO-49). L3 길게 | 커밋 3e66bdef |
| P4 | 미지정 | 도감 B 패드 없음 | ✅ 해소(AUTO-49). LT+Select | 커밋 3e66bdef, `ControlsGuide` 도감 행 |
| P5 | 미지정 | 지도 M 패드 없음 | ✅ 해소(AUTO-49). LT+Start | 커밋 3e66bdef |
| P6 | 미지정 | 미니맵 N 패드 없음(UI 비필수) | 보류 → 후속 후보(번호 미부여): 미니맵 LT+L3 | 위험: L3 는 퀵슬롯 1·4 와 짧게/길게 공유, LT+L3 는 상점 재료 일괄 판매와 겹침 |
| P7 | 미지정 | 포털 ↑ 패드 바인딩 없음 | ✅ 해소(AUTO-49). 십자↑ | 커밋 3e66bdef |
| P8 | 탐색 | 도감 구역 탭 패드 없음 | ✅ 해소(AUTO-50). LB/RB 전환 | 커밋 b22ee2c8 |
| P9 | 일관성 | Select 필드 = 다음 아티팩트, 인벤 = 프리셋 | 문서화 완료(본 표 §2·§1). 키 설정 안내문 반영은 후속 | 이중 의미는 UI 잠금으로 분기 |
| P10 | 이동 | 십자로 이동 불가(스틱만) | 해결(AUTO-65): 설정 PAUSE>SETTINGS>DPAD MOVE(기본 OFF, ES3 `settings.dpadMove`). ON 이면 십자 ←→ 이동, 퀵슬롯 2·3 패드 비활성. 실기 패드 미검증 | `OpenQuestions` 임시 가정 |
| P11 | 설정 | 방어 G·도감 B·지도 M·미니맵 N 키 설정 재지정 대상 아님 | 미해결(후순위). 패드 도감·지도(LT 조합)도 재지정 대상 아님 | `BasicFeatures.md` §5 "남은 한계" |

신규 확인 사항(문서 간 불일치): 보기 필터(§2 표)와 잠금(`BasicFeatures.md` §16)이 둘 다 **LT+R3** 으로 적혀 있다. 실제 바인딩 확인 전까지 어느 쪽이 맞는지 미정 → `OpenQuestions` 임시 가정.

우선순위(남은 것): P10 > P6 > P11 > P9 문서 반영.

## 4. 판단 필요 → OpenQuestions
- `Q-INPUT` 방패 방어 패드 충돌 해결안(P1 후속): 방패 없을 때 LT 는 SkillS 발동 허용 여부. 임시 가정 = 허용(AUTO-46 현행 유지).
- 십자 이동 추가(P10) 시 퀵슬롯 2·3 재배치 방식: 판단 필요(임시 가정 없음, 이동 우선 여부 미정).
- LT+R3 중복(보기 필터 vs 잠금) 해소 방향: 판단 필요. 실제 바인딩 확인 후 결정.

## 5. 패드 버튼 → 액션 최종표 (AUTO-59)

컨텍스트 열: 필드 = 이동·전투 맵, 인벤 = 인벤·상점·창고·제작 등 UI 맵, 공통 = 양쪽.

| 버튼 | 단독 | 짧게 | 길게 | 조합 | 컨텍스트 | 실기 |
|---|---|---|---|---|---|---|
| 왼쪽 스틱 | 이동·커서 | | | | 필드 이동 / 인벤 커서 | 미검증 |
| 십자 ↑ | 포털·대화·채집 | | | | 필드 / 공통(UI/Up) | 미검증 |
| 십자 ↓ | 제작 진입 | | | | 필드 / 인벤 | 미검증 |
| 십자 ← | 퀵슬롯 2 | | | | 필드 | 미검증 |
| 십자 → | 퀵슬롯 3 | | | | 필드 | 미검증 |
| A | 점프 / 선택·확인 | | | | 필드 / 인벤 | 미검증 |
| B | 대시 / 뒤로 | | | | 필드 / 인벤 | 미검증 |
| X | 평타 / 교체 | | | | 필드 / 인벤 | 미검증 |
| Y | 달리기 / 나누기 | | | | 필드 / 인벤 | 미검증 |
| LB | 스킬 A / 페이지·창고 활성 | | | | 필드 / 인벤 | 미검증 |
| RB | 스킬 D / 페이지 | | | | 필드 / 인벤 | 미검증 |
| LT | 스킬 S / 방어(방패) / 소비 사용 | | | LT+R3 필터·잠금(§3 신규 확인 참고), LT+X 가방 확장, LT+Y 되사기, LT+Select 도감, LT+Start 지도, LT+L3 재료 일괄 판매 | 필드 / 인벤 | 미검증 |
| RT | 스킬 F / 되사기 | | | | 필드 / 인벤 | 미검증 |
| L3 | 퀵슬롯 1 | 퀵슬롯 1 | 퀵슬롯 4 | 분해 2회(인벤, 2.5초 안) | 필드 / 인벤 | 미검증 |
| R3 | 아티팩트 | 아티팩트 | | 정렬·제작 핀 | 필드 / 인벤·제작 | 미검증 |
| Select | 다음 아티팩트 짧게 | 다음 아티팩트 | 퀘스트 창 | 프리셋(인벤) | 필드 / 인벤 | 미검증 |
| Start | 인벤 열기 / 일시정지 | 인벤 / 일시정지 | | 지도(LT+Start, 가방·일시정지와 동시 발동 안 함) | 공통 | 미검증 |

비고: 미니맵 토글 패드 없음(P6). 방어 LT·도감/지도 조합은 키 설정 재지정 대상 아님(P11). 전 행 실기 패드 미검증.

## 6. AUTO-63 입력 사양 (AUTO-64·65 가 읽는다)

근거: 필터·잠금 모두 `KeyAction.inputactions` 가 아니라 코드에서 `Gamepad.current` 를 직접 폴링한다. 따라서 AUTO-64 는 inputactions 가 아니라 `InventoryFilterView.cs:64` 한 줄(과 RT 되사기 가드)을 바꾼다. 표는 임시 가정.

| 기능 | 키보드 | 패드(변경 후) | 발동 조건 | 충돌 없음 근거 |
|---|---|---|---|---|
| 잠금 토글 | L | LT+R3 (유지) | 가방 state ""·detail·ShopCusorOn·chestOpen | R3 단독=정렬(`InventoryManager.cs:1249` 이미 `!lt` 가드), 핀 P=제작 창 한정 |
| 보기 필터 순환 | G | **LT+RT** (이동) | state ""·chest 없음 | LT+RT 조합 사용처 없음. 단 RT 단독 3곳이 `!lt` 가드 없이 폴링 → 가드 추가 필수: `Buyback.cs:85`(되사기), `ChestConvenience.cs:173`(창고 G), `GearPresets.cs:117`(프리셋 장착) |
| 십자 이동(옵션) | 없음 | 십자 ←→ (기본 OFF) | 설정 옵션 ON, 필드 | ON 이면 퀵슬롯 2·3 비활성 |

후보 비교: LT 와 쓰이지 않는 버튼은 A·B·LB·RB·RT 이나 A(선택)·B(뒤로)·LB/RB(페이지)는 단독 동작과 겹쳐 RT 가 가드 3줄로 가장 싸다. LT 사용 중: X 가방확장·상점 H, Y 되사기, L3 일괄판매, R3 잠금, Select 도감(필드), Start 지도.
미검증: 위 근거는 정적 grep.

## 7. 구현 결과 (AUTO-64·65, 11-05)

- AUTO-64: 보기 필터 = LT+RT 로 이동(`InventoryFilterView`), RT 단독 3곳(`Buyback`·`ChestConvenience`·`GearPresets`)에 LT 가드. 규칙은 `PadChordRules`. 잠금 LT+R3 유지(`InventoryConvenience`도 `LockChord` 사용). `KeyAction.inputactions` 변경 없음(코드 폴링이라). ReviewCode_1105b 필수 1·2 해소.
- AUTO-65: `Input/DpadMoveRules.cs` + `PlayerController.MoveInput()`(이동 읽기 6곳) + `QuickSlotBar` 2·3 차단. 키보드 불변.
- 검증: EditMode 792 통과(새 9 포함), 컴파일 0. **실기/가상 패드 플레이 미검증**.

## 8. 7차 큐 상태 (AUTO-75, 11-05)
- ✅ IN-2 LT+R3 중복(AUTO-64, 7b674773): 보기 필터 LT+RT 로 이동, RT 단독 3곳(Buyback·ChestConvenience·GearPresets) LT 가드. ReviewCode_1105b [필수] 2건 모두 해소.
- ✅ IN-1 십자 이동 옵션(AUTO-65, 기본 OFF). P10 해소.
- 남음: P6(미니맵 LT+L3), P11(키 설정 재지정 확대), 퀘스트 창 Enter 접기 문구 불일치(PV11, 후속 AUTO 후보). 실기 패드는 여전히 미검증(가상 패드만).

## 9. 키 설정 UI 점검 (AUTO-116, 11-05)
프로브 `Tools/probes/_keybind_ui.cs`(+임시 보조 호출), 로그 `Logs/keybind_ui.log`(gitignore). 플레이 모드에서 `InputBindings` API 직접 호출(실제 키·패드 입력 주입은 안 함, 창 입력 경로는 코드 읽기). 설정 파일 `SaveFile.es3` 백업(`Logs/cfbk`)·복원, 플레이 정지, 콘솔 에러 0.

| 항목 | 결과 | 관찰 |
|---|---|---|
| 모든 액션 행 열거 | 합격 | 14 액션(Jump Dash Run Attack SkillA/S/D/F Quick1~4 Artifact ArtifactNext) + RESET + BACK = 16행. 키보드·패드 표시 전부 출력. Quick4 패드만 "-"(기본 미지정) |
| 충돌 재바인딩 | 합격 | Jump 에 Dash 키 지정 -> `Swapped`(Dash 에 Jump 의 이전 키). 교환형이라 경고 없이 바뀜(표시 갱신) |
| 고정/예약 키 | 합격 | Esc·M·패드 Start·이동 키(←) -> `Reserved`("That key is reserved" 경고) |
| 필수 액션 미지정 방지 | 합격 | 빈/null 경로·없는 액션 -> `Invalid`, 키 제거 수단 없음(미지정 불가) |
| 저장 후 재로드 | 합격 | Commit 후 `settings.bindings` JSON 281자 저장, 새 `KeyAction` 에 로드하면 Jump=F9 로 일치 |
| 기본값 복원 | 합격 | `ResetToDefaults` 후 14 액션 키보드·패드 표시가 최초와 전부 일치, 저장 JSON 길이 0(오버라이드 삭제) |
| 패드로 행 이동 | 합격(코드·바인딩) | UI 액션 Up/Down/Left/Right = 십자 + 왼스틱, Select = 패드 A(buttonSouth)·Z. 행 이동은 위아래 순환, 좌우는 키보드/패드 열 전환 |
| 패드로 재바인딩 진입 | 합격 | `BeginRebind(Attack, 패드)` 진입 후 대기 true, 취소 시 대기 false·값 유지. 코드상 취소 키 = 패드 Start(키보드 Esc), 제한시간 8초 후 자동 취소 |
| 패드로 창 닫기 | 합격(코드) | Esc 또는 패드 Start -> `UiLock.TryBack` -> 닫기. 실제 Start 입력 주입은 미확인 |
| 기본 패드 키로 되돌리기 | **불합격** | LS(Quick1)·RS(Artifact)·Select(ArtifactNext)가 `ReservedPaths` 에 있어 다른 키로 바꾼 뒤 이 기본 키를 직접 다시 지정하면 `Reserved`. RESET TO DEFAULTS 로만 복귀 |
| Quick4 패드 키 | **불합격(약함)** | 기본 미지정이고 십자 위/아래도 `Reserved`(UI 이동용). 선택지가 face 버튼·트리거뿐이며 지정 시 다른 액션과 교환됨 |

- 후속: TechDebt T11-2(예약 목록에 든 기본 패드 키 재지정 불가), T11-3(Quick4 패드 미지정 안내 없음). 수정 금지 범위라 등록만.

## 10. 도감·퀘스트 창 패드 탐색 점검 (AUTO-117, 11-05)
프로브 `Tools/probes/_pad_nav_windows.cs`(템플릿)·실행 `Tools/pad_nav.sh <동작> [횟수]`, 로그 `Logs/pad_bestiary.log`·`Logs/pad_quest.log`(gitignore). 가상 `Gamepad` 에 `GamepadState` 를 넣어 십자·왼스틱·A(South)·B(East)·LB/RB·Start·X/Y 를 실제 입력으로 주입(창 열기는 `Toggle()` 직접 호출, 패드 열기 제스처 LT+Select·Select 길게는 미확인). 저장 백업(`Logs/cfbk`)·복원, 플레이 정지, 콘솔 에러 0. 도감 14칸 4열.

| 창 | 동작 | 결과 | 관찰 |
|---|---|---|---|
| 도감 | 십자 좌/우 | 합격 | 한 칸씩, 0 에서 왼쪽 -> 13(끝) 순환, 끝에서 오른쪽 -> 0 |
| 도감 | 십자 상/하 | 합격 | 4칸 단위. 0행에서 위 = 유지, 마지막 행에서 아래 = 마지막 칸(13) 고정(순환 안 함, 좌우와 다름) |
| 도감 | 왼스틱 | 합격 | 스틱 우 2회 = 십자 우 2회와 동일 |
| 도감 | LB/RB 탭 | 합격 | 도감 <-> 구역 완료율 전환, 커서 유지, 구역 탭에서 십자 입력 무시 |
| 도감 | A(Select) | 해당 없음 | 도감에 선택/접기 동작 없음(입력 무반응) |
| 도감 | B 닫기 | 합격 | 구역 탭에서도 닫힘. 다시 열면 도감 탭, 커서 마지막 위치 유지 |
| 도감 | Start | 불합격(약함) | 열린 상태에서 Start 로 안 닫힘(키 설정 창은 Start 로 닫힘) |
| 도감 | 닫힌 뒤 입력 | 합격 | 닫힌 상태의 Y/X/Select 입력은 상태 변화 없음 |
| 퀘스트 | 십자 좌/우 탭 | 합격 | 받을 수 있는 -> 진행 중 -> 완료 순서, 양끝에서 순환, 탭 바꾸면 커서 0 |
| 퀘스트 | 십자 상/하 | 합격 | 순환(0 에서 위 = 마지막 행, 끝에서 아래 = 0). 행 수보다 큰 횟수도 정상 |
| 퀘스트 | 왼스틱 | 합격 | 탭 전환 동일 |
| 퀘스트 | A 접기/펼치기 | 합격 | 그룹 머리 행 A -> 행 수 15->14, 다시 A -> 복원. 진행 중 탭 4->3->4. 커서는 유지 |
| 퀘스트 | B 닫기 | 합격 | 닫힘. 다시 열면 진행 중 탭 0행 |
| 퀘스트 | Start | 불합격(약함) | 열린 상태에서 안 닫힘 |
| 포커스 | 소실 | 해당 없음 | 두 창 모두 EventSystem 선택을 쓰지 않고 `triggered` 폴링(선택 오브젝트 항상 null)이라 포커스 소실 개념 없음 |

- 후속: TechDebt T11-4(도감·퀘스트 창이 Start 로 안 닫힘, 키 설정 창과 불일치), T11-5(도감 상/하는 고정·좌/우는 순환 불일치, 사소). 수정 금지 범위라 등록만.
