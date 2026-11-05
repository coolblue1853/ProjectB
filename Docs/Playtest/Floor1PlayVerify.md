# 1층계 플레이 검증 PV1 (11-03)

에디터 플레이 모드 실측. 낮 고정, 시간배율 1, 세이브 백업(`Logs/pv1/backup`)·복원 완료, 플레이 정지. 프로브: `Tools/probes/pv1_cf.cs` `pv1_contact.cs` `pv1_mini.cs` `pv1_l7.cs` `pv1_quest.cs` `pv1_recipes.cs` `pv1_recipes_ui.cs`. 로그 `Logs/pv1/*.log`(gitignore). 무기 FangDagger(후딜 0.278초, 판정 0.052초, moveSpeed 3, 에디터 약 85fps).
bash 봇(PT-BOT 본체)은 쓰지 않고 항목별 전용 프로브를 새로 만들었다(봇은 사냥 루프 전용이라 정밀 계측에 부적합).

## (1) 조작감 CF-A~D
| 항목 | 결과 | 수치 |
|---|---|---|
| 후딜 중 앞키 전진 | 통과 | 앞키: 후딜 중 vx 1.71 = 평상시 3.42의 0.5배, 무입력 0, 뒷키 전진 없음. 콤보 1·2타 mul 0.5, 3타 0.25 (평균 |vx| 1.54/1.71/0.90) |
| 방향 전환 히트박스 | 통과 | 뒷키로 평타: 판정(0.052초)+0.05 = 0.10초에 player.scaleX와 weapon.lossyScaleX 가 동시에 -1.5로 뒤집힘(그 전에는 안 뒤집힘). 이후 반대 방향 전진 vx -1.71 |
| 캔슬 창 | 통과 | 점프 캔슬 0.10초, 이동 캔슬 0.11초(=후딜 40%, 0.278×0.4), 턴 0.10초. 층계별 비율 1층 0.4 / 3층 0.3 / 5층 0.2 |
| 점프 가변 높이 | 통과 | 탭 0.826, 0.12초 1.094, 길게 1.616 → 탭/길게 0.51(목표 45~55%) |
| 점프 버퍼 0.15 | 통과 | 착지 0.099초 전 입력 → 착지 직후 재점프(0.66초). 0.171초 전 입력 → 발동 안 함 |
| 코요테 0.12 | 통과(단위 수준) | CoyoteTimer +0.10초 true / +0.14초 false. 실제 낭떠러지 걷기는 미확인 |
| 공중 평타 관성 | 통과 | 앞키 유지: vx 3.42 → 2.39(0.7×moveSpeed×1.14) 유지. 키 뗌: 0.08초 후 0.59, 0.16초 0 (약 0.15초 내 0) |
| 대시 버퍼 0.1 | 통과 | 1층계는 대시 미개방이라 `MovementUnlocks.DebugAll` 임시 ON. 쿨다운 끝 0.06초 전 재입력 → 2회 대시, 0.30초 전 → 1회. 평타 후딜 0.14초에 대시 입력 → 0.14초에 대시 시작(후딜 끊김) |
| 몸박 ×0.5 | 통과(부분) | Rabbit_BD 접촉 13 → 순수 함수 ScaledDamage 6. 실제 HP 감소는 방어로 1(구분 불가). 피해 간격 0.92초(무적 0.9초). 평타 연타 2.5초 겹침 동안 피해 0회(유예 동작) |

