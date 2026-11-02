---
name: heredoc-backslash
description: "Bash heredocs in this environment unescape one level of backslashes; use chr(92) when writing C# escapes via Python"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 7f303872-6cc7-4749-abaf-5659414b9850
  modified: 2026-10-18T10:37:14.777Z
---

The Bash tool here collapses `\\` to `\` inside quoted heredocs. Writing C# string escapes like `"\r"` or `'\n'` from a Python heredoc therefore inserts a literal CR/LF into the source and Unity reports "Newline in constant".

**Why:** Cost three compile round-trips on 2026-10-18 before the cause was found.

**How to apply:** Build backslashes with `chr(92)` (or write the file with the Write tool instead of a heredoc). Verify with `cat -A` when a C# string literal looks wrong. Related: [[no-focus-steal]]

2026-10-19 추가 함정: 파이썬으로 C# 파일을 다시 쓸 때 `open(..., newline='\r\n')`과 `s.replace('\n','\r\n')`를 **같이 쓰면 CR CR LF**(줄 두 배/남은 CR)가 된다. 둘 중 하나만: 바이트로 읽고 `open(p,'wb').write(('\ufeff'+text_with_crlf).encode('utf-8'))`. 쓰고 나면 `b'\r\r\n'` 개수와 HEAD 대비 줄 수를 확인한다.
