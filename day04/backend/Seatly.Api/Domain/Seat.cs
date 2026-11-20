// "Từ vựng" của nghiệp vụ. Không phụ thuộc HTTP hay DB → layer nào cũng dùng được.
// Go: backend/internal/domain/seat.go
namespace Seatly.Api.Domain;

// record = class bất biến, tự có Equals/ToString. Tên property PascalCase,
// System.Text.Json tự đổi sang camelCase khi trả JSON: Id → "id", Booked → "booked".
public record Seat(string Id, string Row, int Number, int Price, bool Booked);

// Go dùng sentinel error (ErrSeatNotFound...). C# hay dùng exception, nhưng lỗi nghiệp vụ
// "đoán trước được" thì trả về kết quả rõ ràng sẽ dễ đọc hơn và không tốn chi phí throw.
public enum BookingStatus
{
    Booked,
    InvalidSeatId,
    NotFound,
    AlreadyBooked,
}

public record BookingResult(BookingStatus Status, Seat? Seat = null);
