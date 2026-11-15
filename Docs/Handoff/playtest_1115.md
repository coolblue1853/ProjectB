# 11-15 사용자 플레이 기록 결과와 할 일

기록: `Logs/playrec/20261115_131248`(17분 42초, Lv1→5, 끝 1~2분은 builder 재컴파일로 오염 가능) + `20261115_133116`(21분 22초, Lv5→8). 둘을 한 판으로 이어서 본다. 요약 `*_summary.md`, 장면 `*_frames/`.
사용자가 플레이 중이면 에디터를 쓰는 작업은 하지 않는다(메모리 no-editor-while-user-plays).

## 기록 수치 (봇·밸런스 기준값)
- 처치 9.8/분 → 18.8/분, 받은 피해 133회/2182 → 167회/1743, 사망 2 → 0.
- 골드 0→807(17분) → 807→20537(21분). 줍기 231 → 334. 판매 38회(대부분 옛 장비).
- 평타 간격 중앙 0.21s, 반응(적 6칸 진입→첫 공격) 중앙 0.71s, 공격 예고→첫 반응 중앙 0.15s(주 반응 = 그냥 때림), 예고 피격률 약 50%.
- 사람 신호: 수량 팝업 바로 닫기 39회, NpcMenu 바로 닫기 21회, UI 위 키 연타 78회.

## 순서 (위에서부터)
1. [x] **스태미너(429de8fcc):** 달리다 대화·창 열면 스태미너가 계속 준다. builder 수정본이 커밋 안 된 채 작업 트리에 있다(`PlayerController.cs` +17줄, `Player/State/RunDrainRules.cs`). 이것을 검증하고 커밋한다(EditMode 테스트 추가, 플레이로 확인).
2. [x] **기록에서 나온 오류(36ce68c37):** 독·출혈 비활성 가드, Missing script 3프리팹 제거, 시체 풀 경고 제거, ↕→↑↓. 10.7초 끊김은 기록 t=0 진입 프레임(게임 결함 아님), GC 40ms·초당 9MB 할당은 TechDebt.
   - `PoisonDamage(Clone)`·`BleedingDamage(Clone)` 비활성 상태 코루틴 시작 Error 105회(t=117~).
   - `PhantomScytheSpiningSlash` Missing script.
   - `GreenHurb: 'dead'` 풀 미등록.
   - ThaleahFat_TMP 에 `↕`(U+2195) 글리프 없음.
   - 최악 프레임 10.7초 끊김과 할당 급증(t≈1136, 힙 1313MB). 원인을 확인한다.
3. [x] **막힘 지점(판정: 지형 정상):** 4장 모두 근접 전투 중 MoveLock(fl=ML)으로 속도 0, 벽·콜라이더 문제 아님. 기록기 stuck 판정 오탐(TechDebt). Lv1_NearForest_SideRoad (-320.7, 1.3) 오른쪽, Lv1_DeepForest (-218~-223, -0.7) 좌우. 프레임 PNG 를 보고 지형·벽 문제인지 판정한다.
4. [x] **옛 장비 정리(사용자, 해시 a5200d7a0, `Docs/Playtest/LegacyGearAudit.md`):** 신발 16종 retired·획득 경로 제거, 세트 장신구 48개 이름 이행(귀걸이·목걸이·반지), 판매 기록의 쥐 두목 계보 장비는 현행 유지. "여전히 장착되는 예전 아이템들이 남아 있다."
   - 판매 기록에 PoisonSword, RatCap, Ring Finger that Tears Wounds, Necklace of Spreading Corruption, Necklace of the Sly Plunderer 가 보인다.
   - 이것들이 폐기 슬롯·옛 체계 아이템인지 대조해 드롭표·상점·제작에서 빼고, 남은 것은 새 슬롯 체계로 이행한다.
   - 참고: `Game/EquipSlotExpansion.md`, `Docs/Handoff/equip_redistribution.md`.
5. [x] **드롭률(a5bec8654·290edbc27·5ba65eb6d·fa3bbfabd, 정예 장비 40%·PhantomScythe 1%·Ghost count×0.6·StoneRabbit EXP14):** 21분에 골드 약 2만, 줍기 334. 메이플 초반 기준(`Docs/Reference/MapleMobs`, `Game/LevelDesignRef.md`)과 비교해 드롭률·골드량을 낮추는 안을 낸다. designer 가 표를 만들고 builder 가 CSV 에 반영한다.
6. [x] **몹 밸런스(36aa02c9d 밤 배율 코드 HP1.4·공격1.3·EXP1.15, a44dc0795 기록기 R2.1):** "아직 안 잡혔다, **밤 몹은 더 세야 한다**." 밤 전용 몹(또는 밤 보정)의 HP·공격·EXP 배율을 정한다. 낮 대비 HP·공격 ×1.3~1.5, EXP·드롭 보상 소폭 상향을 임시 가정으로 `Docs/OpenQuestions.md` 에 적는다. 기록의 맵별 처치 속도와 받은 피해로 1층계 몹별 이상치를 뽑는다.
5·6 공통. **기록 기반 밸런스 조사 도구(사용자 11-15):** `Tools/playrec_balance.py` 를 만든다. 입력은 jsonl 여러 개, 출력은 `Docs/Balance/PlayBalance_<날짜>.md` 이다.
   - **몹별:** 처치 수, 평균 처치 시간(첫 타 → 처치), 받은 피해·플레이어를 때린 비율, EXP/처치, 드롭/처치(골드·아이템 등급별).
   - **맵·시간대별:** 처치/분, EXP/분, 골드/분, 받은 피해/분, 낮/밤 구분. 낮/밤 구분은 기록에 시각 필드가 없으면 R2 에 추가한다.
   - **아이템:** 주운 것·판 것·장착한 것의 빈도. 획득 즉시 판 장비 = 쓸모없는 드롭 후보로 본다.
   - **기준선 대비:** 메이플 초반 기준선(`Docs/Reference/MapleMobs`, `Game/LevelDesignRef.md`)과 비교해 튀는 항목을 표시하고, 수정안 표를 낸다. 수정안은 CSV 컬럼과 값까지 쓴다.
   - **이후 쓰임:** 봇 기록에도 같은 도구를 쓴다. 사람 1회 → 봇 반복 실행으로 표본을 늘리고, 밸런스 수정 → 봇 재실행 → 지표 비교의 자동 반복이 목표다.
7. [ ] **봇:** `PlayBotPlan.md` R3(파이썬 분석 도구, 이전 시도는 중단·미커밋) → B0 입력 재생 → B1~B4. 이 두 기록을 학습 자료로 쓴다.
