# ADR 054: Verify complete player journeys

This record explains why acceptance combines rule evidence with complete input, reproduction and delivery journeys.

## Context

A game can pass isolated assertions and still fail at the boundary between them. A new deal can look correct until an old mismatch callback closes a newly revealed card. A loaded snapshot can look correct until its Resume button disappears with focus still on it. Acceptance needs sequences that cross those boundaries.

The decision is to express acceptance as journeys tied to the existing product requirements. The acceptance specification explicitly says those examples add no rules beyond their defining documents. This record explains why journeys are useful evidence rather than another source of behavior requirements.

## The highest-value sequences

The play journey completes every supported size and checks move accounting, matches, misses and best records. The interruption journey starts or restarts during a mismatch and waits beyond the old interval. The keyboard journey enters the board, learns names, resolves turns, completes, leaves with Tab and starts again.

Reproduction journeys round-trip snapshots, reject malformed drafts and load unfinished progress. They attempt input while paused, Resume and check remaining active time. Delivery opens the freshly built HTML from disk with requests blocked, while persistence uses an HTTP origin. Each sequence joins behaviors that separate unit tests cannot establish by themselves.

## Example: a stale callback

A focused game test can prove that `createGame` returns zero moves and a closed selection. A session test can prove that a mismatched generation is ignored. The browser journey supplies the final connection: perform the mismatch, restart through the actual control, reveal a card and let earlier work become due. The newly revealed card must reflect the current game, not an old callback.

This is not redundant testing of one function at three levels. Each level answers a different question about the same product risk. [Session module](session-module.md), [ADR 013](adr-013-monotonic-clock.md) and [Regression catalogue](regression-catalogue.md) explain the related mechanics.

## Example: accessibility as a sequence

A button can have a correct name in isolation while the whole game is difficult to traverse. The complete 4x4 keyboard journey checks controls, positional names, revealed faces, turn feedback, final summary and exit. Browser accessibility-tree evidence makes those observations reviewable.

A screen reader was not run during the recorded automated verification. Spoken confirmation belongs to user acceptance, as does the real-device long-press context menu. Keeping those boundaries explicit makes the automated evidence credible rather than overstating what browser automation can observe. [Screen reader notes](screen-reader-notes.md) gives the player-facing account.

## Why not only end-to-end tests

Journey tests are expensive places to enumerate malformed JSON or every seed rejection edge. Focused tests isolate those cases faster and report more local failures. Acceptance selects meaningful combinations and user-visible consequences; it does not replace the core test suite.

Likewise, a screenshot at the end of a game cannot establish every transition that led there. Exact clock edges and parser invariants need structured assertions. The strategy uses complementary evidence rather than demanding that one test style prove everything.

## Build provenance and limitations

The browser command type checks and builds current source before journeys begin. Diagnostic entries are built separately, and the wrapper records runtime versions. A passing journey therefore has a concrete artifact and tool context. It still does not establish behavior on every browser, device or unsupported gesture.

[Acceptance](acceptance.md), [ADR 017](adr-017-colocated-tests.md), [ADR 018](adr-018-playwright.md) and [ADR 051](adr-051-test-artifacts.md) are the companion references. The decision keeps acceptance close to the player's actual task while preserving a clear boundary between requirements and examples that demonstrate them.

## The fresh-build browser command

The command sequence ties journeys to current source. A failed game or diagnostic build prevents the browser test process from running.

```json
{
  "scripts": {
    "dev": "vite --host 127.0.0.1",
    "typecheck": "tsc -p tsconfig.app.json && tsc -p tsconfig.node.json && tsc -p tsconfig.e2e.json",
    "test": "vitest run",
    "build": "npm run typecheck && vite build",
    "build:art": "vite build --config vite.art.config.ts && vite build --config vite.session.config.ts",
    "test:e2e": "npm run build && npm run build:art && node --test --test-concurrency=1 \"tests/*.e2e.test.ts\"",
    "replay": "node scripts/replay.ts",
    "format": "prettier --write .",
    "format:check": "prettier --check ."
  }
}
```
