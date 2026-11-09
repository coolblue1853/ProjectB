# 퀘스트 시스템 통합 설계 (REWORK-5, 11-09)

범위: NPC 퀘스트·게시판 퀘스트·사냥꾼 의뢰/현상 3계통을 **한 데이터 모델·한 창(Q)·한 추적 HUD**로 합친다. 입구(NPC·게시판·푯말)는 같은 목록의 필터일 뿐이다. 콘텐츠(유형 정의·대사·층계별 줄기)는 `REWORK-6`(`Game/QuestDesign.md`)에서 확정하고, 이 문서는 틀과 빈칸만 둔다. 구현 현황은 §7 아래 `7-2`에 있다(P0·P1 완료, P2~P5 대기).

관련: `Docs/Playtest/ConventionAudit.md` U6·U6a·U7·U8·U8b·U13, `Game/Floor1Completion.md` §4-1(신규 퀘스트 29건)·§12, `Game/TowerWorld.md`(세계관), `Docs/Handoff/queue.md` REWORK-5·6·BUG-U7.
규칙: 모든 UI는 마우스·키보드·게임패드 3경로. 그래픽 제작 금지(텍스트·기존 UI·기존 아이템 아이콘 재사용). 기존 CSV 컬럼은 **맨 뒤에만 추가**, ES3 기존 키 이름 유지.

---

## 1. 3계통 현황 비교 (코드 확인 11-09)

| 항목 | ① NPC 퀘스트 | ② 게시판 퀘스트 | ③ 사냥 의뢰·현상 |
|---|---|---|---|
| 코드 | `NpcQuest/` : `NpcQuestCatalog`·`NpcQuestLog`·`NpcQuestWindow`·`NpcQuestTrack`·`NpcQuestTracker`·`NpcQuestRewards`·`NpcQuestGiver`·`NpcQuestDriver` | `Quest/QuestBoard.cs`·`QuestCatalog.cs` + 플러그인 Quest Machine(`PixelCrushers.QuestMachine`) | `Floor1Extra/` : `BountyKiosk`(푯말)·`BountyPanel`(UI)·`BountyBoard`(상태)·`BountyRules` |
| 데이터 원본 | `Resources/QuestsFloor1.csv` 13열(`id,title,giver,minLevel,requires,objective,target,count,exp,gold,rewardItem,rewardCount,description`) | **C# 하드코딩** `QuestCatalog.Chain`(사냥 체인)·`Story`(촌장)·부가 3종(`s_wood`·`s_herb`·`s_craft`). 영문 문자열 `Loc.T` | `Resources/Floor1Bounties.csv` (`id,zone,title,targets,count,minLevel,exp,gold,item,itemCount`) |
| 목표 타입 | `NpcQuestObjective` 9종: Talk·Kill·Collect·Craft·Visit·KillElite·Equip·Enhance·Bestiary. 한 퀘스트 목표 1개 | 메시지 카운터(`Kill`/`Pickup`/`Craft`) + 대상 몬스터명 배열. 목표 1개 | 처치(몬스터 id 배열 매칭 `BountyDef.Matches`) 1종 |
| 진행 판정 | `MessageSystem` "Kill"/"Craft" → `NpcQuestDriver` → `NpcQuestLog.OnKill` 등. 수집은 **인벤 보유량 실시간**(`InventoryManager.CountItem`), 장비·강화·도감은 즉석 계산 | Quest Machine 카운터가 **메시지 수신 시점부터 0에서 +1**. 수락 전 보유분은 반영 안 됨 | `BountyRules.OnKill` 이 `BountyState.Progress` 증가 |
| 선행·레벨 | `requires`(id 1개)·`minLevel` → `NpcQuestStatus` Locked/LevelLow/Available/Active/Ready/Done | `QuestStateQuestCondition` 으로 선행 완료 후 게시판에 등장. 레벨 조건 없음 | `minLevel` 만. 하루 3건 `PickOffers`, **동시 1건**만 수락 |
| 보상 | exp·gold·item 1종(`NpcQuestRewards.Apply`, 가방 공간 검사 `CanReceive`) | gold·`stones`(강화석)·exp·rewardItem·`artifact`(아이템 해금) | exp·gold·item(`Floor1Data.Reward`) |
| ES3 키 | `npcq.state`(`id:상태:진행;…`)·`npcq.tracked` | `qm.board`·`qm.elder`·`qm.journal`(Quest Machine 직렬화 문자열) | `f1.bounty`(`BountyState`: Day·Offered·Done·Active·Progress) |
| UI | `NpcQuestWindow`(Q 키, 탭 받을 수 있는/진행 중/완료, 구역 묶음, Z 추적) | Quest Machine 대화 UI(`DialogueBoot`) — 영문 ACCEPT/DECLINE, 흰 네모 아이콘 | `BountyPanel`(푯말에서 ↑로 열림) |
| 추적 HUD | `NpcQuestTracker`(우측 박스, 최대 3개 `NpcQuestTrack.Max`) | Quest Machine HUD(화면 가운데 흰 글씨 `퀘스트 게시판에 보고`, 진행도 없음) → U8b 원인 | `BountyBoard.TrackerText()` 한 줄을 `NpcQuestTracker` 가 앞에 이어 붙임 |
| 입구 | `NpcQuestGiver` 머리 위 !/?, `NpcMenu` Kind.Quest(REWORK-8 완료) | 게시판 오브젝트 ↑, 촌장은 Dialogue System 선택지 | 푯말 ↑ |

