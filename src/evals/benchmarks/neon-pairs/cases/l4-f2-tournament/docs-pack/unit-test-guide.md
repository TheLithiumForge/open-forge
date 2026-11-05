# Unit test guide
This guide describes the small, direct tests used for Neon Pairs game data, parsers, session policy and developer commands.

The tests are ordinary TypeScript modules selected by Vitest. They use explicit inputs and inspect returned values. Most assertions come from `node:assert/strict`, while `test` comes from `vitest`. React and browser fixtures are reserved for behavior that needs a browser. The division is explained in [Test strategy](test-strategy.md).

## Discovery and execution

`vitest.config.ts` includes `src/**/*.test.ts` and `scripts/**/*.test.ts`. `npm test` runs `vitest run`. The browser files under `tests/` are outside that include set; their Node test runner is described in [Browser test guide](browser-test-guide.md). A file called `.e2e.test.ts` is not automatically a Vitest case in this project.

`npm run typecheck` is a separate check across app, Node and e2e compiler projects. Bundling is not the type checker. A successful unit run therefore does not establish that JSX, browser helpers or compiler project boundaries are correct. The release command sequence retains both checks.

Run the tests in a complete writable checkout with their reviewed JSON inputs. Running only fixture-independent cases may help diagnosis, but it cannot produce a complete suite result. [Fixtures guide](fixtures-guide.md) distinguishes inline data from reviewed files.

## A useful test exposes a decision

`game.test.ts` begins with `createGame(EXAMPLE_CODE)`, activates a card and checks the resulting selection, started flag and moves. It then tries invalid indices and repeat activation. This makes the decision boundary visible: a valid first reveal starts play, while ineligible input keeps the existing state.

A representative test can be read without opening React:

```ts
import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, createGame } from './game.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';

test('a repeated first selection preserves the current state', () => {
  const first = activateCard(createGame(EXAMPLE_CODE), 2);
  assert.equal(activateCard(first, 2), first);
  assert.equal(first.moves, 0);
});
```

This example uses the existing style. Identity equality is meaningful here because `activateCard` explicitly returns its input for an ignored activation. Deep equality is appropriate when comparing reconstructed data such as a decoded snapshot. Replacing all identity assertions with structural comparisons would lose that distinction.

The current test titles describe behavior in plain language. Requirement references appear in some assertion messages, which helps trace a failure back to the specification without making the title depend on an ID. The source-convention check also inspects these names.

## Deterministic deck evidence

`game.fixture.ts` supplies the published code `np1-4-1`, its deck, known pairs, the first generator states and literal format constants. The deck is:

```text
6, 7, 0, 5, 2, 2, 0, 5, 3, 7, 1, 6, 4, 4, 1, 3
```

Indices 2 and 6 form a match. Indices 0 and 1 form a miss. Using these facts makes a transition test independent of deck inspection in the browser and easy to verify against the specification.

`deck.test.ts` checks generator states, complete pair counts, supported sizes, maximum seed and frozen board data. Its per-size loops do more than count cards: they also compare generation against `specDeal` from `tests/deal-oracle.ts`. That oracle uses BigInt arithmetic and is kept apart from production deck generation.

An independent algorithm and a literal example serve different purposes. The literal catches a shared misunderstanding of the algorithm; the wider oracle comparisons catch errors that the small example does not reach. The rejection-sampling tests add stored vectors for a draw above the accepted range and a draw exactly at the limit. Ordinary seeds may never exercise either branch during a short run.

The board-code parser tests are strict about grammar. A trailing line break, surrounding space, leading zeros, exponent notation and unsupported sizes are invalid at the core boundary. The Board code UI separately trims surrounding whitespace before calling the parser. A parser test that copied that UI trimming would blur two intentionally different responsibilities.

## State transitions and invariants

The core exposes `createGame`, `activateCard`, `advanceGame`, `isWon` and `faceUpCards`. Tests operate on immutable states and keep earlier states available for comparison. A new reveal should not mutate an earlier game's matched array or selection.

The turn tests cover the three selection branches directly. A match immediately adds two positions to `matched`, closes the selection and costs one move. A miss enters a mismatch with `remainingMs` equal to 800. All activations are ignored while that interval is pending. `advanceGame` handles the common active-time step for elapsed time and remaining mismatch time.

Boundary evidence is especially useful here. Advancing to just before closure, then to closure, distinguishes a real interval from an approximate timer. Invalid time inputs include nonfinite or nonpositive values. Time before the first reveal and time after completion have no effect. The pure tests can assert those facts exactly without scheduling a real timeout.

`completion.test.ts` finishes every supported size. It groups the deterministic deck into face pairs and activates both members of each pair. That use of generated data is appropriate because the question is whether all pairs complete the game, while separate deck tests establish the deal. It would be circular if used as the only proof of shuffle correctness.

## Counter caps remain playable states

