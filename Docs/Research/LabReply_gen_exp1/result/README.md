# 레이어 분리 연구 결과 — 입구 (최신 11-10)

## 먼저 읽을 것(순서)
1. `REVIEW.md` — 검토용 요약(결론·주의점·점검 결과) **← 여기부터**
2. `report.md` — 결과 보고(결과 먼저 + 방법 + 평가 정의)
3. `SLOT_DECISION.md` — 4칸 설계 근거, 머리 경계, 배낭, 대머리 안 판단
4. `PIPELINE.md` — PixelLab 비용·제작 순서, 헤어스타일 교체 설계
5. `failures.md` — 못 푼 것(수치·원인·다음 할 일)
6. `GEN_RESEARCH_GUIDE.md` — 생성 실험 지시서(Sonnet용, 실험 A~E)

## 폴더
| 경로 | 내용 | 상태 |
|---|---|---|
| `regions/` | 영역 지도 21장(L, 64×64, 값 0~8, 기존 형식 그대로) = M1h | **교체용** |
| `under_fixed/` | 목·턱 보정한 속옷 몸 21장 | **교체용** |
| `metrics.json` | 최신 지표(기준선 / 권장안 / 변형) | 최신 |
| `sheets/` | 4칸 조합 시트(기준선 ↔ 권장안), 부위색 시트 | 최신 |
| `labels/` | 정답 라벨 8장 + 만드는 손 보정(`gt_ops.json`) | — |
| `code/` | 재현·내보내기·점검 스크립트(`code/README.md`) | 최신 |
| `study5/`, `unique_frames.json` | 비용 연구(키 프레임, 고유 98장) | 최신 |
| `gen_exp1_reply/` | 실험 1 반환에 대한 답(`gen_exp1_reply/REPLY.md`)과 4칸·머리·배낭 측정 json/png | 최신(REPLY 는 5칸 기준 내용 포함, 머리말 참고) |
| `study2/` | 2차 연구(이음새·깜빡임 등, 옛 지도 기준) | 기록용 |
| `archive/` | 폐기·이전 판(1차 보고·5칸 하이브리드·옛 지시서) | 기록용 |

## 프로젝트 반영 순서(이쪽 저장소 기준)
1. **지도 교체:** `regions/*_region.png` 21장 → `Docs/ArtCompare/Motion/equip/regions/`
2. **속옷 몸 교체:** `under_fixed/*.png` 21장 → `Assets/Art/Player/PixelRig/EquipDS/under/`, 그리고 CelDS 기반 몸 프레임(`under_base.py apply`가 쓰는 곳)에도 같은 21장. 이걸 안 하면 목·턱 노출 보정 효과만 빠지고 깨지지는 않는다.
3. **레이어 내보내기(4칸):** `result/` 폴더에서
   - `python code/export_runtime.py map <원본 코트 21장 폴더> regions base_coat <Equip 루트> --armor --cap-under under_fixed`
   - `python code/export_runtime.py map <잎사귀 21장 폴더> regions leaf_set <Equip 루트> --armor --cap-under under_fixed`
   - `<Equip 루트>` = `Assets/Art/Player/PixelRig/Equip`. 결과는 `equip_ds.export`와 같은 형식이다(`<세트>_<cap|top|glove|legs|shoes>/`, `<클립>_<k>.png` + `_erase.png` + `equip.json`). 바지 레이어는 빈 그림.
4. **바지 칸 외형 끄기:** `Resources/CelEquipMap.csv`에서 바지 아이템 줄을 지운다. 대응표에 없으면 아무것도 안 그려진다. 바지 칸이 비면 기본 `base_coat_legs`(빈 레이어)가 쓰인다. C# 변경 없음.
5. Unity: `Tools/ProjectB/Pixel Rig/Build Adventurer DS` → SampleScene 확인.
6. **(선택) 배낭 켜고 끄기:** 내보내기에 `--split-back <패키지>`를 붙이면 `base_coat_backpack`(slot `back`)이 따로 나온다. C# 필요:
   - `CelEquipLayer.Slot`에 `Back = 25` 추가(갑옷 20과 장갑 30 사이)
   - `PixelRigBuilder`가 `"back"`을 읽게 하기
   - `CelEquipSync`에서 "갑옷 = 기본 코트이고 옵션이 켜져 있을 때만" 배낭 레이어를 목록에 추가
   - 이 경우 `base_coat_top`은 배낭이 빠진 갑옷이 된다.

## 점검(전달 전 연구실에서 실행, 전부 통과)
`python code/check_delivery.py <패키지> --full`
- regions 21장 형식, M1h 재생성과 0픽셀 차이
- 내보내기 구조(폴더·이름·equip.json·erase), 4칸 바지 레이어가 비어 있음
- 내보낸 PNG를 `CelComposer.cs`와 같은 규칙(슬롯 순서, 레이어 차례에 erase로 지금까지의 그림을 지움, a>0 덮어쓰기)으로 다시 합성한 결과 = 평가기 합성, 16조합 × 21프레임에서 0픽셀 차이
- 문서에 적힌 경로가 모두 존재
- `metrics.json` 기준선·권장안을 다시 계산해 일치
