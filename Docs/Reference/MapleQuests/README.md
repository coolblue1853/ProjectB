# 메이플 퀘스트 참고 자료 (MapleQuests)

**용도: 우리 퀘스트의 분위기·말투·구성을 개선할 재료다. 보상은 우리가 더 강하게 줄 것이므로 곡선 모양만 본다. 원문 문장은 이 저장소에 넣지 않고(캐시에만) 우리 문장은 새로 쓴다(`Game/QuestDesign.md`).**

- 출처: `maplestory.io` GMS v83 `/quest/<id>`(목록 2,817 중 2,766 저장, 51개는 서버 500 오류·WZ 캐스팅 실패) + KMS 한글 본문(말투 통계용). 한글 본문은 낮은 id 부터 1,551개만 받았다(서버 응답이 느려 중단, 나머지는 `fetchko` 재실행으로 이어받기). 말투 통계는 받은 범위 기준이다.
- `quests_summary.csv`(UTF-8 BOM): id·이름·한글 이름·지역(시작 NPC 가 서 있는 맵의 street, 마을 가중)·area 코드·min/max_level·시작/완료 NPC·type(처치·수집·처치+수집·전달·대화)·goal(몹/아이템 이름 x수량)·reward_exp/meso/items·prereq/next(퀘스트 id)·stages(로그 단계 수)·repeat·auto_start.
- `chains.md`: 연계 사슬(3단 이상, 지역별). 루트당 최대 4줄.
- `by_level.md`: 레벨대별 수·유형 비율·보상 EXP 중앙값 + 지역별 유형.
- `tone_by_region.md`: 지역별 말투(존댓말 비율·문장 길이)·소재 키워드·분위기 단어·대표 퀘스트 구조 요약(원문 인용 없음).
- 유형 판정: 몹 목표 있으면 처치, 아이템 요구 있으면 수집, 둘 다면 복합, 완료 NPC 가 다르면 전달, 같으면 대화.
- 갱신: `Docs/Reference/MapleMaps/README.md` 의 명령.
