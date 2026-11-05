# 11-05 6차 큐 코드 커밋 정적 리뷰 (AUTO-74, 코드 수정 없음)

- 범위: 8d004636(AUTO-41)·de1e8284(46)·3e66bdef(49)·b22ee2c8(50)·4d73f6bb(51)·5bbb78c0(52)·d7eab187(53)·bf0ea30c(54)·6544dea0(55·56). 형식은 `ReviewCode_1105.md`.
- 검증: `git show` 와 현재 파일 읽기만. 플레이·EditMode 재실행 안 함. **전부 미검증**(재현 가능성은 코드 읽기 추정).
- 심각도 순, 최대 12건.

### [필수]

1. `Assets/Script/UI/InventoryFilterView.cs:64` 와 `Assets/Script/UI/InventoryConvenience.cs:47` — 둘 다 `leftTrigger.isPressed && rightStickButton.wasPressedThisFrame` 를 직접 폴링한다. 가방이 열린 기본 상태(state "")에서 LT+R3 한 번에 보기 필터 순환과 잠금 토글이 같이 발동한다(재현: 높음, 패드 필요). AUTO-49 가 R3 충돌을 정리하면서 남겼다. 대안: 한쪽을 다른 조합으로 이동(`InputAudit.md` §6, AUTO-63 가정은 필터를 LT+RT 로, 단 RT 단독 3곳에 `!lt` 가드 필요).
2. `Assets/Script/UI/Buyback.cs:85`·`ChestConvenience.cs:173`·`GearPresets.cs:117` — `rightTrigger.wasPressedThisFrame` 를 LT 가드 없이 읽는다. AUTO-46 이후 LT 가 방패 방어로 쓰이는데, 방어하며 RT 를 누르면 되사기·창고 G·프리셋 장착이 오발동할 수 있다(재현: 중간, 상점·창고·프리셋 UI 열린 상태 한정이라 전투 중엔 드묾). 대안: 세 곳에 `!pad.leftTrigger.isPressed` 추가(필터 이동 시 어차피 필요).

### [권장]

3. `Assets/Script/UI/KeyBind/PadFieldInput.cs:30-77` — 제스처 판정이 `Refresh()` 를 부르는 소비자(ArtifactSystem·NpcQuestWindow·BestiaryWindow)가 그 프레임에 살아 있어야 갱신된다. 소비자가 모두 비활성인 상태(예: 해당 컴포넌트가 든 오브젝트가 꺼진 씬)에서 SELECT 홀드 타이머가 멈춘다. 재현: 낮음. 대안: 한 곳(`DatabaseManager` 또는 전용 MonoBehaviour)의 Update 에서 Refresh 호출. 해소 AUTO-76(구동기 MonoBehaviour)
4. `Assets/Script/UI/KeyBind/PadFieldInput.cs:60-63` — LT 가 눌린 프레임에 `_select.Cancel()` 만 하고 `_selectInUi` 는 남는다. 이후 LT 를 떼고 SELECT 를 계속 누르고 있으면 `_active=false` 라 제스처는 없지만, 다음 눌림 때까지 `SelectInUi` 가 이전 값이라 다른 소비자가 읽으면 오판 여지. 재현: 낮음. 대안: Cancel 시 InUi 플래그도 초기화. 해소 AUTO-76
5. `Assets/Script/Player/AttackManager.cs`(IsLtGuardConflict) — `c.path == "/Gamepad/leftTrigger"` 문자열 비교. `InputControl.path` 는 첫 접근에 문자열을 만들고 캐시하지만 활성 스킬 슬롯마다 매 프레임 접근한다. 재현: 항상 가능하나 비용 작음. 대안: `c == Gamepad.current?.leftTrigger` 참조 비교. 해소 AUTO-77
6. `Assets/Script/Player/AttackManager.cs`(방패 장착 중 LT 억제) — 방패를 장착했지만 방어를 못 하는 상태(쿨다운 등)에도 LT 로 SkillS 를 못 쓴다. 재현: 방패 장착 + 패드 + SkillS 사용 시 항상. 의도(LT = 방어 전용)와 일치하나 `InputAudit` P1 의 "방패 없을 때 SkillS 동시 발동" 은 그대로. 대안: 의도 확인용 한 줄만 문서화.
7. `Assets/Script/DB/SaveUtil.cs` 전체 — `ES3.KeyExists` 후 `ES3.Load<T>` 라 키는 있으나 형식이 달라진 저장(예: `ArtifactId` enum 값 변경, int→다른 형)은 예외를 그대로 던진다. 기존 삼항도 동일했으므로 회귀는 아님. 호출부 5곳(`BossRetry`·`TreasureCache`·`ArtifactSystem`·`BossReward`·`ForgeState`)의 키 이름·반환 기본값은 diff 상 불변 확인. 대안: 주석에 "예외는 호출부 책임" 명시, `ExtraEquip` 식 try/catch 가 필요한 곳은 적용 제외(AUTO-68 지시와 일치).
8. `Assets/Script/Floor1Extra/TreasureCache.cs` `SaveUtil.LoadOr(SaveKey, new List<string>())` — 키가 있어도 기본 리스트를 매번 할당한다. 호출은 로드 시 1회라 영향 작음. 대안: 없음(참고 수준).
9. ✅(AUTO-81 해소: Start 음수→0) `Assets/Script/Combat/Rules/CountdownTimer.cs:Start` — `Start(seconds)` 가 음수·0 을 그대로 대입한다. `Active` 는 false 지만 `Remaining` 이 음수로 노출되어(`CounterBuff`·`MobReflect` HUD가 Remaining 을 쓰면) 음수 표시 가능. 현재 호출은 `Duration`(양수)뿐. 대안: Start 에서 `Mathf.Max(0f, seconds)`. `Tick` 은 dt<=0 이어도 감소 후 0 하한이라 경계(정확히 0)는 안전.