### 1-1. 이미 된 통합 (재사용)
- `NpcQuestWindow` 가 Q 키·3탭·`QuestListRules.Tab`·Z 추적을 이미 제공한다. `QuestBountyLink`(`bounty:` 접두 id, `NpcQuestDef` 어댑터)로 현상 의뢰를 **읽기 전용**으로 같은 목록에 보여 준다. → 새로 만들지 않고 이 창을 확장한다.
- `NpcMenu`(`Dialogue/NpcMenu.cs`, `NpcMenuRules.Kind.Quest`)가 NPC 선택지 입구를 이미 제공한다.

### 1-2. 남은 문제
1. 게시판 계통(Quest Machine)이 창·HUD·세이브 모두 따로다 → U6·U6a·U8b 원인.
2. 현상 의뢰는 창에서 보기만 가능하고 수락·보고는 푯말에서만. 진행 저장이 `f1.bounty` 별도.
3. 추적 HUD 가 NPC 3개 + 현상 1줄 + 세트 1줄 + (Quest Machine 가운데 글씨)로 갈라져 있고 현상은 `[현상]` 접두 문장 형식이 다르다 → U8.
4. 데이터 한계: 목표 1개, 요약 대사는 `description` 한 칸, 보고 대상은 `giver` 와 동일 고정, 유형(메인/사이드/보상) 없음.
5. **U7 가설(원인 추정, 재현은 BUG-U7 담당):** NPC 수집(`NpcQuestLog.Progress`)은 인벤 보유량을 직접 읽어 0/N 문제가 없다. 사용자가 본 땔감·치유사 부탁(`s_wood`·`s_herb`)은 **게시판 계통의 `Pickup` 메시지 카운터**라 수락 시점부터 0에서 시작한다. → 통합(P4)에서 보드 부가 3종을 NPC 계통 `Collect` 로 옮기면 자연히 해결. 그 전 임시 수정(BUG-U7)은 Quest Machine 카운터 초기값을 수락 시 `CountItem` 으로 세팅.

---

## 2. 통합 데이터 모델

### 2-1. 원칙
- **단일 진실원 = `QuestsFloor1.csv`(→ 층계가 늘면 `QuestsFloor2.csv` … 같은 스키마, 카탈로그는 `ResourceName` 목록으로 병합)**. `NpcQuestDef` 를 공통 `Quest` 정의로 승격(클래스명은 유지, 필드만 추가. 이름 변경은 후속).
- **상태 저장 = `npcq.state`**(기존 형식 `id:상태:진행;…` 유지). 상태 값은 기존 1 진행·2 완료에 **3 포기 후 재수락 가능은 미도입**(포기는 레코드 삭제로 처리).
- 반복(일일) 퀘스트·현상은 상태가 날짜에 의존하므로 별도 키를 계속 쓴다: `f1.bounty` 는 **유지**(일일 3건 제비뽑기·동시 1건·`Done` 목록). 정의만 CSV 통합 대상이 아니라 어댑터로 통합(§2-4).

### 2-2. 공통 Quest 정의 (CSV 기존 13열 + 뒤에 추가)

