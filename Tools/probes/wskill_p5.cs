// WSKILL-V2 P5 검증: 스킬창 열기·탭 전환·투자·상한 표시 텍스트. 플레이 모드, 세이브는 호출자가 백업/복원
var sb = new System.Text.StringBuilder();
sb.AppendLine("kind=" + SkillTreeEffects.CurrentKind() + " equipped=" + (Weapon.Equipped != null ? Weapon.Equipped.name : "none") + " lv=" + LevelSystem.Level);
TypeMasteryService.Data.RaiseTo("Sword", TypeMasteryRules.TotalExpForLevel(8));
SkillTreeWindow.Toggle();
sb.AppendLine("open=" + SkillTreeWindow.IsOpen);
var texts = UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);
foreach (var t in texts) { if (t.transform.root.name == "[SkillTreeWindow]" && !string.IsNullOrEmpty(t.text)) { sb.AppendLine("  " + t.name + ": " + t.text.Replace("\n", " | ")); } }
return sb.ToString();
