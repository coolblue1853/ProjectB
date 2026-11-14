# 1층계 무기 모션 점검 (11-14)

목적: 1층계 무기 40종에 **기본 코트 상태**로 공격 모션을 넣기 전에, 평타·스킬마다 어떤 셀이 실제로 재생되는지와 스킬 로직에 맞는지 정리한다.

## 재생 셀 결정 방식 (`PixelRig.CelKey`)

1. 애니 이름과 같은 셀이 `CelDS/cel.json`에 있으면 그 셀
2. `PixelRig.SkillCels` 표(10개: GreatVashDown→GreatDown 등)
3. `MapName`(이름 글자로 추측: staff·buff→Cast, stab→Stab, up→SwingUp, 그 밖→SwingDown) → 없으면 `CelAlias`

평타는 무기 프리팹 `attackAnimName`, 스킬은 `Skill.skillAnim`. 조사 스크립트: 세션 임시(`wa.py`), 결과는 아래 표.

## 핵심 문제

- **셀은 66개나 있는데 무기에 연결 안 된 것이 많다**: `Bow`·`WhipSnap`·`ShieldBash`·`Throw`·`DashSlash`·`Backstep`·`LowSweep`·`Raise`·`PalmBlast`·`DaggerFlurry`·`Kick`·`Slam` 등. 활 평타가 `Cast`(시전)로 나오고, 방패 강타는 `Guard`(대기 자세), 투창·아대는 던지지 않는다.
- **스킬 로직과 모션이 안 맞는다**: 이름 추측(`MapName`) 때문에 서로 다른 스킬이 같은 셀로 떨어진다. 예: 채찍 `Leap Slam`=찌르기, 클로 `Rending Upswipe`=이동 대시, 활 3스킬 전부 시전, 단검 `Ground Sweep`=올려베기.
- **같은 무기 안 중복**: 활·석궁·랜턴·완드·마도서는 평타·스킬 모두 `Cast` 하나. 단검 버프 2개가 같은 `Charge`, TwinFang 스킬 2개가 같은 `DaggerCross`.
- **평타 타수 부족**: 창·채찍·투창·아대 평타 1종(1타 반복).
- **포효류 스킬 애니 없음**: `Bear Roar`·`Encroaching Darkness`는 `skillAnim` 비어 있음.
- **셀 품질 미확인**: 기본 칼 3타(Stab·SwingDown·SwingUp)만 확정·보존 대상. 나머지 셀은 기본 코트 흰 바탕 확인을 아직 안 했다.

## 무기별 현황과 할 일

표기: 지금 셀 → 제안 셀. ★ = 셀이 없어 새로 만들어야 함.

