# 생성이 필요한 연구 — 실행 지시서 (Sonnet 등 다른 모델이 그대로 따라 하도록 작성)

이 문서는 이 폴더(`result/`)의 코드와 지표를 그대로 써서, PixelLab 생성이 필요한 실험 3개를 순서대로 진행하는 방법이다.
**판단을 줄이려고 모든 단계에 입력·명령·합격 기준을 적었다. 기준에 못 미치면 "다음 단계로 가지 말고 기록하고 멈춘다".**

---

## 0. 먼저 알아야 할 것 (5분)

### 0-1. 이 연구가 푸는 문제
64×64 도트 캐릭터 21프레임을 부위(모자·상의·장갑·바지·신발) 레이어로 나눠, 장비를 **일부만 바꿔 끼워도** 잘림·구멍·침범이 없게 하는 것.
1차 연구 결론: 한 장짜리 완성 그림을 잘라서는 **가려진 부분(코트 자락 밑 바지, 배낭 밑 상의)** 이 없어 빈칸이 생긴다. 생성 없이 메우는 보완 층(M3)은 그럴듯한 무늬를 지어낼 뿐이다.
→ 해결 가설: **생성 단계에서 옷을 한 겹씩 입히고, 단계 사이 픽셀 차이로 레이어를 뽑는다.** 그러면 각 레이어는 그 단계에서 끝까지 그려졌으므로 가려진 부분이 비지 않는다.

### 0-2. 폴더와 파일
| 경로 | 내용 |
|---|---|
| `<패키지>/frames_underwear_base/ds_*.png` | 속옷 몸 21프레임(모든 단계의 출발점) |
| `<패키지>/frames_coat_original/` | 원본 코트 캐릭터 21프레임 |
| `<패키지>/frames_leaf_set_transfer/` | 잎사귀 세트(한 번에 입힌 것) 21프레임 |
| `result/code/layered_extract.py` | **단계 그림 → 부위 레이어 + 지도 + 보고**(이번 실험용, 모의 검증 끝남) |
| `result/code/evaluate.py` | 조합 지표(구멍·속옷 노출·떠 있는 조각·침범) |
| `result/code/sheets.py`, `closeup.py` | 확인 시트 |
| `result/metrics.json` | 1차 연구 수치(비교 기준) |

`<패키지>` = 원래 받은 `LayerExtraction_Package` 폴더. 파이썬 3 + numpy + Pillow 만 필요.
프레임 이름: `ds_idle_0~7`(대기 8), `ds_stab_0~3`(찌르기 4), `ds_down_0~3`(내려베기 4), `ds_up_0~4`(올려베기 5, 3·4번은 뒷모습).

### 0-3. 비용 단가(문서 기준, 실제 잔액으로 처음 1회 확인할 것)
- `edit_image_pro_flash`(기준 그림 1장): 약 5 gen
- `transfer_outfit`(참조 1장의 옷을 2~15프레임에 입힘, 64px): 1회 약 10 gen(문서에 10~20으로 엇갈림)
- 21프레임 = 15 + 6 으로 나눠 2회.
- **처음 호출 전후로 잔액을 적어 실제 단가를 기록한다**(`gen_log.md`에 표로).

### 0-4. 공통 규칙(모든 생성에 적용)
1. 입력 프레임은 **항상 바로 앞 단계 결과**를 쓴다(속옷 → A → B). 원본 코트 그림을 입력으로 쓰지 않는다.
2. 캔버스 64×64, 투명 배경, 프레임 순서·이름 그대로. 시드 고정(예: 41), 같은 단계 안에서는 같은 시드.
3. 프롬프트 공통 꼬리: `same pose, same body proportions, same face and hair, pixel art, no effects, no background, keep everything else unchanged`.
4. 배낭·랜턴·무기·이펙트는 **어느 단계에서도 그리지 않는다**: `no backpack, no bag, no lantern, no weapon, no slash effect`.
5. 결과 PNG 는 `<실험 폴더>/<단계 이름>/ds_idle_0.png` 처럼 **원래 프레임 이름**으로 저장한다(전이 도구가 0.png, 1.png 로 주면 이름을 바꾼다. 순서는 입력 순서 그대로).

---

## 실험 1. 한 겹씩 입히기 시범 — 대기 8프레임 (약 30 gen) ★가장 먼저

**목표**: 단계 차이로 뽑은 레이어가 ① 가려진 부분까지 완전한지 ② 전이가 앞 단계를 다시 그려 생기는 밀림이 얼마인지 숫자로 본다.

### 1-1. 준비
```
mkdir gen_exp1
mkdir gen_exp1/A_legs gen_exp1/B_top
```
입력: `<패키지>/frames_underwear_base/ds_idle_0.png ~ ds_idle_7.png` (8장)

