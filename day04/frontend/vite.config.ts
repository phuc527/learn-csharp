import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    // FE gọi /api/... → Vite chuyển sang BE C# → không cần cấu hình CORS.
    // (Trong Docker thì nginx làm việc này, xem nginx.conf)
    proxy: { "/api": "http://localhost:8080" },
  },
});
