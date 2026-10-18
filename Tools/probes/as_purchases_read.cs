var data = System.AppDomain.CurrentDomain.GetData("pb_purchases");
var err = System.AppDomain.CurrentDomain.GetData("pb_error");
if (data == null) return "no data yet; error=" + (err != null ? err.ToString() : "none");
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var list = (System.Collections.IEnumerable)data.GetType().GetField("list", B).GetValue(data);
var sb = new System.Text.StringBuilder("total=" + data.GetType().GetField("total", B).GetValue(data) + "\n");
foreach (var it in list) sb.Append(it.GetType().GetField("productId", B).GetValue(it) + "\t" + it.GetType().GetField("displayName", B).GetValue(it) + "\n");
return sb.ToString();
