# 장비 슬롯 확장 설계 (11-04 초안 → 11-05 개정) — 비외형 장신구 슬롯, 처음 잠금, 층계별 해금

> 근거: 사용자 11-04 Q11(새 슬롯·잠금·층계별 해금) + **11-05 결정 2**(`Docs/OpenQuestions.md` [결정 11-05]): 칭호(머리 위 표시)·오라·망토 **폐기**, 신규 슬롯은 귀걸이·팔찌·반지·목걸이·마법석 등 **외형에 안 그려지는 장신구류**. 외형에 영향 주는 부위는 **투구·상의·하의·신발·장갑만**(그래픽 리소스 절약). 해금(잠금/층계별) 구조는 유지.
> 상태: 설계. 수치는 임시 가정(`Docs/OpenQuestions.md` Q-SLOT). 구현은 §8 SLOT-R1~.

## 1. 현재 구조 (코드·데이터 확인, 11-05)

- `EquipSlot { Ring=1, Head=2, Necklace=3, Chest=5, Hand=6, Weapon=7, Leg=8, Shoes=9, Title=10, Aura=11, Charm=12, Cape=13 }` (`Inventory/Model/EquipmentModel.cs`). **기본 8칸에 반지·목걸이가 이미 있다**(ItemSheet 약 17행). 10~13 은 SLOT-1~4(11-04)로 들어온 해금형 신규 4칸.
- 외형은 `Player/Rig/CelEquipLayer`(Legs·Body·Gloves·Head)만. Ring·Necklace·Weapon·신규 4칸은 외형 레이어 없음.
- 신규 4칸 구현물: `Inventory/Model/SlotRules.cs`(해금 비트·`EncodeStack`·`EquipExtraSave`), `EquipmentModel.ExtraUnlockMask/LoadExtra/ToExtraArray`, `UI/ExtraEquip.cs`(런타임 패널·ItemData 보관·ES3·해금 보상 `FirstItem`), `UI/ExtraEquipFx.cs`(오라 링·칭호 이름표), `Editor/Tests/EquipSlotExpansionTests.cs`, ItemSheet 260 `TitleWanderer`·261 `AuraFaintGlow`, `BossReward`·`BestiaryMilestones` 의 해금 호출.
- ES3: `equip.extra`(string[4]), `equip.extraUnlock`(int 비트). 세트 판정(`SetBonusEvaluator`)은 기본 8칸만 본다.

## 2. 폐기·재정의

| 11-04 슬롯 | 11-05 처리 |
|---|---|
| Title(10) 칭호 | **폐기**. 머리 위 표시 없음 → 귀걸이 `Earring` 로 재정의 |
| Aura(11) 오라 | **폐기** → 팔찌 `Bracelet` |
| Charm(12) 부적 | **유지**(비외형, 사용자 폐기 목록에 없음) |
| Cape(13) 망토 | **폐기**(외형 레이어 필요) → 마법석 `Stone` |

- **enum 값(10~13)·ES3 키·배열 순서는 그대로 두고 이름/의미만 바꾼다**(세이브 호환: 배열 인덱스 0~3 이 Earring·Bracelet·Charm·Stone 이 됨). 비트도 1=Earring, 2=Bracelet, 4=Charm, 8=Stone.
- 추가 슬롯이 필요하면 **번호 14~(반지2 `Ring2`=14 등)** 와 비트 16~ 로 늘린다. `equip.extra` 배열 길이가 늘어도 로더는 짧은 구 배열을 허용(없는 칸=빈칸)해야 한다.
- 이미 지급된 폐기 아이템(`TitleWanderer`·`AuraFaintGlow`)은 장착 슬롯이 의미가 달라지므로 §6 처리.

## 3. 슬롯 후보와 스탯 성격

비외형 장신구는 **스탯·편의 한 줄**만 준다. 기본 8칸의 반지·목걸이와 겹치지 않게 주 스탯(공/방/HP)보다 **보조 스탯**(크리·회피·회복·드롭·이속·쿨감) 위주.

| 슬롯 | 키 | 성격(임시) | 비고 |
|---|---|---|---|
| 귀걸이 | `Earring` | 크리티컬 확률/피해, 명중 | 공격형 보조 |
| 팔찌 | `Bracelet` | 공격속도·쿨다운 감소·스킬 피해 | 던파 팔찌처럼 스킬 보조 |
| 부적 | `Charm` | 상황 효과(물약 쿨다운 −0.5초, 사망 시 EXP 손실 완화 등) | 편의형 |
| 마법석 | `Stone` | 속성 저항·상태이상 저항·마나 | 방어/속성 보조 |
| 반지 2 | `Ring2`(선택, 14) | 기본 반지와 같은 풀 | 쌍 반지(`InventoryRefactorDesign.md` §5)는 열린 질문 Q-SLOT3 유지 |

원칙(`Floor1Completion.md` 초반 소박): **스탯 인플레 금지**. 한 아이템은 보조 스탯 1~2줄, 층계가 올라도 상한 증가폭 작게. 합산 경로는 기존 장비 합산을 통과(부위 무관 합산 여부는 R1 때 확인).

## 4. 세트 효과와의 관계

- 신규 슬롯 아이템은 **세트 효과(`tfName`) 대상이 아니다**. `SetBonusEvaluator` 무변경(기본 8칸만 판정). 세트 부위 수를 늘리지 않는다 → 던파식 "장신구 세트" 확장은 이 단계 범위 밖.
- 다만 몬스터 계보(`LineageGear.md`) 드롭 장신구가 같은 계보를 암시하는 이름·설명은 허용(효과 연동은 없음).