### 1-2. 단계 A = 하의만 (약 15 gen)
1. 기준 그림: `ds_idle_0` 속옷 몸에 `edit_image_pro_flash`
   - 프롬프트: `Add only dark brown leggings/trousers and keep the existing brown boots. Do NOT add any top, coat, jacket, gloves, hat, backpack. Upper body stays in the plain undershirt.` + 공통 꼬리 + 금지어.
   - 결과를 눈으로 보고 **상의가 그대로 속옷인지** 확인. 아니면 1회 다시(5 gen).
2. `transfer_outfit`: 참조 = 위 기준 그림, 입력 = 속옷 몸 `ds_idle_0~7`(8프레임, 1회).
3. 저장: `gen_exp1/A_legs/ds_idle_0.png ...`

### 1-3. 단계 B = 상의 (약 15 gen)
1. 기준 그림: `gen_exp1/A_legs/ds_idle_0.png` 에 `edit_image_pro_flash`
   - 프롬프트: `Add a long dark-teal coat with a skirt hem reaching mid-thigh, over the existing trousers. Do NOT change the trousers, boots, face or hair. No backpack.` + 공통 꼬리.
   - 확인: 바지·부츠 모양이 단계 A 와 같은가. 다르면 1회 다시.
2. `transfer_outfit`: 참조 = 위 기준 그림, 입력 = **단계 A 결과** `ds_idle_0~7`.
3. 저장: `gen_exp1/B_top/ds_idle_0.png ...`

### 1-4. 추출·채점 (생성 0)
```
python result/code/layered_extract.py <패키지>/frames_underwear_base gen_exp1/out legs+shoes=gen_exp1/A_legs top=gen_exp1/B_top
```
출력: `gen_exp1/out/layers/{legs,shoes,top}/ds_idle_*.png`, `gen_exp1/out/regions/*_region.png`, `gen_exp1/out/extract_report.json`.

`extract_report.json` 에서 프레임마다 읽을 값:
| 키 | 뜻 | 합격 기준 |
|---|---|---|
| `legs+shoes.new_px` | 단계 A 에서 새로 그린 픽셀 | 0 이면 실패(바지가 안 그려짐) |
| `top.new_px` | 단계 B 에서 새로 그린 픽셀 | — |
| `top.erase_px` | 단계 B 가 단계 A 그림을 지운 픽셀 | **≤ 10**. 크면 전이가 하의를 지우고 다시 그린 것 |
| `top.drift_px` | 하의 레이어 안쪽인데 단계 B 에서 바뀐 픽셀(다시 그림 의심) | **≤ 15**. 모의 검증에서 밀림 없이도 약 14 가 잡히므로 15 이하는 정상, 그 이상이면 시트로 눈 확인 |

눈 확인(필수): 각 프레임 `layers/legs` 를 열어 **코트 자락 밑 허벅지 부분까지 바지가 이어져 있는지** 본다. 이게 이 실험의 핵심 질문이다.

### 1-5. 조합 채점
잎사귀 세트(한 번에 입힌 것)와 섞어 입혀(32조합) 1차 권장안과 **같은 스크립트·같은 조건**으로 나란히 채점한다:
```
python result/code/score_layers.py <패키지> gen_exp1/out result/regions
```
출력 예: `{"frames": [...8개], "reference": {"holes": 0, "exposure": 1216, "strays": 96}, "gen": {"holes": .., "exposure": .., "strays": ..}}`
- `reference` = 1차 권장안(원본 코트를 지도로 자르고 보완 층). 대기 8프레임이면 **항상 holes 0 / exposure 1216 / strays 96** 이 나와야 한다(안 나오면 패키지 경로·`result/regions` 가 잘못된 것 → 멈추고 확인).
- `gen` = 이번 단계 레이어. 모자·장갑 단계가 없으면 그 부위는 빈 레이어(속옷 몸 그대로)로 채점된다.
- 지표: holes = 두 옷 다 덮는 자리인데 투명, exposure = 그 자리에 속옷 몸이 드러남, strays = 몸에서 떨어진 조각.

**합격**: `gen.exposure ≤ reference.exposure` 이고 `gen.holes ≤ reference.holes + 50`, 그리고 눈으로 봐서 바지 레이어의 자락 밑이 진짜 바지 그림일 것.
참고(생성 0 모의 실행): 원본 코트를 그대로 단계 B 로 쓴 가짜 데이터에서는 gen = holes 80 / exposure 2392 / strays 96 이 나온다. 가짜 단계 B 에 배낭이 들어 있어 상의 레이어가 배낭까지 품기 때문이다. 실제 실험은 배낭 없이 생성하므로 이 숫자와 비교하지 말 것(파이프라인이 끝까지 도는지 확인용).

### 1-6. 기록
`gen_exp1/RESULT.md` 에: 실제 gen 사용량, 1-4 표 값(프레임별), 1-5 숫자, 눈 확인 결과 한 줄씩, 실패면 증상(예: "단계 B 가 바지 색을 바꿈").

