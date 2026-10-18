# Lv2 콘텐츠 기획 — 마법 절벽 (Magic Cliffs)

> 상태: **기획안(미구현)**, 2026-10-18 작성. QualityRoadmap E4의 상세 기획이다.
> 근거: 캐시된 `.unitypackage`의 내용(pathname·meta·PNG 헤더), 기존 프리팹·씬 YAML, `SetEffectApplier.cs`·`Skill.cs`·`PlayerController.cs`.
> 수치는 모두 **초안**이다. 적용하면 `Game/EnemySpec.md`·`Game/ItemSpec.md`에 반영하고, 이 문서에는 ✅와 커밋 해시를 적는다.

## 0. 현재 기준값 (역산)

| 항목 | 값 | 출처 |
|---|---|---|
| 플레이어 | BoxCollider 0.185×1.04, scale 1.5 → **키 약 1.56u**. fullHP 100, 스태미나 100 | 씬 `Player` |
| 점프 | jumpForce 13, gravityScale 3 → 1단 **약 2.9u**, 2단 **약 5.7u** | `PlayerController` |
| 기존 몬스터 스프라이트 | 고해상도 개별 PNG(예: RatThief 400×336), **PPU 100, Point**, 프리팹 scale 0.69 → 키 약 1.4~1.6u | `Sprite/Enemy/*`, `RatThief_BD` |
| 카메라 | orthographic 5.87(세로 11.7u), 640×360 기준 **약 31px/u**. ProCamera2D PixelPerfect PPU=8(사실상 미사용) | 씬 |
| 맵 배치 | Town 루트 (-305.4, 255), Lv1 SideRoad 루트 (-305.4, 79.9) — **세로로 쌓는 구조** | 씬 |
| 원웨이 발판 | tag `Ground` + **BoxCollider2D**(하향 점프가 이 컴포넌트를 끔). 통과 불가 바닥은 `BaseGround` 컴포넌트를 추가 | `PlayerController.DisableCollision` |
| 사다리/로프 | tag `Ladder` 트리거, 스프라이트 x에 스냅 | 씬 `Ladder (n)` 21개 |
| 몬스터 방향 | 오른쪽 이동 = localScale.x **양수** → 스프라이트는 **오른쪽을 봐야 한다** | `EnemyPatrol` |
| 애니메이터 규약 | 파라미터 `isWalk / isAttack / isHit`, 상태 Stay/Walk/Attack/Hit. 사망은 별도 DeadBody 프리팹 | `RatThifeAnim.controller` |
| Lv1 몬스터 | HP 50 / Def 1, 공격 10~30, 보스 1000HP·할퀴기 50 | `EnemySpec.md` |
| Lv1 장비 | 무기 5~10뎀. R등급 방어구 5부위 합 **Def 60 / HP 700** (Rabbit C등급 Def 33) | `Resources/{부위}/*.prefab` |

## 1. 에셋 인벤토리

캐시 경로: `%APPDATA%/Unity/Asset Store-5.x/<퍼블리셔>/...`. 전부 meta가 포함되어 있다(Luiz Melo·Clembod는 **이미 슬라이스됨**, spriteMode=Multiple).

### 1.1 캐릭터 팩

| 팩 (원래 경로) | 캐릭터 | 프레임 크기 | 애니메이션 (프레임 수) | 실제 몸통(1프레임 알파 bbox) | 메모 |
|---|---|---|---|---|---|
| MonstersCreaturesFantasy (`Assets/Monsters Creatures Fantasy`) | Goblin | 150×150 | Idle 4, Run 8, Attack1 8, Attack2 8, Take Hit 4, Death 4 | 33×36px | 우향 |
| 〃 | Mushroom | 150×150 | Idle 4, Run 8, Attack1 8, Attack2 8, Take Hit 4, Death 4 | 23×37px | 우향 |
| 〃 | Skeleton | 150×150 | Idle 4, **Walk 4**, Attack1 8, Attack2 8, **Shield 4**, Take Hit 4, Death 4 | 45×51px | Run 없음 |
| 〃 | Flying eye | 150×150 | **Flight 8**(=Idle/Run), Attack1 8, Attack2 8, Take Hit 4, Death 4 | 41×31px | 비행 |
| Evil Wizard 2 | Evil Wizard | 250×250 | Idle 8, Run 8, Attack1 8, Attack2 8, Jump 2, Fall 2, Take hit 3, Death 7 | 57×95px | 보스 후보 |
| Bringer Of Death free (Clembod) | Bringer | 140×93 (1120×744 시트, 64칸) | Idle 8, Walk 8, Attack 10, Cast 9, Spell 16, Hurt 3, Death 10 (10fps .anim 포함) | 40×54px | **좌향** → flipX 필요. 이펙트 없는 시트 별도 |
| Martial Hero | 무사 | 200×200 | Idle 8, Run 8, Attack1 6, Attack2 6, Jump 2, Fall 2, Take Hit 4(+흰 실루엣), Death 6 | 37×52px | **Lv2 미사용**(Lv3 결투 NPC/엘리트 예비) |
| Medieval King Pack 2 | 왕 | 160×111 | Idle 8, Run 8, Attack1~3 각 4, Jump 2, Fall 2, Take Hit 4(+실루엣), Death 6 | 31×54px | **Lv2 미사용**(Lv3 보스/마을 NPC 예비) |
| Magic Cliffs — Enemies Addon | Fox Walking | 80×48 개별 PNG | Walk 6 | – | 피격·사망 없음 → 셰이더 플래시·디졸브로 대체 |
| 〃 | Guy Running / Guy Dive Kick | 66×64 / 64×72 | Run 12 / Dive Kick 5 | – | 예비(Lv2 미사용) |
| 〃 Girl Addon 1·2 | 플레이어형 동작 11종 | 128×96 | Drink 9, Piercing 18, Climb 7 … | – | 플레이어는 AnyPortrait → **임포트 제외** |

