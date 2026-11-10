using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Truancy;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.ClearProviders())
    .ConfigureServices(services =>
    {
        services.AddSingleton<IRandomSource>(new RandomSource(Environment.TickCount));
        services.AddSingleton<History>();
        services.AddSingleton<IReadOnlyStudentHistory>(provider => provider.GetRequiredService<History>());
        services.AddSingleton<IStudentHistoryWriter>(provider => provider.GetRequiredService<History>());
        services.AddSingleton<IReadOnlyDictionary<Subject, Teacher>>(provider =>
            TeacherFactory.CreateTeachers(provider.GetRequiredService<IRandomSource>()));
        services.AddSingleton<ISkipStrategy, ReconStrategy>();
        services.AddSingleton<SemesterSimulator>();
        services.AddHostedService<DayLoopService>();
    })
    .Build();

IHostApplicationLifetime lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

Console.WriteLine("Симуляция запущена. Нажмите любую клавишу, чтобы остановить.");

_ = Task.Run(() =>
{
    Console.ReadKey(true);
    lifetime.StopApplication();
});

await host.RunAsync();
