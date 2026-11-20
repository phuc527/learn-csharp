import type { Seat } from "./api";
import { formatPrice } from "./format";

function groupByRow(seats: Seat[]) {
  const rows = new Map<string, Seat[]>();
  for (const seat of seats) {
    rows.set(seat.row, [...(rows.get(seat.row) ?? []), seat]);
  }
  return [...rows];
}

type Props = {
  seats: Seat[];
  selectedId: string | undefined;
  onSelect: (id: string) => void;
};

export default function SeatMap({ seats, selectedId, onSelect }: Props) {
  return (
    <>
      <div className="screen" aria-hidden="true">
        Sân khấu
      </div>

      <fieldset className="seat-map" aria-label="Sơ đồ ghế">
        {groupByRow(seats).map(([row, rowSeats]) => (
          <div className="seat-row" key={row}>
            <span className="row-label">{row}</span>
            {rowSeats.map((seat) => (
              <button
                key={seat.id}
                className="seat"
                disabled={seat.booked}
                aria-pressed={seat.id === selectedId}
                aria-label={`Ghế ${seat.id}, ${formatPrice(seat.price)}${seat.booked ? ", đã đặt" : ""}`}
                onClick={() => onSelect(seat.id)}
              >
                {seat.number}
              </button>
            ))}
          </div>
        ))}
      </fieldset>

      <ul className="legend" aria-label="Chú thích">
        <li>
          <span className="swatch" /> Trống
        </li>
        <li>
          <span className="swatch selected" /> Đang chọn
        </li>
        <li>
          <span className="swatch booked" /> Đã đặt
        </li>
      </ul>
    </>
  );
}
