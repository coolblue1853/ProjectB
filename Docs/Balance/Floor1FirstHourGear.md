# 1층계 첫 1시간 장비 곡선 (Lv1~12)

작성: 2026-10-31. 데이터만 바꾼 변경이다(코드·씬·프리팹·아이콘 변경 없음). 기본 평타·칼 3타·스미어 v7·착용 레벨 하한(Q-LG01)은 건드리지 않았다. 기준: `Docs/Research/LevelingVsGear.md` §1·§5·§6.

## 1. 장비 표 (무기 6종, 장비 스킬이 서로 다름)

| 순서 | 무기 | 입수 | 시각(분, 추정) | 피해(평균) | 장비 스킬(기존) | 세트 |
|---|---|---|---:|---|---|---|
| 0 | RustedSword | 시작 | 0 | 5~10 (7.5) | Smash Down, Slash Attck | - |
| 1 | LeafAdae | Q102 보상(Lv1) | 8~10 | 8~12 (10) | Scattering Leaves(투사), Haste(버프), Falling Leaf Step | Nature's Reward(무기는 비세트) |
| 2 | FangDagger | Q202 보상(Lv4) | 28~32 | 10~15 (12.5), 치명 10/20 | Fang Rush, Pack Step, Howl of the Pack(버프) | Pack Hunter |
| 3 | BearClaw | Q302 보상(Lv6) | 52~58 | 12~18 (15), 치명 8/25 | Mauling Rush, Rending Upswipe, Bear Roar | Ancient Grove |
| 4 | RootStaff | Q304 보상(Lv8) (+ 고대 엔트 확정 보상 BearClaw 또는 RootStaff) | 78~85 | 12~18 (15) | Root Step, Ancient Roots(함정), Sap Mend | Ancient Grove |
| 5 | CrocJawAxe | Q401 보상(Lv9) | 98~105 | 14~21 (17.5), 치명 6/20 | Jaw Shake, Bog Eruption, Tail Whirl, Death Roll | Bog Stalker |

- 간격: 9 → 30 → 55 → 80 → 100분. 첫 대체 무기 8~10분(목표 5~10), 이후 20~25분 간격(목표 20~35). 모든 보상이 고정 지급이라 드롭 불운 꼬리가 없다.
- 피해값은 기존 구역 단계 공식(Q-415, `apply_zone_tier_weapons.cs`) 그대로다. 예산 W(Lv1 10 → Lv3 12 → Lv6 15 → Lv9 18)와 맞는다(시작 무기 평균 7.5 → 17.5).
- 일반 드롭 무기는 그대로 둔다(계보 몹 2%, 샛길 0.1~0.2%). 샛길 장비 드롭은 사실상 없는 수준이므로 첫 1시간 공급원이 아니다.

## 2. 방어구·세트 진척 (3세트)

세트 3개는 모두 기존 ItemSheet·프리팹·아이콘을 쓴다. 부위 5개 + 무기 1개 = 6부위. 퀘스트 보상 1개 + 방어구 대장장이 신규 레시피(`ArmorCraftRecipes.csv`, 15행)로 채운다.

| 세트 | 구역 | 퀘스트 보상 | 제작 재료(주 드롭률 35%) | 예상 2/4/6 시각(분) |
|---|---|---|---|---|
| Pack Hunter(Wolf) | 일반 숲 Lv3~6 | Q201 WolfCap, Q202 FangDagger, Q204 WolfShoes | Wolf Fang·Boar Tusk (Cap 8 / Hand 8 / Leg 8+3 / Shoes 6+5 / Armor 10+8) | 30 / 45 / 60 |
| Ancient Grove | 깊은 숲 Lv6~9 | Q301 GroveCap, Q302 BearClaw, Q304 RootStaff | Heartwood Chip·Bear Pelt (Cap 8 / Hand 8 / Leg 6+4 / Shoes 5+6 / Armor 10+8) | 58 / 85 / 100 |
| Bog Stalker | 늪 Lv9~12 | Q401 CrocJawAxe, Q402 BogCap | Frog Skin·Croc Tooth·Crab Pincer (Cap 8 / Hand 8 / Leg 8 / Shoes 5+5 / Armor 8+6+4) | 102 / 125 / 150 |

- 재료 1개 ≈ 3킬(35%). 일반 숲 5.6~5.9킬/분, 깊은 숲 7킬/분, 늪 3.2킬/분(`Floor1HuntingAudit.md`) 기준 방어구 1부위 ≈ 4~9분 → 8~15분 간격 진척. 광석(T0_Ore)은 요구하지 않는다.
- 부위 의미 있는 보상 간격: 18(WolfCap) → 30(FangDagger, 2세트) → 38~45(제작) → 42(WolfShoes) → 52(BearClaw) → 58(GroveCap) → 80(RootStaff) → 100(CrocJawAxe) 등 8~25분.

## 3. 세트 수치 조정 (SetItem.csv, 누적)

