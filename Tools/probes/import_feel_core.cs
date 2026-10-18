// Feel 핵심만 임포트 (데모·햅틱 샘플 제외한 필터 패키지). 경로가 없으면 아무것도 하지 않는다(빈 경로 ImportPackage 는 "액세스 거부" 창을 띄운다)
// 결과는 AppDomain "pb_import" 에 남는다(완료·실패·취소 이벤트)
string path = "C:/Users/user/AppData/Local/Temp/claude/C--Users-user-ProjectB/7f303872-6cc7-4749-abaf-5659414b9850/scratchpad/Feel_core.unitypackage";
if (!System.IO.File.Exists(path)) return "missing " + path;
System.AppDomain.CurrentDomain.SetData("pb_import", "started");
AssetDatabase.importPackageCompleted += n => System.AppDomain.CurrentDomain.SetData("pb_import", "completed " + n);
AssetDatabase.importPackageFailed += (n, e) => System.AppDomain.CurrentDomain.SetData("pb_import", "failed " + n + ": " + e);
AssetDatabase.importPackageCancelled += n => System.AppDomain.CurrentDomain.SetData("pb_import", "cancelled " + n);
AssetDatabase.ImportPackage(path, false);
return "import requested";
