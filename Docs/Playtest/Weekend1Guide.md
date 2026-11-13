# 1층계 주말 플레이 테스트 가이드 (11-11 작성)

목표: 샛길→일반 숲→깊은 숲→늪→성채→탑→문지기까지 한 번 완주하며 막히는 곳·지루한 곳·이상한 곳을 모은다. 완벽 검증이 아니라 **다음 주 개선 재료 수집**이 목적이다.
이상하면 번호만 적어 두면 된다(예: "G-4 밤 대기 길다"). 아래 UserPlaytest_1111.md 의 미검증 목록도 같이 본다.

## 진행 순서와 볼 것
- G-1 새 게임: 튜토리얼 5단계(이동·공격·점프·물약·Q), 첫 퀘스트 루엔 대화, 첫 레벨업 간격(목표 2~12분)
- G-2 샛길·일반 숲: 쥐(밤 전용) 퀘스트에서 "밤까지 N분" 안내가 보이는가, 낮 대기가 지루하지 않은가
- G-3 마차(수레): 해금 조건 안내, 요금, 이동 후 위치
- G-4 깊은 숲·늪·성채·탑: 입구 안내판에서 **보고**가 되는가(수락은 구역 퀘스트 한정, 마을로 왕복하지 않아도 되는가)
- G-5 레벨 곡선: 구역 진입 시 레벨(권장 -1~+3), 사냥이 퀘스트 없이도 쓸 만한가
- G-6 골드: 물약비 vs 수입, 막히는 시점
- G-7 무기·세트: 구역별 새 무기 타입을 한 번씩 만나는가(탑 근교 포함), 세트 제작 재료 부족 여부
- G-8 정예·미니보스: 등장 위치·난이도·인장 드롭(늪·성채·탑 3곳)
- G-9 문지기: Lv15 권장, 첫 도전 TTK, 보너스 강화석 +2
- G-10 상점: 일괄 판매 버튼(J키·패드 LT+L3 와 같은 결과)

## 알려진 임시·미완 (고장 아님)
- 인장 3종·신규 무기 3종 아이콘 없음, 신규 맵 2장 타일·안개는 임시, 몹 6종 전용 스프라이트 없음
- 늪 Spirit Koi 드롭 3%, Q603 성채 건너뛰기 시 EXP 부족: 다음 주 보정 후보
- 낮 샛길 쥐 도둑·쥐 두목(정예)이 토끼 그림으로 나옴(그래픽 미작업, 스포너 프로필만 교체)
- 도감 B08·B10 수정 예정
- 탑 곁가지 EXP 감액 예정

## 치트·디버그 이동 (11-13 점검)
- 에디터·Development 빌드에서 SR Debugger 로 쓴다. 우하단에 `Cheat` 묶음이 고정 표시된다(Level +1·EXP +500·Gold +1000·Heal·Unstuck·God Mode·Day/Night/Day-Night Cycle·Fill Quests·Save). 전체 창은 화면 오른쪽 위 구석 세 번 탭 또는 Ctrl+Shift+F1, 옵션 탭에서 `Move`(Town·Side Road·Near Forest·Normal Forest·Deep Forest·Swamp·Bandit Fortress·Tower Outskirts)와 `Quest`(Item Key·Give Item·Give LeafAdae+FangDagger) 묶음. 구현 `Assets/Script/DebugTools/SROptions.ProjectB.cs`.
- 릴리스 차단: `Assets/Editor/ReleaseCheatGate.cs` 가 비 Development 빌드 직전 SR Debugger `Settings.IsEnabled` 를 끄고 빌드 뒤 복원한다(`ReleaseCheatGateTests` 로 복원 로직 검증). 실제 릴리스 빌드로 확인한 적은 없다(코드·테스트 확인만).
- 프로브(unity-mcp, 에디터 플레이 중): `bash Tools/u.sh cs Tools/probes/_f1_goto.cs`(요새로 이동, 맵 id 만 바꿔 쓴다), `Tools/probes/f1fr_gotown.cs`(마을). 구역 전체 순회는 `ZONE_MAPS="..." bash Tools/zone_soak.sh 8`(세이브 백업·복원).

## 알려진 콘솔 에러 (11-13 순회: 마을·샛길·일반 숲·깊은 숲·늪·성채·탑 근교, 낮·밤 각 8초, `Tools/zone_soak.sh`)
| 구역 | 낮 에러 | 밤 에러 | 비고 |
|---|---|---|---|
| 마을 | 0 | 0 | |
| 샛길·일반 숲 | 0 | 0 | 낮 경고 1건씩(아틀라스 제외 후) |
| 깊은 숲·늪·탑 근교 | 0 | 0 | 낮 경고 1건씩, 이동 직후 몹 수 0 은 스포너 지연(이상 없음 판정 anomalies=0) |
| 성채 | 0 | 0 | |
총 에러 0건. 경고만 낮 6곳에서 1건씩 있었고 내용은 수집하지 않았다(필요하면 `Logs/zone_soak/*_day.warn.json`).

## 기록 방법
발견 사항을 `Docs/Playtest/Weekend1_Findings.md` 에 번호·한 줄로 쌓는다(없으면 새로 만들기). 다음 주 큐의 원본이 된다.
