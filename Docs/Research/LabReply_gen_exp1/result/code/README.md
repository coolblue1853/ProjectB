# code/

파이썬 3 + numpy + Pillow. `<패키지>` = `frames_coat_original/`, `frames_underwear_base/`, `frames_leaf_set_transfer/`, `regions_current/`가 있는 폴더(`LayerExtraction_Package`).
명령은 `result/` 폴더에서 실행한다고 가정한다(`regions`, `labels`, `under_fixed`가 상대 경로).

## 실제로 쓰는 것(권장 파이프라인)
| 파일 | 역할 | 사용 |
|---|---|---|
| `m1_underdiff.py` | 영역 지도 만들기. `--hybrid` = 권장 지도 M1h(→ `regions/`) | `python code/m1_underdiff.py <패키지> regions --hybrid` |
| `export_runtime.py` | **런타임 형식 내보내기**(`Equip/<세트>_<부위>/` PNG + `_erase.png` + `equip.json`) | `python code/export_runtime.py map <세트 그림 폴더> regions <세트 id> <Equip 루트> --armor --cap-under under_fixed [--split-back <패키지>]` |
| `keyframe_select.py` | 생성할 키 프레임 고르기(원본 프레임만 필요) | `python code/keyframe_select.py <패키지> keys.json 0.2` |
| `study4_slots.py` | **최신 지표**(5칸/4칸, 기준선/권장안) → `metrics.json`의 원본 | `python code/study4_slots.py <패키지> regions labels under_fixed out.json` |
| `sheets.py` | 확인 시트. 4칸: `armor=1 under=under_fixed cap=1` | `python code/sheets.py combos <패키지> regions labels out.png armor=1 under=under_fixed cap=1` |
| `common.py` | 합성(런타임과 같은 순서·지울 마스크), 확장 지도, 세트별 모자(`cap_extend`), 지표 | (모듈) |

## 연구 기록용(결과 재현)
| 파일 | 역할 |
|---|---|
| `baseline_region_map.py` | 기준선 원본(수정 없음). M1h가 머리 규칙을 빌려 씀 |
| `make_labels.py` | 정답 라벨 생성(기준선 + `labels/gt_ops.json` 손 보정) |
| `evaluate.py` | 1차 5칸 평가(32조합). `이름=지도폴더[:fill][:hem=2][:none]` |
| `m2_propagate.py` | M2 키 프레임 라벨 전파(1차, 기각) |
| `m3_amodal.py` | 생성 0 보완 층(가려진 부분 패치 복사). 4칸 보완·배낭 끄기에 쓰임 |
| `closeup.py` | 5칸 확대 전후 비교(1차) |
| `study2.py` | 2차: 이음새·외곽선, 깜빡임, 세트별 지도, 조각 제거, 속옷 몸 전파 |
| `study5_cost.py` | 비용: 앵커·키 프레임 옮기기(`warp_set`) |
| `layered_extract.py`, `sim_layered.py`, `score_layers.py` | 단계 생성(실험 1) 추출·모의 검증·채점. 4칸 결정 이후 기본 경로에서는 안 씀 |

`archive/code/`의 `recut_layers.py`, `hybrid_layers.py`는 5칸 전용(폐기).

Windows 경로는 `C:/...` 형태로 주면 `이름=C:/경로:fill`처럼 옵션을 붙여도 된다(드라이브 문자 처리함).
