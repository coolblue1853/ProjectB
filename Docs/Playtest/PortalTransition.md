# 포털 전환 점검 (Q-PT01, Floor1Completion 14-6)

## 조사 결과 (수정 전)
| 경로 | 입력 잠금 | 암전(아웃) | 카메라 스냅 |
|---|---|---|---|
| Portal.Use (포털 ↑) | 없음(isUsePortal 플래그뿐) | O | CenterOnTargets |
| Portal.TravelNow (사망 부활) | 없음 | 없음(이동 후 검정에서 밝아짐) | O |
| MapInfo.GoToMap/GoToTown (웨이포인트·귀환 주문서·메뉴 마을 귀환) | 없음 | 없음 | O |
| FallRescue | 없음 | O | O |
| Teleport.cs | 스킬 블링크(맵 이동 아님) — 대상 아님 | - | - |

추가 발견: CenterOnTargets 가 경계 밖에 카메라를 놓으면 ProCamera2D 소프트 경계가 도착 후 몇 프레임 끌어당겨 화면이 미끄러짐(좌측 끝 도착 맵, 예: 심부·안개 늪, 약 6칸).

## 수정
- `MapTransit.Run`(신규 공통 함수) = ScreenFader.Transition(입력 잠금 → 암전 0.18초 → 이동 → 속도 0 → 카메라 스냅 + 경계 안으로 즉시 클램프 → 밝아짐 → 잠금 해제).
- `ScreenFader.Transition` 이 전체 구간 InputLock 토큰을 쥔다(끊겨도 Kill 이 풀어 줌).
- Portal.Use·TravelNow, MapInfo.GoToMap, FallRescue 모두 MapTransit.Run 사용. 사망 부활은 HP 회복·상태 해제를 도착 콜백(암전 한가운데)에서 실행.
- `CameraClampRules`(순수) + EditMode 테스트 5개. 전체 EditMode 610/610 통과.

## 검증 (플레이 모드, 낮 고정, 에디터 최소화라 시간이 느림)
- 포털: 숲→심부 입구(Portal_ToCore), 심부→숲(Portal_CoreToDeep), 늪→안개 늪(Portal_ToMist), 안개 늪→늪(Portal_MistToSwamp): 전환 내내 lock=True, 도착 시 카메라 x/y 이동 없음(도착 직후 프레임~밝아짐 후까지 동일 좌표). 수정 전 심부 도착은 카메라 -417 → -411.1 로 미끄러졌음.
- GoToMap(안개 늪·마을·심부 심부): 즉시 lock=True, 암전 중 이동, 카메라 고정.
- 사망→마을 부활: 부활 대기 중 lock, 암전 후 마을 도착, lock 해제.
- 캡처(Logs/playtest/pt_mist_in2_*.png): 검정 2장 → 도착 화면 3장, 위치 변화 없음. 콘솔 에러 0.
- 미검증: FallRescue 실제 낙사 재현(코드 경로만 동일 함수로 교체), 귀환 주문서·메뉴 마을 귀환 실제 호출(GoToTown 경로는 웨이포인트와 동일 함수).
