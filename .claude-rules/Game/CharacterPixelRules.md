# 캐릭터 도트 생성 규칙

> 주인공·NPC 같은 **캐릭터** 도트를 PixelLab으로 만들 때의 규칙이다. 몹·배경·소품은 `ArtStyle.md`를 따른다.
> 스킬 `.claude/skills/pixel-character/`가 이 문서를 그대로 실행한다. **시안을 뽑을 때마다 8절 이력에 한 줄을 추가하고, 사용자 피드백은 해당 절에 규칙으로 바꿔 넣는다**(사용자: "이런 식으로 발전해 나가면서 쌓는 게 좋을 거야").

## 1. 원칙

- 사용자: "그림을 똑같이 만들어 달라는 게 아니라, **캐릭터 신체 외형적 데포르메나 형태**에 **내가 말한 캐릭터의 외형**, 그리고 **저런 류의 색감 및 정도**." "나한테 물어볼 것도 없이 원본 스타일에 가깝게 하면 되는 것."
  - **따라 하는 것**: 앵커의 그림체 — 2등신 데포르메, 도트 크기, 검정 외곽선, 단순한 큰 눈, 평면 음영, 색 쓰는 방식, 담백함의 정도.
  - **따라 하지 않는 것**: 앵커의 내용물(동물 귀·꼬리·붉은 머리·앞치마 등). 캐릭터 내용은 사용자가 말한 외형을 쓴다.
- **판정은 Claude가 한다.** 3절 지표와 비교 시트로 기준에 맞는지 스스로 판정해, 기준을 통과한 것만 사용자에게 보여 준다. "어느 쪽이 좋냐"고 고르게 하지 않는다(사용자가 보고 지적하면 그걸 규칙으로 쌓는다).

## 2. 기준 앵커

| 용도 | 파일 | 출처 |
|---|---|---|
| **형태·그림체 (최종)** | `Docs/ArtCompare/hero_style_anchor_64.png` (64×64 투명, 실제 도트 크기) | 핀 `632896553930144417` 원본 `hero_form_best_1.png`(60×60을 12배 확대한 이미지) |
| 형태 보조 | `Docs/ArtCompare/hero_form_best_2.gif` (128×128, SD 3인, 인게임 키 30~32px) | 핀 `1143492161662892815` |
| 색감 | 위 두 장 — "저 색이 아니라 저런 색감" | 같은 핀 |
| 타입 다양화 참고 | 같은 SD 그림체에 타입만 다른 6인 | 핀 `950681802562666411`(마일드 티니) |

- 앵커는 PixelLab `style_image`로 넣는다. 크기를 바꾸지 않은 실제 도트 크기 PNG여야 한다(확대본을 넣으면 그림체가 무너진다).
- 폐기된 앵커: 3.5~4등신·56~64px(PixelTheWise 라인업), 저채도 흙빛 2안(Owlboy풍) — 형태가 다르다. 색·동세 참고로만.

## 3. 판정 지표 — `python Tools/pixelfx/sprite_metrics.py <png...>`

| 지표 | 뜻 | IDEAL (앵커 수치) | OK (사용자 합격선) |
|---|---|---|---|
| colors | 불투명 색 수 | 12~20 (앵커 18) | 10~28 |
| edge | 이웃 픽셀과 색이 다른 비율 — **잔무늬 양** | 35~48% (앵커 43%) | 30~55% |
| isolated | 외톨이 점 비율 | 0~15% (앵커 10%) | 0~26% |
| black | 검정 비율 — 외곽선 존재감 | 18~32% (앵커 24%) | 16~34% |

- OK 기준은 사용자가 "적당하다"고 한 시안(`hero_proto/hero_mix_Bhair_B2body.png` = 27색·52%·25%·21%)에서 왔다.
- 지표는 담백함만 잰다. **형태(2등신·머리 크기)·디자인**은 비교 시트(`Tools/pixelfx/compare_sheet.py`)로 앵커와 같은 배율에 놓고 눈으로 본다.
- 사용자에게 보여 주는 최소 기준: OK. IDEAL에 가까울수록 좋다.