| 무기 타입(1층계 무기) | 평타 지금 | 스킬 지금 | 할 일 |
|---|---|---|---|
| 한손검(RustedSword·PoisonSword) | Stab/SwingDown/SwingUp(3타 확정), Poison 은 SwingDown 2회 | Smash Down=SwingDown, Slash=GreatSideOne, Vile Ambush=SwingDown, Turf=SwingDown | Poison 평타를 3타로 통일. Smash Down→`LeapSlam`(점프 내려찍기), Vile Ambush→`DashSlash`, Turf→`Slam` |
| 단검(Fang·RatChief·Cutthroat·Bloodsting) | Stab/Stab/SwingDown | 돌진=DaggerCross, 버프 2개=Charge 중복, RatChief·Bloodsting 은 SwingDown/SwingUp/Cast | 평타 끝을 `DaggerFlurry`로. 버프 하나는 `Backstep`(그림자 걸음). Swift Assault·Bloodsting→`DashSlash`, Ground Sweep→`LowSweep`, Rally→`Raise` |
| 쌍검(TwinFang, 4타) | 한손검 셀 4개 | 2개 모두 DaggerCross | 평타 `DaggerCross`/`DaggerFlurry` 계열로. Turn and Thrust→`Backstep`+`Lunge` |
| 활(Shortbow·Longbow) | **Cast** | 전부 Cast | 평타·연사→`Bow`. 독 화살비·Pitch Shot→★하늘로 쏘기(곡사) 1개 |
| 석궁(Sentry) | Cast | 전부 Cast | 평타→`ShoulderAim`(어깨 조준). 연사→`Bow` 재사용 가능 |
| 둔기(Club·Mace·Maul) | GreatDown/GreatSide | 모으기(hold)=SwingDown(한손 셀), 땅 울림=Cast | hold→`Charge`, 땅 울림·Aftershock→`Slam` |
| 도끼(Woodcutter·CrocJaw) | SwingDown/SwingUp | SwingDown·LowSweep·GreatSide·Spin | 대체로 맞음. Timber Chop→`GreatDown`(양손 내려찍기)로 구분 |
| 낫(ScareSide·Phantom) | SwingDown/SwingUp | 도끼와 같은 구성 | 대체로 맞음. Phantom Glide(활주)→`DashSlash` |
| 대검(Sacre·Heartwood·Gatekeeper) | GreatDown/GreatSide | 모으기(hold)=SwingDown, 고대 분노=Cast | hold→`Charge`, Ancient Wrath→`GreatUpper`, Stone Sentinels→`Slam` |
| 채찍(FrogTongue·VineLash) | **Stab 1종** | Lunge, Leap Slam=Stab | 평타→`WhipSnap` + 2타 `SwingSide` 계열. Leap Slam→`LeapSlam` |
| 클로(BearClaw·ScareClow) | Stab/Stab | 돌진·올려베기 모두 `Dash`(이동 셀), 포효 애니 없음 | 평타→★할퀴기 2타(위·아래). Upswipe→`GreatUpper` 또는 ★올려 할퀴기. 포효→`Raise` |
| 너클(Leather·Tusk·Petal·Pebble) | Stab/Stab | Dash·Dash·Punch | 평타→`Punch`(원투). Sway Back·Shell Back→`Backstep`, Hop→`Kick` |
| 창(Wood Spear·WarBanner) | **Stab 1종** | Lunge, Charge Forward=Stab, Rally=Cast | 평타 2~3타: `Stab`→`Lunge`→`SwingSide`(휘둘러 치기). Charge Forward→`DashSlash`, Rally→`Raise` |
| 투창(Javelin) | Stab 1종 | 창과 같음 | 평타→`Throw`(던지기, `ThrownSpearGimmick`와 맞음) |
| 지팡이(LeafStaff·RootStaff) | SwingUp/SwingDown | 순간이동=Charge, 뿌리·치유=Cast | 대체로 맞음. 치유→`Raise`로 구분 |
| 랜턴·완드·마도서(Wisp·Fae·Grimoire) | Cast/Cast | 3개 모두 Cast | 평타 `Cast`. 고리·축복→`Raise`, 창·빔→`PalmBlast` |
| 아대(LeafAdae) | SwingDown 1종 | 흩날리는 잎(round)=SwingDown, 버프 2=Charge | 평타→`Throw`(표창). 흩날리는 잎→`Spin`, 버프 하나→`Backstep` |
| 방패(Shell·Raider·Monolith) | 한손검 3타 | 찌르기=Stab, 강타=**Guard(대기)**, 막기=Guard | 강타→`ShieldBash`, Boulder Toss→`Throw`. 막기(hold)는 `Guard` 유지 |

## 최종 목록(11-14 사용자 정정 — 이 절이 우선)

기본 코트로 완성된 것은 **대기·걷기·달리기·점프와 기본 칼 3타(찌르기·내려베기·올려베기)뿐**이다(g2 내보내기 44프레임과 일치). 아래 `ds_side`·`ds_cross`·`ds_flurry`·`ds_charge`·공중 3종·피격·사망·로프는 코트 기준 미완으로 본다. 전부 새 방식으로 찍는다.

| 순서 | 묶음 | 셀 | 수 |
|---|---|---|---|
| 1 | 기본 동작(모든 무기 공통) | 피격, 사망(넉다운 겸용), 로프 오르기·매달림, 대시 | 4~5 |
| 2 | 기본 칼 공중 공격 | 공중 찌르기·공중 내려베기·공중 올려베기 | 3 |
| 3 | 무기 평타 | 수평 베기, 교차, 연타(단검·쌍검), 양손 내려찍기, 활 쏘기, 던지기, 채찍, 원투, 할퀴기 2타, 시전 | 10 |
| 4 | 스킬 공용 | 돌진 찌르기, 돌진 베기, 뒤로 빠지기, 하단 쓸기, 양손 올려치기, 지면 강타, 점프 내려찍기, 들어올리기·포효, 장풍, 모으기 | 10 |
| 5 | 무기 전용 | 활 곡사, 방패 밀치기, 방패 막기, 회전 베기(M4-a 결과 반영) | 4 |
| 6 | 선택·나중 | 석궁 조준, 발차기, 무기별 공중 공격(대검 등) | 2+ |

