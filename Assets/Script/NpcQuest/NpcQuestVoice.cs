using System;

/// <summary>1층계 NPC 대사. 제안 페이지는 | 로 나누며 한 페이지는 2줄 이내다.</summary>
public static class NpcQuestVoice
{
    public static string[] Offer(NpcQuestDef quest)
    {
        switch (quest.Id)
        {
            case "Q101": return "그 브로치… 아버지가 남긴 것이로군.\n떠나기 전 루엔에게 ↑로 의뢰를 받게.|마을 오른쪽 끝 포털에서 ↑를 누르면 숲 샛길일세.|X 공격·C 점프·Shift 달리기.\nQ 퀘스트·I 가방·1~4 물약. 대시는 나중일세.".Split('|');
            case "Q102": return "샛길 오른쪽 끝 포털로 서쪽 구역에 가.\n오른쪽 고지대의 꽃두더지 8마리를 잡아 줘.|C로 뛰고 ↑로 로프를 잡아. ↑↓로 오르내려.\n끝나면 마을의 내게 돌아와.".Split('|');
            case "Q103": return "샛길 입구나 오른쪽 표본 덤불에서\n약초 3개를 모아 와 주세요.|덤불은 ↑로 살피고 표적에 맞춰 다시 ↑예요.\n보고하면 요리에 쓸 약초 1개를 돌려드릴게요.".Split('|');
            case "Q104": return "밤의 샛길에서 허수아비 6마리를 잡아 줘.\n낮이면 꽃두더지와 토끼를 사냥하며 기다려.|물약부터 챙겨.\n낮에 서 있던 자리만 보고 다가가진 마.".Split('|');
            case "Q105": return "샛길 낮 사냥터의 이끼껍질 딱정벌레 5마리를 잡아 줘.\n느리고 단단해. 서두르지 말고 몸박부터 피해.".Split('|');
            case "Q201": return "샛길 오른쪽 포털 너머 일반 숲으로 가게.\n숲 늑대에게서 별무늬 수피 조각 3개를 찾아 주게.|이 의뢰를 받은 동안에만 조각을 얻을 수 있네.\n브로치의 문양과 닮았는지 살펴보세.".Split('|');
            case "Q202": return "샛길 오른쪽 포털로 일반 숲에 들어가.\n왼쪽으로 가며 멧돼지 8마리를 잡아 줘.".Split('|');
            case "Q203": return "샛길 입구 오른쪽 화로를 써 봐요.\n허브 토끼구이 1개를 만들면 돼요.|토끼고기 2·약초 1·나무 1은 남겨 둬요.\n↑ 시작·X 요리와 뒤집기·ESC 취소예요.".Split('|');
            case "Q204": return "여기 일반 숲 입구에서 왼쪽으로 조금 가 봐.\n금빛 별 관측지를 밟고 내게 돌아와.|이름은 야간 관측지지만 낮에도 갈 수 있어.\n어둡기 전에 길을 알아 두는 거지.".Split('|');
            case "Q301": return "깊은 숲의 고대 엔트 1마리를 쓰러뜨려 주게.\n오래된 나이테에 탑의 흔적이 남아 있네.".Split('|');
            case "Q302": return "깊은 숲의 곰 7마리를 잡아 줘.\n큰 발자국이 길을 가로막고 있어.".Split('|');
            case "Q303": return "깊은 숲 엔트에게서 심재 3개를 가져와.\n죽은 나무도 속은 단단하지.".Split('|');
            case "Q304": return "깊은 숲 정예 둥지를 찾아가 봐.\n남은 흔적을 보고 내게 돌아와.".Split('|');
            case "Q305": return "깊은 숲의 가지뿔 여우 6마리를 잡아 줘.\n짧게 돌진하니까 달려드는 쪽을 봐.".Split('|');
            case "Q401": return "늪 악어에게서 별 문장 조각 3개를 찾아 주게.\n물속에도 같은 흔적이 남았군.".Split('|');
            case "Q402": return "늪 낚시터에서 이끼미꾸라지 2마리를 낚아 와요.\n끊긴 낚싯줄은 제가 정리해 둘게요.".Split('|');
            case "Q403": return "늪 슬라임 8마리를 처치해 주세요.\n밤의 늪에서는 진흙도 움직여요.".Split('|');
            case "Q404": return "늪의 거대 슬라임 1마리를 쓰러뜨려 줘.\n작은 놈들 뒤에 더 큰 흔적이 있어.".Split('|');
            case "Q405": return "늪의 포자등 두꺼비 6마리를 처치해 주세요.\n맞으면 포자 구름이 퍼지니 물러나는 편이 좋아요.".Split('|');
            case "Q501": return "산적 성채의 단검 산적 8명을 처치해 줘요.\n운반대가 끊기면 마을 보급도 멎어요.".Split('|');
            case "Q502": return "성채의 산적 궁수에게서 빼앗긴 화물 4개를 찾아와.\n대장간 물건이다. 아직 쓸 수 있을 거야.".Split('|');
            case "Q503": return "성채의 방패 산적 7명을 처치해 줘.\n안쪽 길을 막고 있어.".Split('|');
            case "Q504": return "성채의 산적 대장 1명을 쓰러뜨려 줘.\n남은 흔적은 그 안으로 이어져.".Split('|');
            case "Q601": return "탑 근교의 네 갈래 별 제단을 찾아가게.\n그곳에서 브로치의 반응을 살펴 주게.".Split('|');
            case "Q602": return "탑 근교 유적에서 아버지의 기록 5편을 모아.\n발굴은 반복할 수 있어. 유물이 나오면 잠시 쉬어.".Split('|');
            case "Q603": return "탑 근교의 중형 골렘 6기를 부숴 줘.\n놈들이 길을 지키는 동안은 짐을 옮길 수 없어.".Split('|');
            case "Q604": return "탑의 문지기 1기를 쓰러뜨려 주게.\n그 너머가 다음 층계로 가는 길일세.".Split('|');
            default: return new[] { quest.Description }; // 신규 퀘스트는 목표 설명만 쓴다(스토리 대사는 나중에 교체)
        }
    }

