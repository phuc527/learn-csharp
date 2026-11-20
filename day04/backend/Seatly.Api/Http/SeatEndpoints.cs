using System.Diagnostics.Metrics;
using Seatly.Api.Domain;
using Seatly.Api.Services;

namespace Seatly.Api.Http;

// Layer HTTP: route → gọi service → đổi kết quả ra HTTP status. Chỉ layer này biết về HTTP.
// Go: backend/internal/httpapi/seat_handler.go
public static class SeatEndpoints
{
    public const string MeterName = "Seatly";
    private static readonly Meter Meter = new(MeterName);

    // Metric nghiệp vụ: bao nhiêu lượt đặt thành công / bị trùng / sai ghế.
    // Prometheus: seatly_bookings_total{result="booked|conflict|not_found|invalid"}
    private static readonly Counter<long> Bookings =
        Meter.CreateCounter<long>("seatly.bookings", description: "Số lượt đặt ghế theo kết quả");

    private const string BadBody = """Body phải có dạng {"seatId": "A1"}""";

    // Extension method: cho phép viết app.MapSeatEndpoints() trong Program.cs
    public static void MapSeatEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        // GET /api/seats
        api.MapGet("/seats", (BookingService svc) => Api.Success(svc.ListSeats()));

        // POST /api/bookings  body: {"seatId": "A1"}
        api.MapPost("/bookings", (BookRequest body, BookingService svc) =>
        {
            BookingResult result = svc.BookSeat(body.SeatId);

            // switch expression trên enum → mỗi kết quả nghiệp vụ 1 HTTP status
            (string metric, IResult response) = result.Status switch
            {
                BookingStatus.Booked => ("booked",
                    Api.Success(result.Seat, $"Đặt ghế {result.Seat!.Id} thành công", StatusCodes.Status201Created)),
                BookingStatus.InvalidSeatId => ("invalid", Api.Error(StatusCodes.Status400BadRequest, BadBody)),
                BookingStatus.NotFound => ("not_found",
                    Api.Error(StatusCodes.Status404NotFound, $"Ghế {body.SeatId} không tồn tại")),
                BookingStatus.AlreadyBooked => ("conflict",
                    Api.Error(StatusCodes.Status409Conflict, $"Ghế {body.SeatId} đã có người đặt")),
                _ => throw new InvalidOperationException($"Chưa xử lý {result.Status}"),
            };

            Bookings.Add(1, new KeyValuePair<string, object?>("result", metric));
            return response;
        });
    }
}

public record BookRequest(string? SeatId);
