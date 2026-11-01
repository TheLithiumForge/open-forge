import { fileURLToPath } from "node:url";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// The UI builds into web/dist, which the eval server serves at http://localhost:4318/.
export default defineConfig({
  root: "web/ui",
  plugins: [react(), tailwindcss()],
  resolve: { alias: { "@": fileURLToPath(new URL("./web/ui", import.meta.url)) } },
  build: { outDir: "../dist", emptyOutDir: true },
});
