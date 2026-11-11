/// <summary>
/// 사냥 푯말에 붙는 1층계 졸업 점검 문구. 사냥 의뢰 화면 자체는 REWORK-5 P4 에서 퀘스트 창(입구 모드, QuestEntrance.Kiosk)으로 통합되어
/// 이 클래스에는 푯말 고유 기능인 졸업 점검만 남았다.
/// </summary>
public static class BountyGraduation
{
    /// <summary>1층계 졸업 점검: 레벨 15, Stoneborn 세트 3부위 이상, 탑 문지기 처치</summary>
    public static string Check()
    {
        int level = LevelSystem.Level;
        int gate = BestiaryRecord.Kills("Gatekeeper_BD");
        string lv = level >= 15 ? "<color=#8fe38f>Lv " + level + " OK</color>" : "<color=#ffc640>Lv " + level + "/15</color>";
        string boss = gate > 0 ? "<color=#8fe38f>탑 문지기 처치</color>" : "<color=#ffc640>탑 문지기 미처치</color>";
        return "1층계 졸업 점검:  " + lv + "   ·   " + boss + "   ·   Stoneborn 세트는 I 창에서 확인";
    }
}
