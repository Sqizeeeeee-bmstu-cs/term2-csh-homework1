using System.Diagnostics;

// ===== TOP-LEVEL STATEMENTS =====
BenchmarkResult MeasureTime(Func<int> func, string name)
{
    var sw = Stopwatch.StartNew();
    int result = func();
    sw.Stop();
    // Переводим в микросекунды (1 мс = 1000 мкс)
    double microseconds = sw.Elapsed.TotalMilliseconds * 1000;
    return new BenchmarkResult(name, microseconds, result);
}

int LevenshteinDistance(string s1, string s2)
{
    s1 = s1.ToUpper();
    s2 = s2.ToUpper();
    
    int m = s1.Length;
    int n = s2.Length;
    
    int[,] d = new int[m + 1, n + 1];
    
    for (int i = 0; i <= m; i++) d[i, 0] = i;
    for (int j = 0; j <= n; j++) d[0, j] = j;
    
    for (int i = 1; i <= m; i++)
    {
        for (int j = 1; j <= n; j++)
        {
            int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
            int delete = d[i - 1, j] + 1;
            int insert = d[i, j - 1] + 1;
            int replace = d[i - 1, j - 1] + cost;
            d[i, j] = Math.Min(Math.Min(delete, insert), replace);
        }
    }
    
    return d[m, n];
}

int DamerauLevenshteinDistance(string s1, string s2)
{
    s1 = s1.ToUpper();
    s2 = s2.ToUpper();
    
    int m = s1.Length;
    int n = s2.Length;
    
    int[,] d = new int[m + 1, n + 1];
    
    for (int i = 0; i <= m; i++) d[i, 0] = i;
    for (int j = 0; j <= n; j++) d[0, j] = j;
    
    for (int i = 1; i <= m; i++)
    {
        for (int j = 1; j <= n; j++)
        {
            int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
            
            int delete = d[i - 1, j] + 1;
            int insert = d[i, j - 1] + 1;
            int replace = d[i - 1, j - 1] + cost;
            
            d[i, j] = Math.Min(Math.Min(delete, insert), replace);
            
            if (i > 1 && j > 1 && 
                s1[i - 1] == s2[j - 2] && 
                s1[i - 2] == s2[j - 1])
            {
                int transpose = d[i - 2, j - 2] + cost;
                d[i, j] = Math.Min(d[i, j], transpose);
            }
        }
    }
    
    return d[m, n];
}

int DamerauLevenshteinOptimized(string s1, string s2)
{
    s1 = s1.ToUpper();
    s2 = s2.ToUpper();
    
    if (s1.Length < s2.Length)
        (s1, s2) = (s2, s1);
    
    int m = s1.Length;
    int n = s2.Length;
    
    int[] prevPrev = new int[n + 1];
    int[] prev = new int[n + 1];
    int[] curr = new int[n + 1];
    
    for (int j = 0; j <= n; j++)
        prev[j] = j;
    
    for (int i = 1; i <= m; i++)
    {
        curr[0] = i;
        
        for (int j = 1; j <= n; j++)
        {
            int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
            
            int delete = prev[j] + 1;
            int insert = curr[j - 1] + 1;
            int replace = prev[j - 1] + cost;
            
            curr[j] = Math.Min(Math.Min(delete, insert), replace);
            
            if (i > 1 && j > 1 && 
                s1[i - 1] == s2[j - 2] && 
                s1[i - 2] == s2[j - 1])
            {
                int transpose = prevPrev[j - 2] + cost;
                curr[j] = Math.Min(curr[j], transpose);
            }
        }
        
        int[] temp = prevPrev;
        prevPrev = prev;
        prev = curr;
        curr = temp;
    }
    
    return prev[n];
}

// Тестовые данные
var testPairs = new (string, string)[]
{
    // Короткие строки из методички
    ("пример", "1пример"),
    ("пример", "при1мер"),
    ("пример", "пример1"),
    ("пример", "12пример"),
    ("пример", "при12мер"),
    ("пример", "пример12"),
    ("пример", "1при2мер3"),
    ("приМер", "прМир"),
    ("ИВАНОВ", "БАННВО"),
    ("ПРИМЕРЫ", "ПРЕДМЕТ"),
    
    // Строки подлиннее для заметной разницы в скорости
    ("программирование", "програмирование"),
    ("алгоритмизация", "алгоримизация"),
    ("реализация", "реолизоция"),
    ("транспозиция", "траспозиция"),
    ("конкатенация", "конкотенация"),
    
    // Совсем длинные строки
    ("параллелепипед", "паралелепипед"),
    ("достопримечательность", "достопримечательность"),
    ("электроэнцефалограф", "электроэнцефалограф")
};

