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

## PixelLab 시험
- ds01(11-01): `create_image_pro_flash` 64×64, style_image = 01_05 대기 첫 프레임(그림체만), 우리 주인공 설명(회금발 포니테일·라벤더 망토·청록 고글·네 갈래 별 브로치·가죽 재킷·단검). 결과 54px·약 3.5등신, 원작보다 외곽선이 진하다. 5 gen. 파일 `Docs/ArtCompare/CharacterGen/ds01/raw_6001_explorer.png`. **사용자 판정 대기.**
- 사용자(11-01) ds01 평: "비슷한 그림체인데 느낌이 좀 다르다". 원인 분석: 검정 굵은 외곽선(원작은 부위색의 어두운 톤), 3.5등신 가는 몸(원작은 약 3등신·머리·옷 덩어리 큼), 채도 낮고 잔디테일 많음.
- 두 레퍼런스(뿔 소녀 / 초록 코트 배낭 = 상점 NPC Nana) 중 **배낭 탐험가 추천**: 탑 오르는 탐험가 설정·어비스 탐굴가 분위기, 실루엣 크고 단순, 초반 소박함.
- ds02(seed 6002): style = Nana 대기 첫 프레임, 약 3등신·부위색 외곽선·큰 덩어리·큰 배낭. 결과 48px, 외곽선·부드러움은 원작에 가까워짐. 남은 문제: 코트·머리·배낭 색이 비슷해 대비가 약하고 얼굴이 덜 읽힘 → 다음엔 코트 색을 머리와 분리(예: 짙은 청록/남색), 얼굴 피부 밝게. 파일 `ds01/raw_6002_explorer_pack.png`. 사용자 판정 대기.
- ds03(seed 6003): ds02 + 코트 짙은 남청록·피부 밝게·청록 눈, style_options color_palette=false(원작 색은 안 빌림). 결과 56px(목표 48보다 큼), 머리·코트·배낭이 확실히 갈리고 얼굴이 읽힌다 — 3장 중 가장 원작 느낌에 가깝다. 파일 `ds01/raw_6003_explorer_navy.png`. 사용자 판정 대기.
