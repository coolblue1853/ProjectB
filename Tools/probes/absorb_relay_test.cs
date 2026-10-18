// B-142 검증(플레이 모드): 활성 AbsorbAnimRelay 의 애니메이터가 BeforeAbsorb → AfterAbsorb 로 넘어갔는지 본다.
// 흡입 스킬(Gel Gather 등)을 쓴 직후 0.2초·1.2초에 실행한다
var sb = new System.Text.StringBuilder();
foreach (var r in UnityEngine.Object.FindObjectsByType<AbsorbAnimRelay>(FindObjectsSortMode.None))
{
    var an = r.GetComponent<Animator>();
    var st = an.GetCurrentAnimatorStateInfo(0);
    string nm = st.IsName("BeforeAbsorb") ? "BeforeAbsorb" : st.IsName("AfterAbsorb") ? "AfterAbsorb" : "other";
    sb.Append(r.transform.root.name + ":" + nm + " t" + st.normalizedTime.ToString("F2") + "; ");
}
return sb.Length == 0 ? "no relay active" : sb.ToString();
