# 데이터 무결성 점검 (AUTO-2, 11-05)

`DataIntegrityTests` 8건 + 기존 `QuestsFloor1DataTests`(선행 순환·보상/목표 실존·EXP 합)로 현재 CSV 점검. **위반 0건.**

| 항목 | 결과 |
|------|------|
| Q604·Q704 선행 사슬이 Q101 로 이어지고 순환 없음 | 통과 |
| Q604 목표(문지기)가 MonsterSheet 에 실존 | 통과 |
| MonsterExtraDrops 몹·아이템 실존 | 통과 |
| ShopStock 아이템 실존 | 통과 |
| Floor1Bounties 목표 몹·보상 아이템 실존 | 통과 |
| Floor1Treasures 보상 아이템 실존 | 통과 |
| Equip 퀘스트 세트 키가 ItemSheet tfName 에 실존 | 통과 |

- 한계: 세트 키 "편입 누락"(ItemSheet setName 표시명 ↔ SetItem 키 접두 매핑)은 규칙이 문서화돼 있지 않아 검사하지 않음.
- AUTO-1 이 CSV 를 바꾸면 이 테스트를 다시 돌린다.

## 2층계 ON 가정 검증 (AUTO-23, 11-05)
`Floor2LinkTests` 4건(스위치 ON 병합 데이터 기준): 드롭 몹·아이템 실존, 신규 `tfName` 의 `_2/_4/_6` 세트 키 존재, 계보 무기가 드롭 표에 있고 4세트에 쿨감 스킬 키(`coolDown <Key>`) 포함, T8~T10 몹 HP·방어가 §3.2 범위(역할 배수 ×0.45~1.5, 보스 ×12~40, 보스 방어 ×1.5±1.5). **위반 0건.**
- 한계: 무기 ItemSheet 행에 스킬 키 열이 없어 "무기→몹 스킬 키" 는 세트 4단계 쿨감 키 존재로만 검사. 몹 스킬 키 실존은 프리팹(그래픽 단계) 이후.
