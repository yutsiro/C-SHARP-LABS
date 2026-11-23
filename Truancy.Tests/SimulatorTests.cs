using Truancy;

namespace Truancy.Tests;

public sealed class SimulatorTests
{
    private sealed class FixedStrategy : ISkipStrategy
    {
        private readonly bool[] _plan;

        public FixedStrategy(bool[] plan) => _plan = plan;

        public bool[] DecideDay(int day, IReadOnlyStudentHistory history) => _plan;
    }

    private static Dictionary<Subject, Teacher> Teachers(Subject focus) =>
        Subjects.All.ToDictionary(
            subject => subject,
            subject => subject == focus
                ? new Teacher(TeacherRule.Random50, default, default)
                : new Teacher(TeacherRule.CopyA, subject, default));

    private static bool[] AllAttend() => Subjects.All.Select(_ => true).ToArray();

    private static bool[] SkipOnly(Subject skip) => Subjects.All.Select(subject => subject != skip).ToArray();

    private static SemesterSimulator Create(Subject focus, bool[] plan, FakeRandom random, History history) =>
        new(Teachers(focus), random, new FixedStrategy(plan), history, history);

    [Fact]
    public void AttendAndAsked_RecordsAskAndKeepsPleasure()
    {
        var history = new History();
        var simulator = Create(Subject.Calculus, AllAttend(), new FakeRandom().AddDouble(0.25), history);

        simulator.AdvanceDay();

        Assert.False(simulator.Expelled);
        Assert.Equal(1, simulator.TotalPleasure);
        Assert.True(history.WasAsked(1, Subject.Calculus));
    }

    [Fact]
    public void AttendAndNotAsked_KeepsPleasure()
    {
        var history = new History();
        var simulator = Create(Subject.Calculus, AllAttend(), new FakeRandom().AddDouble(0.75), history);

        simulator.AdvanceDay();

        Assert.False(simulator.Expelled);
        Assert.Equal(1, simulator.TotalPleasure);
        Assert.False(history.WasAsked(1, Subject.Calculus));
    }

    [Fact]
    public void SkipAndNotAsked_AddsSkipPleasure()
    {
        var history = new History();
        var simulator = Create(Subject.Calculus, SkipOnly(Subject.Calculus), new FakeRandom().AddDouble(0.75), history);

        simulator.AdvanceDay();

        Assert.False(simulator.Expelled);
        Assert.Equal(2, simulator.TotalPleasure);
        Assert.Null(history.WasAsked(1, Subject.Calculus));
    }

    [Fact]
    public void SkipAndAsked_ExpelsAndResetsPleasure()
    {
        var history = new History();
        var simulator = Create(Subject.Calculus, SkipOnly(Subject.Calculus), new FakeRandom().AddDouble(0.25), history);

        simulator.AdvanceDay();

        Assert.True(simulator.Expelled);
        Assert.Equal(0, simulator.TotalPleasure);
        Assert.Equal(1, simulator.ExpulsionDay);
    }

    [Fact]
    public void YesterdayState_IsUpdated()
    {
        var history = new History();
        var teachers = Subjects.All.ToDictionary(
            subject => subject,
            subject => subject switch
            {
                Subject.Calculus => new Teacher(TeacherRule.Random50, default, default),
                Subject.Physics => new Teacher(TeacherRule.CopyA, Subject.Calculus, default),
                _ => new Teacher(TeacherRule.CopyA, subject, default)
            });

        var simulator = new SemesterSimulator(
            teachers,
            new FakeRandom().AddDouble(0.25).AddDouble(0.25),
            new FixedStrategy(AllAttend()),
            history,
            history);

        simulator.AdvanceDay();
        simulator.AdvanceDay();

        Assert.True(history.WasAsked(2, Subject.Physics));
    }

    [Fact]
    public void AfterExpulsion_NoFurtherDaysProcessed()
    {
        var history = new History();
        var simulator = Create(Subject.Calculus, SkipOnly(Subject.Calculus), new FakeRandom().AddDouble(0.25), history);

        simulator.AdvanceDay();

        var day = simulator.Day;
        simulator.AdvanceDay();

        Assert.True(simulator.Expelled);
        Assert.Equal(day, simulator.Day);
        Assert.Equal(0, simulator.TotalPleasure);
    }
}
