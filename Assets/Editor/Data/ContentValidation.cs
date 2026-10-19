#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// 콘텐츠 교차 검증 (AssetIntegrationPlan 6절 9, Odin 데이터 창 탭). 서로 이름으로 이어진 데이터가 어긋나는지 본다:
    /// 퀘스트 대상·보상·선행, 몬스터 CSV ↔ 프리팹, 아이템 아이콘, 효과음, 구역 BGM·앰비언트, NPC 대화.
    /// 프로브에서도 ContentValidation.Build() 로 같은 결과를 얻는다.
    /// </summary>
    public class ContentValidation
    {
        [ShowInInspector, ReadOnly, MultiLineProperty(40), HideLabel]
        string Report;

        [Button("다시 검사")]
        void Refresh()
        {
            Report = Build();
        }

        public ContentValidation()
        {
            Report = Build();
        }

        public static string Build()
        {
            var sb = new StringBuilder();
            var monsterPrefabs = new HashSet<string>(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/Enemy_BD" })
                .Select(g => System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g))));
            var itemNames = new HashSet<string>(ItemTable.Load().Rows.Where(r => r.Name != "-").Select(r => r.Name));
            CheckQuests(sb, monsterPrefabs, itemNames);
            CheckMonsters(sb, monsterPrefabs);
            CheckIcons(sb, itemNames);
            CheckSfx(sb);
            CheckAudio(sb);
            CheckDialogue(sb);
            int quests = QuestCatalog.All().Count() + QuestCatalog.Story.Count;
            string summary = "검사: 퀘스트 " + quests + " · 몬스터 프리팹 " + monsterPrefabs.Count + " / CSV " + MonsterTable.All().Count() +
                " · 아이템 " + itemNames.Count + " · 효과음 " + System.Enum.GetValues(typeof(SfxId)).Length + System.Environment.NewLine;
            return summary + (sb.Length == 0 ? "문제 없음 ✅" : sb.ToString());
        }

        static void CheckQuests(StringBuilder sb, HashSet<string> monsters, HashSet<string> items)
        {
            var ids = new HashSet<string>(QuestCatalog.All().Concat(QuestCatalog.Story).Select(d => d.id));
            foreach (QuestCatalog.Def d in QuestCatalog.All().Concat(QuestCatalog.Story))
            {
                if (d.message == "Kill")
                {
                    foreach (string t in d.targets)
                    {
                        if (!monsters.Contains(t)) sb.AppendLine("❌ 퀘스트 " + d.id + ": 처치 대상 프리팹 없음 " + t);
                    }
                }
                else if (d.message == "Pickup")
                {
                    foreach (string t in d.targets)
                    {
                        if (!items.Contains(t)) sb.AppendLine("❌ 퀘스트 " + d.id + ": 줍기 대상 아이템 없음 " + t);
                    }
                }
                if (!string.IsNullOrEmpty(d.rewardItem) && !items.Contains(d.rewardItem)) sb.AppendLine("❌ 퀘스트 " + d.id + ": 보상 아이템 없음 " + d.rewardItem);
                if (!string.IsNullOrEmpty(d.requires) && !ids.Contains(d.requires)) sb.AppendLine("❌ 퀘스트 " + d.id + ": 선행 퀘스트 없음 " + d.requires);
                if (d.count <= 0) sb.AppendLine("⚠ 퀘스트 " + d.id + ": 목표 수 0");
            }
        }

        static void CheckMonsters(StringBuilder sb, HashSet<string> prefabs)
        {
            var rows = new HashSet<string>(MonsterTable.All().Select(r => r.id));
            foreach (string id in rows.Where(r => !prefabs.Contains(r))) sb.AppendLine("❌ MonsterSheet 행 " + id + ": 프리팹 없음");
            foreach (string p in prefabs.Where(p => !rows.Contains(p))) sb.AppendLine("⚠ 프리팹 " + p + ": MonsterSheet 행 없음(표시명·스탯 기본값)");
        }

        static void CheckIcons(StringBuilder sb, HashSet<string> items)
        {
            foreach (string n in items.OrderBy(x => x))
            {
                if (Resources.Load<Sprite>("Item/" + n) == null) sb.AppendLine("⚠ 아이템 " + n + ": 아이콘 없음(Resources/Item)");
            }
        }

        static void CheckSfx(StringBuilder sb)
        {
            var lib = Resources.Load<SfxLibrary>("Audio/SfxLibrary");
            if (lib == null)
            {
                sb.AppendLine("❌ SfxLibrary 없음");
                return;
            }
            foreach (SfxId id in System.Enum.GetValues(typeof(SfxId)))
            {
                SfxLibrary.Entry e = lib.Get(id);
                if (e == null || e.Pick() == null) sb.AppendLine("❌ 효과음 " + id + ": 클립 없음");
            }
        }

        static void CheckAudio(StringBuilder sb)
        {
            var ma = Object.FindFirstObjectByType<DarkTonic.MasterAudio.MasterAudio>(FindObjectsInactive.Include);
            var maps = new[] { "Town", "Lv1_NearForest", "Lv1_NearForest_SideRoad", "Lv1_NormalForest", "Lv1_DeepForest", "Lv1_Swamp",
                "Lv1_BanditFortress", "Lv1_TowerOutskirts", "Lv2_MagicCliffs", "Lv2_BrokenRampart" };
            if (ma == null)
            {
                sb.AppendLine("⚠ 열린 씬에 MasterAudio 없음 — 재생목록 검사 생략");
            }
            else
            {
                var lists = new HashSet<string>(ma.musicPlaylists.Select(p => p.playlistName));
                foreach (string need in new[] { "Town", "Field", "Field2", "Night", "Boss", "Victory" }) if (!lists.Contains(need)) sb.AppendLine("❌ 재생목록 없음 " + need);
                foreach (string m in maps)
                {
                    foreach (bool night in new[] { false, true })
                    {
                        string z = BgmDirector.ZonePlaylist(m, night);
                        if (z != null && !lists.Contains(z)) sb.AppendLine("❌ 구역 재생목록 없음 " + z + " (" + m + ")");
                    }
                }
            }
            foreach (string m in maps)
            {
                foreach (bool night in new[] { false, true })
                {
                    string c = AmbienceDirector.ClipFor(m, night);
                    if (!string.IsNullOrEmpty(c) && Resources.Load<AudioClip>("Ambience/" + c) == null) sb.AppendLine("❌ 앰비언트 클립 없음 " + c + " (" + m + ")");
                }
            }
        }

        static void CheckDialogue(StringBuilder sb)
        {
            var db = StoryDialogues.Build();
            var titles = new HashSet<string>(db.conversations.Select(c => c.Title));
            foreach (var c in db.conversations)
            {
                var entryIds = new HashSet<int>(c.dialogueEntries.Select(e => e.id));
                foreach (var e in c.dialogueEntries)
                {
                    foreach (var link in e.outgoingLinks)
                    {
                        if (link.destinationConversationID == c.id && !entryIds.Contains(link.destinationDialogueID)) sb.AppendLine("❌ 대화 " + c.Title + ": 끊긴 연결 " + e.id + "→" + link.destinationDialogueID);
                    }
                }
            }
            foreach (TalkNpc npc in Object.FindObjectsByType<TalkNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!titles.Contains(npc.Conversation)) sb.AppendLine("❌ NPC " + npc.name + ": 대화 없음 " + npc.Conversation);
            }
            Object.DestroyImmediate(db);
        }
    }
}
#endif
