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
    [Tooltip("탑 층계 (0 = 마을). TowerWorld.md 3절")]
    public int floor = 1;
    [Tooltip("지역 배너에 표시할 영문 구역 이름 (픽셀 폰트에 한글 없음)")]
    public string zoneTitle;
    [Tooltip("상승 부하 (TowerWorld.md 7절). 외곽 0, 위로 갈수록 증가. 탐험가 칭호 ×15 로 저항")]
    public int ascentLoad;
    [Tooltip("밤 반딧불 연출 (숲 맵)")]
    public bool fireflies;

    void Start()
    {
        if (!fireflies)
        {
            return;
        }
        // 맵 영역 = 스포너 위치들의 경계 (+여유)
        var b = new Bounds(transform.position, Vector3.zero);
        bool any = false;
        foreach (EnemySpawner sp in GetComponentsInChildren<EnemySpawner>(true))
        {
            if (!any) { b = new Bounds(sp.transform.position, Vector3.zero); any = true; }
            else { b.Encapsulate(sp.transform.position); }
        }
        if (any)
        {
            b.Expand(new Vector3(8f, 10f, 0f));
            AmbientFireflies.Attach(this, b);
        }
    }

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
        return GoToMap("Town", traveler);
    }

    /// <summary>지정 맵으로 즉시 이동 (웨이포인트): 다른 맵을 정리·비활성화하고 대상 맵을 켠 뒤 그 맵의 첫 포털 위치에 선다.</summary>
    public static bool GoToMap(string mapName, GameObject traveler)
    {
        var map = GameObject.Find("Map");
        if (map == null || traveler == null)
        {
            return false;
        }
        Transform target = map.transform.Find(mapName);
        Portal entry = target != null ? target.GetComponentInChildren<Portal>(true) : null;
        if (target == null || entry == null)
        {
            return false;
        }
        foreach (Transform m in map.transform)
        {
            if (m == target || !m.gameObject.activeSelf)
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
        target.gameObject.SetActive(true);
        var targetCheck = target.GetComponent<PoolCheck>();
        if (targetCheck != null)
        {
            targetCheck.AbleSpwaner(); // 웨이포인트 이동도 포털처럼 스포너를 켠다
        }
        traveler.transform.position = entry.transform.position;
        Portal.RaiseMapChanged(target.gameObject);
        var cam = Object.FindFirstObjectByType<ProCamera2D>();
        if (cam != null)
        {
            cam.CenterOnTargets();
        }
        GameLog.Info(GameLog.Cat.Map, "웨이포인트 이동: " + mapName);
        return true;
    }

}
