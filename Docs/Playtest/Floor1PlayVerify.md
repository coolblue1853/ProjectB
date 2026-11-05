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

## PV4 #7 재검증 (11-05)
`jumpForce` 10→13 으로 씬 수정 후 프로브 `_mp_jump.cs` 실측: 풀홀드 2.76·0.35초 2.71·탭 0.1초 1.91·0.05초 1.83. 1.0·1.6 계단 탭 통과(여유 0.2 이상), 풀 점프 4 미만이라 4칸 발판은 사다리 필요. 세이브 백업(`Logs/jump1/backup`)·복원, 플레이 정지. JumpReachTests 3건 통과.

# PV5 (11-05) PV2 미확인 보강
프로브 `Tools/probes/pv5_arrive.cs`·`pv5_keys.cs`, 결과 `Logs/pv5/*.txt`(gitignore). 세이브 백업(`Logs/jump1/backup`)·복원, 플레이 정지. 키 입력은 InputSystem 키보드 상태 주입.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| 도착 무적 2초 만료 | 통과 | 실제 `Portal.Use` 4회: 도착 후 1.0·1.8초 `Invincible=true`, 3.0초 `false`(페이드 포함 시작 기준) |
| 초반 3맵 몹 밀집 | 통과(부분) | 샛길 도착: 14유닛 안 몹 1(최근접 14.0), 일반 숲 도착: 0(최근접 14.2). 몹 총 22·20. 안전지대에 막힌 스폰 지점 8개 중 2개. **"전" 수치는 기능 끄는 토글이 없어 재현 못 함**(차단 대상 지점 수로 갈음). `inZone` 2~3은 같은 맵의 다른 도착점 주변 몹 |
| Backspace 튜토리얼 건너뛰기 | 통과 | 표시 중 `showing=True` → Backspace 입력 → `False` |
| J 퀘스트 창 | 통과 | J 1회 열림, 2회 닫힘. 튜토리얼 힌트의 `[J]` 문구 일치 |
| N 키 | 정정 | N 은 미니맵 키(`InputBindings` 예약). 상점 밖 확인 UI 는 `Buyback.CanUse()` 때문에 여전히 미확인 |

## 못 한 것
상점 안 확인 UI, 대장 실제 처치 15% 드롭, 도착 직후 실제 몹 추적 전투.


# PV6 (11-05) AUTO-5·6·7 미검증분
프로브 `Tools/probes/pv6_scale.cs`·`pv6_dbg.cs`, 결과 `Logs/pv6/scale.txt`(gitignore). 세이브 백업(`Logs/pv6/backup`)·복원, 플레이 정지. Lv1_NearForest 에서 실측.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| AUTO-5 정예 분리 생성 공격 ×1.5 | 통과 | 부모 없는 `EnemyDamageObject`(damage 300, `enemyOb`=몹)로 플레이어 피격: 일반 284 → `PromoteElite` 후 434, 비율 1.53(방어력 반올림 차). 보스 페이즈 배율은 EditMode 4건만(실보스 미확인) |
| AUTO-6 `CounterBuff` 컴포넌트 | 통과(부분) | `NotifyBlocked()` 전 배율 1.0 → 후 1.2, `Active=true`, 3.2초 뒤 `false`·1.0. **막기 판정 호출부 없음**(`NotifyBlocked` 호출처 0): 방패 홀드 방어 → 반격 실연결은 미구현, ShellShield 등 장착 경로 검증 불가 |
| AUTO-7 규칙 클래스 무기 연결 | 미연결(기록만) | 석궁 `AmmoMagazine`·투창 `ThrownSpear`·마도서 `TomePage` 는 순수 규칙+테스트뿐, 무기에서 사용 안 함(후순위 L8). 방패 쪽 연결은 큐 `AUTO-15`(=AUTO-7B)에 이미 있음 |

## 발견
- 트리거 충돌(물리)로는 새로 만든 판정이 플레이어에 닿지 않았다. 프로브 한정(생성 레이어·타이밍 추정)이며 실게임 몹 공격 경로는 아님. `OnTriggerEnter2D` 직접 호출로 배율만 확인.

## 못 한 것
실제 몹 분리 생성 탄막(`EnemyRoundAttackNode`) 피격, 실보스 격노 배율, 방패 홀드 입력 경로.


