# Unity 포커스 가드: 사용자가 클릭/Alt+Tab 하지 않았는데 Unity 창(본창·팝업·진행바)이 맨 앞으로 오면
# 직전 창으로 포커스를 되돌리고 Unity를 비활성 최소화한다. 사용자가 직접 Unity를 클릭한 경우는 건드리지 않는다.
# 실행: powershell -WindowStyle Hidden -File Tools/focus_guard.ps1   (중복 실행 방지 뮤텍스)
# 끄기: Logs/focus_guard.off 파일 생성
Add-Type @"
using System; using System.Runtime.InteropServices; using System.Text;
public static class FG {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a, uint b, bool attach);
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vk);
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  public static string Title(IntPtr h) { var sb = new StringBuilder(256); GetWindowText(h, sb, 256); return sb.ToString(); }
}
"@
$created = $false
$mutex = New-Object System.Threading.Mutex($true, "Global\ProjectB_FocusGuard", [ref]$created)
if (-not $created) { "already running"; exit 0 }

$root = Split-Path -Parent $PSScriptRoot
$off = Join-Path $root "Logs\focus_guard.off"
$log = Join-Path $root "Logs\focus_guard.log"
# 사용자 의도 키: 마우스 좌/우/중, Alt, Win, Tab
$intentKeys = 0x01, 0x02, 0x04, 0x12, 0x5B, 0x5C, 0x09
$lastIntent = [DateTime]::MinValue
$prev = [IntPtr]::Zero
$prevIsUnity = $false

function Is-Unity([IntPtr]$h) {
  if ($h -eq [IntPtr]::Zero) { return $false }
  $procId = 0; [FG]::GetWindowThreadProcessId($h, [ref]$procId) | Out-Null
  try { return (Get-Process -Id $procId -ErrorAction Stop).ProcessName -eq "Unity" } catch { return $false }
}

while (-not (Test-Path $off)) {
  foreach ($k in $intentKeys) { if ([FG]::GetAsyncKeyState($k) -band 0x8001) { $lastIntent = Get-Date } }
  $fg = [FG]::GetForegroundWindow()
  if ($fg -ne $prev) {
    $isUnity = Is-Unity $fg
    $userDid = ((Get-Date) - $lastIntent).TotalMilliseconds -lt 700
    if ($isUnity -and -not $prevIsUnity -and -not $userDid -and $prev -ne [IntPtr]::Zero -and [FG]::IsWindow($prev)) {
      $tmp = 0
      $fgThread = [FG]::GetWindowThreadProcessId($fg, [ref]$tmp)
      $me = [FG]::GetCurrentThreadId()
      [FG]::AttachThreadInput($me, $fgThread, $true) | Out-Null
      [FG]::SetForegroundWindow($prev) | Out-Null
      [FG]::BringWindowToTop($prev) | Out-Null
      [FG]::AttachThreadInput($me, $fgThread, $false) | Out-Null
      $u = Get-Process -Name Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 }
      foreach ($p in $u) { [FG]::ShowWindowAsync($p.MainWindowHandle, 7) | Out-Null }
      Add-Content -Path $log -Encoding utf8 -Value ("{0:HH:mm:ss} 되돌림: [{1}] -> [{2}]" -f (Get-Date), [FG]::Title($fg), [FG]::Title($prev))
      Start-Sleep -Milliseconds 100
      $fg = [FG]::GetForegroundWindow()
      $isUnity = Is-Unity $fg
    }
    $prev = $fg
    $prevIsUnity = $isUnity
  }
  Start-Sleep -Milliseconds 40
}
