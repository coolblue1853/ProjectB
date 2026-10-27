# 스컬 원본 에셋 라이브러리 (참고 전용)

사용자가 보유한 스컬(Steam) 정품에서 스프라이트를 추출해 분류한 **참고 자료**다. 모션·이펙트·적·배경·UI 작업의 기준으로 쓴다.

## 0. 사용 규칙 (반드시)

- **참고만 한다.** 스컬 그림을 그대로 쓰거나, 따라 그리거나, 저장소·게임·PixelLab 입력에 넣지 않는다. 형태·프레임 수·타이밍·구성 원리만 가져온다.
- 원본은 **저장소 밖** `C:\Users\user\SkulRef\`에만 둔다. 커밋하지 않는다.
- 캐릭터 형태(2등신, 장비 레이어)는 우리 것이다. 스컬에서 가져오는 것은 **동작 문법**과 **이펙트 문법**이다.
- 비교 시트는 `Docs/ArtCompare/` 에 두되, 스컬 프레임이 들어간 비교 이미지는 커밋하지 않는다(로컬 `SkulRef/compare/`).

## 1. 위치와 도구

| 항목 | 경로 |
|------|------|
| 원본 스프라이트(애셋 이름 @PathID) | `C:\Users\user\SkulRef\spr\<번들>\<CAB>\` |
| 스프라이트 메타(크기·피벗·PPU·아틀라스) | `C:\Users\user\SkulRef\meta.json` |
| **분류 라이브러리** | `C:\Users\user\SkulRef\lib\` (`summary.md` 분류표, `index.json` 애니별 상세) |
| 추출 도구 | AssetStudioModCLI v0.19.0 .NET 8 (`SkulRef\tools\asm8\`, sha256 a16a1cb9…) |
| 메타 생성 / 분류 스크립트 | `Tools/skulref/meta.py`, `Tools/skulref/classify.py` |

다시 만들 때: `meta.py sprdump meta.json` → `classify.py C:/Users/user/SkulRef`.
분류 단위는 **아틀라스 한 벌**(캐릭터 1명·이펙트 팩 1개)이다. 이름만으로 나누면 `AttackA` 같은 이름이 여러 캐릭터에 겹쳐서 섞인다.

## 2. 분류 체계 (lib 폴더 구조)

총 스프라이트 107,791장 → 묶음 2,956개 → 애니 약 15,000개.

### character/ — 묶음마다 폴더, 동작 번호순 띠 + `_sheet.png`

| 소분류 | 묶음 수 | 내용 |
|---|---|---|
| `player_head` | 약 124 | 머리(스컬) 본체. 같은 머리의 등급·각성은 별도 묶음 |
| `enemy` | 78 | 일반 몹 |
| `boss` | 51 | 보스·정예(스프라이트 250장 이상이거나 각성·페이즈 애니 보유) |
| `hero_enemy` | 5 | 용사·모험가형 적 |
| `npc` | 3 | NPC |

묶음 안 파일 접두어(동작 번호): `01_idle` `02_walk_run` `03_dash` `04_jump_fall` `05_jump_attack` `06_attack_basic` `07_skill` `08_swap` `09_guard_parry` `10_hit_dead` `11_named_move`(고유 동작 — RisingSlash, Iaido, Parry 등)

### effect/ — 속성·형태 22종 + 기타

`slash_arc`(베기 호) `thrust_pierce`(찌르기) `impact_hit`(타격) `explosion` `projectile` `beam_laser` `electric` `fire` `ice` `water` `poison_curse` `dark_soul` `holy_light` `nature` `aura_buff` `smoke_dust` `debris_blood` `sign_warning`(경고·표식) `portal_summon` `trail_wind`(잔상·바람) `chain_rope` `generic` `unnamed_anim`(이름 없는 3프레임 이상)

### background/ · ui/ · item/ · unknown/

- background: `tile_ground` `prop_nature` `prop_structure` `prop_object` `layer_sky` `misc`
- ui: `icon_skill` `icon_item_frame` `icon_misc` `portrait` `hud_gauge` `button_key` `window_frame` `font_number` `title_loading`
- item: `drop_currency` `food_consumable`
- unknown/still: 이름이 숫자뿐인 정지 이미지(컷신 파츠·목업 등). 필요할 때 시트로 확인

## 3. 작업 기준 (측정값)

### 3.1 공통

- **PPU 32**(15,048/15,105 애니), 피벗 중앙, 캐릭터 캔버스 200×150.
- 몸 크기는 작고 이펙트가 크다. 공격 모션의 "읽힘"은 몸보다 **스미어 이펙트**가 담당한다.

### 3.2 플레이어 머리 124벌 프레임 수(중앙값, 최소~최대)

| 동작 | 중앙값 | 범위 | 우리 기준 |
|---|---|---|---|
| Idle | 6 | 4~42 | 4~6 |
| Walk | 8 | 5~72 | 6~8 |
| Dash | 4 | 1~10 | 1~4 |
| Jump / Fall / FallRepeat | 3 / 2 / 3 | | 같게 |
| JumpAttack | 6 | 3~20 | 4~6 |
| AttackA / AttackB | 7 / 7 | 4~28 | **4~7** |
| AttackC (3타 보유 58벌) | 8 | 3~19 | 5~8 |
| Skill | 7~9 | | 6~10 |
| Swap(Switch) | 7 | | 7 |

### 3.3 리틀 본(기본 스컬) 평타 — 올려베기·내려베기의 정답 표본

`lib/character/player_head/LittleBone_*/` (번들 57c1fb, 본편 sharedassets2 두 벌)

- **AttackA 5프레임(내려베기)**: ①② 몽둥이를 어깨 뒤로 든 준비 2장 → ③ **몸을 덮는 꽉 찬 흰 초승달**(망토 펄럭임만 보임) → ④ 앞 아래로 끝난 몽둥이 + 얇은 호 잔상 → ⑤ 몽둥이를 다시 든 회복.
- **AttackB 4프레임(올려베기)**: ① 몽둥이 뒤·아래 준비 → ② **아래 앞에서 위 뒤로 감기는 큰 초승달**(몸 대부분 가림) → ③ 머리 위를 지나는 얇은 호 + 몽둥이 위·뒤 → ④ 몽둥이를 머리 위로 든 채 정지.
- **JumpAttack 4프레임**: 공중 웅크림 → 큰 초승달 → 얇은 호 → 몽둥이 아래.
- 몸 자세 변화는 작다(웅크림 1~2px, 다리 벌림 정도). **찌르기처럼 팔을 뻗는 프레임이 없다.** 우리 올려베기 v2가 "찌르기 같다"는 지적을 받은 원인이 이것이다.
- 초승달 문법: 꽉 찬 면(흰색 1~2단 명암) 1프레임 → 얇은 호(2~3px) 1프레임 → 사라짐. 호의 바깥이 두껍고 안쪽 끝이 가늘다.

### 3.4 앞으로의 사용법

1. 모션을 만들기 전에 `lib/character/player_head/`에서 같은 계열 동작을 가진 머리를 2~3벌 찾는다(예: 올려베기 → 리틀 본 AttackB, RisingSlash 계열, VerticalUpSlash).
2. 프레임 수·키 자세·스미어 위치를 표로 적고, 우리 셀을 그 표대로 만든다.
3. 판정은 스컬 띠와 우리 셀을 같은 배율로 나란히 놓고 한다(MotionSet §6, 한 번에 하나씩 보여주기).
4. 이펙트는 `effect/<속성>/`에서 형태·프레임 수를 찾아 PixelLab·자체 이펙트 사양에 적는다.
5. 적·배경·UI도 해당 폴더를 먼저 본다. 초반 콘텐츠는 소박하게(early-game-modest).
