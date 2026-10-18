# TO THE BEYOND — 세계관 · 탑 구조 · 시스템 매핑

> 2026-10-18 사용자 정의. 게임 제목 **TO THE BEYOND**. 장르는 **핵앤슬래시**(메이플 + 디아블로). 로그라이크 기획은 **무시**한다.
> 확정되지 않은 판단은 `Docs/OpenQuestions.md` Q-4xx(임시 가정)에 둔다. 스토리 초안(구버전 포함)은 사용자 원문을 요약했다 — 대사·설정은 변경 가능.

## 1. 한 줄 요약

정상에 오르면 모든 것을 이루어 준다는 **탑**을, 아버지를 찾는 소녀가 오른다. 원하는 아이템을 드롭하는 적을 **오래 사냥**하며 재화·장비를 모아 **강화·세팅**하고 **위 층계로** 올라간다(던파 하늘성 + 더 넓은 범위).

## 2. 스토리 (현행 채택안)

- **배경:** 탑은 어느 날 나타났다. 정상에 오른 자는 진리·보화·불멸을 얻고 원하는 바를 이룬다는 소문.
- **진실(후반 공개):** 탑은 이계가 이 세계의 대지의 기운을 정제·송출하기 위해 세운 장치. 소원은 실제로 이루어지지만, 정상에 오른 자는 **탑의 방어 시스템(문지기)으로 귀속**된다.
- **주인공:** 병을 앓던 소녀. 아버지가 소녀를 치유하려 탑에 올랐고, 소원으로 소녀는 나았다. 아버지는 문지기가 되었다.
- **브로치:** 아버지가 남긴 브로치. 아버지는 "브로치를 지닌 자는 위협으로부터 지켜진다"는 소원도 빌었다 → **사망 시 브로치가 빛나며 마을로 귀환**(현행 사망→마을 부활을 설정으로 정당화).
- **결말:** 정상에서 아버지(최종 문지기)와 전투. 아버지는 브로치를 보고 딸을 알아본다. 선택: ① 아버지와 함께 영원한 문지기 ② 탑의 시스템 붕괴(진엔딩).
- **보조 테마(선택 사항, Q-403):** "탑을 오르려는 의지가 꺾인 자는 결격자들의 도시로 떨어진다", 층계마다 '왜 오르는가(약속/자유)' 질문을 던지며 성장하는 이야기.

## 3. 층계 (7층계, 외곽 → 정상)

| 층계 | 테마 | 몬스터 | 모험가 칭호(안) | 현행 콘텐츠 매핑 |
|---|---|---|---|---|
| 1 | 숲 | 산적·동물, 요정, 골렘(유적) | 서광의 모험가 | **외곽 숲 = 현재 Lv1 NearForest/SideRoad** |
| 2 | 부유성 · 언더 클라우드 | 기사·석상(던파 4기사), 용 | 운해의 모험가 | **현재 Lv2 마법 절벽 = 2층계 입구(임시, Q-402)** |
| 3 | 바다 · 인클루드 클라우드 | 심해·해저, 물고기 | 심록의 모험가 | 미구현 |
| 4 | 오버 클라우드 | 불·대장장이, 용암·마그마, 기계 | 홍염의 모험가 | 미구현 (탐험가 임시 캠프 설정 가능) |
| 5 | 라비린스(미궁) | — | 흑석의 모험가 | 미구현 |
| 6 | 어비스 | 크툴루·요괴·악마·뱀파이어 | 심연의 모험가 | 미구현 (AbyssWizard 콘셉트와 연결 가능) |
| 7 | 창조주의 층계(엔드 오브 타워) | 세라믹 + SF 천사 | 백광의 모험가 | 미구현 |

- 층계 생태계(구버전 설정, 참고): 운해의 물 → 흑철에서 정제 → 미궁이 모험가 차단 → 공허에 폐기 → 창조 층계에 에너지 집적.
- 칭호 0단계 = **견습 모험가**(시작).

## 4. 1층계 — 숲 (바운더리) 6구역 (사용자 정의 2026-10-18)

| 구역 | 설명 | 낮 | 밤 | 보스 | 현행 매핑 / 에셋 |
|---|---|---|---|---|---|
| ① 숲 근교(풀밭) | 마을 앞. **파괴 가능 오브젝트 ✅**(`BreakableProp`, 나무통·상자 도트 `gen_props.py`, 2타 파괴, 나무 80%/돌 35%/약초 30% 드롭, 90초 리스폰, `setup_props.cs` 로 9유닛 간격 배치) | 토끼, 광물토끼(공격, 추격 X), 풀두더지·묘목 두더지(마을 앞) | 쥐, 거대쥐 (or 허수아비) | (PassiveTerror 야간, 임시) | ✅ **현재 Lv1 NearForest + SideRoad** (Rabbit/StoneRabbit/FlowerMole/LeftMole/RatThief/RatChief/Scarecrow) |
| ② 일반 숲 | 탑으로 가는 숲의 초입 | 늑대, 멧돼지, 꽃 몬스터(뿌리 공격), 탄환 공격형, 버섯류 | 위습, 달팽이, 거미, 뱀, 쌍두 | — | 버섯=SporeShroom, 뿌리 공격=FlowerMole BT 재사용 가능. 늑대·멧돼지·위습 등 **스프라이트 없음** |
| ③ 깊은 숲 | 탑 직전의 깊은 숲(유적) | 곰, 고블린, 엔트, 버섯 | 붉은 탐험가(적대 APC, 난입), 유령, 요정 | **고대숲의 엔트** | 고블린=CliffGoblin ✅, 버섯 ✅, 붉은 탐험가=SPUM/Pixel Hero Maker(보유), ✅ **`Lv1_DeepForest` 1차 구현(13절)**: 곰·엔트·유령·요정·고대숲의 엔트 코드 도트. 요정여왕 방(중간보스, 구글 문서) |
| ④ 늪지 | 탑의 그늘에 가려진 축축한 늪 | 개구리, 큰 모기, 슬라임, 등에 꽃이 핀 꽃게, 버섯, 악어 | (미정) | **거대 슬라임** | 스프라이트 없음 |
| ⑤ 산적 성채 | 깊은 숲 뒤 늪을 지나 | 산적(단검·활·방패) | 동일 | **산적 대장** | SPUM(보유)으로 제작 가능, 대장=Medieval King Pack 2/Martial Hero(보유) |
| ⑥ 탑 근교 | 골렘들이 있는 곳 | 골렘(소·중·대) | 골렘 + 유적 정령 | **탑의 문지기** | ✅ **`Lv1_TowerOutskirts` 1차 구현(16절)**: 골렘 3종·유적 정령·문지기 코드 도트 (보스방 레퍼런스: 이끼 낀 거대 석상 유적 — 전용 배경 ⏳) |

- 구글 문서 "1층계 맵 레퍼런스": 얕은 숲 식생(풀·버섯·잡풀·예쁜 풀·이상한 나무) + **파괴 가능 오브젝트(나무통·상자)**, 요정여왕의 방(반딧불 숲, 맵 재사용 + 배경 레이어·식생 추가), 깊은 숲(숲 + 돌 유적), 1층계 보스방(웅장한 골렘 유적, 배경 신규).
- 진행: ① → ② → ③(→ ⑤ 산적 성채 분기) → ④ → ⑥ 탑 근교 → 탑 입구(2층계).
- 현재 Lv2 '마법 절벽'의 고블린·버섯은 ③ 깊은 숲 몬스터와 겹친다 → 마법 절벽 맵은 2층계 입구로 두고(Q-402), 몬스터는 ③에서 재사용.
- **부족한 몬스터 스프라이트**(늑대·멧돼지·위습·달팽이·거미·뱀·쌍두·곰·엔트·유령·요정·개구리·모기·슬라임·꽃게·악어·골렘): 보유 팩 없음 → 이미지 생성(GPT, 사용자 허용) 또는 절차 도트화 파이프라인으로 제작 (Q-405).

