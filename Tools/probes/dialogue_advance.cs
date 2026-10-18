// 대사 진행 검증: 0.4초마다 Z 를 눌러(타자기 스킵 → 다음 줄) 대화가 끝나고 입력 잠금이 풀리는지. 먼저 input_bg_on. 결과 pb_dlg
var kb = UnityEngine.InputSystem.Keyboard.current;
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_dlg", log);
for (int k = 0; k < 16; k++)
{
    float t = 0.4f * k;
    DG.Tweening.DOVirtual.DelayedCall(t, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Z)), true);
    DG.Tweening.DOVirtual.DelayedCall(t + 0.08f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
}
DG.Tweening.DOVirtual.DelayedCall(7f, () => log.Append("active=" + PixelCrushers.DialogueSystem.DialogueManager.isConversationActive + " openUI=" + DatabaseManager.isOpenUI), true);
return "scheduled";
