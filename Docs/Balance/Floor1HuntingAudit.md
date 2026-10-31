# 1층계 사냥 루프 수치 감사 (2026-10-31)

계산기: `Assets/Editor/Dev/HuntingAudit.cs`, `HuntingDensityAudit.cs`. 자동 실행: `bash Tools/u.sh cs Tools/probes/hunt_estimate.cs`, `hunt_density.cs`. 조정 전 입력은 `Floor1HuntingBaseline.csv`에 보존했다. 프로브 원본과 실행 결과는 `Tools/probes/hunt_*.cs`, `Logs/playtest/hunt_*`이며 일회성 프로브는 커밋에서 제외한다.

## 맵별 일반몹 슬롯·화면 밀도

슬롯 합계는 맵 전체 발판의 정원이다. 플레이어가 들어간 트리거만 스폰하며, 발판 중심 표본에서 활성화되는 일반몹 정원은 최대 31~44슬롯이다(보스·채집물 제외). 전체 위치의 상한은 아니다. 트리거 경계 전수 점 겹침 검사에서는 샛길·일반 숲·산적 성채가 낮/밤 모두 58슬롯이었다. 플레이어 콜라이더 크기·이탈 후 페이드 잔존 몹도 별도로 고려해야 한다. 화면 수치는 20×11.25u 카메라, 플레이어 발 위 2u 중심, 기존 발판 중심에서 x 지터 ±1을 적분한 초기 스폰 기대값이다. 실제 이동·AI 순찰 중에는 달라진다.

| 맵 | 낮 슬롯 전→후 | 밤 슬롯 전→후 | 낮 화면 전→후 | 밤 화면 전→후 | 리젠 전→후(초) | 발판 중심 활성 슬롯 최대(낮/밤) |
|---|---:|---:|---|---|---|---|
| Lv1_NearForest_SideRoad | 54→157 | 57→150 | 0~7→6~12 | 0~8→6~12 | 10→10 | 44/44 |
| Lv1_NearForest | 30→72 | 27→65 | 0~7→6~12 | 1~8→6~12 | 10→10 | 31/31 |
| Lv1_NormalForest | 31→157 | 57→150 | 0~4.25→6~11.78 | 0~8→6~12 | 10→10 | 44/44 |
| Lv1_DeepForest | 19→72 | 27→65 | 0~7→6~12 | 1~8→6~12 | 10→10 | 31/31 |
| Lv1_Swamp | 16→91 | 14→73 | 0~3→6~7 | 1~3→6~7 | 10~12→10 | 43/31 |
| Lv1_BanditFortress | 38→157 | 36→150 | 0~5→6~12 | 0~7→6~11.78 | 10→10 | 44/44 |
| Lv1_TowerOutskirts | 18→72 | 20→65 | 0~5→6~12 | 0~5→6~12 | 10→10 | 31/31 |

## 600초 사냥 추정

지역 대표 무기 +0·세트/방어구/스킬/요리 없음, 공격속도 보정 1, 탐험가 칭호 0·상승 부하 원값. 레벨 성장 공격/방어는 반영한다. 기본 공격 판정의 피해 배율·다단히트 수·치명타·방어·레벨차·추가타를 계산한다. 무기 기믹 추가 효과, 다수 대상 동시 타격, 실제 피격 경직/빗나감은 모델에 넣지 않는다.

600초 동안 일반 스포너 슬롯을 캠프끼리 교차 순회하며 한 마리씩 처리한다. 마리당 접근·줍기 3초, 초기 스폰 1초, 죽은 슬롯은 다음 전역 리젠 틱에서 재충전한다. 정예 4%는 HP×4·방어×1.5·EXP×4·골드×3 기대값으로 적분한다. 맵 전역 이동을 물리적으로 재생한 결과가 아니므로, 경로·장비 확보·캠프 재진입 지연은 미검증이다.