## (2) 미니보스·정예 둥지
| 항목 | 결과 | 수치 |
|---|---|---|
| 미니보스 M1~M5 스폰 | 통과 | 5곳 모두 좌표 근처에서 0.0~1.0초 내 소환, elite+miniBoss 플래그: 늑대 HP 460, 곰 1150, 악어 940, 부두목 880, 장로 골렘 1050 |
| 미니보스 드롭 | 통과 | 확정 드롭 표대로 곰 Bear Pelt×6+Stone, 악어 Croc Tooth×6+Stone, 부두목 Iron Rivet×6+Stone×2(+WoodcuttersAxe), 골렘 Golem Pebble×6+Stone×2. 늑대는 같은 날 재처치 배율(0.5/0.25)로 Wolf Fang 3/2, Enhance Stone 은 드롭 후 자동 획득되어 집계 누락(0.3초 시점 존재 확인) |
| 정예 둥지 늪·성채·탑 | 통과 | 표식 4개 각 0.8~0.9초 살피기 → Success → 정예 소환(늪지 게 HP 640 / 산적 단검병 352 / 작은 골렘 400) → 처치 시 ZoneSeal_Swamp / Fortress / Tower 각 1개 드롭 |
| 주의 | 실패(버그) | 구역 인장 3종 아이콘 `Item/ZoneSeal_*` 이미지 없음(콘솔 Error "이미지 파일을 로드하지 못했습니다"). TechDebt 등록 |

## (3) L7 신규 몹 6종
| 몹 | 스폰 비율(기대) | 처치 시 드롭 |
|---|---|---|
| ForestHunter | 4/15=0.27 (0.3) | Gold, Wolf Fang. 확률표 HuntersLongbow 0.02 연결 |
| VineFairy | 5/16=0.31 (0.3) | VineLash 드롭됨(2%, 우연), Boar Tusk, Gold |
| ForestOgre | 6/16=0.38 (0.4) | Gold, Heartwood Chip, Wood. GnarledClub 0.02 |
| BanditSentry | 6/16=0.38 (0.35) | Gold, Broken Arrow, Enhance Stone, BanditShortbow. 추가 드롭표 없음(의도: 이전 대상 아님) |
| BanditWoodcutter | 5/16=0.31 (0.35) | Gold, Bandit Bandana. WoodcuttersAxe 0.02 |
| RuinWarden | 5/15=0.33 (0.3) | Gold, Golem Pebble, StoneHandArmor |
통과. 샘플 1회 비율이라 ±표본 오차. 모두 호스트 BT 재사용(문서대로 원거리·돌진 행동 없음, 미검증 범위 밖).

## (4) L1 퀘스트 목표 타입(NpcQuestLog 실제 API, 선행 퀘스트 완료 처리·레벨 99 오버라이드 후 복원)
| 타입 | 결과 | 수치 |
|---|---|---|
| killElite | 통과 | AlphaWolf 처치 → Q207(AlphaWolf_BD) 0→1 Ready, Q308(`*`) 0→1 Ready, Q309(OldBear_BD) 0 유지(대상 불일치 정상) |
| equip | 통과 | GroveCap→Armor→LegArmor 착용: setsEffectStack 1→2→3, Q307 1→2→3/3 Ready |
| enhance | 통과 | 수락 시 착용 무기 +3 시드 → 즉시 3/3. 무기 0으로 내리고 강화: +1/+2/+3 에 Q409 1/2/3 갱신(이번 런은 이미 +3 이라 수락 시 3/3 — 시드 동작 확인) |
| bestiary | 통과 | 새 종류 처치마다 kinds 10→11→12→13→14, Q610 10→11→12/12 Ready |
주의: `ReleaseEnemy(true,true)` 는 Kill 메시지를 보내지 않는다(프로브 한정). 실제 사망(`damage2Enemy` 과다 피해)으로 확인.

## (5) L5 방어구 레시피 30행 UI
통과. CSV 46행 = UI 46칸 모두 존재(missing 0), 전부 itemData·스프라이트·재료 연결. 방어구 대장장이 창을 E 로 12페이지(7/12 표기) 순회, 페이지 2·5·6·9·11 캡처 `Logs/pv1/recipes_p*.png`. 페이지 6(WispArmor 등) 이미지·이름·재료 0/10 0/8·CRAFT 버튼 정상. 기존 정적 페이지 0~1의 20칸은 창을 열기 전에는 itemData 가 비어 있으나(지연 로드) 연 뒤 정상.

