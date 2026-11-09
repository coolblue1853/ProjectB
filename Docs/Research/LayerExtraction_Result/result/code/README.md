# code/

파이썬 3 + numpy + Pillow. `<패키지>` = `frames_coat_original/`, `frames_underwear_base/`, `frames_leaf_set_transfer/`, `regions_current/` 가 있는 폴더.

| 파일 | 역할 | 사용 |
|---|---|---|
| `baseline_region_map.py` | 기준선 원본(수정 없음, M1h 가 머리 규칙을 빌려 씀) | `python baseline_region_map.py <원본 폴더> <출력> ds_idle_0 ...` |
| `common.py` | 입출력, 확장 지도, 런타임과 같은 합성, 지표 함수 | (모듈) |
| `make_labels.py` | 정답 라벨 생성(기준선 + 손 보정 사각형) | `python make_labels.py <패키지>/regions_current ../labels/gt_ops.json ../labels` |
| `evaluate.py` | IoU + 조합 지표 → metrics.json | `python evaluate.py <패키지> <labels> <metrics.json> 이름=지도폴더[:fill][:hem=2][:none] ...` |
| `m1_underdiff.py` | M1(속옷 차분) / M1h(`--hybrid`, 권장 지도) | `python m1_underdiff.py <패키지> <출력> --hybrid` |
| `m2_propagate.py` | M2(키 프레임 라벨 전파) | `python m2_propagate.py <패키지> <labels> <출력> [--direct]` |
| `m3_amodal.py` | M3 보완 층 PNG 내보내기 | `python m3_amodal.py <패키지> <지도폴더> <출력>` |
| `sheets.py` | 부위색 시트 / 조합 시트 | `python sheets.py regions <패키지> <지도> <png>` · `python sheets.py combos <패키지> <지도> <labels> <png> [fill=m3] [hem=2]` |
| `closeup.py` | 확대 전후 비교 | `python closeup.py <패키지> <png> 이름=지도[:fill][:hem=2] ...` |
| `study2.py` | 2차 연구: 이음새·외곽선, 깜빡임·안정화, 세트별 지도, 조각 제거, 속옷 몸 전파 | `python study2.py <패키지> <지도> <labels> <출력>` |
| `layered_extract.py` | 한 겹씩 입히기 단계 그림 → 레이어·지도·보고 | `python layered_extract.py <속옷 폴더> <출력> legs+shoes=<A> top=<B> [cap+glove=<C>]` |
| `sim_layered.py` | 위 도구 모의 검증(생성 0) | `python sim_layered.py <패키지> <지도> <json>` |
| `score_layers.py` | 단계 레이어 vs 1차 권장안 조합 채점 | `python score_layers.py <패키지> <layered 출력> <M1h 지도>` |

Windows 경로는 `C:/...` 형태로 주면 `이름=C:/경로:fill` 처럼 옵션을 붙여도 된다(드라이브 문자 처리함).
