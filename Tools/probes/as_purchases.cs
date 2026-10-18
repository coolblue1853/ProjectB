// 에디터(로그인 필요)에서 Asset Store 구매 목록을 받아 AppDomain 데이터("pb_purchases")에 저장. as_purchases_read.cs로 읽는다.
var asm = typeof(Editor).Assembly;
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var scType = asm.GetType("UnityEditor.PackageManager.UI.Internal.ServicesContainer");
var sc = scType.BaseType.GetProperty("instance", B).GetValue(null, null);
var apiType = asm.GetType("UnityEditor.PackageManager.UI.Internal.AssetStoreRestAPI");
var api = scType.GetMethod("Resolve", B).MakeGenericMethod(apiType).Invoke(sc, null);
var qType = asm.GetType("UnityEditor.PackageManager.UI.Internal.PurchasesQueryArgs");
var q = qType.GetConstructors(B)[0].Invoke(new object[]{ 0, 200, "", null });
var getP = apiType.GetMethod("GetPurchases", B);
var ps = getP.GetParameters();
System.Func<System.Type, string, System.Delegate> mk = (actionType, key) => {
    var argType = actionType.GetGenericArguments()[0];
    var p = System.Linq.Expressions.Expression.Parameter(argType, "x");
    var setData = typeof(System.AppDomain).GetMethod("SetData", new[]{ typeof(string), typeof(object) });
    var body = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(System.AppDomain.CurrentDomain), setData,
        System.Linq.Expressions.Expression.Constant(key), System.Linq.Expressions.Expression.Convert(p, typeof(object)));
    return System.Linq.Expressions.Expression.Lambda(actionType, body, p).Compile();
};
System.AppDomain.CurrentDomain.SetData("pb_purchases", null);
System.AppDomain.CurrentDomain.SetData("pb_error", null);
getP.Invoke(api, new object[]{ q, mk(ps[1].ParameterType, "pb_purchases"), mk(ps[2].ParameterType, "pb_error") });
return "requested";
