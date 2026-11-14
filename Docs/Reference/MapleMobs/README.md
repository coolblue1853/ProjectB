# 메이플 몹 참고 자료 (MapleMobs)

**용도: 우리 몹 HP·EXP·방어 곡선을 잡는 밸런스 기준 재료다. 그대로 복제하지 않는다.**

- 출처: `maplestory.io` GMS v83 `/mob`(목록 1,597) + `/mob/<id>`(상세). 한글 이름은 `KMS/284` id 매칭. 상세 1,564개 저장, 33개 404.
- `mobs_summary.csv`(UTF-8 BOM): id·이름·한글·level·boss·hp·mp·exp·patk(물리 공격)·pdef·matk·mdef·acc·eva·speed·undead·body_attack(접촉 피해)·map_count·regions(출현 지역 상위 4)·maps(출현 맵 id 최대 25개, 맵은 `MapleMaps/maps_summary.csv` 의 id 로 역참조).
- `mobs_by_level.md`: 레벨 10 단위 HP·EXP·공방·회피 통계, 보스 표, Lv1~40 몹 목록.
- 갱신: `Docs/Reference/MapleMaps/README.md` 의 명령.
