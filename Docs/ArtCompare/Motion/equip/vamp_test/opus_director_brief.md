# 감독 지시서 — 뱀파이어 활공 걷기 (8프레임 루프, 64x64, 오른쪽 옆모습)

너는 실행자다. 이 지시서만 보고 그대로 수행한다. 한글로 보고한다.

## 0. 금지
- 읽지 말 것: `Docs/ArtCompare/Motion/equip/vamp_test/sonnet/`, `vamp_test/opus/`, 다른 실행자의 결과 폴더.
- 이전 결과 이미지(`vamp/glide_*`, `vamp/floatpose.png` 등) 복사·업로드·제출 금지. 입력은 `Docs/ArtCompare/Motion/equip/vamp/under_idle0.png` 하나뿐.
- inpaint 계열(`inpaint_image`, `inpaint_image_pro_flash`), `transfer_outfit`, 부위 오려 붙이기, 팔레트 교체, 골격(`animate_with_skeleton_v3`) 금지.
- Unity·git·저장소 기존 파일 수정 금지, 커밋 금지. 쓰기는 네 결과 폴더(이하 `OUT`) 안에서만.
- **세대 상한 합계 20.** 매 호출 결과의 `estimated_generations` 를 `OUT/gen_log.txt` 에 한 줄씩 누적 기록한다. 다음 호출 후 합계가 18을 넘게 되면 그 호출을 하지 않는다.

## 1. 준비 (0세대)
1. `OUT` 폴더를 만든다.
2. 업로드: `mcp__pixellab__get_upload_url` 로 URL 을 받고 `curl -sS -F "file=@Docs/ArtCompare/Motion/equip/vamp/under_idle0.png" "<URL>"` 실행 → 응답의 `upload:<id>` 를 쓴다. base64 는 잘리므로 쓰지 않는다.
3. 결과 받기: `get_image(job_id)` 로 완료 확인 후 `curl -sS -o 파일 https://api.pixellab.ai/mcp/images/<job_id>/download` (zip 이면 풀어서 png 를 꺼낸다). 대기는 `wait_for_jobs` 를 쓴다.

## 2. 단계 A — 첫 프레임(의상 + 활공 자세를 한 번에), `edit_image_pro_flash` 5세대
- 설정: `image_url = upload:<id>`, `method = "text"`, `seed = 11`, `no_background = true`, `use_color_palette_correction = false`.
- description 원문(그대로):

```
Dress this exact character as an elegant vampire and put her in a floating glide pose, facing right in side view. Outfit: a long black cape with a tall stiff high collar and deep crimson red inner lining, a black tailcoat with small gold buttons over a white ruffled cravat, slim black trousers, small black pointed boots. Pose: the whole body hovers in the air with both feet clearly off the ground, legs held together and slightly bent back, toes pointing down, torso leaning slightly forward, both arms close to the sides of the body, the cape spreading and flowing backward behind her to the left as if pushed by wind, showing the red lining. Keep the exact same face, eyes, cream hair with side bun, goggles on the head, head size, body proportions, canvas position and pixel art style with clean dark outlines. No ground, no shadow, no effects.
```

- 저장: `OUT/a_first.png`(64x64 RGBA).
- **합격 기준(흰 바탕 5배로 확인, 3절 절차):** ① 얼굴·머리카락·고글이 입력과 같은 사람 ② 깃·붉은 안감·검은 망토·흰 크라바트가 보임 ③ 두 발이 땅에서 뜬 자세(발끝 아래로), 망토가 몸 뒤(왼쪽)로 흐름 ④ 떨어진 점·구멍·번짐 없음 ⑤ 몸 전체가 캔버스 안(잘림 없음), 망토 왼쪽 끝이 x≥1.
- **재시도(최대 1회, +5세대):** 위 ①~⑤ 중 하나라도 실패하면 description 은 그대로, seed 만 `12` 로 1회 다시 한다. 두 결과 중 나은 것을 고른다. 둘 다 ③만 실패(옷은 됐지만 서 있음)면, 옷이 나은 쪽 그림을 입력으로 아래 문구로 edit 1회(seed 11) 하는 것으로 재시도를 대신한다:
  `Keep the same character and outfit exactly. Change only the pose: she floats in the air with both feet off the ground, legs together slightly bent back, toes pointing down, torso leaning slightly forward, arms close to the body, cape flowing backward to the left showing the red lining. No ground, no shadow.`
- 픽셀 보정(0세대, 허용): 몸이 캔버스 아래에 붙어 있으면(가장 낮은 불투명 픽셀 y≥60) 그림 전체를 위로 3px 평행이동해 떠 있는 여백을 만든다. 그 외 리터치 금지.
- 2회 모두 기준 미달이면 단계 B로 가지 말고 중단, 보고한다.

