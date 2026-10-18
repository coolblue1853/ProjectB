# Unity 에디터 창을 잠깐 앞으로 가져와 에셋 자동 새로고침(컴파일)을 일으킨 뒤, 원래 창으로 포커스를 돌려준다.
# 사용: powershell -File Tools/focus_unity.ps1 [유지초=4]
param([int]$seconds = 4)
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class W {
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
    [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
}
"@
$prev = [W]::GetForegroundWindow()
$u = Get-Process -Name Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $u) { "unity not running"; exit 1 }
# Alt 키 트릭: 백그라운드 프로세스의 SetForegroundWindow 제한 우회
[W]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[W]::SetForegroundWindow($u.MainWindowHandle) | Out-Null
[W]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Seconds $seconds
if ($prev -ne [IntPtr]::Zero) {
    [W]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
    [W]::SetForegroundWindow($prev) | Out-Null
    [W]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
}
"focused unity for $seconds s"
