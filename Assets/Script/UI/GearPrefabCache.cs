using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 툴팁·상세창이 쓰는 장비 프리팹(Resources/{부위}/{이름}) 캐시(B-017).
/// 열 때마다 Resources.Load 하지 않고, 없는 프리팹(null)도 기억해 경고를 한 번만 남긴다.
/// </summary>
public static class GearPrefabCache
{
    /// <summary>v2(11-11) 폐지 슬롯의 옛 프리팹 폴더: 아이템 장착 부위가 바뀌어도 프리팹은 Resources/{옛부위}/{이름} 에 그대로 둔다</summary>
    static readonly string[] LegacyFolders = { "Head", "Hand", "Leg", "Shoes", "Chest" };

    static readonly Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();

    /// <summary>도메인 리로드를 끈 플레이에서도 이전 세션의 캐시(없음 포함)가 남지 않게 비운다</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _cache.Clear();
    }

    /// <summary>장비 프리팹. 없으면 null</summary>
    public static GameObject Get(string equipArea, string itemName)
    {
        if (string.IsNullOrEmpty(equipArea) || string.IsNullOrEmpty(itemName))
        {
            return null;
        }
        string path = equipArea + "/" + itemName;
        if (_cache.TryGetValue(path, out GameObject prefab))
        {
            return prefab;
        }
        prefab = Resources.Load<GameObject>(path);
        for (int i = 0; prefab == null && i < LegacyFolders.Length; i++)
        {
            if (LegacyFolders[i] != equipArea)
            {
                prefab = Resources.Load<GameObject>(LegacyFolders[i] + "/" + itemName);
            }
        }
        if (prefab == null)
        {
            Debug.LogWarning("[PB][Item] 장비 프리팹 없음: Resources/" + path + " (툴팁 스탯 생략)");
        }
        _cache[path] = prefab;
        return prefab;
    }

    /// <summary>프리팹의 컴포넌트. 프리팹이 없으면 null</summary>
    public static T Comp<T>(GameObject prefab) where T : Component
    {
        return prefab != null ? prefab.GetComponent<T>() : null;
    }
}
