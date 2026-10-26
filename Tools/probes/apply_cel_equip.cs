// 플레이 중: 주인공 셀에 던파식 장비 레이어 적용(CelEquipmentPlan) — __IDS__ 쉼표 목록(Resources/PixelRig/Equip/<id>), 빈 값이면 맨몸
var rig = PlayerController.instance.Rig as PixelRig;
var list = new System.Collections.Generic.List<CelEquipLayer>();
foreach (var id in "__IDS__".Split(',')) { if (id.Length == 0) continue; var a = Resources.Load<CelEquipLayer>("PixelRig/Equip/" + id); if (a != null) list.Add(a); }
var sw = System.Diagnostics.Stopwatch.StartNew();
rig.SetEquipmentLayers(list);
return "layers " + list.Count + " composed in " + sw.ElapsedMilliseconds + "ms";