---

## 실험 2. 다른 종류 옷 1벌 — 한 겹씩 입히기로 (대기 8프레임, 약 30~40 gen)

**목표**: 치마·원피스·갑옷처럼 **코트와 다른 실루엣**에서도 실험 1 방식이 되는지. 1차 연구의 가장 큰 미검증 항목(일반화).

1. 실험 1 이 합격일 때만 진행. 불합격이면 원인부터 기록하고 멈춤.
2. 옷 선택(하나만): **판금 갑옷**(실루엣이 몸보다 넓음) 또는 **원피스/로브**(상의·하의 경계가 없음). 로브가 더 어려운 시험이다.
3. 단계 설계
   - 갑옷: A = `steel greaves and sabatons (leg armor, metal boots)` → B = `steel breastplate with pauldrons`.
   - 로브: A = `plain dark trousers` → B = `long hooded robe reaching the knees` (자락이 무릎까지 = 하의를 가림).
4. 실험 1 의 1-2~1-6 을 그대로 반복(폴더 `gen_exp2/`). 추가로 기록할 것:
   - 로브의 경우 `layered_extract.py` 지도에서 **자락이 상의(2)로 잡히는지** (`regions/*_region.png` 를 `python result/code/sheets.py regions` 로 시트화).
   - 갑옷 어깨처럼 **몸 밖으로 크게 나온 부분**이 `top` 레이어에 다 들어가는지(레이어 PNG 눈 확인).
5. 합격: 실험 1 과 같은 기준 + 레이어 PNG 에 다른 부위 그림이 섞이지 않음.

---

## 실험 3. 본 적용 21프레임 (세트당 약 50~75 gen) — 1·2 합격 후에만

1. 실험 1(또는 2) 단계 설계 그대로, 입력을 21프레임으로(15+6 으로 2회씩).
2. 뒷모습 `ds_up_3`, `ds_up_4` 는 따로 눈 확인: 1차 연구에서 뒷모습은 배낭이 등을 덮어 상의 등판 그림이 없었다. 이번에는 배낭 없이 생성하므로 **등판이 실제로 그려졌는지**가 확인 포인트.
3. 배낭·랜턴은 별도 실험(선택): 마지막 단계 결과에 `edit_image_pro_flash` 로 배낭만 추가 → `layered_extract.py ... back=<배낭 단계 폴더>` 처럼 한 단계 더 붙이면 `back` 레이어가 따로 나온다(코드는 `back` 이름을 지원, 슬롯 7).
4. 1차 표와 같은 지표로 `score_layers.py` 를 21프레임에 돌려 `result/metrics.json` 의 권장안 행과 나란히 표를 만든다.

---

## 4. 실패했을 때 보는 곳
| 증상 | 확인 | 대처 |
|---|---|---|
| `erase_px` 가 큼 | 단계 B 그림에서 바지가 지워졌나 | 프롬프트에 `do not remove or repaint the trousers` 추가, 시드 바꿔 1회 |
| `drift_px` 가 큼(>15) | 단계 B 에서 바지가 1~2px 밀려 다시 그려졌나 | `layered_extract.py` 의 `T_DIFF`(기본 30)를 40 으로 올려 재추출(생성 0). 그래도 크면 기록 |
| 바지 레이어에 상의 픽셀 | 단계 A 에서 상의까지 그려짐 | 단계 A 기준 그림을 다시(상의 금지 문구 강화) |
| 프레임마다 옷 모양이 다름 | 전이 일관성 문제 | 시드 고정 확인, 15프레임 한 번에 넣었는지 확인 |
| 머리·얼굴이 바뀜 | 전이가 머리를 다시 그림 | `under_base.py` 처럼 원본 머리를 덮어씀(1차 패키지 `code/under_base.py` 방식) — 이건 레이어 추출 전에 단계마다 적용 |

## 5. 생성 없이 이미 확인된 것(재시도 불필요)
- `layered_extract.py` 모의 검증(`result/study2/sim_layered.json`): 정답을 아는 가짜 단계 그림 21프레임에서 레이어만으로 단계 그림을 재구성했을 때 틀린 픽셀 평균 단계 A 14px · 단계 B 8px(프레임당 약 1,000px 중), 상의 레이어 IoU 0.997.
  다리 쪽 8×8 블록을 1px 밀어 넣으면(전이 다시 그림 흉내, 프레임당 23px) 그중 약 7px 가 상의 레이어로 샌다 → **밀림의 약 30% 는 못 거른다.** 실험 1 의 `drift_px` 를 꼭 볼 것.
- 속옷 몸을 키 프레임 1장만 생성하고 나머지는 원본끼리 패치 정합으로 옮기는 방법은 실패(구멍 128 → 2,080). 속옷 몸은 프레임마다 생성이 필요하다.
