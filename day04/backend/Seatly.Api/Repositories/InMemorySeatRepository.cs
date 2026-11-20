using Seatly.Api.Domain;

namespace Seatly.Api.Repositories;

// Lưu ghế trong RAM. Đăng ký Singleton → cả app dùng chung 1 instance,
// nhiều request chạy song song trên nhiều thread → phải khoá.
// Go: backend/internal/repository/memory/seat_repository.go (sync.Mutex)
public class InMemorySeatRepository(IEnumerable<Seat> seed) : ISeatRepository
{
    private readonly Lock _lock = new(); // kiểu Lock mới từ .NET 9, dùng với từ khoá lock
    private readonly List<Seat> _seats = [.. seed];

    public IReadOnlyList<Seat> List()
    {
        lock (_lock)
        {
            // Trả bản copy: bên ngoài sửa list cũng không ảnh hưởng dữ liệu bên trong.
            return [.. _seats];
        }
    }

    // Kiểm tra + đánh dấu đã đặt trong CÙNG 1 lần khoá → không thể đặt trùng.
    // (Postgres sau này: UPDATE seats SET booked = true WHERE id = @id AND booked = false)
    public BookingResult Book(string seatId)
    {
        lock (_lock)
        {
            int i = _seats.FindIndex(s => s.Id == seatId);
            if (i == -1)
            {
                return new BookingResult(BookingStatus.NotFound);
            }
            if (_seats[i].Booked)
            {
                return new BookingResult(BookingStatus.AlreadyBooked);
            }

            // record bất biến: "with" tạo bản sao với Booked = true
            _seats[i] = _seats[i] with { Booked = true };
            return new BookingResult(BookingStatus.Booked, _seats[i]);
        }
    }

    // 3 hàng A/B/C × 5 ghế, hàng A giá 1.000.000đ. Khi có DB, phần này nằm trong migration/seed.
    public static IEnumerable<Seat> SeedSeats()
    {
        foreach (string row in new[] { "A", "B", "C" })
        {
            int price = row == "A" ? 1_000_000 : 500_000;
            for (int n = 1; n <= 5; n++)
            {
                yield return new Seat($"{row}{n}", row, n, price, Booked: false);
            }
        }
    }
}
