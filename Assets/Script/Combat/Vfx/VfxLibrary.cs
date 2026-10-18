using System.Collections.Generic;
using UnityEngine;

public enum VfxId
{
    None, CritCross, SlashWhite, SlashRed, SlashBlue, SlashGold, SlashGoldWide, SlashGreenBig, CrackRed,
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
