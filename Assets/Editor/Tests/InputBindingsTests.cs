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
    public void Gamepad_OwnReservedDefault_CanBeReassignedBack_OthersStayReserved()
    {
        // 패드 면 버튼은 모두 기본 사용 중이라 교환(Swapped)이 되지만, 예약(Reserved)이 아니면 된다
        AssertOwnDefaultRoundTrip("Quick1", "<Gamepad>/leftStickPress");
        AssertOwnDefaultRoundTrip("Artifact", "<Gamepad>/rightStickPress");
        AssertOwnDefaultRoundTrip("ArtifactNext", "<Gamepad>/select");
        // 타 액션은 그 키 거부(예약 유지)
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Quick2", true, "<Gamepad>/leftStickPress", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Quick1", true, "<Gamepad>/rightStickPress", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Quick4", true, "<Gamepad>/select", out _));
    }

    private void AssertOwnDefaultRoundTrip(string action, string defaultPath)
    {
        Assert.AreEqual(defaultPath, Path(action, true));
        Assert.AreNotEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, action, true, "<Gamepad>/buttonEast", out _));
        Assert.AreEqual("<Gamepad>/buttonEast", Path(action, true));
        Assert.AreNotEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, action, true, defaultPath, out _)); // 자기 기본 키로 복귀
        Assert.AreEqual(defaultPath, Path(action, true));
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
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/q", out _)); // 퀘스트 창(REWORK-8)
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

    // ── U37: 창 열기 키 ──

    [Test]
    public void Hotkeys_Defaults_KeyboardOnly()
    {
        Assert.AreEqual("<Keyboard>/b", Path("Bestiary", false));
        Assert.AreEqual("<Keyboard>/m", Path("WorldMap", false));
        Assert.AreEqual("<Keyboard>/n", Path("Minimap", false));
        Assert.AreEqual("<Keyboard>/g", Path("Guard", false));
        Assert.AreEqual("<Keyboard>/k", Path("SkillWindow", false));
        Assert.AreEqual("<Keyboard>/q", Path("QuestWindow", false));
        foreach (string name in Hotkeys.All)
        {
            Assert.IsTrue(System.Array.IndexOf(InputBindings.Rebindable, name) >= 0, name);
            Assert.AreEqual(-1, InputBindings.BindingIndex(_ka.asset.FindAction("Player/" + name), true), name + " 패드 칸 없음");
        }
    }

    [Test]
    public void Hotkeys_SwapAmongThemselves()
    {
        var r = InputBindings.Assign(_ka.asset, "Bestiary", false, "<Keyboard>/m", out string swapped);
        Assert.AreEqual(InputBindings.AssignResult.Swapped, r);
        Assert.AreEqual("WorldMap", swapped);
        Assert.AreEqual("<Keyboard>/m", Path("Bestiary", false));
        Assert.AreEqual("<Keyboard>/b", Path("WorldMap", false));
    }

    [Test]
    public void Hotkeys_FreeKey_Ok_AndDefaultsOwnedKeysAllowed()
    {
        Assert.AreEqual(InputBindings.AssignResult.Ok, InputBindings.Assign(_ka.asset, "Minimap", false, "<Keyboard>/u", out _));
        Assert.AreEqual("<Keyboard>/u", Path("Minimap", false));
        // 퀘스트 Q 는 전투 액션에게는 예약이지만 창 열기 키 그룹에는 자기 몫이다
        Assert.AreEqual(InputBindings.AssignResult.Ok, InputBindings.Assign(_ka.asset, "QuestWindow", false, "<Keyboard>/q", out _));
    }

    [Test]
    public void Hotkeys_ConflictsWithCombatOrFixedKeys_Rejected()
    {
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Bestiary", false, "<Keyboard>/c", out _)); // 점프
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Bestiary", false, "<Keyboard>/escape", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Bestiary", false, "<Keyboard>/i", out _)); // 인벤 열기
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Bestiary", false, "<Keyboard>/p", out _));
        Assert.AreEqual(InputBindings.AssignResult.Reserved, InputBindings.Assign(_ka.asset, "Jump", false, "<Keyboard>/g", out _)); // 방어 키는 전투 액션이 못 쓴다
        Assert.AreEqual(InputBindings.AssignResult.Invalid, InputBindings.Assign(_ka.asset, "Bestiary", true, "<Gamepad>/buttonEast", out _)); // 패드 칸 없음
    }

    [Test]
    public void Hotkeys_OverrideJson_OldSaveCompatible()
    {
        string oldJson = _ka.asset.SaveBindingOverridesAsJson(); // 기본(오버라이드 없음) = 신규 액션 항목이 없는 구 세이브와 같다
        var other = new KeyAction();
        try
        {
            other.asset.LoadBindingOverridesFromJson(oldJson);
            InputAction a = other.asset.FindAction("Player/Bestiary");
            Assert.AreEqual("<Keyboard>/b", a.bindings[InputBindings.BindingIndex(a, false)].effectivePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(other.asset);
        }
    }
}
