using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CsvSpawnOverrideTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Override_WithoutScenePoints_UsesCsvWeights(bool emptyRoot)
    {
        var map = new GameObject("Lv1_NearForest_SideRoad");
        map.SetActive(false);
        map.AddComponent<MapInfo>();
        var go = new GameObject("Tree");
        go.transform.SetParent(map.transform);
        var spawner = go.AddComponent<EnemySpawner>();
        spawner.enemyCount = 4;
        spawner.enemyPositionArray = new GameObject[4];
        if (emptyRoot)
        {
            spawner.enemyPositon = new GameObject("EmptyPoints");
            spawner.enemyPositon.transform.SetParent(go.transform);
        }
        go.SetActive(false);
        map.SetActive(true);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(EnemySpawner).GetField("spawnPointWeights", flags).SetValue(spawner, new[] { 0, 3, 0, 1 });
        try
        {
            typeof(EnemySpawner).GetMethod("Start", flags).Invoke(spawner, null);
            Assert.IsTrue(spawner.enabled);
            Assert.AreSame(spawner.enemyPositionArray[0], spawner.enemyPositionArray[2]);
            Assert.AreEqual(-316f, spawner.enemyPositionArray[0].transform.position.x);
            Assert.AreEqual(-705f, spawner.enemyPositionArray[3].transform.position.x);
        }
        finally
        {
            Object.DestroyImmediate(map);
        }
    }
}
