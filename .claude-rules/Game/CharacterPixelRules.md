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
| 10-25 | 주인공 | **Retro Diffusion RD Pro** 2장(앵커 참조, seed 2201, $0.36) | 배경 미제거라 수치 무효(29~32색·33~38%·7~9%) | 담백하지만 **갈색 외곽선·2.5~3등신 → 기준에서 멂** | `hero_proto/rd_1.png`·`rd_2.png`, 시트 `hero_proto/rd_vs_pixellab.png` |
| 10-25 | 주인공 | **사용자 판정: "픽셀랩 좌측(정리본)이 제일 가깝다"** | — | **현재 기준 시안 = `styled1_auto.png`** | 같은 시트 |

## 9. 예산

- PixelLab 요금(10-25): $12/월 2,000 · $24/월 5,000 · $50/월 10,000 generations. 체험판은 매일 5회(최대 20 누적).
- 캐릭터 1장 = Pro Flash 5. 시안 탐색은 하루 무료분으로 1~3장, 본격 제작(애니·방어구 레이어)은 유료 플랜 전제.

## 11. 세대별 보정 학습 — 원본 재현으로 레시피 다듬기 (사용자 제안 2026-10-25)

> 사용자: "그냥 픽셀랩을 쓰자. 완벽하게 하기 위해 **원본을 말로 설명해서 뽑아내서 원본이랑 비슷하게 나오면 완료**되고, 아니면 다른 부분을 조정해서 강화하는 방식으로 **세대별 학습처럼** 돌려 보고 싶다."

- **목표**: 앵커 원본(빨간 머리 여우 소녀, `hero_style_anchor_64.png`)을 **말(설명문)+파라미터만으로** 재현하는 레시피를 찾는다. 재현되는 레시피 = 그림체를 정확히 붙잡은 레시피 → 새 캐릭터(주인공 등)에 내용만 바꿔 적용한다.
- **점수**: `python Tools/pixelfx/sprite_similarity.py <원본> <후보...>` — silhouette(형태)40·palette(색감)20·detail(담백함·외곽선 지표)25·size 15, 종합 0~100. 머리 비율은 긴 머리·꼬리 때문에 자동 추정이 틀려(원본 0.96) 참고값만 — 비교 시트로 눈으로 본다. 검증: 원본끼리 100. ⚠ **같은 내용(원본 재현)끼리만 의미가 있다** — 주인공 시안처럼 내용(배낭 등)이 다르면 형태 점수가 비교 불가라, 사용자가 탈락시킨 텍스트 시안(type1 66.6)과 정리본(63.8)이 비슷하게 나왔다. 세대별 보정 학습(원본 재현)에만 쓰고, 다른 캐릭터 판정은 3절 지표 + 비교 시트로 한다. 가장 낮은 항목(`weakest`)이 다음 세대에서 고칠 곳.
- **루프**(한 세대 = 후보 2~3장, 같은 레시피에 seed만 다르게):
  1. 현재 레시피로 생성 → `Docs/ArtCompare/calib/gNN_sK.png` 저장.
  2. 점수 → 최고 후보 선택, `weakest` 확인.
  3. 한 세대에 **한두 손잡이만** 바꾼다(무엇이 효과였는지 알 수 있게):
     | 약한 항목 | 바꿀 손잡이 |
     |---|---|
     | head | 설명문 비율 표현("head is about 60% of total height", "very short body"), 캔버스 크기 |
     | silhouette | 자세·소품 묘사(정면, 팔 내림, 꼬리 위치), `view`·방향 |
     | palette | 색 단어(crimson/amber/off-white/dark maroon), `max_colors`, 팔레트 강제(`palette`) |
     | detail | 평면 문구, `max_colors`, style_options(detail·shading), 외곽선 문구 |
     | size | 캔버스·"fills the canvas" 문구 |
  4. `Docs/ArtCompare/calib/gen_log.md`에 세대·레시피 변경점·점수 기록. 시트 `compare_sheet.py`로 원본과 나란히.
  5. **종료**: 종합 ≥ 85이고 비교 시트로 봐도 같은 그림체(사용자 확인 1회) → 레시피를 5절 파이프라인에 반영.
