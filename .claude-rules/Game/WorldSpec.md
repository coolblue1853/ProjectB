# 월드 스펙 (시간 · 맵 · 카메라)

## 1. 낮/밤 — `Script/Time/TimeChange.cs`

- 하루는 1200초(20분)다.
- 시간값 0.5~0.9 구간이 **밤**(8분)이고, 나머지가 낮(12분)이다.
- 경과 시간은 세이브에 저장된다.
- 조명은 Lost 플러그인의 LightColorController가 맡는다.
- 횃불(TouchLightManager)은 최대 밝기 60초, 수명 180초다. 🐞 감쇠 구간(0.85~0.35)이 밤 구간과 어긋난다.
- 디버그 키 0을 누르면 +60초 된다(빌드에서는 제거해야 함).

## 2. 맵 · 포털 — `Script/Map/*`

| 맵 | 상태 |
|---|---|
| Town | 상점, 대장간, 창고 |
| Lv1_NearForest | 사냥터 1 |
| Lv1_NearForest_SideRoad | 사냥터 1-2, 보스. 🐞 진입 포털이 끊겨 있다(destination null, 비활성) |

- 포털은 ↑ 키로 이동한다. 이동하면 대상 맵을 켜고, 이전 맵을 끄고, 스포너를 정리한 뒤 카메라를 리센터한다. 1초 뒤 위치를 저장한다.
- 패럴랙스 배경(ParrelaxBacground)과 ProCamera2D PixelPerfect를 쓴다.

## 3. 카메라

- ProCamera2D 확장: PixelPerfect, Geometry/Numeric Boundaries, Rooms, Shake
- 해상도: 16:9 레터박스를 강제한다(`Cam/Fixed.cs`).
- 확인된 플레이 화면: 640×360 Game 뷰에서 Town이 정상 렌더링된다(2026-10-18, Unity 6).
