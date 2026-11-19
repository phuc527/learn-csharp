# Daily Labs: RewardHub (fullstack, chạy bằng Docker)

> Đi kèm [ROADMAP.md](ROADMAP.md). **Day 1 = thứ 5, 19/11/2026.** Mỗi tuần học 6 ngày, nghỉ 1 ngày: 5 lab thường ~60–90 phút, ngày thứ 6 là **Boss lab** 🏆 ~2–3h gộp kiến thức cả tuần.
> Độ khó: ⭐ dễ → ⭐⭐⭐⭐⭐ khó. Khó dần trong từng tuần và qua các tuần. Trễ thì dời ngày, không bỏ lab.

## Stack

| Tầng             | Công nghệ                                                                                                              |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Backend          | .NET 10 (LTS), ASP.NET Core Minimal API, EF Core 10, Dapper                                                            |
| Frontend         | React 19 + Vite + TypeScript + TanStack Query (stack bạn đang dùng)                                                    |
| Hạ tầng (Docker) | Postgres, Redis, RabbitMQ, WireMock, nginx                                                                             |
| Monitoring       | OpenTelemetry SDK → OTel Collector → **Prometheus** (metrics), Tempo (traces), Loki (logs) → **Grafana**, Alertmanager |
| Test             | xUnit v3, Shouldly, NSubstitute, Testcontainers, Vitest, Playwright, Postman CLI                                       |

## Repo đích (tự dựng dần, không scaffold sẵn)

```
rewardhub/
├─ compose.yml            # db, redis, rabbitmq, api, worker, web
├─ compose.monitoring.yml # otel-collector, prometheus, grafana, tempo, loki, alertmanager
├─ ops/
│  ├─ otel-collector.yaml
│  ├─ prometheus/         # prometheus.yml, alerts.yml, recording rules
│  ├─ alertmanager.yml
│  └─ grafana/
│     ├─ provisioning/    # datasources + dashboards khai báo bằng file
│     └─ dashboards/      # JSON dashboard commit vào git (dashboard as code)
├─ compose.prod.yml       # override cho production (tuần 19)
├─ .env.example           # KHÔNG commit .env
├─ Directory.Build.props / Directory.Packages.props
├─ src/
│  ├─ RewardHub.Api/      # Minimal API + SignalR
│  ├─ RewardHub.Worker/   # TCP ingest, outbox dispatcher, consumers
│  └─ FloorSimulator/     # giả lập máy slot gửi message TCP
├─ tests/
│  ├─ RewardHub.UnitTests/
│  └─ RewardHub.IntegrationTests/
└─ web/                   # React + Vite
```

## Quy ước chạy bằng Docker

- **Monitoring:** `docker compose -f compose.yml -f compose.monitoring.yml up -d` → Grafana `localhost:3000`, Prometheus `localhost:9090`. Trước tuần 18 nếu máy yếu có thể dùng tạm 1 container all-in-one `grafana/otel-lgtm`.
- **Dev hằng ngày:** `docker compose up -d db redis rabbitmq` rồi chạy `dotnet watch` + `npm run dev` ở máy (debug nhanh nhất).
- **Full stack:** `docker compose up --build` (hoặc `docker compose watch` để hot reload trong container).
- **Luật Boss lab:** clone sạch → `cp .env.example .env` → `docker compose up --build` → mọi thứ chạy được. Không qua được thì chưa xong.
- Mỗi ngày: 15' đọc docs → 45–60' code → 10' ghi TIL vào `notes/`. Commit cuối buổi.

---

## Đối chiếu nhanh TS / Go → C#

| Bạn đã biết (TS / Go)                    | Trong C#                                               |
| ---------------------------------------- | ------------------------------------------------------ |
| `npm` / `go mod`                         | `dotnet` CLI + NuGet (`dotnet add package`)            |
| `package.json` / `go.mod`                | file `.csproj`                                         |
| `interface`, `type` (TS) / `struct` (Go) | `interface`, `class`, `record`, `struct`               |
| `string \| null` / con trỏ `nil`         | `string?` (nullable reference types)                   |
| `Promise<T>`, `async/await`              | `Task<T>`, `async/await` (gần như y hệt)               |
| `Promise.all`                            | `Task.WhenAll`                                         |
| `arr.filter().map()`                     | LINQ: `list.Where().Select()`                          |
| `(x) => x * 2`                           | `x => x * 2` (lambda, kiểu `Func<int,int>`)            |
| `error` trả về (Go)                      | exception (`try/catch`), dùng cho lỗi bất thường       |
| goroutine + channel (Go)                 | `Task` + `Channel<T>`                                  |
| Express / Gin                            | ASP.NET Core Minimal API                               |
| Prisma / GORM                            | Entity Framework Core                                  |
| Jest / `go test`                         | xUnit (`dotnet test`)                                  |
| `number`                                 | `int`, `long`, `double`, **`decimal` (dùng cho tiền)** |

