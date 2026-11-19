# Roadmap C# Backend: học qua dự án thực tế

> Dựa trên [roadmap.sh/aspnet-core](https://roadmap.sh/aspnet-core) + [roadmap.sh/backend](https://roadmap.sh/backend), nhưng sắp xếp lại theo **lỗ hổng thật** tìm thấy trong code ở `Desktop/Work/Projects`.
> Xuất phát điểm: **chưa biết C#**, đã biết TypeScript/React và đang học Go → học C# qua so sánh với thứ đã biết.
> Cách học: **mỗi ngày 1 lab, dễ → khó** (xem [LABS.md](LABS.md)). Từ tuần 5 làm dự án fullstack **RewardHub** (ASP.NET Core + React, chạy bằng Docker Compose), xen kẽ các refactor kata lấy từ code thật ở công ty.

---

## 0. Hiện trạng: đã kiểm tra những gì

| Project                                                                        | Stack                                                                                                     | Ghi chú                                             |
| ------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| Wynn `OUTABOX.API` (+ GCM, RESYNC, SECURITY, FloorEventService, TCPServerTool) | net7 Web API, EF Core 7 (SQL Server/Postgres/Oracle), JWT, SignalR, hosted services, TCP, Serilog, Docker | Project lớn nhất, ~7k dòng ở controllers + services |
| Tradies `OUTABOX.API` + `TradiesSync`                                          | net7 / net6, cùng bộ khung OUTABOX                                                                        | Copy từ cùng 1 template                             |
| Panther `OUTABOX.API`                                                          | net7, cùng bộ khung OUTABOX                                                                               | Copy lần thứ 4                                      |
| Jackpot `WebSocketServer`                                                      | **net10** console app, WebSocket, Npgsql/SQLite, OpenCV + ONNX (OCR), Serilog                             | Mới nhất, ~16k dòng, có thư mục `test/`             |

**Đích đến:** các project này dùng Web API, EF Core, JWT, SignalR, `BackgroundService`, `Channel<T>`, TCP/WebSocket, Serilog, Docker và nhiều loại DB. Học xong tuần 20 bạn phải đọc hiểu và sửa được chúng.

**Lỗ hổng tìm thấy trong code** (chưa cần hiểu hết lúc này: mỗi mục sẽ có lab tái hiện và sửa ở phase tương ứng):

| #   | Vấn đề                                                                                                                                                                                           | Ở đâu                                                                 | Mức độ            | Học ở      |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- | ----------------- | ---------- |
| 1   | Mật khẩu `sa` của SQL Server + secret key nằm dạng plaintext trong `appsettings.json`                                                                                                            | Wynn `OUTABOX.API/appsettings.json` dòng ~94, ~104                    | 🔴 Cao            | Phase 4    |
| 2   | `ServerCertificateValidationCallback += (...) => true` đặt trong **constructor controller**: tắt kiểm tra TLS cho toàn process, và cộng thêm 1 delegate mỗi request (rò bộ nhớ)                  | `WynnCoinController.cs` constructor                                   | 🔴 Cao            | Phase 4    |
| 3   | `new HttpClient(...)` mỗi lần gọi (10+ chỗ) → cạn socket khi tải cao                                                                                                                             | `WynnCoinController.cs`, `TimedHostedService.cs`, `FloorEventService` | 🟠 Trung bình-Cao | Phase 5    |
| 4   | `.Result` trên Task (block thread); repository trả về `Task<(Task<List<T>>, Task<long>)>`: 2 query song song trên cùng 1 `DbContext` có thể ném lỗi "A second operation was started"             | `BaseApiController.cs:88,224`, `IRepository.cs`                       | 🟠 Trung bình     | Phase 1, 3 |
| 5   | CORS `AllowAnyOrigin`, `FormOptions` giới hạn = `int.MaxValue` → dễ bị DoS                                                                                                                       | Wynn `Program.cs`                                                     | 🟠 Trung bình     | Phase 4    |
| 6   | God class: `WynnCoinController` 1762 dòng, `TCPWorker` 1548 dòng; anti-pattern `ServiceLocator`; generic repository bọc EF                                                                       | Wynn API, `OUTABOX.EF.WebApi/ServiceLocator.cs`                       | 🟡 Thiết kế       | Phase 3, 8 |
| 7   | Bộ `OUTABOX.*` bị copy-paste qua 4 solution (Panther/Tradies/Wynn/Wynn GCM) → sửa bug phải sửa 4 chỗ                                                                                             | Mọi project OUTABOX                                                   | 🟡 Thiết kế       | Phase 8    |
| 8   | net6/net7 **đã hết hỗ trợ** (EOL 2024), không còn bản vá bảo mật; trộn version package (HealthChecks 10.0 trên net7, `Diagnostics.HealthChecks 2.2.0` đã cũ); trộn Newtonsoft + System.Text.Json | Mọi project OUTABOX                                                   | 🟠 Trung bình     | Phase 7    |
| 9   | Bộ OUTABOX **không có test project nào**                                                                                                                                                         | Tất cả                                                                | 🟠 Trung bình     | Phase 6    |
| 10  | Jackpot là console app không dùng Generic Host: tự đọc config, logger tĩnh, bật/tắt feature bằng biến bool                                                                                       | Jackpot `Program.cs`                                                  | 🟡 Thiết kế       | Phase 7    |

> ⚠️ Mục 1 & 2 nên báo team xử lý ngay (rotate password, bỏ bypass TLS), không cần đợi học xong.

---

## Nguyên tắc học thực chiến

1. **Một capstone fullstack xuyên suốt: `RewardHub`**, hệ thống loyalty/reward cho máy slot: backend .NET 10, frontend React 19 + Vite + TS + TanStack Query, Postgres/Redis/RabbitMQ, **tất cả chạy bằng `docker compose`**. Cùng domain với công việc hiện tại, nhưng code mới hoàn toàn trong `Learn/C#`, **không copy code khách hàng**.
2. **Refactor kata**: tái hiện đoạn code xấu từ công việc (viết lại tối giản, đổi tên) → sửa → so sánh. Khi tự tin thì đề xuất PR thật cho team.
3. Mỗi tuần: 1 commit có ý nghĩa lên GitHub + 1 ghi chú TIL ngắn (`notes/YYYY-MM-DD.md`).
4. Target **.NET 10 (LTS, hỗ trợ tới 11/2028)**. Máy đang có SDK 11, build được net10.
5. **Day 1 = 19/11/2026.** Học 6 ngày/tuần (~1–1.5h/ngày, ngày thứ 6 là Boss lab 2–3h), nghỉ 1 ngày → **~20 tuần** (đến khoảng đầu tháng 4/2027), sau đó học liên tục ở Phase 8.
6. **Monitoring có mặt từ sớm:** Prometheus + Grafana vào compose từ tuần 7, mọi bài load test sau đó đều xem trên Grafana; tuần 18–19 nâng lên đủ stack production (Collector, Tempo, Loki, alert, SLO).

---

## Phase F: Nền tảng C# từ con số 0 (tuần 1–4)

roadmap.sh: _C# basics, OOP, .NET CLI, General development skills_

- [ ] Tuần 1: cú pháp, kiểu dữ liệu (`decimal` cho tiền), điều kiện, vòng lặp, method, `List`/`Dictionary`
- [ ] Tuần 2: OOP: class, property, value vs reference type, nullable, interface, kế thừa, `enum`, `record`, exception
- [ ] Tuần 3: generics, lambda/`Func`, LINQ, file I/O, viết test xUnit đầu tiên
- [ ] Tuần 4: `async/await` (so với Promise), `HttpClient`, NuGet, Dockerfile + compose với Postgres

**Xong khi:** tự viết được console app nhiều file có test, gọi API, lưu Postgres, chạy bằng `docker compose up` (Boss lab tuần 4).

---

## Phase 0: Setup chuẩn (đầu tuần 5)

- [ ] `dotnet new sln -n RewardHub`, layout: `src/RewardHub.Api`, `tests/RewardHub.Tests`
- [ ] `Directory.Build.props`: `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<ImplicitUsings>enable</ImplicitUsings>`
- [ ] Central Package Management (`Directory.Packages.props`): sửa ngay bệnh "trộn version" (#8)
- [ ] `.editorconfig` + `dotnet format --verify-no-changes`
- [ ] Docker Desktop: Postgres + SQL Server bằng `docker compose`
- [ ] IDE: Rider hoặc VS Code + C# Dev Kit; biết debug, đặt breakpoint có điều kiện, xem Parallel Stacks

**Xong khi:** `dotnet build` sạch warning, `docker compose up` có DB.

---

## Phase 1: C# hiện đại + async thật kỹ (tuần 5–6)

roadmap.sh: _C# fundamentals, General development skills_

Đã học ở Phase F: class, interface, generics, LINQ cơ bản.
Học kỹ:

- [ ] `record`, `init`, primary constructor, pattern matching (`switch` expression, list patterns)
- [ ] Nullable reference types: đọc hiểu warning, không dùng `!` bừa
- [ ] **async/await bên trong**: `Task` vs `ValueTask`, `CancellationToken` truyền xuyên suốt, vì sao `.Result`/`.Wait()` gây thread-pool starvation (#4), `Task.WhenAll`, `IAsyncEnumerable`
- [ ] `IDisposable`/`IAsyncDisposable`, `using` declaration
- [ ] `Span<T>`, `ReadOnlySpan<char>`: parse chuỗi không cấp phát
- [ ] Collections: `Dictionary`, `ConcurrentDictionary`, `FrozenDictionary`, `Channel<T>`

**🛠 Kata 1: Parser message TCP**
Code Wynn nhận chuỗi fixed-width kiểu `!959036C19100275013937857798...` (card-in, rating). Viết `CardInMessage.TryParse(ReadOnlySpan<char>)` trả về `record`, kèm 10 test xUnit (hợp lệ, thiếu ký tự, sai type code). Đây chính là nền để refactor `TCPWorker` sau này.

**🛠 Kata 2: Săn `.Result`**
Viết lại tối giản `BaseApiController` paging với `(Task, Task)` → sửa thành 2 lệnh `await` tuần tự (hoặc 1 query). Giải thích được vì sao EF Core không cho 2 query song song trên 1 context.

---

## Phase 2: ASP.NET Core nền tảng (tuần 7–8)

roadmap.sh: _ASP.NET Core basics, Dependency Injection, Configuration, Middleware, REST_

- [ ] Hosting model: `WebApplication.CreateBuilder`, Kestrel, environment
- [ ] Middleware pipeline: thứ tự quan trọng, tự viết 1 middleware (correlation id)
- [ ] Controllers vs **Minimal APIs** (+ `MapGroup`, endpoint filters, `TypedResults`)
- [ ] **DI lifetimes**: Singleton / Scoped / Transient, lỗi _captive dependency_, `ValidateScopes`, keyed services. Hiểu vì sao `ServiceLocator` (#6) là anti-pattern
- [ ] **Options pattern**: `IOptions` / `IOptionsMonitor`, `ValidateDataAnnotations().ValidateOnStart()`
- [ ] Model binding + validation (FluentValidation hoặc built-in validation của .NET 10 minimal API)
- [ ] Xử lý lỗi: `IExceptionHandler` + `ProblemDetails` (RFC 9457) thay cho try/catch rải rác
- [ ] OpenAPI: `Microsoft.AspNetCore.OpenApi` (built-in) + Scalar UI
- [ ] REST đúng chuẩn: status code, idempotency, paging, versioning (`Asp.Versioning`)

**🛠 Capstone v1:** `RewardHub` CRUD `Players`, `Machines`, `Rewards` bằng Minimal API, ProblemDetails, OpenAPI, config qua Options có validate.

**🛠 Kata 3:** Viết lại `Program.cs` dài ~300 dòng của Wynn thành các extension method `AddAuth()`, `AddSwaggerDocs()`, `AddApiCors()`, mỗi cái đọc config qua Options có validate.

---

## Phase 3: Data: SQL + EF Core + Dapper (tuần 9–11)

roadmap.sh: _Databases, ORM, EF Core, Dapper, Caching (phần DB)_

- [ ] SQL nền tảng: index (clustered/non-clustered/covering), đọc **execution plan**, transaction + isolation level, deadlock
- [ ] EF Core 10: lifetime `DbContext`, migrations (+ migration bundles cho CI), `AsNoTracking`, projection `Select` thay vì load entity, N+1 & `AsSplitQuery`, `ExecuteUpdate`/`ExecuteDelete`, concurrency token (`rowversion` / `xmin`), interceptors, compiled queries
- [ ] Bỏ thói quen: lazy loading proxies (có trong OUTABOX), generic repository bọc EF
- [ ] Dapper cho hot path / report
- [ ] Postgres vs SQL Server: khác biệt chính (sequence, JSONB, case sensitivity)

**🛠 Capstone v2: Wallet "Coin" (đường tiền, phải đúng tuyệt đối)**
`POST /wallets/{id}/credit` với: optimistic concurrency, **Idempotency-Key header** (gọi lại không cộng tiền 2 lần), bảng ledger append-only, transaction. Viết test bắn 50 request song song, số dư vẫn đúng.

**🛠 Kata 4:** Xoá `IRepository<T>` generic, dùng `DbContext` trực tiếp trong handler + vài query extension. So sánh số dòng trước/sau.

---

## Phase 4: Bảo mật (tuần 12–13)

roadmap.sh: _Authentication, Authorization, Web Security, OWASP_

- [ ] AuthN: JWT đúng cách (thời hạn ngắn, refresh token, rotate key), ASP.NET Core Identity / IdP ngoài (Keycloak, Entra ID)
- [ ] AuthZ: policy-based, resource-based, claims
- [ ] **Secrets**: `dotnet user-secrets` (dev), biến môi trường / Docker secrets / Azure Key Vault (prod). Không bao giờ commit secret (#1); dùng `gitleaks` quét repo
- [ ] TLS: không bao giờ `=> true`; nếu server đối tác dùng cert tự ký thì **pin đúng cert đó** trên 1 named `HttpClient` (#2)
- [ ] CORS đúng origin, request size limit hợp lý (#5), **Rate limiting** built-in (`AddRateLimiter`)
- [ ] OWASP API Security Top 10: BOLA, mass assignment, SSRF (code tải HTML/ảnh trong Wynn có rủi ro này)
- [ ] Hash mật khẩu (BCrypt/Argon2), Data Protection API

**🛠 Capstone v3:** JWT + refresh token, policy `CanCreditWallet`, rate limit endpoint credit, secrets qua user-secrets.

**🛠 Kata 5:** Tái hiện `WynnCoinController` constructor (bypass TLS + `ServicePointManager`) → sửa thành named client với cert pinning. Chạy `gitleaks` trên 1 repo công việc (local, không push kết quả lên đâu) và báo team.

---

## Phase 5: Tích hợp, background, real-time, cache (tuần 14–15)

roadmap.sh: _Resilience, Background tasks, Messaging, Real-time, Caching_

- [ ] **`IHttpClientFactory`** + typed client (#3), `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler`: retry, timeout, circuit breaker; Polly v8 bên dưới)
- [ ] Chỉ retry request idempotent; timeout tổng vs timeout mỗi lần thử
- [ ] Webhook: verify chữ ký HMAC, idempotency, trả 2xx nhanh rồi xử lý async
- [ ] Background: `BackgroundService` + `Channel<T>` (đã làm ở tuần 6, giờ thêm graceful shutdown + `CancellationToken`), Hangfire/Quartz cho job định kỳ
- [ ] **Outbox pattern** (ghi DB + event cùng transaction)
- [ ] Message broker: RabbitMQ (`RabbitMQ.Client` trực tiếp hoặc Wolverine; lưu ý MassTransit v9 đã có license thương mại)
- [ ] SignalR: group, auth, scale-out bằng Redis backplane
- [ ] Cache: `IMemoryCache`, **`HybridCache`** (.NET 9+, chống cache stampede), Redis, Output caching

**🛠 Capstone v4:** Typed client gọi "GCM API" giả (dùng WireMock.Net hoặc Postman mock), resilience handler; khi credit wallet thì ghi outbox → worker publish lên RabbitMQ → SignalR đẩy notification về client.

**🛠 Kata 6:** Viết lại 1 hàm trong `WynnCoinController` dùng `new HttpClient(new LoggingHandler(...))` thành typed client + `DelegatingHandler` đăng ký qua DI.

---

## Phase 6: Testing (tuần 16–17)

roadmap.sh: _Testing: Unit, Integration, E2E_

- [ ] xUnit v3, assertion bằng Shouldly (FluentAssertions v8 đã có license thương mại), NSubstitute
- [ ] Test đúng tầng: domain logic → unit; endpoint → **integration với `WebApplicationFactory`**
- [ ] **Testcontainers** (Postgres/SQL Server/Redis/RabbitMQ thật trong Docker) + Respawn reset DB
- [ ] Characterization test: chụp lại hành vi code cũ trước khi refactor
- [ ] API test bằng Postman CLI (máy đã có) chạy trong CI

**🛠 Capstone v5:** coverage >70% cho phần Wallet; integration test chạy với Postgres thật qua Testcontainers.

**🛠 Kata 7:** Viết characterization test cho parser TCP (Kata 1) với 20 message thật đã ẩn danh, rồi mới tách `TCPWorker` thành `MessageParser` + `MessageHandler`s.

---

## Phase 7: Production-ready (tuần 18–19)

roadmap.sh: _Logging, Observability, Containerization, CI/CD, Performance_

- [ ] Serilog structured logging đúng chuẩn (message template, không nối chuỗi; enrich correlation id)
- [ ] **OpenTelemetry .NET** cho cả 3 loại tín hiệu: metrics, traces, logs. Dùng meter có sẵn của .NET (`Microsoft.AspNetCore.Hosting`, `Microsoft.AspNetCore.Server.Kestrel`, `System.Net.Http`, `System.Runtime`) + instrumentation cho Npgsql, HttpClient
- [ ] **Stack monitoring production:** app → OTLP → **OTel Collector** → **Prometheus** (metrics) / **Tempo** (traces) / **Loki** (logs) → **Grafana**; **Alertmanager** gửi cảnh báo
- [ ] PromQL căn bản: `rate`, `sum by`, `histogram_quantile`; recording rules; tránh label nhiều giá trị (high cardinality)
- [ ] Dashboard kiểu RED (Rate, Errors, Duration) cho API, USE cho DB/Redis; dashboard và datasource **khai báo bằng file trong git** (provisioning)
- [ ] Exemplars: từ điểm p95 trên biểu đồ bấm sang trace để tìm nguyên nhân
- [ ] SLO + error budget + cảnh báo burn rate
- [ ] Health checks: tách liveness / readiness
- [ ] Docker multi-stage, chiseled image, `dotnet publish /t:PublishContainer`
- [ ] GitHub Actions: build → format check → test (Testcontainers) → publish image
- [ ] Hiệu năng: BenchmarkDotNet, `dotnet-counters`, `dotnet-trace`, k6 load test
- [ ] **Nâng cấp phiên bản**: net7 → net10 (breaking changes, EF Core 7 → 10)

**🛠 Capstone v6:** RewardHub có đủ stack Prometheus + Grafana + Tempo + Loki + Alertmanager, dashboard RED và SLO commit trong git, alert đã thử bắn thật, CI xanh, image < 150MB, deploy lên VPS kèm monitoring.

> Không dùng `prometheus-net` (2,1k⭐) hay AppMetrics cho dự án mới: cả hai đã lâu không cập nhật (lần cuối 04/2024 và 07/2025). OpenTelemetry .NET là chuẩn hiện tại và được duy trì liên tục.

**🛠 Kata 8:** Tái hiện `Program.cs` của Jackpot (console + bool flags + logger tĩnh) → chuyển sang `Host.CreateApplicationBuilder` + mỗi module là 1 `BackgroundService` bật qua Options (#10).

**🛠 Kata 9:** Lập kế hoạch nâng cấp 1 project OUTABOX lên net10 (liệt kê breaking changes, package cần đổi) để đề xuất cho team.

---

## Phase 8: Kiến trúc (tuần 20 trở đi)

roadmap.sh: _Design patterns, Architecture, Microservices_

- [ ] SOLID ở mức thực dụng, chỉ trừu tượng hoá khi có ≥2 implementation
- [ ] **Vertical Slice Architecture** (khuyên dùng cho quy mô team hiện tại) vs Clean Architecture
- [ ] Modular monolith trước, microservices sau (chỉ khi có lý do về team/scale)
- [ ] DDD tactical: aggregate, value object, domain event (dùng cho Wallet)
- [ ] CQRS không cần MediatR (MediatR / AutoMapper đã có license thương mại → dùng handler thường + Mapperly hoặc map tay)
- [ ] gRPC cho giao tiếp nội bộ service
- [ ] **Đóng gói code dùng chung** thành internal NuGet package (GitHub Packages / Azure Artifacts) (#7)

**🛠 Kata 10 (dự án lớn):** Đề xuất tách `OUTABOX.Helper` + `OUTABOX.EF.*` thành 1 package có version, để 4 solution cùng dùng, thay cho 4 bản copy.

---

## Những phần trên roadmap.sh nên **bỏ qua / để sau**

| Mục                       | Lý do                                  |
| ------------------------- | -------------------------------------- |
| Blazor, MAUI, Razor Pages | Không phải backend API                 |
| GraphQL (HotChocolate)    | Chưa có nhu cầu trong công việc        |
| Orleans, Akka.NET         | Niche, học khi thật sự cần actor model |
| Kubernetes chuyên sâu     | Docker Compose là đủ ở giai đoạn này   |
| Elasticsearch, ELK        | Loki + Grafana là đủ                   |
| AutoMapper, MediatR       | License thương mại + không cần thiết   |

---

## Tài liệu chính

- Microsoft Learn: ASP.NET Core docs, EF Core docs (đọc mục _Performance_ kỹ)
- Sách: _C# in Depth_ (Jon Skeet), _Dependency Injection Principles, Practices, and Patterns_ (Seemann), _Designing Data-Intensive Applications_ (Kleppmann)
- `davidfowl/AspNetCoreDiagnosticScenarios`: **bắt buộc đọc phần async guidance**, khớp đúng lỗi #3, #4
- Blog: Andrew Lock, Steve Gordon (HttpClient), Milan Jovanović, Nick Chapsas

### Repo tham khảo (số sao kiểm tra trên GitHub ngày 19/11/2026)

**API production / kiến trúc:** đọc để học cách tổ chức, đừng copy nguyên.

| Repo                                       | ⭐    | Học gì                                                                                       |
| ------------------------------------------ | ----- | -------------------------------------------------------------------------------------------- |
| `jasontaylordev/CleanArchitecture`         | 20,7k | Template Clean Architecture phổ biến nhất, cập nhật liên tục                                 |
| `ardalis/CleanArchitecture`                | 18,6k | Template thứ hai, có phiên bản tối giản để so sánh                                           |
| `kgrzybek/modular-monolith-with-ddd`       | 14,1k | Modular monolith + DDD (ít cập nhật từ 2024, đọc phần thiết kế)                              |
| `dotnet/eShop`                             | 11,0k | Ứng dụng mẫu chính thức của Microsoft: Aspire, OTel, RabbitMQ                                |
| `davidfowl/TodoApp`                        | 3,1k  | Minimal API + auth gọn, dễ đọc cho người mới                                                 |
| `phongnguyend/Practical.CleanArchitecture` | 2,5k  | .NET 10 fullstack: monolith, modular monolith, microservices                                 |
| `meysamhadeli/booking-microservices`       | 1,4k  | Vertical slice + **đủ stack OTel Collector, Prometheus, Grafana, Tempo, Loki** trong compose |
| `microsoft/aspire-samples`                 | 1,2k  | Mẫu Aspire chính thức                                                                        |

**Monitoring:**

| Repo                                   | ⭐   | Học gì                                                                                                       |
| -------------------------------------- | ---- | ------------------------------------------------------------------------------------------------------------ |
| `open-telemetry/opentelemetry-dotnet`  | 3,8k | SDK chính thức, đọc thư mục `examples/`                                                                      |
| `grafana/docker-otel-lgtm`             | 2,2k | Grafana + Prometheus + Tempo + Loki trong 1 container, dùng cho dev                                          |
| `JamesNK/aspnetcore-grafana`           | 288  | Dashboard ASP.NET Core của Microsoft (Grafana ID 19924, 19925); bản mới nằm ở `aka.ms/dotnet/grafana-source` |
| `petabridge/dotnet-grafana-dashboards` | 112  | Dashboard Grafana cho các metric OTel của .NET                                                               |
| `Depechie/OpenTelemetryGrafana`        | 88   | Ví dụ nhỏ, dễ đọc: compose + provisioning Grafana + **alert rules**                                          |

**Repo nhiều sao nhưng không nên theo:** `dotnet-architecture/eShopOnContainers` (24k⭐, ngừng từ 2023, đã thay bằng `dotnet/eShop`), `prometheus-net` và AppMetrics (lâu không cập nhật).

## Theo dõi tiến độ

Chi tiết từng ngày: [LABS.md](LABS.md).

| Tuần  | Phase | Capstone / Boss lab                                         | Kata | Done |
| ----- | ----- | ----------------------------------------------------------- | ---- | ---- |
| 1–4   | F     | Slot machine CLI → API + Postgres trong Docker              | –    | [ ]  |
| 5–6   | 0–1   | FloorSimulator (TCP)                                        | 1, 2 | [ ]  |
| 7–8   | 2     | v1: API + React CRUD + Prometheus/Grafana đầu tiên          | 3    | [ ]  |
| 9–11  | 3     | v2: Dashboard + Wallet                                      | 4    | [ ]  |
| 12–13 | 4     | v3: Auth (Keycloak) + pentest                               | 5    | [ ]  |
| 14–15 | 5     | v4: Live Floor (RabbitMQ + SignalR)                         | 6    | [ ]  |
| 16–17 | 6     | v5: Tests + CI                                              | 7    | [ ]  |
| 18–19 | 7     | v6: Prometheus + Grafana + Tempo + Loki, alert, SLO, deploy | 8, 9 | [ ]  |
| 20+   | 8     | Kiến trúc + Final demo                                      | 10   | [ ]  |