## 3. 흰 바탕 5배 확인 절차 (매 단계 필수, 0세대)
python(PIL)으로:
1. 각 프레임을 흰 바탕(255,255,255)에 합성 → `NEAREST` 로 5배(320x320) 확대 → `OUT/check/<이름>_x5.png` 저장 후 **Read 로 한 장씩 눈으로 본다.** 축소본·스트립만 보고 판정 금지.
2. 부스러기 수치: 알파>0 픽셀 중 8이웃에 알파>0 이 하나도 없는 픽셀 수(고립점)와 좌표를 출력. 고립점 1개 이상이면 그 위치를 확대 그림에서 확인한다.
3. 루프 수치(단계 B 이후): 각 인접 프레임 쌍과 끝→처음 쌍의 알파 실루엣 XOR 비율(XOR 픽셀 수 / 합집합 픽셀 수).
4. 얼굴 일관성: 머리 영역(bbox 위쪽 22줄)의 프레임 간 XOR 비율도 출력.

## 4. 단계 B — 활공 모션, `animate_image_pixminimax` 1세대
- 입력: 단계 A 합격 그림을 다시 업로드한 `upload:<id>`.
- 설정: `frame_count = 8`, `seed = 7001`, `direction = "east"`, `view = "side"`, `no_background = true`, `enhance_prompt = false`, `last_frame_url` 없음.
- `subject_description`(그대로): `a small chibi pixel art vampire girl with cream hair and goggles, wearing a black high-collared cape with crimson lining and a black tailcoat`
- `initial_pose`(그대로): `hovering in the air facing right, feet off the ground with toes pointing down, legs together, arms at her sides, cape flowing backward`
- `description`(그대로):

```
Gliding forward smoothly through the air in place without walking, a seamless looping hover cycle. The legs stay together and do not step, toes keep pointing down. The whole body bobs gently up and down by about two pixels: rising during the first half and sinking back during the second half. The cape billows and ripples backward behind her in a smooth wave that travels from the collar to the hem, then settles back to its starting shape at the end. The arms stay close to the body. The head, face and hair stay steady. The final frame returns to the same pose as the first frame.
```

- 결과 9장(0 = 입력, 1~8 = 생성)을 `OUT/raw_0.png ~ raw_8.png` 로 저장.
- **루프 고르기(0세대):** 3절 수치로 8장 연속 구간을 고른다. 기본은 raw_0~7(끝 raw_7→raw_0). raw_1~8 이 이음새 XOR 이 더 작으면 그것을 쓴다. 이음새 XOR 이 인접 쌍 최대값의 1.5배 이하이면 루프 합격.
- **합격 기준:** ① 루프 합격 ② 모든 프레임에서 발이 뜨고 다리가 걸음을 떼지 않음(걷는 것처럼 보이면 불합격) ③ 망토가 뒤로 흐르며 물결, 안감 붉은색 유지, 망토 길이·깃 모양이 프레임마다 같은 옷 ④ 얼굴·머리 XOR 작고 눈으로 봐도 같은 얼굴 ⑤ 고립점 0, 구멍·번짐 없음 ⑥ 캔버스 잘림 없음 ⑦ 몸 전체가 좌우로 2px 넘게 흘러가지 않음.
- **재시도(최대 2회, 각 1세대) — 실패 원인별로 변수 하나만 바꾼다:**
  - 이음새가 튐(①만 실패) → 같은 설정에 `last_frame_url = 단계 A 그림 upload id` 추가, seed 7001 유지.
  - 다리가 걷거나 몸이 거의 안 움직임/망토가 안 흐름 → `last_frame_url` 없이 seed 만 `7002`.
  - 얼굴·옷이 무너짐(④⑤) → seed `7002`.
  - 두 번째 재시도도 같은 원인으로 실패하면 중단하고 가장 나은 결과를 제출하며 결함을 그대로 적는다.
- 픽셀 보정(0세대, 허용 범위만): 몸 전체가 좌우로 흘러가면 각 프레임을 머리 bbox 중심 x 가 첫 프레임과 같도록 정수 평행이동. 고립점 1~2개는 지워도 된다(지운 좌표를 보고에 적음). 모양 다시 그리기 금지.

## 5. 제출물 (`OUT` 안)
- `walk_0.png ~ walk_7.png`: 64x64, RGBA 투명 바탕, 고른 루프 순서대로.
- `walk.gif`: 흰 바탕 5배(320x320), 8프레임, 프레임당 100ms, 무한 반복.
- `walk_strip_x5.png`: 흰 바탕 5배 가로 스트립(검수용).
- `gen_log.txt`: 호출별 도구·seed·estimated_generations·누적 합계.
- `report.md`: 8줄 이내 — 쓴 세대 합계, 단계별 시도 수와 이유, 고른 루프 구간, 이음새·인접 XOR 수치, 고립점 수, 스스로 본 프레임별 결함, 합격 여부 자기 판정.

마지막 응답(감독에게)은 report.md 내용 그대로 8줄 이내.