## Tuần 1 (19/11 – 24/11/2026): Cú pháp C# cơ bản (console)

| Ngày     | Độ khó | Lab                                                                                                                                                                  | Xong khi                                |
| -------- | ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------- |
| Day 1    | ⭐     | Cài VS Code + C# Dev Kit. `dotnet new console -n Hello`, `dotnet run`; đọc hiểu `.csproj` và `Program.cs` (top-level statements); đặt breakpoint, debug F5           | In "Hello" và dừng được ở breakpoint    |
| Day 2    | ⭐     | Biến & kiểu: `int`, `long`, `decimal`, `double`, `bool`, `string`, `var`; string interpolation `$"..."`; `Console.ReadLine` + `int.TryParse`. Lab: máy tính tiền tip | Nhập chữ thay vì số không bị crash      |
| Day 3    | ⭐     | `if`, `switch`, `for`, `foreach`, `while`. Lab: FizzBuzz + trò đoán số                                                                                               | Chơi được đến khi đoán đúng             |
| Day 4    | ⭐     | Method: tham số, giá trị trả về, tham số mặc định, overload. Lab: `GetTier(int points)` trả về Bronze/Silver/Gold                                                    | Gọi được với nhiều input                |
| Day 5    | ⭐⭐   | Mảng, `List<T>`, `Dictionary<TKey,TValue>`. Lab: menu console thêm / xoá / tìm người chơi                                                                            | Không crash khi xoá người không tồn tại |
| Day 6 🏆 | ⭐⭐   | **Slot machine mini:** quay 3 ô ngẫu nhiên (`Random.Shared`), tính thắng/thua, số dư, menu chơi tiếp/thoát                                                           | Chơi được, số dư không âm               |

## Tuần 2 (26/11 – 01/12/2026): Lập trình hướng đối tượng

| Ngày      | Độ khó | Lab                                                                                                                                    | Xong khi                                      |
| --------- | ------ | -------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| Day 7     | ⭐     | `class`, constructor, property (`get; set;`, `init`), `public`/`private`. Lab: class `Player`                                          | Không gán được giá trị sai từ ngoài           |
| Day 8     | ⭐⭐   | Value type vs reference type (`struct` vs `class`), `null`, `?`, `??`, `?.`. Lab: viết code chứng minh copy giá trị vs copy tham chiếu | Giải thích được kết quả in ra                 |
| Day 9     | ⭐⭐   | Kế thừa, `abstract`, `override`, `interface`. Lab: `IGame` với `SlotGame`, `DiceGame`                                                  | Thêm game mới không sửa code cũ               |
| Day 10    | ⭐⭐   | `enum`, `record`, so sánh bằng (`==`) của class vs record. Lab: enum `Tier`, `record RewardDto`                                        | Hiểu vì sao 2 record giống nhau thì bằng nhau |
| Day 11    | ⭐⭐   | Exception: `try/catch/finally`, exception tự định nghĩa, khi nào **không** nên throw. Lab: `InsufficientBalanceException`              | Đặt cược quá số dư báo lỗi rõ ràng            |
| Day 12 🏆 | ⭐⭐⭐ | Refactor slot machine sang OOP: `Player`, `Wallet`, `IGame`, nhiều file + `namespace`; lưu/đọc trạng thái bằng `System.Text.Json`      | Tắt app mở lại vẫn còn số dư                  |

## Tuần 3 (03/12 – 08/12/2026): Generics, lambda, LINQ, test đầu tiên

| Ngày      | Độ khó | Lab                                                                                                  | Xong khi                                     |
| --------- | ------ | ---------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| Day 13    | ⭐⭐   | Generics: tự viết `Result<T>` (thành công/thất bại), generic method, `where T :`                     | Dùng `Result<Player>` và `Result<int>`       |
| Day 14    | ⭐⭐   | Delegate, lambda, `Func`/`Action` (giống arrow function TS)                                          | Truyền hàm làm tham số được                  |
| Day 15    | ⭐⭐   | LINQ: `Where`, `Select`, `OrderBy`, `GroupBy`, `First`, `Any`, `Sum` trên `List<Player>`             | Viết lại 5 câu `filter/map` của JS bằng LINQ |
| Day 16    | ⭐⭐   | Test đầu tiên: `dotnet new xunit`, project reference, `Assert`; test `GetTier`                       | `dotnet test` xanh                           |
| Day 17    | ⭐⭐   | Đọc/ghi file, `using`, `IDisposable`. Lab: đọc file CSV log session                                  | Đọc 1000 dòng không lỗi                      |
| Day 18 🏆 | ⭐⭐⭐ | **Player Report CLI:** đọc CSV 1000 session → LINQ ra top player, tổng theo máy → xuất JSON; 10 test | 10 test xanh, file JSON đúng                 |

