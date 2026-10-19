// 2026-10-19 효과음 추가(AssetIntegrationPlan 6절 12): 강화·아티팩트·지면별 발소리를 SfxLibrary 에 등록. 재실행 안전
var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>("Assets/Resources/Audio/SfxLibrary.asset");
string dir = "Assets/ThirdParty/Sfx/ProSound/";
System.Func<string, AudioClip> C = n => AssetDatabase.LoadAssetAtPath<AudioClip>(dir + n + ".ogg");
var defs = new System.Collections.Generic.List<object[]> {
    new object[]{ SfxId.EnhanceSuccess, new[]{ "chime_bell_positive_ring_02" }, 0.7f, 0.03f },
    new object[]{ SfxId.EnhanceGreat, new[]{ "chimes_magical_bells_04" }, 0.75f, 0.02f },
    new object[]{ SfxId.EnhanceFail, new[]{ "collect_item_jingle_fail_01" }, 0.6f, 0.02f },
    new object[]{ SfxId.ArtifactHeal, new[]{ "healing_magic_spell_01" }, 0.7f, 0.04f },
    new object[]{ SfxId.ArtifactShell, new[]{ "ice_spell_freeze_small_01" }, 0.7f, 0.04f },
    new object[]{ SfxId.ArtifactTime, new[]{ "chimes_magic_bell_ding_1" }, 0.75f, 0f },
    new object[]{ SfxId.ArtifactStar, new[]{ "fireball_blast_projectile_spell_02" }, 0.8f, 0.04f },
    new object[]{ SfxId.FootGrass, new[]{ "footstep_grass_run_01", "footstep_grass_run_02", "footstep_grass_run_03", "footstep_grass_run_04" }, 0.35f, 0.08f },
    new object[]{ SfxId.FootDirt, new[]{ "footstep_dirt_walk_run_01", "footstep_dirt_walk_run_02", "footstep_dirt_walk_run_03", "footstep_dirt_walk_run_04" }, 0.35f, 0.08f },
    new object[]{ SfxId.FootStone, new[]{ "footstep_concrete_run_01", "footstep_concrete_run_02", "footstep_concrete_run_03", "footstep_concrete_run_04" }, 0.35f, 0.08f },
};
var sb = new System.Text.StringBuilder();
foreach (var d in defs)
{
    var id = (SfxId)d[0]; var names = (string[])d[1];
    var e = lib.entries.Find(x => x.id == id);
    if (e == null) { e = new SfxLibrary.Entry { id = id }; lib.entries.Add(e); }
    var clips = new AudioClip[names.Length]; for (int i = 0; i < names.Length; i++) clips[i] = C(names[i]);
    e.clip = clips[0]; e.variants = names.Length > 1 ? clips : null; e.volume = (float)d[2]; e.pitchJitter = (float)d[3];
    sb.Append(id + (clips[0] == null ? "(MISSING)" : "") + " ");
}
EditorUtility.SetDirty(lib); AssetDatabase.SaveAssets();
return sb.ToString();
