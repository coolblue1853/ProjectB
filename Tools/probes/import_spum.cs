// SPUM 패키지 임포트(창 없이). 경로가 없으면 아무것도 하지 않는다 — 빈 경로로 ImportPackage 를 부르면 프로젝트 폴더를 열려다 "액세스 거부" 창이 뜬다
string root = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Unity", "Asset Store-5.x");
string found = null;
foreach (var f in System.IO.Directory.GetFiles(root, "2D Pixel Unit Maker - SPUM.unitypackage", System.IO.SearchOption.AllDirectories)) { found = f; break; }
if (string.IsNullOrEmpty(found) || !System.IO.File.Exists(found)) return "missing package";
AssetDatabase.ImportPackage(found, false);
return "import requested: " + found;