## Tuần 4 (10/12 – 15/12/2026): Async, HTTP, NuGet, Docker cơ bản

| Ngày      | Độ khó | Lab                                                                                    | Xong khi                                |
| --------- | ------ | -------------------------------------------------------------------------------------- | --------------------------------------- |
| Day 19    | ⭐⭐   | `async/await` cơ bản (so với Promise), `Task.Delay`, `Task.WhenAll`                    | Chạy 5 task song song nhanh hơn tuần tự |
| Day 20    | ⭐⭐   | `HttpClient` gọi API công khai (JSONPlaceholder), deserialize JSON vào `record`        | In danh sách từ API                     |
| Day 21    | ⭐⭐   | NuGet: `dotnet add package Bogus` sinh dữ liệu giả, `Spectre.Console` vẽ bảng đẹp      | Thấy package trong `.csproj`            |
| Day 22    | ⭐⭐   | Dockerfile đầu tiên cho console app; `docker build`, `docker run`                      | App chạy trong container                |
| Day 23    | ⭐⭐   | `compose.yml` có Postgres; dùng `Npgsql` thuần insert/select                           | Dữ liệu còn sau khi restart (volume)    |
| Day 24 🏆 | ⭐⭐⭐ | Console app: gọi API → lưu Postgres → in report; toàn bộ chạy bằng `docker compose up` | Clone sạch, 1 lệnh là chạy              |

> Từ tuần 5 trở đi bắt đầu dự án **RewardHub**. Nếu tuần 1–4 thấy chưa chắc, cứ lặp lại Boss lab với đề khác rồi mới đi tiếp.

## Tuần 5 (17/12 – 22/12/2026): C# hiện đại + setup dự án chuẩn

| Ngày      | Độ khó | Lab                                                                                                                                      | Xong khi                                                     |
| --------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Day 25    | ⭐     | Tạo solution, `Directory.Build.props` (nullable, warnings-as-errors), `compose.yml` chỉ có Postgres; kết nối bằng `psql` trong container | `dotnet build` 0 warning, `docker compose exec db psql` chạy |
| Day 26    | ⭐     | Model `Player`, `Machine`, `Reward` bằng `record`; hàm tính tier (Bronze/Silver/Gold/Platinum) bằng `switch` expression                  | 5 test xUnit pass                                            |
| Day 27    | ⭐⭐   | LINQ: sinh 1000 session giả (Bogus) → top 10 player theo turnover, group theo máy, trung bình/giờ                                        | Kết quả đúng với dữ liệu test cố định (seed)                 |
| Day 28    | ⭐⭐   | **Kata 1:** `CardInMessage.TryParse(ReadOnlySpan<char>)` cho message fixed-width kiểu `!9590...`                                         | 10 test: hợp lệ, thiếu ký tự, sai type code                  |
| Day 29    | ⭐⭐   | BenchmarkDotNet: `Substring` vs `Span` cho parser; đọc hiểu cột Allocated                                                                | Có bảng benchmark trong TIL                                  |
| Day 30 🏆 | ⭐⭐⭐ | Console app đọc file message → parse → in ra; **Dockerfile multi-stage**; `docker compose run parser`                                    | Image < 100MB, chạy được bằng compose                        |

## Tuần 6 (24/12 – 29/12/2026): Async, concurrency, TCP

| Ngày      | Độ khó   | Lab                                                                                                                                                                              | Xong khi                                           |
| --------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| Day 31    | ⭐⭐     | Gọi 10 HTTP request tuần tự vs `Task.WhenAll`, đo thời gian                                                                                                                      | Giải thích được chênh lệch                         |
| Day 32    | ⭐⭐     | `CancellationToken`: Ctrl+C dừng êm, `CancelAfter` làm timeout                                                                                                                   | Không còn exception lạ khi huỷ                     |
| Day 33    | ⭐⭐⭐   | **Tái hiện thread-pool starvation:** endpoint dùng `.Result`, bắn tải bằng `bombardier`/k6, xem `dotnet-counters` (ThreadPool Queue Length) → sửa bằng `await`                   | Ảnh chụp counters trước/sau trong TIL              |
| Day 34    | ⭐⭐⭐   | `TcpListener` nhận message → `Channel<T>` bounded → consumer parse (backpressure)                                                                                                | Gửi 10k message không mất cái nào                  |
| Day 35    | ⭐⭐⭐   | Graceful shutdown: SIGTERM xử lý nốt message trong channel rồi mới thoát; `IAsyncEnumerable` đọc stream                                                                          | `docker stop` không mất message                    |
| Day 36 🏆 | ⭐⭐⭐⭐ | **FloorSimulator:** container A giả lập 50 máy slot gửi message TCP mỗi 100ms → container B (TCP server + Channel + parser) log ra. _Simulator này là nguồn dữ liệu cho cả khoá_ | `docker compose up` thấy log message chạy liên tục |