// Вывод информации о системе
Console.WriteLine("=== Информация о системе ===\n");
Console.WriteLine($"Частота таймера: {Stopwatch.Frequency:N0} тиков/сек");
Console.WriteLine($"Точность: {1_000_000.0 / Stopwatch.Frequency:F3} мкс на тик\n");

Console.WriteLine("=== Сравнение производительности алгоритмов ===\n");
Console.WriteLine($"{"Строка1",-25} {"Строка2",-25} {"Алгоритм",-15} {"Результат",-10} {"Время (мкс)",-15}");
Console.WriteLine(new string('-', 105));

foreach (var (s1, s2) in testPairs)
{
    var levResult = MeasureTime(() => LevenshteinDistance(s1, s2), "Левенштейн");
    var damResult = MeasureTime(() => DamerauLevenshteinDistance(s1, s2), "Дамерау-Лев");
    var optResult = MeasureTime(() => DamerauLevenshteinOptimized(s1, s2), "Дамерау-Лев (опт)");
    
    Console.WriteLine($"{s1,-25} {s2,-25} {levResult.Name,-15} {levResult.Result,-10} {levResult.Microseconds,15:F3}");
    Console.WriteLine($"{s1,-25} {s2,-25} {damResult.Name,-15} {damResult.Result,-10} {damResult.Microseconds,15:F3}");
    Console.WriteLine($"{s1,-25} {s2,-25} {optResult.Name,-15} {optResult.Result,-10} {optResult.Microseconds,15:F3}");
    Console.WriteLine(new string('-', 105));
}

// Дополнительный тест: много повторений для статистики
Console.WriteLine("\n=== Тест с многократным выполнением (10000 раз) ===\n");

string longStr1 = "программирование";
string longStr2 = "програмирование";

Console.WriteLine($"Строки: '{longStr1}' <-> '{longStr2}'\n");

var levTotal = MeasureTime(() => {
    int sum = 0;
    for (int i = 0; i < 10000; i++)
        sum += LevenshteinDistance(longStr1, longStr2);
    return sum;
}, "Левенштейн x10000");

var damTotal = MeasureTime(() => {
    int sum = 0;
    for (int i = 0; i < 10000; i++)
        sum += DamerauLevenshteinDistance(longStr1, longStr2);
    return sum;
}, "Дамерау-Лев x10000");

var optTotal = MeasureTime(() => {
    int sum = 0;
    for (int i = 0; i < 10000; i++)
        sum += DamerauLevenshteinOptimized(longStr1, longStr2);
    return sum;
}, "Дамерау-Лев (опт) x10000");

Console.WriteLine($"{levTotal.Name,-25} {levTotal.Microseconds,15:F3} мкс (всего)");
Console.WriteLine($"{levTotal.Name,-25} {levTotal.Microseconds / 10000,15:F3} мкс (в среднем)");
Console.WriteLine();
Console.WriteLine($"{damTotal.Name,-25} {damTotal.Microseconds,15:F3} мкс (всего)");
Console.WriteLine($"{damTotal.Name,-25} {damTotal.Microseconds / 10000,15:F3} мкс (в среднем)");
Console.WriteLine();
Console.WriteLine($"{optTotal.Name,-25} {optTotal.Microseconds,15:F3} мкс (всего)");
Console.WriteLine($"{optTotal.Name,-25} {optTotal.Microseconds / 10000,15:F3} мкс (в среднем)");

// Анализ результатов
Console.WriteLine("\n=== Анализ ===\n");

double levAvg = levTotal.Microseconds / 10000;
double damAvg = damTotal.Microseconds / 10000;
double optAvg = optTotal.Microseconds / 10000;

Console.WriteLine("1. Левенштейн vs Дамерау-Левенштейн:");
Console.WriteLine($"   - Дамерау-Левенштейн медленнее на {(damAvg/levAvg - 1)*100:F1}%");

Console.WriteLine("2. Обычная vs Оптимизированная по памяти:");
Console.WriteLine($"   - Оптимизированная версия {(optAvg < damAvg ? "быстрее" : "медленнее")} на {Math.Abs(optAvg/damAvg - 1)*100:F1}%");

Console.WriteLine("3. Абсолютные цифры для данной системы:");
Console.WriteLine($"   - Левенштейн: {levAvg:F3} мкс в среднем");
Console.WriteLine($"   - Дамерау-Лев: {damAvg:F3} мкс в среднем"); 
Console.WriteLine($"   - Оптимизированный: {optAvg:F3} мкс в среднем");

// ===== TYPE DECLARATIONS =====
record BenchmarkResult(string Name, double Microseconds, int Result);