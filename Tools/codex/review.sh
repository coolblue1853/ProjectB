#!/usr/bin/env bash
# Codex 작업 결과를 다른 Codex 실행이 리뷰·품질 확인: bash Tools/codex/review.sh <작업이름>
# 앞 작업의 보고(Logs/codex/<이름>.last.md)와 아직 커밋 안 된 변경을 리뷰하고 [필수]만 고친다.
cd "$(dirname "$0")/../.."
n="$1"; b="Logs/codex/$n.review.md"
{
  echo "작업: 방금 다른 실행이 끝낸 작업 '$n' 의 리뷰·품질 확인. 너는 그 작업을 하지 않은 리뷰어다."
  echo "앞 작업 보고:"; echo '```'; cat "Logs/codex/$n.last.md"; echo '```'
  echo "1. git diff(커밋 안 된 변경)에서 이 작업 파일만 본다. 정확성·null 안전성·CodeStyle(Update 할당·구독 해제·DOTween Kill·풀 이중 반환·세이브 키 호환)·설계 문서와 불일치를 찾는다."
  echo "2. 보고의 '미검증' 중 확인 가능한 것은 직접 확인한다(프로브·스크린샷, 낮 시간대)."
  echo "3. 실제 버그·규약 위반·문서 불일치인 [필수]만 고친다. 취향은 보고에만."
  echo "4. 게임 품질 관점(클래식 메이플 감, 초반은 소박하게, 유아틱 금지)에서 어색한 점을 적는다."
  echo "5. AGENTS.md 검증 절차 준수. 커밋 금지."
  echo "보고: 발견(파일:줄·심각도·고쳤는지), 검증 숫자, 남은 미검증, 커밋할 파일 목록."
} > "$b"
bash Tools/codex/run.sh "$n.review" "$b"
