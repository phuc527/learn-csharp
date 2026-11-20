using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Seatly.Api.Domain;
using Seatly.Api.Http;

namespace Seatly.Tests;

// Integration test: chạy cả app trong RAM (không mở cổng thật), gọi HTTP như FE gọi.
// Go: backend/internal/httpapi/seat_handler_test.go (httptest)
public class SeatApiTests : IDisposable
{
    // Mỗi test 1 factory mới → dữ liệu ghế sạch, test không ảnh hưởng nhau.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public SeatApiTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetSeats_Returns15Seats()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<Seat>>>("/api/seats");

        Assert.True(body!.Success);
        Assert.Equal(15, body.Data!.Count);
        Assert.Equal(1_000_000, body.Data.Single(s => s.Id == "A1").Price);
    }

    [Fact]
    public async Task Book_ThenBookAgain_Returns201Then409()
    {
        var first = await _client.PostAsJsonAsync("/api/bookings", new { seatId = "B2" });
        var second = await _client.PostAsJsonAsync("/api/bookings", new { seatId = "B2" });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var error = await second.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(error!.Success);
        Assert.Equal("Ghế B2 đã có người đặt", error.Message);
    }

    [Theory]
    [InlineData("""{"seatId": "Z9"}""", HttpStatusCode.NotFound)]
    [InlineData("""{"seatId": ""}""", HttpStatusCode.BadRequest)]
    [InlineData("""{"seat": "A1"}""", HttpStatusCode.BadRequest)] // field lạ
    [InlineData("không phải json", HttpStatusCode.BadRequest)]
    public async Task Book_BadInput_ReturnsErrorEnvelope(string json, HttpStatusCode expected)
    {
        var res = await _client.PostAsync("/api/bookings",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(expected, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(body!.Success); // lỗi nào cũng trả đúng envelope, FE đọc được message
    }
}
