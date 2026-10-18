// 프로파일러에 기록된 최근 프레임들에서 GC Alloc이 큰 샘플(함수) 상위 목록. 사전에 profiler_start 후 몇 초 플레이할 것.
var last = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
var first = Mathf.Max(UnityEditorInternal.ProfilerDriver.firstFrameIndex, last - 120);
var totals = new System.Collections.Generic.Dictionary<string, long>();
var calls = new System.Collections.Generic.Dictionary<string, int>();
int framesRead = 0;
for (int f = first; f <= last; f++)
{
    using (var view = UnityEditorInternal.ProfilerDriver.GetHierarchyFrameDataView(f, 0, UnityEditor.Profiling.HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName | UnityEditor.Profiling.HierarchyFrameDataView.ViewModes.HideEditorOnlySamples, UnityEditor.Profiling.HierarchyFrameDataView.columnGcMemory, false))
    {
        if (view == null || !view.valid) continue;
        framesRead++;
        var stack = new System.Collections.Generic.Stack<int>();
        var children = new System.Collections.Generic.List<int>();
        stack.Push(view.GetRootItemID());
        while (stack.Count > 0)
        {
            int id = stack.Pop();
            view.GetItemChildren(id, children);
            bool hasAllocChild = false;
            foreach (var c in children)
            {
                float gc = view.GetItemColumnDataAsFloat(c, UnityEditor.Profiling.HierarchyFrameDataView.columnGcMemory);
                if (gc > 0) { stack.Push(c); hasAllocChild = true; }
            }
            if (!hasAllocChild && id != view.GetRootItemID())
            {
                // 잎(자기 할당) 기준: 경로 이름
                string path = view.GetItemPath(id);
                long gcSelf = (long)view.GetItemColumnDataAsFloat(id, UnityEditor.Profiling.HierarchyFrameDataView.columnGcMemory);
                long cur; totals.TryGetValue(path, out cur); totals[path] = cur + gcSelf;
                int cc; calls.TryGetValue(path, out cc); calls[path] = cc + 1;
            }
        }
    }
}
var list = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, long>>(totals);
list.Sort((a, b) => b.Value.CompareTo(a.Value));
var sb = new System.Text.StringBuilder("frames=" + framesRead + "\n");
for (int i = 0; i < list.Count && i < 25; i++)
{
    var p = list[i].Key; if (p.Length > 150) p = "..." + p.Substring(p.Length - 150);
    sb.AppendLine((list[i].Value / Mathf.Max(1, framesRead) / 1024f).ToString("F2") + " KB/frame  " + p);
}
return sb.ToString();
