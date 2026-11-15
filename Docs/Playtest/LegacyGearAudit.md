# 옛 장비 정리 대조표 (LegacyGearAudit, 11-15)

근거: `Docs/Handoff/playtest_1115.md` 항목 4(사용자 "여전히 장착되는 예전 아이템이 남아 있다"), `Game/EquipSlotExpansion.md`, `Docs/Handoff/equip_redistribution.md`, `Docs/Handoff/outfit_one_piece_decision.md`, `Game/Floor1Completion.md` §5, `Game/LineageGear.md`.

## 1. 결론

- 사용자가 본 "옛 장비"는 두 종류였다.
  1. **옛 부위 이름이 남은 현행 장신구 48개.** v2 개편(11-11)에서 투구→귀걸이, 하의→목걸이, 장갑→반지로 `equipArea` 는 바뀌었지만 표시 이름은 "쥐 두건(RatCap)", "다리 갑옷(LegArmor)", "밤의 등불 장갑" 그대로였다. 장착하면 반지·목걸이 칸에 들어가 옛 장비처럼 보였다. → **표시 이름만 이행**(키·세트·스탯·계보 그대로, 아이콘은 나중).
  2. **신발 16개.** v2 결정은 "신발 아이템 삭제, 효과는 다른 슬롯에 분배"였는데 임시로 보조장비(Charm)로 옮겨져 계속 떨어지고 팔리고 제작됐다. 보조장비 칸은 1층계 중후반에 열려 대부분 줍자마자 판매. → **획득 경로에서 제거**, `retired=1`.
- 판매 기록의 PoisonSword·Ring Finger that Tears Wounds·Necklace of Spreading Corruption·Necklace of the Sly Plunderer 는 쥐 두목(RatChief) 계보 장비이고 현행 체계(무기·반지·목걸이, `equip_redistribution.md` §2 "단독 장신구 유지", `Floor1Completion.md` §5-1 한손검)라 **유지**. 판매가 많은 이유는 등급·수치 쪽(밸런스) 문제다 → 미해결 1.
- ItemSheet 파싱 이상(헤더·Junk 행 섞임): C# `ItemDatabase` 와 `Tools/playrec_balance.py`(단독 CR 제거) 모두 이상 행 0. Junk 14행은 `category=Junk` 정상 값. 행 내부 단독 CR(열 17 뒤)은 기존 그대로 둔다.

## 2. 조치

| 항목 | 내용 |
|---|---|
| 폐기 표시 | `ItemSheet.csv` 맨 뒤 `retired` 열(1=폐기). 행은 지우지 않는다(세이브 호환). `Floor2Preview/Floor2ItemSheet.csv` 도 머리글을 맞추고 2층계 신발 4종 1 |
| 장착 차단 | `ItemData.retired`·`ItemDef.Retired` → `EquipmentModel.TryGetSlot`·`ExtraEquip.TryHandleEquip`·`TryPlaceDirect` 거부. 판매·창고·버리기는 그대로 |
| 로드 처리 | 기본 칸(`SpawnSavedGear`)·추가 칸(`ExtraEquip.LoadCore`)에 낀 폐기 장비는 장착 해제 후 가방으로, 가방이 가득이면 판매값(price/2) 골드(`ExtraEquip.ReturnOrRefund`, Q-SLOT4 규칙). 빈 칸에 기본 무기 등이 들어가 세이브 칸이 덮이면 사라지므로 칸에 남기지 않는다. 구 칸 이행(`MigrateLegacyGear`)은 폐기 장비를 가방 대상으로 보고, 가득이면 기존처럼 구 칸에 남겨 다음 로드에 재시도(구 칸은 숨김 칸이라 덮이지 않음) |
| 드롭 | `SampleScene` DropManager 109곳·`Rabbit_BD` 프리팹 1곳에서 폐기 항목 삭제(에디터 API). 보스 세트 조각 CliffShoes(AbyssWizard)·ScareShoes(PassiveTerror) 삭제. 안전망: `DropManager.DropOne` 이 폐기 장비를 떨어뜨리지 않고 경고, 정예 확정 장비 후보(`EquipmentNames`)에서도 제외 |
| 상점 | `ShopStock.csv` 기본 장비 페이지의 Shoes 삭제 |
| 제작 | `ArmorCraftRecipes.csv` 신발 9행 삭제. `ZoneProgress.SetParts` 4부위(귀걸이·반지·목걸이·의상) |
| 보상 교체 | Q204 WolfShoes→WolfHandArmor(무리 사냥꾼 반지, 퀘스트 보상이 없던 부위) · M4 산적 부관 확정 OutlawShoes→OutlawHandArmor · Q309 GroveShoes→Enhance Stone×2 · Q609 StoneShoes→Enhance Stone×3(두 세트는 다른 부위가 모두 이미 퀘스트 보상) |
| 이름 이행 | 세트 귀걸이·목걸이·반지 48개 `itemNameT` 교체 + `LocItems` 한국어 + I2 동기화(추가 49). Q206·Q306 제목·설명의 "다리 갑옷·장갑"도 고침 |
| 재발 방지 | `Assets/Editor/Tests/LegacyGearIntegrityTests.cs`: 드롭표(씬·프리팹 YAML)·추가 드롭·미니보스·보스·상점·제작(방어구·자가)·퀘스트·도감·보물·습격·의뢰·무기 시연의 모든 아이템이 시트에 있고 `retired` 가 아닌지, 얻을 수 있는 장신구 표시 이름에 옛 부위 낱말이 없는지, 폐기 정의는 장착 불가인지 |