| 세트 | 변경 전 | 변경 후 |
|---|---|---|
| Wolf 2 | 이속 8 / 치명 5 | 이속 8 / 치명 8 / 증뎀 3 |
| Wolf 4 | 치피 15 / 쿨감 Howl 40, Boar Charge 30 | 치피 15 / 증뎀 7 / 쿨감 동일 |
| Wolf 6 | 공속 10 / 증뎀 10 / HP 50 | 공속 8 / HP 50 |
| Grove 2 | 방어 15 / HP 80 | 방어 8 / HP 40 |
| Grove 6 | 공속 10 / 치피 20 / HP 120 | 공속 6 / 치피 20 / HP 120 |
| Bog 4 | 쿨감 3종 30 | 증뎀 8 / 쿨감 3종 30 |
| Bog 6 | 공속 10 / 출혈 15 / 증뎀 8 | 공속 6 / 출혈 15 / 증뎀 2 |

이유: 4세트가 쿨감뿐이라 DPS가 +6% 안팎이고 6세트는 +30~40%라 4+2와 격차가 컸다. 4세트를 +17~20%, 6세트를 +26~29%로 모았다. 2세트의 생존형(Grove·Bog)은 DPS 0이고 생존 +8~12%대.

### 상대 DPS 추정 (프로브 `gear1_play.cs` 실측 스탯으로 계산)

기댓값 = (1 + 치명률 × (0.20 + 무기치피 + 세트치피)) × (1 + 증뎀) × (1 + 공속). 쿨감의 DPS 기여는 스킬 피해 점유율 가정(Wolf Howl 버프 +2%, Grove Mauling Rush 15% 점유 → +6%, Bog Jaw Shake 25% 점유 → +10.7%)으로 환산했다. 세트 끈 상태(무기만 장착)=1.00.

| 세트 | 실측 스탯(2 / 4 / 6) | 2세트 | 4세트 | 6세트 | 6 ÷ (4+2 최대) |
|---|---|---:|---:|---:|---:|
| Wolf(FangDagger 치명 10/20) | 치명 8 · 증뎀 3 / +치피 15 · 증뎀 10 / +공속 8 | +6.2% | +18.5% | +28% | 1.08 (4+2 = Wolf4 + 생존2) |
| Grove(BearClaw 치명 8/25) | 방어 8 · HP 40 / +증뎀 10 / +치피 20 · 공속 6 | 0% (생존) | +16.6% | +25.5% | 1.08 (Grove4 + 생존2) |
| Bog(CrocJawAxe 치명 6/20) | 이속 6 · 방어 8 / +증뎀 8 / +공속 6 · 증뎀 10 | 0% (생존) | +19.6% | +29% | 1.08 |

목표(2세트 +4~6 또는 생존, 4세트 +15~22, 6세트 +25~35, 6세트가 4+2보다 10% 이하)를 모두 만족한다. 단, 쿨감·점유율은 가정이며 실제 스킬 로테이션 실측은 미검증이다.

### 레벨 : 장비 공격 증가분

Lv1(RustedSword 7.5, 레벨 공격 0) → Lv12(CrocJawAxe 17.5 + 레벨 공격 5, Bog 4세트 ×1.196 또는 6세트 ×1.29). 두 적용 순서 평균으로 레벨 몫을 구하면 4세트 기준 **28 : 72**, 6세트 기준 **27 : 73**이다(목표 25 : 75, 허용 20~30).

## 4. 검증 (2026-10-31)

- 컴파일 에러 0, EditMode 402/402 통과(재실행 포함). 새 CSV를 `SelfCraftPagesTests`가 검증하도록 갱신(방어구 CSV 16행, 모든 결과·재료 이름이 ItemSheet에 존재).
- 프로브 `Tools/probes/gear1_play.cs`(플레이 모드, `NpcQuestRewards.Apply` = 실제 지급 경로): Q102 → LeafAdae 가방 지급, Q201 WolfCap, Q202 FangDagger, Q204 WolfShoes, Q301 GroveCap, Q302 BearClaw, Q304 RootStaff, Q401 CrocJawAxe, Q402 BogCap 전부 지급. LeafAdae 장착 → 스킬 Scattering Leaves, Haste, Falling Leaf Step.
- 세트 스탯 실측(기본 장비 기준 가산): Wolf 2=치명 +8·증뎀 +3·이속 +8 / 4=치피 +15·증뎀 +10 / 6=공속 +8. Grove 4=증뎀 +10 / 6=치피 +20·공속 +6. Bog 4=증뎀 +8 / 6=증뎀 +10·공속 +6·이속 +6.
- 세이브는 백업 후 md5 복원 확인(SaveFile f1cbd3ed…, SaveSlot3 7c5d94a1…).

## 5. 미검증·한계

- 8~10분(LeafAdae)·이후 시각은 `Floor1HuntingAudit.md`의 킬/분과 EXP 곡선에 의한 추정이다. 사람이 60분 플레이한 실측이 아니다.
- 쿨감의 DPS 환산은 점유율 가정이다. 실제 스킬 사용률은 미측정.
- 방어구 HP/방어 자체(Wolf 5부위 합 HP 540 등)는 `LevelingVsGear.md` §6.1의 HP 장비 예산(Lv6 80)보다 훨씬 크다. 이번에는 건드리지 않았다(Q-GF03).
- 가방 15칸: Q201·Q401·Q402 보상은 가방이 가득 차면 못 받는다(`CanReceive` 거부). 체크리스트에 빈칸 확보를 적었다.
- 새 스킬·새 이펙트·새 아이콘 없음. 이펙트 필요 목록은 `Docs/Playtest/TempAssets.md`.
