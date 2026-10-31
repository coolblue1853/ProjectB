// wtype_test / wfix_test 가 AppDomain "pb_wt" 에 쌓은 로그를 읽는다
var l = System.AppDomain.CurrentDomain.GetData("pb_wt");
return l == null ? "no log" : l.ToString();
