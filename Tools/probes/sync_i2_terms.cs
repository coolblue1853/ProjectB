// LocTerms(코드 번역 표) → Resources/I2Languages.asset (English·Korean 언어 + 용어). 재실행 안전(덮어씀)
var asset = AssetDatabase.LoadAssetAtPath<I2.Loc.LanguageSourceAsset>("Assets/Resources/I2Languages.asset");
if (asset == null) return "no I2Languages.asset";
var src = asset.mSource;
if (src.GetLanguageIndex("English") < 0) src.AddLanguage("English", "en");
if (src.GetLanguageIndex("Korean") < 0) src.AddLanguage("Korean", "ko");
int en = src.GetLanguageIndex("English"), ko = src.GetLanguageIndex("Korean");
int added = 0, updated = 0;
foreach (var e in LocTerms.Entries)
{
    string term = Loc.Term(e.en);
    var td = src.GetTermData(term);
    if (td == null) { td = src.AddTerm(term); added++; } else updated++;
    td.Languages[en] = e.en;
    td.Languages[ko] = e.ko;
}
var want = new System.Collections.Generic.HashSet<string>(); foreach (var e in LocTerms.Entries) want.Add(Loc.Term(e.en));
int removed = src.mTerms.RemoveAll(t => !want.Contains(t.Term)); // 표에서 빠진 옛 키 정리
src.UpdateDictionary(true);
EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
return "I2 terms: removed " + removed + " added " + added + " updated " + updated + " total " + src.mTerms.Count + " langs " + string.Join(",", src.GetLanguages().ToArray());
