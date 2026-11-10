# 모션 연구 기록 (11-07~) — 올려베기 M1

규칙·절차: `.claude-rules/Game/MotionTheory.md` §8. 판정표: `Docs/Playtest/MotionJudgeLog.md`.

## 폴더
- `m1_a/` 원본 생성(f0 = 입력, f1~f4 생성). `m1_a_clean/` 이펙트 제거 후 **합격본**. `m1_b/` 이펙트 단어 뺀 문장(동작 약함). `m1_c/` 이펙트 금지 문구 강화(재발). `specs/m1_a_erase.json` 이펙트 제거 설정(다시 돌리면 같은 결과).

## M1-a 합격 생성 호출(그대로 재사용)
- 도구 `animate_image_pixminimax`, `frame_count` 4, `direction` east, `view` side, `seed` 7001.
- `first_frame_url` = `Docs/ArtCompare/CharacterGen/dir8_base/d3_side_r.png` 업로드(우리 기준 옆모습, 랜턴·무기 없음).
- `subject_description`: "a chibi girl explorer with ash-blonde hair, goggles, navy-teal coat and a large brown backpack"
- `initial_pose`: "standing in a low crouch facing right, arms relaxed"
- `description`: "performs a rising upward slash with bare fists: starts crouched low with the right arm pulled back beside the hip, then the whole body rises and the right arm sweeps from low behind up through the front to overhead in one curved arc, back leg pushes, weight moves to the front foot, torso stretches upward. Always faces right, never turns around or toward the camera, same size, same spot, backpack attached to the body, holding nothing"
- 결과: 4프레임, 3·4번 칸에 흰 호 이펙트가 같이 그려짐 → `erase_fx.py` 로 제거.

## 이펙트 제거 순서
1. 이펙트색 파악(흰 248,248,248 / 크림 / 회색). 머리카락도 크림색이라 색만으로 지우면 안 됨.
2. 칸마다 문자 지도(64×64)로 이펙트 위치를 보고 지울 영역(zone)을 정함.
3. 호가 몸을 덮은 칸은 인접 칸의 같은 줄을 `patch_from` 으로 복사, 모자란 픽셀은 `paint`.
4. 8배 확대로 확인 후 GIF.

## 실패·교훈 요약
- 문구로 이펙트를 막을 수 없다(도움말 확인). "slash/arc" 를 빼면 동작이 약해진다(M1-b).
- 골격 방식은 졸라맨 → 자세 부자연(사용자 판정). 문장 방식이 몸 연속성이 좋다.
- 프레임별 다른 방향 기준 그림에서 따로 생성하면 안 이어진다.

## M4-a 한 바퀴 회전 베기 (11-07 합격) — 정확한 호출
- 도구 `animate_image_pixminimax`, `frame_count` 8, `direction` east, `view` side, `seed` 7001, **`last_frame` 없음**.
- `first_frame`: `Docs/ArtCompare/CharacterGen/ds01/motions_bare/up_m1a/raw_1.png`(웅크린 준비 자세, 업로드 → `upload:<id>`).
- `subject_description`: "a chibi girl explorer with ash-blonde hair, goggles, navy-teal coat and a large brown backpack"
- `initial_pose`: "in a low crouch facing right with both fists in front, knees deeply bent"
- `description`: "the girl turns her body one full turn in place: she pivots on her left foot while her right foot steps around, her torso twists and her right arm swings from far behind her around to far out in front at waist height with a bare fist, the left arm pulled in, showing her back in the middle of the turn, and she ends in a wide low lunge facing right with the right arm stretched forward, bare fists holding nothing, no effects, no motion lines, no white arcs"
- 결과: 입력 + 8칸. 5·6번 칸에 흰 호가 그려짐 → `specs/m4_a_erase.json` 로 지움(6번 칸은 7번 칸 줄 보충). 곡도·얼굴 붕괴 없음.
- 같은 입력으로 끝 칸을 고정한 M3-c 는 팔·다리가 얼어붙고 곡도가 그려져 실패 → 고정 금지.