## 5. 해금 조건 (구조 재사용)

처음 4칸 모두 잠금(자물쇠, 클릭 시 해금 조건 툴팁). 기존 `SlotRules.TrySlotForFloor(floor)` 순서 그대로(층계 번호 1~4 → 배열 인덱스 0~3):

| 층계 | 해금 | 조건(임시, 기존 SLOT-3 보상 경로 재사용) |
|---|---|---|
| 1층계 | `Earring` | 1층계 도감(B04 등) 또는 1층계 보스 첫 처치 |
| 2층계 | `Bracelet` | 2층계 보스 첫 처치 |
| 3층계 | `Charm` | 3층계 보스 첫 처치/도감 |
| 4층계 | `Stone` | 4층계 보스 첫 처치 |

- 도감 완성 보상 = "해금 + 해당 슬롯용 첫 아이템 1종". 보스 첫 처치 대체 경로로 영구 차단 방지. 해금은 영구(세이브 단위).
- 첫 아이템(`ExtraEquip.FirstItem`): 슬롯마다 소박한 Normal 등급 1종(예: `EarringPlain`, `BraceletPlain`) — ItemSheet 행 추가(R2).
- 툴팁 문구(`ExtraEquip` 해금 조건 문자열)와 슬롯 표기(`SlotLabel`)도 새 이름으로 교체.

## 6. 폐기 처리 (코드·데이터·세이브)

1. **코드 삭제**: `UI/ExtraEquipFx.cs`(오라 링·이름표) 전체, `ExtraEquip` 의 Aura/Title 분기·칭호 텍스트 관련 코드.
2. **이미 지급된 `TitleWanderer`(260)·`AuraFaintGlow`(261)**: 행은 **삭제하지 않는다**(세이브에 id 가 남아 있어 로드 실패/null 위험). `equipArea` 를 `Etc`(비장착 소지품)로 바꾸고 설명을 "폐기된 기념품", 판매 가능으로 둔다. 장착 중이던 것은 `LoadExtra` 가 `equipArea` 불일치로 보고 **인벤토리로 반환**(가득 차면 바닥 드롭 금지 → 소지품 일시 보관 대신 잔여 칸 없으면 우편/즉시 판매값 골드 지급 중 택1, 임시 가정: 판매값 골드 지급).
3. **장착 판정**: `ItemSheet.equipArea` 값 `Title/Aura/Cape` 는 더 이상 슬롯과 매칭되지 않음. 새 값 `Earring/Bracelet/Stone`(`Charm` 유지) 추가. 기존 부위 문자열 switch 는 default 처리 확인.
4. **ES3**: `equip.extra`·`equip.extraUnlock` 키·타입 불변. 이름 변경 금지·추가만. 구 세이브의 인덱스 0/1 에 폐기 아이템 id 가 있으면 위 2번 규칙으로 반환. 해금 비트는 그대로 유효(칭호 해금자는 귀걸이 해금).
5. **EditMode 테스트** `EquipSlotExpansionTests`: 슬롯 이름 변경 반영 + "폐기 아이템이 낀 구 세이브 로드 → 반환" 케이스 추가.

## 7. 외형·그래픽 정책

- 장비 외형 레이어: **투구(Head)·상의(Body)·하의(Legs)·신발(Shoes)·장갑(Gloves)** 만(`CelEquipLayer`). 무기는 기존 별도 처리.
- 반지·목걸이·귀걸이·팔찌·부적·마법석: **인벤토리/장비창 아이콘만**. 캐릭터에 그리지 않음. 새 `CelEquipLayer.Slot` 추가 금지(SLOT-5 망토 레이어 계획 폐기).
- 아이콘은 기존 장신구 아이콘 재색조/팔레트 변형으로 최소 제작.

## 8. 구현 큐 분해 (`Docs/Handoff/queue.md` SLOT-R1~)

1. **SLOT-R1 모델 재정의·폐기 정리**: enum 이름 변경(값 유지), `SlotRules.Extras`·비트 주석, `LoadExtra` 의 장착 불일치 반환, `ExtraEquipFx.cs` 삭제, `ExtraEquip` Aura/Title 분기 제거, 테스트 갱신.
2. **SLOT-R2 데이터**: ItemSheet 260/261 `Etc` 전환, 신규 슬롯 첫 아이템 4종 + 층계별 드롭 후보 몇 종(귀걸이·팔찌·부적·마법석), 설명·스탯(§3), `FirstItem` 갱신, `ItemSpec.md` 부위 값 표 갱신.
3. **SLOT-R3 UI 문구**: 장비 패널 슬롯 라벨·해금 조건 툴팁·아이콘(소스 재색조), 패드 커서 영향 확인.
4. **SLOT-R4 스탯 합산 확인**: 신규 슬롯 보조 스탯이 `GameData` 합산 경로에 정확히 들어가는지 + 세트 평가 무영향 확인(EditMode).
5. **SLOT-R5 (선택) 반지2**: `Ring2`=14·비트 16, 배열 길이 확장 로더(짧은 구 배열 허용).

## 9. 임시 가정 / 열린 질문 (`Docs/OpenQuestions.md` Q-SLOT)

- Q-SLOT1: 슬롯 4종·층계 배정은 임시(귀걸이→팔찌→부적→마법석).
- Q-SLOT3: 쌍 반지(Ring2) 포함 여부.
- Q-SLOT4: 폐기 아이템 반환 시 가방이 가득 찰 때 처리(판매값 골드 지급 임시).
