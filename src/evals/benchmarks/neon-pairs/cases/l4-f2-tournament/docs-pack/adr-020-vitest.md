# ADR 020: Run pure and command tests with Vitest

This record explains the unit runner and the kinds of behavior it verifies without a browser.

## Decision

Use Vitest for focused tests under `src` and `scripts`. The `npm test` command is `vitest run`, and the configuration includes files ending in `.test.ts` in those two trees. The runner is part of the pinned development tools. It does not become code inside the distributed game.

The pure core makes this a useful boundary. Deck generation, activation, logical advancement, snapshot reading and replay can all run in Node. A test can construct a known state, call a function and inspect its result without timing out a real mismatch or clicking through a large board.

## Evidence the runner supports

Deck tests cover composition, literal generation vectors and rejection sampling. Game tests cover the first reveal, second reveal, ignored activation and completion. Snapshot tests challenge exact fields and internal relationships. Replay tests verify ordered logical actions and time boundaries. The session tests cover state replacement, pause and result feedback without needing React to render.

The CLI tests also run here. They invoke the command wrapper with valid and invalid files and check output streams and exit status. That layer has IO, so it is not a pure function test, but it still does not need a browser. Keeping it next to the wrapper makes the command contract easy to review alongside its implementation.

## Assertions and fixtures

The source commonly uses `node:assert/strict` with Vitest's `test` function. Choosing Vitest did not require adopting a large mocking style or rewriting ordinary assertions into a special matcher vocabulary. Tests read reviewed fixtures where literal expected records are more legible than long inline objects.

Fixtures and expected values need to remain independent enough to detect regressions. In particular, a deterministic generator deserves known expected decks, not merely an assertion that two calls produce the same result. The rule tests also exercise no-op identity and bounded counters, which are easy to miss when only the happy path is visible.

## Alternatives

The Node test runner is already used by browser journeys and could also host all unit tests. Vitest provides a familiar focused-test workflow around the TypeScript source and a clear standalone unit command. Using a browser for every assertion would be slower and would blur whether a failure came from a rule or from its UI adapter.

A component-test-heavy strategy would have offered another layer of simulated rendering. The project instead invests in pure behavior tests and actual browser journeys. This keeps the evidence close to the two main architectural boundaries: deterministic state transitions and browser effects.

## Limits and consequences

A passing Vitest run does not show that `dist/index.html` opens from disk, that a focused card stays visible or that native button events behave correctly. Those claims belong to browser tests. Conversely, a browser win cannot exhaust malformed JSON or seed rejection cases as economically as focused tests can.

The app compiler excludes test and fixture files, while the Node compiler checks them. The typecheck command includes both projects plus the end-to-end configuration. That makes a unit test's type errors visible without giving browser production code an ambient Node environment. [ADR 002](adr-002-typescript.md) explains the compiler split.

The runner choice supports rather than replaces the acceptance journeys. [Unit test guide](unit-test-guide.md), [ADR 017](adr-017-colocated-tests.md) and [ADR 018](adr-018-playwright.md) describe the locations and complementary browser evidence. The unit configuration below is intentionally short because test scope, not a large runner configuration, carries most of the design.

## The complete unit configuration

These patterns include the pure game tests and replay-command tests while leaving application journeys to their separate command.

```ts
import { defineConfig } from 'vitest/config';
export default defineConfig({ test: { include: ['src/**/*.test.ts', 'scripts/**/*.test.ts'] } });
```
