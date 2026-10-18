using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

/// <summary>
/// 맵 루트에 붙는 맵 정보. 포털로 이 맵이 열리면 카메라 아래 경계를 이 맵 기준으로 바꾼다.
/// (ProCamera2D Numeric Boundaries의 아래 경계가 씬 전체에 -10으로 고정되어 Lv2(y≈-180)로 카메라가 내려가지 못하던 문제)
/// MapInfo가 없는 맵은 기본값(-10)을 쓴다.
/// </summary>
public class MapInfo : MonoBehaviour
{
    public const float DefaultBottom = -10f;

    public string displayName;
    public float cameraBottom = DefaultBottom;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook()
    {
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
    }

    static void OnMapChanged(GameObject openMap)
    {
        Apply(openMap != null ? openMap.GetComponent<MapInfo>() : null);
    }

    public static void Apply(MapInfo info)
    {
        var bounds = Object.FindFirstObjectByType<ProCamera2DNumericBoundaries>();
        if (bounds == null)
        {
            return;
        }
        bounds.BottomBoundary = info != null ? info.cameraBottom : DefaultBottom;
        GameLog.Info(GameLog.Cat.Map, "카메라 아래 경계 = " + bounds.BottomBoundary + (info != null ? " (" + info.displayName + ")" : ""));
    }

    /// <summary>어느 맵에 있든 마을로 즉시 이동 (사망 부활 등): 활성 사냥터를 정리·비활성화하고 마을을 켠다.</summary>
    public static bool GoToTown(GameObject traveler)
    {
        var map = GameObject.Find("Map");
        if (map == null || traveler == null)
        {
            return false;
        }
        Transform town = map.transform.Find("Town");
        Portal townPortal = town != null ? town.GetComponentInChildren<Portal>(true) : null;
        if (town == null || townPortal == null)
        {
            return false;
        }
        foreach (Transform m in map.transform)
        {
            if (m == town || !m.gameObject.activeSelf)
            {
                continue;
            }
            var pc = m.GetComponent<PoolCheck>();
            if (pc != null)
            {
                pc.DisableSpwaner();
            }
            m.gameObject.SetActive(false);
        }
        town.gameObject.SetActive(true);
        traveler.transform.position = townPortal.transform.position;
        Portal.RaiseMapChanged(town.gameObject); // BGM·카메라 경계 등 구독자 갱신
        var cam = Object.FindFirstObjectByType<ProCamera2D>();
        if (cam != null)
        {
            cam.CenterOnTargets();
        }
        GameLog.Info(GameLog.Cat.Map, "마을로 귀환");
        return true;
    }
}
