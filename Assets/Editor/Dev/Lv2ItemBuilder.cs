#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// Lv2 아이템 에셋 생성 (Lv2ContentPlan 4절·3.4절): 아이콘 임포트 설정, 장비 프리팹(방어구·무기·장신구), 세트 효과 행, Lv2 스포너 드롭 테이블.
    /// 다시 실행해도 같은 결과(멱등).
    /// </summary>
    public static class Lv2ItemBuilder
    {
        public static string Build()
        {
            var log = new StringBuilder();
            log.Append(ImportIcons()).Append(" | ");
            log.Append(BuildArmor()).Append(" | ");
            log.Append(BuildWeapons()).Append(" | ");
            log.Append(BuildAccessories()).Append(" | ");
            log.Append(AddSetRows()).Append(" | ");
            log.Append(ApplyDrops());
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        static readonly string[] NewItems =
        {
            "T1_Ore", "Cliff Fox Pelt", "Fox Meat", "Glowing Spore", "Spongy Mushroom Cap", "Goblin Fang", "Tarnished Goblin Coin",
            "Membrane Wing", "Bloodshot Lens", "Ancient Bone", "Cracked Shield Shard", "Death Sigil", "Abyssal Core", "Roasted Fox Skewer",
            "CliffCap", "CliffArmor", "CliffLegArmor", "CliffHandArmor", "CliffShoes", "CliffStaff", "CliffBlade", "AbyssStaff",
            "Eye of the Cliffs", "Bringer's Sigil Ring",
        };

        static string ImportIcons()
        {
            int n = 0;
            var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
            foreach (string item in NewItems)
            {
                n += CopyImport("Assets/Resources/Item/" + item + ".png", refImp) ? 1 : 0;
            }
            var refTex = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Sprite/LeafArmor.png");
            CopyImport("Assets/Resources/Sprite/CliffArmor.png", refTex);
            return "아이콘 " + n;
        }

        static bool CopyImport(string path, TextureImporter reference)
        {
            AssetDatabase.ImportAsset(path);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null || reference == null)
            {
                return false;
            }
            var settings = new TextureImporterSettings();
            reference.ReadTextureSettings(settings);
            imp.SetTextureSettings(settings);
            imp.textureCompression = reference.textureCompression;
            imp.filterMode = reference.filterMode;
            imp.SaveAndReimport();
            return true;
        }

        // ── 방어구: Leaf 부위 프리팹 복제 + 스탯 ──
        static string BuildArmor()
        {
            var defs = new (string area, string src, string dst, int armor, int hp, int crit, int move)[]
            {
                ("Head", "LeafCap", "CliffCap", 15, 150, 0, 0),
                ("Chest", "LeafArmor", "CliffArmor", 30, 400, 0, 0),
                ("Leg", "LeafLegArmor", "CliffLegArmor", 15, 150, 0, 0),
                ("Hand", "LeafHandArmor", "CliffHandArmor", 15, 150, 3, 0),
                ("Shoes", "LeafShoes", "CliffShoes", 15, 150, 0, 5),
            };
            int n = 0;
            foreach (var d in defs)
            {
                GameObject root = CopyPrefab("Assets/Resources/" + d.area + "/", d.src, d.dst);
                if (root == null)
                {
                    continue;
                }
                var eq = root.GetComponent<Equipment>();
                if (eq != null)
                {
                    eq.armor = d.armor; eq.hp = d.hp; eq.critical = d.crit; eq.moveSpeed = d.move;
                    eq.basicDmg = 0; eq.criticalDmg = 0; eq.incDmg = 0; eq.attSpeed = 0; eq.poisonDmg = 0; eq.isBleeding = false;
                }
                Save(root, "Assets/Resources/" + d.area + "/" + d.dst + ".prefab");
                n++;
            }
            return "방어구 " + n;
        }

        // ── 무기: Scare 무기 복제 + 데미지 + 스킬 이름(세트 효과 대상) ──
        static string BuildWeapons()
        {
            var defs = new (string src, string dst, int min, int max, int crit, int critDmg, int skillDmg, int finDmg, string[] skills)[]
            {
                ("ScareSide", "CliffBlade", 16, 24, 10, 20, 0, 0, new[] { "Gale Slash", "Cyclone Edge", "Skybreaker", "Talon Dive" }),
                ("ScareStaff", "CliffStaff", 14, 24, 5, 0, 20, 0, new[] { "Arcane Bolt", "Starfall", "Cliff Quake" }),
                ("ScareStaff", "AbyssStaff", 22, 34, 5, 0, 30, 10, new[] { "Abyss Bolt", "Abyss Rain", "Abyss Quake" }),
            };
            int n = 0;
            foreach (var d in defs)
            {
                GameObject root = CopyPrefab("Assets/Resources/Weapon/", d.src, d.dst);
                if (root == null)
                {
                    continue;
                }
                var w = root.GetComponent<Weapon>();
                w.minDmg = d.min; w.maxDmg = d.max; w.critPer = d.crit; w.critDmg = d.critDmg; w.skillDmg = d.skillDmg; w.finDmg = d.finDmg;
                Skill[] skills = root.GetComponentsInChildren<Skill>(true);
                for (int i = 0; i < skills.Length && i < d.skills.Length; i++)
                {
                    skills[i].skillName = d.skills[i];
                }
                Save(root, "Assets/Resources/Weapon/" + d.dst + ".prefab");
                n++;
            }
            return "무기 " + n;
        }

        static string BuildAccessories()
        {
            int n = 0;
            GameObject neck = CopyPrefab("Assets/Resources/Necklace/", "Green Wreath", "Eye of the Cliffs");
            if (neck != null)
            {
                var eq = neck.GetComponent<Equipment>();
                if (eq != null) { Zero(eq); eq.critical = 5; eq.dropRate = 10; }
                Save(neck, "Assets/Resources/Necklace/Eye of the Cliffs.prefab");
                n++;
            }
            GameObject ring = CopyPrefab("Assets/Resources/Ring/", "Nasty Rat Ring", "Bringer's Sigil Ring");
            if (ring != null)
            {
                var eq = ring.GetComponent<Equipment>();
                if (eq != null) { Zero(eq); eq.criticalDmg = 15; eq.incDmg = 5; }
                Save(ring, "Assets/Resources/Ring/Bringer's Sigil Ring.prefab");
                n++;
            }
            return "장신구 " + n;
        }

        static void Zero(Equipment eq)
        {
            eq.armor = 0; eq.hp = 0; eq.basicDmg = 0; eq.critical = 0; eq.criticalDmg = 0; eq.incDmg = 0; eq.dropRate = 0;
            eq.attSpeed = 0; eq.moveSpeed = 0; eq.isBleeding = false; eq.poisonDmg = 0; eq.addIncomingDmg = 0;
        }

        // ── 세트 효과: 2/4/6단계 (6단계는 무기 포함). 스킬 쿨감은 모든 스킬 방식에 적용된다 ──
        static string AddSetRows()
        {
            var t = SetTable.Load();
            var rows = new Dictionary<string, string>
            {
                { "CliffArmor_2", "(+)15 Def/(+)5 Crit" },
                { "CliffArmor_4", "(+)10 IncDmg/(+)20 coolDown Cyclone_Edge/(+)20 coolDown Starfall" },
                { "CliffArmor_6", "(+)20 CritDmg/(+)10 AtkSpeed/(+)30 coolDown Skybreaker/(+)30 coolDown Cliff_Quake/(+)30 coolDown Talon_Dive" },
            };
            foreach (var kv in rows)
            {
                SetRow existing = t.Rows.FirstOrDefault(r => r.Key == kv.Key);
                if (existing != null) existing.Effect = kv.Value;
                else t.Rows.Add(new SetRow { Key = kv.Key, Effect = kv.Value });
            }
            t.Save();
            return "세트 행 " + rows.Count;
        }

        // ── 드롭 테이블 (스포너 이름 → (아이템, 확률)) ──
        static readonly Dictionary<string, (string, float)[]> Drops = new Dictionary<string, (string, float)[]>
        {
            { "L2_A_Shroom", new[] { ("Glowing Spore", 0.15f), ("Spongy Mushroom Cap", 0.10f), ("GreenHurb", 0.02f), ("CliffCap", 0.002f) } },
            { "L2_B_ShroomUp", new[] { ("Glowing Spore", 0.15f), ("Spongy Mushroom Cap", 0.10f), ("CliffCap", 0.002f) } },
            { "L2_B_Goblin", new[] { ("Goblin Fang", 0.15f), ("Tarnished Goblin Coin", 0.05f), ("Cliff Fox Pelt", 0.08f), ("CliffLegArmor", 0.002f), ("CliffBlade", 0.0008f) } },
            { "L2_C_Goblin", new[] { ("Goblin Fang", 0.15f), ("Tarnished Goblin Coin", 0.05f), ("T1_Ore", 0.10f), ("CliffShoes", 0.002f), ("CliffBlade", 0.0008f) } },
            { "L2_B_GoblinN", new[] { ("Goblin Fang", 0.15f), ("Tarnished Goblin Coin", 0.06f), ("Fox Meat", 0.10f), ("CliffLegArmor", 0.002f) } },
            { "L2_A_EyeN", new[] { ("Membrane Wing", 0.15f), ("Bloodshot Lens", 0.05f), ("CliffStaff", 0.0008f), ("Eye of the Cliffs", 0.0005f) } },
            { "L2_B_SkelN", new[] { ("Ancient Bone", 0.15f), ("Cracked Shield Shard", 0.08f), ("T1_Ore", 0.10f), ("CliffArmor", 0.0015f), ("CliffHandArmor", 0.002f) } },
            { "L2_C_SkelN", new[] { ("Ancient Bone", 0.15f), ("Cracked Shield Shard", 0.08f), ("T1_Ore", 0.10f), ("CliffArmor", 0.0015f) } },
            { "L2_C_EliteN", new[] { ("Death Sigil", 0.6f), ("Ancient Bone", 1f), ("CliffArmor", 0.05f), ("CliffBlade", 0.04f), ("CliffStaff", 0.04f), ("Bringer's Sigil Ring", 0.03f) } },
            { "L2_D_BossN", new[] { ("Abyssal Core", 1f), ("Death Sigil", 0.5f), ("CliffCap", 0.15f), ("CliffArmor", 0.15f), ("CliffLegArmor", 0.15f), ("CliffHandArmor", 0.15f), ("CliffShoes", 0.15f), ("CliffStaff", 0.1f), ("CliffBlade", 0.1f), ("Eye of the Cliffs", 0.05f), ("AbyssStaff", 0.03f) } },
        };

        public static string ApplyDrops()
        {
            Transform lv2 = GameObject.Find("Map")?.transform.Find(Lv2MapBuilder.RootName);
            if (lv2 == null)
            {
                return "Lv2 맵 없음";
            }
            int n = 0;
            foreach (DropManager dm in lv2.GetComponentsInChildren<DropManager>(true))
            {
                if (!Drops.TryGetValue(dm.name, out var list))
                {
                    continue;
                }
                var so = new SerializedObject(dm);
                SerializedProperty arr = so.FindProperty("dropItems");
                arr.arraySize = list.Length;
                for (int i = 0; i < list.Length; i++)
                {
                    SerializedProperty e = arr.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("itemName").stringValue = list[i].Item1;
                    e.FindPropertyRelative("dropProbability").floatValue = list[i].Item2;
                    e.FindPropertyRelative("offsetY").floatValue = 0.5f;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                n++;
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(lv2.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(lv2.gameObject.scene);
            return "드롭 테이블 " + n;
        }

        static GameObject CopyPrefab(string dir, string src, string dst)
        {
            string srcPath = dir + src + ".prefab", dstPath = dir + dst + ".prefab";
            if (!File.Exists(srcPath))
            {
                Debug.LogWarning("[PB][Build] 원본 프리팹 없음: " + srcPath);
                return null;
            }
            if (!File.Exists(dstPath))
            {
                AssetDatabase.CopyAsset(srcPath, dstPath);
            }
            GameObject root = PrefabUtility.LoadPrefabContents(dstPath);
            root.name = dst;
            return root;
        }

        static void Save(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
