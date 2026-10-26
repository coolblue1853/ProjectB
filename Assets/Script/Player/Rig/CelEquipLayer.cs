using System;
using UnityEngine;

/// <summary>
/// 던파식 장비 부위 레이어 1개(CelEquipmentPlan 「확정」): 셀 클립 프레임마다 덮어쓰기 그림 + 맨몸에서 지울 마스크.
/// PixelRigBuilder 가 Tools/pixelfx/build_equip.py 산출물(Assets/Art/Player/PixelRig/Equip/&lt;id&gt;/)로 Resources/PixelRig/Equip/&lt;id&gt;.asset 을 만든다.
/// </summary>
[CreateAssetMenu(menuName = "ProjectB/Cel Equip Layer")]
public class CelEquipLayer : ScriptableObject
{
    /// <summary>그리는 순서(작을수록 아래): 맨몸 위에 하의·신발 → 갑옷 → 장갑 → 투구</summary>
    public enum Slot { Legs = 10, Body = 20, Gloves = 30, Head = 40 }

    [Serializable]
    public class Frame
    {
        public string clip;
        public int index;
        public Texture2D layer;
        public Texture2D erase;
    }

    public string id;
    public Slot slot = Slot.Body;
    public Frame[] frames = new Frame[0];

    public Frame Find(string clip, int index)
    {
        foreach (Frame f in frames)
        {
            if (f.index == index && f.clip == clip)
            {
                return f;
            }
        }
        return null;
    }
}