분류 개수: 현행 유지 91 · 이름 이행 48 · 제거(retired) 23(신발 16·더미 5·옛 칭호/오라 2) · 보상 교체 4 · 대응 필요 0.
RatCap: 현행 대응(쥐 세트 귀걸이)이 바로 그 행이라 교체 없이 이름만 "쥐 귀걸이"로 바꿨다. 도감 B08·퀘스트 Q106 보상 그대로.

## 3. 미해결

1. 쥐 두목 계보 장신구·PoisonSword 판매 빈도(획득 즉시 판매) — 수치·등급 문제. 밸런스 담당이 `PlayBalance` 다음 회차에서 본다.
2. 신발 효과 분배(설계 "효과는 다른 슬롯에 얹는다")는 아직 안 함. 지금은 신발 스탯이 게임에서 빠졌다(세트 효과에는 원래 포함되지 않음). 1층계 보조장비 칸 아이템(CharmPlain·HunterCharm) 외 추가가 필요한지 기획 판단.
3. 아이콘은 옛 모자·장갑·각반 그림 그대로(사용자 결정 "아이콘은 나중").
4. 2층계 미리보기(`Floor2ItemSheet`, 기본 꺼짐)의 Cap·LegArmor·HandArmor 표시 이름은 이행 안 함(이행 시 같은 방법).
5. 디버그 키(F1~F6) 지급 목록에 신발이 남아 있다(개발 빌드 전용, 지급돼도 장착 불가).
6. `DropManager.objectInfos`(직렬화 호환용, 미사용)에는 옛 이름이 남아 있다(동작 무관).

## 4. 전수 대조표 (ItemSheet Equip 162행)

나오는 곳: "몹 X" = 씬 DropManager(정리 전 기준, 숫자 접미 생략), `파일:줄` = CSV 행, Boss = `Resources/Boss` 세트 조각·고유. 정예 확정 장비는 같은 몹의 드롭표에서 고른다.

