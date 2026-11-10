# 몸+옷 한 조각 방식: 코드 쪽 회신 (총괄, 11-10)

공유 노트 `piece_system_handover.md` 는 총괄 쪽에서 찾지 못했다(경로 회신 대기). 아래는 코드를 직접 읽은 결과다. 코드는 아직 바꾸지 않았다.

## 1. 코드 변경 가능 여부 (erase 마스크로 되는가)
- **`CelComposer`: 변경 불필요.** 슬롯 낮은 순으로 쌓고, 각 레이어마다 `erase.r>127` 픽셀을 지금까지의 그림에서 지운 뒤 `layer.a>0` 픽셀을 덮어쓴다. '옷' 레이어의 erase = 속옷 바탕의 목 아래 전부, layer = 목~다리·손목 살 + 옷 한 장이면 코드 없이 된다.
- **`PixelRigBuilder`: 변경 불필요.** `equip.json`의 `id`·`ds`·`slot`·프레임별 `<clip>_<k>.png` + `<clip>_<k>_erase.png`를 읽을 뿐이다.
- **`CelEquipLayer.Slot` 순서(필요한 변경 1줄):** 지금 Legs=10, Shoes=15, Body=20, Gloves=30, Head=40. '옷'(Body=20)의 erase가 발목 아래까지 닿으면 먼저 그려진 신발(15)을 지운다. 해결 둘 중 하나: (a) `Shoes`를 25로 올려 신발이 옷 위에 오게 한다(권장, enum 값 한 줄. 에셋 직렬화는 이름이 아닌 값이라 기존 `.asset`은 재빌드 필요), (b) erase를 발목 위로만 만든다.
- **`CelEquipSync`(필요한 변경):** `ArmorBoxes = {2,5,6,8,9}`(모자·갑옷·장갑·바지·신발)과 `DefaultLayers` 5개가 바지(8)·`base_coat_legs`를 따로 가진다. 외형 4칸이면 `ArmorBoxes = {2,5,6,9}`, `DefaultLayers = {base_coat_cap, base_coat_top(=옷 한 벌), base_coat_glove, base_coat_shoes}`로 줄인다. 인벤토리 칸 자체(8번)는 슬롯 개편 때까지 그대로 두고 외형에서만 뺀다. `CelEquipMap.csv`는 `LeafArmor`를 '옷 한 벌' 레이어 id로 바꾼다(`LeafLegArmor`는 이미 삭제됨).

## 2. 고글 = 기본 모자 소속 규칙
- 이미 맞다: 모자 칸이 비면 `DefaultLayers[0]`(`base_coat_cap`)이 깔리고, 모자를 쓰면 그 레이어가 대신 들어가므로 고글은 기본 모자 레이어에만 넣으면 자동으로 사라진다. 코드 변경 없음.
- 단 속옷 바탕 그림에 고글·배낭·랜턴·스카프가 **들어 있으면 안 된다**. 바탕은 머리(머리카락·얼굴)만 남기고, 고글은 `base_coat_cap`, 배낭·랜턴·스카프는 `base_coat_top`('옷') 레이어에 넣는다. 그러면 다른 옷을 입을 때 배낭이 자동으로 숨는다(큐의 배낭 규칙과 일치).
- 새 모자의 erase 마스크가 머리카락 위를 지우는 것은 지금과 같다(모자 레이어가 머리 위에 덮음).

## 3. 내보내기 형식 요구사항
- 레이어 id마다 폴더 `Assets/Art/Player/PixelRig/Equip/<id>/` 와 `equip.json`: `{ "id": "...", "ds": true, "slot": "Body|Gloves|Head|Shoes", "frames": [{ "clip": "...", "k": 0 }, ...] }`. 4칸이면 slot은 Head / Body(옷) / Gloves / Shoes.
- 프레임마다 `<clip>_<k>.png`(덮어쓸 그림)와 `<clip>_<k>_erase.png`(지울 마스크: R>127 이 지움). 캔버스 크기·피벗은 CelDS 프레임과 동일(64 캔버스, 피벗 불변), 알파 0 = 투명.
- **프레임 누락 금지:** 레이어에 없는 프레임은 `Find` 가 null 이라 건너뛰고 바탕(속옷)이 그대로 드러난다. 옷·기본 코트 레이어(`base_coat_top`)는 CelDS 클립 전 프레임을 덮어야 한다. 대기·3타만 있는 지금 상태에서 걷기·달리기·점프 클립은 속옷 바탕이 보인다(현재도 동일, G2 범위). 옷 레이어는 G2 확장 때 같은 클립 목록으로 같이 늘려야 한다.
- 옷의 erase 는 목 아래 전부(발목·손목 포함)여도 된다. 장갑(30)·모자(40)는 옷 위에 그려진다. 신발은 Shoes=25 로 바꾸면 옷의 erase 와 겹쳐도 안전하다.
- 에셋 재빌드: 폴더를 두면 `PixelRigBuilder`가 `Resources/PixelRig/Equip/<id>.asset`을 만든다. 새 id 는 `CelEquipMap.csv`에 연결한다.

## 4. 총괄이 할 일(승인 대기)
1) `CelEquipLayer.Slot.Shoes` 15→25, 2) `CelEquipSync` ArmorBoxes·DefaultLayers 4칸화, 3) `CelEquipMap.csv` 갱신, 4) 테스트·인게임 확인. 그래픽 산출물 도착 후 진행.
