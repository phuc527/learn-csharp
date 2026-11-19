using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// ---------- Metrics: OpenTelemetry → endpoint /metrics cho Prometheus scrape ----------
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("weblab"))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation() // http_server_request_duration_seconds, active_requests, ...
        .AddMeter("System.Runtime")      // GC, memory, thread pool (built-in từ .NET 9)
        .AddMeter(GameMetrics.MeterName) // metric nghiệp vụ của mình
        .AddPrometheusExporter());

builder.Services.AddHealthChecks(); // Day sau thêm .AddNpgSql(), .AddRedis() cho readiness
builder.Logging.AddJsonConsole(); // log JSON 1 dòng, để Loki/ELK parse được

var app = builder.Build();

// Phục vụ file tĩnh trong wwwroot/ (index.html = frontend)
app.UseDefaultFiles();
app.UseStaticFiles();
// Endpoint vận hành: không tính vào metric HTTP để dashboard chỉ phản ánh traffic thật.
app.MapPrometheusScrapingEndpoint().DisableHttpMetrics(); // GET /metrics
app.MapHealthChecks("/health").DisableHttpMetrics();

// ---------- FizzBuzz: for + if/else ----------
app.MapGet("/api/fizzbuzz", (int n) =>
{
    if (n < 1 || n > 100)
    {
        return Results.BadRequest(new { error = "n phải từ 1 đến 100" });
    }

    var items = new List<string>();
    for (int i = 1; i <= n; i++)
    {
        if (i % 15 == 0) items.Add("FizzBuzz");
        else if (i % 3 == 0) items.Add("Fizz");
        else if (i % 5 == 0) items.Add("Buzz");
        else items.Add(i.ToString());
    }
    return Results.Ok(items);
});

// ---------- Đoán số: switch + foreach ----------
// ponytail: state chung trong RAM, mọi người chơi chung 1 ván, không thread-safe, mất khi restart. Day 20+ sẽ lưu DB.
int secret = Random.Shared.Next(1, 101);
var guesses = new List<int>();

app.MapPost("/api/guess", (GuessRequest req) =>
{
    if (req.Number < 1 || req.Number > 100)
    {
        return Results.BadRequest(new { error = "Số phải từ 1 đến 100" });
    }

    guesses.Add(req.Number);

    // switch expression với relational pattern (< 0, > 0)
    string result = (req.Number - secret) switch
    {
        0 => "correct",
        < 0 => "higher",
        > 0 => "lower",
    };
    GameMetrics.Guesses.Add(1, new KeyValuePair<string, object?>("result", result));

    // foreach: đếm số lần đoán "lệch xa" (cách > 20)
    int farMisses = 0;
    foreach (int g in guesses)
    {
        if (Math.Abs(g - secret) > 20) farMisses++;
    }

    var response = new GuessResponse(result, guesses.Count, farMisses);

    if (result == "correct")
    {
        GameMetrics.Attempts.Record(guesses.Count);
        secret = Random.Shared.Next(1, 101); // ván mới
        guesses.Clear();
    }
    return Results.Ok(response);
});

app.Run();

record GuessRequest(int Number);
record GuessResponse(string Result, int Attempts, int FarMisses);

// Metric nghiệp vụ. Tên "weblab.guesses" → Prometheus đổi thành weblab_guesses_total.
static class GameMetrics
{
    public const string MeterName = "WebLab.Game";
    static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> Guesses =
        Meter.CreateCounter<long>("weblab.guesses", description: "Số lần đoán, theo kết quả");

    // Mỗi ván thắng ghi lại số lần đoán. Bucket tự đặt vì mặc định là bucket cho mili-giây.
    public static readonly Histogram<int> Attempts = Meter.CreateHistogram<int>(
        "weblab.game.attempts",
        description: "Số lần đoán để thắng 1 ván",
        advice: new InstrumentAdvice<int> { HistogramBucketBoundaries = [1, 2, 3, 4, 5, 6, 7, 8, 10, 15, 20] });
}
