# 타격감(Game Feel) 구현 계획 — 로드맵 A2~A6

> 상태: 설계안(2026-10-18). 코드 미작성·미검증이다. 구현하면 항목마다 ✅와 커밋 해시를 `Game/QualityRoadmap.md`에 적는다.
> 수치는 웹 자료와 현재 카메라 설정(직교 5.47, PPU 100 → 1080p에서 1유닛 ≈ 99px)을 근거로 잡은 **초기값**이다. 플레이테스트로 조정한다.

## 0. 결론 요약

| 항목 | 수단 | 새 에셋 |
|---|---|---|
| A2 피격 플래시 + 히트스톱 | All In 1 Sprite Shader `HITEFFECT_ON` + **국소 히트스톱**(Animator.speed=0, AnyPortrait 속도 0) | All In 1 Sprite Shader |
| A3 카메라 흔들림 세분화 | 기존 **ProCamera2D** `Shake(duration, strength, …)` + 스로틀 | 없음 |
| A4 사망 디졸브 + 드롭 등급 연출 | All In 1 `FADE_ON`(시체 파츠), `OUTBASE_ON`+`GLOW_ON`(R/U 드롭) + 빛 기둥 파티클 | All In 1 Sprite Shader |
| A5 치명타 숫자 분리 | DamageNumbersPro 프리팹 분리(Normal/Crit/Extra/DoT) + 내장 Style 프리셋 | 없음 |
| A6 스킬·타격 이펙트 | 히트 스파크 + 스킬 VFX, 풀링 | Magic Slashes FX / Particle Pack (**캐시에 없음 → 다운로드 필요**) |

- **Feel은 1단계에서 도입하지 않는다.** 이유는 3절. 필요하면 2단계 선택 사항으로 최소 폴더만 넣는다.
- 모든 연출은 프로젝트 측 파사드 **`HitFeedback`** 한 곳에서 낸다. 무기·스킬마다 `HitFeedbackProfile`(ScriptableObject)로 강도를 데이터화한다.

## 1. 캐시된 패키지 (확인 완료)

`C:\Users\user\AppData\Roaming\Unity\Asset Store-5.x\` 아래:

| 패키지 | 경로 | 크기 |
|---|---|---|
| All In 1 Sprite Shader | `Seaside Studios/Shaders/All In 1 Sprite Shader.unitypackage` | 17 MB |
| Feel v5.9.1 | `More Mountains/ScriptingEffects/Feel.unitypackage` | 316 MB |
| (참고) DOTween Pro, Easy Save, AnyPortrait, Odin | 각 퍼블리셔 폴더 | |

- **Magic Slashes FX, Particle Pack은 캐시에 없다.** Package Manager(My Assets)에서 다운로드가 필요하다 → `Docs/OpenQuestions.md`에 등록하고 A6만 보류한다.

## 2. 아키텍처 — `HitFeedback` 파사드

### 2.1 파일 (신규, `Assets/Script/Combat/Feedback/`)

| 파일 | 역할 |
|---|---|
| `HitFeedbackProfile.cs` | ScriptableObject. 한 종류 타격의 연출 수치 묶음(아래 표) |
| `HitFeedback.cs` | 정적 파사드 + 씬 싱글턴(`HitFeedbackRunner`). 스로틀·예산 관리, 카메라·숫자·VFX·SFX 호출 |
| `SpriteHitFx.cs` | 적·시체·드롭에 붙는 컴포넌트. 머티리얼 인스턴스 캐시, `Flash()`, `Dissolve()`, `SetRarity()` |
| `HitStopLocal.cs` | 대상별 국소 히트스톱(Animator / apPortrait / Rigidbody2D 정지·복원) |
| `HitContext` (struct) | `target, point, attacker, isCrit, isSkill, killed, damage, extraDamage, profile` |

### 2.2 `HitFeedbackProfile` 필드와 기본 에셋

| 필드 | 평타 `HFP_Basic` | 스킬 `HFP_Skill` | 다단/투사체 `HFP_Multi` | 치명 배수 |
|---|---|---|---|---|
| flashDuration (s) | 0.06 | 0.08 | 0.05 | +0.02 |
| flashGlow (`_HitEffectGlow`) | 1 | 1.5 | 1 | 3 (블룸에 살짝 걸림) |
| targetHitStop (s) | 0.04 | 0.06 | 0.02 | +0.03 |
| attackerHitStop (s) | 0.03 | 0.04 | 0 | +0.02 |
| shakeTier | Light | Medium | None(첫 타만 Light) | Heavy로 승급 |
| hitVfx | `VFX_HitSpark_S` | 스킬별 | `VFX_HitSpark_S` | `VFX_HitSpark_Crit` 추가 |
| sfxKey | 기존 `damageSound` 유지 | | | `Hit_Crit` 추가 |

- 무기(`Weapon.attackPrefab[]`)와 스킬(`Skill` 생성 프리팹)의 `DamageObject`에 `public HitFeedbackProfile feedback;` 필드를 둔다. 비어 있으면 `isSkill`로 Basic/Skill 기본값을 쓴다.
- 기존 `DamageObject.ShakeTime`(인스펙터 값)은 **적 경직 흔들림 시간**으로 이미 쓰이고 있다. 프로필 도입 후에도 유지하고, `targetHitStop`은 `max(ShakeTime, profile.targetHitStop)`을 0.03~0.12로 클램프해서 쓴다.

### 2.3 예산·스로틀 (많이 때리는 파밍 게임이라 필수)

| 규칙 | 값 | 이유 |
|---|---|---|
| 카메라 흔들림 | 0.10s 창 안에서 **가장 센 1회만** | 광역 스킬이 10마리를 때려도 1번만 흔든다 |
| 공격자 히트스톱 | `DamageObject` 활성 1회당 **첫 타격 1번만** | 다단히트가 손을 계속 멈추게 하지 않는다 |
| 대상 히트스톱 | 대상별 진행 중이면 `max`로 갱신(누적 금지) | 멈춤이 쌓여 굳는 현상 방지 |
| 히트 VFX | 프레임당 최대 6개 | 파티클 폭주 방지 |
| 치명 숫자 | 프레임당 최대 4개, 나머지는 일반 숫자 | 화면 가독성 |
| 전역 옵션 | `shakeScale`(0~1), `flashEnabled` | 접근성(흔들림 민감 사용자). 기본 1 |

### 2.4 호출 흐름

```
DamageObject.OnTrigger… (DamageObject.cs ≈450, ≈467)
  └ var r = enemyHealth.damage2Enemy(...)          ← 반환형 void → DamageCalculator 결과(struct)로 변경
  └ HitFeedback.OnEnemyHit(new HitContext{ target, point = collision.ClosestPoint(transform.position),
                                           attacker = player, isCrit = r.isCrit, killed = r.killed, profile = feedback })
