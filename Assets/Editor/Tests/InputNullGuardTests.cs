using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

/// <summary>U38: 런타임 스크립트가 입력 장치(Keyboard·Gamepad·Mouse .current)를 null 체크 없이 쓰지 않는지 소스를 훑는다.</summary>
public class InputNullGuardTests
{
    private static readonly Regex Assign = new Regex(@"(?:Keyboard|Gamepad|Mouse|var)\s+(\w+)\s*=\s*(?:UnityEngine\.InputSystem\.)?(?:Keyboard|Gamepad|Mouse)\.current\s*;");
    private static readonly Regex Direct = new Regex(@"(?:Keyboard|Gamepad|Mouse)\.current\s*(?:\.|\[)");
    private const int Window = 30;

    [Test]
    public void 런타임_스크립트는_입력장치를_null체크한다()
    {
        List<string> bad = new List<string>();
        string root = Path.Combine(Application.dataPath, "Script");
        foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (Direct.IsMatch(lines[i]) && !lines[i].Contains("current != null") && !lines[i].Contains("current == null"))
                {
                    bad.Add(Path.GetFileName(file) + ":" + (i + 1) + " 직접 접근");
                    continue;
                }
                Match m = Assign.Match(lines[i]);
                if (m.Success && FirstUseIsDeref(lines, i, m.Groups[1].Value))
                {
                    bad.Add(Path.GetFileName(file) + ":" + (i + 1) + " " + m.Groups[1].Value);
                }
            }
        }
        Assert.IsEmpty(bad, string.Join("\n", bad));
    }

    private static bool FirstUseIsDeref(string[] lines, int from, string v)
    {
        Regex nullCheck = new Regex(@"\b" + v + @"\s*(==|!=)\s*null|\b" + v + @"\s+is\s+(not\s+)?null|\b" + v + @"\s*\?\.");
        Regex deref = new Regex(@"\b" + v + @"\s*(\.\w|\[)");
        for (int j = from + 1; j < lines.Length && j <= from + Window; j++)
        {
            if (nullCheck.IsMatch(lines[j]))
            {
                return false;
            }
            if (deref.IsMatch(lines[j]))
            {
                return true;
            }
        }
        return false;
    }
}
