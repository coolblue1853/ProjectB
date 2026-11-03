using PixelCrushers.DialogueSystem;

/// <summary>대장장이 무기 시연대 대화 (Floor1Completion R4): 대여 제안, 반납 시 무기 1종 선택. 스토리 없이 목표 설명만</summary>
public static partial class StoryDialogues
{
    private const int TrialConvBase = 400;

    private static void BuildWeaponTrials()
    {
        int id = TrialConvBase;
        foreach (WeaponTrialDef d in WeaponTrial.Defs)
        {
            BuildTrialRent(id++, d);
            BuildTrialPick(id++, d);
        }
    }

    private static void BuildTrialRent(int convId, WeaponTrialDef d)
    {
        Conv v = NewConv(convId, WeaponTrial.RentConversation(d.Stage), PlayerId, SmithId);
        DialogueEntry hub = Chain(v, Start(v), SmithId, "무기 시연대다. 계보 무기 " + d.Weapons.Length + "자루를 빌려주지.\n사냥터에서 써 보고, 마음에 드는 하나만 가져가라.");
        DialogueEntry yes = Line(v, PlayerId, "빌린다");
        DialogueEntry no = Line(v, PlayerId, "나중에");
        Link(v, hub, yes);
        Link(v, hub, no);
        yes.userScript = "WeaponTrialRent('" + d.Stage + "')";
        Chain(v, yes, SmithId, "가방에 넣어 뒀다. 다 써 봤으면 다시 와라.");
        Chain(v, no, SmithId, "준비되면 말해.");
    }

    private static void BuildTrialPick(int convId, WeaponTrialDef d)
    {
        Conv v = NewConv(convId, WeaponTrial.PickConversation(d.Stage), PlayerId, SmithId);
        DialogueEntry hub = Chain(v, Start(v), SmithId, "써 봤나? 가져갈 무기를 하나 골라라. 나머지는 돌려받는다.");
        foreach (string w in d.Weapons)
        {
            DialogueEntry pick = Line(v, PlayerId, "[lua(WeaponTrialName('" + w + "'))] 로 한다"); // 영문 ID 대신 한글 이름(대화 때 조회)
            Link(v, hub, pick);
            pick.userScript = "WeaponTrialPick('" + d.Stage + "','" + w + "')";
            Chain(v, pick, SmithId, "좋은 선택이다.");
        }
        DialogueEntry later = Line(v, PlayerId, "더 써 본다");
        Link(v, hub, later);
        Chain(v, later, SmithId, "충분히 써 보고 와.");
    }
}