| 필드 | CSV 열 | 타입 | 설명 / 기본값 |
|---|---|---|---|
| id | `id` (기존) | string | 고유. 현상은 `bounty:` 접두 |
| title | `title` (기존) | string | 제목 |
| giver | `giver` (기존) | npcId | 수락 NPC 또는 입구 id(`questboard`·`elder`·`bountyboard`) |
| minLevel / requires | (기존) | int / id | 레벨·선행 1개 |
| objective·target·count | (기존) | enum·string·int | 1번 목표 |
| exp·gold·rewardItem·rewardCount | (기존) | | 보상 |
| description | `description` (기존) | string | **요약 대사 = 창 우측 상단 한 줄 요약**(예: 재료가 떨어졌다는 식의 의뢰 사정). 평문·쉼표 없음 |
| source | `source` **(신규)** | enum | `npc`·`board`·`bountyOffer`(사냥 의뢰)·`bounty`(현상). 빈칸 = `npc` |
| kind | `kind` **(신규)** | enum | `main`(메인 가이드)·`side`(세계관 사이드)·`reward`(보상용). 빈칸 = `reward` |
| reportTo | `reportTo` **(신규)** | npcId | 완료 보고 대상. 빈칸 = `giver`. 푯말·게시판이면 입구 id |
| offerLine | `offerLine` **(신규)** | string | 수락 대사 (REWORK-6 형식틀 §8) |
| progressLine | `progressLine` **(신규)** | string | 진행 중 NPC 에게 다시 말 걸 때 대사 |
| doneLine | `doneLine` **(신규)** | string | 완료(보고) 대사 |
| zone | `zone` **(신규)** | string | 목표 구역 태그(현재 `NpcQuestZone.Of(d)` 가 파생하는 값을 CSV 로 올림, 빈칸이면 파생 유지) |
| stones | `stones` **(신규)** | int | 강화석 보상(게시판 `Def.stones` 이행용). 기본 0 |
| artifact | `artifact` **(신규)** | string | 완료 시 해금 아티팩트 id(게시판 `Def.artifact` 이행용). 빈칸 = 없음 |
| repeat | `repeat` **(신규)** | enum | `none`·`daily`. 일일은 현상만 사용 |
| sort | `sort` **(신규)** | int | 같은 구역·상태 안 정렬 가중(없으면 `id` 순) |
| target2·count2 | `objective2`·`target2`·`count2` **(신규, P5 이후)** | | 요구 사항 2번째 줄. 처음엔 열만 예약, 판정은 후속(§3 UI 는 목록형이라 확장 즉시 가능) |

주의: ① CSV 파서(`NpcQuestCatalog.Parse`)는 열 수가 모자라는 행을 빈칸으로 처리하게 고친다(구 CSV 호환). ② 열 추가는 `Game/ItemSpec.md` 가 아니라 이 문서 표가 기준이다(퀘스트 CSV 스키마). ③ 평문에 쉼표 금지(기존 규칙). ④ 대사 열은 비어 있으면 `description` 으로 대체해 데이터 이행 중에도 창이 깨지지 않게 한다.

### 2-3. 목표 타입 (확장 ★ = 신규)
기존 9종 유지. 추가 후보(★, 로직 소량): `Deliver`★(아이템을 NPC 에게 전달, `Collect` + reportTo 다른 NPC 로 표현 가능하므로 **새 타입 없이 처리**), `Escort`·`Defend` 는 범위 밖. 즉 P0~P5 에서 목표 타입 추가는 없다.

판정 통일: 수집류(`Collect`)는 항상 `ItemCounter` 실시간 보유량(수락 즉시 반영, U7). 나머지는 `NpcQuestLog.Progress` 규칙 그대로. `MessageSystem` 의 "Pickup" 카운트 방식은 폐기.

### 2-4. 3계통 이행 매핑

| 기존 | 통합 후 | 방법 |
|---|---|---|
| NPC 퀘스트(`QuestsFloor1.csv`) | `source=npc` | 열 추가만. 기존 행 `kind` 는 REWORK-6 재분류표에서 채움(그전엔 빈칸=reward) |
| 게시판 `QuestCatalog.Chain`(5~6건 체인) | `source=board`, `giver=questboard` | C# 정의를 CSV 행으로 옮김(id 유지: `q_rabbits`·`q_moles` …). 영문 텍스트는 한글로 재작성. `stones`·`artifact` 열 사용 |
| 게시판 `Story`(촌장) | `source=npc`, `giver=elder`, `kind=main` 후보 | 동일 |
| 게시판 부가 `s_wood`·`s_herb`·`s_craft` | `Collect`/`Craft` + `source=board` | `Pickup`→`Collect`(U7 해소) |
| 현상 `Floor1Bounties.csv` | **정의는 그대로**, 어댑터 `QuestBountyLink` 확장 | 읽기 전용 → 수락·포기·보고 가능(§2-5). 정의 17건 CSV 이동은 하지 않음(일일 제비뽑기 로직 보존) |

