namespace Seatly.Api.Http;

// Khuôn chung cho MỌI response, FE chỉ cần 1 cách đọc:
//   thành công: {"success": true,  "data": ..., "message": "..."}
//   lỗi:        {"success": false, "data": null, "message": "Ghế A1 đã có người đặt"}
// HTTP status vẫn đúng chuẩn (201, 404, 409...), envelope chỉ bổ sung chứ không thay thế.
// Go: backend/internal/httpapi/response.go
public record ApiResponse<T>(bool Success, T? Data, string Message);

public static class Api
{
    public static IResult Success<T>(T data, string message = "", int status = StatusCodes.Status200OK) =>
        Results.Json(new ApiResponse<T>(true, data, message), statusCode: status);

    public static IResult Error(int status, string message) =>
        Results.Json(new ApiResponse<object>(false, null, message), statusCode: status);
}
