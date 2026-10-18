#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// 보유 에셋스토어 패키지를 대화상자 없이 임포트하고 Assets/ThirdParty/&lt;Name&gt;로 정리한다.
    /// 프로브에서 호출: ThirdPartyImport.ImportAndMove(...), ThirdPartyImport.ApplyPixelSettings(...)
    /// </summary>
    public static class ThirdPartyImport
    {
        public static string CacheRoot =>
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Unity", "Asset Store-5.x");

        /// <summary>패키지 임포트 요청(비동기). 끝나면 MoveRoot로 옮긴다.</summary>
        public static bool Import(string relativePackagePath)
        {
            string full = Path.Combine(CacheRoot, relativePackagePath);
            if (!File.Exists(full))
            {
                Debug.LogWarning("[PB][Import] 패키지 없음: " + full);
                return false;
            }
            AssetDatabase.ImportPackage(full, false);
            return true;
        }

        /// <summary>임포트된 루트 폴더를 ThirdParty 아래로 이동(GUID 유지)하고, 이름에 dropKeywords가 들어간 하위 항목(데모 등)을 삭제한다.</summary>
        public static string MoveRoot(string importedRoot, string targetName, params string[] dropKeywords)
        {
            if (!AssetDatabase.IsValidFolder(importedRoot))
            {
                return "없음: " + importedRoot;
            }
            if (!AssetDatabase.IsValidFolder("Assets/ThirdParty"))
            {
                AssetDatabase.CreateFolder("Assets", "ThirdParty");
            }
            string target = "Assets/ThirdParty/" + targetName;
            if (AssetDatabase.IsValidFolder(target))
            {
                return "이미 있음: " + target;
            }
            string err = AssetDatabase.MoveAsset(importedRoot, target);
            if (!string.IsNullOrEmpty(err))
            {
                return "이동 실패: " + err;
            }

            var toDelete = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("", new[] { target }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string lower = path.ToLowerInvariant();
                bool drop = path.EndsWith(".unity") || path.EndsWith(".cs") || path.EndsWith(".unitypackage");
                foreach (string k in dropKeywords)
                {
                    if (lower.Contains(k.ToLowerInvariant()))
                    {
                        drop = true;
                    }
                }
                if (drop)
                {
                    toDelete.Add(path);
                }
            }
            // 상위 폴더가 먼저 지워지면 하위 경로는 실패하므로 짧은 경로부터
            toDelete.Sort((a, b) => a.Length.CompareTo(b.Length));
            int deleted = 0;
            foreach (string p in toDelete)
            {
                if (AssetDatabase.IsValidFolder(p) || File.Exists(p))
                {
                    if (AssetDatabase.MoveAssetToTrash(p))
                    {
                        deleted++;
                    }
                }
            }
            return target + " (정리 " + deleted + "개)";
        }

        /// <summary>도트 텍스처 공통 설정: Point, 무압축, 밉맵 끔, PPU 지정. 슬라이스는 유지한다.</summary>
        public static int ApplyPixelSettings(string folder, float ppu)
        {
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null)
                {
                    continue;
                }
                bool dirty = imp.filterMode != FilterMode.Point || imp.textureCompression != TextureImporterCompression.Uncompressed
                             || imp.mipmapEnabled || !Mathf.Approximately(imp.spritePixelsPerUnit, ppu) || imp.textureType != TextureImporterType.Sprite;
                if (!dirty)
                {
                    continue;
                }
                if (imp.textureType != TextureImporterType.Sprite)
                {
                    imp.textureType = TextureImporterType.Sprite;
                }
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.spritePixelsPerUnit = ppu;
                imp.SaveAndReimport();
                changed++;
            }
            return changed;
        }
    }
}
#endif