골드/분은 **직접 드롭 기대액**이고 실제 줍기 성공률은 100% 가정이다. 기본 드롭표 판매액은 별도이며, 인벤토리 공간·판매 회수·정예 보장 장비·추가 드롭은 제외했다. 물약은 `min(1, 일반 TTK/6초)`회 접촉/처치(정예 별도)와 실제 받는 피해 공식으로 HP 손실을 추정한다. 자연 회복·음식·레벨업 회복을 빼지 않은 접촉 빈도를 가정한 회복량이며 실제 피격의 상한이 아니다. Lv2는 소형(120/25G), 나머지는 중형(220/45G), 실제 보충은 정수 올림한다.

| 맵 | 시간 | Lv/무기 | 처치/분 전→후 | EXP/분 전→후 | 직접 G/분 전→후 | 판매 잠재 G/분(후) | 물약/10분(후) | 물약비/직접골드 |
|---|---|---|---:|---:|---:|---:|---:|---:|
| Lv1_NearForest_SideRoad | 낮 | 2/RustedSword | 2.8→2.8 | 45.4→45.4 | 17.5→17.5 | 127.4 | 4.0 | 57.3% |
| Lv1_NearForest_SideRoad | 밤 | 2/RustedSword | 3.8→3.8 | 56.0→77.3 | 25.1→34.8 | 152.5 | 5.5 | 39.6% |
| Lv1_NearForest | 낮 | 2/RustedSword | 2.8→2.8 | 47.9→47.5 | 19.2→18.9 | 123.0 | 3.9 | 50.8% |
| Lv1_NearForest | 밤 | 2/RustedSword | 3.6→3.6 | 70.3→70.3 | 31.2→31.2 | 142.9 | 5.0 | 40.4% |
| Lv1_NormalForest | 낮 | 4/FangDagger | 5.6→5.9 | 763.6→171.6 | 150.1→83.7 | 361.3 | 5.5 | 29.4% |
| Lv1_NormalForest | 밤 | 4/FangDagger | 5.8→6.3 | 180.8→184.2 | 83.2→84.8 | 386.4 | 4.5 | 23.7% |
| Lv1_DeepForest | 낮 | 7/BearClaw | 7.0→6.9 | 1203.3→347.4 | 241.9→148.2 | 509.1 | 7.5 | 22.7% |
| Lv1_DeepForest | 밤 | 7/BearClaw | 12.0→12.0 | 518.8→510.7 | 232.2→229.7 | 870.0 | 4.1 | 8.1% |
| Lv1_Swamp | 낮 | 10/CrocJawAxe | 3.3→3.2 | 463.1→283.4 | 110.4→91.6 | 237.9 | 5.3 | 26.2% |
| Lv1_Swamp | 밤 | 10/CrocJawAxe | 3.1→3.2 | 319.3→336.9 | 96.2→98.0 | 242.1 | 5.3 | 24.3% |
| Lv1_BanditFortress | 낮 | 12/CutthroatDagger | 3.2→3.2 | 400.1→420.9 | 109.7→109.7 | 266.8 | 5.4 | 22.2% |
| Lv1_BanditFortress | 밤 | 12/CutthroatDagger | 3.3→3.2 | 451.9→458.8 | 117.3→113.6 | 258.6 | 5.6 | 22.0% |
| Lv1_TowerOutskirts | 낮 | 14/PebbleGauntlet | 1.2→1.2 | 225.1→236.1 | 48.2→48.2 | 118.2 | 2.6 | 24.2% |
| Lv1_TowerOutskirts | 밤 | 14/PebbleGauntlet | 2.0→1.5 | 335.8→278.2 | 77.1→58.9 | 131.8 | 3.0 | 23.0% |

물약은 약 15~45개/시간이다. 깊은 숲 낮 등은 §3.3의 15~30개 목표를 넘는 가정이며, 초반 직접 골드만으로는 물약비 비중이 높다(판매 잠재액을 합치면 낮아짐). 실제 피격 빈도·자연 회복을 계측하기 전에는 몸박·상점 가격·골드 계수를 추가 변경하지 않는다.

## Lv1→15 시간

