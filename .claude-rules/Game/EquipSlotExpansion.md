# 장비 슬롯 확장 설계 (11-04) — 오라·칭호 등 신규 슬롯, 처음 잠금, 층계별 해금

> 근거: 사용자 11-04 Q11 — 도감 보상으로 칭호·특수 아이템과 **새 장비 슬롯**(오라/칭호 등) 신설, 처음엔 잠금·층계별 1~2개 해금, 현재 슬롯은 고정이라 구조 검토.
> 상태: 설계만. 수치는 임시 가정(`Docs/OpenQuestions.md` Q-SLOT). 코드 변경 없음.

## 1. 현재 구조 (코드 확인)

- `EquipSlot { Ring=1, Head=2, Necklace=3, Chest=5, Hand=6, Weapon=7, Leg=8, Shoes=9 }` (`Inventory/Model/EquipmentModel.cs`). 8칸 고정, 4는 빠진 번호(예약 가능).
- 장비 외형은 `Player/Rig/CelEquipLayer`(Legs·Body·Gloves·Head)가 담당. Ring·Necklace·Weapon은 외형 레이어 없음.
- 세이브: `Datas` 의 기존 equipment 배열(차원 유지, 이름 변경 금지). 추가 데이터는 별도 키로 둔 선례 있음(`inventoryFlags`, `quickslot.items`).

## 2. 슬롯 후보 (신규 4종)

| 슬롯 | 키 | 넣는 것 | 외형 영향 | 효과 성격 |
|---|---|---|---|---|
| 칭호 | `Title` | 칭호 아이템(도감·업적 보상) | 없음(이름표 옆 텍스트만) | 소폭 스탯 1줄(예: 드롭률+1%) |
| 오라 | `Aura` | 오라 아이템(보스 첫 처치·도감 완성) | 발밑 은은한 링 파티클(코드 생성, 캐릭터 몸 레이어 무관) | 패시브 1줄(예: 이동속도+3%) |
| 부적 | `Charm` | 부적(퀘스트·상점) | 없음 | 상황 효과(예: 물약 쿨다운 −0.5초) |
| 망토 | `Cape` | 망토(제작·드롭) | **있음** — 몸 뒤 레이어 1장(신규 `CelEquipLayer.Slot.Back=5`, 후속 아트) | 방어/HP 보정 |

원칙: 새 슬롯 아이템은 **스탯 인플레 금지**(초반 소박 원칙, `Floor1Completion.md`). 기본 효과는 편의·수집 보상 성격이고, 세트 효과(`tfName`)에 포함하지 않는다(세트 판정은 기존 8슬롯만 → `SetBonusEvaluator` 무변경).
반지 2칸 여부는 별도 질문이다(`InventoryRefactorDesign.md` §5 유지).

## 3. 해금 조건

처음 4칸 모두 잠금(자물쇠 아이콘, 클릭 시 "해금 조건" 툴팁). 층계별 1~2칸:

| 층계 | 해금 | 조건(임시) |
|---|---|---|
| 1층계 | `Title` | 1층계 도감 완성(몹 도감 등록 전부) 또는 1층계 졸업 — 보상 수령 시 해금 + 첫 칭호 1개 |
| 2층계 | `Aura` | 2층계 보스 첫 처치(+ 도감 50% 이상은 보너스 오라) |
| 3층계 | `Charm` | 3층계 도감 완성 또는 퀘스트 체인 종료 |
| 4층계 | `Cape` | 4층계 보스 첫 처치 |

- 도감 완성 보상은 "해금 + 해당 슬롯용 특수 아이템 1종" 한 묶음이다. 완성하지 못해도 보스 첫 처치로 **한 번 더 열 수 있는 대체 경로**를 둬서 도감 미완이 슬롯 영구 차단이 되지 않게 한다(임시 가정, 도감은 선택 콘텐츠).
- 해금은 영구(주인공 세이브 단위, 아이템을 잃어도 유지).

## 4. 구조 변경 (코드 영향 최소)

1. `EquipSlot` 에 `Title=10, Aura=11, Charm=12, Cape=13` 추가(기존 값 불변). `EquipmentModel` 용량 확장 + `IsUnlocked(slot)`.
2. `SlotRules`(순수): 기본 8칸은 항상 해금, 신규 4칸은 해금 비트 필요. 아이템 장착 판정은 `ItemSheet` 부위 컬럼(`Weapon`/`Head`/…)에 `Title`/`Aura`/`Charm`/`Cape` 값 추가 — 컬럼 추가 없음, 값만 추가. 기존 코드가 부위 문자열을 switch 로 쓰는 곳은 default 처리 확인 필요.
3. 스탯 합산: 신규 슬롯 장비도 기존 장비 스탯 합산 경로를 통과(부위 무관 합산인지 구현 때 확인).
4. UI: 장비창에 신규 4칸 행 추가, 잠금 표시. 기존 8칸 배치·키·게임패드 이동 불변.
5. 외형: `Aura`는 `AuraFx`(코드 생성 링), `Cape`는 후속 레이어. 1~2차에는 Aura·Title·Charm만(외형 레이어 변경 없음).

## 5. ES3 필드 (추가만, 기존 키·배열 이름·차원 변경 금지)

| 키 | 형 | 내용 |
|---|---|---|
| `equip.extra` | string[4] | 신규 슬롯에 낀 아이템 id(순서 Title·Aura·Charm·Cape, 빈칸 "") |
| `equip.extraUnlock` | int | 해금 비트마스크(1=Title, 2=Aura, 4=Charm, 8=Cape) |
| `equip.extraFlags` | 선택 | 강화·잠금 등 인스턴스 값이 필요하면 `inventoryFlags` 방식으로 병렬 배열 |

- 세이브 슬롯 파일별 저장(`quickslot.items` 선례). 읽기 실패·키 없음 → 전부 잠금·빈칸으로 시작(구 세이브 호환). 로드 시 `equip.extra`에 해금 안 된 슬롯 아이템이 있으면 인벤토리로 반환.

## 6. 임시 가정 / 열린 질문 (`Docs/OpenQuestions.md` Q-SLOT)

- Q-SLOT1: 슬롯 4종·순서·층계 배정은 임시. 칭호 vs 오라 해금 순서 바꿔도 무방.
- Q-SLOT2: 부적·망토 효과 수치는 아트 의존이라 후순위.
- Q-SLOT3: 쌍 반지(2칸)는 이 문서 범위 밖.

## 7. 구현 큐 분해 (`Docs/Handoff/queue.md`)

1. SLOT-1 모델·세이브: `EquipSlot` 확장·`SlotRules`·`equip.extra`/`extraUnlock` 직렬화 + EditMode 테스트(구 세이브 로드).
2. SLOT-2 UI: 장비창 신규 4칸·잠금 툴팁·장착 해제, 입력·패드 이동.
3. SLOT-3 해금·보상: 도감 완성/보스 첫 처치 → 해금 + 칭호 첫 아이템, ItemSheet 신규 부위 행 몇 개(Title·Aura 1종씩).
4. SLOT-4 `AuraFx` 코드 생성 링 + 칭호 이름표 텍스트(외형 레이어 변경 없음).
5. SLOT-5 (후속) 망토 `CelEquipLayer.Slot.Back` 아트·레이어.
