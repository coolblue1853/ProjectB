/// <summary>
/// 마을·숲 NPC 인사 대사에 이어 붙이는 힌트 페이지 (루엔·세나·이브). 켜고 끄기: Floor1Content.csv 의 chatter.
/// 한 페이지는 2줄 이내. 새 콘텐츠(숨은 상자·도감 보상·사냥 의뢰)로 이어지는 길잡이를 겸한다.
/// </summary>
public static class Floor1Chatter
{
    public static string[] Pages(string npcId, string greeting)
    {
        if (!Floor1Data.On("chatter"))
        {
            return new[] { greeting };
        }
        switch (npcId)
        {
            case "ruen":
                return new[]
                {
                    greeting,
                    "일손이 필요하면 의뢰 게시판 오른쪽 푯말을 봐.\n하루에 사냥 의뢰가 세 건 걸려.",
                    "정예가 나타나면 화면에 글자가 떠.\n홀로 덤비지 말고 물약부터 챙겨.",
                };
            case "sena":
                return new[]
                {
                    greeting,
                    "도감을 채우면 마을에서 작은 답례가 있어요.\nB 로 열어 보세요.",
                    "표본 덤불은 표적에 맞춰 두 번째 ↑ 를 눌러야 해요.\n서두르면 뿌리가 상해요.",
                };
            case "eve":
                return new[]
                {
                    greeting,
                    "맵 끝자락에는 누가 숨겨 둔 상자가 있어.\n고지대나 막다른 길을 살펴 봐.",
                    "발자국이 이어지면 따라가 봐.\n세 번째 끝에는 주인이 있어.",
                };
        }
        return new[] { greeting };
    }
}
