using Truancy;

IRandomSource random = new RandomSource(Environment.TickCount);

var teachers = TeacherFactory.CreateTeachers(random);

ISkipStrategy strategy = new ReconStrategy();

SemesterEmulator emulator = new SemesterEmulator(teachers, random, strategy);
SemesterResult result = emulator.Run();

double average = result.TotalPleasure / (double)Semester.Days;

Console.WriteLine("Результаты семестра:");
Console.WriteLine($"Стратегия: {strategy.Name}");
Console.WriteLine($"Вылетел: {(result.Expelled ? "да" : "нет")}");

if (result.Expelled)
{
    Console.WriteLine($"День вылета: {result.ExpulsionDay}");
}

Console.WriteLine($"Общее удовольствие: {result.TotalPleasure}");
Console.WriteLine($"Среднее удовольствие в день: {average:F2}");

File.WriteAllLines("semester_log.txt", emulator.Log);
Console.WriteLine("Лог записан в semester_log.txt");
