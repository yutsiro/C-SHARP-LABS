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
    private readonly Dictionary<Subject, bool>[] _attended;
    private readonly Dictionary<Subject, bool?>[] _asked;

    public History()
    {
        _attended = new Dictionary<Subject, bool>[Semester.Days + 1];
        _asked = new Dictionary<Subject, bool?>[Semester.Days + 1];

        foreach (int day in Enumerable.Range(1, Semester.Days))
        {
            _attended[day] = new Dictionary<Subject, bool>();
            _asked[day] = new Dictionary<Subject, bool?>();
        }
    }

    public void RecordAttended(int day, Subject subject, bool asked)
    {
        _attended[day][subject] = true;
        _asked[day][subject] = asked;
    }

    public void RecordSkipped(int day, Subject subject)
    {
        _attended[day][subject] = false;
        _asked[day][subject] = null;
    }

    public bool Attended(int day, Subject subject) => _attended[day].GetValueOrDefault(subject);

    public bool? WasAsked(int day, Subject subject) => _asked[day].GetValueOrDefault(subject);
}
