// 코스 자동 주행(플레이 중): 시작 표지에서 → 방향키 + 점프 키(지상에서는 눌렀다 뗐다, 공중에서는 홀드). 끝나면 "[GlideRes] auto ..." 이름
var kb = UnityEngine.InputSystem.Keyboard.current;
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var rb = pc.GetComponent<Rigidbody2D>();
var m = GlideMotor.Instance;
var cs = UnityEngine.Object.FindFirstObjectByType<GlideCourse>();
cs.gameObject.SetActive(false); cs.gameObject.SetActive(true);
var res = new UnityEngine.GameObject("[GlideRes] auto running");
pc.transform.position = new UnityEngine.Vector3(-433f, 0.5f, pc.transform.position.z);
rb.linearVelocity = UnityEngine.Vector2.zero;
var K = UnityEngine.InputSystem.Key.RightArrow; var J = UnityEngine.InputSystem.Key.C;
int n = 0; bool done = false; int glideSteps = 0; float maxY = -99f; string trace = "";
DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
for (int i = 0; i < 2400; i++)
{
    int k = i;
    DG.Tweening.TweenSettingsExtensions.AppendInterval(seq, 0.05f);
    DG.Tweening.TweenSettingsExtensions.AppendCallback(seq, () =>
    {
        if (done) return;
        bool jump = !m.Grounded || (k % 2 == 1); bool stay = false; var px = pc.transform.position; foreach (var z in cs.Zones) { if (UnityEngine.Mathf.Abs(px.x - z.transform.position.x) < 1.5f && px.y < z.transform.position.y + z.Height - 2.5f) stay = true; }
        var ks = stay ? new UnityEngine.InputSystem.Key[] { J } : (jump ? new UnityEngine.InputSystem.Key[] { K, J } : new UnityEngine.InputSystem.Key[] { K });
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(ks));
        if (m.Gliding) glideSteps++;
        if (pc.transform.position.y > maxY) maxY = pc.transform.position.y;
        if (k % 100 == 0) trace += " [" + k + ":" + pc.transform.position.x.ToString("F0") + "," + pc.transform.position.y.ToString("F0") + (m.Gliding ? "G" : "") + "]";
        if ((cs.HasResult && k > 20) || k == 2399)
        {
            done = true;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
            res.name = "[GlideRes] auto result=" + cs.HasResult + " grade=" + cs.LastGrade + " reward=" + cs.LastReward + " t=" + cs.Elapsed.ToString("F1") + " falls=" + cs.Falls + " sent=" + cs.Sentences + " glideSteps=" + glideSteps + " maxY=" + maxY.ToString("F1") + trace;
        }
    });
}
return "auto started";
