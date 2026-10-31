using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class CastBarTests
{
    private GameObject _go;
    private CastBar _bar;
    private Image _fill;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("CastBarTest");
        _bar = _go.AddComponent<CastBar>();
        if (_go.transform.Find("Box") == null)
        {
            Invoke("Awake");
        }
        _fill = _go.transform.Find("Box/Fill").GetComponent<Image>();
    }

    [TearDown]
    public void TearDown()
    {
        Invoke("OnDisable");
        UnityEngine.Object.DestroyImmediate(_fill.sprite);
        UnityEngine.Object.DestroyImmediate(_go);
    }

    [TestCase(0.25f)]
    [TestCase(0.75f)]
    public void 진행률에_비례해_실제_메시_폭도_변한다(float progress)
    {
        Invoke("DoShow", "검증", new Func<float>(() => progress));
        Invoke("Update");
        _fill.rectTransform.sizeDelta = new Vector2(360f, 26f);
        using (var vh = new VertexHelper())
        {
            typeof(Image).GetMethod("OnPopulateMesh", Private | BindingFlags.DeclaredOnly).Invoke(_fill, new object[] { vh });
            var vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);
            float min = float.MaxValue, max = float.MinValue;
            foreach (UIVertex vertex in vertices)
            {
                min = Mathf.Min(min, vertex.position.x);
                max = Mathf.Max(max, vertex.position.x);
            }
            Assert.AreEqual(_fill.rectTransform.rect.width * progress, max - min, 0.1f);
        }
    }

    [Test]
    public void 취소는_세번_깜박이고_종료한다()
    {
        Invoke("DoShow", "검증", new Func<float>(() => 0.5f));
        Invoke("Update");
        Invoke("DoHide", false);
        Tween blink = (Tween)typeof(CastBar).GetField("_blink", Private).GetValue(_bar);
        Assert.AreEqual(0.42f, blink.Duration(), 0.001f);
        for (int i = 0; i < 3; i++)
        {
            blink.Goto(i * 0.14f + 0.07f);
            Assert.AreEqual(0.15f, _fill.color.a, 0.01f);
            blink.Goto(i * 0.14f + 0.139f);
            Assert.Greater(_fill.color.a, 0.99f);
        }
        blink.Complete(true);
        Assert.IsFalse(_fill.transform.parent.gameObject.activeSelf);
        Invoke("OnDisable");
        Assert.IsFalse(_fill.transform.parent.gameObject.activeSelf);
    }

    private void Invoke(string method, params object[] args)
    {
        typeof(CastBar).GetMethod(method, Private).Invoke(_bar, args);
    }
}
