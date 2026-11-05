# Test policy

This page explains the verification layers used by Neon Pairs and the kinds of failures each layer can establish.

The repository checks rule transitions, serialized input, source conventions and complete browser journeys. These checks share documented examples but do not all run in the same environment. `npm test` runs Vitest; `npm run test:e2e` builds the current application and diagnostic entries, then uses Node's test runner with Playwright browser support.

## Focused evidence stays near behavior

The core has adjacent tests such as `game.test.ts`, `deck.test.ts`, `snapshot.test.ts` and `replay.test.ts`. Their names make the behavior under discussion easy to locate before reading the whole suite. A feature's rules.ts has a rules.test.ts beside it. Complete interaction journeys remain at the application test level, where focus, storage, browser lifecycle and delivery can be observed together.

Adjacency is useful because a transition and its evidence can be read in one pass. It does not mean every production file in the repository has a one-to-one test file. Some concerns have several focused files: completion, move limits, snapshot selection and replay ordering each get separate attention. Other helpers are exercised through a broader behavior or browser journey.

The architecture specification A10 asks for focused tests near behavior, independent literal generation vectors, transitions checked against stated rules and built HTML checked from disk as well as served. Those are different obligations because no single test environment can establish all of them.

## What Vitest includes

`vitest.config.ts` includes `src/**/*.test.ts` and `scripts/**/*.test.ts`. It does not include the browser files in `tests`, which end in `.e2e.test.ts` and run under the separate command. There is no hidden browser suite behind `npm test`.

Most unit tests use `node:assert/strict` with `test` imported from Vitest. They call pure functions directly. Clock arithmetic receives numerical observations; session transitions receive a session object; preference tests pass a memory or failing store. This makes failures specific: a mismatch boundary can be checked without waiting for a real timeout.

The script tests also run here. `replay.test.ts` launches child processes to exercise command output and failure exits, and the source-audit files read authored files and configuration. The CLI test has a 60-second timeout because child processes can be slow on a busy verifier. That allowance is test scheduling, not a game timing constant.

## Independence for generated data

A generator test that computes its expected deck using the same generator can prove little about compatibility. The repository instead contains a literal example deck for `np1-4-1`, initial recurrence values and independent rejection vectors. Browser support also has `specDeal` in `tests/deal-oracle.ts`, which expresses the shuffle with BigInt arithmetic.

The rejection tests cover both a draw above the accepted limit and a draw exactly at it. That equality case matters because a one-character comparison change would alter the seeded result. Completion tests iterate supported grid sizes and activate every actual pair to prove that each generated board can finish with one move per pair.

The literal fixture and oracle serve different purposes. A literal fixes a public example. An independent implementation can check more combinations but can still share a mistaken interpretation of the written rule. The documented format, literal vectors and independent arithmetic together provide stronger evidence than any one of them alone.

## State transitions and identity

`activateCard` returns the same state object for ignored input: an invalid index, a matched position, the already revealed first card or input during a mismatch. Tests can check both the unchanged values and reference identity. Identity matters because the session controller and memoized presentation use it to avoid unnecessary work.

A successful first activation changes `started` and selection without adding a move. A second distinct activation adds one move and either matches immediately or opens a mismatch. The rule tests can check this without going through pointer events. Browser journeys then establish that actual buttons call that same path.

The cap tests cover continued matching and victory at `MOVES_MAX = 999_999`, and elapsed-time tests cover the total cap. A limit on the number displayed or serialized is not evidence that play should terminate there. The meaningful regression is whether ordinary transitions still work at the boundary.

## Format validation evidence

Snapshot tests check outer keys, numeric bounds, matched-position consistency and each selection form. `snapshot-selection.test.ts` focuses on nested selection invariants. A selected index must name an available position, a mismatch must involve different faces and complete matched pairs must be represented together. The decoder reconstructs its deck from the code before checking these relationships.

Replay tests add ordering and action count constraints. Equal timestamps preserve array order, while logical time advances before each activation. `replay-order.test.ts` checks a same-time third activation blocked by the preceding mismatch. These are rules about accepted actions, not browser task order.