- 팩에 들어 있는 `.anim`/`.controller`는 데모용이다(Luiz Melo는 스프라이트 키가 연결되지 않은 클립). **애니메이션은 슬라이스된 스프라이트로 새로 만든다**(5.3절).

### 1.2 환경 · 아이콘 팩

| 팩 | 내용 | 크기 / 설정 | 용도 |
|---|---|---|---|
| Magic Cliffs (`Assets/GothicVania/Magic Cliffs`) | `tileset.png` 640×320(16px 타일, 259개 Tile 에셋 + `TilePalette.prefab`), `cave-tileset.png` 224×256(81타일, `TilesPaletteCave.prefab`) | 원본 PPU 16, Point | 지형, 동굴(보스 제단) |
| 〃 패럴랙스 | `sky.png` 112×304, `clouds.png` 544×236, `far-grounds.png` 616×110, `sea.png` 112×96 | PPU 100 | 4레이어 패럴랙스 |
| 〃 오브젝트 | `objects.png` 288×176(5개 슬라이스), FoliagePack plant-1~5, tree-1~3 | PPU 100 | 장식 |
| Underwater Fantasy | far / foreground-1 / foreground-2 / sand(각 256×192), 머티리얼 4, **스크립트 2개**(`Done_BGScroller.cs`, `Perspective2DSortMode.cs`) | – | **Lv2 미사용**. 나중에 넣을 때 스크립트는 제외 |
| 2D Pixel - RPG Monsters Icon (LAYERLAB) | 8테마(Undead/Jungle/Sea/Forest/Field/Cave/Devil/Ice) × 12 = **96종**. `default size/{shadow,no shadow}` 32×32, `min size` 트림본 | Point, PPU 100 | 도감·드롭 UI(6절) |

### 1.3 픽셀 스케일 결정

- 새 캐릭터의 몸통 높이는 36~95px이다. 기존 몬스터(약 1.5u)와 맞추려면 **약 24px/u**가 필요하다.

| 대상 | PPU | 결과 (키) | 비고 |
|---|---|---|---|
| Goblin / Mushroom / Flying eye | **24** | 1.5u / 1.54u / 1.3u | 플레이어(1.56u)와 비슷 |
| Skeleton | 24 | 2.1u | 탱커라 커도 됨 |
| Bringer (엘리트) | 24 | 2.25u | |
| Evil Wizard (보스) | 24 | **약 4.0u** | 보스 존재감 |
| Fox | 24 | 약 1.2u(48px 프레임) | |
| 타일셋·장식 | **16** | 타일 1칸 = 1u | 레벨 설계 계산이 쉬움 |
| 패럴랙스 | 16 | 화면 높이에 맞춰 스케일 | `ParrelaxBacground` |

- 프리팹 scale은 1로 둔다(템플릿의 0.69를 되돌림).
- 캐릭터(1/24u)와 타일(1/16u)은 텍셀 밀도가 1.5배 다르다. 기존 게임도 고해상도 몬스터와 도트 배경을 섞어 쓰고 있어서 허용한다(Q-L1).
- 공통 임포트 설정: **filterMode Point, Compression None, Mipmap off**. 슬라이스는 원본 meta를 유지하고 pivot은 Center(기존 몬스터와 같은 alignment 0)로 둔다.
- 중심 pivot이라 발 위치는 몸통보다 아래에 있다. 150px 프레임의 발 높이는 **-26px**(= -1.08u)이다(Goblin·Mushroom·Skeleton bbox 하단 y=101 기준). 콜라이더와 GroundCheck는 이 값으로 배치한다.

## 2. Lv2 사냥터 "마법 절벽" 설계

### 2.1 배치와 연결

- 루트 `Lv2_MagicCliffs`를 SideRoad 아래 **(-305.4, -95, 100)**에 둔다. 세로 간격 약 175u로 기존 구조와 같다. 평소에는 비활성이다.
- **입구:** SideRoad 왼쪽 확장 구역의 서쪽 끝(보스 구역 너머, x≈-760)에 `Portal_ToLv2`를 둔다. ↑ 키로 이동한다.
  - destination = `Lv2_MagicCliffs/Spawn_Entrance`, openMap = Lv2, closeMap = SideRoad
- **복귀:** Lv2 입구의 `Portal_ToLv1`이 SideRoad 입구 포털 옆으로 돌아간다. **마을 직행 포털은 없다**(귀환은 Lv1 경유).
- 보스 제단 입구는 내부 포털(openMap 비움, 같은 맵에서 위치만 이동)로 만든다.
- 카메라: Lv2 전용 ProCamera2D Numeric Boundaries(Rooms)를 추가한다. 하늘 레이어 때문에 세로 여유를 4u 둔다.

### 2.2 레이아웃 (가로 약 240u, 타일 1u)

```
 y=13 ─────────────[C3 첨탑 꼭대기]──────────────╗
 y= 8 ──[A2]────────[B3]──────[C2 엘리트 원형 발판 18u]────┐      ║ 내부포털
 y= 4 ────[B1]═══rope═══[B2]────────[C1]──────┤      ╚═> [D 보스 제단 36u, 양옆 발판 y=4]
 y= 0 ▓▓[A 입구 해안]▓▓▓▓▓▓[B 부유 바위]▓▓▓▓▓▓▓[C 첨탑 기단]▓▓▓▓
       x0          60              150              200        240
```

