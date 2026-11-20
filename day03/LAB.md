# Day 3 (21/11/2026): if / switch / vòng lặp, bản fullstack

**Mục tiêu:** làm một web app nhỏ, gồm backend C# (ASP.NET Core Minimal API) và frontend HTML/JS, chạy được bằng `dotnet run` và bằng Docker.

- FizzBuzz: luyện `for` + `if/else`
- Đoán số 1-100: luyện `switch` + `foreach`

`while` bạn đã dùng ở Day 2 (vòng lặp hỏi lại trong `ReadDecimal`).

**Done khi:**

- Mở http://localhost:5xxx chơi đoán số được đến khi đoán đúng.
- Nhập `n=0` vào FizzBuzz thì hiện lỗi, không crash.
- `docker run` cũng chơi được.

> Code đầy đủ (kèm monitoring) đã có sẵn trong `WebLab/` và `ops/`. Các bước 1-5 bên dưới là bản rút gọn, chưa có monitoring, để bạn tự gõ lại từ đầu trong một thư mục khác.

## Chạy nhanh toàn bộ stack

```bash
cd day03
docker compose up -d --build            # app + Prometheus + Grafana
docker compose --profile load up -d     # (tuỳ chọn) traffic giả để dashboard có số
docker compose down                     # tắt; thêm -v để xoá luôn dữ liệu Prometheus/Grafana
```

| URL                           | Là gì                                                                                                                   |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| http://localhost:8080         | Web app (FizzBuzz + Đoán số)                                                                                            |
| http://localhost:8080/metrics | Metric thô, định dạng Prometheus                                                                                        |
| http://localhost:8080/health  | Health check, trả về `Healthy`                                                                                          |
| http://localhost:9090         | Prometheus: tab **Status → Targets** (phải thấy `UP`), tab **Alerts** (3 rule)                                          |
| http://localhost:3000         | Grafana (admin/admin). Home là **WebLab Overview**; trong folder WebLab có thêm 2 dashboard chính thức của ASP.NET Core |

Chạy không cần Docker: `cd WebLab && dotnet run`. Khi đó vẫn có `/metrics`, nhưng không có Prometheus/Grafana.

## Monitoring hoạt động thế nào

```
WebLab (OpenTelemetry SDK) ──GET /metrics mỗi 15s── Prometheus ──PromQL── Grafana
                                                        └─ alerts.yml (rule cảnh báo)
```