### 2-5. 단일 진입 API (`QuestService`, 새 정적 클래스 `NpcQuest/QuestService.cs`)
```
static IReadOnlyList<NpcQuestDef> All();                        // CSV + 현상 어댑터 합산
static QuestView Query(QuestFilter f);                         // 탭·source·giver·zone 필터, 정렬 포함
static bool CanAccept(NpcQuestDef d, out string reason);       // 레벨·선행·동시 슬롯(현상 1)·가방
static bool Accept(string id);        // bounty: 접두면 BountyBoard.Accept, 아니면 NpcQuestLog.Accept
static bool Abandon(string id);       // 진행 기록 삭제(현상은 Active 해제, Offered 유지)
static bool Report(string id);        // 완료 보상(Complete / BountyBoard.TryReport), 가방 검사 동일
static string Objective(NpcQuestDef d);        // '대상 이름 : 현재/필요' 한 줄
static string ReportTarget(NpcQuestDef d);     // 보고할 NPC·입구 이름
```
`NpcQuestLog`·`BountyBoard` 는 내부 저장소로 남고, UI·입구·HUD 는 `QuestService` 만 부른다.

### 2-6. ES3·이행 (세이브 호환)
- 새 키 1개: `quest.migrated`(int 버전). 기존 키 `npcq.state`·`npcq.tracked`·`f1.bounty`·`qm.*` 는 **이름·형식 유지, 삭제 금지**.
- 이행(로드 직후 1회, `QuestMigration.RunOnce()`): `qm.journal`/`qm.board`/`qm.elder` 에 **완료(Successful)** 인 board 퀘스트 id → `npcq.state` 에 `id:2:0` 추가. **진행 중** 인 것은 `id:1:진행값` 으로 추가(처치 카운터 값 사용, 수집류는 실시간이라 0). 성공 후 `quest.migrated=1` 저장. Quest Machine 직렬화 해석은 `QuestJournal` 로드 후 `GetQuestState(id)` 로 읽는다(문자열 직접 파싱 금지).
- 이행 실패(예외)는 로그 `[PB][Quest]` 후 건너뛰고 다음 시작 때 재시도. 기존 세이브가 깨지지 않는지 EditMode 테스트(§6)로 확인.
- 롤백: `quest.migrated` 키만 지우면 재이행. `qm.*` 를 그대로 두므로 구 빌드로 돌아가도 데이터 유지.

---

## 3. 퀘스트 창 (Q 키, 메이플식) — `NpcQuestWindow` 확장

현재 창(Q·3탭·구역 묶음)을 유지하고 **우측 상세 패널**을 메이플식으로 채운다. 그래픽은 기존 코드 생성 UI(텍스트 TMP·사각형) 그대로.

### 3-1. 레이아웃
```
[ 받을 수 있는 N | 진행 중 N | 완료 N ]            Q/BACK 닫기
┌ 목록(구역 접기 헤더) ─────────┐ ┌ 상세 ────────────────────────┐
│ ? 토끼 소동    Lv3            │ │ 제목 · Lv · 유형 태그(메인/사이드/보상)│
│ ! 땔감 모으기  Lv2            │ │ 요약 대사(description)         │
│ · 약초 …                      │ │ ── 요구 사항 ──                │
└──────────────────────────────┘ │ 토끼 : 4/10                    │
                                 │ ── 보상 ──                     │
                                 │ [아이콘] 아이템명 ×n  EXP n  G n │
                                 │ ── 안내 ──                     │
                                 │ 완료 시: ○○에게 보고 (@구역)    │
                                 │ [추적] [포기]                   │
                                 └───────────────────────────────┘
```
- 요구 사항 줄 = `QuestService.Objective(d)`: `대상 표시 이름 : 현재/필요`(대상 이름은 `MonsterTable.DisplayName`·아이템 로컬라이즈, 이미 `NpcQuestWindow.ObjectiveText` 가 비슷한 문장을 생성하므로 이를 `QuestService.Objective` 로 이동). 완료 가능 시 줄 색 강조(기존 `#FFD633`).
- 보상 줄: 아이템은 **기존 아이템 스프라이트**(`IItemCatalog` 의 아이콘) + 한글 이름 + 수량(U6a 영문 대문자 보상 표기 제거). 스프라이트 없으면 이름 텍스트만(흰 네모 금지).
- 완료 가능(`Ready`) 퀘스트는 진행 중 탭 맨 위, 상세 안내에 `○○에게 가서 보고하세요`(`ReportTarget`).
- 받을 수 있는 탭: 레벨 부족은 회색 + `Lv N 필요`, 수락 위치 `○○(@구역)`을 안내.
- 완료 탭: 읽기 전용, 회색.
- 영문 라벨 제거: 제목·버튼 전부 한글(`수락`·`포기`·`추적`·`닫기`). 없는 키는 `Loc` 누락 로그가 아니라 한글 상수로.

