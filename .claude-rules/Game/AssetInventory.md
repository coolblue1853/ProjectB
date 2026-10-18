# 보유 에셋 인벤토리 (Unity Asset Store, 86개)

> 2026-10-18에 계정 `coolblue1853_unity`의 "내 에셋"에서 수집했다. ProjectB(도트 2D 횡스크롤 ARPG)에 쓸 수 있는지를 기준으로 분류한다.
> 도입하면 "적용 현황"을 갱신한다. 대용량(GB급) 팩은 필요한 파일만 골라서 임포트한다.

## 1. 이미 프로젝트에 있음

AnyPortrait, Behavior Designer, DOTween(Pro), Easy Save 3, Damage Numbers Pro, Master Audio 2022, Pro Camera 2D, Dialogue System for Unity, Parallax Dusk Mountain Background, Pixel Perfect Camera

## 2. 도입 후보 — 높음 (도트·2D·전투감)

| 에셋 | 용도 | 비고 |
|---|---|---|
| **Monsters_Creatures_Fantasy** (Luiz Melo) | 신규 몬스터 스프라이트시트(해골, 고블린, 버섯, 눈알 등) | 도트, 애니 포함 → 새 사냥터 |
| **Evil Wizard 2 / Bringer Of Death / Medieval King Pack 2 / Martial Hero** | 보스·엘리트 몬스터 후보 | 도트, 공격/피격/사망 애니 |
| **2D Pixel - RPG Monster Pack** | 일반 몬스터 추가 | 도트 |
| **Magic Cliffs Pixel Art Environment** | 신규 사냥터 타일·배경 | 도트 |
| **Underwater Fantasy** | 수중 테마 맵 | 도트 |
| **Free Pixel Font - Thaleah** | UI·데미지 숫자 도트 폰트 | 한글 미지원 → 숫자/영문 전용 |
| **8-Bit Sfx** | 타격·UI 효과음 | |
| **Feel** (More Mountains) | 타격감(히트스톱, 화면 흔들림, 플래시) | DOTween과 역할 분담 필요 |
| **All In 1 Sprite Shader** | 피격 플래시, 아웃라인, 디졸브(사망), 등급 글로우 | URP 2D 지원 |
| **Magic Slashes FX / Particle Pack** | 스킬·평타 이펙트 | 3D 파티클 → 2D에 맞게 조정 |
| **Quest Machine** | 퀘스트 시스템(Dialogue System과 연동) | 신규 콘텐츠 축 |
| **Text Animator for Unity** | 대사·아이템 이름 연출 | |

## 3. 도입 후보 — 중간

| 에셋 | 용도 |
|---|---|
| Pixel Hero Maker / 2D Pixel Unit Maker - SPUM | NPC·도트 캐릭터 제작 (플레이어는 AnyPortrait 유지) |
| Smart Lighting 2D | 밤 조명 (현재는 URP 2D Light + Lost 플러그인) |
| A* Pathfinding Project Pro | 비행 몬스터 경로 (지상형은 현재 BT로 충분) |
| I2 Localization | 다국어 |
| Odin Inspector | 데이터 편집 툴 (CSV 에디터) |
| UI Particle Image | UI 이펙트 |
| Ultimate Screenshot Tool | 스토어용 스크린샷 |
| Fantasy Music Suite 2 / Free Fantasy Adventure Music Pack / SAGA Complete Collection / Ultimate Game Music Collection / Game Music Treasury | BGM (현재 1곡) |
| SoundBits Free Sound FX / Free Frost Mage SFX / Pro Sound Collection / Ultimate SFX & Music Bundle | 스킬·환경 효과음 |
| Corgi Engine / Flare Engine | 2D 플랫포머 레퍼런스 (이동·사다리·원웨이 구현 참고만) |

## 4. 해당 없음 (3D·FPS·장르 불일치)

FPS Framework, MFPS 2.0, TPS Shooter, Invector TPC, PBR Fighters, Nodachi AnimSet, Toon Dinosaurs 2, 3D Scifi Base, Medical Equipments, Tropical Forest, Advanced Foliage, Conifers, POLYGON Nature, Toony Tiny RTS, Low Poly 계열, Stylized Karts, The Lost Temple, Atlas terrain, Grid Builder 2, Mirrors VR, Super Realistic Diamond, Wireframe Shader, Geometry Algorithms, Ballistics, Smooth Sync, PUN 2, EndlessBook, Procedural Walk, Unlit Stylized Water, Medieval Town Exteriors, Lost Crypt 2D(샘플), RPG Farming Kit, Bad Word Filter, Color Studio, Scenes Switcher, Cartoon GUI Pack(카툰풍), UI Examples, Fighter Pack Bundle, Horror SFX

## 5. 다운로드 · 임포트 방법 (자동화, 창 없이)

- **다운로드(에디터 로그인 필요):**
  1. `Tools/probes/as_purchases.cs` 실행 → 1~2초 뒤 `as_purchases_read.cs`로 구매 목록(productId, 이름)을 읽는다.
  2. `as_download.cs`에서 `__IDS__`를 ID 목록으로 치환해 실행한다. 내부 API `AssetStoreDownloadManager.Download(IEnumerable<long>)`를 쓴다.
  3. 진행 상황은 `%APPDATA%/Unity/Asset Store-5.x/<퍼블리셔>/...`에 `.unitypackage`가 생기는지로 확인한다(`.tmp`가 있으면 받는 중).
  - 로그아웃 상태면 `UnityConnect.ShowLogin()`으로 로그인 창을 띄우고 사용자가 로그인한다(비밀번호 입력은 사용자만).


- 에디터 창을 띄우지 않고 가져온다(포커스 강탈 금지). 캐시된 `.unitypackage`는 `%APPDATA%/Unity/Asset Store-5.x/<퍼블리셔>/...`에 있다.
  - 캐시에 없으면 Package Manager(My Assets)에서 다운로드가 필요하다. → 사용자 요청 또는 `manage_packages` 도구로 처리.
- `AssetDatabase.ImportPackage(path, false)`(unity-mcp `execute_code`)로 대화상자 없이 임포트한다.
- 임포트 경로는 `Assets/ThirdParty/<에셋명>`을 권장한다. 데모 씬과 예제는 커밋에서 제외한다.

## 6. 적용 현황

| 에셋 | 상태 | 커밋 |
|---|---|---|
| All In 1 Sprite Shader | ✅ 임포트 (`Assets/Plugins/AllIn1SpriteShader`, Demo 제외, 17MB) — 피격 플래시·디졸브·등급 아웃라인용 | (이번 커밋) |
