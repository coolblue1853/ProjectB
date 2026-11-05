# 층계 전환 규칙 (11-05, AUTO-18 — 문서·grep 대조, 게임 내 미검증)

근거: `Floor2Plan.md` §1·§3.3·§4, `FloorCoreContent.md`, `WorldSpec.md` §2·§3, 코드 `Assets/Script/Map/{TowerGatePortal,PortalSeal,MapInfo,ArrivalSafeZone}.cs`, `Combat/AscentLoad.cs`, `Item/BossReward.cs`, `Item/RecallCast.cs`. 구현 여부는 grep 기준이며 구현 전 재확인.

## 1. 이동 조건 (올라가기 / 내려가기)

- **내려가기·귀환은 자유.** 각 구역 왼쪽 끝 포털 = 이전 구역(`Floor2Plan.md` §1). 잠금 없음.
- **올라가기는 보스 첫 처치로 연다.** 봉인은 ES3 `bossFirstKill.<보스ID>`(`BossLoot.HasFirstKill`)만 본다. 새 세이브 키를 만들지 않는다.

| 전환 | 열리는 조건 | 구현 |
|---|---|---|
| 샛길 → 2층계 입구 | 외곽 샛길 보스 첫 처치(Q-414) | ✅ `PortalSeal` |
| 1층계 ⑥ 탑 근교 → 2층계 | 탑의 문지기(`Gatekeeper_BD`) 첫 처치 | ✅ `TowerGatePortal`(맵 켜질 때 포털 on/off + 처치 시 '탑 입구 개방' 배너) |
| 2층계 ①~④ 구역 간 | 앞 구역 보스 처치 필요 여부 **미정**(현재 포털은 자유 통행) | 임시 가정: 자유 통행(§5 #1) |
| 2층계 ⑤ → 3층계 | 운해의 용 첫 처치 → `TowerGatePortal` 재사용 | 미구현(용·3층계 맵 없음) |

## 2. 탐험가 칭호와 상승 부하

- 칭호 `AscentLoad.Rank`(0~7, ES3 `explorer.rank`, 초기 0). 저항 = 15 × 칭호. 부족분 = max(0, 맵 부하 − 저항 − 음식 완화), 부족분만큼 주는 피해가 깎인다(§3.3: 10~45 → −20~−90%).
- 승급 경로: 보스 `BossReward.grantsRank` → `AscentLoad.GrantRank`(내려가지 않음, 같거나 낮으면 무시) → RANK UP 배너 + 픽업 로그.
- 1→2층계 연속: 1층계 ⑥ 부하 40, 칭호 2(문지기 처치, 저항 30) → 부족분 10. 2층계 부하 ① 25 · ② 35 · ③ 45 · ④ 50 · ⑤ 60 → 부족분 0 · 5 · 15 · 20 · 30.
- 칭호 1 서광 = 엔트 처치(✅), 2 운해 = 문지기 처치(✅), 3 심록 = 운해의 용 첫 처치(**임시로 Abyss Wizard 가 줌**, 용 구현 시 이전).
- 완화 수단(미구현, §3.3): 탐사 인장 +3 저항(보스 첫 처치 ②③④, ES3 `explorer.seals`), 2층계 6세트 `AscentRes` +5.
- 맵 부하는 맵 진입 시 `AscentLoad.SetLoad(load)` 로 설정(`MapInfo` 의 맵별 값).

## 3. 포털·마을 귀환

- 포털: ↑ 키. `ScreenFader.Transition` 암전(0.18/0.12/0.3초) 속에서 대상 맵 켜기 → 이전 맵 끄기 → 스포너 정리 → 카메라 리센터. 이동 1초 뒤 `SaveManager.SavePos()`(`WorldSpec.md` §2).
- `openMap` 이 비면 같은 맵 안 위치 이동.
- 마을 귀환 3경로 모두 `MapInfo.GoToTown`: ① 귀환 주문서(`RecallCast`, 시전 중 피격·이동·사망 시 취소, 완료 때만 1개 소비) ② 사망 부활(`PlayerHealthManager`) ③ 일시정지 메뉴(`PauseMenu`). 부활 지점 = 마을 고정(층계별 거점 없음 — §5 #2).
- 층계 이동은 칭호를 잃지 않는다. 내려가도 부하만 낮아진다.

## 4. 안전지대

- **도착 안전지대:** `ArrivalSafeZone`(가로 반경 14·세로 9). 포털이 켜질 때 목적지 좌표를 등록, `EnemySpawner.PickSafeIndex` 가 반경 안 스폰을 거른다(보스 포함). 2층계 포털에도 같은 컴포넌트 경로를 쓴다.
- 마을은 몹 없음. 구역 배너는 같은 구역 5분 쿨다운(`ZoneBanner`).
- 보스방 입장 직후 보호(무적 N초)·퇴로 규칙은 **없음** — 보스 도중 포털 이용 가능 여부도 미정(§5 #4).

## 5. 구현 갭 → 큐 후보 ID (코드 필요, 아직 큐에 올리지 않음)

| ID | 갭 | 크기 | 비고 |
|---|---|---|---|
| FT-GATE-2F | 2층계 구역 간 보스 처치 봉인(②→③→④→⑤)을 둘지 결정 후 `PortalSeal` 적용 | 소 | **보류(11-05)**: 임시 가정 = 자유 통행 유지. 인장·상승 부하가 진행을 막으므로 별도 봉인 불필요. 2층계 맵·보스가 생기면 `PortalSeal` 한 줄 추가 |
| FT-SEAL | ✅ 탐사 인장 `explorer.seals` + `AscentRes` 효과 키(§3.3) | 중 | 11-05 구현: `ExplorerSeal`(+3씩·상한 12, ES3 추가만), `BossReward.grantsSeal`(첫 처치), `AscentLoad.GearResist`/`SetGearResist`(세트 효과 연결은 2층계 세트 구현 때). 2층계 보스 프리팹에 `grantsSeal` 켜기는 보스 구현 때 |
| FT-GATE-3F | ⑤ 용 처치 → 3층계 `TowerGatePortal` + 칭호 3 이전(Abyss Wizard 임시 보상 제거) | 중 | 용·3층계 맵 필요 → 맵 이후 |
| FT-RESPAWN | 층계별 거점(마을 외 부활·귀환 지점)이 필요한지 판단, 필요 시 거점 포털 | 중 | **보류(11-05)**: 마을 고정 유지. 2층계 5구역 길이 실측 후 재판단 |
| FT-BOSSROOM | ✅(부분) 보스방 입장 보호·보스 도중 귀환 주문서 제한 규칙 | 소 | 11-05: 살아 있는 보스 30유닛 안이면 `RecallCast.Begin` 거부(`RecallRules.BlockedByBoss`). 입장 보호는 기존 포털 도착 무적 2초가 대신(PV5). 보스 도중 포털 이용은 허용 유지. 게임 내 보스 앞 확인은 미검증 |

## 6. 세이브 필드 (추가만, 이름 변경 금지)

기존: `explorer.rank` · `bossFirstKill.<보스>` · `bossSinceUnique.<보스>`(천장 카운터) · `glide.unlocked` · `forge.unlock` · `forge.pity` · 현재 위치(`SavePos`). `explorer.seals`(FT-SEAL, 인장 개수 int, 구현됨)이 추가됐다. 층계 전환은 새 키 없이 `bossFirstKill` 로 판정한다.
