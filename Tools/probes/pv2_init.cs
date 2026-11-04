Time.timeScale = 1f; Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) TimeChange.instance.LockDaytime = true;
var pc = PlayerController.instance;
return "pos=" + pc.transform.position + " day=" + TimeChange.instance.isDaytime + " bag=" + BagExpansion.Unlocked + " gold=" + (InventoryManager.instance != null) + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
