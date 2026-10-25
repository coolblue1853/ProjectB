---
name: pixel-character
description: Project B 캐릭터(주인공·NPC) 도트를 우리가 다듬어 온 그림체(2등신 데포르메·검정 외곽선·담백한 평면 색)로 PixelLab에서 생성하고, 정리·판정·비교 시트·기록까지 한 번에 하는 절차. "캐릭터 뽑아줘", "NPC 도트 만들어줘", "주인공 시안", "이 그림체로 캐릭터" 같은 요청에 쓴다. 몹·배경·이펙트에는 쓰지 않는다.
---

# 캐릭터 도트 생성 (pixel-character)

규칙의 원본은 `.claude-rules/Game/CharacterPixelRules.md`다. **시작할 때 그 문서를 Read로 읽고** 따른다(이 스킬은 실행 순서만 요약한다). 모든 진행 메시지는 한국어.

## 0. 준비
1. `CharacterPixelRules.md`를 읽는다 — 특히 4절(그림체), 6절(방어구 레이어), 7절(실패 사례).
2. PixelLab 잔액 확인: `mcp__pixellab__get_balance` (Pro Flash 1장 = 5 generations). 모자라면 사용자에게 알리고 멈춘다.
3. 사용자가 말한 **외형**(직업·머리·옷·소품·무기)만 설명문으로 옮긴다. 앵커의 내용물(동물 귀·꼬리·붉은 머리)은 쓰지 않는다. 방어구 레이어를 고려해 부위가 겹치지 않는 실루엣으로 묘사한다.

## 1. 생성 (설명문은 영어)
- 앵커 base64:
  `python -c "import base64;print(base64.b64encode(open('Docs/ArtCompare/hero_style_anchor_64.png','rb').read()).decode())"`
- `mcp__pixellab__create_image_pro_flash` 인자:
  - `description`: `<외형> + ", super deformed chibi, 2 heads tall, full body, standing. Simple clean pixel art: large flat areas of solid color, minimal texture, no noise, no dithering, few details, thick black outline, hair with 2-3 tone strand shading"`
  - `width` 64, `height` 64, `no_background` true, `max_colors` 18, `seed` 고정값(변형 비교 시 같은 seed)
  - `style_image`: `{"base64": "<앵커>", "usage_description": "Copy this exact pixel art drawing style: super deformed 2-head chibi proportions, chunky pixel size, thick black outline, big simple eyes, flat 2-3 tone shading, large flat color areas with almost no texture, about 18 colors. Ignore its animal ears, tail and red hair."}`
  - `style_options`: `{"color_palette": false, "detail": true, "outline": true, "shading": true}`
- 동시 작업은 1개(trial). `mcp__pixellab__wait_for_jobs` → 다운로드:
  `curl -s -o Docs/ArtCompare/<캐릭터>/raw_<seed>.png https://api.pixellab.ai/mcp/images/<job_id>/download`
- **텍스트만 쓰는 `create_image_pixflux`는 쓰지 않는다**(그림체가 따라오지 않음, 7절).

## 2. 정리
`python Tools/pixelfx/flatten_keep_detail.py raw.png clean.png --merge 14 --speck 28 --head-auto`
(몸은 담백하게, 머리는 가닥 음영 유지, 검정 외곽선·고대비 디테일 보존)

## 3. 판정 (Claude가 직접)
1. `python Tools/pixelfx/sprite_metrics.py raw.png clean.png` → **OK 이상**이어야 한다(IDEAL이면 더 좋음).
2. `python Tools/pixelfx/compare_sheet.py Docs/ArtCompare/<캐릭터>/compare.png REF=Docs/ArtCompare/hero_style_anchor_64.png "원본=raw.png" "정리=clean.png"` → Read로 열어 **형태**(2등신·머리 크기·검정 외곽선·눈)와 **사용자가 말한 외형**이 맞는지 본다.
3. FIX거나 형태가 틀리면 seed를 바꿔 1로 돌아간다(예산 안에서). 사용자에게 "어느 게 좋냐"고 고르게 하지 않는다 — 통과한 최선을 가져간다.

## 4. 전달·기록
1. 비교 시트를 `SendUserFile`로 보낸다(캡션 한 줄: 무엇을 어떻게 맞췄는지).
2. `CharacterPixelRules.md` 8절 이력에 한 줄(날짜·캐릭터·seed·지표·판정·파일).
3. 사용자 피드백이 오면 **규칙으로 바꿔** 4~7절에 넣는다(실패면 7절 표, 새 원칙이면 4절). 이 스킬 자체를 고쳐야 하면 같이 고친다.
4. 커밋: `(추가) <캐릭터> 도트 시안 …` + 검증(지표·시트 확인, 인게임 미적용이면 "미검증").
