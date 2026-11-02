# 코드 스타일 · 커밋 규칙

## 1. C#

`.claude/rules/csharp.md`(경로 한정, .cs 작업 때만 로드)로 이동했다. C# 수정 전 로드되지 않았으면 직접 Read.

## 2. Git

- 브랜치는 `main` 단일이고, 기능 단위로 커밋해 push한다.
- 메시지 형식: `(추가|수정|삭제|문서|리팩터) 한 줄 요약` + 빈 줄 + 본문 bullet.
  - 본문에 **검증 방법**을 적는다(예: "검증: playtest 프로브로 처치→드롭 확인"). 확인하지 못했으면 "미검증"이라고 적는다.
  - 마지막 줄은 `Co-Authored-By: Claude ...`이다.
- 커밋하지 않는 것: `Library/`, `Logs/`, `Assets/Screenshots/`, `.claude/settings*.json`, 데모 씬 대용량 에셋.

## 3. 데이터

- 기획 데이터는 `Assets/Resources/*.csv`에 둔다. 컬럼을 추가할 때는 **맨 뒤에** 추가하고 `Game/ItemSpec.md` 표를 갱신한다.
- 세이브(ES3) 필드는 이름을 바꾸지 않는다. 추가만 한다(기존 세이브 호환).
