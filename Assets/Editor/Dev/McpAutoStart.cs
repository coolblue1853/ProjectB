#if UNITY_EDITOR
using UnityEditor;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// unity-mcp(MCP for Unity) 브리지를 에디터 로드 시 자동으로 켜고, 텔레메트리를 끈다.
    /// 자동화(Claude)가 에디터를 열자마자 콘솔/플레이 모드를 다룰 수 있게 하기 위한 개발 전용 설정.
    /// </summary>
    [InitializeOnLoad]
    internal static class McpAutoStart
    {
        private const string AutoStartKey = "MCPForUnity.AutoStartOnLoad";
        private const string TelemetryDisabledKey = "MCPForUnity.TelemetryDisabled";

        static McpAutoStart()
        {
            EditorPrefs.SetBool(TelemetryDisabledKey, true);

            if (EditorPrefs.GetBool(AutoStartKey, false))
            {
                return;
            }

            // 패키지의 자동 시작 핸들러는 도메인 로드 시점에 이 값을 읽으므로, 처음 켤 때는 한 번 더 리로드한다.
            EditorPrefs.SetBool(AutoStartKey, true);
            EditorUtility.RequestScriptReload();
        }
    }
}
#endif