## 5. 시스템 매핑 (기획 용어 ↔ 구현)

| 기획 | 의미 | 구현 |
|---|---|---|
| 형체 코어(베이스 무기) | 무기가 전투 스타일·스킬 세트를 통째로 바꿈 | ✅ 무기마다 스킬 A/S/D/F (`Weapon`/`Skill`) |
| 층계 결정 | 탑의 특수 결정, 코어 강화·스킬 개방 | ✅ 강화석 `Enhance Stone` → **표시 이름 '층계 결정(Floor Crystal)'** (`EnhanceRules`) |
| 힘의 편린 | 패시브 아이템(능력치·세트 효과·특수 능력) | ✅ 방어구·장신구 + 세트 효과 / ⏳ 특수 능력 패시브 |
| 아티팩트 | 액티브 아이템(Shift 발동: 데미지·회복·무적·시간정지) | ✅ `ArtifactSystem` — Shift 사용·Tab 교체, 해금 목록 ES3(artifact.unlocked/equipped). 이슬(회복 35%, 시작 보유)·수호 껍질(무적 3초, PassiveTerror 첫 처치)·모래시계(시간 0.2배 4초, AbyssWizard 첫 처치)·별 폭발(반경 4 피해+넉백, 두더지 퀘스트). HUD 칩 + 쿨다운 |
| 브로치 해방 | 영구 성장(마을 연구실), 층계 보스 처치 시 스토리 해방 | ⏳ 신규 (사망 귀환 = 브로치 설정) |
| 모험가 칭호 | 층계 진행도 | ⏳ HUD 표시 (퀘스트 체인 진행으로 판정) |

## 6. 반영 우선순위 (자율 작업)

1. ✅ 문서화 + 용어(층계 결정) + 지역 배너 + 타이틀 오버레이(`TitleScreen`, PRESS ANY KEY, 자동화는 `dismiss_title.cs`/u.sh 가 자동 해제).
2. 1층계 1-1 보강: ✅ 파괴 가능 오브젝트(나무통·상자), ✅ 밤 반딧불(`AmbientFireflies`, `MapInfo.fireflies`), ✅ 식생 장식 190개(`gen_vegetation.py`: 풀 3·버섯 2·꽃 2·이상한 나무, `setup_vegetation.cs` 배치, 콜라이더 없음).
3. 1-2 요정여왕(중간보스) · 2-1 깊은 숲 · 2-2 골렘 보스 — 보유 에셋으로 가능한 범위부터.
4. 아티팩트(Shift 액티브), 브로치 해방(영구 성장), 모험가 칭호.

## 7. 상승 부하 · 탐험가 칭호 (사용자 2026-10-18: "메이드 인 어비스의 상승부하처럼, 위로 갈수록 부하, 내려오는 건 자유")

- 세계관: 메이드 인 어비스 느낌(탐험가 · 칭호 · 상승 부하 · 유물). 플레이: 메이플 + 디아블로.
- **층계 부하 L**: `MapInfo.ascentLoad`. 1층계 외곽 0 → (1층계 깊은 구역 5~20) → 2층계 입구 15(현 마법 절벽, 임시) → 층계마다 상승.
- **부하 저항 R** = 탐험가 칭호 × 15 (+ 장비 옵션 '상승 저항' · 브로치 해방, 이후).
- **부족분 D = max(0, L − R)** → 주는 피해 ×(1 − 0.02·D, 최소 0.1), 받는 피해 ×(1 + 0.02·D). 메이플 아케인 포스와 같은 성장 목표. 아래층으로 내려가면 즉시 해제(현재 맵의 L만 적용).
- **탐험가 칭호**(어비스의 피리 등급처럼): 0 견습 Novice → 1 서광 Dawn → 2 운해 Cloudsea → 3 심록 Deepgreen → 4 홍염 Crimson → 5 흑석 Blackstone → 6 심연 Abyss → 7 백광 Whitelight. 층계 보스 처치로 승급(`BossReward.grantsRank`: **탑의 문지기=1**(1층계 졸업, 2026-10-19 PassiveTerror 에서 옮김, Q-413), AbyssWizard=2(임시)). ES3 `explorer.rank`.
- HUD: 좌상단 "RANK · LOAD/RES" (부족하면 붉게). ✅ 구현: `AscentLoad`(순수 계산+ES3 `explorer.rank`), `AscentHud`, `MapInfo.ascentLoad`(외곽 0 / 샛길 5 / 마법 절벽 25), `BossReward.grantsRank`(Gatekeeper=1, AbyssWizard=2). 피해 훅: `EnemyHealth`(주는 피해), `PlayerHealthManager`(받는 피해).

## 8. 가벼운 추출(타르코프) 요소 — 제안(임시 가정 Q-406)

- 세트·강화 장비(장착품)는 **안전**: 브로치가 지키는 것은 몸과 장착품.
- 이번 탐사에서 주운 **탐사 가방** 내용(재료·층계 결정·유물)은 사망 시 일부 손실, 마을/층계 캠프로 **귀환**해야 확정.
- **유물**(어비스의 유물처럼): 높은 층계일수록 희귀·고가. 판매·브로치 해방·제작 재료. 부하가 큰 곳일수록 위험·보상 증가.

## 9. 사냥 루프 보조

- **연속 처치 콤보**(`KillCombo`, 메이플 콤보 킬): 3초 창, 우상단 "x N COMBO" + 게이지, 5/10/20 콤보에 드롭률 +5%씩(최대 +20%, 끊기면 회수).
- **층계 지도 / 웨이포인트**(`WaypointMap`, M 키): 방문한 구역(ES3 `zones.visited`)을 층계별로 나열, 선택 시 `MapInfo.GoToMap`으로 즉시 이동. 미방문은 ??? 잠금. 구역마다 부하/저항/부족분 표시.


## 10. 주인공 아트 방향 (사용자 2026-10-18: "나중에 캐릭터도 이런 스타일로 바꾸고 싶다")

- **참고 이미지(대화로 받음, 파일 없음):** 등신이 높은(약 6~7등신) 애니풍 도트. 금발 포니테일, 파란 눈, 베이지 튜닉 + 갈색 벨트, 어깨에 걸친 짙은 망토/코트, 허벅지 스트랩, 검은 부츠. 외곽선은 짙은 적갈색, 명암 2~3단. 대략 64×112px급 해상도.
- **현재:** AnyPortrait 리깅 캐릭터(SD 2~3등신, 고글 모자·적발). 애니(평타·스킬·사다리·대시 등)가 전부 AnyPortrait 기준이다.
- **[임시 가정/추천] (Q-408):** 1단계는 **AnyPortrait 리그 유지 + 파츠 텍스처를 도트로 교체**한다(머리·몸통·팔·다리·망토를 레퍼런스 스타일로 다시 그려 메시에 입힘, Point 필터). 등신이 바뀌므로 본 길이·소켓(무기)·히트박스를 재조정한다. 기존 애니 클립을 재사용할 수 있어 비용이 가장 낮다. 프레임 스프라이트시트 방식(애니 15종 × 6~10프레임)은 품질이 높지만 비용이 커서 2단계 후보로 둔다.
- **선행:** 무기 공격·콤보 정리(WeaponReview) 후. 파츠 시안은 이미지 생성(GPT) → 도트 정리(Tools/pixelfx) 파이프라인으로 만든다.

## 11. 1층계 ②구역 일반 숲 — 1차 구현 (2026-10-19)

