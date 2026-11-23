using Truancy;

namespace Truancy.Tests;

public sealed class TeacherTests
{
    [Fact]
    public void Random50_Asks_WhenValueBelowHalf()
    {
        var teacher = new Teacher(TeacherRule.Random50, default, default);

        var result = teacher.WillAsk(new Dictionary<Subject, bool>(), new FakeRandom().AddDouble(0.25));

        Assert.True(result);
    }

    [Fact]
    public void Random50_DoesNotAsk_WhenValueAboveHalf()
    {
        var teacher = new Teacher(TeacherRule.Random50, default, default);

        var result = teacher.WillAsk(new Dictionary<Subject, bool>(), new FakeRandom().AddDouble(0.75));

        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CopyA_ReturnsSourceValue(bool sourceAsked)
    {
        var teacher = new Teacher(TeacherRule.CopyA, Subject.Calculus, default);
        var yesterday = new Dictionary<Subject, bool> { [Subject.Calculus] = sourceAsked };

        var result = teacher.WillAsk(yesterday, new FakeRandom());

        Assert.Equal(sourceAsked, result);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void XorAB_ReturnsXor(bool a, bool b, bool expected)
    {
        var teacher = new Teacher(TeacherRule.XorAB, Subject.Calculus, Subject.LinearAlgebra);
        var yesterday = new Dictionary<Subject, bool>
        {
            [Subject.Calculus] = a,
            [Subject.LinearAlgebra] = b
        };

        var result = teacher.WillAsk(yesterday, new FakeRandom());

        Assert.Equal(expected, result);
    }
}
