using NUnit.Framework;
using UnityEngine.InputSystem;

public class InputBindingsTests
{
    private KeyAction _ka;

    [SetUp]
    public void SetUp()
    {
        _ka = new KeyAction();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_ka.asset); // Dispose 는 Destroy 라 에디트 모드에서 에러
    }

    private string Path(string action, bool gamepad)
    {
        InputAction a = _ka.asset.FindAction("Player/" + action);
        return a.bindings[InputBindings.BindingIndex(a, gamepad)].effectivePath;
    }

    [Test]
    public void Defaults_ArtifactMovedOffShift()
    {
        Assert.AreEqual("<Keyboard>/v", Path("Artifact", false));
        Assert.AreEqual("<Keyboard>/leftShift", Path("Run", false));
        Assert.AreEqual("<Keyboard>/1", Path("Quick1", false));
        Assert.AreEqual("<Gamepad>/dpad/left", Path("Quick2", true)); // 패드 퀵슬롯 2·3 = 십자 좌·우(BasicFeatures 14절)
        Assert.AreEqual("<Gamepad>/dpad/right", Path("Quick3", true));
        Assert.AreEqual("", Path("Quick4", true)); // 게임패드 미지정 칸도 재지정 대상
    }

    [Test]
    public void Gamepad_DpadUpReserved_DpadLeftSwapsWithQuick2()
    {
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Quick4", true, "<Gamepad>/dpad/up", out _)); // 대화·채집(UI/Up)
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Quick4", true, "<Gamepad>/dpad/down", out _)); // 제작(UI/OpenCraft)
        var r = InputBindings.Assign(_ka.asset, "Quick4", true, "<Gamepad>/dpad/left", out string swapped);
        Assert.AreEqual(InputBindings.AssignResult.Swapped, r);
        Assert.AreEqual("Quick2", swapped);
        Assert.AreEqual("<Gamepad>/dpad/left", Path("Quick4", true));
    }

    [Test]
    public void QuestShortcut_DoesNotUseQuick1_AndIsReserved()
    {
        Assert.AreEqual("<Gamepad>/leftStickPress", Path("Quick1", true));
        Assert.AreEqual(InputBindings.AssignResult.Reserved,
            InputBindings.Assign(_ka.asset, "Quick4", true, "<Gamepad>/rightStickPress", out _));
    }

    [Test]
    public void Assign_FreeKey_Ok()
    {
        var r = InputBindings.Assign(_ka.asset, "Quick1", false, "<Keyboard>/7", out string swapped);
        Assert.AreEqual(InputBindings.AssignResult.Ok, r);
        Assert.IsNull(swapped);
        Assert.AreEqual("<Keyboard>/7", Path("Quick1", false));
    }

    [Test]
    public void Assign_UsedByOtherRebindable_Swaps()
    {
        var r = InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/x", out string swapped); // x = Attack
        Assert.AreEqual(InputBindings.AssignResult.Swapped, r);
        Assert.AreEqual("Attack", swapped);
        Assert.AreEqual("<Keyboard>/x", Path("Jump", false));
        Assert.AreEqual("<Keyboard>/c", Path("Attack", false));
    }

    [Test]
    public void Assign_MovementOrEscape_Reserved()
    {
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/leftArrow", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/escape", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/i", out _)); // 인벤 열기
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", true, "<Gamepad>/start", out _)); // 일시정지
        Assert.AreEqual("<Keyboard>/c", Path("Jump", false));
    }

    [Test]
    public void Assign_NotRebindable_Invalid()
    {
        Assert.AreEqual(InputBindings.AssignResult.Invalid, InputBindings.Assign(_ka.asset, "Move", false, "<Keyboard>/7", out _));
    }

    [Test]
    public void OverrideJson_RoundTripsToAnotherInstance()
    {
        InputBindings.Assign(_ka.asset, "Dash", true, "<Gamepad>/buttonNorth", out string swapped); // North = Run → 교환
        string json = _ka.asset.SaveBindingOverridesAsJson();
        var other = new KeyAction();
        try
        {
            other.asset.LoadBindingOverridesFromJson(json);
            InputAction dash = other.asset.FindAction("Player/Dash");
            InputAction run = other.asset.FindAction("Player/Run");
            Assert.AreEqual("<Gamepad>/buttonNorth", dash.bindings[InputBindings.BindingIndex(dash, true)].effectivePath);
            Assert.AreEqual("Run", swapped);
            Assert.AreEqual("<Gamepad>/buttonEast", run.bindings[InputBindings.BindingIndex(run, true)].effectivePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(other.asset);
        }
    }
}
