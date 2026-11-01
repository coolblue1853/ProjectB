# 던전 슬래셔 레퍼런스 라이브러리 (11-01)

> 사용자: "여기가 도트인데 모션이랑 이펙트를 잘 찍었더라. 스킬 이펙트·캐릭터·캐릭터 모션·UI 다 뽑아오자." 약간 미소녀 계열이지만 주인공 그림체 후보.

## 규칙
- **참고 전용.** 추출물은 저장소 밖 `C:\Users\user\DungeonSlasherRef`. 저장소·빌드·커밋에 넣지 않는다(SkulRef·LimbusRef 와 같은 규칙).
- 그대로 옮기지 않는다. 키 자세·프레임 수·속도(sampleRate)·이펙트 형태·색 쓰는 법을 관찰해 우리 것으로 다시 만든다.
- PixelLab `style_image` 로는 **그림체(비율·외곽선·음영)만** 빌린다. 캐릭터 디자인은 우리 것(TowerWorld 주인공).

## 폴더
| 폴더 | 내용 |
|---|---|
| `raw/` | xapk → apk → `data/pack`(UnityDataAssetPack, 번들 72개), `data/base` |
| `tex/` | Texture2D 4897장(아틀라스 `sactx-…-<이름>`, 배경·UI 원본) |
| `spr/` | Sprite 55,656프레임, SpriteAtlas 이름별 폴더 2242개(`atlas_<캐릭터·보스·맵>`) — 프레임 이름은 숫자뿐 |
| `clips/` | **AnimationClip 8920개**(중복 번들 포함 45,064회 기록): `<클립>_<id>/NN.png` 프레임 순서대로 + `_strip.png` + `_info.txt`(frames·sampleRate) |
| `tools/` | `export.py`(텍스처), `export_sprites2.py`(스프라이트), `export_clips.py`(클립), `sheet_char.py <clips> <접두어> <out>`(캐릭터 모션 시트), `idle_sheet.py`(전 캐릭터 대기 모음) |
| `clips_index.csv` | 클립 색인: category(player·monster·boss·effect·weapon·npc·ui·pet_summon·map_object·other)·frames·fps. `tools/index_clips.py` |
| `previews/player_<직업>_00.png` | 직업별 스킨 00 전 모션 시트(00~09, P10~P22 — 신직업 P11=Guardian 등 66클립) |
| `preview_*.png` | 모음 시트(대기 66종 `preview_idle_all.png`, 기본 기사 `preview_00_00*.png`) |

## 관찰 (첫 정리)
- 플레이어 클립: 구직업 `NN_SS_<모션>`(00~09), 신직업 `PNN_SS_<모션>`(P10~P22, Guard·Parrying·Jump_Attack·Over_* 등 모션이 더 많다). 구직업 클립 이름 `NN_SS_<모션>` = 직업 NN · 스킨 SS. 직업 00~09 × 스킨, 대기 66종. 그 밖 직업(Guardian·Hunter·Ronin·Predator·Werewolf·Summoner·Assassin·Vampire·Dragonian·Berserker·Shaman·Inquisitor…)은 아틀라스 이름으로 있다.
- 기본 기사(00_00) 모션: Idle 6 · Run · Jump 3 · Down 3 · Dash 6 · Attack01 4 · Attack02 4 · Hit 1 · DieN 5 · Climb 1 · Sit · WeaponSkill000/001/006(Do·End), **sampleRate 12**(프레임당 0.083초). 우리 기본 3타(0.04~0.055초)보다 프레임 수가 적고 키 포즈 위주다.
- 캐릭터 키 약 45~55px, 약 3.5등신, 얇은 짙은 외곽선, 2~3단 음영. 이펙트는 캐릭터와 분리된 프레임(흰 고리·선·불꽃 줄기 등)으로 같은 아틀라스에 있다.
- 보스·몹 = `B###`·`E###`·`M###` 아틀라스, 맵·타일 = `Map`·`Tile`·`0.Castle_*` 등.

## 플레이어 모션 통계 (클립 색인, 11-01)

프레임 수 = 클립이 참조하는 서로 다른 스프라이트 수(같은 장 유지 시간은 미반영). fps = 클립 sampleRate.

| 모션 | 클립 수 | 프레임 중앙값(p25~p75) | fps |
|---|---|---|---|
| 평타 Attack | 463 | 4 (3~4) | 12 |
| 스킬 Skill·WeaponSkill | 633 | 5 (4~8) | 12 |
| 점프 공격 | 96 | 4 (3~6) | 12 |
| 대기 Idle | 172 | 8 (7~8) | 12 |
| 달리기 Run | 149 | 8 | 12 |
| 대시 Dash | 153 | 5 (3~6) | 12 |
| 점프·낙하 Jump·Down | 161 | 3 | 12 |
| 피격 Hit · 사다리 Climb | 155 | 1 | 12 |
| 사망 Die | 84 | 7 (6~8) | 12 |

