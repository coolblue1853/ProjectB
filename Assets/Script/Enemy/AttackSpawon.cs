using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpawon : MonoBehaviour
{
    public float summonTime;
    public GameObject attackPrefab;
    public float deletTime;
    private void Start()
    {
        Invoke("Summon", summonTime);
        Invoke("Delete", deletTime);
    }

    void Summon()
    {
        attackPrefab.SetActive(true);
    }
    void Delete()
    {
        Destroy(this.gameObject);
    }
}
