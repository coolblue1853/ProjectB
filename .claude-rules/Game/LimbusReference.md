# 림버스 컴퍼니 레퍼런스(이펙트·동작) — 10-28 사용자 지정

이펙트 품질 목표는 **림버스 컴퍼니**다(스컬 대신). 형태(실루엣)는 도트로 잡고 디테일은 셰이더·파티클 VFX로 채운다. 동작 레퍼런스로도 쓴다.

## 규칙

- **외형 작업은 묻지 않아도 스컬 + 림버스에서 먼저 가져온다(10-28 사용자).** 이펙트·동작·아이콘·UI·배경·몹 형태 등 외형 작업을 시작할 때는 `SkulRef/lib`(스컬 10만 장 분류)와 `LimbusRef`(캡처·fx·all)에서 해당 레퍼런스를 먼저 찾아 문법을 정리하고 만든다. 컨텐츠(기획·수치·스토리)는 사용자가 정하므로 구조만 참고한다.

- 사용자 PC에 설치된 정품에서 꺼낸 **참고 전용** 자료다. 추출물·캡처는 저장소 밖 `C:\Users\user\LimbusRef\` 에만 두고 **커밋하지 않는다**(스컬과 같은 규칙, `SkulAssetLibrary.md`).
- 우리 게임에 그림을 그대로 넣지 않는다. 형태·타이밍·색 구성을 읽어 우리 도트·VFX로 다시 만든다.

## 데이터 위치

- 설치: `C:\Program Files (x86)\Steam\steamapps\common\Limbus Company` (Unity 6000.3.12f1, 버전 문자열이 지워져 있어 도구에 직접 지정해야 한다)
- 실제 에셋: 어드레서블 캐시 `%USERPROFILE%\AppData\LocalLow\Unity\ProjectMoon_LimbusCompany` (UnityFS 번들 1,556개, 17GB, 암호화 없음)
- 주요 컨테이너: `Assets/Prefab/Effect`(이펙트 프리팹), `Assets/FXv2`·`Assets/FX/Grp_Mat`(이펙트 텍스처·머티리얼·셰이더그래프), `Assets/Resources_moved/Prefab/SD/Personality`(수감자 인격), `SD/Enemy`·`SD/EGO`, `Assets/Animation/SD`(클립)
- 목록: `LimbusRef/containers.tsv`(번들·타입·컨테이너, `LimbusRef/tools/list_containers.py`, UnityPy `FALLBACK_UNITY_VERSION`)

## 캐릭터 구조

- 대기·이동 = Spine(`[SpinePivot]/Spine GameObject`), 스킬 = 스프라이트 리그(`[SpRenderer]`) + 이펙트 자식(`DefaultEffectPivot/FX_*`, 클립이 켜고 끈다).
- 클립 이름: `<인격>_S1`, `_S2`, `_S3`, `_Damaged`, `_Guard`, `_Evade`, `_Move` 등. 애니메이터 상태 이름이 클립 이름과 달라 `Animator.Play` 가 안 먹는다 → `AnimationClip.SampleAnimation` 으로 시간 샘플.

## 캡처 방법(우리 에디터 플레이 모드, 프로젝트에 저장 없음)

1. 플레이 모드 진입 → `LimbusRef/tools/lb_load.cs` 로 번들 전부 `AssetBundle.LoadFromFile`(4초).
2. `bash LimbusRef/tools/lb_run.sh <프리팹 경로> <클립들> <이름> [카메라 크기] [x] [y]` → `LimbusRef/cap/<이름>/<클립>/f_###.png`(30fps). Spine 몸은 끄고 클립을 실시간 샘플(파티클은 실시간 시뮬레이션).
3. `python LimbusRef/tools/lb_sheet.py <이름>` → 접촉 시트·GIF·느린 GIF.
- 한계: 게임의 스크립트(Spine 런타임 등)가 없어 그 컴포넌트는 빠진다. 블룸 등 후처리가 없어 이펙트가 실제보다 어둡다.
- 텍스처 일괄 추출: AssetStudioModCLI `--unity-version 6000.3.12f1 --filter-by-container "(FXv2|Prefab/Effect|Assets/FX/|Battle/Skill|Effect/Battle)" --filter-with-regex` → `LimbusRef/fx`(3.6GB).

## 첫 관찰(이스마엘 요정 S1~S3)

- 스킬 동작은 **키 포즈 몇 장을 길게 잡고**, 포즈 사이에 **흐림(모션 블러) 전환 1프레임**을 넣는다. 보간 프레임이 거의 없다.
- 베기 순간 캐릭터를 감싸는 **고리형 궤적**(메시 이펙트)이 1~2프레임 나오고 빠르게 사라진다.

## 베기·찌르기 문법(료슈 S3·이상 S1·돈키호테 S1 캡처, 10-28)

- **모양:** 원이 아니라 몸을 감싸는 **기울어진 타원 고리**. 캐릭터보다 훨씬 크다(지름 ≈ 키 1.5~2배).
- **두께:** 머리(칼이 지금 있는 곳)가 가장 두껍고, 지나온 길로 **긴 꼬리가 바늘처럼** 가늘어진다. 두께는 반지름의 1/8~1/10(우리 v6 은 1/3 이라 뭉툭했다).
- **색:** 몸 색 + 진행 쪽 가장자리 포인트 색(료슈: 검은 먹 + 빨간 날, 이상: 반투명 회색 리본). 전부 흰색인 경우가 없다.
- **시간:** 2~3프레임 보이고 꼬리부터 줄며 사라진다. 잔상이 오래 남지 않는다.
- **동작:** 키 포즈 몇 장을 길게 잡고, 포즈 사이에 흐림 전환 1프레임.