EnemyHealth.damage2Enemy (EnemyHealth.cs:143)
  └ 숫자 스폰은 HitFeedback.SpawnNumber(kind, pos, value)로 위임 (161~172행)
  └ 경직 시퀀스의 shakeTime 구간에 HitStopLocal 적용 (205~216행)
PlayerHealthManager.damage2Player (≈203행) → HitFeedback.OnPlayerHit(dmg)
EnemySpawner.OnEnemyReleased (233행) → DeadBody → SpriteHitFx.Dissolve
DropManager.DropItems (≈151행) → DropItemCheck.SetItem 뒤 SpriteHitFx.SetRarity(itemData.rarity)
```

- `damage2Enemy` 반환형 변경은 기존 호출부(`EnemyDamageObject.cs:225,256`, `Tools/probes/hit_one.cs`)가 반환값을 무시하므로 컴파일이 깨지지 않는다.
- `killed`는 `TryDie()` 결과로 채운다(사망 시 넉백을 건너뛰는 현재 흐름 유지).

## 3. 에셋 도입 판단과 임포트 목록

### 3.1 All In 1 Sprite Shader — **도입**

- 셰이더: **`AllIn1SpriteShader/AllIn1Urp2dRenderer`** (`Shaders/AllIn1Urp2dRenderer.shader`). URP 2D Renderer 전용이며 2D 라이트(Shape Light 0~3)를 받는다. 현재 적은 `Sprite-Lit-Default`를 쓰므로 밤 조명을 유지하려면 이 셰이더가 맞다. (`AllIn1SpriteShader`/`…SRPBatch`는 2D 라이트를 안 받는다.)
- 속성은 `CBUFFER_START(UnityPerMaterial)`에 있다 → **SRP Batcher 호환**.
- 키워드는 전부 `shader_feature_local`이다 → **빌드에 쓰일 키워드는 머티리얼 에셋에 미리 켜 둬야 한다**(런타임에 켜면 빌드에서 스트립된 변형이라 동작 안 함).

| 용도 | 키워드 | 속성 |
|---|---|---|
| 피격 플래시 | `HITEFFECT_ON` | `_HitEffectBlend`(0~1, 0=꺼짐), `_HitEffectColor`, `_HitEffectGlow`(1~100) |
| 디졸브 | `FADE_ON` | `_FadeAmount`(-0.1=온전 → 1=소멸), `_FadeTex`, `_FadeBurnColor`, `_FadeBurnWidth`, `_FadeBurnGlow` |
| 아웃라인 | `OUTBASE_ON` + **`OUTBASEPIXELPERF_ON`**(도트용 픽셀 단위) | `_OutlineColor`, `_OutlineAlpha`, `_OutlineGlow`, `_OutlinePixelWidth` |
| 글로우 | `GLOW_ON` (+`GLOWTEX_ON` 선택) | `_GlowColor`, `_Glow`, `_GlowGlobal` |
| 공통 | — | `_Alpha`(DOFade 대신 사용 가능) |

**임포트:** 패키지는 `Assets/Plugins/AllIn1SpriteShader/`로 풀린다. 그대로 두거나 `AssetDatabase.MoveAsset`으로 `Assets/ThirdParty/AllIn1SpriteShader`로 옮긴다(GUID 유지, 에디터 스크립트에 하드코딩 경로 없음 확인).

| 포함 | 제외(임포트 후 삭제, 커밋 금지) |
|---|---|
| `Shaders/`(54), `Scripts/`(24), `Materials/`(4), `Textures/`(32, 디졸브 노이즈 `seamlessNoise.png`·`flameNoise.png` 필요), `AllIn1SpriteShaderAssembly.asmdef`, `Documentation.pdf` | `Demo/`(137개), `2DRendererDemo.unitypackage` |

**프로젝트 머티리얼 (`Assets/Art/Materials/FX/`):**

| 머티리얼 | 켜 둘 키워드 | 기본값 |
|---|---|---|
| `M_EnemySprite` | `HITEFFECT_ON`, `FADE_ON` | Blend 0, Fade -0.1, BurnColor (1,0.55,0.2), BurnWidth 0.03, BurnGlow 4, FadeTex=seamlessNoise |
| `M_DeadBody` | `HITEFFECT_ON`, `FADE_ON` | 위와 같음 |
| `M_Drop_C` | 없음(=`Sprite-Lit-Default` 그대로) | |
| `M_Drop_R` | `OUTBASE_ON`, `OUTBASEPIXELPERF_ON`, `GLOW_ON` | Outline (0.35,0.6,1), PixelWidth 1, OutlineGlow 2, Glow 0 |
| `M_Drop_U` | 위와 같음 | Outline (1,0.78,0.25), OutlineGlow 4, Glow 1.5 |

### 3.2 Feel (More Mountains) — **1단계 미도입, 2단계 선택**

미도입 이유:
1. 카메라 흔들림은 **ProCamera2D Shake**가 이미 있다. `MMF_CameraShake`는 `MMCameraShaker`를 카메라에 요구해 ProCamera2D와 위치 제어가 충돌한다.
2. `MMF_FreezeFrame`은 `Time.timeScale`을 내리는 방식(+`MMTimeManager` 자동 생성)이다. 로드맵 A2 결정("Time 스케일 대신 애니 속도")과 맞지 않는다. 일반 타격은 국소 히트스톱이 맞다.
3. 스쿼시·스케일은 **DOTween Pro**로 충분하다(`DOPunchScale`).
4. 316MB이고, `MoreMountains.Tools.asmdef`가 `Lofelt.NiceVibrations`(GUID 57a0b9…)를 참조하므로 최소 도입도 NiceVibrations 런타임을 함께 넣어야 한다.

2단계(디자이너가 MMF_Player로 스킬별 연출을 짜고 싶을 때)의 **최소 폴더**:

| 포함 (`Assets/Feel/…` → `Assets/ThirdParty/Feel/…`로 이동) | 제외 |
|---|---|
| `MMFeedbacks/MMFeedbacks`(Core·Feedbacks·Shakers·Springs·Sequencing·MMFloatingText) | `FeelDemos/`, `FeelDemosURP/`, `FeelDemosHDRP/` |
| `MMFeedbacks/Editor`, `MMFeedbacks/Authorizations` | `MMFeedbacks/Demos`, `MMTools/Demos` |
| `MMFeedbacks/MMFeedbacksForThirdParty/URP`(`MMF_Light2D_URP` 등), `…/MMTools`, `…/NiceVibrations` | `…ForThirdParty/HDRP, PostProcessing, Cinemachine, VisualEffectGraph, UIToolkit, TextMeshPro` |
| `MMTools/Core`(asmdef), `MMTools/Foundation`, `MMTools/Accessories`(MMPrototypeTextures·MMDebugMenu는 삭제 가능) | `NiceVibrations/Demo`, `HapticSamples`, `OlderVersions` |
| `NiceVibrations/Lofelt.NiceVibrations.asmdef`, `Scripts`, `Plugins`, `Helpers`, `Define` | |

- 모든 피드백 스크립트는 asmref로 **단일 어셈블리 `MoreMountains.Tools`**에 컴파일된다. 서드파티 폴더는 `MM_URP` 등 versionDefine으로 가드되어 있어 빠져도 안전하다.
- 쓸 만한 피드백: `MMF_Flicker`(MPB 옵션 있음, 기본 `_Color`라 All In 1에는 `MMF_MaterialSetProperty`로 `_HitEffectBlend` 제어), `MMF_SquashAndStretch`, `MMF_ScaleShake`, `MMF_ParticlesInstantiation`(풀 지원), `MMF_Light2D_URP`(치명타 순간 조명). 카메라는 계속 ProCamera2D를 쓴다(`MMF_Events`로 `HitFeedback` 호출).
- `ImportPackage(path,false)`는 선택 임포트가 안 된다. 전부 임포트 → 제외 폴더 `AssetDatabase.DeleteAssets` → 이동 → 컴파일 확인 순서로 한다.

### 3.3 기존 에셋

- **ProCamera2D:** `ProCamera2DShake.Instance.Shake(float duration, Vector2 strength, int vibrato, float randomness, float initialAngle, Vector3 rotation, float smoothness, bool ignoreTimeScale)`. 현재는 BT의 `EnemyCameraShake`(프리셋 이름 `PT_Shake`, 2s·강도 1)만 쓰고, **플레이어 타격에는 흔들림이 없다.** 기본 프리셋 `PlayerHit.asset`(0.04s, 강도 0.5)이 있다.
- **DamageNumbersPro:** 내장 프리셋은 `Editor/Resources/DNP/`의 Style(Basic, Blood, Cool, Fire, Neon, **Pixel**, Saw, Smear), Fade In(Bounce Up, **Stamp**, **Shake**, Scale…), Fade Out, Behaviour(**Combine**, Push Up, Spread Up…). 치명타 전용 프리셋은 없다 → 프리팹을 분리해 조합한다. 현재 숫자 프리팹은 `Assets/Prefab/Enemy/EnemyDamge.prefab`(shaking·scaleByNumber 켜짐, **pooling 꺼짐**).

## 4. A2 — 피격 플래시 + 히트스톱

**적 플래시** (`SpriteHitFx.Flash(duration, glow)`):
- `_HitEffectBlend`를 1로 올렸다가 duration 뒤 0으로 **즉시** 내린다(도트는 페이드보다 하드 컷이 선명하다). 색은 흰색.
- 기간 0.06s(≈60fps 4프레임). 웹 자료의 플래시 0.08s 권장보다 약간 짧게 잡았다. 메이플식으로 초당 타격 수가 많아서 길면 적이 계속 흰색으로 보인다.
- 보스(`isBossMob`)도 플래시는 하되 Blend를 0.6으로 낮춘다(현재 보스는 HP바 토글을 건너뛰는 분기만 있음).
- 훅: `EnemyHealth.damage2Enemy` 153~157행의 `ToggleObject()` 옆. `onlyDamage2Enemy`(독·출혈 틱)는 플래시 대신 Blend 0.3, 0.04s의 약한 색 펄스(독 초록, 출혈 빨강)만 준다.
- 참고: 현재 `ToggleObject`는 **플래시가 아니라 HP 캔버스 2초 표시**다. 이름을 바꾸지는 않는다(F3에서 처리).

**적 히트스톱(국소)** (`HitStopLocal.Apply(seconds)`):
- `Animator.speed = 0`, `Rigidbody2D` 속도 저장 후 0 → 시간 뒤 복원. 시간은 `Time.unscaledDeltaTime`이 아니라 일반 시간으로 잰다(전역 timeScale을 건드리지 않으므로).
- 현재 경직 시퀀스가 이미 "흔들림(shakeTime) → 넉백 → 경직" 순서다. **shakeTime 구간이 곧 히트스톱 구간**이므로 그 구간에 애니만 멈추면 된다(205~216행 `AppendCallback` 안).
- 슈퍼아머(167행)·사망 시에도 짧게(0.03s) 적용한다. `OnDisable`에서 반드시 `anim.speed = 1` 복원(풀 재사용 시 굳은 개체 방지).

**공격자 히트스톱:**
- 플레이어는 AnyPortrait다: `apPortrait.SetAnimationSpeed(0f)` → 시간 뒤 `SetAnimationSpeed(1f × 공속 배율)`. 현재 공속을 애니 속도로 쓰고 있다면 원래 값을 저장해 복원한다(구현 시 `Weapon.cs` 확인).
- 공격 판정 타이밍(`Weapon`의 DOTween 대기)은 멈추지 않는다. 0.03~0.06s라 체감 차이는 없다. 입력 버퍼(B3) 도입 후 다시 본다.
- 무적·이동정지 토큰화(B2)와 충돌하지 않도록 히트스톱은 **애니 속도만** 만진다.

**전역 프리즈(예외):** 보스·엘리트 처치 때만 `Time.timeScale = 0.05`로 0.10s(실시간, `WaitForSecondsRealtime`). 일시정지 UI가 생기면 복원 전에 일시정지 상태를 확인한다. 현재 프로젝트에는 `timeScale`을 쓰는 코드가 없다.

**플레이어 피격:** `PlayerHealthManager.damage2Player`에서 `mainCharacter.SetMeshColorAll(…)`로 0.08s 붉은 틴트. AnyPortrait 색은 **2X 규약**(0.5 회색 = 원색)이므로 (1.0, 0.45, 0.45)로 준다. 그 뒤 기존 알파 깜빡임(0.85↔1, 0.1s 주기)을 그대로 둔다. 카메라는 A3의 PlayerHit.

**근거:** 히트스톱 약 0~0.03s / 강 0.05~0.08s / 치명 0.1~0.15s, 플래시 0.08s가 일반 권장이다. 여기서는 전역이 아닌 국소 정지라 체감이 약하므로 하한 쪽을 쓰되 치명·처치에서 확실히 올린다.

## 5. A3 — 카메라 흔들림 세분화

`HitFeedback.Shake(tier)` → `ProCamera2DShake.Instance.Shake(duration, strength, vibrato, randomness:0.3, initialAngle:-1, rotation:Vector3.zero, smoothness:0.05)`.

| 티어 | 사용처 | duration | strength(유닛) | ≈px@1080p | vibrato |
|---|---|---|---|---|---|
| Light | 평타 적중 | 0.06 | (0.04, 0.03) | 4 | 2 |
| Medium | 스킬 적중 | 0.10 | (0.09, 0.07) | 9 | 4 |
| Heavy | 치명타, 일반몹 처치 | 0.14 | (0.16, 0.12) | 16 | 5 |
| Impact | 보스 처치, 보스 대형 패턴 | 0.25 | (0.30, 0.25) | 30 | 7 |
| PlayerHit | 플레이어 피격 | 0.12 | (0.10, 0.10) | 10 | 3 |

- 0.10s 창 안에서 가장 높은 티어 1회만 실행(2.3절). 진행 중인 흔들림보다 낮은 티어는 버린다.
- 수평 성분을 약간 크게 둔다(횡스크롤에서 타격 방향감). 치명타는 `initialAngle`을 타격 방향(0° 또는 180°)으로 줘 밀리는 느낌을 낸다.
- 모든 strength에 옵션 `shakeScale`을 곱한다. 웹 자료 공통 경고: 짧게(50~300ms), 감쇠, 과하면 멀미.
- 기존 `EnemyCameraShake`(BT, `PT_Shake` 2s)는 보스 연출이라 손대지 않는다.
- 픽셀 퍼펙트 카메라(Pixel Perfect Camera)를 켜는 경우 4px 미만 흔들림은 스냅되어 사라질 수 있다 → Light 티어는 그때 0.05로 올린다.

## 6. A4 — 사망 디졸브 + 드롭 등급 연출

**사망 흐름(현재):** `TryDie` → `ReleaseEnemy`가 적을 즉시 풀로 반환 → `EnemySpawner.OnEnemyReleased`가 `DeadBody`를 꺼내 `Force2DeadBody(force)` → 파츠가 물리로 흩어짐 → 2초 뒤 `ResetParts`로 반환.

**디졸브:**
- `DeadBody` 프리팹의 파츠 SpriteRenderer를 `M_DeadBody`로 바꾼다. 시체 루트에 `SpriteHitFx`(여러 렌더러 지원)를 붙인다.
- 타임라인: 0~0.06s 흰 플래시(처치 순간 강조) → 1.2s부터 0.6s 동안 `_FadeAmount` -0.1→1(DOTween `DOVirtual.Float`) → 2.0s `ResetParts`. 디졸브 끝이 반환보다 0.2s 먼저 끝나야 깜빡 사라지는 프레임이 없다.
- `ResetParts`/`OnDisable`에서 `_FadeAmount = -0.1`, `_HitEffectBlend = 0`으로 되돌린다(풀 재사용).
- 훅: `DeadBody.Force2DeadBody`(시작), `DeadBody.ResetParts`(정리). 처치 피드백(Heavy 흔들림, 처치 히트스톱 0.08s)은 `HitFeedback.OnEnemyHit`의 `killed` 분기에서 낸다.
- `EnemyHealth.DisaperByTime`(시간대 변경 시 1.5s DOFade)은 같은 머티리얼의 `_FadeAmount`로 바꿀 수 있다(선택, 연출 통일).

**드롭 등급(`ItemData.rarity`: "C"/"R"/"U"):**

| 등급 | 머티리얼 | 빛 기둥 | 추가 |
|---|---|---|---|
| C | 기본 | 없음 | |
| R | `M_Drop_R`(파랑 1px 아웃라인) | 파랑, 높이 1.5유닛, 알파 0.35 | |
| U | `M_Drop_U`(금색 아웃라인 + 글로우) | 금색, 높이 2.5유닛, 알파 0.5, 위로 흐르는 입자 8개/s | 착지 시 SFX + Light 흔들림, D6 픽업 로그 강조 |

- 훅: `DropItemCheck.SetItem`(itemData 로드 직후)에서 `SetRarity(itemData.rarity)`. `OnDisable`에서 C 상태로 리셋(같은 `DropItem.prefab`을 풀로 재사용하므로 필수).
- 빛 기둥: `DropItem.prefab`의 자식으로 **비활성 ParticleSystem 1개**(가산 블렌드 세로 스트레치 빌보드, 색만 등급별로 교체). 착지(`GroundCheckOn`) 후 켠다. 공중에서는 꺼 둔다.
- 글로우는 씬 볼륨(`Assets/Scenes/SampleScene/Volum Profile.asset`)의 Bloom과 URP HDR(켜짐)에 의존한다. Bloom threshold가 1보다 높으면 `_OutlineGlow`를 올린다.

## 7. A5 — 치명타 데미지 숫자 분리

현재: `EnemyHealth.damage2Enemy` 161~162행에서 치명이면 `Spawn(pos, finDmg + "!")`처럼 **문자열**로 낸다 → 숫자 서식·scaleByNumber가 꺼지고 스타일 구분이 없다.

| 프리팹(`Assets/Prefab/Enemy/DamageNumbers/`) | 기반 프리셋 | 크기 | 색 | 동작 |
|---|---|---|---|---|
| `DN_Normal` (기존 `EnemyDamge` 복제) | Style **Pixel**, Fade In Scale | 1.0 | 흰~연노랑 | Push Up, lifetime 0.8 |
| `DN_Crit` | Style **Fire**, Fade In **Stamp** | 1.45 | 주황→빨강 그라디언트 | Shaking 켬(0.05, 0.15s), 위쪽 텍스트 "CRITICAL" 0.5배, lifetime 1.0 |
| `DN_Extra` (추가타) | Style **Cool** | 0.8 | 하늘색 | 본 숫자보다 +0.25 위, 0.05s 지연 |
| `DN_Poison` / `DN_Bleed` | Basic / **Blood** | 0.7 | 초록 / 진홍 | Behaviour **Combine**(0.3s 안의 틱 합산) |
| `DN_Player` (플레이어 피격) | Basic | 1.0 | 빨강 | |

- 모든 프리팹 **`enablePooling` 켬**(Normal 60, Crit 20, 나머지 20). DNP는 자체 풀을 쓰므로 `Spawn` 호출만 바꾸면 된다.
- 호출은 `HitFeedback.SpawnNumber(kind, pos, value)` 한 곳으로 모은다. `BleedingAttack`/`PosionAttack`의 `damageNumber` 필드도 이 경로로 바꾼다.
- 겹침: 같은 대상에 0.1s 안에 여러 숫자가 나오면 x를 ±0.15 지터, y를 0.2씩 스택(Behaviour Push Up).
- 폰트: 지금은 프리팹 기본 폰트. Thaleah 픽셀 폰트(미임포트)를 쓰면 TMP 아틀라스를 Raster(포인트 필터)로 만든다. 숫자·영문만이라 한글 문제 없음.
- DNP 내장 프리셋은 에디터 전용 리소스(`Editor/Resources`)라 **인스펙터에서 적용 후 프리팹에 굽는다.** 런타임 참조 금지.

## 8. A6 — 스킬·타격 이펙트

- **선행:** Magic Slashes FX / Particle Pack 다운로드(캐시에 없음). 받기 전까지는 Unity 기본 ParticleSystem으로 `VFX_HitSpark_S`(흰 점 6~8개, 0.15s, 가산)와 `VFX_HitSpark_Crit`(주황 12개 + 링 1개)를 만들어 A2~A5를 먼저 끝낸다.
- 받은 뒤: 필요한 프리팹·텍스처만 `Assets/ThirdParty/MagicSlashesFX/`로. 데모 씬·HDRP/Built-in 머티리얼은 제외한다. 머티리얼은 **URP `Universal Render Pipeline/Particles/Unlit`**(가산)로 변환하고, 렌더러 Sorting Layer를 캐릭터 위 FX 레이어로 맞춘다. 3D 파티클이므로 Z 회전 0, 스케일 0.3~0.6, `Simulation Space=World`.
- 연결: `HitFeedbackProfile.hitVfx`(적중 지점), 스킬별 시전 이펙트는 `Skill`의 기존 지연 이펙트/프리팹 슬롯에 넣는다. 평타 슬래시는 `Weapon.attackPrefab[]`의 자식 이펙트로.
- 풀링: 기존 `PoolAble` + `UnityEngine.Pool.ObjectPool` 패턴을 쓴다. 파티클은 `Stop Action = Callback`에서 `ReleaseObject()`.
- 도트와의 조화: 고해상도 이펙트가 도트와 어긋나면 All In 1의 `PIXELATE_ON`(`_PixelateSize` 64~128)을 이펙트 스프라이트에 쓰거나, 파티클 텍스처 필터를 Point로 둔다.

## 9. 성능 노트

| 주제 | 결정 |
|---|---|
| 머티리얼 인스턴스 vs MaterialPropertyBlock | **적·시체: `renderer.material`로 개체당 인스턴스 1개를 Awake에서 한 번 만든다.** 풀링으로 개수가 제한되고(스포너 슬롯 수), 같은 셰이더 변형이라 **SRP Batcher가 유지**된다. MPB는 쓰는 순간 그 렌더러가 SRP Batcher에서 빠진다. `OnDestroy`에서 인스턴스 `Destroy`. |
| 드롭 | 등급별 공유 머티리얼 3개(`sharedMaterial` 교체). 개체별 값 변경 없음. |
| MPB를 써야 할 때 | 수십 개 이상 동시 플래시가 필요한 소형 투사체 등. 플래시 끝나면 `SetPropertyBlock(null)`로 해제. |
| 셰이더 변형 | 키워드는 머티리얼 에셋에 미리 켠다. 런타임 `EnableKeyword` 금지(빌드 스트립). 플래시·디졸브는 값(Blend/Fade)만 바꾼다. |
| 속성 ID | `Shader.PropertyToID("_HitEffectBlend")` 등을 static readonly로 캐시. |
| 코루틴·트윈 | 플래시는 `DOVirtual.DelayedCall` 대신 `SpriteHitFx`의 Update 타이머(할당 없음). 시퀀스는 `OnDisable`에서 Kill. |
| 숫자 | DNP 풀링 켜기(현재 꺼짐). 문자열 스폰 제거. |
| VFX | 풀 + 프레임당 6개 상한. 화면 밖이면 스폰 생략(`renderer.isVisible` 또는 카메라 뷰포트 체크). |
| GC | `HitContext`는 struct, 프로필은 SO 참조. Update에서 `FindWithTag`·`GetComponent` 금지(`DeadBody.Force2DeadBody`의 `FindWithTag`는 F1에서 캐시). |

## 10. 검증 계획 (unity-mcp, 포커스 금지 원칙 준수)

| 단계 | 방법 | 통과 조건 |
|---|---|---|
| 임포트 | `AssetDatabase.ImportPackage(path,false)` → 제외 폴더 삭제 → `refresh_unity` | 콘솔 에러 0, `Shader.Find("AllIn1SpriteShader/AllIn1Urp2dRenderer") != null` |
| 머티리얼 | 프로브 `Tools/probes/fx_materials.cs`: 각 머티리얼의 `IsKeywordEnabled` 목록 반환 | 3.1절 표와 일치 |
| 플래시 | 프로브 `fx_hit.cs`: `hit_one.cs`처럼 가장 가까운 적에 `damage2Enemy` 1회 → 같은 프레임에 `_HitEffectBlend` 읽기, 0.1s 뒤 다시 읽기 | 1 → 0 |
| 히트스톱 | 같은 프로브에서 `anim.speed` 0 확인 후 0.15s 뒤 1 | 복원됨, 풀 반환 후 재스폰 개체도 1 |
| 흔들림 스로틀 | 프로브로 같은 프레임 10마리 타격 → `HitFeedback` 디버그 카운터 | 흔들림 1회 |
| 사망·드롭 | 기존 `kill_one.cs` → `after_kill.cs` + `_FadeAmount` 1.3s/1.9s 샘플 | 증가 곡선, 반환 후 -0.1 |
| 등급 드롭 | 프로브로 R·U 아이템 `DropItemCheck.SetItem` 강제 | 머티리얼·기둥 색 일치, 반환 후 C 상태 |
| 숫자 | 치명 100% 무기 배열(`[2]=100`)로 타격 | `DN_Crit` 인스턴스 활성, 문자열 "!" 없음 |
| 화면 | `bash Tools/playtest.sh 10 gamefeel` 중 타격 직후 스크린샷 3장(평타·치명·처치) | `Logs/playtest/`에 사본, 흰 플래시·숫자 스타일 육안 확인 |
| 성능 | Frame Debugger 대신 프로브로 `UnityStats.batches`·`setPassCalls`(에디터) 전후 비교, 적 15마리 | SetPass 증가 ≤ 2 |

- 스크린샷만으로 0.06s 플래시는 잡기 어렵다 → 프로브에서 `_HitEffectBlend`를 1로 고정한 상태로 1장 찍고 되돌린다.
- 확인 못 한 항목은 커밋 메시지와 이 문서에 "미검증"으로 적는다.

## 10.5 도트 스킬 이펙트 파이프라인 (2026-10-18)

사용자 요청: "스킬 이펙트 도트를 다시 찍어야 할 수도 — 3D/2D로 만든 뒤 픽셀화". 레퍼런스: 메이플 히어로 스킬 영상(`Tools/shader_jobs/skill_energy/REFERENCE.md`).

| 층 | 무엇 | 파일 |
|---|---|---|
| ① 도트 VFX 베이커 | 3D 파티클(Magic Slashes FX)을 화면 밖 카메라로 프레임 렌더 → 4x4 평균 축소 → 밝기=알파, 상위 2% 정규화 → 후반 40% 소멸 → 외톨이 픽셀 제거 → (가는 선) 1px 팽창·한 단계 어두운 외곽 → 테마 램프 5색 양자화 → 공통 영역 크롭 시트 + `PixelFxClip`. 24 PPU(캐릭터와 같은 도트 밀도) | `Editor/Dev/PixelVfxBaker.cs`, `Tools/probes/bake_pixel_fx.cs`, 산출 `Assets/Art/PixelFx/` |
| ② 도트 재생 | `VfxLibrary.Entry.pixel`이 있으면 `Vfx`가 SpriteRenderer(Unlit `M_PixelFx`)로 프레임 재생, 좌우는 flipX. 평타·스킬 시전·치명 이펙트 8종이 전부 도트로 교체 | `Vfx.cs`, `VfxLibrary.cs`, `PixelFxClip.cs` |
| ③ 원본 스킬 스프라이트 셰이더 | `SkillEnergy2D` — 테마색 리컬러·외곽 글로우·노이즈 흐름·끝 디졸브 (Shader Creator 스킬 절차) | `Assets/Shaders/SkillEnergy2D.shader`, `SkillEnergyFx.cs` |

- 램프(어두움→밝음, 히어로 팔레트 구조): gold `5A1E0A C8501A F5A032 FADF9F FFFAEB` 외 blue/red/green/white/purple — `PixelVfxBaker.Ramps`.
- 새 이펙트 추가: `bake_pixel_fx.cs` 행 추가(VfxId, 프리팹, 회전, 스케일, 반폭, 램프[, 팽창, 수명, 시작시각]) → 프로브 실행.
- ④ 절차 생성 마법 도트(비숍 레퍼런스): `Tools/pixelfx/gen_magic.py` — 방사형 폭발·빛기둥·룬 마법진 × 청/보라/금/녹 12종 → `PixelVfxBaker.ImportSheet`(JSON 메타) → `VfxId.Burst*/Pillar*/Rune*`. 지팡이 4종 스킬에 연결, 빛기둥·마법진은 발밑 높이로(`VfxLibrary.IsGroundFx`).
- 남은 일: 스킬 원본 스프라이트 자체를 베이크 도트로 대체(히트박스 유지·애니 클립 교체).

## 10.6 1층계 이펙트 폴리싱 (2026-10-19)

| 항목 | 구현 | 파일 |
|---|---|---|
| 신규 도트 이펙트 6종 | 연기(회색·보라), 흙먼지(갈색·돌, 바닥 피벗), 지면 충격파, 붉은 폭발. `gen_magic.py` 규칙(밝기장 → 5색 램프)을 재사용한 절차 생성 → `VfxId` 끝에 추가(직렬화 번호 유지), 흙먼지·충격파는 `IsGroundFx` | `Tools/pixelfx/gen_polish_fx.py`, `VfxLibrary.cs` |
| 계보 무기 스킬 시전 이펙트 | `Skill.castVfx` 가 비어 있거나 테마가 맞지 않던 49개 스킬에 몹 팔레트 이펙트(돌진=흙먼지, 유령·산적 그림자=보라 연기, 흡혈·포효=붉은 폭발, 내려찍기=지면 충격파, 방어 자세=룬) | `Tools/probes/build_polish_fx.cs` ⑤ |
| 투사체·소환수 연출 | `ProjectileFx`(판정 코드 무수정 부착형): 꼬리 `TrailRenderer`(공유 `M_Trail`), 켜질 때·꺼질 때 도트 이펙트. 늑대·산적 그림자·돌 골렘·가고일 소환/퇴장 연기, 슬라임 튐, 요정·도깨비불 유도탄 꼬리 + 소멸 폭발, 화살·바위·충격파·깃털 꼬리, 뿌리 솟음 흙먼지, Aftershock·문지기 충격파 지면 충격파 (20 프리팹) | `ProjectileFx.cs`, 프로브 ④ |
| 몹 사망 연출 통일 | 모든 몹 사망 순간 연기 퍼프(크기 = 스프라이트 높이), 정예 = 금빛 폭발 + Heavy 흔들림, 보스 = 금빛 폭발 → 몸 곳곳 연기 4 → 0.6초 뒤 큰 폭발 + Impact. 코드 도트 몹(단일 몸체 시체)은 Death 애니 0.75초 뒤 3x3(72px 초과 4x4) 조각으로 부서져 튕기며 디졸브 — 원본 파츠 시체와 같은 "파편 + 디졸브" | `DeathFx.cs`, `DeathShards.cs`, `DeadBody.cs`, `EnemyHealth.TryDie` |
| 구역 분위기 입자 | `MapInfo.ambience`: 깊은 숲 잎·포자, 늪 바닥 안개·포자, 산적 성채 불씨, 탑 근교 청록 룬. 플레이어를 따라가는 화면 폭 띠, 시스템당 ≤ 50 입자, 시트 `Resources/FX/Px_Ambient`(6x1 격자) + `M_AmbientUnlit/Lit` | `ZoneAmbience.cs` |

- 예산: 조각 풀 상한 96(머티리얼 인스턴스도 최대 96, SRP Batcher 유지) — 조각을 다 낼 여유가 없으면 그 시체는 부서지지 않고 기존 디졸브(동시 10마리 처치 시 96에서 멈춤 확인). 조각 스프라이트는 원본 스프라이트별 1회 생성 캐시, 조각 물리는 Rigidbody 없이 한 Update, 지면은 `GroundProbe.Y` 1회. 이펙트는 기존 `Vfx` 풀·프레임당 6개 상한을 쓰되 보스 처치 폭발 2회만 `ignoreFrameCap`.
- `ProjectileFx` 등장 연출은 첫 LateUpdate(같은 오브젝트 StraightArrow 의 시작 위치 흩뿌림 뒤), 소멸 연출은 부모(무기·플레이어)가 함께 꺼질 때는 생략. 판정 오브젝트가 Instantiate 라 생성마다 TrailRenderer·Gradient 할당이 있다(동시 수 발, 허용).
- 피격 플래시·히트스톱: 1층계 6구역 낮·밤에서 활성 몹 19종 전부 `SpriteHitFx` 플래시(블렌드 > 0.5)·애니 정지 확인(`Tools/probes/hitfx_all.cs`). 코드 도트 몹은 루트 SpriteRenderer 를 쓰므로 런타임 부착이 그대로 동작.
- 성능(에디터 RuntimeMonitor, 5초 요약): 분위기 입자 켬/끔 비교 — 깊은 숲 546/562fps, 늪 574/597~528, 성채 538/543, 탑 근교 555/564. 체감 차이 없음(최악 프레임 스파이크는 프로브 컴파일·맵 이동).
- 검증(플레이): 신규 6종 나란히 재생(`fx_lineup`), 소환수 4종 등장 연기·유도탄 2종 트레일(`projectile_fx_test`), 계보 스킬 시전(뿌리 흙먼지, Wisp Bolt 꼬리, Boulder Toss 꼬리, Quake Slam·Gate Cleave 지면 충격파, Triple Shot 금빛, Rolling Charge 돌먼지 — `skillshot` 스크린샷), 1층계 6구역 낮·밤 사망 연출 12회(코드 도트 몹 조각 9/16, 원본 파츠 몹 0 = 기존 파츠), 정예 사망 금빛 폭발, 산적 대장 처치 3단 연출 스크린샷, `zone_soak` 1층계 6구역 낮·밤 에러 0·경고 0, EditMode 92/92. 스크린샷 `Logs/playtest/death_*`·`skill_*`·`amb_*`·`bossfx_*`(커밋 안 함).
- 남은 것: 몹 공격 판정(보스 패턴) 자체의 예고 이펙트, 계보 스킬 하네스로 Howl of the Pack 발동이 재현되지 않아 늑대 등장 연기는 프리팹 직접 생성으로만 확인, 정예 등장 순간 연출(현재 오라·SFX만), 조각은 투명 칸도 만든다(텍스처 비읽기라 판별 불가) — 조각 수 상한으로만 제한.

## 11. 작업 순서 (커밋 단위)

1. `(추가) All In 1 Sprite Shader 임포트` — Demo 제외, FX 머티리얼 5종, AssetInventory 적용 현황 갱신.
2. `(추가) HitFeedback 파사드 + 프로필 3종` — `damage2Enemy` 반환형, DamageObject 연결, 스로틀. 연출 없이 로그만.
3. `(추가) A2 피격 플래시·국소 히트스톱` — 적 프리팹 머티리얼 교체, 플레이어 틴트.
4. `(추가) A3 카메라 흔들림 티어`.
5. `(추가) A5 데미지 숫자 프리팹 분리 + 풀링`.
6. `(추가) A4 시체 디졸브·드롭 등급 연출`.
7. A6은 에셋 다운로드 후 별도. Feel은 필요해질 때만 3.2절 목록으로.

## 12. 열린 질문 (→ `Docs/OpenQuestions.md`에 임시 가정과 함께 등록)

- Magic Slashes FX / Particle Pack 다운로드 시점. **가정:** A6 보류, 기본 파티클로 대체.
- 화면 흔들림 강도 옵션 UI 노출 여부. **가정:** 설정값만 두고 기본 1.
- 보스 처치 전역 슬로(0.10s) 허용 여부. **가정:** 허용.

## 출처

- Uhiyama Lab, "Fixing My Attacks Don't Feel Like They Hit" (Unity) — 히트스톱 약/강/치명 구간, 플래시 0.08s, MPB와 SRP Batcher 주의: https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/
- Wayline, Game Feel & Juice — 강타 50~100ms 프리즈: https://www.wayline.io/learn/game-feel/1
- Slicker.me, Game Feel in Pygame — 흔들림 50~300ms, 감쇠: https://slicker.me/python/game_feel_pygame.htm
- Eastondev, Where Does Game Feel Come From — 플래시·흔들림·숫자·파티클 레이어링: https://eastondev.com/blog/en/posts/dev/20260521-game-feedback-feel/

## 10.7 기본 무기 평타 이펙트 — PixelLab vfx_workbench + Shader Creator (2026-10-25)

사용자 요청: "기본 무기 이펙트를 새로 만들고 Shader Creator나 픽셀랩 이펙트 생성기를 실제로 연결해 줘" (새 주인공 픽셀 리그와 함께).

| 층 | 무엇 | 파일 |
|---|---|---|
| ① 그림 | PixelLab `vfx_workbench`(구독 무료)로 회색조 4종: 세로 초승달(`slash_trail`), 가로 초승달(같은 부품 facing −90·flatten 0.55·sweep 1.4), 찌르기(`thrust_streak` size·reach 1.3), 타격 별(`hit_star`). 색 `#2e2e2e #7a7a7a #c8c8c8 #ffffff`. 사용자 My effects 에 PB_SlashDown·PB_SlashSide·PB_Thrust·PB_HitStar 로 저장(사이트에서 슬라이더로 다시 고칠 수 있음) | 원본 `Docs/ArtCompare/WeaponFx/pixellab/` |
| ② 변환 | 여러 줄 격자 → 한 줄 시트, 끝 빈 프레임 제거, RGB = 밝기, 피벗 위↔아래 뒤집기 → `PixelVfxBaker.ImportSheet`(24 PPU) | `Tools/pixelfx/import_pixellab_vfx.py`, `Tools/probes/build_weapon_fx.cs`, 산출 `Assets/Art/PixelFx/Px_W*` |
| ③ 셰이더 | `PixelSlash2D`(Shader Creator 절차: 의도 계약 → 정적 게이트 → Unity 컴파일 → 엔진 렌더 측정). 밝기 → 테마 램프 5색 평면 단계색, `_Dissolve` 텍셀 단위 소멸 + 밝은 경계, 자체발광(밤에도 원색) | `Assets/Shaders/PixelSlash2D.shader`, `Resources/FX/M_PixelSlash.mat`, 계약 `Tools/shader_jobs/pixel_slash/intent.json` |
| ④ 재생 | `Vfx.PlayThemed(id, 위치, 좌우, 크기, 램프, 상하반전)` — 램프 색은 MaterialPropertyBlock, 수명 60%부터 디졸브 0→1 | `Vfx.cs`, `PixelRamp.cs` |
| ⑤ 연결 | 근접 평타(기존 `swingVfx` 가 Slash 계열)만: 평타 애니 이름을 `PixelRig.MapName` 으로 묶어 Stab → 찌르기, SwingDown → 세로 초승달, SwingUp → 세로 초승달 상하 반전, SwingSide → 가로 초승달. 테마 색 = 기존 swingVfx 색(흰·적·청·금·녹 → `PixelRamp.FromSwing`) — 계보 무기 데이터 무수정. 판정 프리팹에 붙은 예전 베기 그림은 끈다(판정은 유지). 평타 적중 시 타격 별(무기 테마 색). 지팡이 마법 등은 기존 이펙트. 끄기: `WeaponFx.Enabled` | `WeaponFx.cs`, `Weapon.CreatAttackPrefab`, `HitFeedback.OnEnemyHit` |

- 검증: 셰이더 의도 계약 c1~c5 전부 pass(엔진 렌더 측정 — 4색, 디졸브 면적 단조 감소, 텍셀 단위, 테마 교체, 샘플 1회·분기 0). 플레이(낮)에서 녹슨 검 3단 콤보(찌르기·내려베기·올려베기) 연속 촬영(`Tools/probes/rig_action_strip.cs combo3`), 타격 별 재생. 콘솔 에러 0.
- 미검증: 실제 적을 때려 타격 별이 나오는 경로(재생 함수 직접 호출로만 확인), 지팡이 외 다른 근접 무기 종류별 위치·크기(swingVfxScale 그대로).
- 다음: 스킬 이펙트도 같은 방식(회색조 vfx_workbench + PixelSlash2D)으로 바꿀 수 있다 — 스킬마다 부품 조합을 고르고 `VfxId` 끝에 추가.

