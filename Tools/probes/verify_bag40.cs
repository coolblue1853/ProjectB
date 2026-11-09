var im = UnityEngine.Object.FindFirstObjectByType<InventoryManager>();
return "maxBoxNum=" + (im != null ? im.maxBoxNum : -1) + " start=" + BagExpansionRules.StartSlots + " T=" + Loc.T("Abyss Bolt") + "|" + Loc.T("Increases movement speed by 60%.");
