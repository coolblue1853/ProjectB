using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

/// <summary>
/// 샛길 지형을 복제한 1층계 구역의 구덩이 탈출 포털(11-02 이동 점검).
/// 길 위 고원(x≈-258, 높이 4.4) 끝에서 오른쪽으로 걸으면 높이 8짜리 절벽 아래 구덩이(x -254 ~ -139)로 떨어지는데,
/// 점프로는 못 올라와 걸어서 돌아갈 길이 없었다(깊은 숲·산적 성채·탑 근교는 도착 지점도 구덩이 안).
/// 구덩이 안쪽 끝에 같은 맵 안 이동 포털 하나를 런타임에 만들어 고원 아래 계단으로 올려 준다. 씬은 건드리지 않는다.
/// </summary>
public static class PitReturn
{
    /// <summary>구덩이가 있는 복제 지형 구역</summary>
    private static readonly string[] Maps =
    {
        "Lv1_NearForest_SideRoad", "Lv1_NormalForest", "Lv1_DeepForest", "Lv1_BanditFortress", "Lv1_TowerOutskirts",
    };

    private const float PitX = -248f, RoadX = -274f;
    private const float PitDepthBelow = 2f; // 구덩이 바닥은 길보다 이 이상 낮아야 한다(지형 확인)
    private const string ObjectName = "Portal_PitReturn";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
        GameObject root = GameObject.Find("Map");
        if (root == null)
        {
            return;
        }
        foreach (Transform m in root.transform)
        {
            if (m.gameObject.activeSelf)
            {
                OnMapChanged(m.gameObject);
            }
        }
    }

    private static void OnMapChanged(GameObject map)
    {
        if (map == null || System.Array.IndexOf(Maps, map.name) < 0)
        {
            return;
        }
        Transform container = map.transform.Find("Portal");
        if (container == null || container.Find(ObjectName) != null)
        {
            return;
        }
        Portal sibling = container.GetComponentInChildren<Portal>(true);
        float pitGround = GroundY(PitX, true), roadGround = GroundY(RoadX, false);
        if (sibling == null || float.IsNaN(pitGround) || float.IsNaN(roadGround) || pitGround > roadGround - PitDepthBelow)
        {
            return; // 이 구역엔 구덩이가 없다
        }
        Build(container, sibling, pitGround, roadGround);
    }

    /// <summary>x 위치의 지면 높이: lowest 면 가장 아래 면(구덩이 바닥), 아니면 20 이하 가장 높은 면(길). 없으면 NaN</summary>
    private static float GroundY(float x, bool lowest)
    {
        int mask = LayerMask.GetMask("Ground");
        float best = float.NaN;
        foreach (RaycastHit2D h in Physics2D.RaycastAll(new Vector2(x, 60f), Vector2.down, 120f, mask))
        {
            if (lowest ? (float.IsNaN(best) || h.point.y < best) : (h.point.y <= 20f && (float.IsNaN(best) || h.point.y > best)))
            {
                best = h.point.y;
            }
        }
        return best;
    }

    private static void Build(Transform container, Portal sibling, float pitGround, float roadGround)
    {
        var dest = new GameObject("PitReturn_Arrival").transform;
        dest.SetParent(container, false);
        dest.position = new Vector3(RoadX, roadGround + 1.1f, sibling.transform.position.z);
        Portal portal = PortalFactory.Clone(container, sibling, ObjectName, new Vector3(PitX, pitGround + 1.2f, sibling.transform.position.z), dest, "Upper road");
        GameLog.Info(GameLog.Cat.Map, "구덩이 탈출 포털: " + container.parent.name + " " + portal.transform.position + " → " + dest.position);
    }
}
