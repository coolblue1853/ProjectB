// 플레이 중인지(다른 작업의 캡처 등) 확인: 플레이 중이면 컴파일·플레이 테스트를 미룬다
return UnityEditor.EditorApplication.isPlaying ? "playing" : (UnityEditor.EditorApplication.isCompiling ? "compiling" : "idle");
