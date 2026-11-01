using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>단일 이미지·가로 시트의 참조만 생성한다. 그림·기존 FX 에셋은 변경하지 않는다.</summary>
[InitializeOnLoad]
public class TempSkillFxImporter : AssetPostprocessor
{
    public const string Folder = "Assets/Art/Temp/FX";
    public const string CatalogPath = "Assets/Resources/FX/TempSkillFxLibrary.asset";

    static TempSkillFxImporter()
    {
        EditorApplication.delayCall += Rebuild;
    }

    private void OnPreprocessTexture()
    {
        if (!IsSlot(assetPath))
        {
            return;
        }
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 8192;
    }

    private static bool IsSlot(string path)
    {
        return path.StartsWith(Folder + "/", StringComparison.Ordinal) &&
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
    }

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (Array.Exists(imported, IsSlot) || Array.Exists(deleted, IsSlot) ||
            Array.Exists(moved, IsSlot) || Array.Exists(movedFrom, IsSlot))
        {
            EditorApplication.delayCall -= Rebuild;
            EditorApplication.delayCall += Rebuild;
        }
    }

    public static void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return; // 플레이 정지 후 refresh: 라이브 클립 파괴·카탈로그 변경 방지
        }
        var catalog = AssetDatabase.LoadAssetAtPath<TempSkillFxLibrary>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<TempSkillFxLibrary>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        var entries = new List<TempSkillFxLibrary.Entry>();
        var duplicates = new HashSet<string>(StringComparer.Ordinal);
        if (Directory.Exists(Folder))
        {
            string[] paths = Directory.GetFiles(Folder, "*.png");
            Array.Sort(paths, StringComparer.Ordinal);
            foreach (string path in paths)
            {
                if (!TempSkillFxLibrary.TryParseFile(path, out string id, out int count))
                {
                    Debug.LogWarning("임시 FX 파일명 오류: " + path);
                    continue;
                }
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path.Replace('\\', '/'));
                if (texture == null || texture.width % count != 0)
                {
                    Debug.LogWarning("임시 FX 가로 시트 폭 오류: " + path);
                    continue;
                }
                if (entries.Exists(e => e.skillId == id))
                {
                    duplicates.Add(id);
                    Debug.LogWarning("임시 FX 중복 ID, 기존 FX 사용: " + id);
                }
                entries.Add(new TempSkillFxLibrary.Entry { skillId = id, texture = texture, frameCount = count });
            }
        }
        entries.RemoveAll(e => duplicates.Contains(e.skillId));
        catalog.ClearClips();
        catalog.entries = entries;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssetIfDirty(catalog);
    }
}