## 못 한 것
- 코요테 실제 낭떠러지 걷기, 몸박 ×0.5 의 HP 수치(방어 때문에 1로 눌림), 몬스터 6종의 원거리·돌진 행동(미구현), 밤 시간대 확인, 연속 캡처 눈 검토(레시피 UI 정지 캡처만).

# PV2 (11-04) 11-04 구현분 플레이 확인

에디터 플레이 모드 실측. 낮 고정, 세이브 백업(`Logs/pv2/backup`)·복원 완료, 플레이 정지. 프로브 `Tools/probes/pv2_*.cs`(+기존 `_mp_drop.cs`). 스크린샷 `Logs/pv2/*.png`(gitignore). 통주 재측정 안 함. 키 입력 대신 프로브가 공개 API·리플렉션으로 구동(키 입력 경로 자체는 미확인으로 표시).

| # | 항목 | 결과 | 수치·관찰 |
|---|---|---|---|
| 1 | 도착 안전지대·무적·배너 (60c1f176) | 통과(부분) | 포털 Use 마다 `ArrivalSafeZone` 점 등록(5→9개). 샛길 도착(-425.5,0.3) 직후 반경 14 안 몹 0, 안전지대 안 몹 0(맵 몹 8, 최근접 108.9). 마을→샛길 이동 중 '수호자' 배너 텍스트 없음. `GrantArrivalInvincible` 호출 즉시 Invincible=true(상수 2.0). 미확인: 무적 2초 만료 시각(프로브 지연 ~2.5초), 초반 3맵 밀집 ×0.65의 정량 비교(전/후 미측정), 보스 구역 도착 순간 |
| 2 | Q 창 3탭·현상·Q104→Q105 (e4170c76) | 통과 | 탭 라벨 "받을 수 있는 7 / 진행 중 3 / 완료 4", 구역별 묶음(샛길·숲·마을·화로). 받을 수 있는 탭에 `[현상] 밤 허수아비` 표시(현상 3건 중 1건만 노출 — 조건 필터 추정). 루엔: Q104 보고 → Q202 제안 → 수락 → Q105 제안(같은 NPC 곁에서 연속 제안, 같은 퀘스트 재질문 안 함) → Q105 수락 Active. 캡처 `qwin_tab0/1.png` |
| 3 | 마을 첫 진입 튜토리얼 (df080212) | 통과(부분) | 이동→공격→점프→물약→퀘스트 5단계가 4.2초 간격 순서대로 표시, 문구에 "(Backspace: 건너뛰기)". `SkipTutorial` 호출 즉시 숨김, 이후 재표시 없음. 미확인: 실제 Backspace 키 입력, 퀘스트 힌트 문구가 `[J]`(Q 창 키는 J) |
| 4 | 가방 25칸·확장 (14711f2d) | 통과(부분) | 시작 unlocked=25. 골드 0 → 구매 거부. 골드 충분 시 25→30(200G)→35(300)→40(450)→45(650), `bagUnlocked`·`maxBoxNum` 45. 확장 후 UI 칸 수 반영. 미확인: 상점 N 키 확인 UI(상점 밖에서는 `Buyback.CanUse()` false) |
| 5 | 산적 대장 보상 (9e8ca9d6) | 통과(데이터·훅) / 실패(아이콘) | 훈장·반지 착용 인식. 약탈: 골드 400회 중 x3 29회(7.25%, 목표 8%). 오기 반지: HP5에 치사 피해 9999 → HP 1, 쿨다운 180초 표시. 실패: 두 아이템 `Item/` 이미지·프리팹 로드 Error(TechDebt 등록). 미확인: 실제 대장 처치 15% 드롭, 재치사(쿨다운 중) |
| 6 | 설정 메뉴 (볼륨·키 설정) | 통과 | 일시정지 > 설정: 전체 음량 40% 표시·`settings.volume` 0.4 저장. 키 설정: Jump C→G 재할당 Ok, `settings.bindings` 147자 저장, 초기화 시 C 복원·0자. 조작 안내 창 열림. 버그: 설정 7행 중 "뒤로" 가 하단 안내문과 겹침(`settings.png`) |
| 7 | 드롭 허공 정지 (8c3bdbbe) | 통과 | `_mp_drop` 9개 구역 24~14개씩 떨궈 4초 후: bad 0/24·21·21·24·14·21·24·13·14. 마을은 DropManager 없음 |