| 구역 | x | 층(발판 y) | 낮 | 밤 | 특징 |
|---|---|---|---|---|---|
| A 해안 입구 | 0~60 | 0, 8 | Fox, Mushroom | Mushroom, Flying eye | 입구 포털, 안전지대 x0~8(스포너 없음) |
| B 부유 바위 | 60~150 | 0, 4, 8 | Goblin, Mushroom | Skeleton, Flying eye, Goblin | 원웨이 발판 6개, 로프 4, 사다리 2 |
| C 첨탑 | 150~200 | 0, 4, 8, 13 | Goblin | Skeleton, **엘리트 Bringer**(C2) | y=8→13은 5u 간격이라 2단 점프(5.7u)로 겨우 닿는다 → 로프를 기본 경로로 둔다 |
| D 보스 제단 | 200~240 (동굴 타일) | 0, 4(좌우) | – | **보스 Evil Wizard** | C3에서 내부 포털로 입장. 벽 콜라이더로 닫힌 방 |

- **층 간격:** 4u는 2단 점프로, 5u는 로프로 오른다. 메이플식으로 층마다 최소 1개의 로프/사다리를 둔다. 기존 `Ladder` 오브젝트를 복제한다(scale 0.45, 길이는 y scale로 조절).
- **발판 종류:**
  - 원웨이 발판: tag `Ground`, layer `Platform`, BoxCollider2D + PlatformEffector2D. 기존 원웨이 발판을 복제해서 만든다.
  - y=0 바닥과 벽: `BaseGround` + layer `Ground`. TilemapCollider2D + CompositeCollider2D를 써도 된다.
  - 타일맵은 시각용(`Tilemap_Visual`)과 충돌용을 분리한다.
- **패럴랙스:** sky(0.95) / clouds(0.85) / far-grounds(0.6) / sea(0.4).
  - 기존 SideRoad 배경 오브젝트를 복제해서 `ParrelaxBacground` 배수를 이 값으로 바꾼다.
  - `isInfiniteHor`는 sky·clouds만 켠다.

### 2.3 스포너 그룹 (`EnemySpawner` + `DropManager`, 스포너당 1종)

| 스포너 | 구역/층 | 몬스터 | enemyCount | cycle(s) | 시간대 | 위치 자식 |
|---|---|---|---|---|---|---|
| L2_A_Fox | A/0 | Fox | 4 | 10 | 낮 | 4 |
| L2_A_Shroom | A/0·8 | Mushroom | 4 | 10 | 낮 | 5 |
| L2_A_ShroomN | A/0 | Mushroom | 3 | 10 | 밤 | 3 |
| L2_A_EyeN | A/8 | Flying eye | 3 | 12 | 밤 | 3 (공중 y+2) |
| L2_B_Goblin | B/0·4 | Goblin | 6 | 10 | 낮 | 6 |
| L2_B_Shroom | B/8 | Mushroom | 3 | 10 | 낮 | 3 |
| L2_B_GoblinN | B/4 | Goblin | 3 | 10 | 밤 | 3 |
| L2_B_SkelN | B/0 | Skeleton | 4 | 12 | 밤 | 4 |
| L2_B_EyeN | B/8 | Flying eye | 4 | 12 | 밤 | 4 |
| L2_C_Goblin | C/0·4 | Goblin | 4 | 10 | 낮 | 4 |
| L2_C_SkelN | C/0·4 | Skeleton | 4 | 12 | 밤 | 4 |
| L2_C_EliteN | C2 | Bringer | 1 | **90** | 밤 | 1 (고정, isRandSpawn=0) |
| L2_D_BossN | D/0 | Evil Wizard | 1 | **180** | 밤 | 1 (고정) |
| L2_Rock ×3 | A·B·C | Rock(Lv2 드롭) | 2 | 3~30 | 낮(채집물 기존 설정 복사) | 2 |

- 스포너 콜라이더(영역)는 구역 단위로 겹치지 않게 둔다. 층이 여러 개인 구역은 세로로 길게 잡는다.
- 낮 스포너는 Lv1처럼 `isDaytimeSpowner=1`이다.

### 2.4 난이도 곡선 (EnemySpec 형식)

| 지표 | Lv1 | Lv2 | 의도 |
|---|---|---|---|
| 일반 HP / Def | 50 / 1 (StoneRabbit 10) | 100~260 / 2~12 | Lv1 무기(5~10)로 **약 4.5배 느리게** 잡는다 → Lv2 무기 제작 동기 |
| 일반 공격 | 10~30 | 35~90 | |
| 엘리트 / 보스 HP | – / 1000 | 2500 / 8000 | |
| 보스 최대 단타 | 50 | 150 | |
| 권장 장비 | Rabbit → Leaf/Rat/Scare | R세트 5부위(Def 60, HP 800) → Cliff 세트 | |

- **검산** (받는 피해 = `floor(d²/(d+Def))`, 주는 피해 = `dmg²/(dmg+Def)`):
  - Lv1 R세트(Def 60)가 Goblin 55를 맞으면 26, 보스 150을 맞으면 107이다(HP 800 → 7~8방).
  - Cliff 세트(Def 90+15)는 보스 150을 맞으면 88이다.
  - Lv1 무기 평균 7로 Def 4 Mushroom(160)을 치면 4.5/타 → 36타다.
  - Lv2 무기 평균 19면 15.7/타 → 10타다. Lv1 몬스터를 Lv1 무기로 잡을 때(8타)와 비슷하다.
- Skeleton(Def 12)은 독(방어 무시 고정 피해)으로 잡으면 유리하다. Rat 세트를 계속 쓸 이유가 된다.

