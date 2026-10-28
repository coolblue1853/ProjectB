# 림버스 컴퍼니 레퍼런스(이펙트·동작) — 10-28 사용자 지정

이펙트 품질 목표는 **림버스 컴퍼니**다(스컬 대신). 형태(실루엣)는 도트로 잡고 디테일은 셰이더·파티클 VFX로 채운다. 동작 레퍼런스로도 쓴다.

## 규칙

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
