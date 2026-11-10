namespace Truancy;

public sealed class SemesterSimulator
{
    private readonly IReadOnlyDictionary<Subject, Teacher> _teachers;
    private readonly IRandomSource _random;
    private readonly ISkipStrategy _strategy;
    private readonly IStudentHistoryWriter _historyWriter;
    private readonly IReadOnlyStudentHistory _historyReader;

    private Dictionary<Subject, bool> _yesterdayAsked = new();
    private int _day;
    private int _totalPleasure;
    private bool _expelled;
    private int _expulsionDay = -1;

    public SemesterSimulator(
        IReadOnlyDictionary<Subject, Teacher> teachers,
        IRandomSource random,
        ISkipStrategy strategy,
        IStudentHistoryWriter historyWriter,
        IReadOnlyStudentHistory historyReader)
    {
        _teachers = teachers;
        _random = random;
        _strategy = strategy;
        _historyWriter = historyWriter;
        _historyReader = historyReader;
    }

    public int Day => _day;
    public int TotalPleasure => _totalPleasure;
    public bool Expelled => _expelled;
    public int ExpulsionDay => _expulsionDay;
    public double AveragePleasure => _day == 0 ? 0.0 : _totalPleasure / (double)_day;

    public void AdvanceDay()
    {
        if (_expelled)
        {
            return;
        }

        _day++;

        bool[] plan = _strategy.DecideDay(_day, _historyReader);
        Dictionary<Subject, bool> decision = Subjects.All.Zip(plan)
            .ToDictionary(pair => pair.First, pair => pair.Second);

        Dictionary<Subject, bool> todayAsked = new();
        int skipped = 0;

        foreach (Subject subject in Subjects.All)
        {
            bool willAsk = _teachers[subject].WillAsk(_yesterdayAsked, _random);
            todayAsked[subject] = willAsk;

            if (decision[subject])
            {
                _historyWriter.RecordAttended(_day, subject, willAsk);
            }
            else if (willAsk)
            {
                _expelled = true;
                _expulsionDay = _day;
                _totalPleasure = 0;
                _historyWriter.RecordSkipped(_day, subject);
                return;
            }
            else
            {
                _historyWriter.RecordSkipped(_day, subject);
                skipped++;
            }
        }

        _totalPleasure += 1 + skipped;
        _yesterdayAsked = todayAsked;
    }
}
