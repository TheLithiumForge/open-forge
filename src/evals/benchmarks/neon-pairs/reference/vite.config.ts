import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import { viteSingleFile } from 'vite-plugin-singlefile';

export default defineConfig({
  plugins: [react(), viteSingleFile()],
  build: { target: 'es2023' },
  server: {
    watch: {
      ignored: ['**/.browser-work/**', '**/.playwright-browsers/**', '**/.npm-cache/**'],
    },
  },
});