- 맵 `Lv1_NormalForest`: 외곽 샛길 지형 복제 + 짙은 숲 색조(타일 0.72/0.86/0.72), 상승 부하 10. 입구는 샛길 오른쪽 끝 `Portal_ToNormalForest` ↔ `Portal_ToSideRoad`(x −262). 보스 스포너 끔. 프로브 `build_normal_forest`, `tune_normal_forest`.
- 몬스터(코드 도트 `Tools/pixelfx/gen_forest_mobs.py` → `Tools/probes/build_forest_mobs.cs`): 낮 숲늑대(140HP)·멧돼지(220HP)·포자버섯(Lv2 재사용), 밤 도깨비불(90HP, 파란 Light2D)·숲달팽이(180HP 방어 10). MonsterSheet 행 추가.
- ChatGPT 로그인이 브라우저에 없어 GPT 생성 대신 코드 도트로 그렸다(Q-405 파이프라인은 로그인 시 재개). ⏳ 전용 지형 레이아웃, 꽃뿌리·탄환 몬스터·거미·뱀, 드롭 테이블(현재 원본 스포너 드롭 유지).

## 12. 핵심 설계 원칙 — 몬스터 계보 장비 (사용자 2026-10-19)

> 사용자: "플레이어가 쓰는 무기나 이펙트들이 결국에는 잡은 몬스터와 비슷하다. 예를 들어 허수아비 적을 죽여서 나오는 지팡이가 허수아비를 소환한다. 몹이 쓰는 스킬을 무기가 본떠서 혹은 강화해서 쓰는 경우가 많다. 세트 효과는 더욱 그렇다. 모든 무기가 그런 건 아니지만 대부분. 유니크·레전더리부터 특색 있는 무기가 추가된다."

- **규칙:** 몬스터 1종(또는 구역 무리)마다 그 몬스터의 **외형·공격 패턴을 가져온 무기**와 **세트(방어구)** 를 만든다. 무기 스킬 = 몹 공격의 플레이어판(강화판). 세트 효과 = 그 몹의 특성(무리·돌진·발광·껍데기 등)을 수치·스킬 강화로.
- **등급:** 일반~레어 = 계보 장비(위 규칙). 유니크·레전더리 = 계보를 벗어난 특색 무기(보스·정예 천장 보상, ProgressionDesign).
- **기존 예:** 허수아비 → ScareStaff(허수아비 소환)·ScareSide·ScareClow + ScareArmor 세트 / 쥐 → RatCheifDagger + RatArmor(독·치명) / 잎두더지 → LeafStaff(Rising Roots·Scattering Leaves) + LeafArmor / 절벽 → Cliffwind 무기 + CliffArmor.
- **신규 몹을 추가할 때 체크리스트:** ① 몹 공격 1~2개 → 무기 스킬 A/S/D/F 후보 ② 드롭 무기 1종 이상 ③ 세트 부위(2·4·5세트 효과) ④ 아이콘·이펙트 색은 몹 팔레트 재사용.

### 12.1 ②구역 일반 숲 계보안 (구현 대기)

| 몹 | 무기(드롭) | 무기 스킬 (몹 공격의 플레이어판) | 세트 | 세트 효과 |
|---|---|---|---|---|
| 숲늑대 | **Fang Dagger** (쌍단검) | A 송곳니 연타(3타 돌진), S **늑대 소환**(무리 1~2마리, 몹의 무리 습성) | WolfArmor | 2: 이동속도·치명 / 4: 소환 늑대 수+1, 치명 피해 |
| 멧돼지 | **Tusk Knuckles** (너클) | A **돌진 박치기**(몹 돌진 그대로, 넉백 큼), S 엄니 올려치기(공중 띄움) | BoarArmor | 2: 방어·HP / 4: 돌진 거리·슈퍼아머 |
| 도깨비불 | **Wisp Lantern** (지팡이) | A 유도 도깨비불 탄, S 도깨비불 3개 공전(발광) | WispArmor | 2: 밤에 공격력↑ / 4: 공전 도깨비불 +2, 밤 시야(광원) |
| 숲달팽이 | **Shell Shield** (검+방패) | A 껍데기 방패 막기(받는 피해↓), S 점액 장판(둔화) | SnailArmor | 2: 방어↑ / 4: 막기 중 반사 피해 |

- ✅ **Fang Dagger** (2026-10-19, `build_fang_dagger.cs`): DoubleDagger 기반 9~15, F 스킬 Howl of the Pack(쿨 8s, 선딜 0.3s) → `SummonRunner` 영혼 늑대 2마리(늑대 Run 프레임·파란 발광, 무기 피해 60%로 0.35s마다 물기). 늑대 드롭 2%, 재료 Wolf Fang/Boar Tusk/Wisp Ember/Snail Shell(ItemSheet 106~110, `gen_forest_items.py` 아이콘). 일반 숲 드롭 테이블 교체. ⏳ WolfArmor 세트.
- ✅ **Tusk Knuckles** (`build_tusk_knuckles.cs`): Leather Knuckles 기반 12~18, Boar Charge·Tusk Uppercut 판정 복제본 넉백 ×2·경직 0.4s, 상아 톤, 멧돼지 드롭 2%(ItemSheet 111).
- ✅ **Wisp Lantern** (`build_wisp_lantern.cs`): ScareStaff 기반 11~19, A Wisp Bolt(유도 도깨비불 3발 `HomingOrb`, 판정 중심 조준, 쿨 1.2s), S Ghostlight Ring(공전 3개 `OrbitWisps` 6초, 쿨 10s), 보이지 않는 작은 판정 `WispTouch`. 몬스터 목록 `EnemyHealth.Active`·`IsMonster`(채집물 제외) 추가. 도깨비불 드롭 2%(ItemSheet 112).
- ✅ **Shell Shield** (`build_shell_shield.cs`): RustedSwordandShield 기반 10~16, Shell Bash 넉백 ×1.5, Shell Guard 홀드 방어 버프 100→180·쿨 6s, 보라 껍데기 톤, 달팽이 드롭 2%(ItemSheet 113). ⏳ 점액 장판(둔화) — 몬스터 둔화 시스템이 없어 보류.
- ✅ 세트 2종(구역 낮/밤 묶음, `make_forest_sets.py` + `build_forest_sets.cs`): **Pack Hunter**(WolfArmor 회색: 늑대·멧돼지 + Fang Dagger·Tusk Knuckles) 2: 이동 8·치명 5 / 4: 치명피해 15·Howl 쿨 −40·Boar Charge −30 / 6: 공속 10·피해 10·HP 50. **Night Lantern**(WispArmor 파랑: 도깨비불·달팽이 + Wisp Lantern·Shell Shield) 2: 방어 12·HP 60 / 4: 피해 8·Ghostlight Ring −40·Shell Guard −30 / 6: 방어 20·치명피해 15·Wisp Bolt −40. ItemSheet 114~123, 부위 드롭 각 0.3%. 프로브 `equip_wolf_set_test`(6단계 수치 확인). 아이콘·착용 텍스처는 Leaf 색 변환 임시.
- 구현 순서(자율): Fang Dagger(늑대 소환은 ScareStaff 소환 경로 재사용) → Tusk Knuckles(SkillDash 재사용) → Wisp Lantern → Shell Shield. 아이콘·무기 스프라이트는 `gen_forest_mobs.py` 팔레트로 그린다.

### 12.2 ③구역 깊은 숲 계보 (2026-10-19)

