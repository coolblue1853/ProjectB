// 기록된 프레임 중 가장 느린 프레임에서 self time 상위 샘플 (스파이크 원인 분석)
var last = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
var first = Mathf.Max(UnityEditorInternal.ProfilerDriver.firstFrameIndex, last - 300);
int worst = -1; float worstMs = 0;
for (int f = first; f <= last; f++)
{
    using (var v = UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(f, 0))
    {
        if (v == null || !v.valid) continue;
        float ms = v.frameTimeMs;
        if (ms > worstMs) { worstMs = ms; worst = f; }
    }
}
if (worst < 0) return "no frames";
var sb = new System.Text.StringBuilder("worst frame " + worst + " = " + worstMs.ToString("F1") + "ms\n");
using (var view = UnityEditorInternal.ProfilerDriver.GetHierarchyFrameDataView(worst, 0, UnityEditor.Profiling.HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, UnityEditor.Profiling.HierarchyFrameDataView.columnSelfTime, false))
{
    var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, float>>();
    var stack = new System.Collections.Generic.Stack<int>(); var ch = new System.Collections.Generic.List<int>();
    stack.Push(view.GetRootItemID());
    while (stack.Count > 0)
    {
        int id = stack.Pop(); view.GetItemChildren(id, ch);
        foreach (var c in ch) stack.Push(c);
        if (id == view.GetRootItemID()) continue;
        float self = view.GetItemColumnDataAsFloat(id, UnityEditor.Profiling.HierarchyFrameDataView.columnSelfTime);
        if (self > 5f) items.Add(new System.Collections.Generic.KeyValuePair<string, float>(view.GetItemPath(id), self));
    }
    items.Sort((a, b) => b.Value.CompareTo(a.Value));
    for (int i = 0; i < items.Count && i < 15; i++) { var p = items[i].Key; if (p.Length > 170) p = "..." + p.Substring(p.Length - 170); sb.AppendLine(items[i].Value.ToString("F1") + "ms  " + p); }
}
return sb.ToString();
