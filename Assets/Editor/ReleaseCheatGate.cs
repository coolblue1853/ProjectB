using SRDebugger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// U33: 릴리스(비 Development) 빌드에서는 SR Debugger(치트 버튼 Heal·God Mode 등)를 끈다.
/// 빌드 직전 Settings.IsEnabled 를 Development 여부로 맞추고, 빌드 뒤 원래 값으로 복원한다(자산 diff 방지).
/// 빌드가 실패하면 Postprocess 가 호출되지 않으므로 ① 빌드 종료 감시(update) ② 에디터 로드 시 보정으로 복원한다.
/// 원래 값은 EditorPrefs 에 둬서 에디터 재시작·크래시 뒤에도 남는다.
/// </summary>
[InitializeOnLoad]
public sealed class ReleaseCheatGate : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    // EditorPrefs 는 프로젝트 간 전역이라 프로젝트 경로 해시를 붙여 충돌을 막는다.
    private static readonly string PrevKey = "ReleaseCheatGate.prev." + Application.dataPath.GetHashCode();

    static ReleaseCheatGate()
    {
        // 에디터 로드(도메인 리로드 포함) 시: 빌드 중이 아니고 복원 대기 값이 있으면 되돌린다.
        EditorApplication.delayCall += RestoreIfPending;
    }

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Settings settings = Settings.Instance;
        if (settings == null)
        {
            return;
        }

        // 이미 대기 값이 있으면(이전 빌드 복원 누락) 덮어쓰지 않는다 = 원본 값 보존.
        if (!EditorPrefs.HasKey(PrevKey))
        {
            EditorPrefs.SetBool(PrevKey, settings.IsEnabled);
        }

        bool dev = (report.summary.options & BuildOptions.Development) != 0;
        Apply(settings, dev);
        Debug.Log("[U33] SRDebugger " + (dev ? "켜짐(Development 빌드)" : "꺼짐(릴리스 빌드)"));

        // 빌드 실패·취소 시에도 빌드가 끝나면 복원.
        EditorApplication.update -= WatchBuildEnd;
        EditorApplication.update += WatchBuildEnd;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // 이 시점엔 isBuildingPlayer 가 아직 참일 수 있다. 그 경우 실제 복원은 WatchBuildEnd 가 맡는다.
        RestoreIfPending();
    }

    private static void WatchBuildEnd()
    {
        if (BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        EditorApplication.update -= WatchBuildEnd;
        RestoreIfPending();
    }

    /// <summary>복원 대기 값이 있고 빌드 중이 아니면 SR Debugger 설정을 원래 값으로 되돌린다.</summary>
    internal static void RestoreIfPending()
    {
        if (!EditorPrefs.HasKey(PrevKey) || BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        bool prev = EditorPrefs.GetBool(PrevKey, true);
        EditorPrefs.DeleteKey(PrevKey);
        Settings settings = Settings.Instance;
        if (settings != null && settings.IsEnabled != prev)
        {
            Apply(settings, prev);
        }
    }

    private static void Apply(Settings settings, bool enabled)
    {
        settings.IsEnabled = enabled;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    internal static bool HasPending => EditorPrefs.HasKey(PrevKey);

    internal static void SetPendingForTest(bool prev)
    {
        EditorPrefs.SetBool(PrevKey, prev);
    }
}
