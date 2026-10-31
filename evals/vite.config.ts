import { fileURLToPath } from "node:url";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// The UI builds into ui-dist, which the eval server serves at http://localhost:4318/.
export default defineConfig({
  root: "ui",
  plugins: [react(), tailwindcss()],
  resolve: { alias: { "@": fileURLToPath(new URL("./ui", import.meta.url)) } },
  build: { outDir: "../ui-dist", emptyOutDir: true },
});
