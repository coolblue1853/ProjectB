# 보유 에셋 도입 계획 (2026-10-18 사용자 요청)

> 사용자가 "꼭 써서 지금 상태에서 게임에 넣어줘"(필수), "괜찮으면 넣어줘"(평가 후), "효과음·BGM은 받아 두고 필요할 때"(상시)로 나눠 요청했다.
> 순서는 **인벤토리 Phase 3a를 먼저** 끝낸 뒤 이 문서의 웨이브 순서를 따른다(사용자 지시: 대화 시스템은 3a 뒤).
> 현황은 `Game/AssetInventory.md`와 함께 갱신한다. 각 항목이 끝나면 ✅와 커밋 해시를 적는다.

## 1. 현황 (2026-10-18 20시 확인)

| 에셋 | 프로젝트 | 현재 쓰임 | 비고 |
|---|---|---|---|
| Pro Camera 2D | ✅ 임포트 | 카메라 추적·흔들림(`HitFeedback`, `MapInfo`, `Portal`) | 확장 기능 미사용 |
| Behavior Designer | ✅ | 몬스터·보스 BT (`isAttack` 애니 파라미터) | |
| Master Audio 2022 | ✅ `Plugins/DarkTonic` | `Sfx` 래퍼, 마스터 볼륨 설정 | 플레이리스트·앰비언트 미사용 |
| Quest Machine | ✅ | 퀘스트 게시판·사이드 퀘스트(코드 생성) | 대화 연동 없음 |
| AnyPortrait | ✅ | 플레이어 | |
| Damage Numbers Pro | ✅ | 피해 숫자 | 회복·치명·상태 프리셋 부족 |
| I2 Localization | ❌ 미다운로드 | — | |
| Dialogue System for Unity | ✅ 임포트, **미사용** | 씬에 Dialogue Manager 없음 | |
| Feel (More Mountains) | ⬇ 다운로드됨(302MB), 미임포트 | — | 필요한 폴더만 임포트 |
| All In 1 Sprite Shader | ✅ | 피격 플래시·디졸브·등급 아웃라인 | |
| Odin Inspector | ✅ | 데이터 창(아이템·세트·몬스터 CSV) | |
| Easy Save 3 | ✅ | 전체 세이브 | |
| Text Animator (Legacy) | ✅ `Plugins/Febucci` | 픽업 로그·배너 | 대사 타자기 미사용 |
| Smart Lighting 2D | ❌ 미다운로드 | — | 평가 대상 |
| 2D Pixel Unit Maker - SPUM | ❌ 미다운로드 | — | 도트 인간형 소스로만 |
| 2D Pixel - RPG Monster Pack | ❌ 미다운로드 | — | 1층계 몬스터 후보 |
| Corgi Engine | ❌ 미다운로드 | — | 참고용(임포트 안 함) |
| 효과음·BGM 팩 | 일부 다운로드(Alison Turk, Craze Music, Daniel SoundsGood, Little Robot, SoundBits) | `ThirdParty/Sfx`, `ThirdParty/Music` 일부 | 나머지는 다운로드 후 카탈로그화 |

## 진행 기록

