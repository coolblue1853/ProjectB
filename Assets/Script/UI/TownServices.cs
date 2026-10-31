using System.Collections;
using UnityEngine;

/// <summary>씬을 저장하지 않고 마을 기존 NPC·창고에 안내와 저장 지점을 연결한다.</summary>
public sealed class TownServices : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("[TownServices]").AddComponent<TownServices>();
    }

    private IEnumerator Start()
    {
        // NpcQuestPlacer의 RuntimeInitialize 배치가 모두 끝난 다음 이름표를 붙인다.
        yield return null;
        foreach (MapInfo map in FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (map.name == "Town")
            {
                Setup(map.transform);
                break;
            }
        }
        Destroy(gameObject);
    }

    private static void Setup(Transform town)
    {
        Label(town.Find("Shop"), "보급 상인", "↑ 상점·의뢰 / Q·E 품목");
        Label(town.Find("WeaponSmith"), "무기 대장장이", "↑ 제작·의뢰 / ↓ 강화");
        Label(town.Find("EquipSmith"), "방어구 대장장이", "↑ 제작 / ↓ 강화");
        Label(town.Find("NPC_Elder"), "마을 장로", "↑ 대화·의뢰");
        Label(town.Find("NPC_ruen"), "사냥꾼 루엔", "↑ 대화·의뢰");
        Label(town.Find("NPC_sena"), "약초꾼 세나", "↑ 대화·의뢰");
        Label(town.Find("QuestBoard"), "의뢰 게시판", "↑ 의뢰");
        Transform source = town.Find("Shop");
        if (source == null)
        {
            return;
        }
        foreach (Chest chest in FindObjectsByType<Chest>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (Mathf.Abs(chest.transform.position.x - source.position.x) < 30f)
            {
                chest.transform.SetParent(town, true);
                chest.transform.position += Vector3.right * (-413.2f - chest.transform.position.x);
                Label(chest.transform, "마을 창고", "↑ 보관·꺼내기");
                break;
            }
        }
        var keeper = new GameObject("TownRecordKeeper");
        keeper.transform.SetParent(town, false);
        keeper.transform.position = new Vector3(-405.8f, source.position.y, source.position.z);
        keeper.transform.localScale = source.localScale;
        SpriteRenderer original = source.GetComponent<SpriteRenderer>();
        if (original == null)
        {
            Destroy(keeper);
            return;
        }
        SpriteRenderer sprite = keeper.AddComponent<SpriteRenderer>();
        sprite.sprite = original.sprite;
        sprite.sharedMaterial = original.sharedMaterial;
        sprite.sortingLayerID = original.sortingLayerID;
        sprite.sortingOrder = original.sortingOrder;
        sprite.color = new Color(0.75f, 0.8f, 0.95f);
        keeper.AddComponent<TownSavePoint>();
        Label(keeper.transform, "기록지기", "↑ 여정 저장");
    }

    private static void Label(Transform target, string title, string hint)
    {
        if (target == null)
        {
            return;
        }
        var label = new GameObject("TownLabel_" + target.name);
        label.transform.SetParent(target.parent, false);
        label.AddComponent<TownServiceLabel>().Init(target, title, hint);
        TalkNpc talk = target.GetComponent<TalkNpc>();
        if (talk != null)
        {
            talk.HasServiceLabel = true;
        }
    }
}