- 시사점: 평타 1타를 **3~4장 × 0.083초**로 끝낸다(약 0.3초). 장수 대신 키 포즈와 별도 이펙트 프레임으로 손맛을 낸다. 일부 신직업은 Attack01_Ready → Attack → AttackEnd 로 준비·마무리를 클립으로 나눴다(fps 100 표기는 타임라인 제어용으로 추정).
- 우리 기본 3타(8~12장, 0.04~0.055초)는 더 촘촘하다. 이 그림체로 가면 장수를 줄이고 키 포즈를 강하게 하는 쪽이 생성 비용(PixelLab)에도 유리하다.

## PixelLab 시험
- ds01(11-01): `create_image_pro_flash` 64×64, style_image = 01_05 대기 첫 프레임(그림체만), 우리 주인공 설명(회금발 포니테일·라벤더 망토·청록 고글·네 갈래 별 브로치·가죽 재킷·단검). 결과 54px·약 3.5등신, 원작보다 외곽선이 진하다. 5 gen. 파일 `Docs/ArtCompare/CharacterGen/ds01/raw_6001_explorer.png`. **사용자 판정 대기.**
- 사용자(11-01) ds01 평: "비슷한 그림체인데 느낌이 좀 다르다". 원인 분석: 검정 굵은 외곽선(원작은 부위색의 어두운 톤), 3.5등신 가는 몸(원작은 약 3등신·머리·옷 덩어리 큼), 채도 낮고 잔디테일 많음.
- 두 레퍼런스(뿔 소녀 / 초록 코트 배낭 = 상점 NPC Nana) 중 **배낭 탐험가 추천**: 탑 오르는 탐험가 설정·어비스 탐굴가 분위기, 실루엣 크고 단순, 초반 소박함.
- ds02(seed 6002): style = Nana 대기 첫 프레임, 약 3등신·부위색 외곽선·큰 덩어리·큰 배낭. 결과 48px, 외곽선·부드러움은 원작에 가까워짐. 남은 문제: 코트·머리·배낭 색이 비슷해 대비가 약하고 얼굴이 덜 읽힘 → 다음엔 코트 색을 머리와 분리(예: 짙은 청록/남색), 얼굴 피부 밝게. 파일 `ds01/raw_6002_explorer_pack.png`. 사용자 판정 대기.
- ds03(seed 6003): ds02 + 코트 짙은 남청록·피부 밝게·청록 눈, style_options color_palette=false(원작 색은 안 빌림). 결과 56px(목표 48보다 큼), 머리·코트·배낭이 확실히 갈리고 얼굴이 읽힌다 — 3장 중 가장 원작 느낌에 가깝다. 파일 `ds01/raw_6003_explorer_navy.png`. 사용자 판정 대기.
- **걷기 시험 walk_6101(11-01, 1 gen):** `animate_image_pixminimax` 64×64 8프레임, 첫·끝 프레임 = ds03(루프 고정), direction south-east, view side. 레퍼런스 Nana Run(6장·8fps, 몸 위아래 2px, 배낭 고정).
  - 수치: 프레임마다 색 31개 그대로(뭉개짐 없음), 키 54~56px, 이음새 차이 924 / 이웃 평균 675 = 1.37배(기준 1.5 이하 통과).
  - 눈: 얼굴·머리·배낭 형태 유지, 다리 보폭 보임. 문제 ① 4~7번에서 얼굴이 옆으로 조금 더 돈다 ② 허리 칼이 4~7번에서 사라진다 ③ 폭이 27~32px로 흔들린다.
  - 다음 손잡이: 4번 프레임(반대발)을 1장 직접 고정해 반 주기씩 두 번 생성(첫·끝 고정) 또는 칼을 등으로 옮긴 기준 그림. 파일 `Docs/ArtCompare/CharacterGen/ds01/walk_6101/raw_0~8.png`(8 = 0과 같음).

## 확정 레시피 — ds03 주인공·걷기 (11-01 사용자 합격)

> 사용자(11-01): ds03 "3차는 확실히 잘 나왔다", 걷기 walk_6101 "개잘나왔네… 지금 걷기는 거의 문제없다" → **걷기를 합격 기준**으로 삼는다. "한 번에 잘 나온 건 드문 일"이라 방법을 그대로 남긴다.

