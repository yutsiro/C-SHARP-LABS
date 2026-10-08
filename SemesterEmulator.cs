namespace Truancy;

public sealed record SemesterResult(int TotalPleasure, bool Expelled, int ExpulsionDay);

public sealed class SemesterEmulator
{
    private readonly IReadOnlyDictionary<Subject, Teacher> _teachers;
    private readonly IRandomSource _random;
    private readonly ISkipStrategy _strategy;
    private readonly History _history;

    public SemesterEmulator(IReadOnlyDictionary<Subject, Teacher> teachers, IRandomSource random, ISkipStrategy strategy)
    {
        _teachers = teachers;
        _random = random;
        _strategy = strategy;
        _history = new History();
    }

    public List<string> Log { get; } = new List<string>();

    public SemesterResult Run()
    {
        int totalPleasure = 0;
        bool expelled = false;
        int expulsionDay = -1;

        Dictionary<Subject, bool> yesterdayAsked = new();

        foreach (int day in Enumerable.Range(1, Semester.Days))
        {
            bool[] plan = _strategy.DecideDay(day, _history);

            Dictionary<Subject, bool> decision = new();
            foreach ((Subject subject, bool attend) in Subjects.All.Zip(plan))
            {
                decision[subject] = attend;
            }

            Dictionary<Subject, bool> todayAsked = new();

            foreach (Subject subject in Subjects.All)
            {
                bool willAsk = _teachers[subject].WillAsk(yesterdayAsked, _random);
                todayAsked[subject] = willAsk;

                if (decision[subject])
                {
                    _history.RecordAttended(day, subject, willAsk);
                }
                else if (willAsk)
                {
                    expelled = true;
                    expulsionDay = day;
                    _history.RecordSkipped(day, subject);
                    break;
                }
                else
                {
                    _history.RecordSkipped(day, subject);
                }
            }

            Log.Add(BuildLogLine(day, decision, expelled));

            if (expelled)
            {
                totalPleasure = 0;
                break;
            }

            totalPleasure += 1 + decision.Count(pair => !pair.Value);

            yesterdayAsked = todayAsked;
        }

        return new SemesterResult(totalPleasure, expelled, expulsionDay);
    }

    private static string BuildLogLine(int day, IReadOnlyDictionary<Subject, bool> decision, bool expelled)
    {
        string skippedText = string.Join(", ", Subjects.All.Where(s => !decision[s]).Select(Subjects.Name));

        if (skippedText.Length == 0)
        {
            skippedText = "ничего";
        }

        return expelled
            ? $"День {day}: прогулял — {skippedText} — ВЫЛЕТ"
            : $"День {day}: прогулял — {skippedText}";
    }
}
