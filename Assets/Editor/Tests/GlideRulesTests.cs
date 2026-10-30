using NUnit.Framework;

public class GlideRulesTests
{
    [Test]
    public void Start_NeedsHold_Air_Falling()
    {
        Assert.IsFalse(GlideRules.CanStart(0.1f, true, -2f, false, 12f)); // 짧은 누름 = 기존 점프
        Assert.IsTrue(GlideRules.CanStart(0.2f, true, -2f, false, 12f));
        Assert.IsFalse(GlideRules.CanStart(0.5f, false, -2f, false, 12f)); // 지상
        Assert.IsFalse(GlideRules.CanStart(0.5f, true, 5f, false, 12f)); // 상승 중
        Assert.IsFalse(GlideRules.CanStart(0.5f, true, -2f, true, 12f)); // 막힘
        Assert.IsFalse(GlideRules.CanStart(0.5f, true, -2f, false, 0f)); // 소진
    }

    [Test]
    public void Glide_ClampsFall_AndScalesHorizontal()
    {
        Assert.AreEqual(-1.3f, GlideRules.VerticalSpeed(-9f, true, false, 0f, 0f, 0f), 1e-5f);
        Assert.AreEqual(-0.5f, GlideRules.VerticalSpeed(-0.5f, true, false, 0f, 0f, 0f), 1e-5f);
        Assert.AreEqual(-9f, GlideRules.VerticalSpeed(-9f, false, false, 0f, 0f, 0f), 1e-5f);
        Assert.AreEqual(8.5f, GlideRules.HorizontalSpeed(10f, true, 0f), 1e-5f);
        Assert.AreEqual(10f, GlideRules.HorizontalSpeed(10f, false, 1.5f), 1e-5f);
        Assert.AreEqual(10f, GlideRules.HorizontalSpeed(10f, true, 1.5f), 1e-5f); // 8.5 + 1.5
    }

    [Test]
    public void Updraft_Values_AndNonGlideSlowsFall()
    {
        Assert.AreEqual(1.8f, GlideRules.VerticalSpeed(-1.3f, true, true, GlideRules.WindVy(AirflowKind.UpWeak), 0f, 0f), 1e-5f);
        Assert.AreEqual(3.5f, GlideRules.VerticalSpeed(-1.3f, true, true, GlideRules.WindVy(AirflowKind.UpStrong), 0f, 0f), 1e-5f);
        Assert.AreEqual(-3f, GlideRules.VerticalSpeed(-1.3f, true, true, GlideRules.WindVy(AirflowKind.Down), 0f, 0f), 1e-5f);
        Assert.AreEqual(-3.2f, GlideRules.VerticalSpeed(-9f, false, true, 1.8f, 0f, 0f), 1e-5f);
        Assert.AreEqual(4f, GlideRules.VerticalSpeed(4f, false, true, 1.8f, 0f, 0f), 1e-5f); // 점프 상승은 그대로
        Assert.AreEqual(1.5f, GlideRules.WindVx(AirflowKind.Tail));
        Assert.AreEqual(-1.5f, GlideRules.WindVx(AirflowKind.Head));
    }

    [Test]
    public void ExitDecay_FadesOver_FourTenths()
    {
        // 기류 이탈 직후(비율 1) 상승 속도 유지 → 0.4초 뒤 활공 낙하
        Assert.AreEqual(1.8f, GlideRules.VerticalSpeed(-1.3f, true, false, 0f, 1f, 1.8f), 1e-5f);
        Assert.AreEqual(0.9f, GlideRules.VerticalSpeed(-1.3f, true, false, 0f, 0.5f, 1.8f), 1e-5f);
        Assert.AreEqual(-1.3f, GlideRules.VerticalSpeed(-1.3f, true, false, 0f, 0f, 1.8f), 1e-5f);
    }

    [Test]
    public void Remaining_Drains_Regens_Caps()
    {
        Assert.AreEqual(11f, GlideRules.TickRemaining(12f, 1f, true, false, 12f), 1e-5f);
        Assert.AreEqual(12f, GlideRules.TickRemaining(11f, 1f, true, true, 12f), 1e-5f); // −1 +3 → 상한
        Assert.AreEqual(9f, GlideRules.TickRemaining(6f, 1f, false, true, 12f), 1e-5f);
        Assert.AreEqual(0f, GlideRules.TickRemaining(0.5f, 1f, true, false, 12f), 1e-5f);
        // 12초 소진
        float r = GlideRules.MaxDuration;
        for (int i = 0; i < 1200; i++)
        {
            r = GlideRules.TickRemaining(r, 0.01f, true, false, GlideRules.MaxDuration);
        }
        Assert.AreEqual(0f, r, 0.02f);
    }

    [Test]
    public void AscentDeficit_ShortensDuration_TenToTwentyPercent()
    {
        Assert.AreEqual(12f, GlideRules.MaxDurationFor(0), 1e-5f);
        Assert.AreEqual(12f * 0.9f, GlideRules.MaxDurationFor(1) , 0.1f);
        Assert.AreEqual(12f * 0.8f, GlideRules.MaxDurationFor(50), 1e-4f);
        Assert.AreEqual(12f * 0.8f, GlideRules.MaxDurationFor(99), 1e-4f);
        Assert.Greater(GlideRules.DurationMul(1), GlideRules.DurationMul(40));
    }

    [Test]
    public void Grade_Thresholds()
    {
        Assert.AreEqual(GlideGrade.C, GlideCourseRules.Grade(61f, 60f, 0, 3, 3));
        Assert.AreEqual(GlideGrade.B, GlideCourseRules.Grade(58f, 60f, 2, 3, 3));
        Assert.AreEqual(GlideGrade.A, GlideCourseRules.Grade(51f, 60f, 0, 0, 3));
        Assert.AreEqual(GlideGrade.B, GlideCourseRules.Grade(51f, 60f, 1, 3, 3)); // 추락 있으면 A 불가
        Assert.AreEqual(GlideGrade.S, GlideCourseRules.Grade(42f, 60f, 0, 3, 3));
        Assert.AreEqual(GlideGrade.A, GlideCourseRules.Grade(42f, 60f, 0, 2, 3)); // 문장 부족이면 S 불가
    }

    [Test]
    public void Reward_FloorOfMul_AndRepeatHalf()
    {
        Assert.AreEqual(2, GlideCourseRules.Reward(2, GlideGrade.C, true));
        Assert.AreEqual(2, GlideCourseRules.Reward(2, GlideGrade.B, true)); // 2.4 내림
        Assert.AreEqual(3, GlideCourseRules.Reward(2, GlideGrade.A, true));
        Assert.AreEqual(4, GlideCourseRules.Reward(2, GlideGrade.S, true));
        Assert.AreEqual(1, GlideCourseRules.Reward(2, GlideGrade.C, false));
        Assert.AreEqual(2, GlideCourseRules.Reward(2, GlideGrade.S, false));
    }

    [Test]
    public void DayIndex_ByPeriod()
    {
        Assert.AreEqual(0, GlideCourseRules.DayIndex(299f, 300f));
        Assert.AreEqual(1, GlideCourseRules.DayIndex(300f, 300f));
    }
}