### 3-2. 조작표

| 동작 | 마우스 | 키보드 | 게임패드 |
|---|---|---|---|
| 창 열기/닫기 | 하단 메뉴 버튼·X | `Q` / `Esc` | Start(기존 `NpcQuest` 닫기 규칙) |
| 탭 전환 | 탭 클릭 | `←` `→` | `LB` `RB`(또는 D패드 좌우) |
| 항목 선택 | 클릭·호버 | `↑` `↓` | D패드 상하·왼 스틱 |
| 구역 접기/펼치기 | 헤더 클릭 | `Z`(헤더 선택 시) | `X`(헤더 선택 시) |
| 추적 지정/해제 | `[추적]` 클릭 | `Z`(항목 선택 시) | `X` |
| 포기 | `[포기]` 클릭 → 확인창 | `Delete` → 확인창 | `Y` → 확인창(확인 `A` 취소 `B`) |
| 수락/보고 | 입구에서만(§5) | — | — |

키 충돌은 `Docs/Playtest/InputAudit.md` 와 REWORK-8 키 표로 builder 가 구현 전에 대조한다. `Z` 가 이미 추적·접기에 쓰이는 현행(힌트 문구 `Z 추적 지정(최대 N)/구역 접기`)을 그대로 따른다.

### 3-3. 상태 표시 통일 (U6 해소)
| 상태 | 목록 표시 |
|---|---|
| 수락 가능 | 노란 `!` |
| 레벨 부족 | 회색 `!` + `Lv N` |
| 진행 중 | 접두 없음, 추적 중이면 `*` |
| 완료 가능 | 노란 `?` + 정렬 맨 위 |
| 완료 | 회색, 완료 탭 |
유형 태그는 목록 줄 끝에 한 글자(`주`/`서`/`보`) — 색 구분 없이 글자만(그래픽 제작 금지).

---

## 4. 추적 HUD 통일 — `NpcQuestTracker`

- **하나만 남긴다.** Quest Machine HUD(`showInTrackHUD`)는 보드 계통을 CSV 로 이행(P4)하면 대상이 없어 자연 소멸한다. 이행 전에는 `QuestBoard.Build` 의 HUD 컨텐츠 3줄을 지워 가운데 흰 글씨를 먼저 막는다(P2 선행 조치).
- 위치: 화면 가장자리 고정(현행 우측 컬럼 `HudPolicy.ColumnWidth`). 큰 창이 열리면 숨김(현행 `HudPolicy.Hidden` 유지).
- 형식(전 출처 동일, 한 퀘스트당 2줄):
  ```
  [유형 한 글자] 제목                      (완료 가능이면 노란색)
    목표 문장 현재/필요   @구역              → 완료 가능: '완료! ○○에게 보고'
  ```
  현상도 `[현상] 제목` 접두 대신 같은 형식(유형 태그 `의`/`현`은 선택, §8 확정 때 결정).
- 최대 표시 N = **5**(현행 `NpcQuestTrack.Max` 3 → 5, 메이플 UI 가로 폭 안에서 확인), 초과분은 `…외 n건`. 현상은 `Max` 에 포함(현재는 별도 줄이라 상한 밖이었음).
- 접기: 헤더 줄 `▼ 퀘스트 (n)` 클릭·키로 접으면 헤더만 남김. 접힘 상태는 ES3 새 키 `quest.hudFold`(bool).
- `ZoneProgress.TrackerLine()`(구역 세트 재료 n/m)은 퀘스트 목록 **아래** 별도 한 줄로 유지(퀘스트 아님, 위치만 정리).
- 자동 추적: 새로 수락하면 빈 칸이 있으면 자동 추가(현행 `NpcQuestTrack.OnAccepted` 유지), 완료 후 제거.

---

## 5. 입구 통일 (NPC·게시판·푯말)

