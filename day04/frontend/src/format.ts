export const formatPrice = (vnd: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(
    vnd,
  );
