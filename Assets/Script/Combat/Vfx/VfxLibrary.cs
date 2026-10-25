using System.Collections.Generic;
using UnityEngine;

public enum VfxId
{
    None, CritCross, SlashWhite, SlashRed, SlashBlue, SlashGold, SlashGoldWide, SlashGreenBig, CrackRed,
    // 지팡이 마법 (Tools/pixelfx/gen_magic.py 절차 생성 도트, 비숍 레퍼런스) — 직렬화 번호 유지를 위해 항상 끝에 추가
    BurstBlue, BurstPurple, BurstGold, BurstGreen,
    PillarBlue, PillarPurple, PillarGold, PillarGreen,
    RuneBlue, RunePurple, RuneGold, RuneGreen,
    // 1층계 폴리싱 (Tools/pixelfx/gen_polish_fx.py): 연기·흙먼지·지면 충격파·붉은 폭발
    SmokeGray, SmokePurple, DustBrown, DustStone, GroundShock, BurstRed,
    // 기본 무기 평타 (PixelLab vfx_workbench 회색조 → PixelSlash2D 테마 색, GameFeelPlan 10.7)
    WSlashDown, WSlashSide, WThrust, WHitStar,
}

/// <summary>이펙트 ID → 프리팹·회전·크기 매핑 (Resources/FX/VfxLibrary.asset). Magic Slashes FX의 3D 베기를 2D 화면용으로 눕혀 쓴다.</summary>
[CreateAssetMenu(menuName = "ProjectB/Vfx Library")]
public class VfxLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public VfxId id;
        public GameObject prefab;
        [Tooltip("3D 베기 이펙트는 X -90으로 눕혀야 카메라를 향한다")]
        public Vector3 euler;
        public float scale = 0.5f;
        [Tooltip("풀 반환까지 시간(초)")]
        public float lifetime = 1f;
        public int sortingOrder = 210;
        [Tooltip("도트 버전(PixelVfxBaker 산출물). 있으면 3D 파티클 대신 스프라이트로 재생")]
        public PixelFxClip pixel;
        public bool usePixel = true;
    }

    public List<Entry> entries = new List<Entry>();

    /// <summary>바닥에 붙여야 하는 이펙트(룬 마법진·빛기둥 착탄·흙먼지·지면 충격파) — 시전 위치의 y를 발밑으로 내린다</summary>
    public static bool IsGroundFx(VfxId id)
    {
        return (id >= VfxId.PillarBlue && id <= VfxId.RuneGreen) || id == VfxId.DustBrown || id == VfxId.DustStone || id == VfxId.GroundShock;
    }

    Dictionary<VfxId, Entry> map;

    public Entry Get(VfxId id)
    {
        if (map == null)
        {
            map = new Dictionary<VfxId, Entry>();
            foreach (Entry e in entries)
            {
                map[e.id] = e;
            }
        }
        map.TryGetValue(id, out Entry found);
        return found;
    }
}