| 몹 | 무기(드롭) | 무기 스킬 (몹 공격의 플레이어판) | 세트 | 상태 |
|---|---|---|---|---|
| 숲곰 | **Bear Claw** (갈퀴, ScareClow 기반 13~20) | A Mauling Rush(돌진 할퀴기 판정 복제본 넉백 1.5배·경직 0.5s·피해 1.3배), S Rending Upswipe(같은 강화), D Bear Roar(포효=공속 버프) | Ancient Grove | ✅ 곰 드롭 2% |
| 엔트 | **Root Staff** (지팡이, LeafStaff 기반 14~22) | S **Ancient Roots** = Rising Roots 강화: 뿌리 1.35배 크기 `DeepRoot`(피해 2.6·3타·넉백 14), 기본 5줄기(최대 8, 원본 2), 쿨 2s / A Root Step(순간이동), D Sap Mend(치유) | Ancient Grove | ✅ 엔트 드롭 2% |
| 고대숲의 엔트(보스) | **Heartwood Greatsword** (유니크 대검, SacreGreatSword 기반 22~32) | D **Ancient Wrath**: 1.9배 거대 뿌리 `GiantRoot` 8줄기가 0.08초 간격 파도로 솟는다(피해 3.2, 쿨 9s) — 보스 뿌리 폭발의 플레이어판 | — | ✅ `Resources/Boss/AncientEnt_BD` 고유템 8%·천장 12, 확정 1개는 Bear Claw/Root Staff |
| 유령 | **Phantom Scythe** (낫, ScareSide 기반 14~22) | A **Phantom Reap** = 관통 베기(최대 8명·다 맞혀도 안 사라짐·피해 1.15배), D **Phantom Glide** = 순간이동 베기 + `PhantomVeil`(무적 0.45s·잔상), S Spectral Eruption·Wraith Whirl(유령빛 반투명 톤) | Will-o'-Fae | ✅ 유령 드롭 2% |
| 요정 | **Fae Wand** (지팡이, ScareStaff 기반 12~20) | A **Fairy Dust Bolt** = 분홍 요정 가루 유도탄 4발(`HomingOrb` 재사용, 무기 피해 42%), S **Fae Blessing** = 분홍 회복 장판(HealGround 복제, 원본 100틱의 60%, 쿨 12s), D Starlight Lance(분홍 톤) | Will-o'-Fae | ✅ 요정 드롭 2% |

- 프로브 `build_deep_forest_weapons.cs`(판정 복제·아이콘 임포트·보스 보상), 아이콘 `Tools/pixelfx/gen_deep_forest_items.py`(몹 팔레트 재사용), ItemSheet 124~130.
- 검증 `deep_forest_weapon_test.cs`: Ancient Roots 판정 3개 동시(원본 Rising Roots 1개), Ancient Wrath 5개, Mauling Rush 1개. combo_all 3종 정상(1-2 콤보 순환).
- **2차 (2026-10-19)**: 유령·요정 무기는 `build_deep_forest_weapons2.cs`(판정 복제본 `DamageObject/PhantomScythe`·`FaeWand`, 원본 공유 프리팹 미수정), 아이콘·요정 가루 프레임 `Tools/pixelfx/gen_deep_forest_gear2.py`. Fae Blessing 피벗은 LeafStaff 회복 스킬과 같은 경로(ScareStaff 피벗은 앞쪽이라 플레이어와 안 겹쳐 회복이 안 됐다).
- **세트 2종** (`make_forest_sets.py` Grove/Fae 색 변환 + `build_deep_forest_sets.cs`, ItemSheet 131~142, 부위 드롭 0.3%):
  - **Ancient Grove** (GroveArmor 갈색·이끼, 낮 곰·엔트 + Bear Claw·Root Staff): 2: 방어 15·HP 80 / 4: 피해 10·Ancient Roots 쿨 −40·Mauling Rush −30 / 6: 공속 10·치명피해 20·HP 120. 부위 방어 18/32/18/14/14.
  - **Will-o'-Fae** (FaeArmor 보라·분홍, 밤 유령·요정 + Phantom Scythe·Fae Wand): 2: 이동 8·치명 6 / 4: 치명피해 12·Phantom Glide 쿨 −40·Fairy Dust Bolt −40 / 6: 피해 12·공속 10·드롭률 8. 부위는 방어 낮고 치명·이동 높음.
- 검증(플레이): `equip_deep_forest_set_test.cs`(Grove/Fae 6부위 → 스택 6, 세트 수치·쿨감 키 일치), `skill_trace.cs`(Phantom Glide 13유닛 순간이동 + 무적 0.4s, Phantom Reap 판정, Fairy Dust Bolt 유도탄 4발, Fae Blessing HP 40→55/1.6s), combo_all(Phantom Scythe 1-2 순환 = ScareSide 와 같음, Fae Wand 1단 = ScareStaff 와 같음), 콘솔 에러 0.

## 13. ③구역 깊은 숲 — 1차 구현 (2026-10-19)

- 맵 `Lv1_DeepForest`: 쓰지 않던 원본 근교 지형 `Lv1_NearForest`(x −437~−137) 복제, 돌 유적 색조(타일 0.5/0.62/0.64, 원경·식생 청록 회색), 상승 부하 20, zoneTitle "Deep Forest", 반딧불. 프로브 `build_deep_forest.cs`(재실행하면 복제본을 지우고 다시 만든다).
- 포털: 일반 숲 왼쪽 끝 `Portal_ToDeepForest`(x −722, 원래 Lv2 포털 자리) ↔ 깊은 숲 오른쪽 `Portal_DeepToNormal`(x −236). 깊은 숲 왼쪽 끝은 마을 포털 유지(보스 처치 후 귀환).
- 몬스터(코드 도트 `Tools/pixelfx/gen_deep_forest_mobs.py` → `Tools/probes/build_deep_forest_mobs.cs`, MonsterSheet 행 추가):
  - 낮: **숲곰** 400HP 방어 8(64px, RatThief 템플릿, 일어섰다 앞발 내려찍기) · **엔트** 500HP 방어 10(64px, LeftMole 템플릿=느림, 가지 팔 내려찍기 + 앞 땅에서 뿌리) · 고블린(CliffGoblin) · 포자버섯(SporeShroom) 재사용.
  - 밤: **유령** 160HP(48px, FlowerMole 템플릿, 반투명 72%, 약한 Light2D) · **요정** 110HP(작은 몸, 날개 펄럭, 따뜻한 Light2D).
  - 보스: **고대숲의 엔트** 6000HP 방어 18 슈퍼아머(엔트를 1.5배 좌표로 96px 재작화, 이끼·돌빛 수피·주황 눈·분홍 꽃, Attack2 = 두 팔 들어 넓은 뿌리 폭발), PassiveTerror 템플릿, 18px/유닛(일반 몹보다 1.33배). 낮 스포너, 스폰 지점 x −412, **스포너 영역은 맵 전체(x −470~−130, 양끝 벽)** — 영역 밖으로 끌어내 사라지게(체력 리셋) 하는 공략 방지. 스포너 이름은 몹 이름(Bear1·Ent1·Ghost1·Fairy1…)으로 바꿨다. BossIntro 연출·HP바·초상(RPG Monsters Icon forest_409) 자동.
- 드롭: 곰 Bear Pelt 35%·Bear Claw 2% / 엔트 Heartwood Chip 35%·Root Staff 2% / 유령 Ectoplasm 30% / 요정 Fairy Dust 30% / 고블린·버섯 재료 소량 / 보스 스포너 Heartwood Chip 확정 + 보스 확정 보상.
- 버그 수정 B-117: 스포너 트리거가 겹친 곳에서 다른 스포너 경계를 넘은 몬스터가 사라지던 문제(보스가 등장 직후 증발) — 자기 소속일 때만 사라지게.
- 검증(플레이): 낮 곰 4마리·보스 등장(HP바·초상·이름), 보스 처치 → 배너·확정 보상 드롭(Bear Claw + 강화석, 첫 처치 보너스), 밤 유령 5·요정 5, 포털 왕복(TravelNow), 콘솔 에러 0(테스트용 무적 잠금 경고 제외). 테스트로 생긴 보스 처치 ES3 키는 지웠다.
- ✅ **보스 공격 판정 교체 (2차)**: PassiveTerror 판정(갈퀴 ClowAttack·불꽃 PassiveTerrorAttackPrefab)을 복제한 `Prefab/Enemy/EnemyDamge/AncientEnt/AncientEntRootSwipe`(땅에서 뿌리 다섯 가닥이 솟는 근접, 64px×6 @16ppu = 원본과 같은 4유닛)·`AncientEntRootThorn`(이끼 낀 뿌리 가시 3발 투사체, 32px×3 @12.5ppu). 원본 컨트롤러 + AnimatorOverrideController 로 클립만 교체(키 시간 유지). 보스 스포너 objectInfos 키 `clow`/`fire` 유지, 보스 BT 의 damageOb 참조도 새 GUID 로. 아트 `gen_deep_forest_gear2.py`, 프로브 `build_ent_boss_roots.cs`, 감시 `boss_attack_watch.cs`(종류별 첫 등장 스크린샷). 플레이에서 두 판정 모두 발동 확인.
- 테스트 팁: 비활성 맵 포털에 `TravelNow` 를 직접 부르면 `Portal.Awake` 가 돌지 않아 PoolCheck 가 비고 스포너가 안 켜진다 → `goto_map.cs`(MapInfo.GoToMap) 를 쓴다. 낮 전환은 `set_day.cs`.
- ⏳ 남은 것: 돌 유적 전용 소품·배경(현재 색조만), 붉은 탐험가(적대 APC 난입), 요정여왕 방(중간보스), 지형은 근교 원본 그대로, 세트 착용 텍스처는 Leaf 색 변환 임시, Phantom Glide 반투명은 대시 잔상(하늘색 고정)으로 대신함.

