# 퀄리티업 로드맵

> 목표: **이미 만들어진 기능의 완성도를 끌어올린다.** 새 콘텐츠보다 기존 시스템의 손맛·안정성·가독성을 먼저 높인다.
> 자동 실행 루틴은 TechDebt의 P0~P2를 먼저 처리하고, 그다음 이 문서를 위에서부터 진행한다. 끝난 항목은 ✅ + 커밋 해시를 적는다.

## 게임 방향 (사용자 확정 2026-10-18)

- **조작·이동:** 메이플식. 2단 점프, 대시, 사다리·로프, 하향 점프를 쓴다.
- **아이템:** 던파·디아식. 등급, 세트 효과, 파밍으로 성장한다.
- **핵심 구조:** **무기가 주는 스킬(4슬롯)이 메인 스킬**이 되고, **세트 효과가 그 스킬을 강화**한다(타격 수, 탄 수, 쿨감 등).
- **현재 우선순위:** 인벤토리 / 몬스터 AI / 플레이어 그래픽·애니메이션 / 이펙트의 퀄리티업.

## A. 전투 손맛 (Game Feel)

| # | 항목 | 수단 | 상태 |
|---|---|---|---|
| A1 | 데미지 공식 정상화(정수 나눗셈, 방무, 최대댐) | TechDebt B-101~104 | ✅ |
| A2 | 피격 플래시(흰색) + 히트스톱(0.03~0.06s) | `Combat/Feedback/SpriteHitFx` + `HitFeedback` (몬스터 ✅, 공격자 히트스톱·플레이어 틴트 ⏳) | 🔶 |
| A3 | 타격 시 카메라 흔들림 세분화(평타 약, 스킬 중, 치명 강) | `HitFeedback.Shake` 5단계 + 0.1s 창 스로틀 | ✅ |
| A4 | 사망 디졸브 + 드롭 아이템 등급별 빛 기둥(C/R/U) | 디졸브 ✅(1.2s~), 등급 아웃라인·글로우 ✅, 빛 기둥 파티클 ⏳ | 🔶 |
| A5 | 치명타 데미지 숫자 스타일 분리(색·크기·흔들림) | `HitFeedback.SpawnNumber`: 치명 주황 1.45배(프레임당 4개), 추가타 하늘색, 독 초록, 출혈 빨강 + 숫자 프리팹 풀링 켬 | ✅ |
| A6 | 스킬 이펙트 보강 | Magic Slashes FX / Particle Pack | ⏳ |

## B. 플레이어

| # | 항목 | 상태 |
|---|---|---|
| B1 | 상태 머신 정리: 문자열 state + bool 난립을 enum 기반 FSM으로 바꾼다(이동/점프/대시/사다리/공격/피격) | ⏳ |
| B2 | 무적·슈퍼아머·이동정지를 ref-count(토큰) 방식으로 바꾼다. 다른 출처가 서로의 효과를 끄지 않게 한다(B-114) — PlayerStatus + DB facade, 무적·슈퍼아머 ✅ / MoveLock·UI ⏳ | 🔶 |
| B3 | 입력 버퍼링(점프·공격 0.1s), 코요테 타임(0.08s) — 메이플식 반응성. 낙하 후 공중 점프 1회만(토글 `_fallConsumesGroundJump`) | ✅ (EditMode 테스트, 실제 키 입력 체감은 미검증) |
| B4 | 애니메이션 보강 (AnyPortrait): 대기·걷기·달리기 CrossFade 0.12s ✅, 공격 애니 속도=공속 배율 ✅, 공격자 히트스톱 ✅, 착지 스쿼시(메시만) ✅, 대시·착지 먼지 ✅(DustPuffs), 대시 잔상 ✅(DashAfterimage) |✅ |
| B5 | 스탯 시스템: static 필드를 Modifier 기반 스탯(장비=Source, 세트=Group, 버프=ID)으로 옮긴다 | ⏳ |

## C. 몬스터 AI

| # | 항목 | 상태 |
|---|---|---|
| C1 | 순찰 리쉬 수정, 도주 노드 종료 조건(B-110 외) | ⏳ |
| C2 | 공격 예고(텔레그래프): 공격 전에 0.3~0.5s 경고 표시(느낌표, 바닥 표시) | ⏳ |
| C3 | 추격을 DOMoveX(0.1s 텔레포트식)에서 속도 기반 이동으로 바꿔 부드럽게 | ⏳ |
| C4 | 보스 패턴 페이즈(HP 50%·25%) + 패턴 쿨다운 데이터화 | ⏳ |
| C5 | 몬스터 스탯을 CSV(`MonsterSheet.csv`)로 옮긴다. HP/Def/공격력/드롭 테이블 데이터화 | ⏳ |

## D. 인벤토리 · UI

| # | 항목 | 상태 |
|---|---|---|
| D1 | 수량 캐시 불일치·복제·소실 버그(B-003~006) | ⏳ |
| D2 | InventoryManager(2000줄) 분리: 데이터 모델(InventoryModel) / 뷰 / 입력 | ⏳ |
| D3 | 아이템 툴팁: 등급 색, 세트 효과 진행도(2/5), 비교(착용 중 vs 선택) — 디아식 | ⏳ |
| D4 | 정렬, 자동 정리, 15칸 → 25칸 확장, 페이지 탭 | ⏳ |
| D5 | 장비창에 세트 효과 활성 표시 + 스킬 4슬롯 UI 연결 | ⏳ |
| D6 | 픽업 알림(좌측 하단 로그), 희귀 드롭 강조 | ⏳ |

## E. 콘텐츠 · 에셋 연결

