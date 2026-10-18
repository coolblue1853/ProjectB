#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// Odin 기반 기획 데이터 편집기: ItemSheet.csv / SetItem.csv / 몬스터 프리팹 스탯 / 검증 리포트.
    /// 메뉴: ProjectB/데이터 편집기. 저장하면 CSV를 원래 형식(타입 행·헤더 행·CRLF)으로 다시 쓴다.
    /// </summary>
    public class ProjectBDataWindow : OdinMenuEditorWindow
    {
        public const string ItemCsv = "Assets/Resources/ItemSheet.csv";
        public const string SetCsv = "Assets/Resources/SetItem.csv";

        [MenuItem("ProjectB/데이터 편집기")]
        static void Open()
        {
            var w = GetWindow<ProjectBDataWindow>();
            w.titleContent = new GUIContent("ProjectB 데이터");
            w.Show();
        }

        ItemTable items;
        SetTable sets;

        protected override OdinMenuTree BuildMenuTree()
        {
            items = ItemTable.Load();
            sets = SetTable.Load();
            var tree = new OdinMenuTree(true);
            tree.Config.DrawSearchToolbar = true;
            tree.Add("검증 리포트", new ValidationReport(items, sets));
            tree.Add("아이템 (전체 표)", items);
            foreach (ItemRow r in items.Rows.Where(r => r.Name != "-"))
            {
                string cat = r.Type == "Equip" ? "장비/" + r.EquipArea : r.Type == "Consum" ? "소비" : "재료";
                tree.Add("아이템/" + cat + "/" + r.Name, r);
            }
            tree.Add("세트 효과", sets);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/Enemy_BD" }))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var eh = go != null ? go.GetComponent<EnemyHealth>() : null;
                if (eh != null)
                {
                    tree.Add("몬스터/" + go.name, eh);
                }
            }
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            GUILayout.FlexibleSpace();
            if (SirenixEditorGUI.ToolbarButton("CSV 저장"))
            {
                items.Save();
                sets.Save();
                AssetDatabase.Refresh();
            }
            if (SirenixEditorGUI.ToolbarButton("다시 읽기"))
            {
                ForceMenuTreeRebuild();
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }

    // ───────────── ItemSheet.csv ─────────────

    [System.Serializable]
    public class ItemRow
    {
        static readonly string[] Types = { "Misc", "Consum", "Equip" };
        static readonly string[] Areas = { "-", "Weapon", "Head", "Chest", "Leg", "Hand", "Shoes", "Ring", "Necklace" };
        static readonly string[] Rarities = { "C", "R", "U", "" };

        [ReadOnly, HorizontalGroup("id", 60), HideLabel] public int Index;
        [HorizontalGroup("id"), ValidateInput(nameof(NoComma), "쉼표 금지")] public string Name;
        [ValueDropdown(nameof(Types))] public string Type;
        [TextArea(1, 3), ValidateInput(nameof(NoComma), "쉼표 금지")] public string Description;
        [HorizontalGroup("price")] public int Price;
        [HorizontalGroup("price")] public int Weight;
        public string AcqPath;
        [MinValue(1)] public int MaxStack = 1;
        [FoldoutGroup("소비 효과")] public string EffectOb;
        [FoldoutGroup("소비 효과")] public string EffectPow;
        [ValueDropdown(nameof(Areas)), ShowIf(nameof(IsEquip))] public string EquipArea;
        [ShowIf(nameof(IsEquip)), Tooltip("세트 키 겸 방어구 텍스처 이름")] public string TfName;
        public string ItemNameT;
        public int Tear;
        [ValueDropdown(nameof(Rarities))] public string Rarity;
        public int Upgrade;
        [ShowIf(nameof(IsEquip))] public string SetName;

        bool IsEquip => Type == "Equip";
        static bool NoComma(string v) => v == null || !v.Contains(",");

        [ShowInInspector, PreviewField(48), ReadOnly, PropertyOrder(-1)]
        Sprite Icon => Name == "-" ? null : Resources.Load<Sprite>("Item/" + Name);

        public IEnumerable<string> Problems()
        {
            if (Name == "-")
            {
                yield break;
            }
            if (Icon == null) yield return "아이콘 없음: Resources/Item/" + Name;
            if (IsEquip && Resources.Load<GameObject>(EquipArea + "/" + Name) == null) yield return "장비 프리팹 없음: Resources/" + EquipArea + "/" + Name;
            if (IsEquip && !string.IsNullOrEmpty(TfName) && EquipArea != "Weapon" && Resources.Load<Texture2D>("Sprite/" + TfName) == null) yield return "방어구 텍스처 없음: Resources/Sprite/" + TfName;
            if (string.IsNullOrEmpty(Rarity)) yield return "등급 비어 있음";
            if (!NoComma(Name) || !NoComma(Description)) yield return "쉼표 포함";
        }

        public string ToCsv() => string.Join(",", new[]
        {
            Index.ToString(), Name, Type, Description, Price.ToString(), Weight.ToString(), AcqPath, MaxStack.ToString(),
            EffectOb, EffectPow, EquipArea, TfName, ItemNameT, Tear.ToString(), Rarity, Upgrade.ToString(), SetName,
        });

        public static ItemRow FromCsv(string[] c)
        {
            string G(int i) => i < c.Length ? c[i].Trim() : "";
            int I(int i) => int.TryParse(G(i), out int v) ? v : 0;
            return new ItemRow
            {
                Index = I(0), Name = G(1), Type = G(2), Description = G(3), Price = I(4), Weight = I(5), AcqPath = G(6),
                MaxStack = I(7), EffectOb = G(8), EffectPow = G(9), EquipArea = G(10), TfName = G(11), ItemNameT = G(12),
                Tear = I(13), Rarity = G(14), Upgrade = I(15), SetName = G(16),
            };
        }
    }

    public class ItemTable
    {
        [HideInInspector] public string TypeLine, HeaderLine;

        [TableList(IsReadOnly = false, AlwaysExpanded = true, ShowPaging = true, NumberOfItemsPerPage = 30)]
        public List<ItemRow> Rows = new List<ItemRow>();

        public static ItemTable Load()
        {
            var t = new ItemTable();
            string[] lines = File.ReadAllText(ProjectBDataWindow.ItemCsv).Replace("\r", "").Split('\n');
            t.TypeLine = lines[0];
            t.HeaderLine = lines[1];
            for (int i = 2; i < lines.Length; i++)
            {
                if (lines[i].Trim().Length > 0)
                {
                    t.Rows.Add(ItemRow.FromCsv(lines[i].Split(',')));
                }
            }
            return t;
        }

        public void Save()
        {
            var sb = new StringBuilder();
            sb.Append(TypeLine).Append("\r\n").Append(HeaderLine);
            foreach (ItemRow r in Rows)
            {
                sb.Append("\r\n").Append(r.ToCsv());
            }
            File.WriteAllText(ProjectBDataWindow.ItemCsv, sb.ToString(), new UTF8Encoding(false));
            Debug.Log("[PB][Data] ItemSheet.csv 저장 (" + Rows.Count + "행)");
        }
    }

    // ───────────── SetItem.csv ─────────────

    [System.Serializable]
    public class SetRow
    {
        [HorizontalGroup, HideLabel] public string Key;
        [HorizontalGroup, HideLabel] public string Effect;

        [ShowInInspector, ReadOnly, LabelText("해석")]
        string Parsed => string.Join("  |  ", SetEffectApplier.Parse(Effect).Select(e => (e.value >= 0 ? "+" : "") + e.value + " " + e.stat + (e.skill != null ? " [" + e.skill + "]" : "")));
    }

    public class SetTable
    {
        [HideInInspector] public string TypeLine, HeaderLine;

        [TableList(AlwaysExpanded = true)]
        public List<SetRow> Rows = new List<SetRow>();

        public static SetTable Load()
        {
            var t = new SetTable();
            string[] lines = File.ReadAllText(ProjectBDataWindow.SetCsv).Replace("\r", "").Split('\n');
            t.TypeLine = lines[0];
            t.HeaderLine = lines[1];
            for (int i = 2; i < lines.Length; i++)
            {
                string[] c = lines[i].Split(',');
                if (c.Length >= 2)
                {
                    t.Rows.Add(new SetRow { Key = c[0].Trim(), Effect = c[1].Trim() });
                }
            }
            return t;
        }

        public void Save()
        {
            var sb = new StringBuilder(TypeLine + "\r\n" + HeaderLine);
            foreach (SetRow r in Rows)
            {
                sb.Append("\r\n").Append(r.Key).Append(',').Append(r.Effect);
            }
            File.WriteAllText(ProjectBDataWindow.SetCsv, sb.ToString(), new UTF8Encoding(false));
        }
    }

    // ───────────── 검증 리포트 ─────────────

    public class ValidationReport
    {
        [ShowInInspector, ReadOnly, MultiLineProperty(30), HideLabel]
        string Report;

        public ValidationReport(ItemTable items, SetTable sets)
        {
            Report = Build(items, sets);
        }

        /// <summary>프로브·테스트에서도 쓰는 검증 (문제 목록 문자열)</summary>
        public static string Build(ItemTable items, SetTable sets)
        {
            var sb = new StringBuilder();
            var valid = items.Rows.Where(r => r.Name != "-").ToList();
            foreach (var dup in valid.GroupBy(r => r.Name).Where(g => g.Count() > 1))
            {
                sb.AppendLine("❌ 이름 중복: " + dup.Key);
            }
            foreach (ItemRow r in valid)
            {
                foreach (string p in r.Problems())
                {
                    sb.AppendLine("⚠ [" + r.Index + "] " + r.Name + " — " + p);
                }
            }
            var tfNames = new HashSet<string>(valid.Where(r => !string.IsNullOrEmpty(r.TfName)).Select(r => r.TfName));
            foreach (SetRow s in sets.Rows)
            {
                string setKey = s.Key.Contains("_") ? s.Key.Substring(0, s.Key.LastIndexOf('_')) : s.Key;
                if (!tfNames.Contains(setKey)) sb.AppendLine("❌ 세트 " + s.Key + ": 해당 tfName을 가진 아이템 없음");
                if (SetEffectApplier.Parse(s.Effect).Count == 0) sb.AppendLine("❌ 세트 " + s.Key + ": 효과 해석 실패");
            }
            return sb.Length == 0 ? "문제 없음 ✅" : sb.ToString();
        }
    }
}
#endif
