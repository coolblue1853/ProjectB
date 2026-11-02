---
paths:
  - "**/*.cs"
---

# C# 코드 규칙 (.cs 파일을 다룰 때만 로드)

- **인코딩·서식:** UTF-8(BOM), CRLF, 들여쓰기 4칸(`.editorconfig`).
- **필드:** 새 코드는 `[SerializeField] private T _camelCase;`로 쓴다. 기존 public 필드는 씬·프리팹 직렬화가 걸려 있으므로 이름을 바꿀 때 반드시 `[FormerlySerializedAs("old")]`를 붙인다.
- **스타일:** if/for/while은 항상 중괄호를 쓴다. 접근 제한자를 명시한다.
- **크기:** 클래스는 500줄, 메서드는 40줄 안팎을 목표로 한다. 기존 거대 클래스(InventoryManager 2000줄)는 기능을 고칠 때 조금씩 분리한다.
- **퍼센트 스탯:** 정수 스탯을 % 계산에 쓸 때는 반드시 `/ 100f`를 쓴다. 정수 나눗셈 금지(B-101).
- **입력:** New Input System만 쓴다. 레거시 `Input.GetKey*`는 금지(디버그 키 포함). `Keyboard.current`는 null 체크한다.
- **Update:** 매 프레임 `GetComponent` / `Find*` / `LayerMask.GetMask` / 문자열 연결 / `Debug.Log`를 하지 않는다. 캐시한다.
- **DOTween:** 트윈을 다시 걸기 전에 기존 트윈을 `Kill()`한다. `SetLink(gameObject)` 또는 `OnDisable`에서 정리한다.
- **ObjectPool:** 반환은 한 곳(`Release` 래퍼)에서만 한다. 이중 반환을 방지한다. `CountInactive==0`이라고 Destroy하지 않는다.
- **이벤트 구독:** `OnEnable`에서 구독하면 `OnDisable`에서 해제한다(풀 재사용 시 중복 구독 방지).
- **서드파티·자동 생성 코드는 수정하지 않는다.** 꼭 필요하면 TechDebt에 기록하고 최소한으로 고친다.
