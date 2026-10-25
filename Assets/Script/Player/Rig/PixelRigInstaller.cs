using UnityEngine;

/// <summary>
/// 씬을 고치지 않고 런타임에 새 주인공 픽셀 리그를 붙인다(PixelLook과 같은 방식).
/// Resources/PixelRig/Adventurer 프리팹을 CharGroup 아래에 놓고, 발바닥을 충돌 박스 바닥에 맞춘 뒤 AnyPortrait 캐릭터를 끈다.
/// 끄려면 <see cref="Enabled"/>를 false로 — 그러면 예전 AnyPortrait 캐릭터 그대로다.
/// </summary>
public static class PixelRigInstaller
{
    public const bool Enabled = true;
    private const string PrefabPath = "PixelRig/Adventurer";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!Enabled)
        {
            return;
        }
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        GameObject prefab = Resources.Load<GameObject>(PrefabPath);
        if (pc == null || prefab == null || pc.mainCharacter == null)
        {
            if (pc != null && prefab == null)
            {
                Debug.LogWarning("[PixelRig] Resources/" + PrefabPath + " 프리팹이 없어 AnyPortrait 캐릭터를 쓴다");
            }
            return;
        }
        Transform group = pc.mainCharacter.transform.parent;
        GameObject go = Object.Instantiate(prefab, group);
        go.name = "PixelRig_Adventurer";
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = new Vector3(0f, FeetLocalY(pc, group), 0f);

        pc.mainCharacter.gameObject.SetActive(false);
        pc.UseRig(go.GetComponent<PixelRig>());
    }

    /// <summary>충돌 박스 바닥(발)을 그룹 로컬 y로 — 리그 원점이 발바닥이다</summary>
    private static float FeetLocalY(PlayerController pc, Transform group)
    {
        Collider2D col = pc.GetComponent<Collider2D>();
        if (col == null)
        {
            return 0f;
        }
        float worldY = col.bounds.min.y;
        return group.InverseTransformPoint(new Vector3(pc.transform.position.x, worldY, 0f)).y;
    }
}
