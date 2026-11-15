using Truancy;

namespace Truancy.Tests;

public sealed class StrategyTests
{
    [Fact]
    public void EmptyHistory_AttendsAll()
    {
        bool[] result = new ReconStrategy().DecideDay(1, new History());

        Assert.All(result, attend => Assert.True(attend));
    }

    [Fact]
    public void ReconDays_AttendAll()
    {
        bool[] result = new ReconStrategy().DecideDay(10, new History());

        Assert.All(result, attend => Assert.True(attend));
    }

    [Fact]
    public void NeverAsked_Skips()
    {
        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Physics] = Enumerable.Repeat(false, 10).ToArray()
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void ConsistentlyAsked_Attends()
    {
        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Physics] = Enumerable.Repeat(true, 10).ToArray()
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void CopySource_Skips_WhenSourceNotAskedYesterday()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        bool[] programming = new bool[10];
        for (int day = 0; day < 10; day++)
        {
            programming[day] = day == 0 ? false : calculus[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.Programming] = programming
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Programming]);
    }

    [Fact]
    public void CopySource_Attends_WhenSourceAskedYesterday()
    {
        bool[] calculus = { false, true, false, true, false, true, false, true, false, true };
        bool[] programming = new bool[10];
        for (int day = 0; day < 10; day++)
        {
            programming[day] = day == 0 ? false : calculus[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.Programming] = programming
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Programming]);
    }

    [Fact]
    public void Xor_Attends_WhenInputsDiffer()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        bool[] linearAlgebra = { false, true, false, true, false, true, false, true, false, true };
        bool[] physics = new bool[10];
        for (int day = 0; day < 10; day++)
        {
            physics[day] = day == 0 ? false : calculus[day - 1] != linearAlgebra[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.LinearAlgebra] = linearAlgebra,
            [Subject.Physics] = physics
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.True(ToDecision(result)[Subject.Physics]);
    }

    [Fact]
    public void Xor_Skips_WhenInputsEqual()
    {
        bool[] calculus = { true, false, true, false, true, false, true, false, true, false };
        bool[] linearAlgebra = { false, true, false, true, false, true, false, true, false, false };
        bool[] physics = new bool[10];
        for (int day = 0; day < 10; day++)
        {
            physics[day] = day == 0 ? false : calculus[day - 1] != linearAlgebra[day - 1];
        }

        var history = BuildHistory(new Dictionary<Subject, bool[]>
        {
            [Subject.Calculus] = calculus,
            [Subject.LinearAlgebra] = linearAlgebra,
            [Subject.Physics] = physics
        });

        bool[] result = new ReconStrategy().DecideDay(11, history);

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

        bool[] result = new ReconStrategy().DecideDay(11, history);

        Assert.False(ToDecision(result)[Subject.Physics]);
        Assert.True(ToDecision(result)[Subject.Calculus]);
    }

    private static Dictionary<Subject, bool> ToDecision(bool[] plan) =>
        Subjects.All.Zip(plan).ToDictionary(pair => pair.First, pair => pair.Second);

    private static History BuildHistory(Dictionary<Subject, bool[]> sequences)
    {
        var history = new History();
        foreach ((Subject subject, bool[] asked) in sequences)
        {
            for (int day = 0; day < asked.Length; day++)
            {
                history.RecordAttended(day + 1, subject, asked[day]);
            }
        }
        return history;
    }
}