| id | 이름(키) | 슬롯·세트 | 나오는 곳 | 분류 | 조치 |
|---|---|---|---|---|---|
| 2 | PickAxe | Weapon | 없음 | 더미(시험 행·아이콘 없음) | retired=1(획득 경로 원래 없음) |
| 6 | Cap | Earring·RabbitArmor | 몹 Rabbit·Rabbit2_, ShopStock:11 | 현행 유지(옛 이름) | 이름 이행 "Cap" → "Basic Earring"(키·세트·스탯 유지) |
| 7 | Armor | Chest·RabbitArmor | 몹 Rabbit·Rabbit2_, ShopStock:12 | 현행 유지 | - |
| 8 | HighArmor | Chest | 없음 | 더미(시험 행·아이콘 없음) | retired=1(획득 경로 원래 없음) |
| 9 | LegArmor | Necklace·RabbitArmor | 몹 Rabbit·Rabbit_BD(프리팹), ShopStock:14 | 현행 유지(옛 이름) | 이름 이행 "LegArmor" → "Basic Necklace"(키·세트·스탯 유지) |
| 10 | HandArmor | Ring·RabbitArmor | 몹 Rabbit·Rabbit_BD(프리팹), ShopStock:13 | 현행 유지(옛 이름) | 이름 이행 "HandArmor" → "Basic Ring"(키·세트·스탯 유지) |
| 11 | Necklace | Necklace | 없음 | 더미(시험 행·아이콘 없음) | retired=1(획득 경로 원래 없음) |
| 12 | Ring | Ring | 없음 | 더미(시험 행·아이콘 없음) | retired=1(획득 경로 원래 없음) |
| 13 | ToxicPickAxe | Weapon | 없음 | 더미(시험 행·아이콘 없음) | retired=1(획득 경로 원래 없음) |
| 14 | Shoes | Charm | 몹 Rabbit·Rabbit_BD(프리팹), ShopStock(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 15 | Leather Knuckles | Weapon | 몹 Rabbit·Rabbit2_, ShopStock:9 | 현행 유지 | - |
| 16 | Wood Spear | Weapon | 몹 Rabbit·Rabbit_BD(프리팹), ShopStock:10 | 현행 유지 | - |
| 17 | LeafCap | Earring·LeafArmor | 몹 Flowermole, QuestsFloor1:30 | 현행 유지(옛 이름) | 이름 이행 "LeafCap" → "Leaf Earring"(키·세트·스탯 유지) |
| 18 | LeafArmor | Chest·LeafArmor | 몹 Flowermole | 현행 유지 | - |
| 19 | LeafLegArmor | Necklace·LeafArmor | 몹 Mole | 현행 유지(옛 이름) | 이름 이행 "LeafLegArmor" → "Leaf Necklace"(키·세트·스탯 유지) |
| 20 | LeafHandArmor | Ring·LeafArmor | 몹 Mole | 현행 유지(옛 이름) | 이름 이행 "LeafHandArmor" → "Leaf Ring"(키·세트·스탯 유지) |
| 21 | LeafShoes | Charm | 몹 Mole | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 22 | LeafStaff | Weapon | 몹 Mole | 현행 유지 | - |
| 23 | LeafAdae | Weapon | 몹 Flowermole, QuestsFloor1:3 | 현행 유지 | - |
| 24 | RatCap | Earring·RatArmor | 몹 RatCheif, Floor1BestiaryRewards:9, QuestsFloor1:29 | 현행 유지(옛 이름) | 이름 이행 "RatCap" → "Rat Earring"(키·세트·스탯 유지) |
| 25 | RatArmor | Chest·RatArmor | 몹 RatCheif | 현행 유지 | - |
| 26 | RatLegArmor | Necklace·RatArmor | 몹 RatThief | 현행 유지(옛 이름) | 이름 이행 "RatLegArmor" → "Rat Necklace"(키·세트·스탯 유지) |
| 27 | RatHandArmor | Ring·RatArmor | 몹 RatThief | 현행 유지(옛 이름) | 이름 이행 "RatHandArmor" → "Rat Ring"(키·세트·스탯 유지) |
| 28 | RatShoes | Charm | 몹 RatThief | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 29 | DoubleDagger | Weapon | 몹 RatThief | 현행 유지 | - |
| 30 | PoisonSword | Weapon | 몹 RatCheif | 현행 유지 | - |
| 31 | RatCheifDagger | Weapon | 없음 | 현행 유지 | - |
| 32 | ScareCap | Earring·ScareArmor | 몹 BossSpawoner·CreepingTerror | 현행 유지(옛 이름) | 이름 이행 "ScareCap" → "Scarecrow Earring"(키·세트·스탯 유지) |
| 33 | ScareArmor | Chest·ScareArmor | 몹 BossSpawoner·CreepingTerror | 현행 유지 | - |
| 34 | ScareLegArmor | Necklace·ScareArmor | 몹 BossSpawoner·ScareCrow | 현행 유지(옛 이름) | 이름 이행 "ScareLegArmor" → "Scarecrow Necklace"(키·세트·스탯 유지) |
| 35 | ScareHandArmor | Ring·ScareArmor | 몹 BossSpawoner·ScareCrow | 현행 유지(옛 이름) | 이름 이행 "ScareHandArmor" → "Scarecrow Ring"(키·세트·스탯 유지) |
| 36 | ScareShoes | Charm | 몹 BossSpawoner·ScareCrow, Boss PassiveTerror 세트 조각(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 37 | SacreGreatSword | Weapon | 몹 BossSpawoner·CreepingTerror | 현행 유지 | - |
| 38 | ScareStaff | Weapon | 몹 BossSpawoner·CreepingTerror | 현행 유지 | - |
| 39 | ScareSide | Weapon | 몹 BossSpawoner | 현행 유지 | - |
| 40 | ScareClow | Weapon | 몹 BossSpawoner·ScareCrow | 현행 유지 | - |
| 41 | RustedSword | Weapon | 몹 Rabbit·Rabbit_BD(프리팹), ShopStock:8 | 현행 유지 | - |
| 42 | RustedSwordandShield | Weapon·RabbitArmor | 몹 Rabbit·Rabbit2_ | 현행 유지 | - |
| 43 | Four-Leaf Clover Ring | Ring | 몹 Rabbit·Rabbit_BD(프리팹) | 현행 유지 | - |
| 44 | Rabbit's Foot | Necklace | 몹 Rabbit·Rabbit2_ | 현행 유지 | - |
| 53 | Green Wreath | Necklace | 몹 Mole | 현행 유지 | - |
| 58 | Gentle Spring Guidance | Necklace | 몹 Flowermole | 현행 유지 | - |
| 59 | Sharp Petal Ring | Ring | 몹 Flowermole | 현행 유지 | - |
| 62 | Nasty Rat Ring | Ring | 몹 RatThief | 현행 유지 | - |
| 67 | Necklace of Spreading Corruption | Necklace | 몹 RatCheif | 현행 유지 | - |
| 68 | Necklace of the Sly Plunderer | Necklace | 몹 RatCheif | 현행 유지 | - |
| 69 | Ring Finger that Tears Wounds | Ring | 몹 RatCheif | 현행 유지 | - |
| 72 | Shabby Straw Ring | Ring | 몹 ScareCrow | 현행 유지 | - |
| 78 | Necklace of Silent Screams | Necklace | 몹 BossSpawoner, Boss/PassiveTerror_BD.asset | 현행 유지 | - |
| 95 | CliffCap | Earring·CliffArmor | 몹 L2_A_Shroom·L2_B_ShroomUp·L2_D_BossN | 현행 유지(옛 이름) | 이름 이행 "Cliffwalker Hood" → "Cliffwalker Earring"(키·세트·스탯 유지) |
| 96 | CliffArmor | Chest·CliffArmor | 몹 L2_B_SkelN·L2_C_EliteN·L2_C_SkelN·L2_D_BossN | 현행 유지 | - |
| 97 | CliffLegArmor | Necklace·CliffArmor | 몹 L2_B_Goblin·L2_B_GoblinN·L2_D_BossN | 현행 유지(옛 이름) | 이름 이행 "Cliffwalker Greaves" → "Cliffwalker Necklace"(키·세트·스탯 유지) |
| 98 | CliffHandArmor | Ring·CliffArmor | 몹 L2_B_SkelN·L2_D_BossN | 현행 유지(옛 이름) | 이름 이행 "Cliffwalker Gloves" → "Cliffwalker Ring"(키·세트·스탯 유지) |
| 99 | CliffShoes | Charm | 몹 L2_C_Goblin·L2_D_BossN, Boss AbyssWizard 세트 조각(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 100 | CliffStaff | Weapon·CliffArmor | 몹 L2_A_EyeN·L2_C_EliteN·L2_D_BossN | 현행 유지 | - |
| 101 | CliffBlade | Weapon·CliffArmor | 몹 L2_B_Goblin·L2_C_EliteN·L2_C_Goblin·L2_D_BossN | 현행 유지 | - |
| 102 | AbyssStaff | Weapon | 몹 L2_D_BossN, Boss/AbyssWizard_BD.asset | 현행 유지 | - |
| 103 | Eye of the Cliffs | Necklace | 몹 L2_A_EyeN·L2_D_BossN | 현행 유지 | - |
| 104 | Bringer's Sigil Ring | Ring | 몹 L2_C_EliteN, ArmorCraftRecipes:2 | 현행 유지 | - |
| 106 | FangDagger | Weapon·WolfArmor | 몹 Rabbit·Rabbit2_, QuestsFloor1:8 | 현행 유지 | - |
| 111 | TuskKnuckles | Weapon·WolfArmor | 몹 Mole | 현행 유지 | - |
| 112 | WispLantern | Weapon·WispArmor | 몹 RatThief·ScareCrow | 현행 유지 | - |
| 113 | ShellShield | Weapon·WispArmor | 몹 CreepingTerror·RatCheif | 현행 유지 | - |
| 114 | WolfCap | Earring·WolfArmor | 몹 Mole·Rabbit·Rabbit2_, ArmorCraftRecipes:3, Floor1BestiaryRewards:10, QuestsFloor1:7 | 현행 유지(옛 이름) | 이름 이행 "Pack Hunter Hood" → "Pack Hunter Earring"(키·세트·스탯 유지) |
| 115 | WolfArmor | Chest·WolfArmor | 몹 Mole·Rabbit·Rabbit2_, ArmorCraftRecipes:6, QuestsFloor1:34 | 현행 유지 | - |
| 116 | WolfLegArmor | Necklace·WolfArmor | 몹 Mole·Rabbit·Rabbit2_, ArmorCraftRecipes:5, QuestsFloor1:33 | 현행 유지(옛 이름) | 이름 이행 "Pack Hunter Greaves" → "Pack Hunter Necklace"(키·세트·스탯 유지) |
| 117 | WolfHandArmor | Ring·WolfArmor | 몹 Mole·Rabbit·Rabbit2_, ArmorCraftRecipes:4, QuestsFloor1:10 | 현행 유지(옛 이름) | 이름 이행 "Pack Hunter Gloves" → "Pack Hunter Ring"(키·세트·스탯 유지) |
| 118 | WolfShoes | Charm | 몹 Mole·Rabbit·Rabbit2_, Craft(삭제), Q204 보상→WolfHandArmor | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 119 | WispCap | Earring·WispArmor | 몹 CreepingTerror·RatCheif·RatThief·ScareCrow, ArmorCraftRecipes:15 | 현행 유지(옛 이름) | 이름 이행 "Night Lantern Hood" → "Night Lantern Earring"(키·세트·스탯 유지) |
| 120 | WispArmor | Chest·WispArmor | 몹 CreepingTerror·RatCheif·RatThief·ScareCrow, ArmorCraftRecipes:18 | 현행 유지 | - |
| 121 | WispLegArmor | Necklace·WispArmor | 몹 CreepingTerror·RatCheif·RatThief·ScareCrow, ArmorCraftRecipes:17 | 현행 유지(옛 이름) | 이름 이행 "Night Lantern Greaves" → "Night Lantern Necklace"(키·세트·스탯 유지) |
| 122 | WispHandArmor | Ring·WispArmor | 몹 CreepingTerror·RatCheif·RatThief·ScareCrow, ArmorCraftRecipes:16 | 현행 유지(옛 이름) | 이름 이행 "Night Lantern Gloves" → "Night Lantern Ring"(키·세트·스탯 유지) |
| 123 | WispShoes | Charm | 몹 CreepingTerror·RatCheif·RatThief·ScareCrow, Craft(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 124 | BearClaw | Weapon·GroveArmor | 몹 Bear, QuestsFloor1:12 | 현행 유지 | - |
| 125 | RootStaff | Weapon·GroveArmor | 몹 Ent, QuestsFloor1:14 | 현행 유지 | - |
| 126 | HeartwoodGreatsword | Weapon | Boss/AncientEnt_BD.asset | 현행 유지 | - |
| 131 | PhantomScythe | Weapon·FaeArmor | 몹 Ghost | 현행 유지 | - |
| 132 | FaeWand | Weapon·FaeArmor | 몹 Fairy | 현행 유지 | - |
| 133 | GroveCap | Earring·GroveArmor | 몹 Bear·Ent, ArmorCraftRecipes:7, Floor1BestiaryRewards:11, QuestsFloor1:11 | 현행 유지(옛 이름) | 이름 이행 "Ancient Grove Hood" → "Ancient Grove Earring"(키·세트·스탯 유지) |
| 134 | GroveArmor | Chest·GroveArmor | 몹 Bear·Ent, ArmorCraftRecipes:10, QuestsFloor1:45 | 현행 유지 | - |
| 135 | GroveLegArmor | Necklace·GroveArmor | 몹 Bear·Ent, ArmorCraftRecipes:9, QuestsFloor1:44 | 현행 유지(옛 이름) | 이름 이행 "Ancient Grove Greaves" → "Ancient Grove Necklace"(키·세트·스탯 유지) |
| 136 | GroveHandArmor | Ring·GroveArmor | 몹 Bear·Ent, ArmorCraftRecipes:8, QuestsFloor1:44 | 현행 유지(옛 이름) | 이름 이행 "Ancient Grove Gloves" → "Ancient Grove Ring"(키·세트·스탯 유지) |
| 137 | GroveShoes | Charm | 몹 Bear·Ent, Craft(삭제), Q309 보상→Enhance Stone x2 | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 138 | FaeCap | Earring·FaeArmor | 몹 Fairy·Ghost, ArmorCraftRecipes:19 | 현행 유지(옛 이름) | 이름 이행 "Will-o'-Fae Hood" → "Will-o'-Fae Earring"(키·세트·스탯 유지) |
| 139 | FaeArmor | Chest·FaeArmor | 몹 Fairy·Ghost, ArmorCraftRecipes:22 | 현행 유지 | - |
| 140 | FaeLegArmor | Necklace·FaeArmor | 몹 Fairy·Ghost, ArmorCraftRecipes:21 | 현행 유지(옛 이름) | 이름 이행 "Will-o'-Fae Greaves" → "Will-o'-Fae Necklace"(키·세트·스탯 유지) |
| 141 | FaeHandArmor | Ring·FaeArmor | 몹 Fairy·Ghost, ArmorCraftRecipes:20 | 현행 유지(옛 이름) | 이름 이행 "Will-o'-Fae Gloves" → "Will-o'-Fae Ring"(키·세트·스탯 유지) |
| 142 | FaeShoes | Charm | 몹 Fairy·Ghost, Craft(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 143 | FrogTongueWhip | Weapon·BogArmor | 몹 Frog, Floor1WeaponTrials:3 | 현행 유지 | - |
| 144 | CrocJawAxe | Weapon·BogArmor | 몹 Croc, Floor1WeaponTrials:3, MonsterExtraDrops:9, QuestsFloor1:16 | 현행 유지 | - |
| 145 | PetalPincer | Weapon·BogArmor | 몹 Crab | 현행 유지 | - |
| 146 | BloodstingRapier | Weapon·GloomArmor | 몹 Mosquito | 현행 유지 | - |
| 147 | SlimeMace | Weapon·GloomArmor | 몹 Slime, QuestsFloor1:51 | 현행 유지 | - |
| 148 | PrimordialGel | Weapon | Boss/GiantSlime_BD.asset | 현행 유지 | - |
| 154 | BogCap | Earring·BogArmor | 몹 Crab·Croc·Frog, ArmorCraftRecipes:11, Floor1BestiaryRewards:12, QuestsFloor1:17 | 현행 유지(옛 이름) | 이름 이행 "Bog Stalker Hood" → "Bog Stalker Earring"(키·세트·스탯 유지) |
| 155 | BogArmor | Chest·BogArmor | 몹 Crab·Croc·Frog, ArmorCraftRecipes:14, QuestsFloor1:49 | 현행 유지 | - |
| 156 | BogLegArmor | Necklace·BogArmor | 몹 Crab·Croc·Frog, ArmorCraftRecipes:13, QuestsFloor1:48 | 현행 유지(옛 이름) | 이름 이행 "Bog Stalker Greaves" → "Bog Stalker Necklace"(키·세트·스탯 유지) |
| 157 | BogHandArmor | Ring·BogArmor | 몹 Crab·Croc·Frog, ArmorCraftRecipes:12 | 현행 유지(옛 이름) | 이름 이행 "Bog Stalker Gloves" → "Bog Stalker Ring"(키·세트·스탯 유지) |
| 158 | BogShoes | Charm | 몹 Crab·Croc·Frog, Craft(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 159 | GloomCap | Earring·GloomArmor | 몹 Mosquito·Slime, ArmorCraftRecipes:23 | 현행 유지(옛 이름) | 이름 이행 "Gloom Swarm Hood" → "Gloom Swarm Earring"(키·세트·스탯 유지) |
| 160 | GloomArmor | Chest·GloomArmor | 몹 Mosquito·Slime, ArmorCraftRecipes:26 | 현행 유지 | - |
| 161 | GloomLegArmor | Necklace·GloomArmor | 몹 Mosquito·Slime, ArmorCraftRecipes:25 | 현행 유지(옛 이름) | 이름 이행 "Gloom Swarm Greaves" → "Gloom Swarm Necklace"(키·세트·스탯 유지) |
| 162 | GloomHandArmor | Ring·GloomArmor | 몹 Mosquito·Slime, ArmorCraftRecipes:24 | 현행 유지(옛 이름) | 이름 이행 "Gloom Swarm Gloves" → "Gloom Swarm Ring"(키·세트·스탯 유지) |
| 163 | GloomShoes | Charm | 몹 Mosquito·Slime, Craft(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 164 | CutthroatDagger | Weapon·OutlawArmor | 몹 Cutthroat, Floor1WeaponTrials:3, QuestsFloor1:54 | 현행 유지 | - |
| 165 | BanditShortbow | Weapon·OutlawArmor | 몹 Archer | 현행 유지 | - |
| 166 | RaiderBuckler | Weapon·RaiderArmor | 몹 Shield | 현행 유지 | - |
| 167 | ChiefsWarBanner | Weapon | Boss/BanditChief_BD.asset | 현행 유지 | - |
| 172 | OutlawCap | Earring·OutlawArmor | 몹 Archer·Cutthroat, ArmorCraftRecipes:27, QuestsFloor1:52 | 현행 유지(옛 이름) | 이름 이행 "Outlaw Hood" → "Outlaw Earring"(키·세트·스탯 유지) |
| 173 | OutlawArmor | Chest·OutlawArmor | 몹 Archer·Cutthroat, ArmorCraftRecipes:30, QuestsFloor1:53 | 현행 유지 | - |
| 174 | OutlawLegArmor | Necklace·OutlawArmor | 몹 Archer·Cutthroat, ArmorCraftRecipes:29 | 현행 유지(옛 이름) | 이름 이행 "Outlaw Breeches" → "Outlaw Necklace"(키·세트·스탯 유지) |
| 175 | OutlawHandArmor | Ring·OutlawArmor | 몹 Archer·Cutthroat, ArmorCraftRecipes:28, MiniBosses:5 | 현행 유지(옛 이름) | 이름 이행 "Outlaw Gloves" → "Outlaw Ring"(키·세트·스탯 유지) |
| 176 | OutlawShoes | Charm | 몹 Archer·Cutthroat, Craft(삭제), M4 확정→OutlawHandArmor | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 177 | RaiderCap | Earring·RaiderArmor | 몹 Shield·Torch, ArmorCraftRecipes:31, Floor1BestiaryRewards:13 | 현행 유지(옛 이름) | 이름 이행 "Iron Raider Helm" → "Iron Raider Earring"(키·세트·스탯 유지) |
| 178 | RaiderArmor | Chest·RaiderArmor | 몹 Shield·Torch, ArmorCraftRecipes:34, QuestsFloor1:55 | 현행 유지 | - |
| 179 | RaiderLegArmor | Necklace·RaiderArmor | 몹 Shield·Torch, ArmorCraftRecipes:33 | 현행 유지(옛 이름) | 이름 이행 "Iron Raider Greaves" → "Iron Raider Necklace"(키·세트·스탯 유지) |
| 180 | RaiderHandArmor | Ring·RaiderArmor | 몹 Shield·Torch, ArmorCraftRecipes:32 | 현행 유지(옛 이름) | 이름 이행 "Iron Raider Gauntlets" → "Iron Raider Ring"(키·세트·스탯 유지) |
| 181 | RaiderShoes | Charm | 몹 Shield·Torch, Craft(삭제) | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 182 | PebbleGauntlet | Weapon·StoneArmor | 몹 GolemSmall, MonsterExtraDrops:11 | 현행 유지 | - |
| 183 | QuakestoneMaul | Weapon·StoneArmor | 몹 GolemMedium, MonsterExtraDrops:12 | 현행 유지 | - |
| 184 | MonolithShield | Weapon·StoneArmor | 몹 GolemLarge | 현행 유지 | - |
| 185 | GatekeepersOath | Weapon | Boss/Gatekeeper_BD.asset | 현행 유지 | - |
| 190 | StoneCap | Earring·StoneArmor | 몹 GolemLarge·GolemMedium·GolemSmall·RuinSpirit, ArmorCraftRecipes:35, QuestsFloor1:57 | 현행 유지(옛 이름) | 이름 이행 "Stoneborn Helm" → "Stoneborn Earring"(키·세트·스탯 유지) |
| 191 | StoneArmor | Chest·StoneArmor | 몹 GolemLarge·GolemMedium·GolemSmall·RuinSpirit, ArmorCraftRecipes:38, Floor1BestiaryRewards:14, QuestsFloor1:58 | 현행 유지 | - |
| 192 | StoneLegArmor | Necklace·StoneArmor | 몹 GolemLarge·GolemMedium·GolemSmall·RuinSpirit, ArmorCraftRecipes:37, QuestsFloor1:59 | 현행 유지(옛 이름) | 이름 이행 "Stoneborn Greaves" → "Stoneborn Necklace"(키·세트·스탯 유지) |
| 193 | StoneHandArmor | Ring·StoneArmor | 몹 GolemLarge·GolemMedium·GolemSmall·RuinSpirit, ArmorCraftRecipes:36, QuestsFloor1:60 | 현행 유지(옛 이름) | 이름 이행 "Stoneborn Gauntlets" → "Stoneborn Ring"(키·세트·스탯 유지) |
| 194 | StoneShoes | Charm | 몹 GolemLarge·GolemMedium·GolemSmall·RuinSpirit, Craft(삭제), Q609 보상→Enhance Stone x3 | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 195 | RampartSword | Weapon·RampartArmor | 몹 Knight | 현행 유지 | - |
| 196 | SentryHalberd | Weapon·RampartArmor | 몹 Sentry | 현행 유지 | - |
| 197 | GargoyleWing | Weapon·SkyArmor | 몹 Gargoyle | 현행 유지 | - |
| 198 | SkyfallTalon | Weapon | Boss/GargoyleLord_BD.asset | 현행 유지 | - |
| 203 | RampartCap | Earring·RampartArmor | 몹 Knight·Sentry | 현행 유지(옛 이름) | 이름 이행 "Rampart Guard Helm" → "Rampart Guard Earring"(키·세트·스탯 유지) |
| 204 | RampartArmor | Chest·RampartArmor | 몹 Knight·Sentry | 현행 유지 | - |
| 205 | RampartLegArmor | Necklace·RampartArmor | 몹 Knight·Sentry | 현행 유지(옛 이름) | 이름 이행 "Rampart Guard Greaves" → "Rampart Guard Necklace"(키·세트·스탯 유지) |
| 206 | RampartHandArmor | Ring·RampartArmor | 몹 Knight·Sentry | 현행 유지(옛 이름) | 이름 이행 "Rampart Guard Gauntlets" → "Rampart Guard Ring"(키·세트·스탯 유지) |
| 207 | RampartShoes | Charm | 몹 Knight·Sentry | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 208 | SkyCap | Earring·SkyArmor | 몹 Gargoyle·Wisp | 현행 유지(옛 이름) | 이름 이행 "Skybound Cowl" → "Skybound Earring"(키·세트·스탯 유지) |
| 209 | SkyArmor | Chest·SkyArmor | 몹 Gargoyle·Wisp | 현행 유지 | - |
| 210 | SkyLegArmor | Necklace·SkyArmor | 몹 Gargoyle·Wisp | 현행 유지(옛 이름) | 이름 이행 "Skybound Leggings" → "Skybound Necklace"(키·세트·스탯 유지) |
| 211 | SkyHandArmor | Ring·SkyArmor | 몹 Gargoyle·Wisp | 현행 유지(옛 이름) | 이름 이행 "Skybound Gloves" → "Skybound Ring"(키·세트·스탯 유지) |
| 212 | SkyShoes | Charm | 몹 Gargoyle·Wisp | 옛 체계(폐기 슬롯 신발) | 제거: 드롭·상점·제작·보상에서 뺌, retired=1, 장착 불가·판매 가능 |
| 241 | HuntersLongbow | Weapon·OutlawArmor | Floor1WeaponTrials:2, MonsterExtraDrops:15 | 현행 유지 | - |
| 242 | GnarledClub | Weapon·GroveArmor | Floor1WeaponTrials:2, MonsterExtraDrops:8, MonsterExtraDrops:17 | 현행 유지 | - |
| 243 | WoodcuttersAxe | Weapon·OutlawArmor | MonsterExtraDrops:10, MonsterExtraDrops:18 | 현행 유지 | - |
| 244 | TwinFang | Weapon·WolfArmor | Floor1WeaponTrials:2, MonsterExtraDrops:2, MonsterExtraDrops:7 | 현행 유지 | - |
| 245 | VineLash | Weapon·FaeArmor | MonsterExtraDrops:16 | 현행 유지 | - |
| 258 | BanditPlunderMedal | Necklace | MonsterExtraDrops:25 | 현행 유지 | - |
| 259 | ChiefsLastStandRing | Ring | QuestsFloor1:24 | 현행 유지 | - |
| 260 | TitleWanderer | Etc | 없음 | 옛 체계(11-05 폐기 칭호·오라) | 이미 Etc. retired=1 표시만 |
| 261 | AuraFaintGlow | Etc | 없음 | 옛 체계(11-05 폐기 칭호·오라) | 이미 Etc. retired=1 표시만 |
| 262 | EarringPlain | Earring | 없음 | 현행 유지 | - |
| 263 | EarringHunter | Earring | 없음 | 현행 유지 | - |
| 264 | BraceletPlain | Bracelet | 없음 | 현행 유지 | - |
| 265 | BraceletSwift | Bracelet | 없음 | 현행 유지 | - |
| 266 | CharmPlain | Charm | 없음 | 현행 유지 | - |
| 267 | StonePlain | Stone | 없음 | 현행 유지 | - |
| 313 | SentryCrossbow | Weapon | MonsterExtraDrops:26 | 현행 유지 | - |
| 314 | HuntersJavelin | Weapon | MonsterExtraDrops:27 | 현행 유지 | - |
| 315 | FairysGrimoire | Weapon | MonsterExtraDrops:28, QuestsFloor1:62 | 현행 유지 | - |
| 316 | BeltPlain | Belt | 없음 | 현행 유지 | - |
| 317 | WandererBracelet | Bracelet | 없음 | 현행 유지 | - |
| 318 | HunterCharm | Charm | MonsterExtraDrops:29 | 현행 유지 | - |
