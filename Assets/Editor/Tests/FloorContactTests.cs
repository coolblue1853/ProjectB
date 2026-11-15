using NUnit.Framework;
using UnityEngine;

/// <summary>1칸 턱 고착(11-15): Wall 레이어 꼭대기에 서 있어도 바닥 접촉으로 잡혀야 한다</summary>
public class FloorContactTests
{
    private GameObject _wall;
    private GameObject _body;
    private SimulationMode2D _oldMode;

    [SetUp]
    public void SetUp()
    {
        _oldMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_wall);
        Object.DestroyImmediate(_body);
        Physics2D.simulationMode = _oldMode;
    }

    private bool RestOn(string layer, Vector2 bodyPos)
    {
        _wall = new GameObject("floor") { layer = LayerMask.NameToLayer(layer) };
        _wall.transform.position = new Vector3(0f, -0.5f, 0f);
        _wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f); // 윗면 y=0
        _body = new GameObject("body");
        _body.transform.position = bodyPos;
        Rigidbody2D rb = _body.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        _body.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 1f);
        Physics2D.SyncTransforms();
        for (int i = 0; i < 60; i++)
        {
            Physics2D.Simulate(0.02f);
        }
        return rb.IsTouching(PlayerController.CreateFloorFilter());
    }

    [Test]
    public void WallLayerTop_CountsAsFloor()
    {
        Assert.IsTrue(RestOn("Wall", new Vector2(0f, 0.6f)));
    }

    [Test]
    public void GroundLayerTop_CountsAsFloor()
    {
        Assert.IsTrue(RestOn("Ground", new Vector2(0f, 0.6f)));
    }

    [Test]
    public void WallFaceOnly_IsNotFloor()
    {
        // 바닥 상자 옆면에 붙은 몸(허공, 중력 0): 옆 법선만 있어 바닥이 아니다
        _wall = new GameObject("side") { layer = LayerMask.NameToLayer("Wall") };
        _wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 4f);
        _body = new GameObject("body");
        _body.transform.position = new Vector3(0.59f, 3f, 0f);
        Rigidbody2D rb = _body.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(-2f, 0f);
        _body.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 1f);
        Physics2D.SyncTransforms();
        for (int i = 0; i < 20; i++)
        {
            Physics2D.Simulate(0.02f);
        }
        Assert.IsFalse(rb.IsTouching(PlayerController.CreateFloorFilter()));
    }
}