## 14. ④구역 늪지 — 1차 구현 (2026-10-19)

- 맵 `Lv1_Swamp`: ②(샛길 복제)·③(근교 복제)과 다른 지형인 **Lv2_MagicCliffs 레이아웃**(x −420~−180, 위층 발판·사다리)을 복제, 늪 색조(타일 0.46/0.52/0.36, 배경 ×0.42/0.5/0.32 = 탁한 녹갈색·어둡게), 상승 부하 30, zoneTitle "Swamp", floor 1, 반딧불. 프로브 `build_swamp.cs`(재실행하면 복제본·포털을 지우고 다시 만든다).
- 포털: 깊은 숲 왼쪽 끝(마을 포털 −425 바깥) `Portal_ToSwamp`(x −432.5) ↔ 늪지 왼쪽 `Portal_SwampToDeep`(원래 Lv2→Lv1 포털 자리, x −417).
- 몬스터(코드 도트 `Tools/pixelfx/gen_swamp_mobs.py` → `Tools/probes/build_swamp_mobs.cs`, MonsterSheet 행 추가):
  - 낮: **개구리** 260HP 방어 6(48px, RatThief 템플릿, 깡충 뛰기 Run·혀 뻗기 Attack) · **꽃게** 300HP 방어 14(48px, 집게 연타, 위층 발판 스포너) · **악어** 520HP 방어 10(64px, 입 벌려 물어 흔들기, 밀도 3) · 포자버섯(SporeShroom) 재사용.
  - 밤(습지 안개): **모기** 150HP(48px, FlowerMole 템플릿=비행·유도탄, 붉은 Light2D) · **슬라임** 260HP(48px, LeftMole 템플릿=느림, 초록 Light2D).
  - 보스: **거대 슬라임** 8000HP 방어 20 슈퍼아머(슬라임을 2배 좌표로 96px 재작화, 점액 뿔·삼킨 뼈, Attack2 = 높이 뛰어 내려찍기 + 점액 튐), PassiveTerror 템플릿(18px/유닛), **밤** 스포너, 스폰 x −200(오른쪽 끝), 영역 = 맵 전체(x −422~−178). 초상 RPGMonstersIcon `5 Field/field_501`. BossPhase·HP바·이름 자동.
- 드롭: 개구리 Frog Skin 35%·Frog Tongue Whip 2% / 꽃게 Crab Pincer 35%·Petal Pincer 2% / 악어 Croc Tooth 35%·Croc Jaw Axe 2% / 모기 Blood Sting 30%·Bloodsting Rapier 2% / 슬라임 Slime Gel 33%·Slime Mace 2% / 낮 몹 Bog 부위·밤 몹 Gloom 부위 각 0.3% / 보스 Slime Gel 확정 + `Resources/Boss/GiantSlime_BD`(확정 1개 = Slime Mace·Bloodsting Rapier·Frog Tongue Whip 중, 고유 Primordial Gel 8%·천장 12).
- 버그 수정: **B-118** Lv2 카메라 경계 회전(카메라가 x −300 에서 멈춤, 원본 Lv2 도 수정) · **B-119** 깊은 숲 귀환 포털 비활성 · **B-120** 발판 스포너 영역이 발판보다 높아 플레이어를 못 잡음(늪지만 수정).
- 검증(플레이): 낮 개구리·꽃게(발판)·악어·버섯 스폰, 밤 모기·슬라임, 보스 등장(HP바·초상·이름) → 처치 → 확정 보상 드롭(계보 무기 + 강화석 + 재료), 포털 왕복(Portal.TravelNow), 콘솔 에러 0(테스트 무적 잠금 경고 제외). 테스트로 생긴 보스 처치 ES3 키는 지웠다. 스크린샷 `Logs/playtest/swamp_*.png`(커밋 안 함).

### 14.1 ④구역 늪지 계보 장비 (LineageGear 1절)

| 몹 | 무기(드롭) | 무기 스킬 | 세트 |
|---|---|---|---|
| 개구리 | **Frog Tongue Whip** (Wood Spear 기반 13~20) | A **Tongue Lash** = 찌르기 판정 복제본 넉백 **음수 ×1.8**(적을 앞으로 당김)·3명·경직 0.4s·쿨 2s, S Leap Slam(돌진 찌르기 피해 1.25·넉백 1.4) | Bog Stalker |
| 꽃게 | **Petal Pincer** (Leather Knuckles 기반 13~19) | D **Pincer Flurry** = 올려치기 복제본 다단(3타 설정, 실측 2~3타)·피해 0.6/타·쿨 2s, A Crab Scuttle·S Shell Back(이름만) | Bog Stalker |
| 악어 | **Croc Jaw Axe** (ScareSide 기반 15~23) | A **Jaw Shake** = 내려베기 3타(원본 2)·경직 0.5s·쿨 3s, R2 **Death Roll** = 미끄러지며 베기 피해 1.2·쿨 5s | Bog Stalker |
| 모기 | **Bloodsting Rapier** (RatCheifDagger 기반 12~18, 치명 12) | A **Bloodsting** = 교차 찌르기 복제본 + `LifeStealHit`(적 1명당 무기 평균 피해×배율×20% 회복)·쿨 1.5s, S Swarm Dart, D Swarm Call | Gloom Swarm |
| 슬라임 | **Slime Mace** (SacreGreatSword 기반 15~24) | S **Slime Smash** = 검기 복제본 + `SpawnOnStart` 튀는 슬라임 2(`SummonRunner` 바운스, 무기 피해 45%)·쿨 6s, A Gel Gather(흡입) | Gloom Swarm |
| 거대 슬라임(보스) | **Primordial Gel** (유니크, SacreGreatSword 기반 22~34) | S **Gel Tsunami** = 검기 피해 1.4 + 큰 슬라임 3, A Primordial Pull, **처치 시 35% 슬라임 소환**(`KillSummonProc` + `EnemyHealth.Killed`) | — |

- 프로브 `build_swamp_weapons.cs`(판정 복제 `DamageObject/<무기>/…`, 원본 공유 프리팹 미수정, 소환물 `DamageObject/SlimeMace/SlimeHopper·GelHopper`), 아이콘 `Tools/pixelfx/gen_swamp_items.py`(몹 팔레트), ItemSheet 143~163.
- **세트 2종** (`make_forest_sets.py Bog Gloom` 색 변환 + `build_swamp_sets.cs`, 부위 드롭 0.3%):
  - **Bog Stalker** (BogArmor 올리브·갈색, 낮 개구리·꽃게·악어 + 무기 3종): 2: 이동 6·방어 8 / 4: Tongue Lash·Jaw Shake·Pincer Flurry 쿨 −30 / 6: 공속 10·출혈 피해 15·피해 8. 부위 방어 20/36/20/16/16.
  - **Gloom Swarm** (GloomArmor 핏빛 보라, 밤 모기·슬라임 + 무기 2종): 2: HP 80·치명 5(흡혈 키 `LifeSteal` 은 만들지 않고 무기 Bloodsting 에 맡김) / 4: Bloodsting 쿨 −40·Slime Smash −35 / 6: 독 12·피해 10·치명피해 10. 부위는 HP·치명 위주.
