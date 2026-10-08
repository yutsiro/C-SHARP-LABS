namespace Truancy;

public enum Subject
{
    Calculus,
    LinearAlgebra,
    DiscreteMath,
    Programming,
    Physics,
    English
}

public static class Subjects
{
    public static readonly Subject[] All = Enum.GetValues<Subject>();

    public static string Name(Subject subject) => subject switch
    {
        Subject.Calculus => "Матанализ",
        Subject.LinearAlgebra => "Линейная алгебра",
        Subject.DiscreteMath => "Дискретная математика",
        Subject.Programming => "Программирование",
        Subject.Physics => "Физика",
        Subject.English => "Английский",
        _ => subject.ToString()
    };
}

public static class Semester
{
    public const int Days = 100;
}
