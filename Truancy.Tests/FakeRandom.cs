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

    public int Next(int maxValue) => _ints.Count > 0 ? _ints.Dequeue() : 0;

    public double NextDouble() => _doubles.Count > 0 ? _doubles.Dequeue() : 0.0;
}