## Tuần 7 (31/12 – 05/01/2027): ASP.NET Core nền tảng + khởi tạo frontend

| Ngày      | Độ khó   | Lab                                                                                                                                                                                                                                                                                                                         | Xong khi                                                        |
| --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| Day 37    | ⭐       | Minimal API `GET /health`, thêm service `api` vào compose                                                                                                                                                                                                                                                                   | `curl localhost:8080/health` từ máy host                        |
| Day 38    | ⭐⭐     | Middleware correlation-id (đọc/sinh `X-Correlation-Id`, đẩy vào log scope); thử đổi thứ tự middleware                                                                                                                                                                                                                       | Header trả về đúng, log có correlation id                       |
| Day 39    | ⭐⭐     | DI lifetimes: service trả GUID với 3 lifetime, quan sát; cố ý gây **captive dependency**, bật `ValidateScopes` để bắt                                                                                                                                                                                                       | Giải thích được lỗi bằng lời của mình                           |
| Day 40    | ⭐⭐     | Options pattern: `RewardOptions` + `ValidateDataAnnotations().ValidateOnStart()`; override qua env trong compose (`Reward__PointsPerDollar`)                                                                                                                                                                                | Sai config thì app không start                                  |
| Day 41    | ⭐⭐     | `npm create vite@latest web -- --template react-ts`, TanStack Query gọi `/health`; cấu hình CORS **đúng origin** (không `AllowAnyOrigin`)                                                                                                                                                                                   | Trang web hiện trạng thái API                                   |
| Day 42 🏆 | ⭐⭐⭐⭐ | Compose có `db + api + web`, bật `docker compose watch`. **Thêm Prometheus + Grafana lần đầu:** API mở `/metrics` (OpenTelemetry + gói `OpenTelemetry.Exporter.Prometheus.AspNetCore`, cài với `--prerelease`), Prometheus scrape, import dashboard ASP.NET Core chính thức của Microsoft (Grafana ID **19924**, **19925**) | Sửa code tự reload; Grafana thấy request rate + latency của API |

## Tuần 8 (07/01 – 12/01/2027): REST API + màn hình CRUD

| Ngày      | Độ khó | Lab                                                                                                                                         | Xong khi                                    |
| --------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| Day 43    | ⭐⭐   | CRUD `Players` (in-memory), `MapGroup`, `TypedResults`                                                                                      | Đủ 5 endpoint, status code đúng             |
| Day 44    | ⭐⭐   | Validation + `ProblemDetails` + `IExceptionHandler` toàn cục (không try/catch rải rác)                                                      | Lỗi trả về chuẩn RFC 9457 kèm `traceId`     |
| Day 45    | ⭐⭐   | OpenAPI built-in + Scalar UI; sinh type TS bằng `openapi-typescript` vào `web/`                                                             | Frontend dùng type sinh ra, không tự gõ tay |
| Day 46    | ⭐⭐⭐ | Trang Players: danh sách + form tạo (mutation), hiện lỗi validation theo từng field từ ProblemDetails                                       | Nhập sai thấy lỗi đúng field                |
| Day 47    | ⭐⭐⭐ | Paging (offset vs cursor), filter, sort ở API; bảng có phân trang ở web                                                                     | Giải thích khi nào dùng cursor              |
| Day 48 🏆 | ⭐⭐⭐ | **Kata 3:** tách `Program.cs` thành các extension `AddApiCors()`, `AddApiDocs()`, `AddProblemHandling()`. Demo **Capstone v1** bằng compose | Program.cs < 40 dòng, compose chạy sạch     |

## Tuần 9 (14/01 – 19/01/2027): SQL + EF Core cơ bản

| Ngày      | Độ khó   | Lab                                                                                                                                       | Xong khi                                     |
| --------- | -------- | ----------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| Day 49    | ⭐⭐     | SQL tay: tạo bảng 1M dòng bằng `generate_series`, `EXPLAIN ANALYZE` trước/sau khi thêm index                                              | Query từ giây → mili giây                    |
| Day 50    | ⭐⭐     | EF Core: `DbContext`, Fluent config, migration đầu tiên; apply bằng **migration bundle** chạy như 1 service one-off trong compose         | `docker compose run migrate` tạo bảng        |
| Day 51    | ⭐⭐     | Thay in-memory bằng EF; `AsNoTracking`, projection `Select` ra DTO                                                                        | Không endpoint nào trả entity trực tiếp      |
| Day 52    | ⭐⭐⭐   | Bật log SQL, tìm **N+1** khi list player kèm session → sửa bằng projection / `AsSplitQuery`                                               | Số query giảm từ N+1 về 1–2                  |
| Day 53    | ⭐⭐⭐   | Worker nhận message từ FloorSimulator → ghi `Sessions` vào DB theo batch                                                                  | 100k session trong DB                        |
| Day 54 🏆 | ⭐⭐⭐⭐ | Trang **Dashboard** React: top player, turnover theo máy (Recharts); tối ưu query để k6 p95 < 100ms; vừa chạy k6 vừa xem p95 trên Grafana | Ảnh Grafana trước/sau + báo cáo k6 trong TIL |

