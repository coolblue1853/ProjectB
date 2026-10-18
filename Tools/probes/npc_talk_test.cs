// 마을 NPC 대화: 플레이어를 장로 옆으로 옮기고 위 방향키 → Village/Elder 시작. 먼저 input_bg_on. 결과 pb_npc
var kb = UnityEngine.InputSystem.Keyboard.current;
var npc = GameObject.Find("NPC_Elder");
if (npc == null) return "no npc (town inactive?)";
var pc = PlayerController.instance;
pc.transform.position = npc.transform.position + new Vector3(1.2f, 0.3f, 0f);
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_npc", log);
DG.Tweening.DOVirtual.DelayedCall(0.5f, () => log.Append("prompt=" + npc.transform.Find("TalkPrompt").gameObject.activeSelf + "; "), true);
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.UpArrow)), true);
DG.Tweening.DOVirtual.DelayedCall(0.68f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => log.Append("active=" + PixelCrushers.DialogueSystem.DialogueManager.isConversationActive + " conv=" + PixelCrushers.DialogueSystem.DialogueManager.lastConversationStarted), true);
return "scheduled";
