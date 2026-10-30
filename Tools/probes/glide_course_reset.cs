var cs = UnityEngine.Object.FindFirstObjectByType<GlideCourse>();
cs.gameObject.SetActive(false); cs.gameObject.SetActive(true);
return "reset running=" + cs.Running;