## 3. 신규 몬스터

### 3.1 스탯

| 몬스터 (프리팹) | 시간대 | HP / Def | 성향 | 공격 (피해) | 추격 (u/0.1s) | 슈퍼아머 |
|---|---|---|---|---|---|---|
| Fox (`CliffFox_BD`) | 낮 | 100 / 2 | 비선공, 맞으면 도주 | 없음 | 0.3 | – |
| Mushroom (`SporeShroom_BD`) | 낮·밤 | 160 / 4 | 맞으면 반격 | A1 박치기 근접 45 / A2 포자 투척(포물선) 35 | 0.15 | – |
| Goblin (`CliffGoblin_BD`) | 낮·밤 | 150 / 4 | 선공, 벽 점프 | A1 베기 55 / A2 2연타 35×2 | 0.35 | – |
| Flying eye (`GazeEye_BD`) | 밤 | 120 / 3 | 선공, 비행(중력 0) | A1 물기 근접 50 / A2 유도 눈빛탄 40 | 0.3 | – |
| Skeleton (`CliffSkeleton_BD`) | 밤 | 260 / 12 | 선공 | A1 베기 70 / A2 방패 자세→내려찍기 90 | 0.2 | – |
| **엘리트** Bringer (`DeathBringer_BD`) | 밤 | 2500 / 15 | 선공 | 낫 휘두르기 110 / Spell(플레이어 위치 소환) 90 | 0.25 | ✅ |
| **보스** Evil Wizard (`AbyssWizard_BD`) | 밤 | 8000 / 20 | 거리 4~10 유지, 2페이즈 | A1 마검 130 / A2 돌진베기 150 / 원형 탄막 12발 60 | 0.3 | ✅ (isBossMob) |

- 감지 범위는 기존과 같이 20×10이다. 보스는 PassiveTerror처럼 화면 고정 HP바를 쓴다.
- 엘리트는 일반 HP바를 쓰고 hpHeight를 2.4로 올린다.

### 3.2 BT 구성 (기존 BD_ 태스크만 사용)

| 몬스터 | 복제할 템플릿 | 변경점 |
|---|---|---|
| Fox | `Rabbit_BD` | 도주 분기를 그대로 쓴다. RunAway가 끝나지 않는 버그(B-110)는 그대로 상속 → C1 수정 시 함께 해결 |
| Mushroom | `LeftMole_BD`(반격 + 유도탄) | 공격 Selector를 둘로 나눈다. 근거리(AttackRange 1.5u)면 A1 `EnemyAttackNode(Bite)`, 아니면 A2 `EnemyAttackNode(Spore, isLaunch 포물선)` |
| Goblin | `RatThief_BD`(선공 근접) | Wait 0.5를 **0.35**로 줄인다. A2는 `Sequence[Attack→Wait 0.2→Attack]`로 2연타. EnemyJump(vy 10) 유지 |
| Flying eye | `FlowerMole_BD`(선공 유도) | Rigidbody2D gravityScale 0. `EnemyGroundChecker`·낭떠러지 분기와 `EnemyJump`를 삭제한다(지면이 없어 계속 반전됨). 추격은 DOMoveX라 **높이를 유지**한다. 같은 층이면 A1, 아니면 A2 유도탄 |
| Skeleton | `RatChief_BD` | A2 직전에 `SetBool(isGuard)` + Wait 0.5(Shield 애니) = **텔레그래프**. Def 12로 방패를 표현한다(가드 중 방어 상승은 신규 태스크가 필요 → 보류) |
| Bringer | `PassiveTerror_BD`(보스 트리) | HP바 ON/OFF·카메라 흔들림 노드를 뺀다. `RandomSelector`[낫(근접) / Cast→Spell]. Spell은 `EnemyAttackNode(isSummonPlayerPosX)`로 **AttackSpawon** 프리팹을 소환한다(0.6초 경고 후 판정) |
| Evil Wizard | `PassiveTerror_BD` | 접촉 연출·HP바 ON 유지. `EnemyHealthChecker(healthTreshold 4000)`로 **2페이즈**를 나눈다. 1페이즈 [A1 / 원형탄 8발]. 2페이즈 [A1 / A2 돌진 / 원형탄 12발 + 기둥 3개(AttackSpawon)], Wait 1.0→0.6. `EnemyJump`로 양옆 발판(y=4)에 오른다(Jump/Fall 애니) |

- **텔레그래프(C2 미구현):** 코드 추가 없이 두 방식으로 해결한다.
  - 근접: `SetBool(isAttack)` → `Wait(윈드업)` → `EnemyAttackNode`. Luiz Melo Attack 8프레임은 4~5번째가 타격이다. 12fps면 **0.35~0.4초**다.
  - 소환형: `AttackSpawon`(경고 스프라이트를 켜고, summonTime 0.6초 뒤 판정 자식을 켬). 기존 DPAttackSpawon 패턴이다.
  - 느낌표 아이콘은 C2에서 따로 구현한다.

### 3.3 공격 프리팹 (`Assets/Prefab/Enemy/EnemyDamge/Lv2/`)

> **구현(2026-10-18):** `Tools/probes/build_lv2_attacks.cs` — 템플릿 공격을 `L2_*`로 복제해 피해만 바꾸고, Lv2 스포너 objectInfos의 **같은 키**(leaf/stab/flower/clow/fire)에 끼운다(BT attackName 무변경). 실제 키가 하나뿐이라 근접·원거리 2종 구분은 아직 없음: Shroom 40(45/35 절충)·Goblin 55·Eye 45(50/40 절충)·Skel 70·Bringer 110/60·Wizard 130/60. 정예 스포너(L2_C_EliteN)는 `eliteChance=0`. 맵 재빌드 시 프로브 재실행.

