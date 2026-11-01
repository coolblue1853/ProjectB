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
| `clips/` | **AnimationClip 8170개**: `<클립>_<id>/NN.png` 프레임 순서대로 + `_strip.png` + `_info.txt`(frames·sampleRate) |
| `tools/` | `export.py`(텍스처), `export_sprites2.py`(스프라이트), `export_clips.py`(클립), `sheet_char.py <clips> <접두어> <out>`(캐릭터 모션 시트), `idle_sheet.py`(전 캐릭터 대기 모음) |
| `preview_*.png` | 모음 시트(대기 66종 `preview_idle_all.png`, 기본 기사 `preview_00_00*.png`) |

## 관찰 (첫 정리)
- 플레이어 클립 이름 `NN_SS_<모션>` = 직업 NN · 스킨 SS. 직업 00~09 × 스킨, 대기 66종. 그 밖 직업(Guardian·Hunter·Ronin·Predator·Werewolf·Summoner·Assassin·Vampire·Dragonian·Berserker·Shaman·Inquisitor…)은 아틀라스 이름으로 있다.
- 기본 기사(00_00) 모션: Idle 6 · Run · Jump 3 · Down 3 · Dash 6 · Attack01 4 · Attack02 4 · Hit 1 · DieN 5 · Climb 1 · Sit · WeaponSkill000/001/006(Do·End), **sampleRate 12**(프레임당 0.083초). 우리 기본 3타(0.04~0.055초)보다 프레임 수가 적고 키 포즈 위주다.
- 캐릭터 키 약 45~55px, 약 3.5등신, 얇은 짙은 외곽선, 2~3단 음영. 이펙트는 캐릭터와 분리된 프레임(흰 고리·선·불꽃 줄기 등)으로 같은 아틀라스에 있다.
- 보스·몹 = `B###`·`E###`·`M###` 아틀라스, 맵·타일 = `Map`·`Tile`·`0.Castle_*` 등.

## PixelLab 시험
- ds01(11-01): `create_image_pro_flash` 64×64, style_image = 01_05 대기 첫 프레임(그림체만), 우리 주인공 설명(회금발 포니테일·라벤더 망토·청록 고글·네 갈래 별 브로치·가죽 재킷·단검). 결과 54px·약 3.5등신, 원작보다 외곽선이 진하다. 5 gen. 파일 `Docs/ArtCompare/CharacterGen/ds01/raw_6001_explorer.png`. **사용자 판정 대기.**
