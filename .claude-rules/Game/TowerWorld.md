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
| ⑥ 탑 근교 | 골렘들이 있는 곳 | 골렘(소·중·대) | (미정) | **탑의 문지기** | 골렘 스프라이트 없음(구글 문서 보스방 레퍼런스: 이끼 낀 거대 석상 유적) |

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
- **탐험가 칭호**(어비스의 피리 등급처럼): 0 견습 Novice → 1 서광 Dawn → 2 운해 Cloudsea → 3 심록 Deepgreen → 4 홍염 Crimson → 5 흑석 Blackstone → 6 심연 Abyss → 7 백광 Whitelight. 층계 보스 처치로 승급(`BossReward.grantsRank`, 임시: PassiveTerror=1, AbyssWizard=2). ES3 `explorer.rank`.
- HUD: 좌상단 "RANK · LOAD/RES" (부족하면 붉게). ✅ 구현: `AscentLoad`(순수 계산+ES3 `explorer.rank`), `AscentHud`, `MapInfo.ascentLoad`(외곽 0 / 샛길 5 / 마법 절벽 25), `BossReward.grantsRank`(PassiveTerror=1, AbyssWizard=2). 피해 훅: `EnemyHealth`(주는 피해), `PlayerHealthManager`(받는 피해).

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