`move-cap.test.ts` creates progress near or at the move limit. It checks reaching 999999, matching and completion at the cap, pending-miss closure and continued snapshot export. A cap test that checked only the displayed number could miss a state that refuses every later action.

`elapsed-time.test.ts` similarly checks the elapsed maximum of 359999999ms, import rejection above that maximum and continued play at the cap. The relevant behavior is bounded data with normal transitions, rather than a special end-of-game condition. Best-score storage tests also exercise the move cap, because a value accepted by live play needs consistent persistence handling.

These cases construct states directly for speed and precision. They do not require simulating hundreds of thousands of turns or waiting days. When the constructed state is also used as snapshot input, it still needs to satisfy the format's other invariants. A deliberately invalid fixture can otherwise cause a parser failure before the cap condition is reached.

## Validate one inconsistent fact at a time

`snapshot.test.ts` has a small `snapshot(overrides)` builder that starts with a complete version 1 object. It serializes the baseline and applies explicit overrides. This keeps malformed cases readable while preserving all unrelated fields.

The started-zero-move case needs care. A started game with no moves is valid only with one selected card. Testing a negative elapsed value on a started, closed, zero-move baseline would produce two reasons for rejection. The existing `ONE_REVEALED` baseline avoids that ambiguity and first asserts that the valid form is accepted.

Other cases cover whole matched pairs, duplicate indices, out-of-range selections, selecting a matched position, extra keys, pending misses with equal faces, and insufficient moves for progress. `snapshot-selection.test.ts` isolates selection and matched-position validation so a nested-field failure can be understood separately from outer-object validation.

Serialization tests check that exported fields match `SNAPSHOT_KEYS`, that matched positions are normalized without changing source order, and that fractional active time remains importable after export. A pending miss with a quarter millisecond remaining is serialized as a positive whole-millisecond interval, preserving a valid pending state. The exact browser-clock arithmetic is covered separately.

`saved-format.test.ts` checks UTF-8 byte measurement before parsing. ASCII padding can reach a byte boundary conveniently, while multibyte examples distinguish bytes from JavaScript string length. The snapshot limit is 65536 bytes; replay has its own larger limit. These are product format limits, not new test-only limits.

## Replay order and command behavior

`replay.test.ts` advances the logical timeline before each action. It checks a late first reveal, ignored activations and a mismatch deadline equal to the next action timestamp. `replay-order.test.ts` concentrates on equal-time array order, where reordering actions can change which card remains selected.

The replay state calculation is pure. The CLI tests in `scripts/replay.test.ts` cover a different seam: file arguments, working-directory resolution, stdout, stderr and exit status. They create temporary directories and files, invoke the command, and remove the test directories afterward. A command that computes the correct final state but prints an npm banner on stdout fails its machine-readable contract.

The relative-path test invokes the npm script from a caller folder and checks failure messages against a reviewed message fixture. Its distinction between unreadable files, malformed JSON and rule-invalid data is tested at the command boundary. None of this requires adding replay controls to the browser interface.

## Session and storage fakes are small

`session.test.ts` composes game calls with preferences and feedback. It checks stale generations, imported pause behavior, event progression and live completion. The tests can hold two sessions at once and prove that a callback meant for one cannot alter the other. The browser lifecycle suite then checks actual scheduled work and cleanup.

`storage.test.ts` provides a Map-backed `PreferenceStore` with only `getItem` and `setItem`. It covers separate minima for each size, equal and worse scores, malformed JSON, unsupported sizes and invalid score data. A throwing store verifies exception handling. A null store covers an unavailable adapter.

There is no need to fake the entire browser storage API for those unit cases. The missing getter-access failure is handled by a real browser init script in `browser-environment.e2e.test.ts`. Keeping each fake close to the behavior prevents an elaborate imitation from becoming another implementation to debug.

## Presentation logic without rendering

`card-state.test.ts` tests the text and state returned by `describeCard`. Hidden-card labels identify row, column and closed state. Revealed and matched labels add the known face and state. The file reads a reviewed expectation so changes to production face descriptions do not automatically rewrite the expected names.

`keyboard.test.ts` checks row and column boundaries and modifier handling through `navigationTarget`. The pure result can establish an intended destination, but cannot prove that native focus moves there or that scrolling preserves its outline. Those checks belong to the browser suite.

`face-design.test.ts` checks 128 unique identities through shape, marker and fill, table-derived parts and reviewed geometry. Browser screenshots add rendered evidence. A unit assertion about SVG coordinates and a neutral screenshot about rendered silhouette complement each other; neither is a full human distinguishability review.

## Source checks and follow-through

`source-audit.test.ts` and `source-conventions.test.ts` run in the same Vitest command but inspect source and package data rather than game states. [Test strategy](test-strategy.md) describes their coverage and text-scanning limits.

The strongest unit case usually starts with a small valid state, changes one relevant fact and expects a clearly different result. Browser evidence follows where the behavior crosses into native interaction or effects. [Regression catalogue](regression-catalogue.md) gives examples of those paired investigations.
