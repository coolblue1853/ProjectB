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
