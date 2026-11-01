import assert from 'node:assert/strict';
import { before, after, test as nodeTest } from 'node:test';
import { mkdir, writeFile } from 'node:fs/promises';
import { basename, join } from 'node:path';
import { chromium } from '@playwright/test';
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
        npm: process.env.npm_config_user_agent?.match(/^npm\/([^ ]+)/)?.[1] ?? null,
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
    await body({ browser, context, page, gameURL: artifact.url }, artifacts);
    assert.deepEqual(pageErrors, []);
  });
}
