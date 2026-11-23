namespace Truancy;

public interface IReadOnlyStudentHistory
{
    bool Attended(int day, Subject subject);
    bool? WasAsked(int day, Subject subject);
}

public interface IStudentHistoryWriter
{
    void RecordAttended(int day, Subject subject, bool asked);
    void RecordSkipped(int day, Subject subject);
}

public sealed class History : IReadOnlyStudentHistory, IStudentHistoryWriter
{
    private readonly List<Dictionary<Subject, bool>> _attended = new();
    private readonly List<Dictionary<Subject, bool?>> _asked = new();

    public void RecordAttended(int day, Subject subject, bool asked)
    {
        EnsureDay(day);
        _attended[day - 1][subject] = true;
        _asked[day - 1][subject] = asked;
    }

    public void RecordSkipped(int day, Subject subject)
    {
        EnsureDay(day);
        _attended[day - 1][subject] = false;
        _asked[day - 1][subject] = null;
    }

    public bool Attended(int day, Subject subject) =>
        day - 1 < _attended.Count && _attended[day - 1].GetValueOrDefault(subject);

    public bool? WasAsked(int day, Subject subject) =>
        day - 1 < _asked.Count ? _asked[day - 1].GetValueOrDefault(subject) : null;

    private void EnsureDay(int day)
    {
        while (_attended.Count < day)
        {
            _attended.Add(new());
            _asked.Add(new());
        }
    }
}
