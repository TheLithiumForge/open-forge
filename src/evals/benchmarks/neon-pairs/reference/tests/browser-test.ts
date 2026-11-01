import assert from 'node:assert/strict';
import { before, after, test as nodeTest } from 'node:test';
import { mkdir, writeFile } from 'node:fs/promises';
import { basename, join } from 'node:path';
import { chromium } from '@playwright/test';
import { readFile } from 'node:fs/promises';
import { spawnSync } from 'node:child_process';
import { REFERENCE_ROOT, serveArtifact } from './artifact-server.ts';
import type { Browser, BrowserContext, BrowserContextOptions, Page } from '@playwright/test';

const BROWSER_TEMP = join(REFERENCE_ROOT, '.browser-work');
const RESULTS = join(REFERENCE_ROOT, 'test-results');
const CHROMIUM_VERSION = '156.0.8078.4';
const EXECUTABLE_ENV = 'PLAYWRIGHT_CHROMIUM_EXECUTABLE';
let browser: Browser | undefined;
let artifact: Awaited<ReturnType<typeof serveArtifact>> | undefined;

before(async () => {
  await mkdir(BROWSER_TEMP, { recursive: true });
  await mkdir(RESULTS, { recursive: true });
  process.env.TEMP = BROWSER_TEMP;
  process.env.TMP = BROWSER_TEMP;
  const executable = process.env[EXECUTABLE_ENV];
  const npmResult =
    process.platform === 'win32'
      ? spawnSync('cmd.exe', ['/d', '/c', 'npm --version'], { encoding: 'utf8' })
      : spawnSync('npm', ['--version'], { encoding: 'utf8' });
  assert.equal(npmResult.status, 0, 'T03.4 npm version command');
  const npm = npmResult.stdout.trim();
  const playwright: unknown = JSON.parse(
    await readFile(new URL(import.meta.resolve('@playwright/test/package.json')), 'utf8'),
  );
  assert.equal(process.versions.node, '26.12.1', 'T03.4 named Node');
  assert.equal(npm, '11.20.0', 'T03.4 named npm');
  assert.ok(playwright && typeof playwright === 'object' && 'version' in playwright);
  assert.equal(playwright.version, '1.64.1', 'T03.4 named Playwright');
  browser = await chromium.launch({
    headless: true,
    ...(executable ? { executablePath: executable } : {}),
  });
  assert.equal(browser.version(), CHROMIUM_VERSION);
  artifact = await serveArtifact();
  await writeFile(
    join(RESULTS, 'browser-runtime.json'),
    JSON.stringify(
      {
        node: process.versions.node,
        chromium: browser.version(),
        npm,
        playwright: playwright.version,
      },
      null,
      2,
    ),
  );
});

after(async () => {
  await browser?.close();
  await artifact?.close();
});

interface BrowserFixture {
  browser: Browser;
  context: BrowserContext;
  page: Page;
  gameURL: string;
}
interface Artifacts {
  outputPath(file: string): string;
}

export function test(
  name: string,
  body: (fixture: BrowserFixture, artifacts: Artifacts) => Promise<void>,
  options: BrowserContextOptions = {},
): void {
  nodeTest(name, { timeout: 30_000 }, async (t) => {
    if (!browser || !artifact) throw new Error('Browser verification did not start.');
    const context = await browser.newContext(options);
    t.after(() => context.close());
    const page = await context.newPage();
    const pageErrors: string[] = [];
    page.on('pageerror', (error) => pageErrors.push(error.message));
    const directory = join(RESULTS, name.replace(/[^a-z0-9]+/gi, '-'));
    await mkdir(directory, { recursive: true });
    const artifacts = {
      outputPath(file: string): string {
        if (basename(file) !== file) throw new Error('Expected an artifact filename.');
        return join(directory, file);
      },
    };
    try {
      await body({ browser, context, page, gameURL: artifact.url }, artifacts);
    } catch (error) {
      if (process.env.NEON_TEST_FAILURE)
        await writeFile(
          process.env.NEON_TEST_FAILURE,
          JSON.stringify({ name, message: error instanceof Error ? error.message : String(error) }),
        );
      throw error;
    }
    assert.deepEqual(pageErrors, []);
  });
}
