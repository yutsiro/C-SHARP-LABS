namespace Truancy;

public sealed class Teacher(Subject subject, TeacherRule rule, Subject a, Subject b)
{
    public Subject Subject { get; } = subject;
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
        Subjects.All.ToDictionary(subject => subject, subject => CreateTeacher(random, subject));

    private static Teacher CreateTeacher(IRandomSource random, Subject subject)
    {
        TeacherRule[] rules = Enum.GetValues<TeacherRule>();
        TeacherRule rule = rules[random.Next(rules.Length)];

        Subject a = subject;
        Subject b = subject;

        if (rule == TeacherRule.CopyA)
        {
            a = RandomSubject(random);
        }
        else if (rule == TeacherRule.XorAB)
        {
            a = RandomSubject(random);
            b = RandomSubjectExcept(random, a);
        }

        return new Teacher(subject, rule, a, b);
    }

    private static Subject RandomSubject(IRandomSource random) =>
        Subjects.All[random.Next(Subjects.All.Length)];

    private static Subject RandomSubjectExcept(IRandomSource random, Subject except) =>
        Subjects.All.Where(s => s != except).ElementAt(random.Next(Subjects.All.Length - 1));
}