- **W1 1차 (2026-10-18):** ✅ Dialogue System 런타임 구성(`Assets/Script/Dialogue/`: `DialogueBoot` 컨트롤러·코드 생성 DB `StoryDialogues`, 픽셀 프레임 대사창 `PixelDialogueUI` + Text Animator 타자기·선택지, `UiLock "Dialogue"`). 첫 실행 오프닝 독백(ES3 `story.opening`), 마을 NPC 3명(장로·대장장이·상인 — SPUM 기본 유닛에 머리·수염·옷 교체, `TalkNpc` 위 방향키, 프로브 `setup_npcs`). 검증: 오프닝 자동 시작 → Z로 끝까지 진행 후 잠금 해제(`dialogue_advance`), 장로 대화 시작(`npc_talk_test`) 스크린샷. ⏳ Quest Machine 연동·초상·세이브 브리지·대사 중 BGM 덕킹.
- **W1 사운드 (2026-10-18):** ✅ 대사 중 BGM 덕킹(×0.45). ✅ `Tools/sound_catalog.py` → `Docs/SoundCatalog.md`(팩 12종, 오디오 약 2.4만 개 목록). ✅ `Tools/extract_audio.py`로 필요한 곡만 OGG 변환 추출(6곡, 각 1~2MB): Town+Marketplace, Field+Forest LOOP, Night+Moonlit Forest, Boss+Boss Battle 2, 신규 Field2(2층계↑: Fields of Mystery, Forbidden Forest). `BgmDirector`가 맵 층계로 Field/Field2 선택(프로브 `bgm_state` — 마을 Town → Lv2 Field2 → Lv1 Field). SAGA 팩은 다운로드 0바이트라 재요청.
- **W2 Damage Numbers (2026-10-18):** ✅ `NumberKind.Heal`(초록)·`Hurt`(빨강) 추가, 플레이어 `HpHeal`/`HpDown`이 머리 위에 표시(`PlayerHealthManager._numberPrefab` = EnemyDamge, 프로브 `setup_player_numbers`). ⏳ BLOCK/IMMUNE 텍스트, 골드·경험치 숫자.
- **W2 ProCamera (2026-10-18):** ✅ `BossIntro`: 보스 활성화 시 카메라 타깃 추가(보스·플레이어 함께 프레이밍)+줌아웃 1.5, 흔들림·배너, 1.6초 뒤 복귀, 같은 보스 60초 쿨다운. 프로브 `boss_intro_test`(`__BOSS__`=PassiveTerror: 타깃 2→1). ⏳ 맵별 경계(Rooms).
- **W2 Feel (2026-10-18):** ✅ 핵심만 `Assets/Feel`(64MB: MMFeedbacks·MMTools·NiceVibrations, 데모 제외). `AssetDatabase.ImportPackage`가 필터 패키지를 "completed"로 끝내고 아무것도 넣지 않아서, .unitypackage의 asset/asset.meta를 pathname 위치에 직접 풀었다(GUID 유지). 코드는 `MoreMountains.Tools` 어셈블리. `FeelFx.Pop/PlayerPop`(MMF_Scale Additive, 화면 흔들림 설정 0이면 끔) → 강화 성공(+5↑ 크게)·승급·보스 보상. 프로브 `feel_pop_test`(3.0→3.51→3.0). 시간 조절 피드백은 일시정지·모래시계와 충돌해 쓰지 않는다.
- **SPUM:** ❌ 사용 안 함(사용자 2026-10-19: "너무 안 어울린다, 그 에셋은 안 쓰는 걸로"). 프로젝트에서 삭제. 마을 NPC는 기존 NPC 도트 양식(24×32 논리 픽셀·8배·PPU 100)으로 코드 생성(`Tools/pixelfx/gen_elder.py`) — 장로 1명 + 대사창 초상. 인간형 적(산적 등)도 같은 방식으로 그린다.

## 2. 필수 도입 — 게임에서 실제로 쓰는 형태

