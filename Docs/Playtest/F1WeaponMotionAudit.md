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

## 해야 할 일 정리

1. **연결 작업(생성 0, 코드)**: 위 표 제안대로 무기 프리팹 `attackAnimName`·`skillAnim` 또는 `SkillCels` 표를 고친다. 스킬 판정 시점(`WeaponFx.StrikeFrame`·`MotionFxTable`)이 새 셀에 맞는지 같이 확인. 총괄(코드) 몫.
2. **기존 셀 흰 바탕 확인(생성 0)**: 연결에 쓰는 셀 약 20개(Bow·WhipSnap·ShieldBash·Throw·DashSlash·Backstep·LowSweep·Raise·PalmBlast·DaggerFlurry·DaggerCross·Kick·Slam·LeapSlam·Lunge·Punch·GreatDown·GreatSide·GreatUpper·Charge·Cast·Spin·ShoulderAim)를 기본 코트로 찍어 탈락만 추린다.
3. **새로 만들 셀(PixelLab, Opus 하위 세션)**: ★ 하늘로 쏘기(활 곡사), ★ 할퀴기 2타(클로), (선택) ★ 올려 할퀴기. 2번에서 탈락한 셀은 재생성 목록에 추가.
4. **스킬 이름 없는 애니 채우기**: `Bear Roar`·`Encroaching Darkness` `skillAnim`=`Raise`.

미검증: 표는 프리팹·코드 정적 분석. 실제 재생 화면은 확인하지 않았다.
