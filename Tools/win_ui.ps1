# Windows UI 도우미: 화면 캡처 / Unity 대화상자 나열 / 버튼 클릭
# 사용: powershell -File Tools/win_ui.ps1 shot <out.png>
#       powershell -File Tools/win_ui.ps1 dialogs
#       powershell -File Tools/win_ui.ps1 click "<창 제목 일부>" "<버튼 이름>"
param([string]$cmd = "dialogs", [string]$a1 = "", [string]$a2 = "")
Add-Type -AssemblyName System.Windows.Forms, System.Drawing, UIAutomationClient, UIAutomationTypes

function Get-UnityWindows {
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pids = (Get-Process -Name Unity, "Unity Hub" -ErrorAction SilentlyContinue).Id
    $cond = [System.Windows.Automation.Condition]::TrueCondition
    $root.FindAll([System.Windows.Automation.TreeScope]::Children, $cond) |
        Where-Object { $pids -contains $_.Current.ProcessId }
}

switch ($cmd) {
    "shot" {
        $b = [System.Windows.Forms.SystemInformation]::VirtualScreen
        $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.CopyFromScreen($b.Left, $b.Top, 0, 0, $bmp.Size)
        $bmp.Save($a1, [System.Drawing.Imaging.ImageFormat]::Png)
        "saved $a1 ($($b.Width)x$($b.Height))"
    }
    "dialogs" {
        foreach ($w in Get-UnityWindows) {
            "WINDOW: [$($w.Current.Name)] class=$($w.Current.ClassName) pid=$($w.Current.ProcessId)"
            $btnCond = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Button)
            foreach ($bt in $w.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)) {
                "   button: [$($bt.Current.Name)]"
            }
            $txtCond = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Text)
            foreach ($t in $w.FindAll([System.Windows.Automation.TreeScope]::Descendants, $txtCond)) {
                $n = $t.Current.Name
                if ($n) { "   text: $($n.Substring(0, [Math]::Min(300, $n.Length)))" }
            }
        }
    }
    "click" {
        foreach ($w in Get-UnityWindows) {
            if ($w.Current.Name -notlike "*$a1*") { continue }
            $btnCond = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty, $a2)
            $bt = $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
            if ($bt) {
                $bt.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
                "clicked [$a2] in [$($w.Current.Name)]"
                exit 0
            }
        }
        "not found"
        exit 1
    }
}
