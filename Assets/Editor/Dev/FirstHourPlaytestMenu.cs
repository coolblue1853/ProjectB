#if UNITY_EDITOR
using UnityEditor;

namespace ProjectB.EditorTools
{
    /// <summary>첫 1시간 검증용 세션 설정. 씬·세이브·기본 낮 고정 설정은 저장하지 않는다.</summary>
    internal static class FirstHourPlaytestMenu
    {
        private const string CycleMenu = "Tools/ProjectB/Playtest/Enable Day-Night Cycle (Play Mode)";
        private const string DayMenu = "Tools/ProjectB/Playtest/Daytime for Screenshot (Play Mode)";
        private const string SaveMenu = "Tools/ProjectB/Playtest/Save Progress (Play Mode)";

        [MenuItem(CycleMenu)]
        private static void EnableCycle()
        {
            TimeChange.instance.LockDaytime = false;
        }

        [MenuItem(DayMenu)]
        private static void Daytime()
        {
            TimeChange.instance.LockDaytime = true;
        }

        [MenuItem(SaveMenu)]
        private static void SaveProgress()
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.SaveProgress();
            }
        }

        [MenuItem(CycleMenu, true)]
        [MenuItem(DayMenu, true)]
        [MenuItem(SaveMenu, true)]
        private static bool CanSetTime()
        {
            return EditorApplication.isPlaying && TimeChange.instance != null;
        }
    }
}
#endif
