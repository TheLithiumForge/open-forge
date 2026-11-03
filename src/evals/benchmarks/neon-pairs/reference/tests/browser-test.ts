import assert from 'node:assert/strict';
import { before, after, test as nodeTest } from 'node:test';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { basename, join } from 'node:path';
import { chromium, expect as playwrightExpect } from '@playwright/test';
import { spawnSync } from 'node:child_process';
import { REFERENCE_ROOT, serveArtifact } from './artifact-server.ts';
import type { Browser, BrowserContext, BrowserContextOptions, Page } from '@playwright/test';

const BROWSER_TEMP = join(REFERENCE_ROOT, '.browser-work');
const RESULTS = join(REFERENCE_ROOT, 'test-results');
const CHROMIUM_VERSION = '156.0.8078.4';
const EXECUTABLE_ENV = 'PLAYWRIGHT_CHROMIUM_EXECUTABLE';
// Set to 1 for named-combination evidence. Other supported runtimes run and record their versions.
const NAMED_COMBINATION_ENV = 'NEON_PAIRS_NAMED_COMBINATION';
const NAMED_COMBINATION = {
  node: '26.12.1',
  npm: '11.20.0',
  playwright: '1.64.1',
  chromium: CHROMIUM_VERSION,
};
// Playwright hides scrollbars by default. Visible ones take layout space, as on a Windows desktop.
const HIDE_SCROLLBARS = '--hide-scrollbars';
export const Scrollbars = { hidden: 'hidden', visible: 'visible' } as const;
type ScrollbarMode = (typeof Scrollbars)[keyof typeof Scrollbars];
// A busy verifier runs many browsers at once, so a single wait gets more than Playwright's 5s default.
export const expect = playwrightExpect.configure({ timeout: 15_000 });
// Most journeys fit the standard limit. The accessibility journey and the large boards take the long one.
export const TestTimeouts = { standard: 30_000, long: 90_000 } as const;
// npm names itself and its version to every script it runs, so a browser file needs no npm process of its own.
const NPM_AGENT_ENV = 'npm_config_user_agent';
let browser: Browser | undefined;
let scrollbarBrowser: Promise<Browser> | undefined;
let artifact: Awaited<ReturnType<typeof serveArtifact>> | undefined;

before(async () => {
  await mkdir(BROWSER_TEMP, { recursive: true });
  await mkdir(RESULTS, { recursive: true });
  process.env.TEMP = BROWSER_TEMP;
  process.env.TMP = BROWSER_TEMP;
  const npm = /^npm\/(\S+)/.exec(process.env[NPM_AGENT_ENV] ?? '')?.[1] ?? npmVersion();
  const playwright: unknown = JSON.parse(
    await readFile(new URL(import.meta.resolve('@playwright/test/package.json')), 'utf8'),
  );
  assert.ok(playwright && typeof playwright === 'object' && 'version' in playwright);
  browser = await launch(Scrollbars.hidden);
  const runtime = {
    node: process.versions.node,
    npm,
    playwright: String(playwright.version),
    chromium: browser.version(),
  };
  const named = process.env[NAMED_COMBINATION_ENV] === '1';
  await writeFile(
    join(RESULTS, 'browser-runtime.json'),
    JSON.stringify({ ...runtime, namedCombination: named }, null, 2),
  );
  if (named) assert.deepEqual(runtime, NAMED_COMBINATION, 'T03 named combination');
  artifact = await serveArtifact();
});

after(async () => {
  await browser?.close();
  await (await scrollbarBrowser)?.close();
  await artifact?.close();
});

function launch(scrollbars: ScrollbarMode): Promise<Browser> {
  const executable = process.env[EXECUTABLE_ENV];
  return chromium.launch({
    headless: true,
    ...(executable ? { executablePath: executable } : {}),
    ...(scrollbars === Scrollbars.visible ? { ignoreDefaultArgs: [HIDE_SCROLLBARS] } : {}),
  });
}

interface BrowserFixture {
  browser: Browser;
  context: BrowserContext;
  page: Page;
  gameURL: string;
}
interface Artifacts {
  outputPath(file: string): string;
}

function npmVersion(): string {
  const npmResult =
    process.platform === 'win32'
      ? spawnSync('cmd.exe', ['/d', '/c', 'npm --version'], { encoding: 'utf8' })
      : spawnSync('npm', ['--version'], { encoding: 'utf8' });
  assert.equal(npmResult.status, 0, 'T03 npm version command');
  return npmResult.stdout.trim();
}

export function test(
  name: string,
  body: (fixture: BrowserFixture, artifacts: Artifacts) => Promise<void>,
  options: BrowserContextOptions = {},
  scrollbars: ScrollbarMode = Scrollbars.hidden,
  timeout: number = TestTimeouts.standard,
): void {
  nodeTest(name, { timeout }, async (t) => {
    if (!browser || !artifact) throw new Error('Browser verification did not start.');
    const owner =
      scrollbars === Scrollbars.visible ? await (scrollbarBrowser ??= launch(scrollbars)) : browser;
    const context = await owner.newContext(options);
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
    await body({ browser: owner, context, page, gameURL: artifact.url }, artifacts);
    assert.deepEqual(pageErrors, []);
  });
}