## Tuần 10 (21/01 – 26/01/2027): Đường tiền: Wallet (phải đúng tuyệt đối)

| Ngày      | Độ khó   | Lab                                                                                                                                                 | Xong khi                          |
| --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| Day 55    | ⭐⭐⭐   | Tái hiện **lost update**: 2 request credit song song làm sai số dư                                                                                  | Viết được test làm lỗi xuất hiện  |
| Day 56    | ⭐⭐⭐   | Optimistic concurrency bằng `xmin` (Postgres); bắt `DbUpdateConcurrencyException` → 409                                                             | Test T2 không còn sai             |
| Day 57    | ⭐⭐⭐   | Ledger append-only (`WalletTransactions`), số dư suy ra từ ledger; `ExecuteUpdate`                                                                  | Không có `UPDATE` sửa lịch sử     |
| Day 58    | ⭐⭐⭐⭐ | **Idempotency-Key:** bảng có unique constraint, gọi lại trả response cũ, không cộng tiền 2 lần                                                      | Gửi 1 key 10 lần → cộng 1 lần     |
| Day 59    | ⭐⭐⭐   | Report giao dịch bằng Dapper, so sánh code + hiệu năng với EF                                                                                       | Có bảng so sánh trong TIL         |
| Day 60 🏆 | ⭐⭐⭐⭐ | Trang Wallet: nút credit/debit, mỗi lần bấm sinh `crypto.randomUUID()` làm key; double-click không cộng 2 lần; k6 50 VU song song → số dư cuối đúng | Số dư = tổng ledger sau load test |

## Tuần 11 (28/01 – 02/02/2027): Dọn kiến trúc data + caching

| Ngày      | Độ khó   | Lab                                                                                               | Xong khi                                |
| --------- | -------- | ------------------------------------------------------------------------------------------------- | --------------------------------------- |
| Day 61    | ⭐⭐     | **Kata 4:** viết `IRepository<T>` generic như OUTABOX, rồi xoá, dùng `DbContext` trực tiếp        | Ghi số dòng trước/sau                   |
| Day 62    | ⭐⭐⭐   | **Kata 2:** tái hiện `(Task, Task)` 2 query song song trên 1 DbContext → thấy lỗi → sửa           | Hiểu vì sao DbContext không thread-safe |
| Day 63    | ⭐⭐     | `IMemoryCache` cho danh sách máy + invalidation khi sửa                                           | Sửa máy thì cache cập nhật              |
| Day 64    | ⭐⭐⭐   | Redis vào compose + **`HybridCache`**; test cache stampede (100 request cùng lúc khi cache trống) | DB chỉ nhận 1 query                     |
| Day 65    | ⭐⭐⭐   | Output caching cho dashboard, invalidate theo tag khi có session mới                              | Response header thể hiện cache hit      |
| Day 66 🏆 | ⭐⭐⭐⭐ | Đo dashboard trước/sau cache bằng k6; viết TIL: cache cái gì, TTL bao lâu, rủi ro dữ liệu cũ      | Có số liệu p50/p95                      |

## Tuần 12 (04/02 – 09/02/2027): Bảo mật 1: Auth

| Ngày      | Độ khó   | Lab                                                                                                                 | Xong khi                                     |
| --------- | -------- | ------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| Day 67    | ⭐⭐     | Secrets: `dotnet user-secrets`, `.env` + `secrets:` của compose, `.gitignore`; chạy `gitleaks`                      | gitleaks sạch, repo không có secret          |
| Day 68    | ⭐⭐⭐   | Phát hành/xác thực JWT, thời hạn 5–15 phút, `[Authorize]` / `RequireAuthorization()`                                | Token hết hạn → 401 đúng                     |
| Day 69    | ⭐⭐⭐   | Refresh token: lưu dạng **hash** trong DB, rotate mỗi lần dùng, phát hiện dùng lại                                  | Dùng lại refresh token cũ → thu hồi cả chuỗi |
| Day 70    | ⭐⭐⭐   | Login ở React: access token giữ trong memory, refresh qua cookie `httpOnly`; tự refresh khi gặp 401                 | F5 trang vẫn đăng nhập                       |
| Day 71    | ⭐⭐⭐   | Role Admin / Host / Player, policy `CanCreditWallet`; route guard ở React                                           | Player gọi credit → 403                      |
| Day 72 🏆 | ⭐⭐⭐⭐ | Thay auth tự viết bằng **Keycloak** trong compose; React login OIDC (`oidc-client-ts`), API validate token Keycloak | Login qua Keycloak, API nhận role đúng       |

