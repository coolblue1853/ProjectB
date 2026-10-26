// 마을 정지 촬영(에디트·플레이 모드 공용): Town 맵을 임시 카메라로 렌더해 Logs/town/__NAME__.png 저장.
// 중심 x=__X__, 640x360 화면 기준 1080p(×3) 비율, 세로 반폭 __H__ 유닛. 전역광은 지금 씬 상태 그대로.
var main = Camera.main;
var go = new GameObject("TownShotCam"); var cam = go.AddComponent<Camera>();
if (main != null) { cam.CopyFrom(main); }
cam.orthographic = true; cam.orthographicSize = __H__f;
cam.transform.position = new Vector3(__X__f, __Y__f, -10f);
int w = 1920, h = 1080;
var rt = new RenderTexture(w, h, 24); rt.filterMode = FilterMode.Point; cam.targetTexture = rt; cam.Render();
var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); RenderTexture.active = null;
cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(go); rt.Release();
System.IO.Directory.CreateDirectory("Logs/town");
System.IO.File.WriteAllBytes("Logs/town/__NAME__.png", tex.EncodeToPNG());
return "saved Logs/town/__NAME__.png main=" + (main != null ? main.orthographicSize.ToString("F2") : "none");