| 입구 | 현재 | 통합 후 |
|---|---|---|
| NPC | `NpcMenu` Kind.Quest → `NpcQuestVoice` 대화 | 동일. 선택지 노출 조건 = `QuestService.Query(giver=npcId)` 에 수락 가능·완료 가능·진행 중 퀘스트가 있을 때. 머리 위 `!`/`?`(`NpcQuestGiver`)는 같은 `Query` 결과로 계산 |
| 게시판 | Quest Machine 대화 UI | `QuestEntrance.Open(QuestFilter{giver="questboard"})` : 같은 창을 **입구 모드**로 연다 |
| 푯말(사냥꾼 현상) | `BountyPanel` | 입구 모드 창(필터 `source=bounty*`) + 오늘 제안 3건 표시. `BountyPanel` 의 `GraduationCheck` 등 푯말 고유 기능은 유지 |

- **입구 모드:** 같은 창에서 `수락`·`보고` 버튼이 활성(마우스 클릭·`Enter`/`↑`·패드 `A`). 일반 Q 모드에서는 비활성(보기·추적·포기만). 이유: 메이플 방식(수락·보고는 NPC 에서), 3계통이 같은 규칙.
- 수락 가능 목록은 필터된 giver 의 퀘스트만 보이고, 푯말은 오늘 제안 3건, 게시판은 체인의 다음 단계.
- NPC 대화(대사 단계)는 NPC 입구에서만 재생: 수락 시 `offerLine`, 진행 중 `progressLine`, 완료 시 `doneLine`. 게시판·푯말은 대사 없이 요약만.
- 보상 수령 규칙 통일: `Report` 에서 `NpcQuestRewards.CanReceive`(가방 공간) 실패 시 `InventoryFullHint` 안내 후 완료 보류(현행과 같음, 보드·푯말에도 적용).
- 수락 규칙: 레벨·선행 충족 + (현상은 동시 1건) . 포기: 진행 기록 삭제, 수집·장비류는 아이템·장비 영향 없음. 완료한 퀘스트는 재수락 불가(`repeat=daily` 현상만 날마다 새로 제안).

---

## 6. U7 수집 퀘스트 — 창고 보관분 정책 제안

**제안(임시 가정):** 진행도는 **가방(인벤토리) 보유분만** 센다. 창고 보관분은 센 값에 넣지 않는다.
- 근거: ① 제출 소모(`ItemRemover`)가 가방 기준이라 보이는 수량과 소모 수량이 항상 일치(메이플 방식). ② 창고 꺼내기 버그 U17 이 남아 있어 창고 포함 시 '있다고 표시되는데 못 낸다' 위험. ③ 구현 단순(현행 `CountItem` 유지).
- 보완: 요구 사항 줄 끝에 창고 보관분이 있으면 `(창고 N)` 을 회색으로 덧붙인다(`창고 보관 아이템은 꺼내야 제출됩니다` 안내 한 줄은 상세에). 창고 수량 조회 API 가 없으면 이 줄은 P2 이후로 미룸.
- 수락 즉시 반영: 이미 `Collect` 는 실시간 보유량이므로 NPC 계통은 정상. 게시판 계통 `Pickup` 카운터가 원인으로 추정(§1-2-5) → 이행 후 해결, 이행 전에는 BUG-U7 임시 수정.
- 검증: 아이템 N개 보유 → 수락 → 창에서 `N/필요` → 제출 후 소모. 창고에만 둔 경우 `0/필요 (창고 N)` 로 표시되는지.

---

## 7. 구현 단계 P0~P5