## Tuần 13 (11/02 – 16/02/2027): Bảo mật 2: tấn công & phòng thủ

| Ngày      | Độ khó   | Lab                                                                                                                                              | Xong khi                 |
| --------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------ |
| Day 73    | ⭐⭐     | Security headers, HTTPS dev cert trong container, request size limit hợp lý                                                                      | Không còn `int.MaxValue` |
| Day 74    | ⭐⭐⭐   | Rate limiting (token bucket theo user) cho endpoint credit; UI hiện thông báo 429                                                                | Spam bị chặn, UI báo rõ  |
| Day 75    | ⭐⭐⭐   | **BOLA:** player A xem được wallet của B → sửa bằng resource-based authorization                                                                 | Test A đọc B → 403/404   |
| Day 76    | ⭐⭐⭐   | **SSRF:** endpoint "tải ảnh từ URL" → tấn công vào `http://db:5432`, `http://redis:6379` trong mạng Docker → sửa bằng allowlist + chặn IP nội bộ | Payload tấn công bị chặn |
| Day 77    | ⭐⭐⭐   | **Kata 5:** container "partner API" dùng cert tự ký; client bypass TLS `=> true` → sửa bằng pin đúng cert trên named HttpClient                  | Cert lạ bị từ chối       |
| Day 78 🏆 | ⭐⭐⭐⭐ | Tự pentest RewardHub theo OWASP API Top 10, viết report, sửa ≥3 lỗi                                                                              | Report + PR sửa          |

## Tuần 14 (18/02 – 23/02/2027): Tích hợp hệ thống ngoài

| Ngày      | Độ khó   | Lab                                                                                                                     | Xong khi                             |
| --------- | -------- | ----------------------------------------------------------------------------------------------------------------------- | ------------------------------------ |
| Day 79    | ⭐⭐     | Tái hiện **cạn socket**: vòng lặp `new HttpClient()`, đếm `TIME_WAIT` bằng `netstat` → sửa bằng `IHttpClientFactory`    | Số socket ổn định                    |
| Day 80    | ⭐⭐⭐   | **Kata 6:** typed client gọi "GCM partner" (WireMock.Net container) + `DelegatingHandler` log request/response          | Không còn `new HttpClient`           |
| Day 81    | ⭐⭐⭐   | `AddStandardResilienceHandler`; cho WireMock trả 500/chậm, quan sát retry, timeout, circuit breaker                     | Log thấy breaker mở/đóng             |
| Day 82    | ⭐⭐⭐   | Webhook receiver: verify chữ ký HMAC, idempotency, trả 202 ngay                                                         | Sai chữ ký → 401, gửi trùng → bỏ qua |
| Day 83    | ⭐⭐⭐   | `BackgroundService` xử lý hàng đợi webhook; `HostOptions.ShutdownTimeout`; `docker compose stop` không mất việc         | Restart không mất webhook            |
| Day 84 🏆 | ⭐⭐⭐⭐ | E2E: Simulator → TCP worker → DB → rule "mỗi 1000 điểm thưởng 10 coin" → gọi partner API (resilience) → cập nhật wallet | Chạy liên tục 10 phút không lỗi      |

## Tuần 15 (25/02 – 02/03/2027): Messaging + real-time

| Ngày      | Độ khó     | Lab                                                                                                                                 | Xong khi                               |
| --------- | ---------- | ----------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| Day 85    | ⭐⭐       | RabbitMQ vào compose (management UI), publish/consume bằng `RabbitMQ.Client`                                                        | Thấy message trong UI                  |
| Day 86    | ⭐⭐⭐     | **Outbox pattern:** ghi event cùng transaction với wallet; dispatcher publish lên RabbitMQ                                          | Tắt Rabbit rồi bật lại không mất event |
| Day 87    | ⭐⭐⭐     | Consumer idempotent + retry + dead-letter queue                                                                                     | Message lỗi nằm ở DLQ                  |
| Day 88    | ⭐⭐⭐     | SignalR hub có auth; React dùng `@microsoft/signalr` hiện jackpot ticker + số dư realtime                                           | Credit ở tab A, tab B cập nhật ngay    |
| Day 89    | ⭐⭐⭐⭐   | `docker compose up --scale api=2` + nginx/YARP load balancer + **Redis backplane** cho SignalR                                      | Client ở 2 replica đều nhận event      |
| Day 90 🏆 | ⭐⭐⭐⭐⭐ | **Live Floor Dashboard:** lưới 50 máy cập nhật realtime (Simulator → Rabbit → SignalR → React); kill 1 replica api, UI tự reconnect | Demo kill container không mất UI       |

## Tuần 16 (04/03 – 09/03/2027): Testing backend

