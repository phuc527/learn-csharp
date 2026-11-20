// Khớp với record Seat trong backend/Seatly.Api/Domain/Seat.cs
export type Seat = {
  id: string;
  row: string;
  number: number;
  price: number;
  booked: boolean;
};

// Mọi response của backend có dạng này (xem backend/Seatly.Api/Http/ApiResponse.cs)
type ApiResponse<T> = {
  success: boolean;
  data: T;
  message: string;
};

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);
  const body: ApiResponse<T> | null = await res.json().catch(() => null);
  if (!res.ok || !body?.success) {
    throw new Error(body?.message || `Lỗi máy chủ (${res.status})`);
  }
  return body.data;
}

export const getSeats = () => request<Seat[]>("/api/seats");

export const bookSeat = (seatId: string) =>
  request<Seat>("/api/bookings", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ seatId }),
  });
