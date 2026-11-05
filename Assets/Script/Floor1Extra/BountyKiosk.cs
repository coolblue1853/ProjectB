using TMPro;
using UnityEngine;

/// <summary>마을 사냥 의뢰 푯말: 가까이 서면 "↑ 사냥 의뢰", ↑ 로 BountyPanel 을 연다</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BountyKiosk : MonoBehaviour
{
    private TextMeshPro _prompt;
    private bool _near;

    private void Awake()
    {
        var go = new GameObject("KioskPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 3.4f, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme != null && theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = "↑ 사냥 의뢰";
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme != null ? theme.unique : Color.yellow;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    private void OnEnable()
    {
        Floor1Input.Acquire();
    }

    private void OnDisable()
    {
        Floor1Input.Release();
        _near = false;
    }

    private void Update()
    {
        bool show = _near && !BountyPanel.IsOpen;
        if (_prompt.gameObject.activeSelf != show)
        {
            _prompt.gameObject.SetActive(show);
        }
        if (show && Floor1Input.UpPressed && !DatabaseManager.isOpenUI)
        {
            BountyPanel.Open();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = false;
        }
    }
}

/// <summary>푯말 배치: 마을 의뢰 게시판(QuestBoard) 오른쪽에 코드 도트 푯말을 세운다 (씬 파일 수정 없음)</summary>
public static class BountyKioskPlacer
{
    private const float OffsetX = 7f;

    private static Sprite s_sign;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_sign = null;
    }

    public static void Place()
    {
        MapInfo town = null;
        foreach (MapInfo m in Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (m.name == "Town")
            {
                town = m;
            }
        }
        Transform board = town != null ? town.transform.Find("QuestBoard") : null;
        if (board == null)
        {
            Debug.LogWarning("[PB][Floor1] 사냥 의뢰 푯말 배치 실패: 마을 QuestBoard 없음");
            return;
        }
        var go = new GameObject("BountyKiosk");
        go.transform.SetParent(town.transform, false);
        go.transform.position = new Vector3(PickX(town, board), board.position.y, board.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3.5f, 3f);
        box.offset = new Vector2(0f, 1.5f);
        var art = new GameObject("Art");
        art.transform.SetParent(go.transform, false);
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sprite = SignSprite();
        SpriteRenderer src = board.GetComponentInChildren<SpriteRenderer>(true);
        if (src != null)
        {
            sr.sortingLayerID = src.sortingLayerID;
            sr.sortingOrder = src.sortingOrder;
        }
        go.AddComponent<BountyKiosk>();
        go.AddComponent<MinimapMarker>().Init(new Color(1f, 0.72f, 0.2f), 7f, null);
    }

    /// <summary>푯말 x. 푯말 트리거가 포털 트리거와 겹치면 ↑ 가 의뢰창으로 가 포털을 못 타므로(11-05), 겹치지 않는 후보를 고른다</summary>
    private static float PickX(MapInfo town, Transform board)
    {
        float[] offsets = { OffsetX, -OffsetX, 3.5f, -3.5f, 10.5f };
        Portal[] portals = town.GetComponentsInChildren<Portal>(true);
        foreach (float offset in offsets)
        {
            float x = board.position.x + offset;
            if (!OverlapsPortal(portals, x) && HasGround(x, board.position.y))
            {
                return x;
            }
        }
        return board.position.x + OffsetX;
    }

    /// <summary>푯말 자리 아래에 트리거가 아닌 바닥 콜라이더가 있는가 (맵 밖·허공 배치 방지)</summary>
    private static bool HasGround(float x, float y)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y + 3f), Vector2.down, 8f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    internal static bool OverlapsPortal(Portal[] portals, float x)
    {
        const float half = 1.75f + 0.5f; // 푯말 트리거 반폭 + 여유
        foreach (Portal portal in portals)
        {
            Collider2D col = portal.GetComponentInChildren<Collider2D>(true);
            float px = col != null ? col.bounds.center.x : portal.transform.position.x;
            float ext = col != null ? col.bounds.extents.x : 0.5f;
            if (Mathf.Abs(px - x) < half + ext)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>푯말 (24×30, PPU 16): 나무 기둥 + 널빤지에 붙은 종이 3장</summary>
    private static Sprite SignSprite()
    {
        if (s_sign != null)
        {
            return s_sign;
        }
        const int w = 24;
        const int h = 30;
        var wood = new Color32(104, 78, 54, 255);
        var woodDark = new Color32(66, 48, 36, 255);
        var paper = new Color32(206, 192, 150, 255);
        var ink = new Color32(70, 52, 40, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < 22; y++)
        {
            px[y * w + 11] = wood;
            px[y * w + 12] = woodDark;
        }
        for (int y = 14; y < 29; y++)
        {
            for (int x = 1; x < 23; x++)
            {
                px[y * w + x] = (y == 14 || y == 28 || x == 1 || x == 22) ? woodDark : wood;
            }
        }
        int[] sheetX = { 3, 9, 15 };
        for (int k = 0; k < sheetX.Length; k++)
        {
            for (int y = 17; y < 26; y++)
            {
                for (int x = sheetX[k]; x < sheetX[k] + 5; x++)
                {
                    px[y * w + x] = paper;
                }
            }
            for (int y = 19; y < 25; y += 2)
            {
                for (int x = sheetX[k] + 1; x < sheetX[k] + 4; x++)
                {
                    px[y * w + x] = ink;
                }
            }
        }
        s_sign = MiniGameSprites.MakeSprite(px, w, h);
        return s_sign;
    }
}
