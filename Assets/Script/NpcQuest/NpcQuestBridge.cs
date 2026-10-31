/// <summary>Dialogue System Lua 함수의 실제 구현 (DialogueBoot 이 NpcQuestAccept / NpcQuestComplete 로 등록)</summary>
public static class NpcQuestBridge
{
    public static void Accept(string questId)
    {
        NpcQuestLog.Accept(questId);
    }

    public static void Complete(string questId)
    {
        NpcQuestLog.Complete(questId);
    }
}