| 키(attackName) | 복제 원본 | damage | 비고 |
|---|---|---|---|
| ShroomBite / GoblinSlash / EyeBite / SkelSlash | `Bite(Enemy)` | 45 / 55 / 50 / 70 | 근접. 크기는 몸통 폭 +0.5u |
| GoblinDouble | `Bite(Enemy)` | 35 | 2회 호출 |
| SkelSmash | `Bite(Enemy)` | 90 | knockForce를 높인다 |
| ShroomSpore | `ScareCrowAttackPrefab` | 35 | isLaunch, isGravityFall, isGroundDestroy |
| EyeGaze | `LeafAttackPrefab`(유도) | 40 | |
| BringerScythe | `ClowAttack` | 110 | Attack 10프레임 중 6번째 |
| BringerSpell | `DPAttackSpawon` | 90 | 경고 = Spell 시트 1~6프레임, 판정 = 7~12프레임. summonTime 0.6, deletTime 1.6 |
| WizardSlash / WizardRush | `ClowAttack` | 130 / 150 | |
| WizardOrb | `PassiveTerrorAttackPrefab` | 60 | `EnemyRoundAttackNode` bulletCount 8/12, angle 0~180 |
| WizardPillar | `DPAttackSpawon` | 80 | 플레이어 x ±3 위치에 3개 |

- 각 스포너의 `objectInfos`에 `enemy`, `dead`, 공격 키를 등록한다. 보스와 엘리트의 탄은 count 12 이상으로 둔다.
- 사망 연출: `RatThiefDeadBody`를 복제해서 Death 클립을 재생하고, 끝나면 기존 디졸브를 건다.
  - Fox는 Death 애니가 없다 → 마지막 Walk 프레임 + 디졸브.

### 3.4 드롭 테이블 (스포너 DropManager, 독립 판정)

| 몬스터 | 재료 | 장비 |
|---|---|---|
| Fox | Cliff Fox Pelt 15%, Fox Meat 20% | CliffShoes 0.2%, CliffHandArmor 0.1% |
| Mushroom | Glowing Spore 15%, Spongy Mushroom Cap 10%, GreenHurb 2% | CliffCap 0.2% |
| Goblin | Goblin Fang 15%, Tarnished Goblin Coin 5% | CliffLegArmor 0.2%, CliffBlade 0.08% |
| Flying eye | Membrane Wing 15%, Bloodshot Lens 5% | CliffStaff 0.08%, Eye of the Cliffs 0.05% |
| Skeleton | Ancient Bone 15%, Cracked Shield Shard 8% | CliffArmor 0.15%, CliffHandArmor 0.2% |
| 엘리트 Bringer | Death Sigil 60%, Ancient Bone 100% | CliffArmor 5%, CliffBlade 4%, CliffStaff 4%, Bringer's Sigil Ring 3% |
| 보스 Evil Wizard | Abyssal Core 100%, Death Sigil 50% | Cliff 방어구 각 15%, CliffStaff/CliffBlade 각 10%, Eye of the Cliffs 5%, **AbyssStaff 3%** |
| Lv2 Rock | T1_Ore 100%, T0_Ore 50% | – |

## 4. 신규 세트 "Cliffwalker's Vow" (tfName `CliffArmor`)

### 4.1 ItemSheet.csv 행

- placeholder 행 81~104(`name='-'`)를 **제자리에서 교체**한다. 행 수와 index가 유지된다.
- 컬럼 순서는 `index,name,type,description,price,weight,acqPath,maxStack,effectOb,effectPow,equipArea,tfName,itemNameT,tear,rarity,upgrade,setName`이다.
- description에는 쉼표를 쓰지 않는다. tear는 사용처가 확인되지 않아 0으로 둔다.

```csv
81,T1_Ore,Misc,World,200,1,-,99,-,0,-,,Cliff Crystal Ore,0,C,0,
82,Cliff Fox Pelt,Misc,World,150,1,-,99,-,0,-,,Cliff Fox Pelt,0,C,0,
83,Fox Meat,Misc,World,120,1,-,50,-,0,-,,Fox Meat,0,C,0,
84,Glowing Spore,Misc,World,150,1,-,99,-,0,-,,Glowing Spore,0,C,0,
85,Spongy Mushroom Cap,Misc,World,150,1,-,99,-,0,-,,Spongy Mushroom Cap,0,C,0,
86,Goblin Fang,Misc,World,150,1,-,99,-,0,-,,Goblin Fang,0,C,0,
87,Tarnished Goblin Coin,Misc,World,300,1,-,99,-,0,-,,Tarnished Goblin Coin,0,R,0,
88,Membrane Wing,Misc,World,150,1,-,99,-,0,-,,Membrane Wing,0,C,0,
89,Bloodshot Lens,Misc,World,300,1,-,99,-,0,-,,Bloodshot Lens,0,R,0,
90,Ancient Bone,Misc,World,150,1,-,99,-,0,-,,Ancient Bone,0,C,0,
91,Cracked Shield Shard,Misc,World,300,1,-,99,-,0,-,,Cracked Shield Shard,0,R,0,
92,Death Sigil,Misc,World,800,1,-,99,-,0,-,,Death Sigil,0,R,0,
93,Abyssal Core,Misc,World,3000,1,-,99,-,0,-,,Abyssal Core,0,U,0,
94,Roasted Fox Skewer,Consum,eat!,150,5,cook,50,fullness +/def +,150/8_180,-,,Roasted Fox Skewer,0,R,0,
95,CliffCap,Equip,equipment,1200,50,Craft,1,-,0,Head,CliffArmor,Cliffwalker Hood,0,R,0,Cliffwalker's Vow
96,CliffArmor,Equip,equipment,1500,50,Craft,1,-,0,Chest,CliffArmor,Cliffwalker Mail,0,R,0,Cliffwalker's Vow
97,CliffLegArmor,Equip,equipment,1200,50,Craft,1,-,0,Leg,CliffArmor,Cliffwalker Greaves,0,R,0,Cliffwalker's Vow
98,CliffHandArmor,Equip,equipment,1200,50,Craft,1,-,0,Hand,CliffArmor,Cliffwalker Gloves,0,R,0,Cliffwalker's Vow
99,CliffShoes,Equip,equipment,900,50,Craft,1,-,0,Shoes,CliffArmor,Cliffwalker Boots,0,R,0,Cliffwalker's Vow
100,CliffStaff,Equip,equipment,1500,2,Craft,1,-,0,Weapon,CliffArmor,Cliffwind Staff,0,R,0,Cliffwalker's Vow
101,CliffBlade,Equip,equipment,1500,2,Craft,1,-,0,Weapon,CliffArmor,Cliffwind Blade,0,R,0,Cliffwalker's Vow
102,AbyssStaff,Equip,equipment,5000,2,Boss,1,-,0,Weapon,,Abyssal Wizard Staff,0,U,0,
103,Eye of the Cliffs,Equip,equipment,800,1,-,1,-,0,Necklace,,Eye of the Cliffs,0,R,0,
104,Bringer's Sigil Ring,Equip,equipment,1500,1,-,1,-,0,Ring,,Bringer's Sigil Ring,0,U,0,
```

