var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
return "scene=" + s.name + " play=" + Application.isPlaying + " pc=" + (PlayerController.instance != null) + " tc=" + (TimeChange.instance != null) + " title=" + (UnityEngine.Object.FindFirstObjectByType<TitleScreen>() != null);
