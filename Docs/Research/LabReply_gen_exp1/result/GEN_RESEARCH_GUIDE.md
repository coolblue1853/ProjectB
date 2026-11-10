# 생성 실험 지시서 (최신 11-10) — Sonnet 등 다른 모델이 그대로 따라 하도록

이 문서만 보면 된다. 예전 판(실험 1·1b·2·3 등)은 `archive/GEN_RESEARCH_GUIDE_history.md`이고 **더 이상 하지 않는다.**
규칙: 각 실험의 합격 기준에 못 미치면 다음 실험으로 가지 말고, `RESULT.md`에 기록하고 멈춘다. 생성은 사용자가 하고, 결과 폴더를 연구실로 보낸다.

## 0. 알아야 할 것
- **설계(확정):** 외형 4칸 = 머리 · 갑옷(한 벌) · 장갑 · 신발. 배낭은 기본 코트 전용 꾸미기(켜고 끄기, 다른 옷이면 사라짐). 색 변형은 쓰지 않고 세트마다 새로 생성한다.
- **생성 원칙:** 세트는 항상 **한 벌을 한 번에** 입힌다. 부위별 단독 생성은 품질이 급감한다(11-10 사용자 확인).
- **PixelLab 단가(공식 문서 확인):** Transfer Outfit (Pro)은 32~64px에서 1회에 2~15프레임, **10 gen**, 내부적으로 4×4 격자로 처리한다. Edit Animation (Pro)은 64px에서 16프레임까지 텍스트 지시 하나로 일괄 수정, 10 gen. 둘 다 Aseprite·Pixelorama 확장 전용.
- **같은 호출 안의 프레임끼리 일관성이 높다**(한 격자로 생성). 키 프레임이 적을수록 호출 수가 줄어 세트 안의 일관성에도 유리하다.
- **공통 프롬프트 꼬리:** `same pose, same body proportions, same face and hair, pixel art, no effects, no background, keep everything else unchanged`
- **공통 금지어(새 세트):** `no backpack, no bag, no lantern, no weapon`
- **파일 이름:** 결과 PNG는 원래 프레임 이름(`ds_idle_0.png` …)으로 저장한다. 시드는 고정한다(예: 41).
- `<패키지>` = `LayerExtraction_Package` 폴더, `result/` = 이 폴더.

## 실험 A. 키 프레임만 넣은 전이의 품질 — 가장 먼저 (약 10 gen)
목적: 비용표(세트당 75 → 35 gen)의 전제인 "키 7장만 `transfer_outfit`에 넣어도 키 그림 품질이 같다"를 확인한다.
1. 입력: `<패키지>/frames_underwear_base/`의 7장. `ds_down_0, ds_down_2, ds_idle_0, ds_stab_1, ds_up_0, ds_up_1, ds_up_3` (`result/study5/keyframe_select.json`의 `by_tau["0.2"].keys`)
2. 참조: 잎사귀 세트 기준 그림(예전 실험의 `ref_b.png`). 시드 41. Transfer Outfit 1회.
3. 저장: `gen_expA/keys/<프레임 이름>.png` (7장)
4. 연구실로: 폴더 + 사용 gen.
5. 연구실이 할 일: 같은 7장을 기존 `frames_leaf_set_transfer`(21장을 한꺼번에 전이한 것)와 비교하고, 나머지 14장을 키에서 옮겨 `study4_slots.py` 지표를 낸다.
6. 합격(눈 + 수치): 키 그림이 기존 잎사귀와 같은 옷으로 보이고, 연구실 지표(합성 1개당 구멍·노출·조각)가 기존 잎사귀 21장으로 잰 값의 ±20% 안.

## 실험 B. 기본 코트 뒷모습 배낭 제거 (약 10 gen)
목적: 배낭을 껐을 때 뒷모습 등판(지금은 생성 0 보완 무늬)을 진짜 그림으로.
1. 입력: `<패키지>/frames_coat_original/ds_up_3.png`, `ds_up_4.png`
2. 각 프레임 `edit_image_pro_flash` 1회(5 gen × 2):
   `Remove the backpack and the lantern completely. Show the back of the same dark-teal coat where the backpack was. Do not change the pose, hair, head, arms, legs, boots or coat outline.` + 공통 꼬리
3. 저장: `gen_expB/coat_nobag/ds_up_3.png`, `ds_up_4.png`. 마음에 안 들면 그 프레임만 1회 다시.
4. 합격: 눈 확인(코트 실루엣·팔·머리가 원본과 같음). 연구실이 배낭 바깥 픽셀 변화를 재서 프레임당 30px 이하.

## 실험 C. 새 세트 1벌 — 표준 절차 시범 (약 15~20 gen, 실험 A 합격 후)
1. 기준 그림: 속옷 몸 `ds_idle_0`에 `edit_image_pro_flash`로 새 옷 **한 벌**(모자·장갑·신발 포함, 배낭·소품 없음). 5 gen, 시안 2개면 10.
   - 옷 종류는 코트와 실루엣이 다른 것(원피스·판금 갑옷 등)을 권장한다. 다른 종류에서도 지도로 잘리는지 확인하기 위해서다.
2. Transfer Outfit 1회: 입력 = 실험 A의 키 7장 속옷 몸, 참조 = 1번 그림, 시드 41(10 gen).
3. 저장: `gen_expC/<세트 이름>/keys/<프레임 이름>.png`
4. 연구실이 할 일: 나머지 14장 옮기기 → `export_runtime.py map … --armor --cap-under under_fixed` → 코트·잎사귀와 섞어 입혀 지표·시트 작성.
5. 합격: 구멍·노출·조각이 `metrics.json`의 `recommended` 값(합성 1개당 0.19 · 2.71 · 0.52)의 2배 이하. 눈으로 봐서 갑옷 레이어에 머리카락·손이 섞이지 않을 것.

## 실험 D. 전체 98장 키 수 확인 (생성 0, 사용자 쪽 데이터)
1. 프로젝트의 원본 코트 98장(고유 프레임 목록: `result/unique_frames.json`)을 `frames_coat_original/` 형식(`ds_<동작>_<k>.png`)으로 한 폴더에 모은다. 그 폴더의 부모를 `<패키지98>`이라 하자.
2. `python result/code/keyframe_select.py <패키지98> keys98.json 0.15,0.2,0.25`
3. `keys98.json`의 `by_tau["0.2"].n_keys` 기록 → 세트당 실제 비용 = ceil(n_keys / 15) × 10 + 5.

## 실험 E(선택, 헤어스타일 교체를 넣을 때만). 대머리 속옷 몸 + 헤어 1종 (약 20 gen)
1. 대기 8프레임 속옷 몸에 `edit_image_pro_flash`로 대머리 기준 그림(5) → Transfer Outfit 8장(10) → `gen_expE/bald/`
2. 대머리 기준 그림에 지금 머리 모양을 다시 그린 그림 1장(5) → `gen_expE/hair_ref.png`
3. 연구실이 할 일: 차이로 헤어 레이어를 뽑고 앞머리/뒷머리로 나눈 뒤, 잎사귀 두건과 "모자 윗선 위로 솟은 머리만 지움" 규칙을 확인한다.

## 하지 말 것
- 상의·바지·모자·장갑을 단독 단계로 생성하지 않는다(품질 급감).
- 하의만 단계 생성(예전 실험 1b)은 4칸에서 필요 없다.
- 색 변형으로 세트를 늘리지 않는다(기획: 세트마다 새로 생성).