| Ngày      | Độ khó   | Lab                                                                           | Xong khi                                 |
| --------- | -------- | ----------------------------------------------------------------------------- | ---------------------------------------- |
| Day 91    | ⭐⭐     | xUnit v3 + Shouldly cho domain (tier, reward rule)                            | Coverage domain > 90%                    |
| Day 92    | ⭐⭐     | NSubstitute mock partner client; `FakeTimeProvider` test logic theo thời gian | Không test nào dùng `DateTime.Now`       |
| Day 93    | ⭐⭐⭐   | Integration test bằng `WebApplicationFactory` cho Players                     | Test gọi HTTP thật vào app in-memory     |
| Day 94    | ⭐⭐⭐   | **Testcontainers** Postgres + Respawn reset DB giữa các test                  | Không dùng EF InMemory provider          |
| Day 95    | ⭐⭐⭐   | Test auth: test auth handler, kiểm tra policy trả 403 đúng                    | Mỗi policy có ≥1 test                    |
| Day 96 🏆 | ⭐⭐⭐⭐ | Integration test 50 credit song song trên Postgres thật, số dư đúng           | Test xanh 10 lần liên tiếp (không flaky) |

## Tuần 17 (11/03 – 16/03/2027): Testing fullstack + CI

| Ngày       | Độ khó   | Lab                                                                                                                                    | Xong khi                              |
| ---------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| Day 97     | ⭐⭐     | Vitest + React Testing Library + MSW cho component Wallet                                                                              | Test cả loading / error / empty state |
| Day 98     | ⭐⭐⭐   | Playwright E2E: login → credit → thấy số dư, chạy với stack compose                                                                    | E2E xanh trên máy                     |
| Day 99     | ⭐⭐⭐   | Postman collection + **Postman CLI** chạy với stack compose                                                                            | `postman collection run` pass         |
| Day 100    | ⭐⭐⭐   | **Kata 7:** characterization test cho parser với 20 message ẩn danh → tách worker lớn thành `MessageParser` + từng `IMessageHandler`   | Test cũ vẫn xanh sau refactor         |
| Day 101    | ⭐⭐⭐   | Testcontainers RabbitMQ: test luồng outbox → consumer                                                                                  | Test E2E messaging xanh               |
| Day 102 🏆 | ⭐⭐⭐⭐ | **GitHub Actions:** build + `dotnet format` check + unit + integration (Testcontainers) + Vitest + Playwright trên `docker compose up` | PR có badge xanh                      |

## Tuần 18 (18/03 – 23/03/2027): Monitoring kiểu production: Prometheus + Grafana + Tempo + Loki

Pattern lấy từ `meysamhadeli/booking-microservices` và `Depechie/OpenTelemetryGrafana`: app chỉ nói chuyện với **OTel Collector** qua OTLP; Collector phân phối metrics → Prometheus, traces → Tempo, logs → Loki; Grafana đọc cả 3. Đổi backend không phải sửa code C#.

| Ngày       | Độ khó     | Lab                                                                                                                                                                                                                                                                                                                                       | Xong khi                                                   |
| ---------- | ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| Day 103    | ⭐⭐       | Health check liveness (`/health/live`) / readiness (`/health/ready`, kiểm tra DB + Redis + RabbitMQ); compose `healthcheck` + `depends_on: condition: service_healthy`                                                                                                                                                                    | api chỉ start khi db sẵn sàng; ready trả 503 khi tắt DB    |
| Day 104    | ⭐⭐⭐     | Chuyển từ scrape `/metrics` sang **OTel Collector**: API + worker gửi OTLP → Collector → Prometheus. Thêm `postgres_exporter`, `redis_exporter`, plugin Prometheus của RabbitMQ (cổng 15692)                                                                                                                                              | Trang Targets của Prometheus: tất cả **UP**                |
| Day 105    | ⭐⭐⭐     | Logs: Serilog (message template) → OTLP → Collector → **Loki**; mỗi dòng log có `trace_id`; UI React hiện `traceId` lấy từ ProblemDetails khi lỗi                                                                                                                                                                                         | Copy traceId từ UI → tìm thấy log trong Grafana Explore    |
| Day 106    | ⭐⭐⭐     | Traces → **Tempo**: API → Postgres → RabbitMQ → worker; bật **exemplars** để từ điểm p95 trên biểu đồ Prometheus bấm sang trace                                                                                                                                                                                                           | 1 trace đi qua 3 service; bấm exemplar mở được trace       |
| Day 107    | ⭐⭐⭐⭐   | Business metrics bằng `Meter`: `rewardhub_credits_total` (counter), `rewardhub_credit_duration_seconds` (histogram), độ dài queue (gauge). **Không** dùng label có nhiều giá trị (playerId, URL thô). Dashboard RewardHub kiểu **RED** (Rate, Errors, Duration) + USE cho DB/Redis, lưu JSON vào `ops/grafana/dashboards` và provisioning | Clone sạch + compose up → dashboard tự có, không click tay |
| Day 108 🏆 | ⭐⭐⭐⭐⭐ | **Alerting + Incident drill:** viết rule Prometheus (5xx > 5% trong 5 phút, p95 credit > 500ms, target down, queue tăng liên tục) → Alertmanager → webhook (Discord/Slack hoặc WireMock). Tự gây lỗi (query chậm / partner sập), alert phải bắn, tìm root cause theo đường **metric → exemplar → trace → log**, viết postmortem           | Alert bắn và tự resolve; postmortem trong `notes/`         |

