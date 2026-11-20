// Điểm khởi động: cấu hình, nối các layer (DI), middleware, route.
// Chạy (trong thư mục backend/Seatly.Api): dotnet run → http://localhost:8080
// Go: backend/cmd/api/main.go
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Seatly.Api.Http;
using Seatly.Api.Repositories;
using Seatly.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Log ----------
builder.Logging.ClearProviders().AddJsonConsole(); // 1 dòng JSON / sự kiện, Loki/Datadog đọc được
builder.Services.AddHttpLogging(o =>
{
    // Mỗi request 1 dòng log: method, path, status, thời gian (giống middleware logRequests bên Go)
    o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
    o.CombineLogs = true;
});

// ---------- Giới hạn & JSON ----------
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 1_048_576); // 1 MB
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow); // chặn field lạ
// Body sai → ném BadHttpRequestException để exception handler bên dưới trả envelope (mặc định chỉ ném ở Development).
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

// ---------- Dependency Injection: repo → service ----------
// Go nối tay: repo := memory.New...; svc := service.New...(repo). C# đăng ký, framework tự nối.
builder.Services.AddSingleton<ISeatRepository>(new InMemorySeatRepository(InMemorySeatRepository.SeedSeats()));
builder.Services.AddSingleton<BookingService>();

// ---------- Health + Metrics ----------
builder.Services.AddHealthChecks();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("seatly-api"))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation() // http_server_request_duration_seconds, ...
        .AddMeter("System.Runtime")      // GC, memory, thread pool
        .AddMeter(SeatEndpoints.MeterName)
        .AddPrometheusExporter());

var app = builder.Build();

// ---------- Middleware (thứ tự quan trọng, giống app.use của Express) ----------
app.UseHttpLogging();
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    // Mọi exception chưa bắt rơi vào đây. Client chỉ thấy câu chung, chi tiết nằm trong log.
    // Log mặc định của middleware đã tắt trong appsettings.json: body sai là lỗi của client, không log Error.
    var error = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    IResult result = error is BadHttpRequestException bad
        ? Api.Error(bad.StatusCode, """Body phải có dạng {"seatId": "A1"}""")
        : Api.Error(StatusCodes.Status500InternalServerError, "Lỗi máy chủ, thử lại sau");
    if (error is not BadHttpRequestException)
    {
        app.Logger.LogError(error, "Unhandled exception {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
    }
    await result.ExecuteAsync(ctx);
}));

// ---------- Route ----------
// ponytail: /metrics chung cổng với API cho gọn; nginx của FE chỉ chuyển /api nên user ngoài không thấy.
app.MapPrometheusScrapingEndpoint().DisableHttpMetrics();
app.MapHealthChecks("/healthz").DisableHttpMetrics();
app.MapSeatEndpoints();

// Graceful shutdown (Ctrl+C / docker stop) có sẵn: host chờ request đang chạy xong rồi mới tắt.
app.Run();

// Để project test truy cập được Program (WebApplicationFactory<Program>)
public partial class Program;