- 필요 EXP: **89,200** (`ExpToNext(1..14)` 합). 113,700은 Lv16 도달 기준이다.
- 목표 이전 퀘스트: 전부 수행 시 **26,425**(29.62%), 산적 분기 제외 **23,014**(25.80%). Lv15 요구 Q604의 2,000 EXP는 제외한다.
- 퀘스트는 최소 레벨·선행 충족 순서로 보상을 적용하고 초과 EXP를 이월한다. 수행 지연은 별도 예산으로 더한다. 실제 NPC 이동·수집 확률·선행 완료 시점을 재생한 시뮬레이션은 아니다.
- 비사냥 예산: NPC 왕복 24분 + 채집/요리/낚시/발굴 24분. 산적 분기는 왕복 8분 추가. 필수 보스는 대표 무기의 단일 대상 기대 DPS로 별도 계산(고대 엔트·거대 슬라임, 분기 시 산적 대장). 퀘스트 일반몹 처치는 사냥 시간에 포함되어 중복 가산하지 않는다.
- 경로: 샛길 Lv1~2 → 일반 숲 Lv3~5 → 깊은 숲 Lv6~8 → 늪 Lv9~12 → 탑 근교 Lv13~14. 선택 경로는 Lv11~12에 산적 성채를 이용한다. 일반 숲길(NearForest)은 샛길 대체 사냥터다.

| 경로 | 사냥 전→후(분) | 퀘스트·이동·보스(분) | 합계 전→후 | 낮만(후) | 밤만(후) |
|---|---:|---:|---|---|---|
| 메인·산적 제외 | 187.89→218.12 | 57.26 | 245.16→**275.39분(4시간 35분)** | 295.43분(4시간 55분) | 258.62분(4시간 19분) |
| 산적 포함 | 183.24→200.08 | 72.11 | 255.35→**272.19분(4시간 32분)** | 289.11분(4시간 49분) | 258.00분(4시간 18분) |

합계의 기본값은 낮/밤 EXP 속도를 50:50으로 섞은 비교 시나리오다. 현재 테스트용 낮 고정에서는 **낮만** 열을 본다. 시간대 교대 없이 밤만 사냥하는 열도 비교 시나리오이며, 실제 보스·퀘스트 때문에 낮 이동이 필요하다. 세 시나리오 중앙값은 목표 3~5시간 범위지만, 실제 동선·플레이 숙련·스킬/광역·장비 확보에 따른 분산은 미검증이다.

## 실제 평타 표본과 피해량 보정

낮에 자동 접근·평타 API를 실행했다. **매 프레임 HP를 채우는 스트레스 프로브**이며 실제 물약·쿨다운·사망·회피·입력 잠금을 사람 플레이처럼 재현하지 않는다. 피격원인도 접촉/투사체/보스로 분류하지 않았다. 처치·EXP·드롭·HP 감소는 실제 게임 경로다. 30초 표본은 정예·남은 HP·드롭 난수의 영향이 커서 처치/골드를 그대로 10분 확정 수치로 보지 않는다.

| 맵 | 표본(초) | 몬스터 처치 | EXP/분 | 직접 골드 발생/분 | HP 손실/분 | 물약 등가/10분 | 관측 HP-DPS/계산 DPS |
|---|---:|---:|---:|---:|---:|---:|---:|
| Lv1_NearForest_SideRoad | 30 | 3 | 72.0 | 24.0 | 26.0 | 2.2 | 0.920 |
| Lv1_NearForest | 30 | 3 | 72.0 | 36.0 | 0.0 | 0.0 | 0.840 |
| Lv1_NormalForest | 30 | 4 | 184.0 | 110.0 | 1216.0 | 55.3 | 0.984 |
| Lv1_DeepForest | 30 | 8 | 688.0 | 332.0 | 1602.0 | 72.8 | 2.154 |
| Lv1_Swamp | 30 | 2 | 268.0 | 156.0 | 832.0 | 37.8 | 2.340 |
| Lv1_BanditFortress | 30 | 1 | 242.0 | 114.0 | 634.0 | 28.8 | 0.989 |
| Lv1_TowerOutskirts | 90 | 3 | 292.0 | 96.7 | 2845.3 | 129.3 | 0.967 |