### 1) 캐릭터 1장 (create_image_pro_flash, 1장 약 5 gen)
- 64×64, `no_background: true`, seed 6003.
- `style_image` = 원작 상점 NPC **Nana 대기 첫 프레임**을 64×64 캔버스에 놓은 것(`DungeonSlasherRef/style_nana_64.png`). usage_description: "그림체만 복사(약 3등신·큰 머리 덩어리·부위색 외곽선·평면 음영·픽셀 밀도), 얼굴·색·디자인은 복사 금지".
- `style_options`: `color_palette: false`(원작 색 안 빌림), outline·detail·shading true.
- 설명문 핵심(그대로 재사용): 옆모습 대기 / 약 3등신·큰 머리·작은 몸·짧은 다리 / 풍성한 회금발 낮은 포니테일 / 청록 렌즈 황동 고글 / **짙은 남청록 긴 코트(머리색과 확실히 다른 색)** / 크림 스카프 / 금색 네 갈래 별 브로치 / 큰 갈색 가죽 배낭(침낭·작은 랜턴) / 검은 스타킹·갈색 부츠 / 허리 단검 / 밝은 피부·큰 청록 눈 / 오른쪽 3/4 / **부위색의 어두운 톤 외곽선(검정 금지)** / 큰 색 덩어리·머리·코트·배낭 대비 강하게·잔디테일 적게 / 몽환·섬뜩한 어비스 탐험가, 유아틱 금지 / 약 48px.
- 결과 56px(그대로 써도 된다고 사용자 확인). 파일 `Docs/ArtCompare/CharacterGen/ds01/raw_6003_explorer_navy.png`, PixelLab URL `https://api.pixellab.ai/mcp/images/501da339-8d43-4aea-aed3-9a4570fce0a4/download`.
- 실패에서 배운 것: ① 원작(01_05 기사)을 style로 쓰면 3.5등신·검정 굵은 외곽선이 나와 "느낌이 다르다"(ds01) ② 코트·머리·배낭 색이 비슷하면 얼굴이 안 읽힌다(ds02) → 색 덩어리를 서로 다른 색상으로 갈라 말한다.

### 2) 모션 (animate_image_pixminimax, 64px 8장 = 1 gen)
- `first_frame_url` = `last_frame_url` = 위 ds03 URL(**루프 고정** — 첫·끝이 같아 이음새가 안 튄다).
- `frame_count` 8, `direction` south-east, `view` side, seed 6101.
- `subject_description`: "a chibi girl explorer with ash-blonde hair, goggles, navy-teal coat and a large brown backpack".
- `initial_pose`: "standing upright, arms relaxed at the sides".
- description: 제자리 한 주기, 작은 교차 보폭, 몸 1px 위아래, 팔 살짝, 코트 끝·포니테일 살짝, **배낭은 몸에 붙어 같이 움직임**, "같은 자리·같은 크기·같은 방향, 돌지 않음".
- **보간 넣지 않는다.** 원작이 12fps 키 프레임만 쓴다. 우리 픽셀 보간은 뭉개진다. 재생은 0.1초/장(원작 Run 8fps 참고).
- 합격 수치: 프레임마다 색 수 그대로(31), 키 54~56px, 루프 이음새 차이 ≤ 이웃 평균 × 1.5.
- 남은 사소한 흠(사용자 "거의 문제없다"로 수용): 4~7번 얼굴 조금 더 돎·허리 칼 사라짐·폭 27~32px.
- 다른 모션도 같은 틀: 기준 ds03 1장 + 동작 설명 + "same spot/size/facing, backpack attached". 루프 아닌 동작(공격·점프)은 끝 프레임을 고정하지 않거나 ds03으로 돌아오게 한다.

