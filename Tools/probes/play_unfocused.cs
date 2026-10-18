// Game 뷰를 "Play Unfocused"로 설정해 플레이 진입 시 포커스를 빼앗지 않게 한다.
var t = System.Type.GetType("UnityEditor.PlayModeView, UnityEditor") ;
var gvType = System.Type.GetType("UnityEditor.GameView, UnityEditor");
var sb = new System.Text.StringBuilder();
foreach (var w in Resources.FindObjectsOfTypeAll(gvType))
{
    foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
    {
        if (p.Name == "enterPlayModeBehavior")
        {
            var enumType = p.PropertyType;
            p.SetValue(w, System.Enum.Parse(enumType, "PlayUnfocused"));
            sb.Append("set " + p.GetValue(w) + "; ");
        }
    }
}
return sb.Length > 0 ? sb.ToString() : "property not found";
