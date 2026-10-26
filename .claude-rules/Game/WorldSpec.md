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
| Town | 상점, 대장간, 창고. **10-26 퀄리티업**: 왼쪽부터 상점 노점(`Bld_Stall` x -427.5, 등불 광원)·상인 → 대장간(`Bld_Forge` x -418.3, 화로 광원 `LightFlicker`)·무기/방어구 대장장이 → 창고 → 장로 집(House)·장로. NPC 4명은 주인공 셀과 같은 그림체(PPU 36 × 1.5배 = 24도트/유닛, 2프레임 숨쉬기). 재구성 프로브 `Tools/probes/setup_town_v2.cs`, 촬영 `Tools/probes/town_shot.cs` |
| Lv1_NearForest_SideRoad | **현재 Lv1 사냥터.** NearForest 구역(x -408~-235) + 왼쪽 확장 구역(x ≈ -716, 두더지·꽃두더지·밤 허수아비·크리핑테러·보스). 내부 포털 -236 ↔ -728은 이동 전용 |
| Lv1_NearForest | 구버전(비활성, 미사용). SideRoad의 부분집합 |
| **Lv2_MagicCliffs** | 사냥터 2 "마법 절벽" (x -420~-180, 바닥 y -180). SideRoad 서쪽(`Portal_ToLv2`, x≈-722)에서 진입. 낮: 버섯·고블린 / 밤: 눈·해골·고블린, 엘리트 Bringer(y=12 발판), 보스 Abyss Wizard(오른쪽 끝). 생성: `Tools/probes/build_lv2_map.cs` |

- 포털은 ↑ 키로 이동한다. `openMap`이 비어 있으면 같은 맵 안에서 위치만 옮긴다. 이동하면 대상 맵을 켜고, 이전 맵을 끄고, 스포너를 정리한 뒤 카메라를 리센터한다. 1초 뒤 위치를 저장한다.
- 패럴랙스 배경(ParrelaxBacground)과 ProCamera2D PixelPerfect를 쓴다.

## 3. 카메라

- 맵별 카메라 아래 경계: 맵 루트의 `MapInfo.cameraBottom`(없으면 -10). 포털 이동(`Portal.MapChanged`) 때 적용된다.
- 어디서든 마을 귀환: `MapInfo.GoToTown`(사망 부활에서 사용).

- ProCamera2D 확장: PixelPerfect, Geometry/Numeric Boundaries, Rooms, Shake
- 해상도: 16:9 레터박스를 강제한다(`Cam/Fixed.cs`).
- 확인된 플레이 화면: 640×360 Game 뷰에서 Town이 정상 렌더링된다(2026-10-18, Unity 6).
