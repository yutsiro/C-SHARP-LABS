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
}
