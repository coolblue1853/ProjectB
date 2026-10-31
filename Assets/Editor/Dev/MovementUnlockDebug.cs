using UnityEditor;

public static class MovementUnlockDebug
{
    private const string Menu = "Tools/ProjectB/Movement/Unlock All (Play Mode)";
    [MenuItem(Menu)]
    private static void Toggle()
    {
        MovementUnlocks state = MovementUnlocks.Instance;
        if (state != null) { state.DebugAll = !state.DebugAll; }
    }

    [MenuItem(Menu, true)]
    private static bool Validate()
    {
        MovementUnlocks state = MovementUnlocks.Instance;
        UnityEditor.Menu.SetChecked(Menu, state != null && state.DebugAll);
        return EditorApplication.isPlaying && state != null;
    }
}
