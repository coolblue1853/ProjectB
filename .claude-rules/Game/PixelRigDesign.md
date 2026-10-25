# 픽셀 리그 설계 — 새 주인공(기본 모험가) 애니메이션

> 결정(사용자 2026-10-25): **Unity 리깅 + 픽셀 격자 렌더**로 간다(셀 애니메이션 아님). "기본 모험가부터 만들어서 넣어 줘."
> 이유: 장비를 바꾸면 외형이 바뀌어야 하는데, 셀 애니는 애니 수 × 프레임 × 방어구 세트만큼 그림이 곱해진다. 리그는 부위 그림만 바꾸면 되고, 애니 키프레임을 Claude가 데이터·스크립트로 만들고 고칠 수 있다(AnyPortrait는 GUI 전용 포맷이라 협업이 어렵다). 셀은 이펙트와 특수 동작에만 쓴다.

## 1. 구조

- **아트:** `Docs/ArtCompare/CharacterGen/hero_adventurer/clean_5102.png`(CharacterPixelRules 0절 레시피, 64×64, 키 47px, 20색). 원본은 왼쪽을 보므로 **좌우 반전**해 오른쪽을 보게 한다(플레이어 루트 scale.x +1.5 = 오른쪽).
- **부위 분할:** `Tools/pixelfx/split_rig_parts.py` → 부위별 64×64 PNG(나머지 투명) + 피벗 JSON. 부위: 배낭·몸통·머리·왼손·오른손·왼다리·오른다리. 팔(소매)은 몸통에 남기고 손만 분리한다(손 3~4px라 소매째 돌리면 뭉개진다).
- **Unity:** 뼈 = 빈 Transform 계층, 부위 = SpriteRenderer. 스프라이트는 64×64 캔버스 그대로, 피벗 = 그 부위의 관절점이라 정지 자세가 원본 그림과 픽셀 단위로 같다.
  - 계층: 루트(`PixelRig`) → `LegB`, `LegF`, `Hips`(몸통) → `Pack`, `Head`, `ShoulderB`→`HandB`(+`LeftWeapon`), `ShoulderF`→`HandF`(+`RightWeapon`). F = 앞(바라보는 쪽), B = 뒤.
  - 그리기 순서(뒤→앞): 배낭 < 뒷손 < 다리 < 몸통 < 머리 < 앞손 < 무기.
- **크기:** 스프라이트 PPU 36 × 루트 스케일 1.5 → 아트 1px = 1/24유닛 = 몹(24 PPU)과 같은 도트 크기, 화면 격자(PixelLook 48)의 정수 2칸. 키 약 2.0유닛(기존 AnyPortrait 약 1.6). → `Docs/OpenQuestions.md` Q(임시 가정: 2.0 유지).
- **픽셀 격자:** 뼈 위치는 매 프레임 아트 픽셀(1/36 로컬)에 스냅한다. 회전은 작게(±40° 이내) 쓰고, 화면 전체는 기존 URP Pixel Perfect(PixelLook)가 격자로 정리한다.

## 2. 애니메이션

- 클립 = Unity `AnimationClip`(.anim) 에셋, 뼈 Transform의 위치·회전 커브. 에디터 스크립트 `Assets/Editor/Dev/PixelRigBuilder.cs`가 키프레임 표에서 생성한다(값을 바꾸면 다시 굽는다). Animation 창에서 손으로 고쳐도 된다.
- 재생: `PixelRig`(런타임)가 `clip.SampleAnimation`으로 직접 샘플 — Play / CrossFade(짧은 블렌드 생략) / IsPlaying / 속도.
- 기본 클립: Idle, Walk, Run, Jump, Dash, Rope(Up/Down/Stay), 공격 5종(Stab·SwingDown·SwingUp·SwingSide·Cast).
- 기존 애니 이름(평타 13·스킬 약 23)은 이름 규칙으로 기본 클립에 매핑한다(`PixelRig.MapName`): Stab/Straight/Punch→Stab, Up/upper→SwingUp, Side/Wide/Vesh/Spin→SwingSide, Staff/Buff/Shield/Blow/Spead→Cast, 그 외→SwingDown. 고유 동작이 필요한 스킬은 나중에 클립을 추가하고 매핑 표에 넣는다.

## 3. 코드 연결

- `CharacterRig`(추상) — 플레이어 코드가 쓰는 API: Play, CrossFade, IsPlaying, SetAnimationSpeed(전체/클립), SetMeshAlphaAll, SetControlParamInt, GetBoneSocket, SetMeshImage.
  - `PortraitRig` = apPortrait 래퍼(기존 동작 그대로), `PixelRig` = 새 리그.
- `PlayerController.Rig`가 활성 리그를 돌려준다(자식에 켜진 PixelRig가 있으면 그것, 없으면 apPortrait 래퍼). `mainCharacter`(apPortrait) 필드는 씬 직렬화 때문에 남긴다.
- 픽셀 리그에서 아직 안 되는 것: 방어구 외형 교체(SetMeshImage는 무시 — CharacterPixelRules 6절 레이어 설계 후), 눈 표정 파라미터(앞머리로 가려짐).

## 4. 이력
- 10-25: 결정, 기본 모험가 5102 선택, 설계 작성.
- 10-25: 구현·인게임 적용.
  - 부위 7개(재합성 차이 0px), 뼈 9개, 클립 12개.
  - **앞팔은 손목 채널 추가**: 기존 무기 그림은 "손목 0 = 칼날 위" 기준이라 팔만 돌리면 찌르기 때 칼날이 뒤를 향했다. 클립 표는 `Arm(시간, 팔 각도, 칼날 각도)`로 적고 손목 = 칼날 − 90 − 팔.
  - 무기 소켓 배율 2.4(예전 3).
  - 무기 해제 시 픽셀 리그 소켓도 비운다(`AttackManager.ClearSocket`).
  - 대시 잔상이 SpriteRenderer 부위도 복사한다.
  - 검증 ①: 편집 모드 클립 시트 `Tools/probes/rig_clip_sheet.cs`(행 = 클립, 열 = 6시점, 무기 지정).
  - 검증 ②: 플레이 확대 캡처 `rig_closeup.cs`, 대시 잔상 7조각, 플레이 콘솔 에러 0(경고는 리그를 끈 상태와 같음).
  - ⚠ Unity 창이 최소화돼 있으면 게임 시간이 느려 실시간 연속 캡처로는 동작이 안 보인다 → 클립 시트로 본다.
  - 남은 일: TechDebt B-213~215, Q-421(키 2.0유닛).
