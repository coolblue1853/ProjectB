// OnItemAdded 이벤트가 획득 경로(AddSource)와 함께 오는지
var got = new System.Collections.Generic.List<string>();
System.Action<ItemStack, AddSource> h = (s, src) => got.Add(s.Id + "/" + src);
InventoryService.OnItemAdded += h;
var inv = InventoryManager.instance;
inv.CreatItem("Wood", false, null, AddSource.Pickup);
inv.CreatItem("Wood", false, null, AddSource.Craft);
InventoryService.OnItemAdded -= h;
inv.DeletItemByName("Wood", 2);
return string.Join(", ", got.ToArray()) + " | Wood=" + inv.CountItem("Wood");