### 4.2 장비 스탯 (프리팹 `Equipment` / `Weapon` 필드)

| 아이템 | 스탯 |
|---|---|
| CliffCap / CliffLegArmor | armor 15, hp 150 |
| CliffArmor | armor 30, hp 400 |
| CliffHandArmor | armor 15, hp 150, critical 3 |
| CliffShoes | armor 15, hp 150, moveSpeed 5 |
| (방어구 합) | **Def 90 / HP 1000** (Leaf: 60 / 700) |
| CliffStaff | min 14 / max 24, 치명 5, 스킬뎀 20 |
| CliffBlade | min 16 / max 24, 치명 10, 치피 20 |
| AbyssStaff | min 22 / max 34, 스킬뎀 30, 최종뎀 10 (세트 외 유니크, 보스 전용) |
| Eye of the Cliffs | critical 5, dropRate 10 |
| Bringer's Sigil Ring | criticalDmg 15, incDmg 5 |

### 4.3 무기 스킬 (A/S/D/F, `Skill.skillName`은 세트 효과의 스킬명과 정확히 일치해야 함)

| 무기 | 슬롯 | skillName | 방식 | 쿨/스태미나 | 세트 강화 지점 |
|---|---|---|---|---|---|
| CliffStaff | A | Arcane Bolt | Normal, 발사체 | 1.5s / 5 | – |
| | S | Starfall | **Round** bulletCount 3, angle 60~120 | 6s / 10 | bulletCount |
| | D | Cliff Quake | **다단** Ground, objectMaxCount 3, interval 0.2 (전방 2u 간격) | 8s / 12 | skillHitCount |
| | F | Mana Tide | 홀드 빔(isHoldSkill), 다단히트 | 15s / 20 | coolDown |
| CliffBlade | A | Gale Slash | Normal 근접 | 1s / 5 | – |
| | S | Cyclone Edge | **다단** objectMaxCount 3, interval 0.12 | 5s / 10 | skillHitCount |
| | D | Talon Dive | 대시 공격(DamageObject 대시 옵션) | 7s / 10 | coolDown |
| | F | Skybreaker | **Round** bulletCount 3 (전방 부채꼴 풍인) | 12s / 15 | bulletCount |

- 🔴 **skillHitCount는 `isEffectMaxAdd=1`인 다단 스킬에만 먹는다.**
  - 실제 개수는 `min(objectMaxCount + 보너스, skillprefab/skillPivot 길이)`다(`Skill.cs:187-190`).
  - 그래서 Cliff Quake는 배열 길이 **5**(3+2), Cyclone Edge는 **4**(3+1)로 만든다.
- bulletCount는 `isRoundAttack=1`에만 먹는다.

### 4.4 SetItem.csv 행

```csv
CliffArmor_2,(+)15 Def/(+)5 Crit
CliffArmor_4,(+)2 bulletCount Starfall/(+)1 skillHitCount Cyclone_Edge/(+)10 IncDmg
CliffArmor_6,(+)2 skillHitCount Cliff_Quake/(+)2 bulletCount Skybreaker/(+)30 coolDown Mana_Tide/(+)30 coolDown Talon_Dive/(+)20 CritDmg
```

- 방어구 5부위만 입으면 2·4단계까지 켜진다. **무기까지 입은 6부위에서만 6단계**가 켜진다. "무기 스킬 + 세트 강화" 방향을 그대로 보여 준다.
- 단계는 정확히 일치할 때 켜지고 누적된다(ItemSpec 3절).
- 장착하지 않은 무기의 스킬을 지목한 효과는 아무 일도 하지 않는다(사전 키만 생김).

### 4.5 제작 레시피 (씬 `NeedItem` 오브젝트, 방어구/무기 대장장이·자가 제작)

