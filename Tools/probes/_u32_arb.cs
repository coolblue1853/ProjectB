// U32 중재: 플레이어 근처 더미 2개(거리 1·3) + 같은 자리 2개로 Contend 판정 → Logs/playtest/u32.txt
var pc = PlayerController.instance; if (pc == null) return "no player";
var a = new GameObject("arbA"); var b = new GameObject("arbB"); var c = new GameObject("arbC");
Vector3 p = pc.transform.position;
a.transform.position = p + Vector3.right * 0.05f; b.transform.position = p + Vector3.right * 0.9f; c.transform.position = p + Vector3.right * 0.06f;
InteractArbiter.Clear();
var sb = new System.Text.StringBuilder(); int n = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    bool ra = InteractArbiter.Contend(a, a.transform.position, true);
    bool rb = InteractArbiter.Contend(b, b.transform.position, true);
    bool rc = InteractArbiter.Contend(c, c.transform.position, true);
    n++; if (n > 3) sb.AppendLine("f" + n + " near(1.0)=" + ra + " far(3.0)=" + rb + " co-located(1.1)=" + rc);
    if (n >= 8) { sb.AppendLine("player=" + p); System.IO.File.WriteAllText("Logs/playtest/u32.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; UnityEngine.Object.DestroyImmediate(a); UnityEngine.Object.DestroyImmediate(b); UnityEngine.Object.DestroyImmediate(c); InteractArbiter.Clear(); }
};
UnityEditor.EditorApplication.update += upd; return "started";