## 4. 그림체 규칙

1. **2등신 데포르메**: 머리 높이 ≥ 몸 높이. 팔다리는 짧고 뭉툭(손 2~3px 덩어리, 발 2px).
2. **크기**: 생성 캔버스 64×64(캐릭터 키 약 46~50px). 인게임 목표 키 32~36px은 레이어 구조가 정해진 뒤 축소판으로 다시 맞춘다(축소는 재생성으로, 리샘플링 금지).
3. **외곽선**: 검정 1px, 실루엣 전체 + 눈선.
4. **얼굴**: 단순한 큰 눈(하이라이트 1px), 볼 홍조 1~2px, 입 1~2px. 앞머리로 눈을 가리는 디자인도 허용(사용자 주인공 = 눈가림 더벅머리).
5. **색감**: 캐릭터당 주색 하나 + 흰·연회색 무채색 + 작은 반대색 포인트(눈·장식 1~2곳). 채도 중상(0.5~0.7), 명도 밝게, 탁한 갈색·회색 금지. 대비는 어두운 면 대신 검정 외곽선으로.
6. **담백함**: 큰 평면 색 덩어리. 면 안의 잔무늬·노이즈·디더링 금지 — 사용자가 "디테일이 너무 많다"고 한 것의 정체는 이 잔무늬였다(시안 72% → 앵커 43%).
7. **단, 머리카락은 2~3단 가닥 음영을 남긴다**: 단색 면으로 밀면 "머리 디테일이 너무 빠졌다"(사용자). 담백화는 몸·옷·소품에 강하게, 머리에 약하게.
8. **고대비 디테일은 지킨다**: 브로치·버클·옷깃·무기 날·눈가 — 전체 색 감축으로 뭉개면 "그냥 색만 뭉개진 것 같다"(사용자).

## 5. 생성 파이프라인

1. **설명문** = `[사용자가 말한 외형]` + 고정 꼬리말:
   `"super deformed chibi, 2 heads tall, full body, standing. Simple clean pixel art: large flat areas of solid color, minimal texture, no noise, no dithering, few details, thick black outline, hair with 2-3 tone strand shading"`
2. **PixelLab** `create_image_pro_flash`: width 64 · height 64 · `no_background` true · `max_colors` 18 · `seed` 고정(같은 디자인 비교용) ·
   `style_image` = `{"base64": <앵커 base64>, "usage_description": "Copy this exact pixel art drawing style: super deformed 2-head chibi proportions, chunky pixel size, thick black outline, big simple eyes, flat 2-3 tone shading, large flat color areas with almost no texture, about 18 colors. Ignore its animal ears, tail and red hair."}` ·
   `style_options` = `{"color_palette": false, "detail": true, "outline": true, "shading": true}`.
   앵커 base64: `python -c "import base64;print(base64.b64encode(open('Docs/ArtCompare/hero_style_anchor_64.png','rb').read()).decode())"`.
   비용 1장 5 generations. **텍스트만 쓰는 pixflux로는 그림체가 따라오지 않는다**(1차 6타입 전부 불합격).
3. **정리**: `python Tools/pixelfx/flatten_keep_detail.py in.png out.png --merge 14 --speck 28 --head-auto` — 몸은 가까운 색 병합 + 비슷한 외톨이 점 제거, 머리(위 55%)는 약하게(--head-merge 6, --head-speck 20), 검정 외곽선 불변.
   - **원본이 이미 OK(edge ≤ 55%)면 정리를 건너뛰거나 `--merge 8 --speck 20`으로 약하게.** max16 원본(OK)을 기본 강도로 한 번 더 정리하니 수치는 IDEAL(13색·37%·12%)인데 **머리 가닥이 전부 지워져 단색**이 됐다 — 지표는 머리 밋밋함을 못 잡는다.