| 결과물 | 재료 (수량) | UI |
|---|---|---|
| CliffCap | T1_Ore 8, Wood 10, Glowing Spore 12, Cliff Fox Pelt 6 | 방어구 |
| CliffArmor | T1_Ore 15, Wood 15, Ancient Bone 12, Cracked Shield Shard 4 | 방어구 |
| CliffLegArmor | T1_Ore 8, Wood 10, Goblin Fang 12, Cliff Fox Pelt 6 | 방어구 |
| CliffHandArmor | T1_Ore 8, Wood 8, Membrane Wing 10, Goblin Fang 6 | 방어구 |
| CliffShoes | T1_Ore 6, Wood 8, Cliff Fox Pelt 10, Spongy Mushroom Cap 6 | 방어구 |
| CliffBlade | T1_Ore 15, Wood 10, Goblin Fang 15, Tarnished Goblin Coin 5 | 무기 (**낮 재료만** → 첫 관문 무기) |
| CliffStaff | T1_Ore 12, Wood 20, Glowing Spore 15, Bloodshot Lens 4 | 무기 |
| AbyssStaff | Abyssal Core 3, Death Sigil 5, T1_Ore 30 | 무기 (보스 3% 천장 보정) |
| Eye of the Cliffs | Bloodshot Lens 10, Membrane Wing 10, T1_Ore 5 | 방어구 |
| Bringer's Sigil Ring | Death Sigil 8, T1_Ore 10 | 방어구 |
| Roasted Fox Skewer | Fox Meat 2, Wood 1 | 자가 |

## 5. 구현 단계 (빌더용)

- 원칙은 `Framework/EditorAutomation.md`를 따른다(창 포커스 금지, 대화상자 없는 API, unity-mcp `execute_code`/프로브).
- 단계마다 **프로브 결과를 커밋 메시지에 적는다.** 확인하지 못했으면 "미검증"이라고 적는다.

| 단계 | 작업 | 검증 프로브 (기대값) |
|---|---|---|
| 5.1 임포트 | `AssetDatabase.ImportPackage(path, false)`로 MonstersCreaturesFantasy, Evil Wizard 2, Bringer Of Death free, Magic Cliffs, RPG Monsters Icon **5개만** 가져온다(Martial Hero·Medieval King·Underwater는 보류). 원래 경로(`Assets/Monsters Creatures Fantasy`, `Assets/GothicVania`, `Assets/Layer Lab` …)에 풀리므로 `AssetDatabase.MoveAsset`으로 `Assets/ThirdParty/{MonstersCreaturesFantasy,EvilWizard2,BringerOfDeath,MagicCliffs,RPGMonstersIcon}`로 옮긴다. 데모 씬(`Demo/`, `Scene demo/`, `Demo.unity`, `PreviewScene_*`), `Magic-Cliffs-Girl-Addon-1·2`, LAYERLAB `Scripts/`·`bg.png`, 팩의 데모 `.controller/.anim`은 `DeleteAsset` | 신규 `Tools/probes/lv2_import.cs`: 폴더별 Texture2D 수, 원래 경로 잔존 0, `.unity`/`.cs` 0, 콘솔 에러 0 |
| 5.2 텍스처 설정 | 에디터 스크립트로 `TextureImporter` 일괄 설정: 캐릭터 PPU 24 · 타일/패럴랙스/장식 PPU 16 · 아이콘 PPU 32(Single). 공통 `filterMode=Point`, `textureCompression=Uncompressed`, mipmap off, 기존 슬라이스 유지 | `lv2_texcheck.cs`: 규칙 위반 importer 0줄 |
| 5.3 애니메이션 | 슬라이스 스프라이트로 `AnimationClip` 생성(12fps, Bringer 10fps, `EditorCurveBinding` m_Sprite). Stay·Walk·Hit loop, Attack·Death loop off. `Assets/Sprite/Enemy/<Monster>/`에 controller와 함께 둔다. `RatThifeAnim.controller` 구조(`isWalk/isAttack/isHit`) + `isAttack2`, `isGuard`(Skeleton), `isCast`(Bringer), `isJump`(Wizard). DeadBody용 Death controller 별도. 아틀라스 `Assets/Atlas/<Monster>Atlas.spriteatlas` | `lv2_anim.cs`: controller별 파라미터 이름·상태 수·clip 길이 > 0·첫 키 스프라이트 not null |
| 5.4 몬스터 프리팹 | 3.2절 템플릿을 `AssetDatabase.CopyAsset`으로 `Assets/Prefab/Enemy_BD/<새이름>_BD.prefab`에 복제 → `PrefabUtility.LoadPrefabContents`로 수정: 루트 scale 1, sprite = Idle 0번, Animator.controller, `EnemyHealth`(maxHP, enemyDef, isSuperArmor, isBossMob, hpHeight, deadBody), BoxCollider2D = 1프레임 알파 bbox ÷ 24, 자식(GroundCheck 발 y -1.08, WallCheck, AttackPivot, AttackRange). Bringer는 SpriteRenderer.flipX=1. **BT 필드**(attackName, damageOb, Wait, Patrol xMin/xMax/originMax, healthTreshold)는 BD JSON에 있다 → `BehaviorSource.CheckForSerialization(true)` → 태스크 필드 변경 → `JSONSerialization.Save` → `SetDirty`. 실패하면 사용자가 BD 에디터에서 고칠 목록을 `Docs/OpenQuestions.md`에 남긴다(창을 띄우지 않음). 공격 프리팹(3.3절)도 복제 후 `damage`·콜라이더 수정 | `lv2_prefabs.cs`: 프리팹별 maxHP/enemyDef/controller/deadBody/behaviorName 출력, BT JSON의 `attackName` 목록과 스포너 objectInfos 키 대조 → 누락 0 |
| 5.5 맵·스포너·포털 | 에디터 스크립트로 `Lv2_MagicCliffs` 생성: Grid(cell 1), `Tilemap_Visual`, 충돌 오브젝트(BaseGround 바닥 / 원웨이 BoxCollider 발판), Ladder 복제, 패럴랙스 4장. 레이아웃은 C# 배열(발판 rect, 사다리 위치)로 두어 재생성 가능하게. 기존 스포너를 복제해 2.3절 값(enemyCount, cycleFloat, isDaytimeSpowner, `enemyPositon` 자식, objectInfos)과 3.4절 dropItems 입력. 포털 3개(SideRoad↔Lv2, C3→D)에 `destination/openMap/closeMap/proCamera/player` 연결, 카메라 경계 추가. 루트는 비활성으로 두고 `EditorSceneManager.SaveScene` | `portals.cs`: 새 포털 dest/open/close not NULL. 플레이 → `use_portal.cs` → `state.cs`(활성 맵·위치). `tp_spawner.cs`(KEY=`L2_B_Goblin`) 5초 후 EnemyHealth ≥ 1. `set_night.cs` → `tp_spawner.cs`(`L2_D_BossN`) → 보스 1 + 고정 HP바. `playtest.sh 10 lv2` 스크린샷으로 플레이어·Goblin 키 비교. 하향 점프·로프는 사용자 확인(입력 주입 금지) |
| 5.6 아이템·세트·제작 | `ItemSheet.csv` 81~104행 교체, `SetItem.csv` 3행 추가. 아이콘 `Resources/Item/{name}.png`는 기존 아이콘 색조 회전 임시본(몬스터 초상 팩은 재료 아이콘에 쓰지 않음). 방어구 `Resources/{부위}/Cliff*.prefab`은 Leaf 복제 + 4.2절 스탯. 무기 `Resources/Weapon/Cliff{Staff,Blade}·AbyssStaff.prefab`은 LeafStaff·ScareSide 복제 + 4.3절 스킬, DamageObject는 `Resources/Weapon/DamageObject/Cliff*/`. `Resources/Sprite/CliffArmor.png`는 ScareArmor.png 색조 변환. 대장장이 UI 아래 `NeedItem` 11개(4.5절) | EditMode 테스트: ItemSheet 전 행 17컬럼·name 중복 0·Equip 행의 `Resources/{equipArea}/{name}`과 `Resources/Item/{name}` 존재, `SetEffectApplier.Parse(CliffArmor_6)` 5항목·`_`→공백. 플레이: `equip_weapon.cs`/`inv_test.cs`로 5부위 → playerDef +15, +CliffBlade → `skillBulletCount["Skybreaker"]==2`, `skillCoolDown["Talon Dive"]==30`, 해제 시 0. `kill_one.cs`/`after_kill.cs`로 드롭 키 일치(경고 0) |
| 5.7 문서 | `EnemySpec.md` 1·3·4절, `ItemSpec.md` 3절, `WorldSpec.md` 2절, `AssetInventory.md` 6절, `QualityRoadmap.md` E4 갱신 | – |