## Tuần 19 (25/03 – 30/03/2027): Docker production + hiệu năng

| Ngày       | Độ khó   | Lab                                                                                                                                                                                                                        | Xong khi                                                                              |
| ---------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| Day 109    | ⭐⭐     | Dockerfile production: multi-stage, chiseled, non-root; so sánh với `dotnet publish /t:PublishContainer`                                                                                                                   | Image api < 150MB, chạy non-root                                                      |
| Day 110    | ⭐⭐⭐   | Build React → container **nginx**: reverse proxy `/api` → api (hết cần CORS), SPA fallback, gzip, cache header                                                                                                             | 1 domain phục vụ cả web + api                                                         |
| Day 111    | ⭐⭐⭐   | `compose.prod.yml`: giới hạn tài nguyên, restart policy, secrets, migration bundle chạy trước api. Monitoring: Prometheus/Loki/Tempo **không mở cổng ra ngoài**, mật khẩu admin Grafana qua secret, đặt retention + volume | `docker compose -f compose.yml -f compose.monitoring.yml -f compose.prod.yml up` chạy |
| Day 112    | ⭐⭐⭐   | k6 load test (đẩy kết quả k6 vào Prometheus, xem cùng dashboard) + `dotnet-trace` flame graph → sửa 1 điểm nóng                                                                                                            | p95 cải thiện, có ảnh Grafana trước/sau                                               |
| Day 113    | ⭐⭐⭐⭐ | **SLO:** 99.5% request credit thành công và < 300ms trong 30 ngày; recording rules + cảnh báo **burn rate** nhiều cửa sổ (1h/5m, 6h/30m); dashboard error budget                                                           | Gây lỗi 2% → cảnh báo burn rate nhanh bắn                                             |
| Day 114 🏆 | ⭐⭐⭐⭐ | Deploy lên VPS rẻ (hoặc VM local) bằng compose + Caddy HTTPS tự động, **kèm stack monitoring**; GitHub Actions push image lên GHCR rồi deploy                                                                              | URL HTTPS chạy; Grafana (sau đăng nhập) thấy traffic production                       |

## Tuần 20 (01/04 – 06/04/2027): Kiến trúc + tổng kết

| Ngày       | Độ khó     | Lab                                                                                                                                                                                               | Xong khi                              |
| ---------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| Day 115    | ⭐⭐⭐     | Tổ chức lại theo **Vertical Slice** (`Features/Wallet/Credit.cs` …)                                                                                                                               | Mỗi feature nằm gọn 1 folder          |
| Day 116    | ⭐⭐⭐     | Aggregate `Wallet` + value object `Money` + domain event                                                                                                                                          | Logic tiền không còn ở endpoint       |
| Day 117    | ⭐⭐⭐     | **Kata 8:** console kiểu Jackpot (bool flags + logger tĩnh) → `Host.CreateApplicationBuilder` + mỗi module 1 `BackgroundService` bật qua Options                                                  | Bật/tắt module bằng config            |
| Day 118    | ⭐⭐⭐     | **Kata 9:** project mẫu net7 (đặt `TargetFramework` tay) → nâng lên net10, ghi lại breaking changes                                                                                               | Checklist nâng cấp dùng được cho team |
| Day 119    | ⭐⭐⭐     | **Kata 10:** đóng gói thư viện dùng chung thành NuGet (local feed / GitHub Packages), RewardHub dùng lại                                                                                          | `dotnet add package` từ feed riêng    |
| Day 120 🏆 | ⭐⭐⭐⭐⭐ | **Final demo:** clone sạch → `docker compose up --build` → toàn hệ thống chạy; README có sơ đồ kiến trúc; quay demo 5 phút. Sau đó chọn 1 lỗi từ bảng audit trong ROADMAP để làm PR thật cho team | Demo + PR thật                        |

---

## Khi bị kẹt

1. Đọc lỗi + docs Microsoft Learn trước (15').
2. Viết lại vấn đề trong 3 câu vào TIL (thường tự ra đáp án).
3. Hỏi AI **sau khi** đã thử, và yêu cầu giải thích chứ không xin code hoàn chỉnh.
4. Trễ lịch thì dời lab, không bỏ Boss lab: Boss lab là bằng chứng bạn đã hiểu.
