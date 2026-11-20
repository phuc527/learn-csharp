import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { bookSeat, getSeats } from "./api";
import { formatPrice } from "./format";
import SeatMap from "./SeatMap";

export default function App() {
  const queryClient = useQueryClient();
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const seatsQuery = useQuery({ queryKey: ["seats"], queryFn: getSeats });

  const booking = useMutation({
    mutationFn: bookSeat,
    onSuccess: () => setSelectedId(null),
    // Thành công hay lỗi (vd 409 có người đặt trước) đều tải lại sơ đồ mới nhất
    onSettled: () => queryClient.invalidateQueries({ queryKey: ["seats"] }),
  });

  const seats = seatsQuery.data ?? [];
  // Ghế vừa bị người khác đặt mất (409) thì không còn tính là đang chọn
  const selected = seats.find((s) => s.id === selectedId && !s.booked);

  function selectSeat(id: string) {
    booking.reset();
    setSelectedId(id === selectedId ? null : id);
  }

  return (
    <main className="page">
      <header>
        <h1>Seatly</h1>
        <p className="muted">Đêm nhạc Mùa Thu · chọn 1 ghế để đặt</p>
      </header>

      {seatsQuery.isPending && <p className="muted">Đang tải sơ đồ ghế…</p>}

      {seatsQuery.isError && (
        <div className="notice error" role="alert">
          <p>Không tải được sơ đồ ghế: {seatsQuery.error.message}</p>
          <p className="muted">Kiểm tra backend đã chạy chưa.</p>
          <button className="btn" onClick={() => seatsQuery.refetch()}>
            Thử lại
          </button>
        </div>
      )}

      {seatsQuery.isSuccess && seats.length === 0 && (
        <p className="muted">Chưa có ghế nào.</p>
      )}

      {seats.length > 0 && (
        <>
          <SeatMap
            seats={seats}
            selectedId={selected?.id}
            onSelect={selectSeat}
          />

          <section className="summary">
            <div>
              <p className="muted">Ghế đang chọn</p>
              <p className="summary-value">
                {selected
                  ? `${selected.id} · ${formatPrice(selected.price)}`
                  : "Chưa chọn"}
              </p>
            </div>
            <button
              className="btn primary"
              disabled={!selected || booking.isPending}
              onClick={() => selected && booking.mutate(selected.id)}
            >
              {booking.isPending ? "Đang đặt…" : "Đặt ghế"}
            </button>
          </section>

          <div aria-live="polite">
            {booking.isSuccess && (
              <p className="notice success">
                Đã đặt ghế {booking.data.id}. Hẹn gặp bạn ở sự kiện!
              </p>
            )}
            {booking.isError && (
              <p className="notice error" role="alert">
                {booking.error.message}
              </p>
            )}
          </div>
        </>
      )}
    </main>
  );
}