- 검증(플레이, `swamp_weapon_test.cs` = 가까운 몹 앞에 서서 슬롯 발동 → 대상 x·HP·소환물 추적): Tongue Lash 대상이 플레이어 쪽으로 0.76 끌려옴, Bloodsting HP 90→96, Slime Smash 소환물 2·추가 타격, Jaw Shake 3타, Pincer Flurry 2타, Death Roll 44 피해, Gel Tsunami 소환물 3, `kill_proc_test.cs` 처치 소환 발동. `equip_swamp_set_test.cs` Bog/Gloom 6부위 → 스택 6, 세트 수치·쿨감 키 일치. combo_all 6종 = 원본 무기와 같은 순환(1-1 창 / 1-2-3 단검 / 1-2 대검·낫·너클).
- ⏳ 남은 것: 모기는 FlowerMole 템플릿이라 공격이 유도탄(흡혈 돌진 아님) · 보스 공격 판정은 PassiveTerror 원본(갈퀴·불꽃) 그대로(③ 2차처럼 점액 판정 교체 필요) · 보스 분열 패턴 없음 · Petal Pincer 방어 자세(Shell Guard 재사용)·Bloodsting S 3연 돌진 미구현 · 늪 전용 소품·안개 효과 없음(색조만) · 세트 착용 텍스처 Leaf 색 변환 임시.

## 15. ⑤구역 산적 성채 — 1차 구현 (2026-10-19, LineageGear 2절)

- 맵 `Lv1_BanditFortress`: ②③④가 샛길·근교·Lv2 지형을 모두 써서 **외곽 샛길(SideRoad, x −753~−137) 레이아웃 복제**(② 일반 숲과 같은 지형 — 성채 전용 지형은 ⏳). 성채 색조(타일 0.66/0.5/0.4, 원경·식생 ×0.62/0.44~0.48/0.34~0.38 = 갈색·어둡게) + 고정 횃불 Light2D 12개(주황, 밤에 윤곽), 상승 부하 30, zoneTitle "Bandit Fortress", floor 1, 반딧불 없음. 프로브 `build_bandit_fortress.cs`(재실행하면 복제본·포털을 지우고 다시 만든다).
- 포털(분기): 깊은 숲 가운데 아래층 `Portal_ToFortress`(x −305, 버섯 스폰 지면) ↔ 성채 오른쪽 끝 `Portal_FortressToDeep`(원본 PotalLv1_NearForest2 자리 −236). 왼쪽 마을 포털(−425) 유지. B-119 재발 방지로 두 포털 모두 `SetActive(true)`. 플레이에서 왕복 확인.
- 몬스터(코드 도트 `Tools/pixelfx/gen_bandit_mobs.py` → `Tools/probes/build_bandit_mobs.cs`, MonsterSheet 행 추가). **SPUM 금지** → 마을 NPC(`gen_elder.py`) 양식(큰 머리 2등신·짙은 외곽선)을 48px 몬스터 프레임에 맞춰 그림(키 약 34px ≈ 플레이어와 비슷):
  - 낮: **산적 단검병** 280HP 방어 6(RatThief 템플릿=빠른 근접, 붉은 두건·복면, 쌍단검 2연 찌르기) · **산적 궁수** 220HP(FlowerMole 템플릿=원거리, 녹색 두건·화살통, 시위 당겨 쏘기) — 탄을 `EnemyDamge/Bandit/BanditArrow`(FlowerAttackPrefab 복제, 애니메이터 제거·화살 스프라이트, 발사 순간 플레이어 쪽 직선) 로 교체 · **산적 방패병** 480HP 방어 22 슈퍼아머(RatThief, 철모·둥근 방패 밀치기).
  - 밤: 같은 종 + **횃불 순찰병** 340HP(RatThief, 횃불 휘두르기, 주황 Light2D 0.9/3.2).
  - 보스: **산적 대장** 9000HP 방어 22 슈퍼아머(96px, 뿔 투구·안대·수염·모피 망토·곡도, Attack2 = 칼 치켜들고 외침), PassiveTerror 템플릿(18px/유닛), **낮** 스포너 = 원본 샛길 보스 자리(위쪽 망루 −533/24), 영역 = 맵 전체(x −755~−135). 초상 = 대장 도트 Idle_0(사람형 초상 에셋 없음). 공격 판정은 `build_ent_boss_roots.cs` 방식으로 교체: 갈퀴 → `ChiefCleave`(곡도 큰 베기 초승달, 64px×6 @16ppu), 불꽃 → `ChiefAxe`(회전 손도끼 투척, 32px×3 @12.5ppu), 스포너 키 clow/fire 유지, BT damageOb GUID 교체.
  - **2페이즈 부하 호출**: `BossMinionCall`(신규) 이 `BossPhase.PhaseEntered`(신규 정적 이벤트)를 듣고 HP 50% 진입 시 `EnemySpawner.SpawnMinion`(신규, 슬롯 밖 소환·enemyNum −1·드롭 없음·"minionDead" 풀 시체) 으로 단검병 2명을 양옆에 세운다. 보스 스포너 풀에 minion/minionDead/stab 추가. 배너 "ENRAGED — Calls the gang!".