## 발견 버그
- (P3) `BanditPlunderMedal`·`ChiefsLastStandRing` 아이콘 없음(`Resources/Item/`). → TechDebt 등록
- (P4) 일시정지 > 설정 마지막 행 "뒤로" 가 안내 줄과 겹침. → TechDebt 등록
- (참고) `_mp_drop` 프로브가 Invincible 30초 잠금을 반복 걸어 ANOMALY 로그가 나옴(프로브 한정, 게임 버그 아님)

## 못 한 것
키 입력 경로(Backspace·N·J), 무적 2초 만료, 밀집 완화 전후 정량, 대장 실제 처치.

# PV3 (11-04) 슬롯·사망 페널티·설정 화면 플레이 확인

에디터 플레이 모드 실측(SampleScene, 낮 고정, Lv9). 세이브 백업(`Logs/pv3/backup`)·복원 완료, 플레이 정지. 프로브 `Tools/probes/pv3_*.cs`, 캡처 `Logs/pv3/*.png`(gitignore). 보상은 `ExtraEquipRewards` 를 직접 호출(실제 보스 처치 경로는 미확인), 장착은 `ExtraEquip.TryHandleEquip` 호출(드래그·클릭 입력 경로 미확인).

| # | 항목 | 결과 | 수치·관찰 |
|---|---|---|---|
| 1 | 신규 4칸·잠금 툴팁 | 통과(레이아웃 이슈) | 초기 4칸 모두 잠김, 라벨 `[X] TITLE/AURA/CHARM/CAPE`. Box0 호버 → 팁 "Locked: complete the Floor 1 bestiary or first-clear a Floor 1 boss". 이슈: 패널이 인벤 칸 아래쪽(2·3행 사이)에 겹쳐 아이템 칸 일부를 가림 (`slots_locked.png`) |
| 2 | 해금·지급·외형 | 통과 | `OnFloorBossFirstKill(1)` → Title 해금+`TitleWanderer` 1개, `OnBestiaryMilestone("B04")` 중복 호출 → 추가 지급 없음, `OnFloorBossFirstKill(2)` → Aura 해금+`AuraFaintGlow` 1개(Charm·Cape 잠금 유지). 장착 후 라벨 'Title: Wanderer'·'Faint Glow Aura', `ExtraEquipFx` 의 `AuraRing`(32점, 발밑)·`TitleTag`(머리 위 +2.3) 활성, 캡처 `fx_equipped.png`에서 링·이름표 확인. 이슈: 이름표 영문 'Title: Wanderer' 가 한글 텍스트(NPC 이름표 추정)와 겹쳐 읽기 어려움 |
| 3 | 사망 페널티 | 통과 | `LevelSystem.ApplyDeathPenalty`: Lv1(50)·Lv10(2000) 손실 0 / Lv11 2000→1900(5%), 보스 1860(7%) / Lv20 3000→2850(5%), 보스 2790(7%). 레벨 다운 없음. 미확인: 실제 `Die()` 경로의 `BossNearPlayer()` 연결(코드상 `PlayerHealthManager.cs:217`), 사망 배너 문구 |
| 4 | 설정 '뒤로' 겹침 | 통과 | 설정 7행 + '뒤로' 가 하단 안내문(위/아래·좌우 조절·ESC 뒤로) 위에 분리되어 표시, 겹침 없음 (`pause_settings.png`) |
| 5 | 대장 훈장·반지 훅 | PV2 에서 확인 | PV2 #5 로 갈음(약탈 7.25%·오기 반지 HP1). 이번엔 재확인 안 함 |

