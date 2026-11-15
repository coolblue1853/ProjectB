using System.ComponentModel;

/// <summary>자동 플레이 봇 켜기·끄기(SR Debugger "Bot" 묶음). 봇은 진짜 가상 입력만 쓴다 — PlayBotDesign.md 10절</summary>
public partial class SROptions
{
    private const string BotCategory = "Bot";

    [Category(BotCategory), DisplayName("Bot Hunt 5 min")]
    public void BotHunt5()
    {
        PlayBot.Run(5f, PlayBot.GoalHunt);
    }

    [Category(BotCategory), DisplayName("Bot Stop")]
    public void BotStop()
    {
        PlayBot.Stop();
    }

    [Category(BotCategory), DisplayName("Bot Status")]
    public string BotStatus => PlayBot.Status();
}
