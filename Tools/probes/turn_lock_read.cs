// turn_lock_test.cs 결과 읽기
var d = System.AppDomain.CurrentDomain.GetData("pb_turnlock");
return d == null ? "none" : d.ToString();