### [참고]

10. `Assets/Script/DB/Floor2Data.cs:56-68` — `HeaderMatches` 가 base 앞 3줄 중 어느 줄과도 같으면 통과한다. 초안 헤더가 타입 행과 같은 문자열이면(현실적으로 불가) 잘못 통과. 본질적 위험 없음. 같은 파일의 병합 거절은 `Debug.LogError` 라 플레이 중 한 번 찍히고 원문 반환(안전).
11. `Assets/Script/UI/Bestiary/BestiaryWindow.cs`(AUTO-50) — LB/RB 를 `Gamepad.current` 로 직접 읽는다. 도감이 열려 있는 동안 `UiLock` 으로 필드 입력이 막히는지에 의존. 열림 중 스킬 A/D(LB/RB) 동시 발동 여부는 `AttackManager` 의 `isOpenUI` 검사에 달렸다(코드상 `isOpenUI == false` 일 때만 무기 처리). UiLock 이 `isOpenUI` 를 올리는지는 이번 점검에서 미확인.
12. `Assets/Script/Localization/LocUi.cs`·`Assets/Resources/I2Languages.asset`(AUTO-55) — 삭제한 번역 4종(`U: buy back last sold item`, `U Pad RT: buy back top`, `Crystal refund on 9`, `HAMMER!`)을 `Assets/Script` 에서 grep 한 결과 .cs 직접 참조는 0 이다. 프리팹·씬의 I2 Localize 컴포넌트가 term 을 가리키는지, `LocUi` 의 동적 키 조합(문자열 연결)으로 조회하는지는 미확인. 재현: 낮음. 대안: 프리팹 YAML 에서 term 이름 grep 1회.

### 확인 결과(문제 없음)

- `UiText.SetIfChanged`: `t == null` 가드 있고 `NeedsSet` 이 null 을 빈 문자열로 취급해 null 대입도 안전. 호출 3곳(`LocUi`·`CraftPinHud`·`MinimapHud`) 모두 `TMP_Text` 확장 호출.
- `ShieldGuard`/`PlayerGuard` InputAction Dispose 누락(전 리뷰 #9)은 이번 범위가 아니라 재확인하지 않음.
- 입력 바인딩 추가: `PortalOn` 에 `<Gamepad>/dpad/up` 1개 추가(KeyAction.cs 동기 생성). 같은 맵의 다른 액션과 dpad/up 중복은 파일 grep 상 Player 맵 안에서 `PortalOn` 이외 확인 못 함(UI 맵의 dpad/up 은 별도 맵).