| 단계 | 내용 | 건드릴 파일 | 검증 |
|---|---|---|---|
| P0 데이터 모델 **(완료 11-09)** | CSV 열 추가·`NpcQuestDef` 필드 추가·`Parse` 열 부족 허용·`QuestService` 뼈대(`All`/`Query`/`CanAccept`/`Objective`/`ReportTarget`) | `NpcQuestCatalog.cs`, `Resources/QuestsFloor1.csv`(열만 추가, 기존 행 빈칸), 새 `QuestService.cs`, `Assets/Editor/Tests/` | EditMode: 구 CSV 파싱 OK·기존 퀘스트 id/값 불변·`Objective` 문자열 스냅샷. `Tools/u.sh` 컴파일 에러 0 |
| P1 이행 **(완료 11-09)** | `QuestMigration.RunOnce`(qm.* → npcq.state), 게시판 `Chain`/`Story`/부가 3종 → CSV 행, 보상 `stones`·`artifact` 지급을 `NpcQuestRewards.Apply` 에 반영, `Pickup`→`Collect`. 한글 대사 재작성 | 새 `QuestMigration.cs`, `NpcQuestRewards.cs`, `QuestsFloor1.csv`, `Quest/QuestBoard.cs`(Build 호출 중단·입구만 남김) | EditMode: 샘플 세이브(구 qm.* 가 든 문자열)로 이행 전후 상태 일치. 플레이: 보드 퀘스트 수락→처치→보고→보상(골드·강화석·아티팩트) |
| P2 창 | 상세 패널(요약·요구 사항·보상 아이콘·보고 안내)·포기·유형 태그·한글 라벨·추적 Z(현상 포함)·입구 모드 플래그 | `NpcQuestWindow.cs`(500줄 넘으면 `QuestWindowDetail.cs` 분리), `QuestListRules`, `QuestService.cs` | 플레이: 3탭·마우스/키보드/패드 3경로 각각 선택·추적·포기. 스크린샷으로 글자 크기·줄바꿈·보상 표기(영문 없음) 확인. 선행 조치: Quest Machine HUD 컨텐츠 제거 |
| P3 HUD | `NpcQuestTracker` 단일화(현상·세트줄 정리)·최대 5·접기·`quest.hudFold` | `NpcQuestTracker.cs`, `NpcQuestTrack.cs`(Max), `BountyBoard.TrackerText` 사용 중단, `HudPolicy` | 플레이: 수락 6건 → 5개+`…외 1건`, 접기 토글, 완료 시 `○○에게 보고`, 큰 창 열면 숨김 |
| P4 입구 통일 | 게시판·푯말을 `QuestEntrance.Open` 으로 교체, `BountyPanel` 은 입구 모드 창 사용, NPC 선택지 노출 조건을 `Query` 로 | `QuestBoard.cs`, `BountyKiosk.cs`·`BountyPanel.cs`, `NpcMenuRules.cs`·`NpcInteraction.cs`, `NpcQuestGiver.cs`, `QuestBountyLink.cs` | 플레이: 세 입구 모두 같은 창·같은 규칙, 보상 가방 부족 시 안내·보류 |
| P5 정리·확장 | Quest Machine 퀘스트 UI·`QuestBoard` 의 Build 코드 제거(플러그인은 다른 곳에서 안 쓰면 참조 정리), 2번째 목표 `objective2` 판정(필요 시) | `Quest/QuestCatalog.cs`(삭제 후보), `DialogueBoot.cs` 일부 | 컴파일·`Docs/Playtest/ConventionAudit.md` U6·U6a·U7·U8·U8b 갱신 |

각 단계: 정적 테스트(EditMode)→Unity 콘솔 에러 0→플레이 모드 실제 입력 경로 확인→커밋. 확인 못 한 것은 "미검증"으로 기록. `BountyBoard` 일일 3건·`ZoneProgress` 는 건드리지 않는다.

### 7-2. 구현 현황 (11-09)
- **P0 완료:** `NpcQuestDef` 확장 필드 14개(source·kind·reportTo·offerLine·progressLine·doneLine·zone·stones·artifact·repeat·sort·objective2·target2·count2, CSV 13열 뒤 선택 열, 구 CSV 호환), `QuestService`(`All/Query/StatusOf/CanAccept/Accept/Abandon/Report/Objective/ReportTarget`·`Warn` 훅), `NpcQuestLog.Abandon/SeedRecord/Loaded`, 현상 `BountyRules/BountyBoard.Abandon`. kill target `a|b` 목록·`FirstTarget`.
- **P1 완료:** 게시판 Chain 5 + 부가 3 + 촌장 Story 3 = 11건을 `QuestsFloor1.csv` 행으로 이행(id 유지, 한글 재작성, exp 0·stones·artifact, 게시판 giver=`questboard`, 촌장 giver=`elder` kind=main·minLevel 6/13/15). `s_wood`·`s_herb` 는 `collect`(U7 해소), `s_craft` 는 `craft`+target `Any`(CraftingManager 가 따로 보내는 Any 메시지). `NpcQuestRewards` 가 stones·artifact 지급·가방 공간 검사. `QuestMigration`(qm.journal → npcq.state, 키 `quest.migrated`, qm.* 디스크 유지, 저널 메모리에서만 삭제). `QuestBoard` 는 ↑ 입구(`NpcQuestGiver("questboard")` 대화)로 축소, `DialogueBoot` 촌장 퀘스트 대화는 NpcQuestGiver 로 연결.
- **미검증(플레이):** 게시판 ↑ 대화·머리 위 !·촌장 "일 묻기"→퀘스트 대화·실제 구 세이브 이행은 에디터에 씬이 없어 EditMode 로만 확인. 다음 세션이 플레이 검증부터 한다.
- **남은 것:** P2(창 상세 패널·포기·입구 모드·한글 라벨), P3(HUD 단일화·최대 5·접기·`quest.hudFold`), P4(푯말·NPC 선택지를 `QuestService.Query` 로 통일), P5(`QuestCatalog` 삭제·Quest Machine UI 정리·`objective2`). `QuestCatalog` 는 이행 id·동등성 테스트 기준값으로 남겨 둠.