| File                                      | Vai trò                                                                                                                                                                                   |
| ----------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WebLab/Program.cs` (`AddOpenTelemetry`)  | Thu metric HTTP (built-in của ASP.NET Core), runtime (GC, memory, thread pool), metric nghiệp vụ `GameMetrics`                                                                            |
| `ops/prometheus/prometheus.yml`           | Scrape `weblab:8080/metrics`                                                                                                                                                              |
| `ops/prometheus/alerts.yml`               | 3 cảnh báo: app down, tỉ lệ 5xx > 5%, p95 > 500ms                                                                                                                                         |
| `ops/grafana/provisioning/`               | Datasource + dashboard được nạp tự động khi Grafana khởi động (cấu hình dạng code, không click tay)                                                                                       |
| `ops/grafana/dashboards/weblab.json`      | Dashboard tự viết: RED (Rate/Errors/Duration) + nghiệp vụ + .NET runtime                                                                                                                  |
| `ops/grafana/dashboards/aspnetcore*.json` | Dashboard chính thức [19924](https://grafana.com/grafana/dashboards/19924) / [19925](https://grafana.com/grafana/dashboards/19925) từ team ASP.NET Core (repo JamesNK/aspnetcore-grafana) |

**Các điểm "kiểu production" trong code:**

- **Metric HTTP** dùng `http_server_request_duration_seconds` đúng chuẩn OpenTelemetry semantic conventions, nên dashboard cộng đồng dùng lại được.
- **`/metrics` và `/health`** gắn `.DisableHttpMetrics()`, để traffic từ Prometheus/load balancer không làm sai số liệu.
- **Metric nghiệp vụ** (`weblab_guesses_total{result}`, histogram `weblab_game_attempts`) mới là thứ sếp hỏi; metric kỹ thuật chỉ là phần nền.
- **Label phải có ít giá trị** (`result` chỉ có 3 giá trị). Tuyệt đối không dùng userId hay số đoán làm label, vì mỗi giá trị khác nhau tạo ra một time series mới (cardinality explosion).
- **Log JSON** (`AddJsonConsole`) để Loki/ELK parse được (tuần 18).
- **Vận hành:** image ghim đúng version, mật khẩu Grafana lấy từ biến môi trường, dashboard không sửa được trên UI (`allowUiUpdates: false`), container chạy user non-root (`USER $APP_UID` trong Dockerfile; image `aspnet` có sẵn user `app` nhưng mặc định vẫn chạy root).

**Thử ngay:**

1. Chạy `--profile load`, mở Grafana và xem số nhảy. Panel "Tỉ lệ lỗi" khoảng 15%, vì loadgen cố ý gửi `n=0` hoặc `n>100`.
2. Prometheus → Graph, gõ `rate(weblab_guesses_total[1m])`.
3. `docker compose stop weblab`, chờ khoảng 1 phút, xem alert `WebLabDown` chuyển Pending → Firing. Sau đó `docker compose start weblab`.

Chưa làm (để tuần 18): trace (Tempo), log tập trung (Loki), Alertmanager gửi Slack, exemplars.

---

## Bước 1: Tạo project (5 phút)

```bash
cd day03
dotnet new web -n WebLab -f net10.0
cd WebLab
mkdir wwwroot
```

`dotnet new web` tạo project ASP.NET Core rỗng. Khác với `console`, file `.csproj` của nó dùng `Sdk="Microsoft.NET.Sdk.Web"`.

## Bước 2: Backend `Program.cs` (30 phút)

Thay toàn bộ nội dung `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Phục vụ file tĩnh trong wwwroot/ (index.html = frontend)
app.UseDefaultFiles();
app.UseStaticFiles();

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
// ponytail: state chung trong RAM, mọi người chơi chung 1 ván; mất khi restart. Day 20+ sẽ lưu DB.
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

    // foreach: đếm số lần đoán "lệch xa" (cách > 20)
    int farMisses = 0;
    foreach (int g in guesses)
    {
        if (Math.Abs(g - secret) > 20) farMisses++;
    }

    var response = new GuessResponse(result, guesses.Count, farMisses);

    if (result == "correct")
    {
        secret = Random.Shared.Next(1, 101); // ván mới
        guesses.Clear();
    }
    return Results.Ok(response);
});

app.Run();

record GuessRequest(int Number);
record GuessResponse(string Result, int Attempts, int FarMisses);
```

**Giải thích nhanh:**

| Code                                                  | Ý nghĩa                                                                       | Tương đương TS/Go                        |
| ----------------------------------------------------- | ----------------------------------------------------------------------------- | ---------------------------------------- |
| `app.MapGet("/api/fizzbuzz", (int n) => ...)`         | Route GET. `n` lấy tự động từ query `?n=15`; nếu không phải số thì tự trả 400 | Express `app.get` + parse query          |
| `app.MapPost(..., (GuessRequest req) => ...)`         | Body JSON tự map vào `req` (không phân biệt hoa thường: `number` → `Number`)  | `req.body` + zod                         |
| `Results.Ok(x)` / `Results.BadRequest(x)`             | Trả về 200 / 400 kèm JSON                                                     | `res.status(400).json()`                 |
| `record GuessRequest(int Number);`                    | Kiểu dữ liệu bất biến, 1 dòng                                                 | `type GuessRequest = { number: number }` |
| `(a - b) switch { 0 => ..., < 0 => ..., > 0 => ... }` | Switch expression trả về giá trị, dùng relational pattern                     | `match` / chuỗi ternary                  |
| `foreach (int g in guesses)`                          | Duyệt từng phần tử                                                            | `for (const g of guesses)`               |
| `record` khai báo **sau** `app.Run()`                 | Top-level statements bắt buộc khai báo type ở cuối file                       |                                          |

## Bước 3: Frontend `wwwroot/index.html` (20 phút)

```html
<!doctype html>
<html lang="vi">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Day 3 Lab</title>
    <style>
      body {
        font-family: system-ui, sans-serif;
        max-width: 560px;
        margin: 2rem auto;
        padding: 0 1rem;
      }
      section {
        border: 1px solid #ccc;
        border-radius: 8px;
        padding: 1rem;
        margin-bottom: 1rem;
      }
      .error {
        color: #b00020;
      }
      ol {
        columns: 4;
      }
    </style>
  </head>
  <body>
    <h1>Day 3: if / switch / loops</h1>

    <section>
      <h2>FizzBuzz</h2>
      <form id="fb-form">
        <label for="fb-n">Đến số (1-100):</label>
        <input id="fb-n" type="number" min="1" max="100" value="15" required />
        <button>Chạy</button>
      </form>
      <p id="fb-error" class="error" role="alert"></p>
      <ol id="fb-list"></ol>
    </section>

    <section>
      <h2>Đoán số (1-100)</h2>
      <form id="g-form">
        <label for="g-n">Số bạn đoán:</label>
        <input id="g-n" type="number" min="1" max="100" required />
        <button>Đoán</button>
      </form>
      <p id="g-result" aria-live="polite"></p>
    </section>

    <script>
      const messages = {
        higher: "Lớn hơn nữa ⬆️",
        lower: "Nhỏ hơn nữa ⬇️",
        correct: "🎉 Chính xác!",
      };

      document
        .getElementById("fb-form")
        .addEventListener("submit", async (e) => {
          e.preventDefault();
          const n = document.getElementById("fb-n").value;
          const res = await fetch(`/api/fizzbuzz?n=${n}`);
          const data = await res.json();
          document.getElementById("fb-error").textContent = res.ok
            ? ""
            : data.error;
          document.getElementById("fb-list").innerHTML = res.ok
            ? data.map((x) => `<li>${x}</li>`).join("")
            : "";
        });

      document
        .getElementById("g-form")
        .addEventListener("submit", async (e) => {
          e.preventDefault();
          const input = document.getElementById("g-n");
          const res = await fetch("/api/guess", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ number: Number(input.value) }),
          });
          const data = await res.json();
          document.getElementById("g-result").textContent = res.ok
            ? `${messages[data.result]} (lần ${data.attempts}, đoán lệch xa ${data.farMisses} lần)`
            : data.error;
          input.select();
        });
    </script>
  </body>
