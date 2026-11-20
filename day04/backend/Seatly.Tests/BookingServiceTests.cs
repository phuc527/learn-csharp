using Seatly.Api.Domain;
using Seatly.Api.Repositories;
using Seatly.Api.Services;

namespace Seatly.Tests;

// Unit test cho luật nghiệp vụ. Go: backend/internal/service/booking_service_test.go
public class BookingServiceTests
{
    private static BookingService NewService() =>
        new(new InMemorySeatRepository([new Seat("A1", "A", 1, 100, Booked: false)]));

    // [Theory] + [InlineData] = table-driven test của Go: mỗi dòng 1 trường hợp.
    [Theory]
    [InlineData("Z9", BookingStatus.NotFound)]
    [InlineData("   ", BookingStatus.InvalidSeatId)]
    [InlineData(null, BookingStatus.InvalidSeatId)]
    public void BookSeat_InvalidInput_ReturnsError(string? seatId, BookingStatus expected)
    {
        Assert.Equal(expected, NewService().BookSeat(seatId).Status);
    }

    [Fact]
    public void BookSeat_Twice_SecondIsAlreadyBooked()
    {
        var svc = NewService();
        Assert.Equal(BookingStatus.Booked, svc.BookSeat("A1").Status);
        Assert.Equal(BookingStatus.AlreadyBooked, svc.BookSeat("A1").Status);
    }

    [Fact]
    public void BookSeat_NormalizesId()
    {
        var result = NewService().BookSeat(" a1 ");
        Assert.Equal(BookingStatus.Booked, result.Status);
        Assert.Equal("A1", result.Seat?.Id);
        Assert.True(result.Seat?.Booked);
    }

    // 100 người cùng giành A1 → chỉ đúng 1 người thành công.
    [Fact]
    public void BookSeat_Concurrent_OnlyOneWins()
    {
        var svc = NewService();
        int ok = 0;
        Parallel.For(0, 100, _ =>
        {
            if (svc.BookSeat("A1").Status == BookingStatus.Booked)
            {
                Interlocked.Increment(ref ok); // ++ không an toàn khi nhiều thread cùng tăng
            }
        });
        Assert.Equal(1, ok);
    }
}
