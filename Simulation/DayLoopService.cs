using Microsoft.Extensions.Hosting;

namespace Truancy;

public sealed class DayLoopService : BackgroundService
{
    private static readonly TimeSpan DayInterval = TimeSpan.FromSeconds(5);

    private readonly SemesterSimulator _simulator;
    private readonly IHostApplicationLifetime _lifetime;

    public DayLoopService(SemesterSimulator simulator, IHostApplicationLifetime lifetime)
    {
        _simulator = simulator;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _simulator.AdvanceDay();

            Console.WriteLine(
                $"День {_simulator.Day}: всего {_simulator.TotalPleasure}, в среднем {_simulator.AveragePleasure:F2}");

            if (_simulator.Expelled)
            {
                Console.WriteLine($"Студент вылетел на дне {_simulator.ExpulsionDay}. Остановка.");
                _lifetime.StopApplication();
                return;
            }

            await Task.Delay(DayInterval, stoppingToken);
        }
    }
}
