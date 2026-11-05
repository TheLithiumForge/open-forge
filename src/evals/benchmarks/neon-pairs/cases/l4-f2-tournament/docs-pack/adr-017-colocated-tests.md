# ADR 017: Keep focused tests beside behavior

This record explains the split between neighboring unit tests and application-level browser journeys.

## Decision

Focused tests sit beside the behavior they exercise. Deck tests live in `src/game` with deck generation, clock tests in `session` with clock behavior, and navigation tests in `board` with keyboard movement. Complete browser journeys live in `tests`. The split follows what the evidence needs to instantiate.

A rule such as “activating the same first card has no effect” does not need a browser. A claim that Tab leaves the board and the remembered focus outline remains visible does. Keeping these tests in different layers makes both the location and the failure more informative. [Test strategy](test-strategy.md) describes the broader distribution.

## Why proximity helps

A developer changing the mismatch transition can find its focused tests without searching an unrelated central suite. A reviewer can read the function and nearby examples together. Colocation also makes small behavior modules less likely to lose their explanatory tests during a refactor: the relevant files occupy the same part of the tree.

The unit configuration includes `src/**/*.test.ts` and `scripts/**/*.test.ts`. That selection permits command-wrapper tests near the replay script while keeping application journeys out of the unit runner. File proximity alone does not determine a test's environment; the runner configuration makes the distinction explicit.

## Independent expectations

Tests are useful when they challenge the implementation's assumptions. Deck checks include literal expected values from the documented generation algorithm. A test that generated an “expected” deck by calling the production generator would only prove self-consistency. The source also includes a separate deal oracle for browser fixtures, making deterministic control possible without exposing hidden cards through application globals.

Selection tests exercise ignored actions, immediate matches and blocked mismatches. Clock tests compare fractional observations and inactive intervals. Snapshot tests validate malformed and inconsistent input rather than only round-tripping one happy state. These examples explain what colocation is meant to preserve: focused behavioral evidence, not a parallel copy of each line of source.

## Browser scope remains separate

The application tests open built HTML and inspect real controls, computed styles, scrolling, accessibility trees and lifecycle. They use documented codes and snapshot controls to reach states. The diagnostic session harness exists for lifecycle operations such as unmounting that normal players do not invoke from a game button.

These tests belong at application level because their assertions span several modules. Moving them next to a leaf component would obscure that they exercise the built entry and browser adapter. Conversely, turning every numeric parser edge into an end-to-end journey would make the suite slower and the failure less local. See [ADR 018](adr-018-playwright.md) and [ADR 020](adr-020-vitest.md).

## Costs and maintenance

The source tree contains tests and fixtures that the app compiler excludes. Build boundaries and source audits therefore matter: a test file being near production code does not make it part of the delivered game. The source audit checks that shipped source has no diagnostic references, and separate Vite entries hold diagnostic pages.

A central unit-test directory was considered simpler to scan by test type, but it detached examples from the functions under review. The current organization favors the engineer opening one behavior directory to understand both implementation and evidence. It is consistent with the existing architecture and does not imply a new filename convention for future feature modules. [Fixtures guide](fixtures-guide.md), [Source audit notes](source-audit-notes.md) and [ADR 051](adr-051-test-artifacts.md) provide the supporting details.

## The focused-test runner boundary

The include patterns explain which colocated tests execute under the unit command. Browser journeys use a separate command.

```ts
import { defineConfig } from 'vitest/config';
export default defineConfig({ test: { include: ['src/**/*.test.ts', 'scripts/**/*.test.ts'] } });
```
