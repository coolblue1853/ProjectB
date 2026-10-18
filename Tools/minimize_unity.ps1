# Unity 에디터 창을 포커스를 주지 않고 최소화 (SW_SHOWMINNOACTIVE=7). 컴파일/리로드 팝업이 입력을 가로채지 않게 한다.
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class W2 { [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr h, int c); [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h); }
"@
$u = Get-Process -Name Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 }
foreach ($p in $u) { if (-not [W2]::IsIconic($p.MainWindowHandle)) { [W2]::ShowWindowAsync($p.MainWindowHandle, 7) | Out-Null; "minimized $($p.Id)" } else { "already minimized $($p.Id)" } }
