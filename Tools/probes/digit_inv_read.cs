var inv = InventoryManager.instance;
return "open=" + inv.inventory.activeSelf + " box=" + inv.nowBox + " potions=" + inv.CountItem("Small HP Potion") + " state=" + inv.state;
