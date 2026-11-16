// 가방 가득 + 발밑 드롭 → "가방이 가득 찼습니다" 안내(주말1 버그 2). 플레이 중. 1단계: 준비·드롭. 2단계(bagfull_drop_read): 문구 읽기
var inv = InventoryManager.instance; var cat = CsvItemCatalog.Shared;
inv.maxBoxNum = 15; int per = inv.maxHor * inv.maxVer;
var bag = new SlotContainer(cat, 5 * per, 15);
for (int i = 0; i < 5 * per; i++) { bag.PutAt(i, new ItemStack("RustedSword", 1, 0, i + 1)); }
inv.Service.LoadFrom(SaveMapper.ToArray(bag, 5, per), ItemFlagSave.ToArray(bag, 5, per));
var dm = UnityEngine.Object.FindFirstObjectByType<DropManager>(UnityEngine.FindObjectsInactive.Include);
var go = dm.GetGo("");
var pp = PlayerController.instance.transform.position;
go.transform.position = pp;
var d = go.GetComponent<DropItemCheck>(); d.SetItem("Wood");
typeof(DropItemCheck).GetMethod("GroundCheckOn", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(d, null);
return "room=" + inv.HasRoomFor("Wood") + " dropAt=" + pp;
