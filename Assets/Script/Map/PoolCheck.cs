using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCheck : MonoBehaviour
{

    public GameObject dayTimeSpwaner;
    public GameObject nightTimeSpwaner;

    public void AbleSpwaner()
    {
        for(int i = 0; i < dayTimeSpwaner.transform.childCount; i++)
        {
            dayTimeSpwaner.transform.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = 0; i < nightTimeSpwaner.transform.childCount; i++)
        {
            nightTimeSpwaner.transform.GetChild(i).gameObject.SetActive(true);
        }
    }
    public void DisableSpwaner()
    {
        for (int i = 0; i < dayTimeSpwaner.transform.childCount; i++)
        {
            EnemySpawner spawner = dayTimeSpwaner.transform.GetChild(i).GetComponent<EnemySpawner>();
            spawner.ClearAllPools();
            DropManager dropManager = dayTimeSpwaner.transform.GetChild(i).GetComponent<DropManager>();
            dropManager.ClearAll();
            dayTimeSpwaner.transform.GetChild(i).gameObject.SetActive(false);

        }
        for (int i = 0; i < nightTimeSpwaner.transform.childCount; i++)
        {
            EnemySpawner spawner = nightTimeSpwaner.transform.GetChild(i).GetComponent<EnemySpawner>();
            spawner.ClearAllPools();
            DropManager dropManager = nightTimeSpwaner.transform.GetChild(i).GetComponent<DropManager>();
            dropManager.ClearAll();
            nightTimeSpwaner.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