| 에셋 | 넣을 곳 (구체 기능) | 완료 기준 |
|---|---|---|
| **Dialogue System** | 마을 NPC 대화(촌장·대장장이·상인·게시판 관리인), 스토리 대사(브로치·아버지, 첫 진입 독백), 구역 첫 방문 짧은 독백, NPC 머리 위 bark. 데이터베이스는 코드/CSV로 생성. 세이브는 Pixel Crushers Save System → ES3 브리지(키 `dialogue.*`) | 마을 NPC 3명 대화 + 오프닝 독백이 플레이에서 열리고, 입력 잠금(`UiLock "Dialogue"`)·세이브 왕복 확인 |
| **Text Animator** | Dialogue System 대사창 타자기·흔들림 태그(`<shake>`, `<wave>`), 보스 배너 | 대사창에서 태그 연출 확인 |
| **Quest Machine** | Dialogue System 연동으로 NPC가 퀘스트를 주고 받는다(게시판은 유지). 스토리 퀘스트 "아버지의 흔적" 1편 | NPC 대화에서 수락→처치→보고 루프 |
| **I2 Localization** | UI·아이템명·대사 키를 I2 소스로 이전. 1차는 **영어 + 한국어**. 한글 도트 폰트(OFL, 예: Galmuri) TMP 에셋 추가 → Q-407 | 설정 메뉴에 LANGUAGE 항목, 전환 시 UI·대사 즉시 갱신 |
| **Feel** | `HitFeedback` 파사드 뒤 구현으로 MMF_Player 프리셋 추가: 레벨업·강화 성공·희귀 드롭·보스 처치·포털 진입. DOTween은 UI 트윈 전담(역할 분담) | 프리셋 5종이 파사드 호출로 재생, 설정의 흔들림·플래시 배율 존중 |
| **Pro Camera 2D** | 맵별 Numeric Boundaries/Rooms, 보스방 진입 시 Zoom-to-fit + Cinematics(보스 소개 1.5초), 포털 페이드 전환 | 보스 2종 인트로 연출, 맵 경계 밖 노출 0 |
| **Behavior Designer** | 신규 몬스터용 공용 서브트리(순찰·추적·원거리·돌진) External Tree, 보스 패턴 셀렉터(페이즈별 분기 — `BossPhase.Phase` 공유 변수) | 신규 몬스터는 서브트리 조합만으로 제작 |
| **Master Audio** | 구역·낮밤별 플레이리스트, 숲 앰비언트(새·벌레·바람) 그룹, 보스전 BGM 크로스페이드, 대사 중 덕킹 | 구역 이동·밤 전환 시 BGM/앰비언트 교체 |
| **AnyPortrait** | 스토리 NPC(아버지/문지기) 또는 마을 NPC 1명 대기·대화 모션 — 비용 크면 SPUM으로 대체 | NPC 1명 이상 |
| **Damage Numbers Pro** | 회복(초록)·치명(큰 글자)·상태이상·"BLOCK/IMMUNE" 프리셋, 경험치·골드 획득 숫자 | 프리셋 4종 |
| **All In 1 Sprite Shader** | 정예 몬스터 아웃라인 글로우, 드롭 아이템 등급 글로우, 독·빙결 틴트, 포털·결정 홀로그램 | 키워드 프리셋을 `HitFeedback`/드롭에서 사용 |
| **Odin** | 드롭 테이블·몬스터 공격 CSV 편집 탭(C5 잔여), 대사/퀘스트 데이터 검증기 | 데이터 창 탭 추가 |
| **Easy Save 3** | 세이브 슬롯 3개 + 자동 백업(`.bak`), `saveVersion` 필드(InventoryRefactor 2.7) | 슬롯 선택·백업 복구 프로브 |

## 3. 평가 후 도입

| 에셋 | 평가 방법 | 채택 기준 / 기본 방침 |
|---|---|---|
| **Smart Lighting 2D** | 밤 숲 1개 맵에서 현재 URP 2D Light와 A/B 스크린샷·프레임 시간 비교 | 그림자·광원 품질이 확실히 낫고 비용이 같거나 적을 때만. 아니면 URP 2D 유지 |
| **SPUM** | 런타임 시스템은 넣지 않는다. 에디터에서 인간형(산적 단검·활·방패, 산적 두목, 붉은 탐험가, 마을 주민)을 조합→**스프라이트시트로 굽고** 기존 MonsterBuilder 규격(24 PPU)에 맞춘다 | 1층계 ⑤구역 산적 3종 + 두목, 마을 NPC |
| **RPG Monster Pack** | 프레임·PPU·애니(대기·이동·공격·피격·사망) 확인 후 1층계 ②~④구역 몬스터에 매핑(늑대·멧돼지·버섯·슬라임·개구리 등 있으면 우선). 팩에 패턴이 없으면 BT 서브트리로 패턴을 만든다 | 구역당 3종 이상 확보되면 Q-405 가정(GPT 생성)보다 우선 |
| **Corgi Engine** | **프로젝트에 임포트하지 않는다**(입력·물리 충돌). 별도 폴더에 풀어 코드만 참고 | 후보 기능: 원웨이 발판 하강, 사다리/로프, 벽 점프·벽 미끄럼, 움직이는 발판, 레지 그랩, 넉백 곡선, 코요테 타임·점프 버퍼. 채택 시 `PlayerStateDesign` 단계에 맞춰 직접 구현 |

## 4. 효과음·BGM