    public static string Progress(NpcQuestDef quest)
    {
        switch (quest.Id)
        {
            case "Q101": return "루엔에게 먼저 가 보게.\n숲으로 나가는 길은 마을 오른쪽 끝일세.";
            case "Q102": return "고지대 꽃두더지 8마리야.\n로프는 ↑로 잡아. 발밑부터 살펴.";
            case "Q103": return "약초 3개예요. 뿌리까지 다치지 않게 살펴요.\n샛길 입구와 오른쪽 덤불에 있어요.";
            case "Q104": return "허수아비는 밤에만 나와. 6마리야.\n물약이 모자라면 먼저 돌아와.";
            case "Q105": return "이끼껍질 딱정벌레 5마리야.\n껍질이 단단하니 몇 번 더 쳐야 해.";
            case "Q201": return "일반 숲 늑대에게서 수피 조각 3개일세.\n작은 흔적도 버리지 말게.";
            case "Q202": return "일반 숲의 멧돼지 8마리야.\n돌진하니 거리를 벌리며 잡아.";
            case "Q203": return "토끼고기 2·약초 1·나무 1이에요.\n샛길 화로에서 ↑ 시작·X 요리와 뒤집기·ESC 취소.";
            case "Q204": return "입구 왼쪽의 금빛 별을 밟으면 돼.\n낮에도 괜찮아. 돌아오는 길도 봐 두고.";
            case "Q301": return "깊은 숲의 고대 엔트일세.\n그 기억에서 문양을 찾을 수 있겠지.";
            case "Q302": return "깊은 숲의 곰 7마리야.\n앞발이 들리면 거리를 벌려.";
            case "Q303": return "엔트 심재 3개다.\n겉껍질 말고 안쪽 나무를 가져와.";
            case "Q304": return "깊은 숲의 정예 둥지야.\n비어 보여도 발밑 흔적은 남아 있어.";
            case "Q305": return "가지뿔 여우 6마리야.\n돌진이 끝난 틈에 쳐.";
            case "Q401": return "늪 악어가 삼킨 별 문장 조각 3개일세.\n물가에서는 오래 머물지 말게.";
            case "Q402": return "늪 낚시터의 이끼미꾸라지 2마리예요.\n물약도 챙겨요. 물가가 늘 조용하진 않아요.";
            case "Q403": return "늪 슬라임 8마리예요.\n진흙에 가려진 몸을 조심하세요.";
            case "Q404": return "거대 슬라임이야. 한 마리면 돼.\n물약이 떨어지기 전에 끝내.";
            case "Q405": return "포자등 두꺼비 6마리예요.\n포자가 퍼지면 숨을 고르고 물러나세요.";
            case "Q501": return "성채의 단검 산적 8명이에요.\n돌아올 때 보급로 사정도 알려 줘요.";
            case "Q502": return "산적 궁수에게서 화물 4개다.\n상자째 가져와. 안은 내가 살핀다.";
            case "Q503": return "방패 산적 7명이야.\n정면에 오래 서 있지 마.";
            case "Q504": return "산적 대장이 남았어. 한 명이야.\n돌아올 길을 놓치지는 말고.";
            case "Q601": return "탑 근교의 별 제단일세.\n브로치가 반응하면 돌아와 들려주게.";
            case "Q602": return "모을 기록은 다섯 편이야.\n같은 유적도 쉬었다 다시 파 봐.";
            case "Q603": return "중형 골렘 6기다.\n탑 근교에서 찾아. 작은 놈과 혼동 말고.";
            case "Q604": return "탑 근교의 문지기가 남았네.\n준비를 마치고 문 앞에 서게.";
            default: return "아직 끝나지 않았어.\n" + quest.Description;
        }
    }