# PV7 (11-05) AUTO-12~17 플레이 확인
프로브 `Tools/probes/pv7_equip.cs`·`pv7_guard.cs`·`pv7_mob.cs`·`pv7_reflect.cs`, 결과 `Logs/pv7/*.txt`(gitignore). SampleScene·낮·Lv1_NearForest(-384,9 부근, 플레이어 근처에서만 몹 스폰), 세이브 백업(`Logs/pv7/backup`)·복원, 플레이 정지, 콘솔 에러 0. 키 입력은 InputSystem 상태 주입(G).

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| 방패 장착 훅 (AUTO-15) | 통과 | ShellShield 장착 → `ShieldGuard`·`CounterBuff` 존재, `PlayerGuard.Source` 연결 |
| G 홀드 방어 | 통과 | 미방어 피해 100 → 86 손실. G 0.5초 홀드 후 같은 100 → 26 손실(×0.3, blocked), 경직 없음 |
| 반격 버프 | 통과 | 막은 직후 `CounterBuff.Active`, 배율 1.2, 3.5초 뒤 해제 |
| 패링 (AUTO-17) | 통과 | G 누른 0.08초 안 피격 → 손실 0·parried, 반격 버프 활성. 반사 비율 0(ShellShield 기본값)이라 `reflected=0` |
| 몹 둔화 (AUTO-12) | 통과 | `MoveSpeedMul` 1 → 0.5(1.5초) → 1 복귀 |
| 석화 연쇄 제한 | 통과 | 석화1 BT 정지 → 0.5초 뒤 복귀, 면역 중 석화2 무시, 3초 뒤 석화3 정상 |
| 화상 | 통과 | 공격력 1000 → 4.6초 HP −600(150×4) |
| 적 대상 장판 (AUTO-13) | 통과 | 반경 3·틱 50·2초 → −200, 둔화 0.6 동시 적용 |
| 몹 반사 자세 (AUTO-14) | 통과 | 몹 손실 22002 → 플레이어 손실 6585(≈30%). 주의: `damage2Enemy` 의 피해 배열은 [최소,최대] 2칸 필요 |
| 플레이어 수면 | 통과 | `Apply(2)` → 최소 3초로 보정, InputLock 잡힘, 0.3초 뒤 피격에 해제·락 해제, 재적용 후 자연 종료 |
| 석궁·투창·마도서 래퍼 (AUTO-16) | **미연결** | `AmmoGimmick`·`ThrownSpearGimmick`·`TomePageGimmick` 를 쓰는 프리팹·코드 0건. 무기에 안 붙어 플레이 확인 불가 |

## 발견
- (P2) AUTO-16 래퍼 3종이 어떤 무기 프리팹에도 안 붙음 → 석궁/창/책 무기에 컴포넌트 부착 + 임시 HUD 연결이 남음(TechDebt 등록).

## 못 한 것
실제 몹의 반사 자세·화상·석화 사용 공격 경로(2층계 몹 미구현), 방패 패드(왼쪽 트리거) 입력, 방어 중 몹 실공격 피격, 방어 연출 그래픽(보류).

# PV8 (11-05) PV7 잔여: AUTO-16 래퍼 부착·FT-BOSSROOM
프로브 `Tools/probes/_pv8_*.cs`. 세이브 백업(`/tmp/savebk_pv8`)·복원, 플레이 정지, 콘솔 에러 0. 무기 호출은 실제 경로 `Weapon.MeleeAttack()`.
프리팹 부착(최소 수정, 컴포넌트 1개씩): `BanditShortbow`+`AmmoGimmick`, `Wood Spear`+`ThrownSpearGimmick`, `FaeWand`+`TomePageGimmick`. 석궁·투창·마도서 전용 무기는 아직 없어(Floor1Completion §5 후속) 같은 계열 임시 대용으로 붙임. 전용 무기가 생기면 그쪽으로 옮긴다.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| 석궁 장전 | 통과 | 탄 3/3 → 평타마다 −1(2·1) → 마지막 발 뒤 "재장전 1.2s" → 3/3 복귀 |
| 투창 | 통과 | 창 보유 → 평타 시 "투창 비행" → 착지 "창 박힘(함정)" → 함정 시간 뒤 "창 박힘" → `Retrieve()` 참 → 창 보유 → 다시 던짐. 박힌 중 평타는 재투척 안 함 |
| 마도서 페이지 | 통과 | 장 불→냉기→번개→불 순환, 전환 직후 3.0s 쿨 표시, 쿨 중 재전환 거부 |
| 보스 근접 귀환 거부 (FT-BOSSROOM) | 통과 | Lv1_DeepForest 에서 실제 보스(AncientEnt, 13.5 거리) 앞 `RecallCast.Begin` 거부(시전 안 됨). 보스 45 거리로 이동하면 시전 시작 |

## 발견
- (P3) 세 래퍼 효과는 상태·임시 HUD 텍스트뿐: 석궁 발사 차단·치명 확정, 투창 실제 투사체·함정 판정, 마도서 장별 평타는 후속. HUD(`OnGUI`)는 같은 위치라 한 번에 한 무기만 보임. 마도서 C 키는 다른 쓰임과 충돌 없음(grep).
- 못 확인: 키보드 C 입력 자체(프로브는 `TrySwitch` 직접 호출), 보스 근접 귀환 거부 시 화면 안내 문구 모습(그래픽 보류).

