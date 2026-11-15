// B0 입력 재생: __PATH__ 의 [__FROM__, __FROM__+__SEC__) 초 구간 in 행을 가상 키보드로 재생
return "steps=" + BotReplay.Play("__PATH__", __FROM__f, __SEC__f) + " rec=" + PlayRecorder.FilePath;
