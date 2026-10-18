// input_bg_on 복구 (프로젝트 기본값)
var s = UnityEngine.InputSystem.InputSystem.settings;
s.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices;
s.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.PointersAndKeyboardsRespectGameViewFocus;
return "restored";
