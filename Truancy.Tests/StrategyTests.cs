using Truancy;

namespace Truancy.Tests;

public sealed class StrategyTests
{
    [Fact]
    public void EmptyHistory_AttendsAll()
    {
        var result = new ReconStrategy().DecideDay(1, new History());

        Assert.All(result, attend => Assert.True(attend));
    }

    [Fact]
    public void ReconDays_AttendAll()
    {
        var result = new ReconStrategy().DecideDay(10, new History());

        Assert.All(result, attend => Assert.True(attend));
    }

    [Fact]
    public void NeverAsked_Skips()
    {
        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Physics] = Enumerable.Repeat(false, 10).ToArray()
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void ConsistentlyAsked_Attends()
    {
        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Physics] = Enumerable.Repeat(true, 10).ToArray()
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void CopySource_Skips_WhenSourceNotAskedYesterday()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        var programming = new bool[10];
        for (var day = 0; day < 10; day++)
        {
            programming[day] = day == 0 ? false : calculus[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.Programming] = programming
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Programming]);
    }

    [Fact]
    public void CopySource_Attends_WhenSourceAskedYesterday()
    {
        bool[] calculus = { false, true, false, true, false, true, false, true, false, true };
        var programming = new bool[10];
        for (var day = 0; day < 10; day++)
        {
            programming[day] = day == 0 ? false : calculus[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.Programming] = programming
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Programming]);
    }

    [Fact]
    public void Xor_Attends_WhenInputsDiffer()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        bool[] linearAlgebra = { false, true, false, true, false, true, false, true, false, true };
        var physics = new bool[10];
        for (var day = 0; day < 10; day++)
        {
            physics[day] = day == 0 ? false : calculus[day - 1] != linearAlgebra[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.LinearAlgebra] = linearAlgebra,
            [Subject.Physics] = physics
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void Xor_Skips_WhenInputsEqual()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        bool[] linearAlgebra = { false, true, false, true, false, true, false, true, false, false };
        var physics = new bool[10];
        for (var day = 0; day < 10; day++)
        {
            physics[day] = day == 0 ? false : calculus[day - 1] != linearAlgebra[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.LinearAlgebra] = linearAlgebra,
            [Subject.Physics] = physics
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void Decisions_AreIndependentPerSubject()
    {
        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Physics] = Enumerable.Repeat(false, 10).ToArray(),
            [Subject.Calculus] = Enumerable.Repeat(true, 10).ToArray()
        });

        var result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Physics]);
        Assert.True(ToDecision(result)[Subject.Calculus]);
    }

    private static Dictionary<Subject, bool> ToDecision(bool[] plan) =>
        Subjects.All.Zip(plan).ToDictionary(pair => pair.First, pair => pair.Second);

    private static History BuildHistory(Dictionary<Subject, bool[]> sequences)
    {
        var history = new History();
        foreach (var (subject, asked) in sequences)
        {
            for (var day = 0; day < asked.Length; day++)
            {
                history.RecordAttended(day + 1, subject, asked[day]);
            }
        }
        return history;
    }
}
