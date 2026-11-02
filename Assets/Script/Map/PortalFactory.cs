using UnityEngine;

/// <summary>
/// 런타임 포털 복제(PitReturn·RoadLink 공용): 이웃 포털의 충돌 범위·그림을 그대로 빌려 같은 맵 컨테이너에 포털을 하나 더 만든다. 씬은 건드리지 않는다.
/// closeMap/openMap 을 주면 맵 전환 포털이 된다(같은 맵 안 이동이면 비운다).
/// </summary>
public static class PortalFactory
{
    public static Portal Clone(Transform container, Portal sibling, string objectName, Vector3 position, Transform destination, string label, GameObject closeMap = null, GameObject openMap = null)
    {
        var go = new GameObject(objectName);
        go.SetActive(false); // 필드를 다 채운 뒤 켜서 Portal.Awake 가 완성된 상태로 돈다
        go.transform.SetParent(container, false);
        go.transform.position = position;

        var src = sibling.GetComponentInChildren<BoxCollider2D>(true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = src != null ? src.size : new Vector2(1.5f, 3f);
        col.offset = src != null ? src.offset : Vector2.zero;

        var srcRenderer = sibling.GetComponentInChildren<SpriteRenderer>(true);
        if (srcRenderer != null)
        {
            var vis = new GameObject("Visual");
            vis.transform.SetParent(go.transform, false);
            vis.transform.localPosition = srcRenderer.transform.position - sibling.transform.position;
            vis.transform.localScale = srcRenderer.transform.lossyScale;
            var sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = srcRenderer.sprite;
            sr.sharedMaterial = srcRenderer.sharedMaterial;
            sr.color = srcRenderer.color;
            sr.sortingLayerID = srcRenderer.sortingLayerID;
            sr.sortingOrder = srcRenderer.sortingOrder;
        }

        var portal = go.AddComponent<Portal>();
        portal.destination = destination;
        portal.proCamera = sibling.proCamera;
        portal.labelOverride = label;
        portal.closeMap = closeMap;
        portal.openMap = openMap;
        go.SetActive(true);
        return portal;
    }
}
