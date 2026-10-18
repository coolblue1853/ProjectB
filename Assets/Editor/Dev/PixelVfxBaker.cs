#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// 픽셀 VFX 베이커: 3D 파티클 프리팹(Magic Slashes FX 등)을 화면 밖 직교 카메라로 프레임별 렌더 →
    /// 4x4 블록 평균 축소 → 밝기를 알파로(가산 파티클은 알파가 의미 없음) → 알파 컷으로 도트 외곽 →
    /// 테마 램프 N색 양자화 → 전 프레임 공통 영역으로 잘라 스프라이트 시트 + PixelFxClip 저장.
    /// 도트 밀도는 캐릭터와 같은 PPU(기본 24)로 맞춘다. 레퍼런스 측정: Tools/shader_jobs/skill_energy/REFERENCE.md
    /// </summary>
    public static class PixelVfxBaker
    {
        public const string OutDir = "Assets/Art/PixelFx";
        const int Layer = 31;
        const int Super = 4; // 슈퍼샘플 배수 (도트 1개 = 렌더 4x4)

        public class Spec
        {
            public string name;
            public string prefabPath;
            public Vector3 euler;
            public float scale = 0.35f;
            public float halfExtent = 3f;   // 렌더 영역 반폭(월드)
            public float ppu = 24f;
            public float fps = 20f;
            public int maxFrames = 12;
            public float startTime = 0.06f;
            public float lifetime = 0.6f;
            public float alphaCut = 0.22f;
            public float gain = 1f;          // 정규화 뒤 추가 배율
            public float gamma = 1f;         // >1 이면 어두운 쪽으로, <1 이면 밝은 쪽으로 램프 분포
            public float fadeFrom = 0.6f;
            public int dilate;               // 가는 선 이펙트 두께 보강(팽창 픽셀은 한 단계 어둡게 = 도트 외곽선)    // 이 비율 이후 프레임은 점점 깎여 사라짐
            public Color[] ramp;             // 어두움 → 밝음
        }

        // 메이플 히어로 레퍼런스(흰빛 중심 → 금 → 주황 → 적) 구조를 테마별로
        public static readonly Dictionary<string, Color[]> Ramps = new Dictionary<string, Color[]>
        {
            { "gold",  Hex("5A1E0A", "C8501A", "F5A032", "FADF9F", "FFFAEB") },
            { "blue",  Hex("141E5A", "285AC8", "50AAFF", "AAE1FF", "F0FAFF") },
            { "red",   Hex("460A14", "AA1E28", "F0503C", "FFAA8C", "FFF0E6") },
            { "green", Hex("0A3214", "1E8228", "5AD250", "BEFFA0", "F5FFEB") },
            { "white", Hex("3C3C50", "828CAA", "C8D2E6", "EBF0FA", "FFFFFF") },
            { "purple", Hex("28104A", "6A28B4", "B45AF0", "E1AAFF", "FAF0FF") },
        };

        static Color[] Hex(params string[] hex)
        {
            var c = new Color[hex.Length];
            for (int i = 0; i < hex.Length; i++)
            {
                ColorUtility.TryParseHtmlString("#" + hex[i], out c[i]);
            }
            return c;
        }

        public static string Bake(Spec s)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s.prefabPath);
            if (prefab == null)
            {
                return s.name + ": 프리팹 없음 " + s.prefabPath;
            }
            Directory.CreateDirectory(OutDir);

            int px = Mathf.RoundToInt(s.halfExtent * 2f * s.ppu);   // 도트 해상도(정사각)
            int hi = px * Super;
            Vector3 origin = new Vector3(5000f, 5000f, 0f);

            var root = new GameObject("__PixelVfxBake") { hideFlags = HideFlags.HideAndDontSave };
            root.transform.position = origin;
            var fx = (GameObject)Object.Instantiate(prefab, origin, Quaternion.Euler(s.euler), root.transform);
            fx.transform.localScale = Vector3.one * s.scale;
            foreach (var t in fx.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = Layer;
            }
            var systems = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems)
            {
                var main = ps.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.useUnscaledTime = false;
            }

            var camGo = new GameObject("__PixelVfxCam") { hideFlags = HideFlags.HideAndDontSave };
            camGo.transform.position = origin + new Vector3(0f, 0f, -20f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = s.halfExtent;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.cullingMask = 1 << Layer;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.enabled = false;

            var rt = new RenderTexture(hi, hi, 24, RenderTextureFormat.ARGBHalf) { antiAliasing = 1 };
            cam.targetTexture = rt;
            var read = new Texture2D(hi, hi, TextureFormat.RGBAHalf, false, true);

            int frames = Mathf.Clamp(Mathf.CeilToInt(s.lifetime * s.fps), 2, s.maxFrames);
            var lum = new List<float[]>();   // 프레임별 도트 밝기(0..1, 알파컷 전)
            try
            {
                for (int f = 0; f < frames; f++)
                {
                    float t = s.startTime + f / s.fps;
                    foreach (var ps in systems)
                    {
                        if (ps.transform.parent == null || ps.transform.parent.GetComponentInParent<ParticleSystem>() == null)
                        {
                            ps.Simulate(t, true, true, false); // 루트 시스템만 재시작 시뮬레이션 (자식 포함)
                        }
                    }
                    RenderOnce(cam, rt);
                    var prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    read.ReadPixels(new Rect(0, 0, hi, hi), 0, 0);
                    read.Apply();
                    RenderTexture.active = prev;
                    lum.Add(Downsample(read.GetPixels(), hi, px, 1f));
                }
            }
            finally
            {
                cam.targetTexture = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(read);
                Object.DestroyImmediate(camGo);
                Object.DestroyImmediate(root);
            }

            PostProcess(lum, px, s);

            // 전 프레임 공통 영역(1px 여백)
            int minX = px, minY = px, maxX = -1, maxY = -1;
            foreach (var l in lum)
            {
                for (int y = 0; y < px; y++)
                {
                    for (int x = 0; x < px; x++)
                    {
                        if (l[y * px + x] >= s.alphaCut)
                        {
                            minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                        }
                    }
                }
            }
            if (maxX < 0)
            {
                return s.name + ": 렌더 결과가 비어 있음 (스케일·범위·시간 확인)";
            }
            minX = Mathf.Max(0, minX - 1); minY = Mathf.Max(0, minY - 1);
            maxX = Mathf.Min(px - 1, maxX + 1); maxY = Mathf.Min(px - 1, maxY + 1);
            int w = maxX - minX + 1, h = maxY - minY + 1;

            var sheet = new Texture2D(w * frames, h, TextureFormat.RGBA32, false);
            var clear = new Color32[w * frames * h];
            sheet.SetPixels32(clear);
            int lit = 0;
            for (int f = 0; f < frames; f++)
            {
                float[] l = lum[f];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float v = l[(y + minY) * px + (x + minX)];
                        if (v < s.alphaCut)
                        {
                            continue;
                        }
                        // 컷 이상 구간을 램프 인덱스로 균등 분할 (밝을수록 흰빛)
                        float k = Mathf.InverseLerp(s.alphaCut, 1f, v);
                        int idx = Mathf.Clamp(Mathf.FloorToInt(k * s.ramp.Length), 0, s.ramp.Length - 1);
                        sheet.SetPixel(f * w + x, y, s.ramp[idx]);
                        lit++;
                    }
                }
            }
            sheet.Apply();

            string pngPath = OutDir + "/" + s.name + ".png";
            File.WriteAllBytes(pngPath, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);

            // 피벗 = 원래 이펙트 중심(렌더 영역 중앙)이 잘린 영역 안에서 놓이는 위치
            Vector2 pivot = new Vector2((px * 0.5f - minX) / w, (px * 0.5f - minY) / h);
            Sprite[] sprites = SliceSheet(pngPath, frames, w, h, pivot, s.ppu);

            string clipPath = OutDir + "/" + s.name + ".asset";
            var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(clipPath);
            if (clip == null)
            {
                clip = ScriptableObject.CreateInstance<PixelFxClip>();
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            clip.frames = sprites;
            clip.fps = s.fps;
            clip.source = s.prefabPath + " euler=" + s.euler + " scale=" + s.scale;
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            return s.name + ": " + frames + "f " + w + "x" + h + "px lit=" + lit;
        }

        /// <summary>정규화(상위 2% 밝기 = 1) → 감마 → 후반 프레임 소멸 엔벨로프 → 외톨이 픽셀 제거</summary>
        static void PostProcess(List<float[]> lum, int px, Spec s)
        {
            var all = new List<float>();
            foreach (var l in lum)
            {
                foreach (float v in l)
                {
                    if (v > 0.02f)
                    {
                        all.Add(v);
                    }
                }
            }
            if (all.Count == 0)
            {
                return;
            }
            all.Sort();
            float p98 = Mathf.Max(0.05f, all[Mathf.Min(all.Count - 1, (int)(all.Count * 0.98f))]);
            int n = lum.Count;
            for (int f = 0; f < n; f++)
            {
                float life = n <= 1 ? 0f : (float)f / (n - 1);
                float env = life <= s.fadeFrom ? 1f : Mathf.Lerp(1f, 0.25f, (life - s.fadeFrom) / (1f - s.fadeFrom));
                float[] l = lum[f];
                for (int i = 0; i < l.Length; i++)
                {
                    l[i] = Mathf.Pow(Mathf.Clamp01(l[i] / p98 * s.gain), s.gamma) * env;
                }
                // 외톨이 픽셀 제거 (8이웃 중 컷 이상이 1개 이하)
                var copy = (float[])l.Clone();
                for (int y = 1; y < px - 1; y++)
                {
                    for (int x = 1; x < px - 1; x++)
                    {
                        if (copy[y * px + x] < s.alphaCut)
                        {
                            continue;
                        }
                        int nb = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if ((dx != 0 || dy != 0) && copy[(y + dy) * px + x + dx] >= s.alphaCut)
                                {
                                    nb++;
                                }
                            }
                        }
                        if (nb <= 1)
                        {
                            l[y * px + x] = 0f;
                        }
                    }
                }
                for (int d = 0; d < s.dilate; d++)
                {
                    Dilate(l, px, s.alphaCut);
                }
            }
        }

        /// <summary>빈 픽셀 중 4이웃에 컷 이상이 있으면 이웃 최대값의 80%로 채운다 (선 두께 +1, 바깥이 한 단계 어두움)</summary>
        static void Dilate(float[] l, int px, float cut)
        {
            var copy = (float[])l.Clone();
            for (int y = 1; y < px - 1; y++)
            {
                for (int x = 1; x < px - 1; x++)
                {
                    int i = y * px + x;
                    if (copy[i] >= cut)
                    {
                        continue;
                    }
                    float m = Mathf.Max(Mathf.Max(copy[i - 1], copy[i + 1]), Mathf.Max(copy[i - px], copy[i + px]));
                    if (m >= cut)
                    {
                        l[i] = Mathf.Max(cut, m * 0.8f);
                    }
                }
            }
        }

        static void RenderOnce(Camera cam, RenderTexture rt)
        {
            var req = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(cam, req))
            {
                RenderPipeline.SubmitRenderRequest(cam, req);
            }
            else
            {
                cam.Render();
            }
        }

        /// <summary>hi x hi → px x px 블록 평균 후 밝기(max rgb, 알파 반영)를 0..1로</summary>
        static float[] Downsample(Color[] src, int hi, int px, float gain)
        {
            var outL = new float[px * px];
            float inv = 1f / (Super * Super);
            for (int y = 0; y < px; y++)
            {
                for (int x = 0; x < px; x++)
                {
                    float sum = 0f;
                    for (int dy = 0; dy < Super; dy++)
                    {
                        int row = (y * Super + dy) * hi + x * Super;
                        for (int dx = 0; dx < Super; dx++)
                        {
                            Color c = src[row + dx];
                            float m = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                            sum += Mathf.Max(m, c.a * 0.5f * m);
                        }
                    }
                    outL[y * px + x] = Mathf.Clamp01(sum * inv * gain);
                }
            }
            return outL;
        }

        static Sprite[] SliceSheet(string path, int frames, int w, int h, Vector2 pivot, float ppu)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.spritePixelsPerUnit = ppu;
#pragma warning disable 618 // spritesheet 는 Unity 6에서도 동작 (ISpriteEditorDataProvider 대체는 패키지 의존)
            var metas = new SpriteMetaData[frames];
            for (int f = 0; f < frames; f++)
            {
                metas[f] = new SpriteMetaData
                {
                    name = Path.GetFileNameWithoutExtension(path) + "_" + f,
                    rect = new Rect(f * w, 0, w, h),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = pivot,
                };
            }
            imp.spritesheet = metas;
#pragma warning restore 618
            imp.SaveAndReimport();
            var sprites = new List<Sprite>();
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (o is Sprite sp)
                {
                    sprites.Add(sp);
                }
            }
            sprites.Sort((a, b) => a.rect.x.CompareTo(b.rect.x));
            return sprites.ToArray();
        }
    }
}
#endif