Round trips establish that the serializer and decoder agree on valid states, but rejection cases are necessary because both could otherwise agree on an overly permissive format. The repository's exact-key helpers and validators make those cases inspectable. [Snapshot format](snapshot-format.md) and [Replay format](replay-format.md) provide the product-facing definitions.

## Browser command and fresh output

The end-to-end script is:

```sh
npm run build && npm run build:art && node --test --test-concurrency=1 "tests/*.e2e.test.ts"
```

The production build type checks first. Both build stages precede the test runner with `&&`, so a failure prevents the suite from opening an older output. `build:art` creates the face sheet and the session harness separately from `dist/index.html`.

The browser wrapper in `tests/browser-test.ts` creates a new context per journey and registers its closure with the Node test context. It collects page errors and asserts that none occurred after a successful journey body. It creates an artifact directory from the test name and restricts output paths to a filename. A shared browser and local server are closed in the file-level teardown.

The standard journey timeout is 30 seconds and the long one is 90 seconds. Playwright assertions use a 15-second configured timeout. These values make verification tolerant of busy machines; they do not lengthen the logical mismatch interval, which controlled-clock tests exercise precisely.

## Disk and HTTP evidence

`tests/game-fixture.ts` defines a `file:` URL for the distribution and separate URLs for the two diagnostic builds. `tests/artifact-server.ts` also serves the built HTML on an ephemeral loopback port, accepting only `/`. This is a test server around the finished file, not the Vite development server.

Disk delivery demonstrates that the HTML carries its own code and assets. HTTP-origin journeys establish preference persistence in a browser environment with a stable storage origin for the journey. A result from one mode does not establish the other, because storage behavior on local files varies.

`build-output.e2e.test.ts` reads the HTML and checks for the excluded fetch and module-preload markers. Browser journeys can additionally watch requests. Source inspection, artifact inspection and actual browser behavior contribute different evidence about offline delivery. [Offline delivery](offline-delivery.md) explains why all three are useful.

## Browser interactions that pure tests cannot see

Arrow navigation tests can check the helper's target arithmetic directly, but only the browser can show where focus lands, whether Tab leaves and whether scrolling keeps the focus ring visible. Native click behavior also matters for keyboard activation, context menus and touch scrolling. The card component uses the native click path rather than a separate game action per input method.

Artwork tests render all 128 faces through the real `Card` component in the separate face sheet. Layout journeys cover large boards, visible scrollbars, zoom and touch. Accessibility journeys inspect the Chromium accessibility tree through a complete keyboard interaction. The README explicitly separates this tree evidence from a spoken screen-reader acceptance journey.

These distinctions prevent overclaiming. A passing tree check does not mean a human has listened to every announcement. A passing layout measurement does not mean every browser family has been tested. [Browser support](browser-support.md) and [Accessibility audit](accessibility-audit.md) are the related places for that evidence.

## Time and lifecycle regressions

The clock tests are intentionally layered. `browser-clock.test.ts` checks arithmetic, while `session.test.ts` checks stale generations and paused state. Browser files exercise hidden intervals, idle pause periods, fractional observations and exports between timer callbacks.

`fractional-clock.e2e.test.ts` overrides the observed clock and sends an overdue click in the same browser task. No scheduled timer can run first, so this isolates the controller's synchronize-before-activate behavior. `idle-clock.e2e.test.ts` avoids inspecting the game repeatedly during inactivity, exposing bugs that an incidental export synchronization could otherwise mask.

The session harness makes teardown observable by unmounting the actual game. `timer-lifecycle.e2e.test.ts` instruments timers and visibility listeners, checks that one active timer exists, then checks cleanup after unmount. It also follows an earlier mismatch with a later mismatch in the same deal. This catches mistakes that merely changing the deal generation would not expose.

## Source and runtime records

[Source audit notes](source-audit-notes.md) records the package, compiler, source and naming patterns inspected by the static checks. Browser setup separately writes its observed runtime versions and can enforce the named combination. These records describe different evidence; neither replaces a behavior test.

For a review, the useful account names the changed behavior and the check that observes it. [Code review guide](code-review-guide.md) gives examples of that mapping.

