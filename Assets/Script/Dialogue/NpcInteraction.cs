using System;
using UnityEngine;

/// <summary>
/// NPC ↑ 상호작용 공용 진입점 (REWORK-8). NPC 가 가진 기능(퀘스트·상점·제작·강화·시험·대화)만 모아 선택지를 띄운다.
/// 항목이 하나뿐이면 메뉴 없이 바로 실행한다. 퀘스트 대화는 "퀘스트 이야기" 를 골랐을 때만 시작한다.
/// </summary>
public static class NpcInteraction
{
    public const string MenuTitle = "무엇을 할까요?";
    private static int s_lastRunFrame = -1;

    public static void Run(GameObject npc, Action shop, Action craft, Action enhance, Action talk)
    {
        if (s_lastRunFrame == Time.frameCount)
        {
            return; // 한 NPC 에 상호작용 컴포넌트가 여럿이어도 같은 ↑ 프레임에는 한 번만
        }
        s_lastRunFrame = Time.frameCount;
        NpcQuestGiver giver = npc != null ? npc.GetComponent<NpcQuestGiver>() : null;
        bool quest = giver != null && giver.HasQuest;
        bool trial = giver != null && giver.NpcId == "smith";
        FastTravelNpc coach = npc != null ? npc.GetComponent<FastTravelNpc>() : null;
        var kinds = NpcMenuRules.Build(quest, shop != null, craft != null, enhance != null, trial, talk != null, coach != null);
        Action<NpcMenuRules.Kind> act = kind =>
        {
            switch (kind)
            {
                case NpcMenuRules.Kind.Quest: giver.TalkQuest(); break;
                case NpcMenuRules.Kind.Shop: shop(); break;
                case NpcMenuRules.Kind.Craft: craft(); break;
                case NpcMenuRules.Kind.Enhance: enhance(); break;
                case NpcMenuRules.Kind.Trial: WeaponTrial.TryTalk(npc.transform); break;
                case NpcMenuRules.Kind.Talk: talk(); break;
                case NpcMenuRules.Kind.Travel: coach.OpenDestinations(); break;
            }
        };
        if (!NpcMenuRules.NeedsMenu(kinds))
        {
            if (kinds[0] != NpcMenuRules.Kind.Close)
            {
                act(kinds[0]);
            }
            return;
        }
        NpcMenu.Open(MenuTitle, kinds, act);
    }
}