물약 등가는 해당 HP를 물약으로만 메우는 계산값으로, 실제 아이템 사용 실측이 아니다. 무회피 표본의 일반 숲~탑 근교는 약 **173~776개/시간 등가**로, 15~30개 목표를 크게 넘는다. 따라서 앞 표의 15~45개/시간 가정을 검증된 소비량으로 취급하지 않는다. 정상 회피·장비 방어·피격원인별 분석을 하기 전에는 몸박/골드/물약 가격을 임의로 낮추지 않았다(Q-448).

실제 HP 피해/초 ÷ 대표 캠프 단일 대상 계산 DPS를 보정 계수로 삼아 600초 모델의 전투 시간만 보정했다. 접근·줍기 3초는 계속 더하므로 이동 비용을 제거하는 보정은 아니다. **낮 표본만 보정하고 밤은 미보정**이다. 데이터가 없는/0피해 표본은 계수 1로 처리한다. 이 계산도 EditMode 테스트로 고정했다.

| 경로 | 단일 대상 모델(혼합) | 낮 실측 피해량 보정(혼합) | 보정한 낮만 |
|---|---:|---:|---:|
| 산적 제외 | 275.39분(4시간 35분) | **248.08분(4시간 8분)** | **247.32분(4시간 7분)** |
| 산적 포함 | 272.19분(4시간 32분) | **263.83분(4시간 24분)** | **273.76분(4시간 34분)** |

퀘스트·이동·보스 예산은 동일하다. 이 비교 범위와 보정 없는 낮 경로(최대 4시간 55분)는 3~5시간 목표 안에 있다. 장비 확보·동선·회피를 포함한 전체 클리어 실측값은 아니다.

Lv15 도달 뒤 층계 문지기까지 마무리하면 별도 시간이 든다. Lv15/+0/부하40 단일 대상 계산 TTK **18.87분** + 최종 왕복·보고 가정 **2분**을 더하면, 실측 피해량 보정 기준 층계 완료는 산적 제외 **268.95분(혼합)/268.19분(낮만)**, 포함 **284.70분/294.63분**으로 약 **4시간 28~55분**이다. 보정 없는 단일 대상·낮 경로는 최대 **316.30분(5시간 16분)**이다. 문지기 페이즈·회피·부하 저항 장비를 실제 재생한 TTK는 미검증이다. Q604의 EXP는 여전히 Lv15 도달 전에 계산하지 않는다.

§3.2의 일반몹 1~3초 TTK 목표는 +0 대표 무기·단일 대상 평타만으로 미달한다. 대표 캠프 계산 TTK는 토끼 7.7~8.7초, 늑대 5.7초, 곰 5.6초, 개구리 11.9초, 산적 13.2초, 작은 골렘 24.5초다. 스킬·세트·강화 포함 TTK는 미검증이며 이번에는 HP·전투 모션을 변경하지 않았다.

## 스포너별 조정 전→후 (보스·채집물 포함)

`수치 키`가 프리팹과 다르면 같은 아트·AI를 지역 변종으로 재사용한다. 2층계 원본 데이터는 유지한다. 밤 CreepingTerror는 누락됐던 레벨/EXP/접촉 값을 추가하고 기존 HP50/방어1을 보존했다. 위치별 슬롯 가중치는 `HuntingSpawnsFloor1.csv`의 `pointWeights`에 있다.