## 발견 버그
- (P3) `ExtraEquipPanel` 이 인벤 아이템 칸을 가림(앵커 하단 중앙, 2·3행 사이).
- (P3) `TitleTag` 영문 이름표가 한글 이름표와 겹침(플레이어 머리 위 높이).

## 못 한 것
실제 보스 처치 첫 보상, 도감 B04 실제 달성, 장비창 드래그·클릭 장착, 실사망(`Die`) 경로, 밤 시간대.

# PV4 (11-05) 비외형 슬롯 SLOT-R·DEC-1105-A 플레이 확인

에디터 플레이 모드 실측(SampleScene, Lv9). 세이브 백업(`Logs/pv4/backup`)·복원 완료, 플레이 정지. 프로브 `Tools/probes/pv4_*.cs`, 캡처 `Logs/pv4/*.png`(gitignore). 보상은 `ExtraEquipRewards` 직접 호출, 장착은 `TryHandleEquip` 호출(드래그·클릭 입력 경로 미확인). 키 입력은 InputSystem 상태 주입.

| # | 항목 | 결과 | 수치·관찰 |
|---|---|---|---|
| 1 | 슬롯 UI·잠금 툴팁 | 통과 | 초기 라벨 `[X] EARRING/BRACELET/CHARM/STONE`, 호버 문구 4종 새 이름·층계 조건 일치. 인벤 창 캡처에서 패널 정상(`slots_ui.png`) |
| 2 | 해금·첫 아이템 | 통과 | `OnFloorBossFirstKill(1~4)`+`B04` 중복 호출 → 4칸 해금, EarringPlain·BraceletPlain·CharmPlain·StonePlain 각 1개(중복 지급 없음) |
| 3 | 장착 스탯 | 통과 | Hunter 귀걸이+Swift 팔찌+Plain 부적+Plain 마법석 장착 → 크리 +3·크피 +5·공속 +3·이속 +2·드롭 +3·방어 +2. 귀걸이 해제 시 크리 +0 복귀. 같은 칸 교체 시 이전 아이템 인벤 반환 |
| 4 | 사망 TIP 배너 | 통과 | 연속 사망 2회 → 둘째 줄 아래 TIP 이 다음 문구로 순환(HP 물약→귀환 주문서) |
| 5 | 퀘스트 창 접기 | 통과(일부) | J 창에서 Z: `[-]`→`[+] (1)`→`[-]` 토글 확인. **Enter 는 토글 안 됨**(UI.Select 바인딩 = Z·패드 A 뿐) |
| 6 | 약탈의 훈장 골드 | 통과 | `PlunderRules.Apply`: 100→150, 99→148(내림), 미장착 100 |
| 7 | 탭 점프 1.0·1.6 계단 | **불통과** | 씬에 직렬화된 `jumpForce=10`(코드 기본 13). 실측 상승: 탭(0.1~0.12초) 1.08~1.20, 풀홀드 1.62. 1.0 계단은 탭 통과, **1.6 계단은 탭 불가**(풀홀드 한계) |

## 발견 버그
- (P2) `SampleScene` 의 `PlayerController.jumpForce=10` 이 코드 기본 13 을 덮어 `JumpReachTests`(13 가정) 와 실제가 어긋남 → 탭 점프로 1.6 계단 불가. 13 으로 올릴지 계단 높이를 낮출지 결정 필요(TechDebt, 미수정: 씬 직렬화·밸런스 판단).
- (P4) 퀘스트 창 접기 안내/커밋 문구의 "Enter" 는 실제로 바인딩 없음 → Z 만 동작(문구 정정 또는 Enter 추가).
- (프로브 한정) `PlayerHealthManager.Die` 를 연속 2회 호출하면 `deathInputLock` 토큰이 덮어써져 InputLock 이 남음(실게임 경로 아님).

## 못 한 것
실제 보스 처치·도감 달성 경로, 장비창 드래그·클릭 장착, 패드 커서의 신규 4칸 이동, 사망 배너 육안 캡처 검토.