## 우리 적용 — 칼 스미어 v7(`Tools/pixelfx/gen_smear.py` ELLIPSE·NEEDLE)

- 내려·올려베기: 절차 생성 타원 고리(중심·가로/세로 반지름·기울기·머리 각·진행 방향·꼬리 230°/220°·머리 두께 7/6px). 4프레임 36fps, 프레임마다 꼬리 비율 1 → 0.62 → 0.34 → 0.14.
- 찌르기: 바늘 쐐기(손 뒤 12px → 칼끝 너머 58px, 70% 지점 최대 두께 5px). 뒤에서부터 줄며 앞으로 나아간다.
- 받침 레이어(`SmearAura`)는 같은 타원을 쓴다(글로우 안쪽 끔·칼선 머리 쪽 45%·바람 호 1줄). 시트 캔버스 112 → 136(오른쪽).

## 타격 모양 문법(Fx_T_Shape_Hit·Impact·Crack 텍스처, 10-28) — 다음 작업: 타격 이펙트 v7

- **방향성 쐐기**(hit_02·03): 맞은 방향으로 뻗는 찢긴 쐐기, 뒤는 톱니. 2x2 플립북으로 모양이 바뀐다.
- **불규칙 바늘 별**(hit_04~06): 4~8갈래, 갈래 길이가 서로 다르고 바늘처럼 뾰족하다. 둥근 섬광이 아니다.
- **방사형 줄기 고리**(impact_03·05·09): 가운데가 비고 바깥으로 짧은 줄무늬가 퍼지는 링.
- **금 간 선**(crack_02·07·11): 중심에서 갈라지는 잔금(강한 타격·마무리 타에).
- 우리 적용 계획: 큰 형태(쐐기·별) = 도트 절차 생성, 줄기 고리·잔금 = 셰이더·파티클 받침. 지금 `Px_WHitStar`(PB_HitImpact_v3, 둥근 섬광+불꽃)를 교체.

## 다음 작업 — 림버스 동작으로 마스터 모션 38개 재검토(10-28 사용자)

- 캡처가 끝나면(인격 262·적 249) 스킬 동작을 **동작 종류별로 분류**한다: 베기(내려·올려·가로·회전), 찌르기, 내려찍기, 휘두르기(둔기), 사격·투척, 시전, 발차기·맨손, 잡기·던지기, 돌진, 점프 공격, 막기·회피·피격 등.
- 분류 결과를 `MotionSet.md` 의 마스터 모션 38개와 `JobCoverage.md` 직업 대응표에 대 보고, **빠진 동작 / 합칠 수 있는 동작**을 표로 정리한다. 직업 전부를 림버스에서 찾을 수는 없으니 동작 문법(자세·타이밍) 위주로 본다.
- 림버스 동작은 **프레임 수가 적다**(키 포즈 몇 장 + 흐림 전환 1프레임). 자세·실루엣·완급(유지 시간)은 그대로 참고하고, 프레임 수는 우리 셀 기준(타 3~12프레임)에 맞춰 보간한다.
- 도구: `LimbusRef/tools/lb_keypose.py`(키 포즈 수·유지 프레임·블러 프레임 → `LimbusRef/keypose.tsv`). 첫 표본: 기본 스킬 = 준비 자세 약 0.33초 유지 → 전환 2~3프레임 → 타격 자세 유지(우리는 준비 0.16~0.19초로 짧다).

## 전체 재료 추출(10-28, 저장소 밖 `LimbusRef/all`, 36GB·26만 파일)

| 분류 | 파일 수 | 쓸 곳 |
|---|---|---|
| `Resources_moved/Prefab`(SD 캐릭터·전투 연출 부품) | 15.7만 | 인격·적·EGO 스프라이트 리그 조각 → 동작 실루엣·부위 분할 참고 |
| `Resources_moved/RPGSystem` | 6.4만 | 거울 던전·RPG 모드 맵·노드·아이콘 → 던전 구조·UI |
| `Resources_moved/Sprite` | 2.4만 | UI·스킬·아이템·초상화 아이콘 → 아이콘 문법 |
| `Resources_moved/Story` | 1만 | 스토리 배경·스탠딩 → 분위기(메이드 인 어비스 몽환과 함께) |
| `Resources_moved/Buf` | 3,372 | 버프·디버프 아이콘 → 상태 이상 아이콘 문법 |
| `Resources_moved/StaticData/static-data` | 2,200(JSON) | skill 147·buff 87·passive 77·enemy 43·item 48·abnormality 200여·ego-gift 32·panic-buff 31 등 → **컨텐츠 기획 참고**(수치 구조·버프 종류·기믹) |

- 이펙트 쪽은 `LimbusRef/fx`(1.6만 장, 3.6GB): `Fx_T_Shape_*`(베기·타격·잔금·줄기), 노이즈, 머티리얼별 텍스처.
- 컨텐츠는 사용자가 기획한다. 여기서는 구조·문법만 읽고, 그림·문구·수치를 그대로 옮기지 않는다.