- 보유 사운드 팩을 전부 다운로드 캐시에 받아 둔다(사용자 허락 완료).
- `Tools/sound_catalog.py`로 패키지 안 파일명을 카테고리(타격·마법·UI·발소리·환경·BGM)별로 `Docs/SoundCatalog.md`에 정리한다. **프로젝트에는 쓰는 파일만** `ThirdParty/Sfx`·`ThirdParty/Music`에 임포트한다.
- 우선 교체 대상: 구역별 BGM(숲 근교·일반 숲·깊은 숲·늪·산적 성채·탑 근교·마을·보스), 발소리(풀·흙·돌), 강화 성공/실패, 아티팩트 4종.

## 5. 웨이브 순서

| 웨이브 | 내용 | 선행 |
|---|---|---|
| W0 | 인벤토리 Phase 3a (진행 중) | — |
| W1 | Dialogue System + Text Animator + Quest Machine 연동(마을 NPC·오프닝), Master Audio 구역 BGM/앰비언트 | W0 |
| W2 | Feel 부분 임포트 + 프리셋, Damage Numbers·All In 1 프리셋 확장, ProCamera 보스 인트로 | — |
| W3 | I2 Localization + 한글 도트 폰트(Q-407) | W1(대사 키) |
| W4 | RPG Monster Pack·SPUM으로 1층계 ②구역 몬스터 + BT 서브트리, Odin 드롭 테이블 탭 | 다운로드 |
| W5 | Smart Lighting 평가, Corgi 참고 기능(원웨이 하강·사다리), ES3 슬롯·백업 | — |
| 상시 | 사운드 카탈로그 → 필요 시 임포트 | 다운로드 |

- 다운로드는 사용자가 "안 물어봐도 되니 알아서 받아서 진행"하라고 허락했다(2026-10-18). 보유 에셋은 묻지 않고 받고, 받은 목록·용량은 `Docs/AutomationLog.md`에 적는다.
- 각 항목은 플레이 프로브 또는 스크린샷으로 검증하고, 못 하면 "미검증"으로 적는다.

## 6. 작업 큐 (2026-10-19 사용자: "이미지류는 PixelLab 충전까지 미루고 스토어 에셋 남은 작업 진행")

1. ✅ Damage Numbers: `HitFeedback.SpawnText` — IMMUNE(무적 중 피격)·BLOCK(방어로 0 피해, 0.5초 제한)·POISON/BLEED(새로 걸릴 때)·+N G(퀘스트 골드). 경험치 시스템은 없어 제외 → 2. ✅ Feel 프리셋 5종(`FeelFx.RankUp/EnhanceSuccess/RareDrop/BossKill/PortalEnter`: MMF_ImageAlpha 화면 플래시 + 팝, 보스 처치 슬로모션 0.25×0.5초 — MMF_TimescaleModifier는 MMTimeManager가 일시정지와 충돌해 자체 구현) → 3. ✅ All In 1 정예 금빛 1px 외곽선(`Resources/FX/M_EnemySpriteElite` OUTBASE·PIXELPERF, `SpriteHitFx.SetElite`) + 독/출혈 맥동 틴트(`StatusTint`). 빙결은 게임에 둔화 효과가 없어 보류 → 4. ✅ Master Audio 구역 재생목록 DeepForest(Mystery Ambience)·Swamp(Ominous Ambience, 밤 포함)·Ruins(Nordic Landscape), 보스 곡은 보스가 11유닛 안일 때만(`BgmDirector`) + `AmbienceDirector` 구역·낮밤 앰비언트 크로스페이드(숲 새·개울/풀벌레, 늪 개구리, 성채·근교·2층계 바람, 마을 풍경 소리, `Resources/Ambience`) → 5. ProCamera 맵 경계·포털 페이드 → 6. ES3 슬롯·백업·saveVersion → 7. Dialogue+Quest NPC 연동·스토리 퀘스트 → 8. BT 공용 서브트리·보스 페이즈 → 9. Odin 탭·검증기 → 10. I2 Localization → 11. Smart Lighting 평가 → 12. 사운드 교체(구역 BGM·발소리·강화음)
- RPG Monster Pack·이미지 생성 작업은 이미지 작업 재개 때.
