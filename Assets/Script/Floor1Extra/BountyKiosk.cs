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
        go.transform.position = new Vector3(board.position.x + OffsetX, board.position.y, board.position.z);
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
