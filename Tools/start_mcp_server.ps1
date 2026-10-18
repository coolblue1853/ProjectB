# unity-mcp HTTP 서버를 숨김 창으로 실행 (포커스 넛지·텔레메트리 끔)
$env:UNITY_MCP_DISABLE_FOCUS_NUDGE = 'true'
$env:DISABLE_TELEMETRY = 'true'
$env:UNITY_MCP_DISABLE_TELEMETRY = 'true'
$listening = Get-NetTCPConnection -LocalPort 8080 -State Listen -ErrorAction SilentlyContinue
if ($listening) { "already running (pid $($listening.OwningProcess))"; exit 0 }
Start-Process -WindowStyle Hidden 'C:\Users\user\.local\bin\uvx.exe' -ArgumentList '--from','mcpforunityserver','mcp-for-unity','--transport','http','--http-url','http://127.0.0.1:8080'
"started"
