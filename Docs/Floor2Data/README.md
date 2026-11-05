# 2층계 데이터 초안 (11-05, AUTO-8) — 미리보기, Resources 에 넣지 않음

근거: `Floor2Plan.md` §2~5(구역 표·T 단계 수치 §3.2·계보 무기/세트). 생성: 공식 스크립트(수치 재현 가능). 맵·아이콘·도트·프리팹은 제외.

| 파일 | 대응 스키마 | 행 | 컬럼 검사 |
|---|---|---|---|
| `Floor2Zones.csv` | 신규(구역 표 참고용) | 5 | 일관 |
| `Floor2Monsters.csv` | `MonsterSheet.csv` 18열 | 21 | 18열 헤더 일치, ID 중복 0 |
| `Floor2Cliff_T6.csv` | MonsterSheet 기존 ① 몹 T6 상향안(HP·방어 전후) | 6 | 참고용 |
| `Floor2ItemSheet_draft.csv` | `ItemSheet.csv` 19열(index 268~) | 45 | 19열 헤더 일치, name 중복 0 |
| `Floor2SetItem_draft.csv` | `SetItem.csv` 2열 | 12 | 일치 |
| `Floor2ExtraDrops_draft.csv` | `MonsterExtraDrops.csv` 4열 | 28 | 일치, 드롭 아이템 전부 ItemSheet 초안에 존재 |

## 수치 규칙 (Floor2Plan §3.2)
- 평균 HP = 160×1.25^(T−1) (T8 760 / T9 950 / T10 1190), 방어 = 3T+2.
- 역할: 근접 ×1.0, 탱커 ×1.45(superArmor 1), 원거리·비행 ×0.7, 밤 정령 HP ×0.5·방어 ×0.4. 기존 ②(`RampartKnight_BD` 640/24 등)와 같은 비율임을 대조.
- 구역 보스 HP = 평균×30, 층계 보스 ×40, 보스 방어 ×1.5. 4기사 중간보스 = 평균×12(임시).
- 레벨·EXP·접촉 피해: ② 값(L18 334, L19 394, 접촉 24~31)에서 T당 레벨 +1, EXP ×1.18, 접촉 +3 외삽.
- 아이템 가격: ②(무기 600·방어구 1100/1300/1100/1100/900)에서 T당 +15%(T10 은 무기 950). 보스 고유 U 12000/15000, 레전더리 30000 은 임시. 레전더리 등급 키는 기존 C/R/U 뿐이라 `U` 로 대체.
- 세트 키: GardenArmor(Garden Warden)·MoonStoneArmor(Moonlit Stone)·OrderArmor(Order of Four)·DrakeArmor(Cloudsea Drake). 보너스 키 이름은 신규 스킬명 임시.
- 드롭: 재료 32%(②와 같은 30~35%), 계보 무기 2%, 보스 고유 8%(용 5%). 부위 0.3% 드롭·확정 보스 드롭은 `BossLoot` 쪽이라 이 초안에 없음.

## 부족한 정보 → `Docs/OpenQuestions.md` Q-F2D1~4 임시 가정
## 편입 방법(후속)
Unity 사용 가능한 세션에서 CSV 맨 뒤에 붙이고 `DataIntegrityTests` 실행. 프리팹·아이콘이 없으면 검증 리포트의 누락 목록에 나오므로 무기/방어구 프리팹 복제와 함께 진행.
