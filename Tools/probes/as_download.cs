// Asset Store 패키지 다운로드(창 없이). ID 목록은 __IDS__ 치환. 로그인 필요. 받은 파일은 %APPDATA%/Unity/Asset Store-5.x 에 캐시된다
var asm = typeof(Editor).Assembly;
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var scType = asm.GetType("UnityEditor.PackageManager.UI.Internal.ServicesContainer");
var sc = scType.BaseType.GetProperty("instance", B).GetValue(null, null);
var dmType = asm.GetType("UnityEditor.PackageManager.UI.Internal.AssetStoreDownloadManager");
var dm = scType.GetMethod("Resolve", B).MakeGenericMethod(dmType).Invoke(sc, null);
var ids = new System.Collections.Generic.List<long>(new long[]{ __IDS__ });
System.Reflection.MethodInfo dl = null;
foreach (var m in dmType.GetMethods(B)) if (m.Name == "Download" && m.GetParameters()[0].ParameterType != typeof(long)) dl = m;
dl.Invoke(dm, new object[]{ ids });
return "download requested: " + ids.Count + " inProgress=" + dmType.GetMethod("DownloadInProgressCount", B).Invoke(dm, null);
