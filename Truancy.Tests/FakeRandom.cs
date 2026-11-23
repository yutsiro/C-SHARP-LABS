using Truancy;

namespace Truancy.Tests;

internal sealed class FakeRandom : IRandomSource
{
    private readonly Queue<int> _ints = new();
    private readonly Queue<double> _doubles = new();

    public FakeRandom AddInt(int value)
    {
        _ints.Enqueue(value);
        return this;
    }

    public FakeRandom AddDouble(double value)
    {
        _doubles.Enqueue(value);
        return this;
    }

    public int Next(int maxValue) =>
        _ints.TryDequeue(out var value)
            ? value
            : throw new InvalidOperationException("FakeRandom: значение для Next не задано.");

    public double NextDouble() =>
        _doubles.TryDequeue(out var value)
            ? value
            : throw new InvalidOperationException("FakeRandom: значение для NextDouble не задано.");
}