# PV9 (11-05) EditMode 통과분 플레이 모드 확인
프로브 `Tools/probes/pv9_extra.cs`·`pv9_burn.cs`·`pv9_ui.cs`·`pv9_fold.cs`(+ 재사용 `nest_setup`·`nest_drive`·`pv4_tapjump`·`pv4_death`). SampleScene·낮, 세이브 백업(`/tmp/savebk_pv9`)·복원, Loc 원래(English) 복원, 플레이 정지.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| 장신구 세이브/로드 왕복 | 통과 | 1·2층 해금(mask 3) → `Load()` 후 mask 3 유지·SaveBlocked 거짓. 폐기 `TitleWanderer` 를 칸0에 심고 Load → 인벤 0→1, 다시 Load → 1(재지급 없음), 저장 키 칸0 비워짐 |
| 화상 재적용 | 통과 | 재적용 3회 뒤 자식 `PosionAttack` 1개. 총 손실 900(150×6, 재적용이 기존 틱을 갱신하며 지속 연장. 3중첩이면 ~1800) |
| NestHuntUI 습격 문구 | 통과(표시) | Lv1_NormalForest 흔적 3개 → Stage=Wave, "Nest raid wave 1/2 / Monsters left: 3 - defeat them all" 표시 |
| NpcQuestTracker 캐시 | 통과(일부) | 추적 박스 표시·`_lastText` 에 퀘스트+구역 문구 보관. 캐시로 TMP 재계산 생략하는 횟수는 미측정 |
| Loc 영어/한국어 전환 | **실패→수정** | 신규 24개 키(습격 문구·장비 슬롯 해금 등 LocUi)가 `I2Languages.asset` 에 없어 한국어 모드에서도 영어 표시. `sync_i2_terms.cs` 재실행(added 24, 옛 키 1 정리)으로 해결 — 재검증 안 함 |
| 사망 TIP | 통과 | 사망 2회 → TIP 문구 순환(보스 패턴→HP 물약 퀵슬롯). 배너 alpha 육안 미확인 |
| 퀘스트 접기 | 통과 | `QuestFoldRules.Toggle` False→True→False, ES3 `npcq.folded='1|PV9'` 저장 |
| 탭 점프 | 통과 | C 0.1초 탭 상승 1.83(1.0·1.6 계단 통과) |

## 발견
- (P3) LocUi 키 추가 때 I2 에셋 동기화(`sync_i2_terms.cs`)를 안 해서 한국어 모드에서 영어가 나옴 → 수정함. 재발 방지: 키 추가 후 sync 실행 또는 EditMode 테스트(asset 에 term 존재)를 후속으로.
- 폐기 아이콘 `Item/TitleWanderer` 로드 Error 1건(프로브가 폐기 아이템을 인벤에 넣어 발생, 실플레이 경로는 아님).

## PV9 추가 (11-05, AUTO-44) 미검증 통합 #1·3·4·5
프로브 `Tools/probes/_pv9_lt.cs`·`_pv9_mob.cs`·`_pv9_slot.cs`·`_pv9_loc.cs`, 결과 `Logs/pv9/*`. SampleScene·낮, 세이브 백업(`Logs/pv9b_save`)·복원(3파일 cmp 동일), 슬롯 pref 원복(2), 플레이 정지, 콘솔 에러 0.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| #4 패드 LT (AUTO-46) | 통과 | 방패 장착 중 가상 패드 LT 1.0 → `ShieldGuard.Holding` 참, SkillS 쿨 0 유지(무시). LT 뗌 → Holding 거짓. 키보드 S → SkillS 쿨 4.70(발동). 실기 패드는 미검증(가상 주입) |
| #1 실 몹 공격 방어 | 통과 | SporeShroom 을 1.2 거리에 고정. 미방어 6초 19×5=95. G 홀드: 첫 피격 19→6(≈×0.3, blocked). 이후 피격 19 → 방벽 3초(`BarrierDuration`) 지나 Holding 거짓이라 설계대로. 재누름 필요 |
| #3 슬롯 전환 신규 키 | 통과 | 슬롯2 `equip.extraUnlock`=5·`npcq.folded`='S1mark', 슬롯3 =6·'S3mark' 기록 → 2→3→2→3→1 전환(SetNextSlot+재시작)마다 값 그대로, 슬롯1 은 키 없음(교차 오염 0). `settings.bindings` 는 공용 `SaveFile.es3` 라 슬롯과 무관(설계) |
| #5 Loc 한국어 | 통과(1건 예외) | `LocUi.Entries` 376개 중 한국어 일치 375, 영어 모드 불일치 0, 언어 원복 Korean. 예외: `[Sealed]`→`[봉인]` 안 바뀜(I2 가 대괄호 용어를 특수 처리하는 듯) |

- #2 정예 분리 생성 탄막 피격: 이번에 안 함(정예 둥지 소환 프로브 필요, 중간 규모). 다음 묶음으로 남김.

## PV10 (11-05, AUTO-48) 미검증 통합 #2·#15
프로브 `Tools/probes/_pv10_a.cs`·`_pv10_b.cs`·`_pv10_eq.cs`·`_pv10_c.cs`·`_pv10_keyC.cs`·`_pv10_d.cs`. SampleScene·낮·Lv1_TowerOutskirts, 세이브 3파일 백업(`Logs/pv10b_save`)·복원(cmp 동일), 입력 백그라운드 설정 원복, 플레이 정지, 콘솔 에러 0.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| #2 분리 생성 탄막 정예 ×1.5 | 통과(보스 몸 대용) | `EnemyRoundAttackNode`(fire, 반경 0.5, 3발)를 보스 Gatekeeper 의 BT 에서 꺼내 `OnStart()` 실행. 일반 피격 45(5/5) → `PromoteElite` 후 73(5/5). 역산 원피해 56, 방어 11: 56×1.5−11=73 로 정확히 ×1.5 적용(표면 비율 1.62 는 방어력 차감 때문) |
| #15 키보드 C 마도서 | 통과 | FaeWand 장착(`TomePageGimmick`), 가상 키 C 주입 → 장 냉기→번개 전환(쿨 3초 후 입력). 직접 호출 아님 |
| #15 보스 근처 귀환 거부 문구 | 통과 | 보스 4.3 거리에서 `RecallCast.Begin` 거부(시전 안 됨), PickupLog 에 "보스가 가까워 귀환할 수 없다" 활성 표시(alpha 1.0). 문구 모습(그래픽)은 미검토 |

