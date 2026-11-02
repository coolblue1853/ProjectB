using UnityEngine;

/// <summary>
/// 일반 숲 도착 보정(11-02 TechDebt). 깊은 숲 → 일반 숲 포털의 도착 지점(x -722)이 걸어서 길로 나갈 수 없는 왼쪽 산이라서,
/// 도착을 길 위(시작 계단 근처)로 옮기고, 길에서 깊은 숲으로 가는 포털(원래는 그 산 위에 있던 것)을 길에 하나 더 만든다.
/// 씬은 건드리지 않고 PitReturn 과 같은 방식으로 런타임에 만든다. 끄기: Floor1Content.csv 의 roadlink.
/// </summary>
public static class RoadLink
{
    private const string MapName = "Lv1_NormalForest";
    private const string SourceMap = "Lv1_DeepForest";
    private const string ObjectName = "Portal_RoadToDeep";
    private const string ArrivalName = "RoadLink_Arrival";
    private const float ArrivalX = -300f;
    private const float ArrivalY = 1.5f; // 길 지면 0.4 + 1.1 (맵이 꺼져 있어도 쓸 수 있게 잰 값으로 고정)
    private const float PortalX = -334f;
    private const float PortalY = 1.2f;  // 지면 0.0 + 1.2
    private const float MountainX = -600f; // 이보다 왼쪽이면 걸어서 나갈 수 없는 산 도착으로 본다

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!Floor1Data.On("roadlink"))
        {
            return;
        }
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
        GameObject root = GameObject.Find("Map");
        if (root == null)
        {
            return;
        }
        foreach (Transform m in root.transform)
        {
            if (m.name == MapName || m.name == SourceMap)
            {
                Apply(m.gameObject);
            }
        }
    }

    private static void OnMapChanged(GameObject map)
    {
        // 도착 직전에 목적지가 길로 바뀌어 있어야 해서 두 맵 모두 맵이 켜질 때 한 번씩 확인한다(이미 고쳤으면 건너뜀)
        if (map != null && (map.name == MapName || map.name == SourceMap))
        {
            Apply(map);
        }
    }

    private static void Apply(GameObject map)
    {
        GameObject root = GameObject.Find("Map");
        if (root == null)
        {
            return;
        }
        Transform normal = root.transform.Find(MapName);
        Transform deep = root.transform.Find(SourceMap);
        if (normal == null || deep == null)
        {
            return;
        }
        Transform container = normal.Find("Portal");
        Portal toDeep = null;
        Portal fromDeep = null;
        foreach (Portal p in deep.GetComponentsInChildren<Portal>(true))
        {
            if (p.destination != null && p.destination.IsChildOf(normal) && p.name != "Portal_PitReturn")
            {
                fromDeep = p;
            }
        }
        if (container != null)
        {
            foreach (Portal p in container.GetComponentsInChildren<Portal>(true))
            {
                if (p.destination != null && p.destination.IsChildOf(deep) && p.transform.position.x < MountainX)
                {
                    toDeep = p;
                }
            }
        }
        if (fromDeep == null || container == null || fromDeep.destination.position.x > MountainX)
        {
            return; // 이미 길로 옮겼거나 구조가 달라졌다
        }
        var arrival = new GameObject(ArrivalName).transform;
        arrival.SetParent(container, false);
        arrival.position = new Vector3(ArrivalX, ArrivalY, fromDeep.destination.position.z);
        Transform oldArrival = fromDeep.destination;
        fromDeep.destination = arrival;
        if (toDeep != null && container.Find(ObjectName) == null)
        {
            PortalFactory.Clone(container, toDeep, ObjectName, new Vector3(PortalX, PortalY, toDeep.transform.position.z), toDeep.destination, null, toDeep.closeMap, toDeep.openMap);
        }
        GameLog.Info(GameLog.Cat.Map, "일반 숲 도착을 길로 이동: " + oldArrival.position + " → " + arrival.position);
    }
}