| # | 항목 | 상태 |
|---|---|---|
| E1 | Rat / Scare 세트 효과 정의(ItemSpec 제안안) | ⏳ |
| E2 | SideRoad 포털 연결(B-111) | ⏳ |
| E3 | BGM 추가: 마을 / 사냥터 / 밤 / 보스 (보유 음악팩) — `Audio/BgmDirector` + MasterAudio 재생목록 Town/Field/Night/Boss/Victory | ✅ (승리 팡파레 연결 ⏳) |
| E4 | Lv2 사냥터 + 신규 몬스터 + 세트 — 맵 ✅, 몬스터 6종 ✅, 아이템 24·Cliff 세트 2/4/6·드롭 10 ✅, 제작 레시피 ⏳, 스킬 이펙트 교체 ⏳ | 🔶 |

## F. 성능 · 코드 품질

| # | 항목 | 상태 |
|---|---|---|
| F1 | Update 내 할당·조회 제거(P3 목록), Debug.Log 정리 — 대기 애니 재호출 캐시, 바닥 레이캐스트 마스크 캐시, 드롭 공유 풀, 풀 유지(맵 재진입 재생성 제거), 워밍업 프레임 분산 | 🔶 |
| F2 | 텍스처 압축/아틀라스 경고 정리 (도트는 Point + 무압축) | ⏳ |
| F3 | 오타 이름 정리(FormerlySerializedAs) | ⏳ |
| F4 | EditMode 테스트: 데미지 공식 ✅, 세트 판정, 인벤토리 스택 (Unity Test Framework, `Assets/Editor/Tests`) | 🔶 |

## H. 빠져 있던 기본 기능

| # | 항목 | 상태 |
|---|---|---|
| H1 | 플레이어 사망·부활: HP 0 → 입력 잠금·무적 → 2.5초 뒤 마을 부활(HP·스태미나 가득, 2초 무적). 기존엔 매 프레임 로그만 찍고 아무 처리 없음 | ✅ |

## G. 보유 에셋 실사용 완성 (임포트에서 끝내지 않는다 — 사용자 요청 2026-10-18)

| # | 에셋 | 사용 단계 | 상태 |
|---|---|---|---|
| G1 | All In 1 Sprite Shader | 피격 플래시·디졸브·드롭 등급 외곽선 | ✅ |
| G2 | 음악팩 2종 | 상황별 BGM(마을/필드/밤/보스), 보스 처치 승리 팡파레 | 🔶 (팡파레 ⏳) |
| G3 | Monsters Creatures Fantasy / Evil Wizard 2 / Bringer | Lv2 몬스터·엘리트·보스 프리팹(`Editor/Dev/MonsterBuilder`, `Tools/probes/build_lv2_monsters.cs`) | ✅ 6종 생성(맵 배치는 G4) |
| G4 | Magic Cliffs | Lv2 맵 `Editor/Dev/Lv2MapBuilder`(코드 레이아웃 재생성): 바닥·벽 타일, 원웨이 발판 10, 로프 10, 배경 4층, 스포너 10, SideRoad 왕복 포털, 카메라 경계·`MapInfo` | 🔶 (드롭 테이블 5.6, 아트 다듬기 ⏳) |
| G5 | Odin Inspector | `Editor/Data/ProjectBDataWindow`(메뉴 ProjectB/데이터 편집기): 아이템 트리·표 편집(드롭다운·쉼표 검증·아이콘 미리보기), 세트 효과 해석 미리보기, 몬스터 스탯, 검증 리포트, CSV 저장(형식 보존) + DataIntegrityTests | ✅ |
| G6 | 8-Bit Sfx / Frost Mage SFX / SoundBits | `Audio/Sfx` + `Resources/Audio/SfxLibrary`(14종): 타격·치명·처치·보스 승리·픽업·희귀 드롭·제작 성공/실패·인벤 열기·포털·사망 | ✅ (SoundBits는 미사용) |
| G7 | Free Pixel Font Thaleah | `ThirdParty/Font/ThaleahFat_TMP`(비트맵·Point) → 데미지 숫자 프리팹 3종 적용, 크기 상향(일반 1.35·치명 1.9) | ✅ (영문 UI 적용 ⏳) |
| G8 | Magic Slashes FX / Particle Pack | ✅ **도트화(GameFeelPlan 10.5): PixelVfxBaker로 8종 도트 베이크, Vfx가 도트로 재생** · `Vfx` 풀(프레임당 6개) + `Resources/FX/VfxLibrary`(3D 베기를 X-90°로 눕혀 2D화) · 평타 `Weapon.swingVfx` · 스킬 `Skill.castVfx`(Cliff/Abyss 무기 테마, `Lv2ItemBuilder.ApplyVfx`) · 치명 X자(CritCross) · 적중 스파크 `HitSparks`. `DustPuffs` 대시·착지 먼지 ✅ · `DropLightPillar` R 파랑/U 금색 빛 기둥 ✅· `DashAfterimage` 대시·스킬 대시 잔상(AnyPortrait 메시 33개 복사, 풀링) ✅ | ✅ |
| G9 | Text Animator | ✅ 획득 로그 `PickupLog`(+순수 `PickupLogModel`, 테스트 4개): 좌하단 5줄·1.5초 병합·R/U `<wave>`+글로우·인벤 가득 경고. ⏳ 대사(Quest Machine G11과 함께)·세트 아이템 강조 | 🔶 |
| G10 | RPG Monsters Icon | 몬스터 도감·픽업 로그·보스 HP바 초상 | ⏳ |
| G11 | Quest Machine (+Dialogue System) | 마을 NPC 퀘스트(사냥·수집·제작) | ⏳ |
| G12 | Feel | (보류) 스킬별 연출이 필요해지면 MMF_Player | 보류 |