## 6. RPG Monsters Icon 활용

| 용도 | 내용 | 연결 |
|---|---|---|
| **몬스터 도감**(신규 UI) | 처치 수, 출현 맵·시간대(낮/밤 아이콘), 발견한 드롭 목록. `shadow` 32×32를 ×2 정수 배율로 표시 | 처치 = `EnemyHealth` 사망 처리, 키 = 프리팹 이름 |
| 드롭·픽업 로그 (D6) | "Goblin → Goblin Fang" 줄 앞에 출처 몬스터 아이콘 | DropManager가 몬스터 키를 함께 넘김 |
| 포털·사냥터 표지판 | 포털 위에 서식 몬스터 아이콘 나열(밤 전용은 달 테두리) | 포털 자식 SpriteRenderer, PPU 32 |
| 보스·엘리트 HP바 초상 | 화면 고정 HP바 왼쪽 | `EnemyHPBarON` 대상 UI |

- **아이콘 매핑**(근사치, 임포트 후 눈으로 확정): Mushroom `forest_402`, Goblin `jungle_204`, Flying eye `field_507`, Skeleton `undead_104`(방패 해골), Bringer `undead_112`(사신), Evil Wizard `devil_711` 또는 `undead_110`, Fox `jungle_208`.
- Lv1 몬스터(Rabbit·Mole·Rat·Scarecrow)는 맞는 아이콘이 없다 → 근사 아이콘이나 기존 스프라이트 축소본을 쓴다.
- `min size`(트림본 10~29px)는 미니맵 마커·스포너 디버그 기즈모 전용이다.

## 7. 열린 질문 (임시 가정)

| # | 질문 | 임시 가정 |
|---|---|---|
| Q-L1 | 캐릭터 PPU 24와 타일 PPU 16의 텍셀 밀도 차이 | 허용. 거슬리면 타일 PPU 24 + Grid cell 0.667 |
| Q-L2 | 세트를 6부위(무기 포함) 2/4/6단계로 할지, 2/5로 할지 | 2/4/6, 6단계는 무기 필수 |
| Q-L3 | CliffArmor AnyPortrait 텍스처·아이콘 정식 아트 | 색조 변환 임시본 |
| Q-L4 | Lv2 입구(SideRoad 서쪽 끝 vs 마을) | SideRoad 서쪽 끝 |
| Q-L5 | Fox(피격·사망 애니 없음) 포함 여부 | 포함(셰이더 플래시·디졸브). 빼면 일반 4종 |
| Q-L6 | Lv1 무기 5~10뎀이 확정값인가 | 확정으로 보고 Lv2 HP 결정. 바뀌면 2.4절 재검산 |
