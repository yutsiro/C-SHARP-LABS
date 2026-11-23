namespace Truancy;

public interface IRandomSource
{
    int Next(int maxValue);
    double NextDouble();
}

public sealed class RandomSource : IRandomSource
{
    private readonly Random _random;

    public RandomSource(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public double NextDouble()
    {
        return _random.NextDouble();
    }
}