- 한계: 1층계 일반 몹에는 분리 생성 탄막 노드가 없고(노드 쓰는 8종은 전부 보스 프리팹) 보스는 `PromoteElite` 가 거부돼, 프로브에서만 `isBossMob` 를 잠시 껐다 켜서 정예 판정을 만들었다. 보스 페이즈 배율은 두 경우 동일(1단계).
- 발견: 실게임에서 정예 + 분리 탄막 조합은 현재 존재하지 않음(2층계 몹 추가 때 재확인 대상).

## PV11 결과 (11-05, AUTO-51~54 변경분 확인)
프로브 `Tools/probes/_pv11_save.cs`·`_pv11_day.cs`·`_pv11_eq.cs`·`_pv11_hud.cs`·`_pv11_mv.cs`·`_pv11_atk.cs`, 결과 `Logs/pv11/atk.txt`·`hud.png`. SampleScene, 세이브 3파일 백업(`Logs/pv11b_save`)·복원(cmp 동일), 플레이 정지, 콘솔 에러 0.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| AUTO-53 `SaveUtil.LoadOr` | 통과 | `ForgeState.Pity` 7 저장→7, 키 삭제→0(기본값), 원복. `UnlockMask` 5→5, 삭제→0. `Floor1Clock.Day` 키 없음→1(기본값) |
| AUTO-52 임시 HUD 단일화 | 통과 | 장착 시 `Wood Spear`=[창 보유], `BanditShortbow`=[탄 3/3], `FaeWand`=[장: 불]. 3종 모두 `GimmickHud.Draw` 경유. 화면 모습은 스샷 `hud.png`(그래픽 보류 임시) |
| AUTO-51 소환수 | 통과 | `WolfSpirit`(SummonRunner) 소환 3초: 몹 HP -31 |
| AUTO-51 유도탄 | 통과 | `WispOrb`(HomingOrb): 3초 -48 |
| AUTO-51 궤도 불 | 통과 | `WispRing`(OrbitWisps) 반경 안 몹: 3초 -68 / -295 / -232(겹침 포함). 반경 밖(1.8)이면 0, 설계대로 |

- 한계: 프리팹 직접 소환(장착 무기 스킬 키 경로 아님). 타격 판정 경로(`TryGetComponent`)는 동일.

# 미검증 통합 (11-05, AUTO-43)
PV1~PV9 의 "못 한 것·미연결·미검증" 정리. 예상 사용량: 소=프로브 1개·5분, 중=프로브 2~3개, 대=신규 구현 필요. 해소=후속 PV 에서 확인됨(취소선 대신 "해소" 표기).

