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
