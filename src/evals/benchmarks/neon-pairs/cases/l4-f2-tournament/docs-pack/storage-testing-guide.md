# Storage testing guide

This guide maps persistence behavior to the project tests and shows how to isolate reads, writes, session updates and browser lifetime.

Start by identifying the boundary under test. A unit store double can prove parsing and failure handling. A session test can prove when a best becomes eligible. A browser reload can prove that the accepted preference value influences a new visit. No one of those observations substitutes for the other two.

## Existing evidence to read first

| Source file | Behavior covered |
| --- | --- |
| `src/game/session/storage.test.ts` | Round trips, per-size minima, invalid input, move cap, denied methods, unrelated keys |
| `src/game/session/session.test.ts` | Completion records and session transitions that carry preferences |
| `tests/persistence-contract.e2e.test.ts` | Reloaded bests and sizes, fresh progress, accepted and rejected loads, malformed saved data |
| `tests/browser-environment.e2e.test.ts` | Continued play when acquiring browser storage throws |

The source filenames here are code references, not additional documentation pages. [Unit test guide](unit-test-guide.md) and [Browser test guide](browser-test-guide.md) describe the broader harness. The distinctions below are specific to persistence.

## Use a served origin for durable-preference journeys

The browser fixture provides `gameURL` from `serveArtifact`, which reads the built HTML and serves it over local HTTP. Its server binds to `127.0.0.1` on a private port. Each test gets a fresh browser context and page, and the harness checks accumulated page errors after the journey.

The fixture helpers also have a direct-disk default URL. A persistence test using `loadFixedBoard` or `loadTimedBoard` passes its served `gameURL` explicitly, as the existing persistence contract tests do. Omitting it can move a test to `file:` delivery, where browser storage behavior is not the same evidence. The technology specification calls out that distinction.

A reload within the same test keeps the same context and origin. A new test starts a fresh context. A new artifact-server run may choose another port. These differences explain many apparently inconsistent storage observations without any change to the preference code.

## A store double for operation boundaries

The unit suite uses a map-backed `PreferenceStore`. For a diagnosis, a recording variant can capture operation order while preserving the same two-method interface:

```ts
const entries = new Map<string, string>();
const calls: Array<readonly [string, string]> = [];
const store: PreferenceStore = {
  getItem(key) {
    calls.push(['read', key]);
    return entries.get(key) ?? null;
  },
  setItem(key, value) {
    calls.push(['write', key]);
    entries.set(key, value);
  },
};
```

The call log identifies the key used by each operation and helps isolate a read failure from a write failure. Seed a valid baseline, invoke the operation under investigation, and compare its result with the captured calls. [Corrupt data policy](corrupt-data-policy.md) covers the separate timing question for rejected text.

## Build invalid fixtures from a valid baseline

A useful baseline is `{ schemaVersion: 1, size: 6, bestMoves: { "4": 8, "6": 20 } }`. Change one condition at a time. Representative groups are:

- Syntax and outer shape: malformed text, null, arrays, primitive values and missing fields.
- Version and selected size: wrong discriminator, string size and unsupported side length.
- Map shape: missing map, null map or array map.
- Entry keys: unsupported size or noncanonical spelling such as `04`.
- Entry values: strings, fractions, negative counts, counts below the pair minimum or above 999999.

The expected reader result for a rejected aggregate is the whole fallback, including selected size 4 and an empty map. Testing only that one bad entry disappears would miss the actual whole-value behavior. Extra top-level fields are a separate acceptance case because the preference reader currently ignores them.

A good boundary pair differs by one unit: 8 versus 7 for a 4x4 completed count, or 999999 versus 1000000 for the upper bound. This gives clearer evidence than a collection of arbitrarily large invalid numbers. [Preferences store](preferences-store.md) describes the code checks behind those pairs.

## Keep score selection separate from storage

`rememberBest` can be tested without a store. Start with records for two sizes, submit a better count for one, and inspect the other. Submit an equal count and a worse count and compare object identity with the original preference value. The current unit test does both identity and content checks.

These checks do not establish that a real card activation submits the right result. For that, the session test completes a known board through `activateSession` and inspects its preferences. The browser persistence contract adds rendering, the effect and reload to that same transition.

## Reload tests need two different assertions

After an eligible completion, assert that the size's best is visible. Then reload on the same served origin and assert it again. This distinguishes in-memory success from a durable save. The persistence contract loops through better, equal and worse results to cover replacement and preservation.

Also inspect exported game progress after reload. The existing test expects empty matches, zero moves, zero elapsed time, a false started flag and a closed selection. A test that checks only the best could accidentally accept automatic game restoration even though the specification starts a fresh deal.

There is no useful assertion that the new board code must differ after every independent page load. Startup creates a fresh deal rather than restoring the old one, but a storage test can establish the intended state through cleared progress and retained preferences without depending on a random coincidence being impossible. New Game's seed handling is a separate behavior.

## Accepted and rejected loads

A valid board-code load updates the selected size. A subsequent invalid board-code load leaves that preference alone. Reload checks the last accepted size. The same principle applies to snapshots: a valid 6x6 snapshot can change the size, while a rejected snapshot containing an extra field does not replace it with another size.

These cases exercise control validation before persistence. Typing into the input alone is not a load. The browser journey activates the actual load button and then observes the result. [Reproduction panel](reproduction-panel.md) explains the visible interaction being exercised.

## Inject failures at the stage being checked

A getter that throws before initialization tests `browserStore`. A store whose `getItem` throws tests `readPreferences`. A readable baseline with a throwing `setItem` tests failed updates. Using only one double that throws everywhere proves the broad fallback but does not distinguish these paths.

For a write-only failure, observe both states: the new best remains in the session, while the browser string is unchanged. A later read gets the earlier persisted state. For a null store, subsequent attempts are no-ops through the captured null rather than fresh browser probes. The current implementation has no retry queue to drain or success flag to wait for.

Physical quota exhaustion and private-window lifecycle are separate environment checks. The test suite does not include a browser-capacity sweep. [Storage quota notes](storage-quota-notes.md) gives measured payload sizes and the evidence boundary; [Private browsing notes](private-browsing-notes.md) explains why a same-session reload does not prove persistence after the private context ends.

## Running checks without confusing artifacts

In a writable working checkout, `npm test` runs the unit and static checks. `npm run test:e2e` type checks and builds current source, builds diagnostic entries and then runs browser journeys against the result. The command does not fall back to an older distribution after a failed build.

The harness writes browser runtime details to `test-results/browser-runtime.json`. The README names the compatibility combination and describes its recorded results. A report from another environment is useful with its actual version record, rather than borrowing the named combination's label.

A concise persistence result identifies the input value, operation exercised, in-memory outcome, stored outcome and whether a same-origin reload occurred. Those observations explain a failure more precisely than “saving works” or “preferences reset,” and make it possible to locate the next check without widening the product's storage contract.
