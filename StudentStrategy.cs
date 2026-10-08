namespace Truancy;

public interface ISkipStrategy
{
    string Name { get; }
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}

public sealed class ReconStrategy : ISkipStrategy
{
    private const int ReconDays = 10;

    private enum Prediction { Never, Copy, Xor, Random }

    private sealed record TeacherGuess(Prediction Prediction, Subject A = Subject.Calculus, Subject B = Subject.Calculus);

    private bool _classified;
    private readonly Dictionary<Subject, TeacherGuess> _guesses = new();

    public string Name => "Recon10";

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

        Dictionary<Subject, bool> yesterday = Subjects.All.ToDictionary(
            subject => subject,
            subject => history.WasAsked(day - 1, subject) ?? false);

        return Subjects.All.Select(subject => ShouldAttend(subject, yesterday)).ToArray();
    }

    private bool ShouldAttend(Subject subject, IReadOnlyDictionary<Subject, bool> yesterday)
    {
        TeacherGuess guess = _guesses[subject];

        return guess.Prediction switch
        {
            Prediction.Never => false,
            Prediction.Copy => yesterday[guess.A],
            Prediction.Xor => yesterday[guess.A] == yesterday[guess.B],
            _ => true
        };
    }

    private void Classify(IReadOnlyStudentHistory history)
    {
        Dictionary<Subject, bool[]> ask = Subjects.All.ToDictionary(
            subject => subject,
            subject => new bool[ReconDays + 1]);

        foreach (int day in Enumerable.Range(1, ReconDays))
        {
            foreach (Subject subject in Subjects.All)
            {
                ask[subject][day] = history.WasAsked(day, subject) ?? false;
            }
        }

        foreach (Subject subject in Subjects.All)
        {
            _guesses[subject] = DetectGuess(ask, subject);
        }
    }

    private TeacherGuess DetectGuess(Dictionary<Subject, bool[]> ask, Subject subject)
    {
        if (Enumerable.Range(1, ReconDays).All(day => !ask[subject][day]))
        {
            return new TeacherGuess(Prediction.Never);
        }

        foreach (Subject a in Subjects.All)
        {
            if (MatchesCopy(ask, subject, a))
            {
                return new TeacherGuess(Prediction.Copy, a);
            }
        }

        foreach (Subject a in Subjects.All)
        {
            foreach (Subject b in Subjects.All)
            {
                if (a != b && MatchesXor(ask, subject, a, b))
                {
                    return new TeacherGuess(Prediction.Xor, a, b);
                }
            }
        }

        return new TeacherGuess(Prediction.Random);
    }

    private bool MatchesCopy(Dictionary<Subject, bool[]> ask, Subject subject, Subject a) =>
        Enumerable.Range(2, ReconDays - 1).All(day => ask[subject][day] == ask[a][day - 1]);

    private bool MatchesXor(Dictionary<Subject, bool[]> ask, Subject subject, Subject a, Subject b) =>
        Enumerable.Range(2, ReconDays - 1).All(day => ask[subject][day] == (ask[a][day - 1] != ask[b][day - 1]));
}
