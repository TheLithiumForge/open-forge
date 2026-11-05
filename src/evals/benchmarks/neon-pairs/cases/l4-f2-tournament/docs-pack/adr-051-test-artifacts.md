# ADR 051: Retain evidence outside the shipped game

This record explains diagnostic builds, screenshots and runtime records without adding debug controls to the distribution.

## The useful distinction

A green assertion can establish that a measured condition passed, but it cannot show a reviewer what all 128 faces looked like or which browser produced an accessibility-tree record. The project therefore generates reviewable artifacts during verification. Those outputs belong to the test environment, not to a hidden diagnostic mode of the game.

The decision separates three things that are easy to confuse: the playable artifact, diagnostic entry pages and evidence captured from tests. Only the first is the player's game. The others help establish claims about it.

| Output | Purpose |
| --- | --- |
| `dist/index.html` | The offline playable distribution |
| `.art/index.html` | The separately built face-sheet page |
| `.session/index.html` | The separately built session lifecycle harness |
| `test-results/browser-runtime.json` | Actual tool and browser versions used by verification |
| Per-test files under `test-results` | Screenshots and structured journey evidence |

## Why separate entries

The face sheet needs to show all designs at once. The session harness needs lifecycle controls such as unmounting. Neither is a normal player action, so placing them behind a hash in the production entry would expand the shipped behavior and risk exposing diagnostic information.

Separate Vite configurations build those pages with the same React and single-file tooling. They reuse production components where appropriate. An art test explicitly checks that visiting the face sheet with a session-like hash still produces a face sheet and no unmount control. Separation is therefore observable rather than just a folder naming convention.

## What the evidence adds

The art journey captures color, grayscale and neutralized sheets at the actual minimum card size. Mask uniqueness can catch collapsed geometry; screenshots let a person judge legibility and distinguishability. The accessibility journey records tree observations through a complete keyboard game. The runtime record identifies Node, npm, Playwright and Chromium versions.

Those outputs answer different review questions. A screenshot does not establish accessible names. A tree record does not show a clipped focus ring. A runtime JSON file does not prove the journey passed by itself. Their value comes from relating each artifact to the test and claim that produced it. [Fixtures guide](fixtures-guide.md) explains expected-input material, which is another distinct category.

## Ownership and cleanup

`browser-test.ts` gives each test an `outputPath` function rooted in a directory derived from the test name. It accepts a filename rather than an arbitrary path. The wrapper creates fresh contexts, gathers page errors and closes its resources. The local artifact server serves the built HTML, not source transformed on request.

The main development watcher ignores browser temporary folders, browser downloads and npm cache directories. Diagnostic files do not need to trigger the game's development rebuild loop. The production source audit also rejects diagnostic references in shipped authored code.

## Costs and boundaries

Artifacts take disk space and can become stale if detached from the run that created them. The project records concrete output locations and runtime versions. A reviewer can assess the current run without assuming a generated image is automatically authoritative forever.

[ADR 018](adr-018-playwright.md), [ADR 034](adr-034-face-count.md), [ADR 054](adr-054-acceptance-journeys.md) and [Browser test guide](browser-test-guide.md) explain how the evidence fits the validation strategy. The core decision is to make hard-to-summarize observations inspectable while keeping engineering-only entry points out of the player's artifact.

## Runtime provenance written by the wrapper

The wrapper records the versions it observes and optionally checks them against the named compatibility combination.

```ts
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
```
