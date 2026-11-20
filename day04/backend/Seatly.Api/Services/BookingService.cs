using Seatly.Api.Domain;
using Seatly.Api.Repositories;

namespace Seatly.Api.Services;

// Luật nghiệp vụ. Không biết gì về HTTP hay DB cụ thể.
// Primary constructor: (ISeatRepository repo) được DI tự truyền vào.
// Go: backend/internal/service/booking_service.go
public class BookingService(ISeatRepository repo)
{
    public IReadOnlyList<Seat> ListSeats() => repo.List();

    public BookingResult BookSeat(string? seatId)
    {
        string id = seatId?.Trim().ToUpperInvariant() ?? "";
        if (id == "")
        {
            return new BookingResult(BookingStatus.InvalidSeatId);
        }
        return repo.Book(id);
    }
}