합계 약 33~35셀. 예상 PixelLab 약 70~100생성(셀당 2~3). 모두 Opus 하위 세션, 판정은 흰 바탕 5배 프레임별.

## 정정(11-14): 셀 이름 66개 중 실제 그림은 13종뿐

`CelDS/cel.json`을 열어 보니 이름만 66개이고 대부분 다른 셀 그림을 빌려 쓴다. 새 방식(ds03 기준 그림 + `animate_image_pixminimax` 문장 방식, 빈손 생성 → 이펙트 지우기 → 무기는 코드)으로 만든 실제 공격 그림은 아래뿐이다.

- 확정: `ds_stab`·`ds_down`·`ds_up`(기본 칼 3타), 이동·점프·피격·사망·로프
- 있음(품질 미확인): `ds_side`(수평), `ds_cross`(교차), `ds_flurry`(연타), `ds_charge`(모으기), `ds_airstab`·`ds_airdown`·`ds_airup`(공중)

빌려 쓰는 예: `Bow`·`Throw`·`ShieldBash`·`Kick`·`PalmBlast`·`Lunge` = 찌르기 그림, `WhipSnap`·`Slam`·`LowSweep`·`LeapSlam`·`GreatDown` = 내려베기 그림, `Guard` = 대기, `Backstep` = 피격, `Punch` = 연타, `Raise`·`GreatUpper` = 올려베기, `Spin`·`DashSlash` = 수평. 그래서 위 표의 "연결"만으로는 화면이 안 바뀐다 → **새 방식으로 새로 찍어야 한다.**

### 1층계에 필요한 새 셀 (새 방식, Opus 하위 세션)

| # | 셀 | 쓰는 무기 | 비고 |
|---|---|---|---|
| 1 | `Bow` 활 쏘기 | 활·석궁 평타·연사 | |
| 2 | 곡사(하늘로 쏘기) | 활 화살비·Pitch Shot | 신규 이름 |
| 3 | `ShoulderAim` 어깨 조준 | 석궁 | 선택(1번 재사용 가능) |
| 4 | `Throw` 던지기 | 투창·아대 평타, 방패 Boulder Toss | |
| 5 | `WhipSnap` 채찍 휘둘러 치기 | 채찍 평타 | 사슬은 `WeaponChain` 코드 |
| 6 | `ShieldBash` 방패 밀치기 | 방패 강타 | |
| 7 | `Guard` 방패 막기 | 방패 막기(hold) | 지금 대기 그림 |
| 8 | `Punch` 원투 | 너클 평타 | |
| 9 | `Lunge` 몸째 돌진 찌르기 | 창·채찍·투창 스킬, 창 2타 | |
| 10 | `DashSlash` 돌진 베기 | 한손검·단검·창·낫 스킬 | |
| 11 | `Backstep` 뒤로 빠지기 | 단검·쌍검·너클·아대 스킬 | |
| 12 | `LowSweep` 하단 쓸기 | 도끼·낫·단검 스킬 | |
| 13 | `GreatDown` 양손 내려찍기 | 대검·둔기 평타, 도끼 스킬 | 지금 한손 내려베기 그림 |
| 14 | `GreatUpper` 양손 올려치기 | 대검 스킬 | |
| 15 | `Slam` 지면 강타 | 둔기·대검·한손검 스킬 | |
| 16 | `LeapSlam` 점프 내려찍기 | 한손검·채찍 스킬 | |
| 17 | `Spin` 회전 베기 | 도끼·낫 hold, 아대 | 합격 레시피 M4-a(`MotionTheory.md` §11) 결과 있음 → 셀 반영만(생성 0~1) |
| 18 | `Raise` 들어올리기·포효 | 지팡이 치유, 버프·포효, 깃발 | |
| 19 | `PalmBlast` 장풍·빔 | 완드·마도서·랜턴 | |
| 20 | 할퀴기 2타 | 클로 평타 | 신규 이름 |
| 21 | (선택) `Kick` | 너클 Hop | |

