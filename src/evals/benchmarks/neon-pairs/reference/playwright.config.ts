import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  testMatch: '**/*.e2e.test.ts',
  fullyParallel: true,
  workers: 2,
  forbidOnly: true,
  retries: 0,
  use: { browserName: 'chromium', headless: true, trace: 'retain-on-failure' },
});
