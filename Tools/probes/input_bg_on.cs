// 테스트용: 에디터가 최소화·비포커스여도 키 입력을 받게 함 (장치 재활성 포함). 끝나면 input_bg_off
var s = UnityEngine.InputSystem.InputSystem.settings;
var prev = s.backgroundBehavior + "/" + s.editorInputBehaviorInPlayMode;
s.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
s.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
UnityEngine.InputSystem.InputSystem.EnableDevice(UnityEngine.InputSystem.Keyboard.current);
return "kbEnabled=" + UnityEngine.InputSystem.Keyboard.current.enabled + " prev=" + prev + " asset=" + AssetDatabase.GetAssetPath(s);