기존 그림 재사용(새로 안 찍음): 한손검 3타, 수평(`ds_side`: 도끼·대검 수평, 창 휘두르기), 교차·연타(단검·쌍검), 모으기(`Cast`·`Charge`: 지팡이·시전·버프 일부). 단 이 4종도 기본 코트 흰 바탕 확인이 먼저다.

예상 PixelLab: 셀 하나 4~8장 1호출 = 1생성, 재시도 포함 셀당 2~3생성 → 약 40~60생성.

## 해야 할 일 정리 (정정 전 초안 — 위 정정 절이 우선)

1. **연결 작업(생성 0, 코드)**: 위 표 제안대로 무기 프리팹 `attackAnimName`·`skillAnim` 또는 `SkillCels` 표를 고친다. 스킬 판정 시점(`WeaponFx.StrikeFrame`·`MotionFxTable`)이 새 셀에 맞는지 같이 확인. 총괄(코드) 몫.
2. **기존 셀 흰 바탕 확인(생성 0)**: 연결에 쓰는 셀 약 20개(Bow·WhipSnap·ShieldBash·Throw·DashSlash·Backstep·LowSweep·Raise·PalmBlast·DaggerFlurry·DaggerCross·Kick·Slam·LeapSlam·Lunge·Punch·GreatDown·GreatSide·GreatUpper·Charge·Cast·Spin·ShoulderAim)를 기본 코트로 찍어 탈락만 추린다.
3. **새로 만들 셀(PixelLab, Opus 하위 세션)**: ★ 하늘로 쏘기(활 곡사), ★ 할퀴기 2타(클로), (선택) ★ 올려 할퀴기. 2번에서 탈락한 셀은 재생성 목록에 추가.
4. **스킬 이름 없는 애니 채우기**: `Bear Roar`·`Encroaching Darkness` `skillAnim`=`Raise`.

미검증: 표는 프리팹·코드 정적 분석. 실제 재생 화면은 확인하지 않았다.

## 진행 기록
- 11-14 피격(코트): pixminimax 4장, 첫·끝 = `EquipDS/base_coat/ds_idle_0`, east·side·seed 7001, 2생성(1회는 속옷 몸 `CelDS/ds_idle_0`을 잘못 넣어 버림). 속도선은 최대 연결 덩어리만 남겨 지움. 결과 `Docs/ArtCompare/Motion/coat_basic/hurt_g2/c1~c3.png`. **사용자 합격(11-14)**, 게임 미반영(의상 한 벌 구조 정리 뒤 반영). 다음: 대시 신규(사망·로프는 유지).
- 11-14 대시(코트): 1차 `dash_g1` 탈락(4칸 같은 자세·코트가 가로 네모 덩어리). 2차 `dash_g2` = 문구를 "네 단계(무릎 굽힘→밀기 30°→낮은 보폭→착지 감속), 칸마다 다르게, 코트 모양 유지"로 바꿈, seed 7001, 단계가 또렷하고 얼굴·코트·배낭 유지. 4칸이 감속 대신 공중 보폭. 사용자 판정 대기. 누적 2생성.
- 11-14 대시 2차 사용자 합격.
- 11-14 공중 3종(코트, 빈손): 첫 프레임 `base_coat/ds_arc_1`(점프 정점), seed 7001, 4장씩. 찌르기 `airstab_g1`, 올려베기 `airup_g1`(호 이펙트 색 지정 지움), 내려베기 1차 탈락(팔이 안 올라감·호) → 2차 `airdown_g2` "네 단계(머리 위→얼굴 앞→무릎 높이→유지)" 문구, 1칸은 뒷팔이 올라가 빼고 2~4칸 사용. 4생성. 미리보기 `coat_basic/air_g_x4.gif`. 사용자 판정 대기.
- 주의: `CelDS/ds_*`는 이제 속옷 몸이다. 코트 기준 그림은 `EquipDS/base_coat/`.
