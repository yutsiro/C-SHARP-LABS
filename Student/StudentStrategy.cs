namespace Truancy;

public interface ISkipStrategy
{
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}

public sealed class ReconStrategy : ISkipStrategy
{
    private const int ReconDays = 10;

    private abstract record TeacherGuess;

    private sealed record NeverGuess : TeacherGuess;

    private sealed record CopyGuess(Subject A) : TeacherGuess;

    private sealed record XorGuess(Subject A, Subject B) : TeacherGuess;

    private sealed record RandomGuess : TeacherGuess;

    private bool _classified;
    private readonly Dictionary<Subject, TeacherGuess> _guesses = new();

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        if (day <= ReconDays)
        {
            return Subjects.All.Select(_ => true).ToArray();
        }

        if (!_classified)
        {
            Classify(history);
            _classified = true;
        }

        var yesterday = Subjects.All.ToDictionary(
            subject => subject,
            subject => history.WasAsked(day - 1, subject) ?? false);

        return Subjects.All.Select(subject => ShouldAttend(subject, yesterday)).ToArray();
    }

    private bool ShouldAttend(Subject subject, IReadOnlyDictionary<Subject, bool> yesterday) =>
        _guesses[subject] switch
        {
            NeverGuess => false,
            CopyGuess copy => yesterday[copy.A],
            XorGuess xor => yesterday[xor.A] != yesterday[xor.B],
            _ => true
        };

    private void Classify(IReadOnlyStudentHistory history)
    {
        var ask = Subjects.All.ToDictionary(
            subject => subject,
            subject => new bool[ReconDays + 1]);

        foreach (var day in Enumerable.Range(1, ReconDays))
        {
            foreach (var subject in Subjects.All)
            {
                ask[subject][day] = history.WasAsked(day, subject) ?? false;
            }
        }

        foreach (var subject in Subjects.All)
        {
            _guesses[subject] = DetectGuess(ask, subject);
        }
    }

    private TeacherGuess DetectGuess(Dictionary<Subject, bool[]> ask, Subject subject)
    {
        if (Enumerable.Range(1, ReconDays).All(day => !ask[subject][day]))
        {
            return new NeverGuess();
        }

        foreach (var a in Subjects.All)
        {
            if (MatchesCopy(ask, subject, a))
            {
                return new CopyGuess(a);
            }
        }

        foreach (var a in Subjects.All)
        {
            foreach (var b in Subjects.All)
            {
                if (a != b && MatchesXor(ask, subject, a, b))
                {
                    return new XorGuess(a, b);
                }
            }
        }

        return new RandomGuess();
    }

    private bool MatchesCopy(Dictionary<Subject, bool[]> ask, Subject subject, Subject a) =>
        Enumerable.Range(2, ReconDays - 1).All(day => ask[subject][day] == ask[a][day - 1]);

    private bool MatchesXor(Dictionary<Subject, bool[]> ask, Subject subject, Subject a, Subject b) =>
        Enumerable.Range(2, ReconDays - 1).All(day => ask[subject][day] == (ask[a][day - 1] != ask[b][day - 1]));
}