- **두 가지 모드**를 나눠 잰다: (A) style_image 없이 말만 — 설명문의 힘 측정, (B) style_image(앵커) 포함 — 실제 생산 조건. B는 원본을 직접 보므로 점수가 높게 나오는 게 당연하고, A의 점수가 레시피 품질이다.
- 비용: Pro Flash 5/장. 세대당 2~3장 × 6~10세대 ≈ 60~150 generations → 유료 플랜 필요(무료는 하루 1장 수준).
- **진행 방식 변경(사용자 10-25)**: 0세대(원본 재현, 말만) 뒤부터는 **세대마다 사용자 허가** → **현재 생산 레시피로 랜덤 컨셉 캐릭터 4장** → Claude 판정(3절 지표 + 비교 시트) + **사용자 굿/배드** → 레시피 수정. 원본 재현 점수는 참고용으로 계속 잰다. 기록 `Docs/ArtCompare/calib/gen_log.md`.
- 플랜: PixelLab Pixel Apprentice $12/월 2,000 generations(10-25 결제, 11-25 갱신).

### 0세대 레시피 (원본 설명)

- 설명문(A·B 공통 내용부): `"super deformed chibi girl, about 2 heads tall, head is about 60% of her total height, very short body and stubby limbs, standing facing the viewer with arms down. Very long crimson red hair falling past her knees with darker red strands, messy bangs, big fluffy fox ears with light gray inner fur on top of her head, a big fluffy fox tail that is crimson with a white tip curling to her right. Large amber eyes with white highlights, a small open smile with one tiny fang, pink blush. White blouse with puffy sleeves and a red ribbon, a dark maroon pinafore apron dress with a small gold buckle, short pink legs. Simple clean pixel art: large flat areas of solid color, thick black 1px outline, flat 2-3 tone shading, about 18 colors"`
- 파라미터: `create_image_pro_flash` 64×64, `no_background` true, `max_colors` 18, seed 3001~3003. (A) style_image 없음 / (B) style_image = 앵커, style_options outline·detail·shading 켬, color_palette 켬(원본 재현이므로).

## 10. Retro Diffusion (비교 도입, 2026-10-25)

- 연결: MCP `https://mcp.retrodiffusion.ai/mcp`, 헤더 `Authorization: Bearer rdpk-…`. 처음 등록 때 안내 예시 `rdpk-YOUR-KEY`가 그대로 들어가 인증이 안 됐다(도구가 로그인·스타일 4개만 보임) → 실제 키로 재등록. **세션 시작 뒤 등록한 MCP는 그 세션에 도구로 안 잡히므로**, 설정(`~/.claude.json`)을 읽어 직접 호출하는 스크립트(스크래치패드 `rd_call.py`, 출력은 `PYTHONIOENCODING=utf-8`)로 썼다.
- 가입 무료 잔액: 크레딧 50 = $0.50. RD Pro 장당 약 $0.18 → 무료로는 2장.
- 캐릭터에 쓸 설정: `create_inference` · `prompt_style` `rd_pro__default` · 64×64 · `reference_images` = [앵커 base64] · `remove_bg` true + 프롬프트에 "On a plain white background" · `upload_outputs` true · `seed` 고정. 프롬프트는 **대상만 묘사**하고 "pixel art"라는 말을 쓰지 않는다(스타일이 렌더링 담당, 공식 지침). 생성 전 `estimate_inference_cost`로 무료 견적.
- 커스텀 스타일(`create_user_style`)은 공개 API에서 참조 이미지 **1장**까지 — 앵커 1장으로 만들 수 있다.
- 애니메이션: `rd_animation__any_animation`(64×64 입력), `rd_advanced_animation__custom_action`(시작 프레임 + 동작 묘사, 32~256), 8방향 회전(80×80), `rd_animation__large_walking_idle`(64×64 4방향 걷기·대기).
- **비교 결론(10-25)**: RD Pro는 잔무늬 없이 담백하고 디자인 반영(눈가림 앞머리·배낭·브로치)도 좋았지만, **외곽선이 갈색이고 몸이 길어(2.5~3등신)** 앵커 그림체에서 멀었다. 사용자도 "PixelLab 좌측(정리본)이 제일 가깝다". → **캐릭터 생성은 PixelLab 파이프라인 유지.** RD는 담백한 소품·아이콘·타일셋 쪽 후보로 남긴다.
- 주의: `estimate_inference_cost`는 `remove_bg`·`seed` 인자를 받지 않아 실패했고, `create_inference`에서도 배경 제거가 반영되지 않아 숲 배경이 남았다 → 쓸 거면 생성 뒤 `run_edit_tool`의 배경 제거나 `Tools/pixelfx/remove_flat_bg.py`로 지운다.
- 남은 무료 잔액 $0.14.