- 드롭: 단검병 Bandit Bandana 35%·Cutthroat Dagger 2% / 궁수 Broken Arrow 35%·Bandit Shortbow 2% / 방패병 Iron Rivet 35%·Raider Buckler 2% / 횃불 Pitch Torch 30% / 단검·궁수 Outlaw 부위, 방패·횃불 Raider 부위 각 0.3% / 보스 Bandit Bandana 확정 + `Resources/Boss/BanditChief_BD`(확정 1개 = 계보 무기 3종 중, 고유 Chief's War Banner 8%·천장 12).
- 버그 수정: **B-125** 몹 직선 탄(EnemyDamageObject 비추적 발사)이 왼쪽을 보던 몹에서 좌우 반전된 채 날던 문제(화살이 거꾸로) · **B-126** EnemyHealth.OnEnable 이 Kill 된 시퀀스에 IsPlaying 을 불러 DOTween "죽은 트윈" 경고가 풀 워밍업마다 수십 개.
- 검증(플레이): 낮 단검병·궁수(화살 발사)·방패병 스폰, 밤 횃불 순찰병(주황 빛)·단검병, 보스 등장(HP바·초상·이름) + ChiefCleave·ChiefAxe 발동, HP 55% 피해 → 2페이즈·부하 2명 소환, 처치 → 배너·확정 보상(Raider Buckler + 재료), 포털 왕복, 콘솔 에러 0. 스크린샷 `Logs/playtest/fort_*.png`·`bandit_boss_*.png`·`boss_Chief*.png`(커밋 안 함). 테스트 뒤 세이브는 백업본으로 복원.

### 15.1 ⑤구역 계보 장비 (LineageGear 2절)

| 몹 | 무기(드롭) | 무기 스킬 | 세트 |
|---|---|---|---|
| 단검병 | **Cutthroat Dagger** (DoubleDagger 기반 13~19, 치명 15·치피 30) | A **Backstab** = 앞쪽 6유닛 안 가장 가까운 몹 뒤로 순간 이동해 돌아서 찌르기(`BackstabStep`: 무적 0.3s·잔상, 이 판정만 치명 +40%·치피 +30% = 백어택), 쿨 3s. 벽 너머로는 안 넘어감. S Shadow Step·Cutpurse Stride(원본 버프, 이름만) | Outlaw |
| 궁수 | **Bandit Shortbow** (ScareStaff 기반 12~19, 신규 원거리) | 평타 = 직선 화살(`StraightArrow` 신규: 바라보는 방향 직진·트리거 충돌·관통 수·각도/퍼짐/시작 흩뿌림, 근접 판정 끈 DarkAttack 복제 `BowShot` 이 `SpawnOnStart` 로 쏨), A **Triple Shot** = 화살 3발(퍼짐 6°, 1관통, 쿨 1.5s), S **Poison Volley** = 위에서 −65° 로 쏟아지는 독화살 6발(독 4×6), D Pitch Shot(원본 직선 판정 주황 톤). 손에 든 그림은 활 아이콘 | Outlaw |
| 방패병 | **Raider Buckler** (Shell Shield 기반 12~18) | A Raider Thrust, S **Shield Shove** = 방패 돌진 밀치기(넉백 1.6배·경직 0.5s, 쿨 4s), D **Counter Stance** = Shell Guard 재사용(홀드 방어 +200, 쿨 6s) | Iron Raider |
| 산적 대장(보스) | **Chief's War Banner** (유니크, Wood Spear 기반 22~32) | A Banner Thrust·S Raid Charge(피해 1.3배), F **Rally the Gang** = 산적 그림자 3명 돌격(`SummonRunner`, 단검병 Run 프레임 보라 그림자, 무기 피해 50%·0.3s 간격, 쿨 9s). 손에 든 그림은 깃발 아이콘 | — |

- 프로브 `build_bandit_weapons.cs`(판정 복제 `DamageObject/<무기>/…`, 원본 공유 프리팹 미수정), 아이콘 `Tools/pixelfx/gen_bandit_items.py`(산적 옷 팔레트), ItemSheet 164~181(빈 행 채움 — 다음 구역은 행 추가 필요).
- **세트 2종** (`make_forest_sets.py Outlaw Raider` 색 변환 + `build_bandit_sets.cs`, 부위 드롭 0.3%):
  - **Outlaw** (OutlawArmor 검붉은 가죽, 단검병·궁수 + Cutthroat Dagger·Bandit Shortbow): 2: 치명 6·이동 5 / 4: 치명피해 25·Backstab 쿨 −35·Triple Shot −30 / 6: 공속 12·피해 8. 부위는 치명·이동 위주.
  - **Iron Raider** (RaiderArmor 쇠빛 회색, 방패병·횃불 + Raider Buckler): 2: 방어 15·HP 60 / 4: Shield Shove 쿨 −30·Counter Stance −35·피해 8 / 6: HP 150·방어 20·치명피해 10. 부위 방어 26/44/26/20/20.
  - LineageGear 원안의 `BackstabDmg`(백어택 치피)·"반격 피해"·"HP 15%" 는 신규 키 없이 기존 키(CritDmg·쿨감·Hp 정수)로 대체 — 백어택 보너스 자체는 Backstab 판정이 갖는다.
- 검증(플레이, `bandit_weapon_test.cs` = 같은 층 가까운 몹 앞에서 슬롯/평타 발동 → 판정·화살·소환물·플레이어 x·HP 추적): Backstab 플레이어 −308.3→−305.3 순간 이동·방향 반전·피해, 평타 화살 명중(ArrowTouch), Triple Shot 화살 3발·전체 피해 50, Poison Volley 6발·독 피해, Pitch Shot, Shield Shove 돌진, Counter Stance 방어 판정, Rally the Gang 그림자 3·전체 피해 67. `equip_bandit_set_test.cs` Outlaw/Raider 6부위 → 스택 6, 수치·쿨감 키 일치(새 플레이에서). combo_all = 원본과 같은 순환(단검 1-2-3, 방패 1-2-3, 활·깃발 1).
- ⏳ 남은 것: 성채 전용 지형·소품(목책·망루·천막) 없음(샛길 지형 + 색조·횃불 빛) · 하늘(카메라 배경)은 색조가 안 먹음 · 궁수 화살은 조준 직선(플레이어 쪽)이라 위아래로 비스듬히 날 수 있음 · 횃불 순찰병 계보 무기 없음 · 부하는 보스가 죽어도 남음(맵 이탈 시 정리) · 세트 착용 텍스처 Leaf 색 변환 임시 · 손 무기 그림은 아이콘 재사용(활 당기기 애니 없음, 지팡이 모션).

## 16. ⑥구역 탑 근교 — 1차 구현 (2026-10-19, LineageGear 3절) — 1층계 졸업 지역

- 맵 `Lv1_TowerOutskirts`: ③ 깊은 숲과 같은 **근교 원본 지형(Lv1_NearForest) 복제**, 돌·유적 톤(타일 0.74/0.6/0.66 = 풀 초록을 눌러 회갈색, 원경·식생은 채도를 70% 빼고 ×0.66~0.72) + 청록 룬 빛 Light2D 8개, 상승 부하 **40**, zoneTitle "Tower Outskirts", floor 1, 반딧불 없음. 양끝에 보이지 않는 벽 `EdgeWall_Left`·`EdgeWall_Right`(원본 지형에 끝 벽이 없어 보스 넉백에 밀리면 맵 밖으로 떨어짐, B-130 — 같은 지형의 깊은 숲·샛길·일반 숲·산적 성채도 `build_edge_walls.cs` 로 함께 막음). 프로브 `build_tower_outskirts.cs`(재실행하면 복제본·포털을 지우고 다시 만든다).
- 포털(진행): ④ 늪지 오른쪽 끝 `Portal_ToOutskirts`(x −186, 보스 자리 너머) ↔ 탑 근교 오른쪽 `Portal_OutskirtsToSwamp`(−236). 왼쪽 마을 포털(−425) 유지. **탑 입구** `Portal_ToTower`(−432.5) → 2층계 `Lv2_MagicCliffs` 입구(`Portal_ToLv1` 자리 도착) — `TowerGatePortal`(맵 Portal 묶음)이 문지기 첫 처치 기록(ES3 `bossFirstKill.Gatekeeper_BD`, 새 키 없음)이 없으면 끄고, 처치 순간 켜며 "TOWER GATE — The way to the 2nd floor is open" 배너. 2층계 쪽 귀환 포털은 그대로 샛길행(내려가기는 자유). 샛길→2층계 지름길 `Portal_ToLv2` 는 `PortalSeal` 로 문지기 첫 처치 전에는 막힌다(보이되 이동 대신 토스트 "The path upward is sealed by the Tower.", Q-414 임시 가정 적용). B-119 재발 방지로 모든 포털을 켠 채로 만든다.
- 몬스터(코드 도트 `Tools/pixelfx/gen_outskirts_mobs.py` → `Tools/probes/build_outskirts_mobs.cs`, MonsterSheet 행 추가, 공통 팔레트: 돌 회색·이끼·청록 룬 눈):
  - 낮: **소형 골렘** 360HP 방어 14(48px, RatThief 템플릿, Run·Attack = 몸을 말아 구르는 돌진) · **중형 골렘** 620HP 방어 20(64px, LeftMole 템플릿=느림, 두 주먹 내려치기 → 판정 `GolemQuake` = 땅에서 솟는 돌 가시, LeafAttackPrefab 복제) · **대형 골렘** 950HP 방어 28 **슈퍼아머**(80px, FlowerMole 템플릿=원거리, 등 뒤 바위를 들어 던짐 → 탄 `GolemBoulder`, FlowerAttackPrefab 복제·애니메이터 제거, 조준 직선).
  - 밤: 골렘 + **유적 정령** 170HP(48px, FlowerMole, 룬 돌 조각 핵 + 청록 빛 무리, Light2D 0.9/3, 탄 `RuinBolt`).
  - 보스: **탑의 문지기(Gatekeeper of the Tower)** 15000HP 방어 28 슈퍼아머 — 128px 거대 갑옷 수호자(이끼 낀 판금, 바이저 룬 빛, 너덜너덜한 망토, 앞에 꽂아 세운 대검, **가슴에 브로치와 같은 문장**: 금 고리 + 네 갈래 별), PassiveTerror 템플릿(18px/유닛 → 약 5.5유닛 키). **낮** 스포너, 스폰 x −412(왼쪽 끝 = 탑 입구 앞), 영역 = 맵 전체. 초상 = 문지기 도트 Idle 첫 프레임. 공격 판정 교체(`build_ent_boss_roots.cs` 방식, 원본 공유 프리팹 미수정): 갈퀴 → `GateCleave`(대검 내려찍기 궤적 + 룬 섬광 + 땅 파편, 64px×6 @16ppu), 불꽃 → `GateShockwave`(룬 충격파 초승달, 32px×3 @12.5ppu), BT damageOb GUID 교체. Attack2 = 대검을 땅에 꽂아 룬 고리.
  - 3페이즈 = `BossPhase` 자동(50%·25%), **2페이즈 부하 호출** `BossMinionCall`(소형 골렘 2, 배너 "AWAKENED — The stones rise!" = 새 직렬화 필드 `_bannerText`). 보스가 죽거나 사라지면 부하도 정리(B-127).
- **스토리 연출**: 첫 조우 대사 `BossEncounterTalk`(보스 9유닛 안, ES3 `story.gatekeeper.met` 1회, 대화 중 보스 BT 정지) → `StoryDialogues.Gatekeeper`: 문지기 "...None... shall pass... the gate." / 독백 "A giant in armor, older than the ruins around it." · "That crest on its chest... it's the same pattern as my brooch." · "...Father?" / 문지기 "...Turn... back..." / 독백 "No. Whatever you are, I'm going up." — **아버지임은 암시만**(2절 결말과 충돌하지 않게 확정하지 않음). 처치 시 배너 대기열(`BossBanner` 큐): "GATEKEEPER OF THE TOWER DEFEATED" → "RANK UP / DAWN EXPLORER"(`BossReward.grantsRank`=1) → "TOWER GATE"(포털 개방).
- 드롭: 소형 Golem Pebble 35%·Pebble Gauntlet 2% / 중형 Quake Core 33%·Quakestone Maul 2% / 대형 Monolith Shard 33%·Monolith Shield 2% / 정령 Ruin Mote 30% / 모두 Stoneborn 부위 각 0.3% / 보스 Golem Pebble 확정 + `Resources/Boss/Gatekeeper_BD`(확정 1개 = 계보 무기 3종 중, 레전더리 Gatekeeper's Oath 6%·천장 15, 강화석 4~7 + 첫 처치 8, 칭호 1).
- 버그 수정: **B-127**(f626e6f5) 보스 사망·사라짐·시간대 전환 시 부하 정리 · **B-128**(cf1c4377) 장비창이 닫힌 채 같은 세트 무기 교체 시 세트 스택·무기 프리팹 누적.
- 검증(플레이): 낮 소형·중형 골렘·보스 스폰, 대형 골렘 바위 투척(`GolemBoulder`)·정령 탄(`RuinBolt`) 발사, 보스 조우 대사 6줄(스크린샷 `gate_talk1`·`gate_talk_father`) → 대화 뒤 BT 재개·`GateCleave`·`GateShockwave` 발동, `gatekeeper_boss_test.cs`: HP 55% → 2페이즈 + 소형 골렘 2, 80% → 3페이즈, 처치 → 확정 보상(Monolith Shield + 강화석 12 + 재료)·칭호 0→1·포털 개방·처치 4초 뒤 부하 0, 배너 순서 스크린샷 3장, `Portal_ToTower` → Lv2 도착(부하 25), 늪지 ↔ 탑 근교 왕복. 콘솔 에러 0(테스트 생존용 무적 토큰의 RuntimeMonitor 12초 경고 제외, B-132). 테스트 뒤 세이브는 백업본으로 복원. 스크린샷 `Logs/playtest/outskirts_*`·`gate_*`·`boss_Gate*`(커밋 안 함).

### 16.1 ⑥구역 계보 장비 (LineageGear 3절)

| 몹 | 무기(드롭) | 무기 스킬 | 세트 |
|---|---|---|---|
| 소형 골렘 | **Pebble Gauntlet** (Leather Knuckles 기반 14~21) | A **Rolling Charge** = 대시 + 앞으로 구르는 돌(`StraightArrow` 관통 4, 무기 피해 130%, 쿨 2.5s — Dash 판정은 콜라이더 없는 이동 표식이라 돌이 피해를 맡음), C Rubble Uppercut(1.2배·넉백 1.3배), R Pebble Hop(이름만) | Stoneborn |
| 중형 골렘 | **Quakestone Maul** (SacreGreatSword 기반 18~28) | S **Quake Slam** = 검기 1.35배·넉백 1.4배·경직 0.45s·쿨 3s, D **Aftershock** = CliffStaff 의 Cliff Quake 를 복사·강화(1.6배, 1.25배 크기, 쿨 6s), A Tremor Pull(원본 흡입, 이름만) | Stoneborn |
| 대형 골렘 | **Monolith Shield** (Shell Shield 기반 15~22) | A **Boulder Toss** = 포물선 바위(`StraightArrow._gravity` 24, 38°, 관통 2, 160%, 쿨 2.5s), C **Monolith Stance** = Shell Guard 홀드 방어 +250 + **슈퍼아머**(`addSuperArmor`), B Granite Bash(넉백 1.4배) | Stoneborn |
| 탑의 문지기(보스) | **Gatekeeper's Oath** (레전더리 특색, SacreGreatSword 기반 26~38, 치명 10·치피 35) | S **Gate Cleave** = 검기 1.5배 + 땅을 미는 룬 충격파(`StraightArrow._ignoreTerrain`, 관통 6), D **Stone Sentinels** = 소형 골렘 3기가 굴러 돌격(`SummonRunner`, 무기 피해 60%·0.3s 간격, 쿨 10s), A Oath Bind(원본 흡입) — 문지기 패턴의 플레이어판 | — |

- 프로브 `build_outskirts_weapons.cs`(판정 복제 `DamageObject/<무기>/…`·`DamageObject/Outskirts/`, 다른 무기 스킬 복사 도우미 `addSkill`), 아이콘 `Tools/pixelfx/gen_outskirts_items.py`(골렘·문지기 팔레트), ItemSheet 182~194(**맨 뒤에 행 추가**). AbsorbDark 원본에 누락 스크립트가 있어 복제 저장이 안 되므로 A(흡입)는 원본 판정을 그대로 쓴다.
- **세트 Stoneborn** (StoneArmor 돌 회색, `make_forest_sets.py Stone` + `build_outskirts_sets.cs`, 구역 1세트 = 1층계 졸업 세트): 2: 방어 20·HP 80 / 4: Rolling Charge·Aftershock·Boulder Toss 쿨 −30·피해 8 / 6: 방어 25·HP 180·피해 10. 부위 방어 30/50/30/24/24·HP 200/480/200/160/140. 원안의 "슈퍼아머 지속 +1s"·`DmgReduce` 신규 키는 만들지 않음(기존 키만).
- 검증(플레이, `bandit_weapon_test.cs`): Rolling Charge 대시 + 구르는 돌 명중(대상 8.8 밀림), Quake Slam·Aftershock 판정·피해, Boulder Toss 바위 명중(전체 피해 33), Monolith Stance 홀드 판정, Gate Cleave 검기 + `ShockwaveTouch`(피해 57·전체 69), Stone Sentinels 소환 3·연속 타격. `equip_stone_set_test.cs` 6부위 → 스택 6, 쿨감 키 3종 30, 피해 +18. combo_all = 원본과 같은 순환(너클 1-2, 대검 1-2, 방패 1-2-3). 피해 수치는 칭호 0(부하 부족 −80%) 상태 값.
- ⏳ 남은 것: 탑 근교 전용 지형·석상 소품 없음(근교 지형 + 곱셈 색조라 풀이 회갈색 정도) · 낮 전용 보스(밤엔 사라짐, 다른 구역 보스와 같음) · 조우 대사 중 주변 골렘은 계속 움직임(B-131) · 대형 골렘 바위는 조준 직선(포물선은 플레이어 무기만) · 레전더리 등급 표시는 U(고유)로 대신 · 세트 착용 텍스처 Leaf 색 변환 임시 · Stone Sentinels 소환 골렘은 SummonRunner 직선 돌격(Run 프레임만).
