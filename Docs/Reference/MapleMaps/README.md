# 메이플 맵 참고 자료 (MapleMaps)

**용도: 우리 맵·몹·장비 설계를 개선할 재료다. 그대로 넣는 게 아니다. 구조·수치만 참고하고 그림·지형 모양은 복제하지 않는다.**

## 출처
- `maplestory.io` 공개 API, GMS v83(클래식 빅뱅 전 데이터). 메랜닷컴(itshim.dev)이 쓰는 같은 소스로 확인했다. 한글 이름은 같은 API 의 `KMS/284` 를 id 로 매칭(KMS 는 최신판이라 v83 당시 이름과 다를 수 있다).
- 목록 4,411개 중 상세 3,878개 저장. 533개는 404(데이터 없음, 대부분 이벤트·삭제 맵). 수집 시점 11-14.
- 원본 JSON 은 `Tools/cache/maple_maps/`(커밋 안 함). 여기 문서는 요약 결과만.

## 파일
| 파일 | 내용 |
|---|---|
| `maps_summary.csv` | 한 줄 = 한 맵(전 지역 3,878). UTF-8 BOM. 열은 아래 |
| `by_region/<지역>.md` | 지역별 표(마을·사냥터)와 레벨대별 대표 지형 서술(층·로프 좌표 패턴) |
| `levels_1_70.md` | Lv1~70 사냥터 레벨순 + 구간별 통계(폭·층·로프·스폰·밀도 중앙값·범위) |
| `npcs_list.csv` | NPC id·영문·한글 이름(1,733) |
| `items_list.csv` | 아이템 id·이름·분류(12,578). 상세 없음 |

몹은 `Docs/Reference/MapleMobs/`, 퀘스트는 `Docs/Reference/MapleQuests/`.

## maps_summary.csv 열
- `kind`: `town`(isTown) / `event`(제외 street: Mu Lung Dojo·Goldrich's Maze·Haunted House·Monster Carnival·Party Quest·Adobis·Mini Dungeon·탑승 중·Premium/Unique Road·Warning Street·Hidden Chamber·Cave of Life 등) / `hunt`(몹 스폰 1개 이상) / `indoor`(몹 없음·폭<1500·층≤2: 상점·집·퀘스트방) / `other`.
- `is_town`: 1 이면 마을. `region`: streetName(공백 정리). `name`·`name_ko`.
- `width`·`height`: px. vrBounds 가 비면 발판(footholds) 좌표 범위. (대부분 발판 범위)
- `floors`: 수평 발판 y 를 40px 근접값으로 묶고 길이 150px 이상인 묶음 수(경사 많은 맵은 크게 나온다). `platforms`: 연결된 발판 덩어리 수(100px 이상).
- `fh_len`: 비수직 발판 총 길이. `ropes`·`ladders`: 개수, `lr_avg`: 평균 길이.
- `portals`: 스폰 지점(sp)을 뺀 포털 수. API 가 대상 맵을 못 풀어 null 로 준 포털도 출구로 센다. `exits`: 다른 맵으로 가는 포털(null 포함), `self_portals`: 같은 맵 안 이동, `hidden`: type 1(비가시) 포털 수(추정), `script`: toMap 없는 스크립트형(추정). 일반/히든 구분은 이 API 로 정확히 못 한다.
- `spawns`: 몹 스폰 지점 수, `species`: 몹 종 수, `lv_min`·`lv_max`: 몹 레벨, `mob_rate`: 리젠 배율(1=기본).
- `dens`: 발판 1000px 당 스폰 지점 수. `exp_sum`·`hp_sum`: 모든 스폰 몹이 한 번씩 나올 때 EXP·HP 합.
- `mobs`: `이름(Lv,x스폰수)`. `mobs_ko`: 한글 이름. `return`: 귀환 맵 id. `bgm`.

## 갱신 방법
```
python -I Tools/maple_maps_dump.py fetch      # 목록·맵·퀘스트·몹 받기(이어받기, 0.3초 간격, 약 40분)
python -I Tools/maple_maps_dump.py fetchko    # 퀘스트 한글 본문(KMS)
python -I Tools/maple_maps_dump.py build      # 요약 생성(Docs/Reference/*)
```
`Tools/maple_ref_build.py` 가 몹·퀘스트·NPC·아이템 요약을 만든다. 캐시가 있으면 그 id 는 건너뛴다. 실패 목록은 `Tools/cache/maple_maps/_fails.json`.

## 주의
- 수치는 v83 기준이라 우리 게임에 그대로 맞지 않는다(해상도·점프 높이·속도 다름). 대응 비율로 쓴다.
- `floors`·`hidden` 은 규칙 기반 추정이다.