| 맵 | 낮/밤 | 스포너 | 기존 프리팹 → 수치 키 | 수 전→후 | 리젠 전→후(초) | 레벨 전→후 | EXP 전→후 |
|---|---|---|---|---:|---:|---:|---:|
| Lv1_NearForest_SideRoad | 밤 | BossSpawoner | PassiveTerror_BD | 1→1 | 120→120 | 3→3 | 320→320 |
| Lv1_NearForest_SideRoad | 밤 | CreepingTerror | CreepingTerror_BD | 5→27 | 10→10 | 0→3 | 0→19 |
| Lv1_NearForest_SideRoad | 밤 | CreepingTerror2 | CreepingTerror_BD | 14→27 | 10→10 | 0→3 | 0→19 |
| Lv1_NearForest_SideRoad | 낮 | Flowermole1 | FlowerMole_BD | 6→28 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest_SideRoad | 낮 | Flowermole2 | FlowerMole_BD | 7→27 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest_SideRoad | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NearForest_SideRoad | 낮 | Mole | LeftMole_BD | 5→15 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest_SideRoad | 낮 | Mole2 | LeftMole_BD | 6→14 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest_SideRoad | 낮 | Mole3 | LeftMole_BD | 5→16 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest_SideRoad | 낮 | Rabbit | Rabbit_BD | 8→18 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest_SideRoad | 낮 | Rabbit2 | Rabbit_BD | 5→8 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest_SideRoad | 낮 | Rabbit2_2 | StoneRabbit_BD | 5→15 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 낮 | Rabbit3 | StoneRabbit_BD | 7→16 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 밤 | RatCheif | RatChief_BD | 5→8 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 밤 | RatCheif2 | RatChief_BD | 7→16 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 밤 | RatThief | RatThief_BD | 5→18 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest_SideRoad | 밤 | RatThief2 | RatThief_BD | 5→8 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest_SideRoad | 밤 | ScareCrow | Scarecrow_BD | 5→15 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 밤 | ScareCrow2 | Scarecrow_BD | 6→14 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 밤 | ScareCrow3 | Scarecrow_BD | 5→17 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest_SideRoad | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NearForest_SideRoad | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |
| Lv1_NearForest | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NearForest | 낮 | Mole | LeftMole_BD | 5→15 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest | 낮 | Rabbit | Rabbit_BD | 8→18 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest | 낮 | Rabbit2 | Rabbit_BD | 5→8 | 10→10 | 1→1 | 12→12 |
| Lv1_NearForest | 낮 | Rabbit2_2 | StoneRabbit_BD | 5→15 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest | 낮 | Rabbit3 | StoneRabbit_BD | 7→16 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest | 밤 | RatCheif | RatChief_BD | 5→8 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest | 밤 | RatCheif2 | RatChief_BD | 7→16 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest | 밤 | RatThief | RatThief_BD | 5→18 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest | 밤 | RatThief2 | RatThief_BD | 5→8 | 10→10 | 2→2 | 15→15 |
| Lv1_NearForest | 밤 | ScareCrow | Scarecrow_BD | 5→15 | 10→10 | 3→3 | 19→19 |
| Lv1_NearForest | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NearForest | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |
| Lv1_NormalForest | 밤 | CreepingTerror | ForestSnail_BD | 5→27 | 10→10 | 6→6 | 35→35 |
| Lv1_NormalForest | 밤 | CreepingTerror2 | ForestSnail_BD | 14→27 | 10→10 | 6→6 | 35→35 |
| Lv1_NormalForest | 낮 | Flowermole1 | SporeShroom_BD → ForestSpore_BD | 6→28 | 10→10 | 17→5 | 283→28 |
| Lv1_NormalForest | 낮 | Flowermole2 | SporeShroom_BD → ForestSpore_BD | 7→27 | 10→10 | 17→5 | 283→28 |
| Lv1_NormalForest | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NormalForest | 낮 | Mole | WildBoar_BD | 2→15 | 10→10 | 5→5 | 28→28 |
| Lv1_NormalForest | 낮 | Mole2 | WildBoar_BD | 3→14 | 10→10 | 5→5 | 28→28 |
| Lv1_NormalForest | 낮 | Mole3 | WildBoar_BD | 2→16 | 10→10 | 5→5 | 28→28 |
| Lv1_NormalForest | 낮 | Rabbit | ForestWolf_BD | 4→18 | 10→10 | 4→4 | 23→23 |
| Lv1_NormalForest | 낮 | Rabbit2 | ForestWolf_BD | 2→8 | 10→10 | 4→4 | 23→23 |
| Lv1_NormalForest | 낮 | Rabbit2_2 | ForestWolf_BD | 2→15 | 10→10 | 4→4 | 23→23 |
| Lv1_NormalForest | 낮 | Rabbit3 | ForestWolf_BD | 3→16 | 10→10 | 4→4 | 23→23 |
| Lv1_NormalForest | 밤 | RatCheif | ForestSnail_BD | 5→8 | 10→10 | 6→6 | 35→35 |
| Lv1_NormalForest | 밤 | RatCheif2 | ForestSnail_BD | 7→16 | 10→10 | 6→6 | 35→35 |
| Lv1_NormalForest | 밤 | RatThief | Wisp_BD | 5→18 | 10→10 | 3→3 | 19→19 |
| Lv1_NormalForest | 밤 | RatThief2 | Wisp_BD | 5→8 | 10→10 | 3→3 | 19→19 |
| Lv1_NormalForest | 밤 | ScareCrow | Wisp_BD | 5→15 | 10→10 | 3→3 | 19→19 |
| Lv1_NormalForest | 밤 | ScareCrow2 | Wisp_BD | 6→14 | 10→10 | 3→3 | 19→19 |
| Lv1_NormalForest | 밤 | ScareCrow3 | Wisp_BD | 5→17 | 10→10 | 3→3 | 19→19 |
| Lv1_NormalForest | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_NormalForest | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |
| Lv1_DeepForest | 낮 | Bear1 | ForestBear_BD | 4→18 | 10→10 | 7→7 | 43→43 |
| Lv1_DeepForest | 낮 | Bear2 | ForestBear_BD | 2→15 | 10→10 | 7→7 | 43→43 |
| Lv1_DeepForest | 낮 | BossSpawoner | AncientEnt_BD | 1→1 | 120→120 | 9→9 | 950→950 |
| Lv1_DeepForest | 낮 | Ent1 | Ent_BD | 3→16 | 10→10 | 8→8 | 52→52 |
| Lv1_DeepForest | 밤 | Fairy1 | Fairy_BD | 5→8 | 10→10 | 7→7 | 43→43 |
| Lv1_DeepForest | 밤 | Fairy2 | Fairy_BD | 7→16 | 10→10 | 7→7 | 43→43 |
| Lv1_DeepForest | 밤 | Ghost1 | Ghost_BD | 5→18 | 10→10 | 6→6 | 35→35 |
| Lv1_DeepForest | 밤 | Ghost2 | Ghost_BD | 5→8 | 10→10 | 6→6 | 35→35 |
| Lv1_DeepForest | 밤 | Ghost3 | Ghost_BD | 5→15 | 10→10 | 6→6 | 35→35 |
| Lv1_DeepForest | 낮 | Goblin1 | CliffGoblin_BD → DeepGoblin_BD | 5→8 | 10→10 | 16→7 | 240→43 |
| Lv1_DeepForest | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_DeepForest | 낮 | Shroom1 | SporeShroom_BD → DeepSpore_BD | 5→15 | 10→10 | 17→7 | 283→43 |
| Lv1_DeepForest | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_DeepForest | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |
| Lv1_Swamp | 밤 | BossSpawoner | GiantSlime_BD | 1→1 | 180→180 | 12→12 | 1700→1700 |
| Lv1_Swamp | 낮 | Crab1 | MarshCrab_BD | 3→7 | 10→10 | 10→10 | 77→81 |
| Lv1_Swamp | 낮 | Croc1 | BogCroc_BD | 3→24 | 10→10 | 11→11 | 94→99 |
| Lv1_Swamp | 낮 | Frog1 | SwampFrog_BD | 6→36 | 10→10 | 9→9 | 64→67 |
| Lv1_Swamp | 밤 | Mosquito1 | SwampMosquito_BD | 3→18 | 12→10 | 9→9 | 64→67 |
| Lv1_Swamp | 밤 | Mosquito2 | SwampMosquito_BD | 3→7 | 10→10 | 9→9 | 64→67 |
| Lv1_Swamp | 낮 | Shroom1 | SporeShroom_BD → SwampSpore_BD | 4→24 | 10→10 | 17→9 | 283→67 |
| Lv1_Swamp | 밤 | Slime1 | BogSlime_BD | 4→24 | 12→10 | 12→12 | 115→121 |
| Lv1_Swamp | 밤 | Slime2 | BogSlime_BD | 4→24 | 12→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 낮 | Archer1 | BanditArcher_BD | 4→8 | 10→10 | 11→11 | 94→99 |
| Lv1_BanditFortress | 낮 | Archer2 | BanditArcher_BD | 4→16 | 10→10 | 11→11 | 94→99 |
| Lv1_BanditFortress | 낮 | Archer3 | BanditArcher_BD | 5→28 | 10→10 | 11→11 | 94→99 |
| Lv1_BanditFortress | 밤 | Archer4 | BanditArcher_BD | 4→8 | 10→10 | 11→11 | 94→99 |
| Lv1_BanditFortress | 밤 | Archer5 | BanditArcher_BD | 4→16 | 10→10 | 11→11 | 94→99 |
| Lv1_BanditFortress | 낮 | BossSpawoner | BanditChief_BD | 1→1 | 120→120 | 14→14 | 2400→2400 |
| Lv1_BanditFortress | 낮 | Cutthroat1 | BanditCutthroat_BD | 5→18 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 낮 | Cutthroat2 | BanditCutthroat_BD | 4→15 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 낮 | Cutthroat3 | BanditCutthroat_BD | 5→16 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 낮 | Cutthroat4 | BanditCutthroat_BD | 5→27 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 밤 | Cutthroat5 | BanditCutthroat_BD | 4→8 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 밤 | Cutthroat6 | BanditCutthroat_BD | 4→27 | 10→10 | 12→12 | 115→121 |
| Lv1_BanditFortress | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_BanditFortress | 낮 | Shield1 | BanditShieldman_BD | 3→15 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 낮 | Shield2 | BanditShieldman_BD | 3→14 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 밤 | Shield3 | BanditShieldman_BD | 3→14 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_BanditFortress | 밤 | Torch1 | BanditTorchbearer_BD | 4→18 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 밤 | Torch2 | BanditTorchbearer_BD | 4→15 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 밤 | Torch3 | BanditTorchbearer_BD | 4→17 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 밤 | Torch4 | BanditTorchbearer_BD | 5→27 | 10→10 | 13→13 | 139→146 |
| Lv1_BanditFortress | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |
| Lv1_TowerOutskirts | 낮 | BossSpawoner | Gatekeeper_BD | 1→1 | 120→120 | 15→15 | 3800→3800 |
| Lv1_TowerOutskirts | 낮 | GolemLarge1 | GolemLarge_BD | 3→16 | 10→10 | 15→15 | 203→213 |
| Lv1_TowerOutskirts | 밤 | GolemLarge2 | GolemLarge_BD | 2→9 | 10→10 | 15→15 | 203→213 |
| Lv1_TowerOutskirts | 낮 | GolemMedium1 | GolemMedium_BD | 3→8 | 10→10 | 14→14 | 169→177 |
| Lv1_TowerOutskirts | 낮 | GolemMedium2 | GolemMedium_BD | 3→15 | 10→10 | 14→14 | 169→177 |
| Lv1_TowerOutskirts | 밤 | GolemMedium3 | GolemMedium_BD | 3→15 | 10→10 | 14→14 | 169→177 |
| Lv1_TowerOutskirts | 낮 | GolemSmall1 | GolemSmall_BD | 5→18 | 10→10 | 13→13 | 139→146 |
| Lv1_TowerOutskirts | 낮 | GolemSmall2 | GolemSmall_BD | 4→15 | 10→10 | 13→13 | 139→146 |
| Lv1_TowerOutskirts | 밤 | GolemSmall3 | GolemSmall_BD | 4→18 | 10→10 | 13→13 | 139→146 |
| Lv1_TowerOutskirts | 낮 | GreenHurb | GreenHurb | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_TowerOutskirts | 밤 | RuinSpirit1 | RuinSpirit_BD | 5→7 | 10→10 | 13→13 | 139→146 |
| Lv1_TowerOutskirts | 밤 | RuinSpirit2 | RuinSpirit_BD | 6→16 | 10→10 | 13→13 | 139→146 |
| Lv1_TowerOutskirts | 낮 | Stone | Rock | 4→4 | 30→30 | 0→0 | 0→0 |
| Lv1_TowerOutskirts | 낮 | Tree | tree | 4→4 | 3→3 | 0→0 | 0→0 |

