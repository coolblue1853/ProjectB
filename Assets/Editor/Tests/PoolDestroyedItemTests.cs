using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>R10: 풀 안에서 파괴된 공격 판정을 꺼내도 예외 없이 새로 받고, 반환된 판정은 몹에서 떨어진다.</summary>
public class PoolDestroyedItemTests
{
    private static EnemySpawner MakeSpawner(GameObject damagePrefab, out GameObject root)
    {
        root = new GameObject("R10Spawner");
        root.SetActive(false);
        EnemySpawner spawner = root.AddComponent<EnemySpawner>();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo infosField = typeof(EnemySpawner).GetField("objectInfos", flags);
        Type infoType = infosField.FieldType.GetElementType();
        object info = Activator.CreateInstance(infoType);
        infoType.GetField("objectName").SetValue(info, "hit");
        infoType.GetField("perfab").SetValue(info, damagePrefab);
        infoType.GetField("count").SetValue(info, 0);
        Array arr = Array.CreateInstance(infoType, 1);
        arr.SetValue(info, 0);
        infosField.SetValue(spawner, arr);
        typeof(EnemySpawner).GetMethod("Init", flags).Invoke(spawner, null);
        return spawner;
    }

    [Test]
    public void GetOb_SkipsItemDestroyedInsidePool()
    {
        var prefab = new GameObject("R10Hit");
        prefab.AddComponent<EnemyDamageObject>();
        var enemy = new GameObject("R10Enemy");
        GameObject root;
        EnemySpawner spawner = MakeSpawner(prefab, out root);
        try
        {
            GameObject hit = spawner.GetOb("hit");
            Assert.IsNotNull(hit);
            hit.transform.SetParent(enemy.transform);
            hit.GetComponent<PoolAble>().ReleaseObject();
            Assert.IsNull(hit.transform.parent, "반환된 판정은 몹에서 떨어져야 한다");

            // 풀 안의 항목이 파괴된 상황(과거: 부모 몹과 함께 파괴)
            UnityEngine.Object.DestroyImmediate(hit);
            GameObject next = null;
            Assert.DoesNotThrow(() => next = spawner.GetOb("hit"));
            Assert.IsNotNull(next);
            Assert.IsTrue(next.activeSelf);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }

    [Test]
    public void EnemyDamageObject_Release_DetachesFromOwnerAndSurvivesOwnerDestroy()
    {
        var prefab = new GameObject("R10Hit2");
        prefab.AddComponent<EnemyDamageObject>();
        var enemy = new GameObject("R10Enemy2");
        GameObject root;
        EnemySpawner spawner = MakeSpawner(prefab, out root);
        try
        {
            GameObject hit = spawner.GetOb("hit");
            hit.transform.SetParent(enemy.transform);
            hit.GetComponent<PoolAble>().ReleaseObject();
            UnityEngine.Object.DestroyImmediate(enemy); // 몹이 풀 초과로 파괴
            Assert.IsTrue(hit != null, "판정이 몹과 함께 파괴되면 안 된다");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }
}
