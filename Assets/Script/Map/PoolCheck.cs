using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCheck : MonoBehaviour
{

    public GameObject dayTimeSpwaner;
    public GameObject nightTimeSpwaner;

    public void AbleSpwaner()
    {
        SetChildrenActive(dayTimeSpwaner, true);
        SetChildrenActive(nightTimeSpwaner, true);
    }

    public void DisableSpwaner()
    {
        ClearAndDisable(dayTimeSpwaner);
        ClearAndDisable(nightTimeSpwaner);
    }

    private static void SetChildrenActive(GameObject root, bool active)
    {
        if (root == null)
        {
            return;
        }
        for (int i = 0; i < root.transform.childCount; i++)
        {
            root.transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 자식 스포너 풀·드롭 정리 후 끈다. 스포너가 아닌 자식(정리용 빈 오브젝트 등)이 섞여도 NRE 없이 건너뛴다(B-141).
    /// </summary>
    private static void ClearAndDisable(GameObject root)
    {
        if (root == null)
        {
            return;
        }
        for (int i = 0; i < root.transform.childCount; i++)
        {
            Transform child = root.transform.GetChild(i);
            if (child.TryGetComponent(out EnemySpawner spawner))
            {
                spawner.ClearAllPools();
            }
            if (child.TryGetComponent(out DropManager dropManager))
            {
                dropManager.ClearAll();
            }
            child.gameObject.SetActive(false);
        }
    }
}
