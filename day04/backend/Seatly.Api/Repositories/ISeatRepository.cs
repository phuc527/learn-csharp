using Seatly.Api.Domain;

namespace Seatly.Api.Repositories;

// "Hợp đồng" lưu trữ. Hôm nay: InMemorySeatRepository. Sau này: PostgresSeatRepository (EF Core),
// service không phải sửa. Khác Go: C# đặt interface cạnh implementation, đăng ký qua DI.
public interface ISeatRepository
{
    IReadOnlyList<Seat> List();
    BookingResult Book(string seatId);
}
