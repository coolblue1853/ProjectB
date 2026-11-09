# 레이어 분리 연구 패키지 (11-09)

먼저 `01_brief_problem_and_prompt.md`(문제·연구 질문·평가·지시문)를 읽고, 이 문서로 파일과 데이터 형식, **돌려주는 방법**을 확인한다.

## A. 데이터 형식
- 프레임 PNG: 64×64 RGBA, 투명 배경, 발바닥이 y=60 부근에 닿는다. 이름 `ds_<클립>_<번호>.png`.
- 클립(총 21프레임): `ds_idle_0~7`(숨쉬기 대기) · `ds_stab_0~3`(찌르기, 팔을 앞으로 뻗음) · `ds_down_0~3`(내려베기, 몸이 앞으로 숙임) · `ds_up_0~4`(올려베기, 마지막 두 프레임은 **등 뒤에서 본 모습**).
- 영역 지도 `<프레임>_region.png`: 8비트 그레이(L), 픽셀값 = 부위 번호. 0 투명, 1 모자(머리 윗부분), 2 상의, 3 장갑(손), 4 바지, 5 신발, 6 맨몸(얼굴·머리카락·고글, 교체 안 함), 7 배낭·랜턴(기본 코트 상의에만 소속), 8 코트 치마 자락(기본 코트에선 상의 소속, 다른 세트에선 바지 소속). `regions_current/region_sheet*.png`는 확인용 색칠 시트(왼쪽 원본, 오른쪽 부위색: 초록 모자·파랑 상의·노랑 장갑·보라 바지·주황 신발·회색 맨몸·갈색 배낭).
- 레이어 PNG `layers_current/<세트>/<부위>/<클립>_<번호>.png`: 그 부위에 해당하는 장비 픽셀만 남기고 나머지는 투명. 이름 대응: `ds_idle→Idle`, `ds_stab→Stab`, `ds_down→SwingDown`, `ds_up→SwingUp`.
- 합성 규칙(런타임과 같음): `속옷 몸 프레임 위에 부위 레이어를 순서대로 덧그림`. 순서: 바지 → 신발 → 상의 → 장갑 → 모자. 레이어 알파가 0이 아닌 픽셀이 아래를 덮는다. 지우기 없음.

## B. 폴더
| 폴더 | 내용 |
|---|---|
| `frames_coat_original/` | 원본 21프레임. 코트+바지+배낭+랜턴을 입은 기본 캐릭터. **분할 대상** |
| `frames_underwear_base/` | 같은 자세, 코트·배낭·랜턴을 벗긴 속옷 몸. 얼굴·머리카락·고글은 원본 그대로 덮어 씀(AI 생성 + 합성) |
| `frames_leaf_set_transfer/` | 같은 자세, 다른 옷(초록 잎사귀 세트, AI 의상 전이)을 입힌 그림. 배낭·랜턴이 같이 그려져 있음(분할 시 버림). **교체 테스트용 두 번째 옷** |
| `regions_current/` | 현재 규칙으로 만든 영역 지도(기준선). 소수 프레임은 눈으로 검토했고 전체 정답은 아님 |
| `layers_current/` | 현재 기준선으로 뜯은 레이어(`base_coat`=원본 코트, `leaf_set`=잎사귀) |
| `sheets/` | 문제와 해결을 보여주는 합성 시트: `combos_leaf_base.png`·`mix_top_leaf.png`·`mix_legs_leaf.png`(예전 코트-기반 방식의 구멍=빨강), `mix_under.png`·`mix_under2.png`(현재 방식, 15가지·6가지 부위 조합), `under_cmp.png`(원본↔속옷 몸 비교), `under_head.png`, `ref_cmp.png` |
| `unit_scene_shots/` | 게임 안에서 실제로 보이는 모습(3840×2160 전체 + 확대) |
| `code/` | 기준선 코드(`region_map.py`=규칙 분할, `equip_ds.py`=확장·합성·내보내기·조합 시트, `under_base.py`=머리 덮어쓰기), 런타임 합성 C#(`CelEquipLayer.cs`·`CelComposer.cs`·`CelEquipSync.cs`), `cel.json`(클립·프레임 정보) |
| `docs/` | `EquipSplitPlaybook.md`(절차·함정), `CelEquipmentPlan.md`(장비 레이어 설계 기록), `CharacterPixelRules.md`(그림체 규칙), `MotionJudgeLog.md` |

