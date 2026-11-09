# 장비 부위 분리 노하우 (11-09, G1-6)

새 몸·모션·방어구를 부위별 교체 레이어로 쪼갤 때 이 순서로 한다. 스크립트는 `Tools/pixelfx/`.

## 구조 (한 줄)
**속옷 기반 몸(겉으로 안 보임) → 그 위에 5부위 레이어(모자·상의·장갑·바지·신발)를 쌓음.** 지울 영역 없음(erase 는 빈 마스크). 빈 칸이면 기본 코트 레이어(`base_coat_*`)가 보인다.
- 기본 의상(코트·바지) = 장비 세트 `base_coat`. 속옷 몸은 어떤 부위를 벗어도 구멍이 안 나게 하는 기반일 뿐이다.
- **배낭·랜턴은 상의에 붙은 것**으로 판정(상의를 바꾸면 사라짐). 다른 세트에는 배낭 없음.
- 치마·코트 자락은 허리 아래 = **바지 부위**. 상의는 허리 위(몸통·팔)만.

## 절차
1. **기준 그림 → 속옷 몸**: 대기 1프레임을 `edit_image_pro_flash`(5 gen)로 "코트·배낭·랜턴 제거, 속옷만"으로 다시 그린다 → 나머지 프레임을 `transfer_outfit`(64px 15프레임/회, 약 10 gen)으로. seed 고정. 결과 `Docs/ArtCompare/Motion/equip/under/{t1,t2}`.
2. `python Tools/pixelfx/under_base.py [apply]` : 원본 머리(모자 부위+머리 맨몸)를 속옷 몸 위에 덮어 얼굴·머리카락·고글 보존 → `EquipDS/under/`. `apply` 가 CelDS 프레임 교체(원본은 `EquipDS/base_coat/`에 보관).
3. **영역 지도** `python Tools/pixelfx/region_map.py <원본 코트 폴더 = EquipDS/base_coat> Docs/ArtCompare/Motion/equip/regions <프레임...>` (CelDS 가 아니라 **원본 코트 프레임**을 읽는다). 라벨: 1 모자 2 상의 3 장갑 4 바지 5 신발 6 맨몸 7 배낭·랜턴. 후처리: `skirt_to_legs`(폭 급증 줄 아래 상의→바지), `lantern_to_back`(노란 불빛+틀). 프레임별 보정은 `region_overrides.json`.
4. 세트 전이 그림(`transfer_outfit`, 기준=속옷 몸 위에 입힌 그림)을 `EquipDS/<세트>/`에 두고 `python Tools/pixelfx/equip_ds.py export <세트>` → `Equip/<세트>_<부위>/`. 레이어 = 장비 픽셀 중 확장 지도(몸 밖 3칸)가 그 부위인 곳.
5. Unity: 컴파일 후 `Tools/ProjectB/Pixel Rig/Build Adventurer DS`. 아이템 이름→레이어는 `Resources/CelEquipMap.csv`.
6. 검증: 부위 조합 시트(`equip_ds.py combos` 또는 `under/mix_under.png` 방식) — 잘림·투명 구멍·상의가 바지 침범 없음, 그다음 인게임(**SampleScene**에서, 빈 Untitled 씬 아님).

## 함정
- 지도는 **원본 코트 프레임**으로 만든다. CelDS 를 속옷으로 교체한 뒤 CelDS 를 읽으면 엉망.
- 장비 레이어가 안 보이는 부위 = 장착 칸 번호(`CelEquipSync.ArmorBoxes` 2·5·6·8·9)·CelEquipMap 이름·`forDS` 확인.
- 머리 덮어쓰기 때 코트색·갈색(배낭) 픽셀은 제외(`under_base.head_mask`), 고글(청록)은 유지.
- 장갑 영역은 작다(손만). 소매는 상의 쪽.
- 걷기·달리기 등 다른 모션은 아직 코트가 구워진 원본(G2 확장 전). 움직이면 모습이 바뀌어 보임.
