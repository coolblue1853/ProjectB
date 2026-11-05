# 세이브 마이그레이션 안전 점검 (11-05, AUTO-80)

읽기·grep 점검이며 코드는 수정하지 않았다. 원칙은 `Framework/CodeStyle.md` §3: ES3 필드 이름은 바꾸지 않고 추가만 한다. 기본값 판정은 정적 읽기이고 실제 구세이브 로드는 미실행(Unity 미사용).

## 1. 읽기 방식 (안전 판정의 기준)

| 방식 | 키 없을 때 | 형식이 다를 때 |
|---|---|---|
| `SaveUtil.LoadOr(key, 기본)` (`DB/SaveUtil.cs`) | 기본값 | **예외**(호출부 책임, ReviewCode_1105b #7) |
| `ES3.KeyExists(k) ? ES3.Load<T>(k) : 기본` | 기본값 | **예외** |
| `ES3.Load<T>(k, 기본)` | 기본값 | 예외 |
| `ES3.LoadInto(Datas)` (`SaveManager`) | 필드 유지(기본값) | 예외 가능, 필드 단위 보존 |

## 2. ES3 키 전수 표

슬롯 파일 = `SaveSlots.PathOf(slot)`(슬롯1 = `SaveFile.es3`, 이하 `SaveSlotN.es3`). "공유" = `SaveSlots.SharedFile`(슬롯1 파일과 같은 이름이라 슬롯1 세이브와 한 파일에 섞임).

| 키 | 소유 클래스 | 타입 | 기본값 | 키 없음 안전 | 형식 변경 위험 |
|---|---|---|---|---|---|
| `Datas` | `DB/SaveManager` | `Datas` 클래스(money·pos·time·레벨 등) | `LoadInto` 라 필드 유지 | O | 필드 추가는 안전, 이름 변경 시 값 유실 |
| `saveVersion` | `DB/SaveSlots` | int(1) | 없으면 1 기록 | O | 마이그레이션 기준점, 아직 분기 없음 |
| `explorer.rank` | `Combat/AscentLoad` | int | 0, Clamp 0..Max | O | 낮음 |
| `explorer.seals` | `Combat/ExplorerSeal` | int | 0, Clamp | O | 낮음 |
| `story.opening` | `Dialogue/DialogueBoot` | bool | false | O | 낮음 |
| `<boss>.seen` 류 `_seenKey` | `Enemy/BossEncounterTalk` | bool | 없음=false | O | 키 이름이 보스 id 에 의존 |
| `excavation.records` | `Excavation/FathersRecords` | int 비트마스크 | 0 | O | 비트 의미 변경 금지 |
| `fishing.rod` | `Fishing/FishingSave` | int | 0 | O | 낮음 |
| `f1.bestiary.claimed` | `Floor1Extra/BestiaryMilestones` | `List<string>` | 빈 목록 | O | id 문자열 의존 |
| `f1.day` | `Floor1Extra/BossRetry` | int | 1 | O | 낮음 |
| `<보스키>`(`Key(boss)`) | `Floor1Extra/BossRetry` | string `일:횟수` | 없음 | O | `Split(':')` 파싱 예외 가능(손상 문자열) |
| `f1.bounty` | `Floor1Extra/BountyBoard` | `BountyState` 클래스 | 새 상태 | O | 클래스 필드 이름 변경 시 값 유실 |
| `f1.caches` | `Floor1Extra/TreasureCache` | `List<string>` | 빈 목록 | O | 낮음 |
| `WeaponTrial` 키(`Key(stage)`) | `Floor1Extra/WeaponTrial` | int 상태(NotStarted/Rented/Done) | NotStarted | O | 상태 값 의미 변경 금지 |
| `equip.extra` | `Inventory/Model/SlotRules`·`UI/ExtraEquip` | `string[]` | null | O | 슬롯 수 변경 시 길이 불일치(§3) |
| `equip.extraUnlock` | 동일 | int 마스크 | 0 | O | 비트 의미 변경 금지 |
| `artifact.unlocked` | `Item/ArtifactSystem` | `List<ArtifactId>` | 없음 | O | **enum 값 순서 변경 시 의미 바뀜** |
| `artifact.equipped` | 동일 | `ArtifactId` | `HealingDew` | O | 동일 |
| `<보스>.first`·`<킬키>` | `Item/BossReward` | bool·int | false·0 | O | 낮음 |
| `enhanceFailStreak` | `Item/EnhanceService` | int | 0 | O | 낮음 |
| `forge.unlock`·`forge.pity` | `Item/ForgeState` | int | 0 | O | 낮음 |
| `npcq.state` | `NpcQuest/NpcQuestLog` | string(직렬화 문자열) | `""` | O | try/catch 있음(425~430), 포맷 버전 필드 없음 |
| `npcq.tracked` | `NpcQuest/NpcQuestTrack` | string | null | O | 낮음 |
| `npcq.folded` | `NpcQuest/NpcQuestZone` | string | `""` | O | 구역 이름 문자열 의존 |
| `movement.highestFloor` | `Player/State/MovementUnlocks` | int | 1, Clamp 1..7 | O | 낮음 |
| `movement.broochStage` | 동일 | int | 0, Clamp | O | 낮음 |
| `glide.unlocked` | `Glide/GlideMotor`(읽기는 MovementUnlocks) | bool | false | O | 구 세이브 호환 유지용, 이름 변경 금지 |
| `qm.board`·`qm.elder`·`qm.journal` | `Quest/QuestBoard` | string | 키 있을 때만 적용 | O | 포맷 변경 시 `ApplyData` 예외 |
| `bestiary.kills` | `UI/Bestiary/BestiaryData` | `Dictionary<string,int>` | 빈 사전 | O | 몹 id 이름 변경 시 유실 |
| `craftpin` | `UI/CraftPinHud` | string | `""` | O | 낮음 |
| `hints.seen` | `UI/FirstHints` | string | null | O | 낮음 |
| `gear.presets` | `UI/GearPresets` | `string[]` | null→빈 | O | 낮음 |
| `quickslot.items` | `UI/QuickSlot/QuickSlotBar` | `string[]` | null→빈 | O | 아이템 이름 변경 시 유실 |
| `zones.visited` | `UI/WaypointMap` | `List<string>` | 빈 | O | 낮음 |
| `settings.volume/shake/flash/hints/pickup/dpadMove` | `UI/GameSettings` | float/float/bool/bool/int/bool | 1/1/true/true/0/false | O | 공유 파일. `pickup` enum 캐스트 범위 확인 없음 |
| `settings.bindings` | `UI/KeyBind/InputBindings` | string(JSON) | `""` | O | try/catch 있음(75~79,185~189,204~208) |
| `settings.minimap` | `UI/Minimap/MinimapHud` | bool | true | O | 낮음 |
| `settings.lang` | `Localization/Loc` | **PlayerPrefs**(ES3 아님) | — | — | 세이브 슬롯과 무관 |

미확인: 인벤토리·상자 저장(`UI/InventoryManager.SaveInventory`)은 이번 grep 에서 `ES3.` 직접 호출이 없어 `Datas` 내부 필드나 `Chest` 쪽 저장으로 보인다. 별도 점검 필요. `EnemySpawner`의 `enemy`/`dead`/`minion` 은 상수만 확인(런타임 풀 키로 보이며 ES3 저장 아님).

## 3. 신규 필드 구세이브 로드 시나리오

| 신규 | 구세이브 상태 | 결과(정적) | 위험 |
|---|---|---|---|
| EquipSlotExpansion 4슬롯 (`equip.extra` `string[]`, `equip.extraUnlock` int) | 두 키 없음 | `LoadOr` 기본 null/0 → 잠금·빈 슬롯 | 없음 |
| 위 + 이후 슬롯 수 증가 | `string[]` 길이 4 | 모델이 길이 불일치를 어떻게 다루는지 `SlotRules.cs` 의 모델 복원 코드 확인 필요 | 중 |
| 위 + 읽기 예외 | 키는 있으나 형식 다름 | `ExtraEquip.cs:160~172` try/catch → `_saveBlocked` 로 저장 차단(덮어쓰기 방지) | 낮음(방어 있음). 단 `SlotRules.cs:94` 의 `LoadOr` 는 try/catch 밖 |
| 퀘스트 접기 `npcq.folded` | 키 없음 | `""` → 모두 펼침 | 없음 |
| `settings.bindings` | 키 없음/손상 | `""`/예외 후 기본 바인딩 | 없음(방어 있음) |
| `settings.dpadMove`·`settings.pickup` | 키 없음 | false / 0 | 없음 |
| 슬롯1 = 공유 파일 | 설정 키와 슬롯1 세이브가 같은 `SaveFile.es3` | 슬롯1 삭제·교체 시 설정 유실 또는 슬롯2에 설정 비공유 | 중(설계 의도 확인 필요) |

## 4. 예외 처리 위치

- 방어 있음: `SaveSlots.VerifyAndBackup`(전체 값 읽기 검증 + 백업 복구), `ExtraEquip` 로드, `InputBindings`, `NpcQuestLog`.
- 방어 없음(예외 전파 시 해당 시스템 초기화 실패): `SaveUtil.LoadOr` 호출부 — `BossRetry`·`TreasureCache`·`ArtifactSystem`·`BossReward`·`ForgeState`·`SlotRules.cs:94`, 그리고 `ES3.KeyExists ? Load` 직접 사용처(`BestiaryMilestones`·`BountyBoard`·`BestiaryData`·`WaypointMap`·`QuestBoard`·`GameSettings` 등).
- 연결: ReviewCode_1105b #7 — "예외는 호출부 책임" 주석 명시 또는 `SaveUtil.TryLoad` 추가가 대안.

## 5. 위험 항목별 수정 후보 (건별 1줄)

1. ✅(AUTO-100) `SaveUtil.LoadOr` 형식 불일치 예외: `LoadOr` 내부 try/catch 후 기본값 반환 + 경고 로그(저장 차단이 필요한 호출부는 별도 `TryLoad`).
2. ✅(AUTO-100: 이미 try/catch 안, 저장 차단 호출부는 `LoadOrThrow`) `SlotRules.cs:94` try/catch 밖 `LoadOr`: `ExtraEquip` 처럼 감싸거나 1번으로 해결.
3. ✅(AUTO-101) `artifact.unlocked/equipped` enum 의존: `ArtifactId` 에 "순서 변경 금지, 맨 뒤 추가" 주석 + 값 고정 테스트.
4. ✅(AUTO-101, `ParseKills`) `Floor1Extra/BossRetry` `Split(':')` 파싱: 길이 확인 후 기본값.
5. ✅(AUTO-101, `SerializedClasses_FieldNamesFixed`) `BountyState` 등 클래스 직렬화: 필드 이름 고정 목록을 `SaveCompatTests.KeyNames_StillInSource` 에 추가.
6. 공유 설정 파일 = 슬롯1 파일: 설정을 별도 `Settings.es3` 로 분리하는 안과 현행 유지 결정(OpenQuestions 후보).
7. `saveVersion` 은 기록만 하고 분기가 없음: 첫 비호환 변경 때 `if version < N` 마이그레이션 훅 추가.
8. ✅(AUTO-100: 기존 `Enum.IsDefined` 가드 확인) `GameSettings` `pickup` int→enum: 범위 밖 값 Clamp.
9. 인벤·상자 저장: 키·타입 표 추가 점검(미확인).

## 6. 필요 테스트 목록

기존: `SaveCompatTests`(`KeyNames_StillInSource`·`OldSave_MissingKeys_LoadDefaults`·`OldSave_DefaultsAreSafe`), `SaveMapperTests`, `SaveUtilTests`.

추가 후보(EditMode, 임시 ES3 파일):
- `SaveUtil.LoadOr` 형식 불일치(int 키에 string 저장) 동작 고정.
- 위 §2 키 이름 목록 전체 소스 존재 검사(신규 `equip.extra*`·`npcq.folded`·`settings.bindings` 포함).
- `equip.extra` 길이 3/4/5 로드 시 모델 크기 안전.
- `BossRetry` 손상 문자열(`"abc"`, `"1"`) 파싱.
- `ArtifactId` 정수 값 고정(`HealingDew` 등).
- `saveVersion` 없는 파일 로드 후 1 기록.

## 9차 반영 (11-05, AUTO-105)
- ✅ §5 #1~5·#8 (AUTO-100 9aaedfd1, AUTO-101 8c06d65d): `LoadOr` 기본값+경고, 저장 차단 호출부 `LoadOrThrow`, `ArtifactId` 값·`BountyState`/`Datas` 필드명 고정, `BossRetry.ParseKills`.
- 남음: #6 공유 설정 파일 분리 결정, #7 saveVersion 분기 훅, #9 인벤·상자 키 점검(미확인).