## C. 지금 알려진 문제 프레임 (눈으로 본 것)
- `ds_idle_*`: 왼쪽 허리 랜턴의 어두운 틀 몇 픽셀이 바지(보라)로 분류됨.
- `ds_stab_*`: 소매의 격자 무늬가 거칠고, 뒷다리와 코트 자락이 겹침.
- `ds_down_2/3`: 몸이 숙여 허리 위치 추정(상의 폭 급증 줄)이 흔들림.
- `ds_up_3/4`: 등 뒤 모습이라 얼굴·고글이 없어 머리 기준선(고글)을 못 찾음. 부위 구분은 위치 추정에 의존.
- 장갑(3)은 손 몇 픽셀뿐이라 소매·머리카락과 자주 섞임.
- 기준선에서 '속옷 위에 얹기'로 바꾼 뒤 조합 시트에서는 잘림·빈칸이 거의 없었다(`mix_under.png`). 남은 문제는 주로 **분류 정확도**와 **다른 옷 종류(치마·원피스·갑옷)에서의 일반화**다.

## D. 돌려주는 방법 (이 형식이면 바로 끼워 넣을 수 있다)
연구 대화창은 아래를 **한 폴더(또는 zip)**로 모아 사용자에게 전달한다. 사용자가 그 폴더를 이쪽 대화창에 붙인다.

```
result/
  report.md            # 결과 먼저 10줄 이내 + 방법 설명 + 실패 사례 표
  metrics.json         # {"baseline":{...}, "<방법명>":{ "iou_per_class":{...}, "holes":N, "intrusion":N, "frames":21 }}
  regions/<프레임>_region.png   # 21장, 위 A절 형식(L, 값 0~8, 64×64) 그대로. 파일명도 같게
  code/                # 재현 스크립트(파이썬, 입력 폴더·출력 폴더를 인자로 받는 CLI)
  sheets/              # 부위색 확인 시트, 조합 시트(전후 비교)
  failures.md          # 해결 못 한 프레임·영역 표(프레임, 부위, 증상, 추정 원인)
```
요건:
1. `regions/`의 21장은 **현재 코드의 입력/출력과 호환**되어야 한다(같은 이름·같은 값 의미). 그러면 이쪽에서 `Docs/ArtCompare/Motion/equip/regions/`에 교체하고 `python Tools/pixelfx/equip_ds.py export leaf_set`과 `export base_coat`로 레이어를 다시 만든 뒤 Unity 빌드(`Tools/ProjectB/Pixel Rig/Build Adventurer DS`)만 하면 인게임에 반영된다.
2. 새 방법이 지도 외에 **추가 입력**(속옷 몸 프레임, 관절 키포인트, 모델 가중치)을 필요로 하면 `report.md`에 정확히 적는다.
3. 지표는 반드시 기준선(`regions_current/`)과 같은 테스트 세트·같은 정의로 비교한다. 정답 라벨을 새로 만들었다면 `labels/` 폴더에 같이 넣는다.
4. 조합 시트는 `sheets/`에 두 장: ① 기준선 ② 새 방법. 같은 부위 조합·같은 프레임으로 비교한다. 구멍은 빨강으로 표시.
5. 못 푼 부분은 `failures.md`에 숨기지 말고 쓴다.

이쪽에서 받은 뒤에 할 일(참고): 지도 교체 → 레이어 재내보내기 → 조합 시트 재생성 → Unity 빌드 → SampleScene 플레이 캡처 확인 → 커밋.