    public static string Report(NpcQuestDef quest)
    {
        switch (quest.Id)
        {
            case "Q101": return "이것을 챙겨 가게.\n아버지의 흔적을 찾거든 내게도 들려주게.";
            case "Q102": return "발자국이 줄었겠군.\n이걸 챙겨. 다음 사냥에도 쓸 거야.";
            case "Q103": return "잘 가져오셨어요. 아직 풀 냄새가 남았네요.\n약초 1개는 돌려드릴게요. 요리에 쓰세요.";
            case "Q104": return "밤길이 조금은 조용해지겠어.\n수고했어. 물약 챙겨 가.";
            case "Q105": return "딱정벌레 껍질이 꽤 단단했지.\n수고했어. 물약 챙겨 가.";
            case "Q201": return "네 갈래 별… 브로치와 같은 문양이군.\n아직은 뜻을 단정할 수 없네.";
            case "Q202": return "무리가 물러났군.\n이 단검은 네가 쓰는 편이 낫겠어.";
            case "Q203": return "이 정도면 숲에서도 한 끼는 버티겠어요.\n구이 하나 더 챙겨요. 빈속으로 가지 말고.";
            case "Q204": return "다녀왔구나. 길의 모양은 기억해 둬.\n밤에는 같은 숲도 낯설어지거든.";
            case "Q301": return "나이테에도 그 문장이 있었나.\n탑보다 오래된 나무가 무엇을 보았을까.";
            case "Q302": return "길을 되찾았군.\n곰 발톱이야. 네 손에 맞게 써.";
            case "Q303": return "이 정도 결이면 쓸 만해.\n층계 결정이다. 장비 손볼 때 가져와.";
            case "Q304": return "빈 둥지도 길을 말해 주지.\n다음에 갔을 땐 주인이 돌아왔을지도 몰라.";
            case "Q305": return "여우 길이 조금 넓어졌군.\n물약 챙겨. 더 깊이 갈 거라면.";
            case "Q401": return "물에 잠겨도 문양은 지워지지 않았군.\n탑으로 이어지는 흔적일지도 모르네.";
            case "Q402": return "두 마리 모두 받았어요.\n늪에서 돌아오는 물건은 늘 젖어 있네요.";
            case "Q403": return "돌아오셨네요. 상처는 없으세요?\n물약을 챙겨 드릴게요.";
            case "Q404": return "큰 흔적이 끊겼군.\n성채 쪽과 탑 근교를 살펴볼 차례야.";
            case "Q405": return "포자를 뒤집어쓰진 않으셨죠?\n물약을 챙겨 드릴게요.";
            case "Q501": return "이제 짐을 보낼 수 있겠어요.\n약속한 대금이에요. 무사히 와서 다행이고요.";
            case "Q502": return "네 상자, 맞군. 쇠도 무사해.\n이 광석은 네 몫이다.";
            case "Q503": return "안쪽 길이 열렸겠군.\n등 뒤도 살펴. 성채는 아직 조용하지 않아.";
            case "Q504": return "성채를 지나왔구나.\n이 결정은 네가 챙겨. 다음 길은 더 높아.";
            case "Q601": return "브로치가 제단에 응답했나.\n아버지의 자취도 그 길에 남아 있겠지.";
            case "Q602": return "다섯 편을 다 찾았구나.\n아버지가 남긴 말은 네가 간직해.";
            case "Q603": return "여섯 기, 확인했다.\n층계 결정이다. 다음 문 앞에서 장비부터 살펴.";
            case "Q604": return "브로치에 새 길이 새겨졌군.\n다음 층계에서도 무사히 돌아오게.";
            default: return "수고했어.\n보상을 받아 가.";
        }
    }

    public static string Accepted(string npcId)
    {
        switch (npcId)
        {
            case "elder": return "서두르지 말게. 흔적을 찾으면 돌아오게.";
            case "ruen": return "끝나면 마을에서 보자. 물약 챙기고.";
            case "sena": return "다녀오세요. 무리하면 돌아오셔도 돼요.";
            case "eve": return "다녀와. 네가 본 길도 들려줘.";
            case "merchant": return "일을 마치면 제게 와요. 보상은 맡아 둘게요.";
            case "smith": return "일 끝나면 대장간으로 와.";
            default: return "준비되면 다시 와 주세요.";
        }
    }

    public static string Declined(string npcId)
    {
        switch (npcId)
        {
            case "elder": return "준비가 먼저일세. 기다리겠네.";
            case "ruen": return "급할 건 없어. 장비부터 살펴.";
            case "sena": return "괜찮아요. 몸부터 돌보세요.";
            case "eve": return "길은 남아 있어. 준비되면 와.";
            case "merchant": return "준비되면 와요. 물자는 미리 챙기고요.";
            case "smith": return "그래. 준비 끝나면 와.";
            default: return "준비되면 다시 와 주세요.";
        }
    }

}
