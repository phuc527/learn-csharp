# Day 4: Seatly, app đặt vé (C# + React)

Bản C# của `Go-Fullstack/seatly-fresher`. **API contract giữ nguyên**, nên frontend React gần như copy y hệt (chỉ sửa comment). Có 1 sự kiện, 15 ghế (A/B/C × 5), đặt từng ghế, dữ liệu lưu trong RAM.

```
day04/
├── backend/
│   ├── Seatly.slnx                    # solution: gom 2 project
│   ├── Seatly.Api/
│   │   ├── Program.cs                 # khởi động: log, DI, middleware, metrics, route
│   │   ├── Domain/Seat.cs             # Seat, BookingStatus, BookingResult
│   │   ├── Repositories/              # ISeatRepository + InMemorySeatRepository (lock)
│   │   ├── Services/BookingService.cs # luật nghiệp vụ
│   │   ├── Http/                      # ApiResponse (envelope) + SeatEndpoints (route)
│   │   └── Dockerfile
│   └── Seatly.Tests/                  # xUnit: unit test service + integration test HTTP
├── frontend/                          # React 19 + Vite + TS + React Query (copy từ bản Go)
├── monitoring/                        # Prometheus (scrape + alerts) + Grafana (provisioning + 3 dashboard)
└── compose.yaml
```

## Cách 1: chạy khi code hằng ngày

```bash
# Terminal 1: backend → http://localhost:8080
cd day04/backend/Seatly.Api
dotnet watch        # tự reload khi sửa code (hơn Go phải Ctrl+C chạy lại)
```

```bash
# Terminal 2: frontend → http://localhost:5173
cd day04/frontend
npm install         # lần đầu
npm run dev
```

Vite chuyển `/api/...` sang `localhost:8080` (`vite.config.ts`), nên không cần CORS.

**Debug:** mở `day04/backend` trong VS Code, đặt breakpoint ở dòng `BookingResult result = svc.BookSeat(...)` trong `SeatEndpoints.cs`, bấm F5, rồi bấm "Đặt ghế" trên web.

**Test:**

```bash
cd day04/backend
dotnet test         # 12 test: luật đặt ghế, 100 thread giành 1 ghế, HTTP 201/409/404/400
```

## Cách 2: Docker (cả monitoring)

```bash
cd day04
docker compose up -d --build
docker compose logs -f backend   # log JSON, mỗi request 1 dòng
docker compose down              # thêm -v để xoá dữ liệu Prometheus/Grafana
```

| Service      | URL                   | Ghi chú                                                                                      |
| ------------ | --------------------- | -------------------------------------------------------------------------------------------- |
| `frontend`   | http://localhost:3000 | nginx phục vụ React, chuyển `/api` sang `backend`. `/metrics` không lộ ra ngoài              |
| `backend`    | http://localhost:8080 | `frontend` chỉ chạy khi `/healthz` của backend OK. Chạy user non-root                        |
| `prometheus` | http://localhost:9090 | Status → Targets (`seatly-api` UP), Alerts (4 rule)                                          |
| `grafana`    | http://localhost:3001 | admin/admin. Home: **Seatly Overview**. Folder Seatly có 2 dashboard ASP.NET Core chính thức |

**Thử monitoring:** đặt vài ghế, cố tình đặt trùng, rồi xem panel "Tỉ lệ kết quả đặt ghế" trên Grafana. Bắn tải từ Git Bash:

```bash
for i in $(seq 1 200); do curl -s -o /dev/null localhost:8080/api/seats; done
```

## Go → C#: cùng một việc, hai cách viết

| Việc                 | Go (seatly-fresher)                         | C# (bản này)                                                      |
| -------------------- | ------------------------------------------- | ----------------------------------------------------------------- |
| Model                | `struct` + tag `json:"id"`                  | `record Seat(...)`; JSON tự đổi sang camelCase                    |
| Lỗi nghiệp vụ        | sentinel error + `errors.Is`                | `enum BookingStatus` + `switch` expression                        |
| Khoá chống đặt trùng | `sync.Mutex`                                | `Lock` + từ khoá `lock`                                           |
| Nối các layer        | gọi `New...()` bằng tay trong `main.go`     | DI: `AddSingleton<...>()`, framework tự truyền vào constructor    |
| Route                | `mux.HandleFunc("POST /api/bookings", ...)` | `api.MapPost("/bookings", ...)`, `MapGroup("/api")`               |
| Đọc body             | `readJSON` + `DisallowUnknownFields`        | tham số `BookRequest body` + `UnmappedMemberHandling.Disallow`    |
| Middleware           | tự viết `logRequests`, `recoverPanic`       | có sẵn: `UseHttpLogging`, `UseExceptionHandler`                   |
| Metrics HTTP         | tự viết `measure` + `promauto`              | OpenTelemetry `AddAspNetCoreInstrumentation()`, không cần tự viết |
| Graceful shutdown    | `signal.NotifyContext` + `srv.Shutdown`     | có sẵn trong host                                                 |
| Test                 | table test + `httptest`                     | `[Theory]` + `[InlineData]`, `WebApplicationFactory<Program>`     |

Nhận xét: Go bắt bạn tự viết nhiều thứ hơn (hiểu sâu). ASP.NET Core có sẵn gần hết, nên việc chính là **biết cái nào có sẵn và cấu hình đúng**.

## Thứ tự đọc code

1. `Domain/Seat.cs`: dữ liệu và kết quả nghiệp vụ
2. `Repositories/InMemorySeatRepository.cs`: `lock`, `with`, `yield return`
3. `Services/BookingService.cs`: primary constructor, `?.` và `??`
4. `Http/SeatEndpoints.cs`: route, switch expression, tuple `(metric, response)`, metric nghiệp vụ
5. `Program.cs`: ghép mọi thứ lại
6. `Seatly.Tests/`: đọc test để hiểu hành vi mong đợi

## Bài tập (từ dễ đến khó)

1. ⭐ Thêm hàng D giá 300.000đ. Sửa `SeedSeats()`, chạy lại, xem FE tự hiện.
2. ⭐ `GET /api/seats?available=true` chỉ trả ghế trống. Gợi ý: tham số `bool? available` + LINQ `.Where(s => !s.Booked)`.
3. ⭐⭐ `DELETE /api/bookings/{seatId}` để huỷ đặt: 200 nếu huỷ được, 404 nếu ghế không có, 409 nếu ghế chưa đặt. Viết test trước.
4. ⭐⭐ Đặt nhiều ghế 1 lần: `{"seatIds": ["A1","A2"]}`, **tất cả hoặc không có ghế nào** (một ghế bị trùng thì không đặt ghế nào).
5. ⭐⭐⭐ Thêm field `customerName` (bắt buộc, 2-50 ký tự). Thiếu thì trả 400 kèm message rõ ràng.

Các level tiếp theo (theo roadmap): Postgres + EF Core thay `InMemorySeatRepository` (tuần 5-6), giữ chỗ tạm có hết hạn bằng Redis, rồi auth.
