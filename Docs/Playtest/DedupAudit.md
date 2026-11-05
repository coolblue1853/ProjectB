# 중복 코드 정리 후보 (AUTO-45, 11-05)
조사만, 코드 수정 없음. 방법: `Assets/Script/**` grep(`Tick(float`, `Time.time` 만료, `ES3.KeyExists ? ES3.Load`, `.text =`). 위험도: 세이브 호환 영향 여부 기준(상=세이브 키/형식 변경 가능, 중=동작 동일 리팩터지만 테스트 필요, 하=표시/내부).
읽은 규약: `CodeStyle.md`, `.claude/rules/csharp.md`(공통 헬퍼는 순수 클래스+EditMode 테스트).

| # | 패턴 | 위치(파일:줄) | 제안 공통 헬퍼 | 위험도 |
|---|---|---|---|---|
| 1 ✅(8곳 적용: +ExtraEquip·SlotRules·NpcQuestLog, 나머지 20곳 후속) | `ES3.KeyExists(k) ? ES3.Load<T>(k) : 기본값` 28곳 (ES3 의 `Load<T>(key, default)` 와 동일) | `BossRetry.cs:21`, `TreasureCache.cs:33`, `ExtraEquip.cs:165-166`, `SlotRules.cs:93-94`, `ArtifactSystem.cs:78`, `BossReward.cs:83`, `ForgeState.cs:17,24`, `NpcQuestLog.cs:440` 외 | `SaveUtil.LoadOr<T>(key, def)` (ES3 슬롯 파일 인자 처리도 한 곳에) | 하(키·형식 불변) |
| 2 ✅(AUTO-82 확인: 미적용 0곳, 적용 7곳 전부: CounterBuff·MobReflect·MobDebuff 둔화·SleepRules·TomePage·AmmoMagazine·ThrownSpear) | 남은 시간 `Mathf.Max(0, _remaining - dt)` + `Mathf.Max(_remaining, seconds)` 갱신형 타이머 | `CounterBuffRules.cs:23`, `MobDebuffRules.cs:41`(`_slowRemain`), `MobReflectRules.cs:20`, `TomePage.cs:27-33`(`_cooldown`), `AmmoMagazine.cs:36`, `ThrownSpear.cs:49`(`_trapRemaining`), `SleepRules.cs:32` | 구조체 `CountdownTimer { Start, Extend(max), Tick(dt), Remaining, Active }` | 하(순수 규칙, EditMode 테스트로 보호) |
| 3 ✅(`Cooldown` 구조체 + BestiaryMilestones·RareColony·NightRaid·ContactDamage·TempMobDash 5곳 적용, 나머지 ChargeShot 은 제외: 경과시간 `Time.time-_pendingAt` 비교와 `<=` 경계 유효시각이라 Cooldown(>=) 의미와 다름) | `Time.time + 간격` 다음 허용 시각 패턴 | `NightRaid.cs:195,286,334`, `BestiaryMilestones.cs:114-118`, `ContactDamage.cs:67-71`, `RareColony.cs:152-156`, `Enemy/TempMobDash.cs:57-111`, `ChargeShot.cs:52-84` | `Cooldown { Ready(now), Arm(now, interval) }` (순수, 시각 주입) | 하 |
| 4 ✅ | 임시 `OnGUI` HUD 3종 (같은 좌표 `Rect(20,120,220,24)`) | `AmmoGimmick.cs:36`, `ThrownSpearGimmick.cs:44`, `TomePageGimmick.cs:26` | `GimmickHud.Draw(string)` 한 컴포넌트(좌표 1곳) — 그래픽 확정 시 어차피 교체 | 하 |
| 5 ✅ | 문자열 갱신 시 이전 값과 비교 후 `.text =` 대입(`_lastText` 류) | `NpcQuestTracker.cs`(5곳), 그 외 UI `.text =` 214곳 중 매 프레임 갱신 후보 `InventoryFilterView.cs`, `CraftPinHud.cs`, `MinimapHud.cs` | `UiText.SetIfChanged(TMP_Text, string)` 확장 | 하(GC·재레이아웃 절약, 표시만) |
| 6 | `"day:count"` 식 문자열 직렬화 + `Split(':')` 파싱 | `BossRetry.cs:128-142`, `GearPresetModel.cs:103`, `NpcQuestLog.cs:402`, `MiniBossTable.cs:62` | `PackedPair.Parse/Format`(TryParse 검증 포함) | **상**: 기존 저장 형식 그대로 유지해야 함. 공통화해도 문자열 포맷은 건드리지 말 것 |
| 7 | `Keyboard.current` / `Gamepad.current` null 검사 + `wasPressedThisFrame` 조합 반복 | `BagExpansion.cs:63-64`, `Buyback.cs:72-85`, `InventoryFilterView.cs:63-64`, `InventoryConvenience.cs:46-52`, `ShopConvenience.cs:77-84`, `DismantleInput.cs:54`, `CraftingManager.cs:754`, `NpcQuestWindow.cs:107` | `PadKeys.Pressed(Key, padButton, requireLT)` 입력 도우미 (InputAudit 의 충돌 정리와 함께) | 중(입력 순서·UI 잠금 영향. 프로브 회귀 필요) |
| 8 | 규칙 클래스에 `Load`/`Save` 쌍 + 세이브 키 상수 + 슬롯 처리 반복 | `BossRetry.cs`, `WeaponTrial.cs:118,216,255`, `TreasureCache.cs:33-48`, `BountyBoard.cs:46-164`, `ZoneProgress.cs` | 키별 `SaveField<T>(key)`(Get/Set/Exists) 래퍼 | **상**: 키 이름·형식 불변 필수(`CodeStyle.md` §3), 슬롯 전환 시 캐시 무효화 동작 보존 |

## 제안 순서
1. #1·#4: 기계적, 위험 최저.
2. #2·#3: 순수 타이머/쿨다운 구조체 + 테스트, 이후 규칙 클래스를 점진 교체.
3. #5: UI 갱신 경로, 프로파일러로 효과 확인 후.
4. #7: 입력 정리(InputAudit)와 한 묶음.
5. #6·#8: 세이브 형식 영향이 있어 마지막, 기존 세이브 로드 테스트 필수.

## 참고
- 확인 못 한 것: 호출 빈도(실제 GC 영향)는 미측정. 위 줄 번호는 11-05 시점.
- 건드리지 말 것: 서드파티·자동 생성(`KeyAction.cs`).

#5 적용: `UiText.SetIfChanged` 신규, `CraftPinHud`·`MinimapHud` 적용. `InventoryFilterView` 는 TMP 가 아니라 OnGUI 라벨이라 대상 아님(OnGUI 는 GUI 교체 시 정리).

## 7차 큐 종료 상태 (AUTO-75, 11-05)
- ✅ #1 SaveUtil.LoadOr(AUTO-53·68), #2 CountdownTimer(AUTO-54·66·67), #3 Cooldown(AUTO-69), #5 UiText.SetIfChanged(AUTO-56), GimmickHud 단일화(AUTO-52).
- 보류: LP-6 StatusTint 자식 탐색 캐시는 불가 사유 기록(AUTO-70). 나머지 후보는 효과 작아 보류.