</html>
```

`UseDefaultFiles()` + `UseStaticFiles()` cho phép backend tự phục vụ `index.html`. Vì frontend và API cùng origin nên không cần CORS. Khi lên React + Vite (tuần 4) sẽ phải xử lý chuyện này.

## Bước 4: Chạy và debug (10 phút)

```bash
dotnet run
```

- Mở URL in ra trong terminal (dạng `http://localhost:5xxx`).
- Gọi thử API trực tiếp: `http://localhost:5xxx/api/fizzbuzz?n=15`.
- **Debug:** đặt breakpoint ở dòng `string result = ...`, bấm F5, đoán một số trên web. Trong Variables, xem `secret` để biết đáp án 😉

## Bước 5: Docker (15 phút)

`Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebLab.dll"]
```

`.dockerignore`:

```
bin/
obj/
```

```bash
docker build -t day03-weblab .
docker run --rm -p 8080:8080 day03-weblab
```

Mở http://localhost:8080. Từ .NET 8, ASP.NET Core trong container mặc định nghe cổng **8080** (không còn là 80).

Multi-stage build gồm 2 stage:

- stage `sdk` (~800MB) dùng để build
- stage `aspnet` (~230MB) chỉ để chạy, nên image cuối nhỏ hơn

---

## Thử thách thêm (chọn 1-2)

1. ⭐ Thêm gợi ý "Gần lắm rồi!" khi lệch ≤ 5. Gợi ý: thêm một nhánh `when` vào switch.
2. ⭐ Endpoint `GET /api/guess/history` trả về danh sách các số đã đoán trong ván hiện tại.
3. ⭐⭐ Giới hạn 7 lần đoán. Lần thứ 7 sai thì trả `"lose"` kèm đáp án, rồi bắt đầu ván mới.
4. ⭐⭐ Đổi FizzBuzz sang dùng `switch` với tuple: `(i % 3, i % 5) switch { (0, 0) => "FizzBuzz", ... }`.

## Câu hỏi tự kiểm tra

- Gọi `/api/fizzbuzz?n=abc` thì trả về mã gì? Ai xử lý chỗ đó, code của bạn hay framework?
- Mở 2 tab cùng chơi thì chuyện gì xảy ra? Vì sao? (Gợi ý: `secret` là biến dùng chung của cả app.)
- Nếu bỏ nhánh `> 0 =>` trong switch thì compiler báo gì?