## 검증 상태

- EditMode 전체 **366/366 통과**, 실패/스킵 0.
- 화면 기대 밀도: **613개 발판 중심 × 해당 시간대**, 범위 이탈·가중치 길이 오류 **0**.
- 플레이 회귀: **14/14 조합**, 일반 스포너 **92개**, 실제 처치 **92**, EXP 기대/실제 **7,142/7,142**, 골드 더미 발생 합 **2,093G**, 실패 **0**. 리젠 콜백을 직접 호출한 검사로 10초 실시간 대기 내구 테스트는 아니다.
- 실제 평타: 최종 7맵 표본 게임 시간 270초·몬스터 24처치·HP 피해량 7,854. 일반 숲·깊은 숲은 채집물 파괴가 처치 수에 섞이지 않도록 IsMonster 필터를 넣어 재계측했다. 원본/재계측 로그는 Logs/playtest/hunt_melee_auto.csv, hunt_melee_recheck.csv, 최종 입력은 Floor1HuntingMeleeSample.csv.
- 낮 화면: `Logs/playtest/hunt_daytime.png` 육안 확인. 화면 안 토끼 여러 마리·다층 발판·레벨 표기 확인.
- 활성 객체 29개(몹·채집물 포함)인 사냥터에서 직접 플레이 종료: DOTween 정리 오류 **1→0**. EnemySpawner.OnApplicationQuit에서 파괴 전에 풀을 반환한다. 최종 컴파일·플레이·정지 뒤 콘솔 에러 **0**.
- 플레이 정지 완료. SaveFile.es3 복원 **1/1**, MD5 **F1CBD3EDF7E845CACE5442F6641C5B2F**(전후 동일), 추가 es3 파일 없음. 검증 JSON: Logs/playtest/hunt_save_restore.json.
- 커밋·push 없음. 리뷰 체크리스트: 정수/퍼센트 연산, 풀 반환 단일 경로, 최초 활성화 순서, 정예 초기화, 데이터 키·세이브 호환, 프레임별 할당을 확인했다. 씬·프리팹 수동 변경 없음.
- 미검증(원 작업 종료 시점, 후속 리뷰는 아래 참조): 600초 실시간 연속 사냥, Lv1→15 전체 NPC/수집/장비 동선, 인간·게임패드 조작, 스킬/광역 최적 사냥, 장시간 GC/성능.

