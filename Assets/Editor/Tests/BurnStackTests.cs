using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>ReviewCode_1105 #4: 화상 재적용은 틱을 중첩하지 않고 기존 틱을 갱신한다</summary>
public class BurnStackTests
{

    [Test]
    public void ApplyBurn_Repeated_KeepsSingleTick()
    {
        var go = new GameObject("BurnTarget");
        var prefab = new GameObject("PoisonProto");
        try
        {
            prefab.AddComponent<PosionAttack>();
            var enemy = go.AddComponent<EnemyHealth>();
            enemy.poisonPrefab = prefab;
            enemy.ApplyBurn(100);
            enemy.ApplyBurn(100);
            enemy.ApplyBurn(100);
            Assert.AreEqual(1, go.GetComponentsInChildren<PosionAttack>().Length);
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(prefab);
        }
    }

    [Test]
    public void CreatPoison_StillStacksForPoison()
    {
        var go = new GameObject("PoisonTarget");
        var prefab = new GameObject("PoisonProto");
        try
        {
            prefab.AddComponent<PosionAttack>();
            var enemy = go.AddComponent<EnemyHealth>();
            enemy.poisonPrefab = prefab;
            enemy.CreatPoisonPrefab(1, 1f, 1);
            enemy.CreatPoisonPrefab(1, 1f, 1);
            Assert.AreEqual(2, go.GetComponentsInChildren<PosionAttack>().Length);
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(prefab);
        }
    }
}