4. **판정**: `sprite_metrics.py`로 OK 이상인지 확인 → `compare_sheet.py out.png REF=<앵커> "원본=..." "정리=..."`로 형태 확인. 실패하면 seed를 바꿔 다시(같은 날 예산 안에서).
5. **보관**: `Docs/ArtCompare/<캐릭터>/` 아래 원본·정리본·비교 시트, 8절 이력 한 줄.

## 6. 방어구 교체 대비 (사용자 필수 조건, Q-420)

- "방어구를 교체하면 교체한 방어구에 따라 외형이 바뀌어야 하니 그거에 대비되어야 한다."
- **[임시 가정] 레이어 합성**: 맨몸 기본형(머리카락·얼굴·피부·속옷) 시트 + 부위별 방어구 오버레이 시트(머리·몸통·손·다리·신발, 같은 프레임·같은 발 위치) + 배낭·무기 별도 레이어를 런타임에 겹친다.
- 그래서 디자인 단계부터 **부위가 겹치지 않는 실루엣**으로 그린다: 모자·투구는 머리카락 위에 얹히는 모양, 코트·갑옷은 몸통, 장갑·부츠는 끝단, 배낭은 등 뒤 별도.
- 제작 순서(유료 플랜 필요 기능 포함): 맨몸 기본형 확정 → `create_character_state`/`edit_image`("wearing X", 기본형 팔레트 유지) 또는 `transfer_outfit`(유료)로 착용판 → 기본형과의 차이 픽셀만 잘라 오버레이.

## 7. 실패 사례 (반복 금지)

| 날짜 | 한 것 | 결과·사용자 반응 | 교훈 |
|---|---|---|---|
| 10-25 | pixflux 텍스트만으로 6타입 | "이거랑 비슷한 느낌인 건 하나도 없다" | 그림체는 style_image로만 따라온다 |
| 10-25 | Pro Flash 시안을 16색 일괄 감축(B) | "단순히 뭉개서 디테일이 사라지고 색만 뭉개졌다" | 고대비 디테일 보존형 정리만 쓴다 |
| 10-25 | 디테일 보존형 강하게(B3·B4) | 배낭 초록이 갈색에 먹힘 | merge 14 이하 |
| 10-25 | 머리까지 같은 강도로 평면화(B2) | "머리 디테일이 너무 빠졌다" | 머리는 약하게(--head-auto) |
| 10-25 | 이미 담백한 원본을 기본 강도로 또 정리 | 수치는 IDEAL, 머리 가닥 소실 | 원본 OK면 정리 생략·약하게, 시트로 머리 확인 |
| 10-25 | 변형 여러 개를 보여 주고 고르게 함 | "나한테 물어볼 것도 없이 원본 스타일에 가깝게 하면 된다" | 지표·시트로 직접 판정해 가져온다 |

## 8. 이력

| 날짜 | 캐릭터 | 시안 | 지표 | 판정 | 파일 |
|---|---|---|---|---|---|
| 10-25 | 주인공(모험가 소녀) | Pro Flash seed 2201 원본 | 37색·72%·41%·21% | FIX | `hero_proto/styled1.png` |
| 10-25 | 주인공 | 위 + max_colors 16 재생성 | 16색·53%·22%·19% | OK | `hero_proto/styled2_flat.png` |
| 10-25 | 주인공 | styled1 + 정리(--head-auto) | 28색·52%·25%·21% | OK — 사용자 "적당" | `hero_proto/styled1_auto.png` (= 수작업 합성 `hero_mix_Bhair_B2body.png`) |
| 10-25 | 주인공 | styled2_flat + 기본 강도 정리 | 13색·37%·12%·19% | 수치 IDEAL이나 **머리 단색 → 불합격**(과정리) | `hero_proto/styled2_auto.png`, 시트 `hero_proto/compare.png` |

## 9. 예산

- PixelLab 요금(10-25): $12/월 2,000 · $24/월 5,000 · $50/월 10,000 generations. 체험판은 매일 5회(최대 20 누적).
- 캐릭터 1장 = Pro Flash 5. 시안 탐색은 하루 무료분으로 1~3장, 본격 제작(애니·방어구 레이어)은 유료 플랜 전제.
