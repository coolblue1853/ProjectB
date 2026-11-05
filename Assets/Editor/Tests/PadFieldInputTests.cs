using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine.InputSystem;

public class PadFieldInputTests
{
    private const float Hold = 0.8f;

    [Test]
    public void ShortPress_IsTapOnRelease()
    {
        var t = new PadHoldTracker();
        Assert.AreEqual(PadGesture.None, t.Step(true, true, 0f, Hold));
        Assert.AreEqual(PadGesture.None, t.Step(false, true, 0.3f, Hold));
        Assert.AreEqual(PadGesture.Tap, t.Step(false, false, 0.4f, Hold));
        Assert.AreEqual(PadGesture.None, t.Step(false, false, 0.5f, Hold));
    }

    [Test]
    public void LongPress_HoldOnceThenNoTapOnRelease()
    {
        var t = new PadHoldTracker();
        t.Step(true, true, 0f, Hold);
        Assert.AreEqual(PadGesture.None, t.Step(false, true, 0.79f, Hold));
        Assert.AreEqual(PadGesture.Hold, t.Step(false, true, 0.8f, Hold));
        Assert.AreEqual(PadGesture.None, t.Step(false, true, 1.5f, Hold));
        Assert.AreEqual(PadGesture.None, t.Step(false, false, 1.6f, Hold));
    }

    [Test]
    public void SameFramePressRelease_IsTap_AndCancelDropsGesture()
    {
        var t = new PadHoldTracker();
        Assert.AreEqual(PadGesture.Tap, t.Step(true, false, 0f, Hold));
        t.Step(true, true, 1f, Hold);
        t.Cancel();
        Assert.AreEqual(PadGesture.None, t.Step(false, false, 1.1f, Hold));
    }

    [Test]
    public void StepTab_WrapsAtBothEnds()
    {
        Assert.AreEqual(1, PadFieldInputRules.StepTab(0, 2, 1));
        Assert.AreEqual(0, PadFieldInputRules.StepTab(1, 2, 1));
        Assert.AreEqual(1, PadFieldInputRules.StepTab(0, 2, -1));
        Assert.AreEqual(0, PadFieldInputRules.StepTab(2, 3, 1));
        Assert.AreEqual(2, PadFieldInputRules.StepTab(0, 3, -1));
        Assert.AreEqual(0, PadFieldInputRules.StepTab(0, 1, 1));
        Assert.AreEqual(0, PadFieldInputRules.StepTab(0, 0, 1));
    }

    [Test]
    public void LtChord_NeedsLtHeldAndButtonEdge()
    {
        Assert.IsTrue(PadFieldInputRules.IsLtChord(true, true));
        Assert.IsFalse(PadFieldInputRules.IsLtChord(false, true));
        Assert.IsFalse(PadFieldInputRules.IsLtChord(true, false));
    }

    // 같은 액션 맵 안에서 한 패드 버튼이 둘 이상의 액션에 묶인 경우. Player 맵은 0건이어야 하고, UI 맵의 기존 중복(방향 탐색·확인 겸용 등)만 허용한다
    private static readonly string[] AllowedUiShared =
    {
        "<Gamepad>/dpad/up", "<Gamepad>/leftStick/up", "<Gamepad>/dpad/down", "<Gamepad>/leftStick/down",
        "<Gamepad>/dpad/left", "<Gamepad>/leftStick/left", "<Gamepad>/dpad/right", "<Gamepad>/leftStick/right",
        "<Gamepad>/buttonSouth", "<Gamepad>/leftShoulder",
    };

    [Test]
    public void GamepadPath_NotBoundTwiceInSameMap_ExceptKnownUiNavigation()
    {
        var asset = InputActionAsset.FromJson(File.ReadAllText("Assets/InputSystem/KeyAction.inputactions"));
        foreach (InputActionMap map in asset.actionMaps)
        {
            var byPath = new Dictionary<string, HashSet<string>>();
            foreach (InputBinding b in map.bindings)
            {
                if (b.isComposite || string.IsNullOrEmpty(b.path) || !b.path.StartsWith("<Gamepad>"))
                {
                    continue;
                }
                if (!byPath.TryGetValue(b.path, out HashSet<string> set))
                {
                    set = new HashSet<string>();
                    byPath[b.path] = set;
                }
                set.Add(b.action);
            }
            foreach (KeyValuePair<string, HashSet<string>> kv in byPath)
            {
                if (kv.Value.Count > 1)
                {
                    Assert.AreEqual("UI", map.name, map.name + " 맵에서 " + kv.Key + " 이중 바인딩");
                    CollectionAssert.Contains(AllowedUiShared, kv.Key, "UI 맵 신규 이중 바인딩 " + kv.Key + " : " + string.Join(",", kv.Value));
                }
            }
        }
    }

    [Test]
    public void PortalOn_HasDpadUpAndKeyboardUp()
    {
        var asset = InputActionAsset.FromJson(File.ReadAllText("Assets/InputSystem/KeyAction.inputactions"));
        InputAction a = asset.FindAction("Portal/PortalOn");
        var paths = new List<string>();
        foreach (InputBinding b in a.bindings)
        {
            paths.Add(b.path);
        }
        CollectionAssert.Contains(paths, "<Keyboard>/upArrow");
        CollectionAssert.Contains(paths, "<Gamepad>/dpad/up");
    }
}