### 7-1. 함정
- `NpcQuestLog` 는 정적 클래스라 테스트용 훅(`LevelGetter`·`ItemCounter`·`SaveHook`)을 쓰고 `ResetStatics` 에 새 훅을 꼭 추가.
- 현상 id(`bounty:`)는 `NpcQuestLog` 에 기록하지 않는다(상태는 `BountyBoard`). `QuestService` 가 접두로 분기.
- `NpcQuestTrack.Max` 를 올릴 때 저장된 `npcq.tracked`(최대 3개) 호환 — 형식 불변이라 안전.
- Quest Machine 직렬화(`QuestJournal.ApplyData`)는 `Start` 코루틴 한 프레임 뒤에 로드되므로 이행은 그 이후에 실행(`QuestBoard.Start` 의 `yield return null` 뒤).
- 퀘스트 창이 열려 있는 동안 `UiLock "NpcQuest"` 유지, 입구 모드도 같은 락을 사용.

---

## 8. REWORK-6 확정 대기 항목 (형식 틀만, 빈칸)

### 8-1. 유형 3종 정의
| 유형 | CSV `kind` | 역할 | 정의(확정 대기) | 층계별 분량 |
|---|---|---|---|---|
| 메인 스토리 가이드 | `main` | 진행 안내·세계관 줄기 | ______ | ______ |
| 사이드(세계관) | `side` | 세계관 설명용 이야기 | ______ | ______ |
| 보상용 | `reward` | 사냥·수집·반복 | ______ | ______ |

### 8-2. 대사 단계 형식 (퀘스트 1건당)
메이플의 단계 구성(시작 대사 → 수락/거절 → 진행 중 대사 → 완료 대사 → 보상)만 참고하고 문장·고유명사는 `TowerWorld.md` 세계관으로 새로 쓴다.
| 단계 | CSV 열 | 길이 상한(안) | 내용 |
|---|---|---|---|
| 요약 | `description` | 40자 | 창에 표시되는 한 줄 사정 |
| 수락 대사 | `offerLine` | ______ | ______ |
| 진행 대사 | `progressLine` | ______ | ______ |
| 완료 대사 | `doneLine` | ______ | ______ |
| 거절 대사 | (열 없음, 기본 문구) | ______ | ______ |

### 8-3. 채울 표 (REWORK-6 산출 `QuestDesign.md` 에서 가져옴)
- 층계별 메인 줄기: 1층계 ______ / 2층계 ______
- 사이드 사슬 후보: ______
- 현재 퀘스트 재분류표(`QuestsFloor1.csv` + 게시판 13건 + 현상 17건 → `kind`): ______
- 클래식 메이플 종류 분포·사슬 길이·레벨 구간(조사 결과): ______

---

## 9. 열린 질문·임시 가정 (`Docs/OpenQuestions.md` Q-QS1~6)

1. Q-QS1 수집 퀘스트에 창고 보관분 포함 → **가정: 포함 안 함**, 표시만 덧붙임(§6).
2. Q-QS2 추적 HUD 최대 개수 → **가정: 5**(현재 3), 현상 포함.
3. Q-QS3 Q 창에서 수락·보고 가능 여부 → **가정: 불가**(입구 모드에서만). 메이플 방식.
4. Q-QS4 현상(일일 제비뽑기)을 CSV 로 옮길지 → **가정: 정의 그대로 두고 어댑터 확장**.
5. Q-QS5 퀘스트 포기 허용 → **가정: 허용**(확인창, 기록 삭제, 완료분 재수락 불가).
6. Q-QS6 게시판 체인 5~6건 → CSV 이행 시 한글 재작성·`kind` 분류는 REWORK-6 결과 대기. 임시: `reward`.
