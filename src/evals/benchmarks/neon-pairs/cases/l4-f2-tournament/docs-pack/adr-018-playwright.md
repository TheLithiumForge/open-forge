# ADR 018: Use Playwright for browser evidence

This record explains the browser automation choice and the limits of its accessibility and device evidence.

## Context

Pure rule tests cannot show whether a focused card is clipped, whether a textarea draft survives a render, or whether a built file tries to fetch another asset. Neon Pairs needs a real browser layer for those questions. Playwright supplies Chromium control, input, browser contexts, clock control and access to browser observations.

The decision is to use Playwright for those capabilities while the actual test wrapper uses Node's test runner. `tests/browser-test.ts` imports `test` from `node:test` and browser facilities from `@playwright/test`. Calling the suite “Playwright tests” describes the automation dependency, not a claim that it is driven by the Playwright test-runner CLI.

## What the wrapper owns

The wrapper launches Chromium, starts a private local server for the built artifact and creates a fresh context per journey. It collects page errors, gives tests an artifact-output function and closes contexts after their use. The file-level cleanup closes browsers and the server.

A second browser can run with visible scrollbars because default headless settings can hide the layout space they occupy. That is a substantive test distinction for a board whose focus outline must fit inside a scroll viewport. [Browser test guide](browser-test-guide.md) explains the setup and the separate contexts.

The command first builds current source, then diagnostic entries, then runs the browser files with test concurrency set to one. A build failure stops that chain. It does not continue against an earlier `dist` just because a file remains there.

## Why browser observations matter

Input tests exercise native buttons and real focus. Layout tests inspect bounding boxes and computed styles. The artwork tests rasterize the actual SVG components at the minimum size. The file-delivery journey blocks networking. These observations cover behavior that a mocked DOM or an isolated component render would approximate poorly.

The browser clock is controlled for timing edges. Visibility tests explicitly drive the browser adapter's visibility input because headless tabs do not necessarily behave like interactive desktop tabs. That limitation is documented in the test rather than concealed behind a test name claiming to have physically switched applications.

## Evidence is bounded

The named compatibility combination is recorded as Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. The wrapper writes the runtime it actually used and can require that named combination through its verification flag. The README records results for that combination; it does not establish results for every browser family.

Accessibility-tree checks cover a complete keyboard journey, names, states and feedback. They are not a record of spoken output from a screen reader. The README explicitly records that a screen reader was not run. Real-device long-press context-menu behavior likewise remains a user-acceptance observation. [Accessibility audit](accessibility-audit.md) keeps those distinctions visible.

## Alternatives and costs

A DOM-only suite would be faster but would miss actual scrolling, native event behavior and final artifact delivery. Manual-only checks would leave exact timer and replacement regressions difficult to reproduce. Browser automation adds downloads, temporary files and cleanup obligations, so it is reserved for claims that need it.

The suite produces targeted screenshots and structured evidence rather than treating a green process exit as the whole record. [ADR 051](adr-051-test-artifacts.md), [ADR 054](adr-054-acceptance-journeys.md) and [Browser support](browser-support.md) describe how those results are used.

## Fresh context and cleanup ownership

Each journey gets an isolated context, a page-error check and a bounded artifact path. This is the application’s wrapper around Playwright.

```ts
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
```
