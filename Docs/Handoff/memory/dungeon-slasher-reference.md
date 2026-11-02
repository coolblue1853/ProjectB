---
name: dungeon-slasher-reference
description: 11-01 사용자: 던전 슬래셔(모바일 도트 액션) APK 리소스를 스컬·림버스처럼 전부 추출해 모션·이펙트·캐릭터·UI 참고로 쓴다, 주인공을 이 그림체(미소녀 계열 등신 높은 도트)로 시험
metadata:
  type: project
---

2026-11-01 사용자가 `Downloads/Dungeon+Slasher_+Action+RPG_0.776.3_APKPure.xapk`를 주며 "도트인데 모션이랑 이펙트를 잘 찍었다, 스킬 이펙트·캐릭터·캐릭터 모션·UI 다 뽑아오자"고 했다. 약간 미소녀 계열이지만 이 방향으로 주인공을 만들어 보는 것도 고려 중(PixelLab 1장 시험 요청).

추출 위치: `C:\Users\user\DungeonSlasherRef`(저장소 밖). `raw/`(apk 풀기), `tex/`(Texture2D 4897장), `spr/`(Sprite 프레임, 아틀라스별), `tools/export*.py`(UnityPy).

**Why:** 스컬·림버스와 같은 레퍼런스 라이브러리 용도. 상용 게임 리소스라 참고만.
**How to apply:** 저장소·게임 빌드에 복사·커밋 금지(SkulRef 규칙과 같다). 키 자세·프레임 수·이펙트 형태·색 쓰는 법을 관찰해 우리 것으로 다시 만든다. [[skul-asset-library]] [[limbus-fx-reference]] [[visual-ref-skul-limbus]] [[graphics-user-review]]
