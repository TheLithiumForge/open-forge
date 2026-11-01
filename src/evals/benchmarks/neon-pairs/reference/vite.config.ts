import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import { viteSingleFile } from 'vite-plugin-singlefile';

export default defineConfig({
  plugins: [react(), viteSingleFile()],
  // One inline file has no chunks to preload, so leave out the preload helper and its fetch call.
  build: { target: 'es2023', modulePreload: false },
  server: {
    watch: {
      ignored: ['**/.browser-work/**', '**/.playwright-browsers/**', '**/.npm-cache/**'],
    },
  },
});