## 바뀐 파일

- 신규 데이터: `Assets/Resources/HuntingSpawnsFloor1.csv`(+meta).
- 신규 계산/도구: `Assets/Script/Enemy/HuntingSpawnTable.cs`, `HuntingRules.cs`, `Assets/Editor/Dev/HuntingAudit.cs`, `HuntingDensityAudit.cs`(+각 meta).
- 신규 테스트: `Assets/Editor/Tests/HuntingRulesTests.cs`(+meta).
- 수정: `Assets/Resources/MonsterSheet.csv`, `Assets/Script/Enemy/EnemySpawner.cs`, `EnemyHealth.cs`, `MonsterTable.cs`, `Assets/Script/Player/Level/LevelSystem.cs`.
- 문서 신규: `Docs/Balance/Floor1HuntingBaseline.csv`, `Floor1HuntingMeleeSample.csv`, `Floor1HuntingAudit.md`.
- 문서 수정: `.claude-rules/Game/ClassicMapleDesign.md`, `EnemySpec.md`, `Docs/OpenQuestions.md`.
- 커밋에서 제외: 이번에 만든 `Tools/probes/hunt_*.cs` 전부, `Logs/hunt_*`, `Logs/playtest/hunt_*`, `Logs/save-backup-hunt-*`, `Assets/Screenshots/*`. 작업 전부터 있던 변경 파일은 이번 목록에 포함하지 않는다.

## 후속 독립 리뷰

도감 처치 키 회귀와 활성 정원 상한 표기를 수정했다. 일반 숲 낮 600게임초(배율 1) 연속 검사 86처치·EXP 2,254·직접 골드 1,012G·HP 손실 17,206. 자동 접근·HP 상시 보충 조건으로 정상 회피/물약 사용 검증은 아니다. 일반 숲 트리거 겹침 위치에서 자연 리젠으로 58슬롯 실제 점유를 확인했다. 상세 발견·검증·커밋 목록: `Docs/Balance/Floor1HuntingReview.md`.