| # | 항목 | 원인 | 검증 방법 | 선행 조건 | 예상 | 상태 |
|---|---|---|---|---|---|---|
| 1 | 실 몹 공격 경로로 방어 중 피격 | PV7 은 `damage2Player` 직접 호출 | 몹 1마리를 붙여 G 홀드 중 실피격 수치 | 없음(Lv1_NearForest) | 소 | 해소(PV9 추가) |
| 2 | 정예 분리 생성 탄막(`EnemyRoundAttackNode`) 피격 ×1.5 | PV6 은 부모 없는 판정 직접 생성 | 정예 승격 몹의 실탄막 피격 손실 비교 | 정예 둥지 소환 프로브(`nest_*`) | 중 | 해소(PV10, 보스 몸 대용 ×1.5 확인. 1층계 일반 몹엔 노드 없음) |
| 3 | 세이브 슬롯 전환 후 신규 키 유지 | 신규 ES3 키(장신구 해금·접기·bindings) 슬롯별 처리 미확인 | 슬롯 A 저장→B 전환→A 복귀 후 키 값 대조 | 슬롯 3개 백업(`SaveSlotCheck.md`) | 중 | 해소(PV9 추가) |
| 4 | 방패 패드(LT) 입력 | LT 가 SkillS 와 겹침(InputAudit P1) | 가상 패드 LT 주입 후 방어 vs 스킬 발동 | 가상 패드 프로브(`pad_quick_probe` 참고) | 소 | 해소(PV9 추가) |
| 5 | Loc 한국어 전환 재검증 | PV9 에서 sync 후 재확인 안 함 | Loc 한국어로 24키 표시 확인 | 없음 | 소 | 해소(PV9 추가) |
| 6 | 퀘스트 접기 Enter/패드 키 | UI.Select 에 Enter 없음(PV4 #5). AUTO-60 대조: `Enter` 는 별도 액션으로 바인딩되어 있고(UI.Select 아님), 패드 A 는 Select 에 있음 → 사유 일부 무효, 실제 토글 미확인 | Enter·패드 A 토글 | 제안 확정(OpenQuestions) | 소 | 부분 통과·실패(PV11 AUTO-61: Z·패드 A 토글, Enter 무동작 → TechDebt) |
| 7 | 실보스 격노 배율·페이즈 | EditMode 4건만 | 보스방에서 HP 구간별 피격 수치 | 보스 대기 시간 | 중 | 보류(긴 시간) |
| 8 | 실 보스 처치 첫 보상·도감 B04 달성·대장 15% 드롭 | 시간·확률 의존 | 실제 처치/통주 | 통주 봇(사용자 보류) | 대 | 보류 |
| 9 | 실사망 `Die()` 경로 BossNear 페널티 연결 | PV3 은 규칙 직접 호출. AUTO-60 대조: `PlayerHealthManager.cs:217` 에 `BossNearPlayer()` 연결 코드 있음 → 연결은 완료, 실행 확인만 남음 | 보스 근처 사망 후 골드 7% | 보스 접근(프로브로 HP 0 강제) | 중 | 해소(PV11 AUTO-61: 연결 확인. 페널티는 골드 아닌 EXP 5%, 보스 구분 없음 — 11-05 결정) |
| 10 | 장비창 드래그·클릭 장착, 패드 커서 신규 4칸 이동 | UI 입력 합성 어려움 | 패드/마우스 입력 합성 | UI 프로브 | 대 | 보류 |
| 11 | 밤 시간대 확인 | 테스트는 낮 고정 | 밤 전환 후 스폰·화면(스폰·상태만, 그래픽 판정 제외) | 시간 설정 | 중 | 해소(PV11 AUTO-61, 그래픽 제외) |
| 12 | 코요테·몬스터 6종 원거리·돌진 | 행동 미구현 | 구현 후 | 구현 | 대 | 보류(2층계/미구현) |
| 13 | 몹 반사·화상·석화 실 공격 경로 | 2층계 몹 미구현 | 몹 구현 후 | Floor2 병합 | 대 | 보류(2층계 몹) |
| 14 | 방어 연출·아이콘 2개·툴팁 위치·사망 배너 육안 | 그래픽 | 사용자 검토 | 그래픽 | - | 보류(그래픽) |
| 15 | 키보드 C(마도서) 실입력·보스 귀환 거부 안내 문구 | PV8 은 직접 호출 | 키 주입 | 없음 | 소 | 해소(PV10. 문구 그래픽은 미검토) |
| 16 | 탭 점프 1.6 계단 | PV4 불통과 | PV9 에서 "탭 점프 통과"로 갱신됨 | - | - | 해소(PV9) |
| 17 | 석궁·투창·마도서 래퍼 부착 | PV7 미연결 | PV8 에서 통과 | - | - | 해소(PV8) |
| 18 | 방패 막기 → 반격 버프 호출부(`NotifyBlocked`) | PV6 호출처 0 | PV7 G 홀드로 반격 활성 확인 | - | - | 해소(PV7) |
| 19 | 사망 TIP 순환·퀘스트 접기 토글 | PV4 일부 | PV9 통과 | - | - | 해소(PV9) |
| 20 | 폐기 아이콘 `TitleWanderer` 로드 에러 | 프로브 한정 | 실경로 아님 | - | - | 무시(기록만) |
| 21 | 소환수 스킬을 장착 무기 키 경로로 발동 | PV10/PV11 은 프리팹 직접 소환 | FangDagger 장착 후 가상 키 F | 없음 | 소 | 해소(PV12). 실기 패드 RT 는 미검증 |

## 다음 PV 묶음(AUTO-44) 대상 선정
1. 항목 1 실 몹 방어 중 피격
2. 항목 2 정예 분리 생성 탄막 피격
3. 항목 3 세이브 슬롯 전환 후 신규 키 유지
4. 항목 4 방패 LT 입력(InputAudit P1 확인)
5. 항목 5 Loc 한국어 재검증
이유: 그래픽·2층계 몹·긴 대기 불필요, 한 세션에서 프로브 5개 안. 실패는 `TechDebt.md` 등록.
(PV10 으로 #2·#15 해소되어 위 묶음 중 2건 완료, 아래 PV11 로 이월.)

## PV11 대상 선정 (AUTO-60, 11-05, 미실행)
1. 항목 6 퀘스트 접기 Enter·패드 A 토글 (소)
2. 항목 9 실사망 `Die()` 의 BossNear 골드 페널티 (중, 보스 근처 HP 0 강제)
3. 항목 11 밤 시간대 스폰·상태 (중, 그래픽 제외)
이유: 셋 다 프로브로 확인 가능하고 그래픽·2층계 몹 불필요. 항목 10(패드 커서 합성)·12·13·14 는 제외. 항목 7·8 은 긴 대기로 보류 유지.

## PV11 추가 (11-05, AUTO-61) 미검증 통합 #6·#9·#11
프로브 `Tools/probes/_pv11_q_open.cs`·`_pv11_q_state.cs`·`_pv11_keyZ.cs`·`_pv11_keyEnter.cs`·`_pv11_padA.cs`·`_pv11_die.cs`·`_pv11_night2.cs`, 결과 `Logs/pv11/die.txt`·`night2.txt`. SampleScene, 세이브 3파일 백업(`Logs/pv12b_save`)·복원(cmp 동일), 입력 백그라운드 원복, 플레이 정지, 콘솔 에러 0.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| #6 퀘스트 접기 Z | 통과 | Lv20 퀘스트 창 진행 탭 머리글 행(rows 4): 가상 Z → 접힘(rows 3, IsFolded 참) → Z 다시 → 펼침(rows 4) |
| #6 퀘스트 접기 패드 A | 통과 | 가상 패드 buttonSouth 같은 토글 2회 확인(rows 4→3→4). 실기 패드는 미검증 |
| #6 퀘스트 접기 Enter | **실패** | 가상 Enter → rows 4 그대로. `NpcQuestWindow` 는 `UI.Select`(Z·패드 A)만 쓰고 Enter 는 별도 액션. 하단 안내 문구는 "Z/Enter 로 접기" 라서 거짓 → TechDebt |
| #9 보스 근처 `Die()` | 통과(전제 수정) | Lv1_TowerOutskirts, Gatekeeper 3유닛 앞에서 `BossNearPlayer()` 참 → `Die()`: EXP 1000→950(-50, 5%), 골드 10000 불변. 보스 없는 곳 Lv20도 동일 -50. Lv8 은 100→100(0). 큐의 "골드 7%" 는 옛 설계(11-05 결정: EXP 5%, Lv1~10 0, 보스 구분 없음) |
| #11 밤 스폰 | 통과(상태만) | Lv1_NearForest 스포너 13개(낮형 8·밤형 5). 낮: GreenHurb 3+Rabbit_BD 13. 밤 전환 12초 후 ScareCrow 구역: RatChief_BD 10+Scarecrow_BD 8. 낮형 몹 사라지고 밤형이 스폰. 화면·밝기는 미판정 |

## PV12 (11-05, AUTO-62) 소환수 스킬 키 경로
프로브 `Tools/probes/_pv12_eq.cs`·`_pv12_summon.cs`, 결과 `Logs/pv12/summon.txt`. 세이브 복원·플레이 정지·에러 0.

| 항목 | 결과 | 수치·관찰 |
|---|---|---|
| FangDagger 장착 + 가상 키 F(스킬 슬롯 3 "Howl of the Pack") | 통과 | 발동 전 쿨 0·스태미나 100 → +0.5초 쿨 4.3·스태미나 95(소모 5). `WolfSpirit`(SummonRunner) 2마리 0.3초에 생성, 오른쪽으로 1.0유닛/0.2초(실시간) 달리다 2.1초에 소멸(수명 후 0마리) |
| 몹 피격 | 통과 | 앞 2.5유닛에 둔 GreenHurb HP 10 → -58(피해 68), 소환수가 지나간 뒤 변화 없음 |

- 한계: 실기 패드 RT 는 미검증. 소환 위치·방향은 플레이어 `localScale.x` 기준(코드 확인).

## 미검증 통합 갱신 (11-05, AUTO-75)
- #6 퀘스트 접기: Z·패드 A 통과(가상), Enter 실패(문구 "Z/Enter" 거짓, TechDebt 등록). #9 BossNear 페널티 통과(PV11). #11 밤 스폰 해소(상태만).
- #2·#15 PV10 해소. 보류 유지: #7 보스 격노(긴 시간), #8 첫 보상(통주 봇), #10 장비창 드래그·패드 커서, #12·#13 2층계 몹. 실기 패드는 전부 미검증.
- 후속 PV12: 소환수 키 경로(FangDagger F) 통과.

## PV13 (11-05) Cooldown 교체 확인 (AUTO-83·84)
프로브 `Tools/probes/_pv13.cs`, 결과 `Logs/pv13/result.txt`. 세이브 백업→복원·플레이 정지·콘솔 에러 0.

| 항목 | 결과 | 관찰 |
|---|---|---|
| ContactDamage (AUTO-83) | 통과 | Scarecrow 를 플레이어에 겹쳐 3.5초: 피격 4회, 간격 약 0.9초(`noDamgeTime` 1) — 0.1초 검사 간격이 연타로 새지 않음 |
| TempMobDash (AUTO-84) | 통과(제한) | 4칸 거리에서 0.7초에 돌진 1회, 9초 안에 2회째 없음(돌진 후 사거리 2 미만/쿨 4.15초 내). 재돌진 간격은 미확인 |
| NightRaid (AUTO-83) | 통과 | `DebugBegin` true → 6초 뒤 active, wave 1, 동시 생존 1. 마감·다음 스폰 쿨 만료는 짧아서 미확인 |

- 한계: FJ-6(석궁 탄수·마도서 장 HUD)은 플레이 미검증(EditMode `HudText` 만).

### PV13 추가 (AUTO-93·94, 11-05)
프로브 `Tools/probes/_fj6_hud.cs`(→`Logs/fj6/result.txt`), `Tools/probes/_fj_spear.cs`(→`Logs/fj_spear/result.txt`). 세이브 백업→복원·플레이 정지·콘솔 에러 0.

**FJ-6 HUD (AUTO-93) 통과** — Loc 한국어. 장착 후 `GimmickHud` 에 쓰는 `HudText` 문자열:

| 무기 | 동작 | HUD 전 → 후 |
|---|---|---|
| SentryCrossbow(313) | 평타 1·2·3발 | `탄 3/3` → `탄 2/3` → `탄 1/3` → `재장전 1.2s` (재장전 중 평타는 무반응, 이후 `탄 3/3` 복귀. 8발 시도에 두 번 사이클) |
| FairysGrimoire(315) | `TrySwitch` 3회(쿨 3초 간격) | `장: 불` → `장: 냉기 (3.0s)` → `장: 번개 (3.0s)` → `장: 불 (3.0s)`, 쿨 중 재전환 거부(ok=false, `불 (2.8s)`) |
| HuntersJavelin(314) | 평타 1회 | `창 보유` → `투창 비행` → +1s `창 박힘(함정)` → +6.5s `창 박힘` |

- Loc 10키(`Reloading {0}s`·`Ammo {0}/{1}`·`Spear` 4종·`Fire/Frost/Lightning`·`Page: {0}`) 모두 한글로 출력. 스크린샷 미촬영(문자열 로그).

**투창 착지 (AUTO-94) 통과** — `FlightTime(min(막힘 거리, 6) / 18)`. 실제 착지(`InFlight` 종료)까지 게임 시간:

| 경우 | 막힘 거리 | 기대 비행 | 실측 착지 | 착지 지점 계산 |
|---|---|---|---|---|
| 바닥(열린 지면, 앞 막힘 없음) | 6.000(사거리) | 0.333s | 0.334s | 던진 x + 6 |
| 벽(BaseTileWall, 사거리 안) | 5.670 | 0.315s | 0.315s | 던진 x + 5.67 |
| 허공(4칸 위, 막힘 없음) | 6.000 | 0.333s | 0.335s | 던진 x + 6 |
| 사거리 밖 벽(9칸) | 6.000 | 0.333s | 0.334s | 사거리 6 에서 멈춤 |

- 모든 경우 착지 즉시 `Planted`·`TrapActive` 참. 사거리 밖 투척은 사거리(6)로 잘려 처리됨.
- 한계: 착지 지점은 계산값뿐, 박힌 창/함정 오브젝트는 생성되지 않음(`TechDebt.md` 등재). 레이는 수평이라 바닥·허공 차이 없음. 프로브는 플레이어를 키네매틱으로 고정하고 `Physics2D.SyncTransforms` 후 측정(첫 시도는 낙하·동기화 누락으로 무효였음).

### PV13 추가 (AUTO-95, 11-05)
프로브 `Tools/probes/_dash_cd.cs`(→`Logs/dash_cd/result.txt`). 세이브 백업→복원·플레이 정지·콘솔 에러 0.

**TempMobDash 재돌진 쿨 (AUTO-95) 통과** — Scarecrow+TempMobDash 를 플레이어(키네매틱 고정) 4칸 옆에 두고, 돌진 후 2.3칸 안이면 4칸으로 되돌림(5회). 게임 시간 기준:

| 돌진 | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| 돌진 시작(초) | 0.65 | 4.80 | 8.95 | 13.10 | 17.25 | 21.40 |
| 웅크림 시작(초) | 0.30 | 4.45 | 8.60 | 12.75 | 16.91 | 21.06 |

- 간격 5개 모두 4.15초 = 웅크림 0.35 + 돌진 0.3 + 쿨 3.5(기대 하한과 일치). 연속 돌진·미발동 없음, 첫 돌진은 `OnEnable` 0.3초 지연 뒤.

### PV13 추가 (AUTO-96, 11-05)
프로브 `Tools/probes/_nr_full.cs`(→`Logs/nr_full/result.txt`). 세이브 백업→복원·플레이 정지·콘솔 에러 0.

**NightRaid 끝까지 (AUTO-96) 통과** — 밤 전환(`Night()`) 후 SideRoad 에서 `DebugBegin`(오늘 구역은 NormalForest 로 추첨돼 디버그 시작 사용), 스폰 1초 뒤 `ReleaseEnemy(true,true)` 로 처치. Lv9, 게임 시간:

| 시각(초) | 이벤트 | 값 |
|---|---|---|
| 0.0 | 시작 | active, wave 1 (SideRoad 2웨이브×4마리) |
| 1.0 | 첫 스폰 | 시작 후 1초(`_nextSpawn` 1f) |
| 5.9 / 6.6 | 스폰 #2 / #3 | 간격 0.70 (`SpawnInterval`, Cooldown 교체 후 불변) |
| 14.3 | 스폰 #4 → 15.3 웨이브 2 | 웨이브 2 첫 스폰 +1.0 |
| 16.3 / 24.7 / 25.4 | 스폰 #5 / #6 / #7 | #6→#7 간격 0.70 |
| 34.6 | 종료(Complete) | 처치 7, 최대 동시 2 |

- 보상: 골드 +40, EXP +35, Potion +1 = `Floor1Raids.csv` SideRoad 행과 일치.
- 종료 후 8초: 신규 스폰 0, raiders 0, alive 0, active=false. `Day()`+시간 진행 후 isDay=true, active=false, raiders 0 (낮 복귀 정상).
- 이상: 한쪽 지면 없는 곳에서 스폰 실패 8초 대기로 7마리만 스폰, 웨이브 사이 6~8초 정체 → `TechDebt.md` 등재(보상은 정상).

## 9차 미검증 통합 갱신 (11-05, AUTO-105)
- 해소: PV13 석궁 탄수·마도서 장 HUD(AUTO-93), 투창 착지(AUTO-94), 대시 재돌진 쿨(AUTO-95), NightRaid 흐름(AUTO-96), CF-12 이동 캔슬 지연 실측(AUTO-97).
- 남음: 실기 패드 전반, 313~315 아이콘(그래픽 제외), 투창 함정 피해 미구현(TechDebt AUTO-94).

## PV14 (11-05) AUTO-106~110 실기 확인
프로브 `Tools/probes/_dash_cd.cs`·`_nr_full.cs`·`_fj_spear.cs`·`_spear_cap.cs`, 결과 `Logs/dash_cd`·`Logs/nr_full`·`Logs/fj_spear/result.txt`(gitignore). 세이브 백업(`Logs/auto110`)·복원, 플레이 정지, 콘솔 에러 0. EditMode 900 통과.

- **TempMobDash (AUTO-108)**: 돌진 시작 간격 4.15초 ×5, 쿨 기대값 4.15 이상 만족(AUTO-95와 동일). 풀링 재사용 경로는 별도 미확인.
- **NightRaid (AUTO-106·107)**: 정의 8마리 모두 스폰(이전 7마리), 8.22초에 종료(이전 34.6초, 웨이브 사이 6~8초 정체 해소). 보상 골드 +40·EXP +35·Potion +1 동일, 종료 후 8초 신규 스폰 0, 낮 복귀 정상. 해소.
- **투창 함정 (AUTO-110)**: 바닥·허공·사거리 밖 3경우에서 착지 지점(-479.5)에 함정 생성, 착지 후 약 4초에 소멸(live 0), 5개 연속 생성 시 live 3 상한. 벽 안 경우는 맵에 벽이 없어 미확인.
- **미확인 유지**: `_l3InUi`(AUTO-109 a, 실기 패드 필요), `CountdownTimer` 음수 dt(EditMode 케이스로만 확인), 함정의 실제 적 피해 틱.
- **Floor2Data 헤더 LogError**: `Floor2Data.MergeRows` 가 헤더 열 순서가 다르면 의도적으로 LogError 후 병합 거절. 발생 지점은 `Floor2DataTests.MergeRows_RejectsMismatchedHeaderOrder` 한 곳뿐이며 이미 `LogAssert.Expect` 처리됨(의도된 로그, 수정 불필요).

### AUTO-115 1층계 구간별 자동 점검 (11-05)
프로브 `Tools/probes/_f1_fullrun.cs`(템플릿)·실행 `Tools/f1_fullrun.sh <맵> <시작Lv> <끝Lv>`, 로그 `Logs/f1_fullrun.log`(gitignore). 세이브 백업(`Logs/cfbk`)·복원, 플레이 정지. 낮, 플레이어 레벨을 구간 시작 Lv·EXP 0 으로 설정 후 `MapInfo.GoToMap` → 스포너 위로 이동·`CycleCheck` → 대표 몹 10마리(종류당 최대 3) 실제 처치(`damage2Enemy` 반복, 정상 사망·EXP 경로). **시간은 측정이 아니라 모델 환산**: 마리당 접근 3초 + 필요 타수 x 0.4초(실측 아님), k=1, 퀘스트·보스·포탈 이동·줍기는 미포함. 골드는 바닥 드롭이라 +0(줍기 안 함). 시작 마을→2층계 포탈 전 구간 연속 주행과 보스 전투는 하지 못함(구간 단위 표본만).

| 구간(맵) | Lv | 표본 | EXP/마리 | 초/마리(모델) | EXP/분 | 문서 모델 EXP/분 | 편차 | 구간 필요 EXP | 처치 수 | 사냥 시간(k=1) |
|---|---|---|---|---|---|---|---|---|---|---|
| 샛길(NearForest_SideRoad) | 1->3 | 10 | 8.2 | 4.0 | 123 | - | - | 1,150 | 약 140 | 약 9분 |
| 일반 숲 | 3->6 | 10 | 10.4 | 5.1 | 122 | - | - | 3,450 | 약 332 | 약 28분 |
| 깊은 숲 | 6->9 | 10 | 22.1 | 10.9 | 122 | 150 | -19% | 10,600 | 약 480 | 약 87분 |
| 늪(Lv1_Swamp·Lv1_MistSwamp) | 9->12 | 0 | - | - | - | 270 | 막힘 | 22,500 | - | - |
| 산적 성채 | 12->13 | 10 | 50.2 | 7.4 | 407 | 420 | -3% | 14,000 | 약 279 | 약 34분 |
| 탑 근교 | 13->15 | 10 | 64.6 | 7.2 | 538 | 420 | +28% | 37,500 | 약 580 | 약 70분 |

- 막힘: **늪** 두 맵 모두 `GoToMap` 성공·활성 스포너 8~9개·스포너 위 이동과 `CycleCheck` 후 12~25초 대기에도 몹 0(다른 구간은 같은 절차로 120~250마리). 원인 미확인(스포너 조건·좌표 문제 추정) -> TechDebt 후보.
- 해석: 표는 퀘스트 EXP 를 빼지 않은 구간 전체 EXP 라서 문서 §1 의 "반복 사냥 EXP"(퀘스트 차감)와 직접 비교하지 못하고, 분당 EXP 만 비교한다. 편차는 모두 +-30% 안(탑 근교 +28% 가 가장 큼, 대방어 골렘이 EXP 가 높고 타수 모델이 0.4초 고정이라 과대).
- 이상: 표본 중 일부 `Ghost_BD` 가 HP 230/84 로 갈린다(정예/변종 추정, 미확인). 스포너 일괄 `CycleCheck` 는 활성 몬스터 130~260 으로 `[PB][Spawn][ANOMALY]` 로그를 낸다(프로브가 만든 인위적 상태, 정상 플레이 아님).
- 콘솔: 위 ANOMALY 외 새 에러 없음.
