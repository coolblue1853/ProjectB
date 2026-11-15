// 다음 플레이 시작 기록 출처를 "__SRC__" 로(사람 폴더에 빈 세션을 만들지 않게). 플레이 전에 부른다
UnityEditor.SessionState.SetString(PlayRecorder.PendingSourceKey, "__SRC__");
return "pending source=__SRC__";
