using SRDebugger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// U33: 릴리스(비 Development) 빌드에서는 SR Debugger(치트 버튼 Heal·God Mode 등)를 끈다.
/// 빌드 직전 Settings.IsEnabled 를 Development 여부로 맞추고, 빌드 뒤 원래 값으로 복원한다(자산 diff 방지).
/// </summary>
public sealed class ReleaseCheatGate : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const string PrevKey = "ReleaseCheatGate.prev";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Settings settings = Settings.Instance;
        if (settings == null)
        {
            return;
        }

        SessionState.SetBool(PrevKey, settings.IsEnabled);
        bool dev = (report.summary.options & BuildOptions.Development) != 0;
        Apply(settings, dev);
        Debug.Log("[U33] SRDebugger " + (dev ? "켜짐(Development 빌드)" : "꺼짐(릴리스 빌드)"));
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Settings settings = Settings.Instance;
        if (settings != null && SessionState.GetBool(PrevKey, settings.IsEnabled) != settings.IsEnabled)
        {
            Apply(settings, SessionState.GetBool(PrevKey, true));
        }
    }

    private static void Apply(Settings settings, bool enabled)
    {
        settings.IsEnabled = enabled;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}