### 3) 게임 적용 — 1시간 테스트 분량 (11-01, 사용자 "허락 없이 최대한")
- 생성(모두 pixminimax 64px, 첫 프레임 = ds03 URL, 1 gen씩, 합계 15 gen): idle 6201·run 6301(첫·끝 고정 루프) / jump 6401 / hurt 6601(4장, 첫·끝 고정) / death 6701 / 평타 stab 6511·down 6512·up 6513(4장, direction east) / flurry 6514·cross 6515·charge 6516 / rope 6801(돌아서 등 보고 오르기) / 시험 atk1~3 6501~6503(8장은 도중에 몸이 돌아 대부분 기각, atk2 1·2·3·7만 옆베기로). 원본 `Docs/ArtCompare/CharacterGen/ds01/motions/<이름>/raw_N.png`.
- 배운 것: 공격은 **4장 + direction east + "always faces right, never turns around or toward the camera"** 가 안전하다. 8장 공격은 5번째부터 몸을 돌리거나 정면을 본다. 공격 그림엔 스미어(흰 호)가 같이 그려져 나온다(원작 느낌 — 그대로 씀). 피격은 피(빨강)가 그려져 지웠다. 로프는 그림에 밧줄까지 그려지지만 게임 밧줄과 거의 겹쳐 그대로 둔다.
- 변환: `Tools/pixelfx/build_cel_ds.py` → `Assets/Art/Player/PixelRig/CelDS/`(ds_<묶음>_N.png + cel.json, 빌더의 56개 클립 이름 전부를 가장 가까운 묶음에 연결). 피벗 (32, 60). 클립마다 `strike`(타격 프레임) — 판정·궤적 시각에 쓴다. 평타 시간 0.07·0.06·0.09·0.1초.
- 굽기: 메뉴 `Tools/ProjectB/Pixel Rig/Build Adventurer DS`(프로브 `Tools/probes/build_ds.cs`). 프리팹 `_celArtWeapon = true` → 무기 스프라이트 숨김, 장비 레이어 합성 안 함, 리그 모션 스미어·무기 기본 스미어 안 띄움(`CharacterRig.ArtOwnsWeapon`), 걷기 시작 프레임 0. 되돌리기 = `Build Adventurer`.
- 플레이 확인(11-01, 낮, `Logs/playtest/rig_act_{walk,run,jump,combo3,hurt,death,rope}.png`, `ds_game_idle.png`): 발이 땅에 붙고 무기 겹침 없음, 3타 찌르기→내려베기→올려베기, 피격 흰 번쩍·깜빡임, 사망 무릎→엎드림, 로프 등 보고 오르기·멈춤 정상. 사망 컷에 흰 칼 모양이 잠깐 보이는데 플레이어 스프라이트가 아니다(근처 SpriteRenderer 는 셀 1개뿐) — 사망 연출 쪽, 미확인.
- 남은 흠: 크기 56px(예전 48px)라 판정 박스보다 조금 크다 / 무기 종류가 겉모습에 안 드러남 / 장비 외형 없음 / 대시·변신·직업 셀 없음(1층계엔 안 씀).
- 2차(11-01, 1시간 테스트 순서): 공중 평타 3종(airstab 6521·airdown 6522·airup 6523 — 첫 프레임 = 점프 꼭대기 `jump?index=3`, 다리 접음 유지) → 점프 중 시작한 공격은 `Air`+셀 이름(`PixelRig._airAttack`). 녹슨 검 스킬 수평 베기 sweep 6524(옆베기 묶음 교체). 연속 찌르기 새 시도 flurry2 6525는 웅크린 자세·정면 돎이라 기각. StabEnd·SwingDownEnd 는 마지막 1장으로(전체 재생 반복 방지). 확인: `Logs/playtest/rig_act_airattack.png`.
- 3차(11-01): 연속 찌르기 flurry3 6526 — 첫 프레임 = 찌르기 뻗은 자세(stab ?index=2)에서 시작하니 웅크리지 않고 팔만 당겼다 뻗는다(0·1·2·4·5·6 반복). 무기 평타 궤적(옆베기 WSlashSide 포함)은 CelDS 에서 전부 끔 — 교차 베기 그림 위에 큰 초승달이 겹쳤다.
- **걷기 교체 walk_6104(11-01, 사용자 "걷기 팔이 이상"):** walk_6101·6102·6103 모두 뒤 절반(4~7번)에서 몸이 3/4 정면으로 돌고 두 팔이 옆으로 벌어지며 허리 칼이 사라졌다(8장 한 번 생성의 버릇). 해결 = **반 주기 2번**: ① ds03 → 보폭 프레임(6102 의 3번, `76f6879d…?index=3`) 4장 ② 보폭 프레임 → ds03 4장, 둘 다 첫·끝 고정(seed 6104·6105, 2 gen). 8장 내내 옆모습·팔 옆구리·칼 허리, 색 31 그대로. 긴 루프 동작은 이 방식이 안전하다.
- **달리기 교체 run_6311(11-01, 사용자 "달리기 팔이 여전히 이상"):** 옛 run(6301) 도 4~7번에서 몸이 정면으로 돌고 팔이 벌어졌다. 걷기와 같은 반 주기 2번 — ds03 → 옛 run 3번(옆모습 보폭, base64) 4장 / 그 프레임 → ds03 4장(seed 6311·6312, 2 gen, 0.08초). 8장 내내 옆모습·팔 몸에 붙음.
