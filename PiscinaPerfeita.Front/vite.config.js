// ============================================================
//  Piscina Perfeita — Vite config
// ============================================================
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],

  resolve: {
    alias: {
      // "@/config"     → src/config/index.js
      // "@/components" → src/components/
      // "@/modules"    → src/modules/
      "@": path.resolve(__dirname, "src"),
    },
  },

  //Avisa para procurar o .env o nivel acima ja que ele nao existe no mesmo nivel do package.json
  envDir: path.resolve(__dirname, "../"),

  server: {
    port: 5173,
    proxy: {
      // Todas as chamadas /api/* são redirecionadas para o backend ASP.NET Core
      "/api": {
        target: "http://localhost:5000",
        changeOrigin: true,
        secure: false,
      },
    },
  },

  build: {
    outDir: "dist",
    sourcemap: true,
  },
});
