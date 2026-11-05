# 11-05 C# 리뷰 (정적 검토, 코드 수정 없음)

- 범위: 11-05 커밋의 위험 지점. SLOT-R(신규 슬롯), 사망 페널티(LevelRules), 방패 막기 훅(PlayerGuard·ShieldGuard·CounterBuff, AUTO-15·17), MobDebuffRules·EnemyHealth 필드, FT-SEAL·BOSSROOM(ExplorerSeal·RecallCast), Floor2Data 스위치.
- 검증: 정적 읽기만. 플레이·EditMode 재실행 안 함. 미검증.
- 심각도 순, 최대 15건.

### [필수]

1. `Assets/Script/UI/ExtraEquip.cs:156-161, 177-186` — Load 중 폐기 장신구를 인벤으로 반환하고 Save()를 부르지 않는다. ES3 extra 키에는 옛 id가 남아, 다음 LoadInventory 때 같은 아이템이 또 반환된다(인벤 저장과 맞물리면 세션마다 증식). 대안: 반환 처리 뒤 해당 칸을 비우고 Save() 호출, 인벤 저장 순서 확인. 재현 미확인. **[해결 11-05: Load 중 폐기 반환 직후 칸을 비우고 저장(인벤 먼저), 재로드 재지급 없음. ExtraEquipLoadTests]**
2. `Assets/Script/UI/ExtraEquip.cs:167-172` — catch가 `_mask`와 Items를 전부 0/빈칸으로 만든다. 이후 Unlock·Equip이 Save()를 부르면 `equip.extraUnlock`이 0으로 덮여 해금이 영구 소실된다. 대안: 항목별 검사로 범위를 좁히고, 읽기 실패 시 Save를 막는 플래그를 둔다. **[해결 11-05: catch 가 mask 를 유지하고 Save 를 차단(SaveBlocked). ExtraEquipLoadTests]**

### [권장]

3. `Assets/Script/Player/PlayerHealthManager.cs:348-357, 383` — 막기 성공 후 dmgAfDef가 0이면 PARRY/GUARD와 BLOCK 텍스트가 겹쳐 뜬다. 대안: BLOCK 조건에 `!guarded`를 추가.
4. `Assets/Script/Enemy/EnemyHealth.cs:633, 709-719` — ApplyBurn이 호출마다 PosionAttack을 새로 Instantiate한다. 화상 중첩 상한이 없어 연타 시 틱 피해가 N배가 된다. 대안: 재적용 시 기존 틱을 갱신하거나 상한 설정. 설계 의도 확인 필요.
5. `Assets/Script/UI/ExtraEquip.cs:237-240` — 해금 첫 아이템 CreatItem 반환값을 무시한다. 가방이 가득이면 해금은 저장된 채 아이템만 사라진다. 대안: 실패 시 골드 지급 또는 보류 큐. **[해결 11-05: 해금 첫 아이템 지급 실패 시 판매값 골드(GrantFirstItem). ExtraEquipLoadTests]**
6. `Assets/Script/UI/ExtraEquip.cs:95-100` — 장착 교체 시 old 반환 CreatItem 결과를 무시한다. RemoveAt(1) 직후라 보통 성공하지만 보장은 없다. 대안: 반환 성공을 확인한 뒤 Items를 교체. **[해결 11-05: 교체 반환 실패 시 판매값 골드(ReturnOrRefund). ExtraEquipLoadTests]**
7. `Assets/Script/Item/RecallCast.cs:155-161` — Stop(null)이 Ended(true)를 먼저 발행한 뒤 RemoveOne이 실패해도 이동 없이 완료로 처리된다. 구독 UI가 성공으로 표시할 수 있다. 대안: RemoveOne 성공을 확인한 뒤 완료 처리.
8. `Assets/Script/Enemy/MobDebuffRules.cs:64` — 석화 면역 타이머를 Snare 호출 전에 기록한다. Snare가 보스·behaviorTree null로 거절해도 면역이 소모된다. 대안: 실제 적용 성공 뒤에만 면역을 기록.

### [참고]

9. `Assets/Script/Weapon/Gimmick/ShieldGuard.cs:39-41` — InputAction을 만들지만 Dispose하는 곳이 없다(OnDestroy 없음). 대안: OnDestroy에서 Dispose.
10. `Assets/Script/Weapon/Gimmick/PlayerGuard.cs:16, 69` — LastParried가 정적 상태라 stale 값이 남을 수 있다. 현재는 guarded 확인 뒤에만 읽어 안전. 대안: Intercept의 반환 구조체로 전달.
11. `Assets/Script/Item/RecallCast.cs:16-17` — 취소 조건 summary가 BossBlockRange 위에 붙어 있다. 대안: 주석 위치 정리.
12. `Assets/Script/DB/Floor2Data.cs:33-41` — 스위치 ON 시 base는 CRLF, 덧붙인 행은 LF로 섞인다. 열 순서(MonsterSheet 대 Floor2Monsters)도 켜기 전에 확인 필요. 프로덕션 코드에서 Enabled=true 호출은 없고 테스트만 켠다. 현재 무영향.
13. `Assets/Script/Inventory/Model/ExtraEquipStats.cs:46-50` — 첫 Resources.Load가 null이면 빈 테이블을 영구 캐시한다. 대안: 빈 결과는 캐시하지 않는다.
14. `Assets/Script/UI/ExtraEquip.cs:79` — 폐기 아이템(equipArea "Title" 등)은 TryParse 실패로 TryHandleEquip이 false를 반환해 기본 장착 경로로 넘어간다. EquipmentModel.cs:71은 로드 경로만 막는다. 장착 경로 동작 확인 필요.
15. `Assets/Script/Combat/ExplorerSeal.cs:24-28` — Count는 AfterSceneLoad에서만 ES3로부터 읽는다. 세이브 초기화 기능이 생기면 Count 리셋을 함께 넣어야 한다. 현재 DeleteKey 호출은 없다.

### 잘된 점

- SLOT-R1: EquipSlot 10~13 값·ES3 키·비트를 그대로 유지했다. 세이브 호환이 깨지지 않는다.
- 신규 ES3 키(`explorer.seals`, `npcq.folded`)는 추가 전용이다. 기존 키 변경·삭제는 찾지 못했다.
- 사망 페널티 변경(ee83ac4b)은 상수 제거와 테스트 갱신이 같이 들어갔고, 호출 인자는 무시하되 호환을 유지했다.
- 가드 판정을 GuardRules·GuardState 순수 클래스로 분리해 EditMode로 검증할 수 있다.
- Floor2Data 스위치는 기본 OFF이고 Merge가 OFF면 원문을 그대로 돌려준다.
- EnemyHealth는 필드 초기화에 기대지 않는 lazy 생성을 쓰고 이유를 주석으로 남겼다.
