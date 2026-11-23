namespace Truancy;

public sealed class Teacher(TeacherRule rule, Subject a, Subject b)
{
    public TeacherRule Rule { get; } = rule;
    public Subject A { get; } = a;
    public Subject B { get; } = b;

    public bool WillAsk(IReadOnlyDictionary<Subject, bool> yesterdayAsked, IRandomSource random) => Rule switch
    {
        TeacherRule.Random50 => random.NextDouble() < 0.5,
        TeacherRule.CopyA => yesterdayAsked.GetValueOrDefault(A),
        _ => yesterdayAsked.GetValueOrDefault(A) != yesterdayAsked.GetValueOrDefault(B)
    };
}

public static class TeacherFactory
{
    public static Dictionary<Subject, Teacher> CreateTeachers(IRandomSource random) =>
        Subjects.All.ToDictionary(subject => subject, _ => CreateTeacher(random));

    private static Teacher CreateTeacher(IRandomSource random)
    {
        TeacherRule[] rules = Enum.GetValues<TeacherRule>();
        TeacherRule rule = rules[random.Next(rules.Length)];

        if (rule == TeacherRule.Random50)
        {
            return new Teacher(rule, default, default);
        }

        Subject a = RandomSubject(random);

        return rule == TeacherRule.CopyA
            ? new Teacher(rule, a, default)
            : new Teacher(rule, a, RandomSubjectExcept(random, a));
    }

    private static Subject RandomSubject(IRandomSource random) =>
        Subjects.All[random.Next(Subjects.All.Length)];

    private static Subject RandomSubjectExcept(IRandomSource random, Subject except) =>
        Subjects.All.Where(s => s != except).ElementAt(random.Next(Subjects.All.Length - 1));
}
