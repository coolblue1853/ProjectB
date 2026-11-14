using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 아이콘 로드(Resources/Item/{name}). 파일이 없으면 null 을 돌려 호출 쪽이 기존 임시 그림(프리팹 기본)을 그대로 쓰게 하고,
/// 콘솔에는 이름당 한 번만 Warning 을 남긴다(Error 아님). 그래픽은 그래픽 방 담당이라 여기서 만들지 않는다.
/// </summary>
public static class ItemIconLoader
{
    private static readonly HashSet<string> s_warned = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_warned.Clear();
    }

    public static Sprite Load(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return null;
        }
        Sprite sprite = Resources.Load<Sprite>("Item/" + itemName);
        if (sprite == null && s_warned.Add(itemName))
        {
            Debug.LogWarning("아이템 아이콘 없음(임시 아이콘 사용): Item/" + itemName);
        }
        return sprite;
    }

    /// <summary>경고 없이 읽는다 (없을 수 있는 이름을 확인할 때 — 퀘스트 목록 아이콘 대체 판정)</summary>
    public static Sprite LoadQuiet(string itemName)
    {
        return string.IsNullOrEmpty(itemName) ? null : Resources.Load<Sprite>("Item/" + itemName);
    }

    /// <summary>이미 경고한 이름인지(테스트용)</summary>
    public static bool HasWarned(string itemName)
    {
        return s_warned.Contains(itemName);
    }
}
