---
open-forge:
  description: Anonymous Stage 1 proposals from the five members of the reference test audit council, each with a verdict per test file, coverage proof, expected effect, risks and an experiment
  tags: [Memory, Analysis, Contextual, Evidence, Evaluation, NeonPairs, Tests]
---

# Test Audit Council Proposals

Five members answered one brief, shuffled under letters A to E with persona names removed, as the reviewers saw them. Lightly condensed from the members' answers.

## Proposal A

### 1. Verdict table

| File | Verdict | Journey | Reason |
|---|---|---|---|
| scripts/replay.test.ts | trim: drop npm from "replay CLI prints a final snapshot and rejects invalid files and arguments", keep npm only in "the replay command reads a relative path from the caller folder and names what failed", 30s timeouts | R1 | only load-sensitive unit file |
| scripts/source-audit.test.ts | keep | A6, D4 | only unit test that catches B4 |
| scripts/source-conventions.test.ts | keep | D4 | |
| card-state, face-design, completion, deck-rejection, deck, elapsed-time, game, move-cap, replay-order, replay, saved-format, snapshot-selection, snapshot, browser-clock, session unit tests | keep | various | snapshot.ts is what B5 changes |
| src/game/board/keyboard.test.ts | keep byte-identical | C1 | B2 and P1 patches delete and restore it |
| storage.test.ts | keep | P6 | catches B1 in the unit run |
| accessibility | keep | C3 | catches B2 |
| all-sizes | trim to the deal test plus sizes 4 and 16 | P1, R1 | sizes 6 to 14 repeat completion.test.ts |
| announcements | keep | P3 | |
| art | trim: drop "records the default board and focused revealed state for visual review" | A2 | screenshots only, no assertion |
| browser-environment | keep | P4, A6 | catches B4 |
| build-output | keep | D1 | |
| card-render | merge into art | A2 | same face sheet |
| clock-edges | trim to "an overdue mismatch resolves before the next click even when its timer is delayed" | P3 | other two repeat elapsed-time and control-contract export |
| control-contract | keep | C1, R4, R5 | catches B1 |
| controls | merge into control-contract | R2 | same labels and limits |
| elapsed-cap | drop | R4 | elapsed-time.test.ts covers it |
| focus-clearance | merge into focus-entry | A3 | same nearest scroll |
| focus-entry | keep | C1 | catches B2 |
| fractional-clock | merge into clock-edges | P3 | |
| idle-clock | drop | P4, R4 | clock and session units plus browser-environment cover it |
| input-contract | trim to "hidden DOM contains only position state and identical neutral backs" | C2 | hover test is style |
| keyboard | keep byte-identical | C1 | B2 and P1 patches |
| layout | trim 16x16 to 360x800 and 1280x800 | A1, A3 | spec names only those two |
| lifecycle | keep | P5 | |
| matched-mark | trim to 16x16 at 360x800 | A3 | |
| motion | keep | A5 | only test that catches B3, also guards normal motion |
| move-cap | drop | P6 | three unit files cover it |
| persistence-contract | keep test 1; merge "snapshot loads save only a valid size and every reload starts fresh" into preferences | P6, D3 | catches B1's worse-score half |
| play | keep | C1, D2 | catches B2 |
| pointer-focus | trim to 360x800 and 1280x800 clicks, 360x800 taps | C1 | |
| preferences | keep | D3 | |
| product-contract | keep | A1 | catches B1 |
| saved-bests | drop | R5 | "a new completion records bests but completed imports never do" covers it |
| scrollbars | trim to 16x16 at 1280x800 | A3 | |
| snapshots | keep | R2, R4, R5 | catches B1 and B2 |
| spacing | trim to 360px | A1 | |
| status-controls | keep | P6, A3 | catches B1 and B2 |
| status | keep | P1 | catches B1 |
| style | keep | A1 | |
| timer-lifecycle | keep | P5 | |
| touch-scroll | keep | C1, A3 | V19 names both viewports |
| zoom | keep | A4 | |

### 2. Coverage proof

P1 completion.test.ts, all-sizes (4, 16), status. P2 game.test.ts, keyboard. P3 game.test.ts, announcements, clock-edges. P4 browser-environment hidden test. P5 lifecycle Restart, timer-lifecycle. P6 persistence-contract better/equal/worse, storage.test.ts. C1 play, keyboard, focus-entry, control-contract, pointer-focus, touch-scroll. C2 input-contract, card-state. C3 accessibility. A1 layout, style, spacing, product-contract. A2 art, face-design. A3 layout, matched-mark, scrollbars, focus-entry. A4 zoom. A5 motion. A6 browser-environment, source-audit. R1 all-sizes deal test, deck.test.ts, both replay files. R2 snapshot.test.ts, control-contract, snapshots. R3 replay.test.ts, replay-order.test.ts. R4 snapshots Resume test. R5 control-contract "export keeps populated progress running and exposes a static imported win". D1 to D4 the commands, build-output, play offline, preferences, source audits.

### 3. Expected effect

Browser tests 93 to 66 in 29 files. The brief's 87 is wrong: the files and every validation.json show 93. The audit's 22 unit files is wrong: there are 20. Time about 664s to 470s under the 2026-11-03 load; a validation run on the verifier takes about 166s, so about 120s there. The unit run keeps all 132 tests, about 6s, without replay timeouts.

Shipped tests each planted bug fails, from validation.json and the sources:

| Task | Before | After |
|---|---|---|
| B1 | 2 unit (storage.test.ts) plus 7 browser: control-contract x2, product-contract, snapshots, status, status-controls, persistence-contract | unchanged |
| B2 | 0 unit (its tests are deleted) plus 13 browser: accessibility, focus-clearance, focus-entry x3, layout x3, play x2, snapshots, status-controls, zoom | 12 |
| B3 | 0 unit plus 1 browser: motion "reduced motion removes card transitions and completion animation" | unchanged |
| B4 | 1 unit (source-audit) plus 1 browser: "keeps the initial focus and produces no audio during play" | unchanged |

These failures are how the agent finds each bug, so keeping them keeps each task as hard as before. Keep every test that catches a planted bug, drop only tests that catch none. The audit's "one browser test per journey" would cut B1's and B2's failure sets and change task difficulty, not just time.

### 4. Risks

The agent's own run stops catching render bugs only at sizes 6 to 14, regressions at 1280x720, 1280x1400 and 640x400, clock wiring (elapsed-cap, idle), and a shown best that changes during an import. The hidden clause checks still catch all of these. Only T07.5's "left the suite red" signal narrows, and none of B1 to B4 touch those areas. That weakens the process-discipline signal for future feature tasks such as F1 more than for these bug fixes. Any reference edit changes the hashes in every validation.json, so every case is validated again and the reference regraded; leaving keyboard files byte-identical avoids regenerating the B2 and P1 patches. Load is still the main problem: B1's failing browser tests took 5.3 to 6.2s on the verifier against Playwright's 5s expect timeout, and T07.5 fails if any one of 66 tests fails, so contention can still cause false regressions. B3's README says its unit tests fail at the start, but validation.json shows T07.4 passing there.

### 5. Experiment

For B1, B2, B3, B4 and P1, run the current and trimmed reference through validate-case ten times each under load matching 2026-11-03 (all check slots busy). Right if the starting failure titles match the counts above, and the solution stage never fails T07.4 or T07.5 on the trimmed reference while it does on the current one.


## Proposal B

### 1. Verdict table

Codes number acceptance sentences in order: J0 intro, then P Play, C Controls, A Appearance, R Reproduction, D Delivery.

| File | Verdict | Reason (journey) |
|---|---|---|
| completion.test.ts | keep, absorb all-sizes | P1; also compare each size's deal with deal-oracle.ts |
| game.test.ts | keep, extend | P2, P3 (see the defect in section 2) |
| deck, deck-rejection, elapsed-time, move-cap, replay, replay-order, saved-format, snapshot, snapshot-selection, card-state, face-design, keyboard, browser-clock, session, storage unit tests | keep | R1, R07 cap, R05 cap, R2, R3, C1, A1, P4, J0, R5, P6 |
| scripts/replay.test.ts | trim | C14: call node directly, keep one npm call, 30s timeout; drop the exact stderr fixture, since C14 asks only for "a message" |
| scripts/source-audit.test.ts | keep | D1, D4, S02 |
| scripts/source-conventions.test.ts | trim | keep export-owner (T06); the other three state no spec rule and fail T07.4 on agent code |
| accessibility | keep, absorb announcements test 1 as a repeated miss | C2 |
| all-sizes | move to unit | P1 says what to finish, not where; its example-deck test repeats deck.test.ts |
| announcements | merge into accessibility | accessibility already announces each load kind twice |
| art | trim: keep the 44px grayscale test with marker and palette checks folded in; drop the harness test and the screenshot-only test | A1 |
| card-render | merge into art | A1, proves the face sheet draws the real card |
| browser-environment | trim: keep P4 and A4, drop storage-denied (storage.test.ts) | P4, A4 |
| build-output | keep | D2 |
| clock-edges | drop | R3 is a replay sentence, held by replay.test.ts |
| control-contract | trim: keep tests 1-2; test 3 repeats snapshots | C1 |
| controls | merge into control-contract | only draft-survives-card and selectable export are new |
| elapsed-cap | drop | elapsed-time.test.ts |
| focus-clearance | merge into layout 360x800 | A2 interior outline |
| focus-entry | trim into keyboard: keep "every keyboard move after a pointer click shows the focus outline" | nearest alignment isn't a journey |
| fractional-clock | drop | snapshot.test.ts |
| idle-clock | merge: hidden case into browser-environment, paused case into snapshots | |
| input-contract | trim: keep hidden-DOM test, move hover to style | C1 |
| keyboard | trim: keep the Tab test | C1; keyboard.test.ts covers modifiers |
| layout | trim: drop 1280x1400 | V19 names only 360x800 and 1280x800 |
| lifecycle | keep | J0, P3, P5, I03 |
| matched-mark | trim: drop 8x8 | A1 |
| motion | keep | A3 |
| move-cap | drop | unit tests |
| persistence-contract | keep, absorb Best: none and Restart checks | P6, D3 |
| play | trim: keep taps, offline and edges; the keyboard game repeats accessibility | C1, D2 |
| pointer-focus | trim: keep 1280x800 click and 360x800 tap; 1280x720 is unstated | C1 |
| preferences | merge into persistence-contract | P6 |
| product-contract | merge: statuses into status, colors into style; whitespace-trim test serves no journey | |
| saved-bests | merge into snapshots, with a saved best | R5 |
| scrollbars | trim to 1280x800 at 16x16 | |
| snapshots | keep | R4, R5 |
| spacing | trim to one viewport; V09 names none | A1 |
| status | keep | J0 |
| status-controls | merge: Enter focus into control-contract, best label into persistence-contract; drop the phone-result test | |
| style | keep | A1 |
| timer-lifecycle | trim: keep unmount | A04, D4 |
| touch-scroll | keep | A2 |
| zoom | keep | A2, V20 |

### 2. Coverage proof

J0 status, lifecycle, session.test.ts. P1 completion.test.ts with the oracle. P3 game.test.ts, lifecycle. P4 browser-environment. P5 lifecycle. P6 persistence-contract. P2 is a defect both now and after any cuts: the journey says "Reveal one card twice, then reveal a different card", but "first reveal starts play without a move and ignores invalid activation" never reveals the different card. Fix: add a reveal of card 6 and assert one move. C1 play, keyboard, keyboard.test.ts, control-contract, input-contract, lifecycle. C2 accessibility. A1 style, spacing, art, matched-mark, face-design.test.ts. A2 layout, touch-scroll, zoom. A3 motion. A4 browser-environment, source-audit.test.ts. R1 deck.test.ts, scripts/replay.test.ts. R2 snapshot.test.ts. R3 "replay resolves exact timeout edges before equal-time ordered activation". R4 and R5 snapshots. D1 and D4 source-audit.test.ts. D2 play, build-output. D3 persistence-contract.

### 3. Expected effect

About 38 browser tests in 22 files, down from 93 in 37 (the run held 87). Roughly 310s under the same load, against 664s. 15 fewer Chromium launches and npm --version spawns. The unit command makes one npm spawn instead of six. The audit has two errors: 20 unit files, not 22, and its "two viewport sizes" rule would cut V20's 640x400 viewport at scale 2, which the spec states.

### 4. Risks

No browser test is left for an overdue timer before a click, or export between render ticks, so a broken clock.synchronize() would pass. Sizes 8 to 14 are never rendered in the built game; the oracle catches deal bugs, not layout bugs there. T07.5 stops detecting agent regressions in storage denial, Board code trim, modifier keys and narrow scrollbars. Without the convention tests, requirement IDs and non-question names get through unless a rubric grades them. Seeded bugs stay detected: B1 by storage.test.ts and persistence-contract, B3 by motion, B4 by browser-environment and source-audit.test.ts. B2's regenerated seed must still delete only the keyboard files, so accessibility and play edges keep failing. accessibility.fixture.json changes with the merged miss.

### 5. Experiment

Run a mutation matrix against the current and proposed suites, five times each under the 34-agent load: the four seeds plus about ten adapter mutants (no clock.synchronize(), rounded export, no visibility listener, no storage catch, no trim, wrapping at edges). Right if every mutant that breaks a journey fails both suites, every mutant caught only by the current suite maps to no acceptance sentence, and timing failures fall.


## Proposal C

### 1. Verdict table

Journey keys: P1 every size, P2 move count, P3 match and 800ms mismatch, P4 hidden page, P5 restart during mismatch, P6 bests after reload; C1 pointer/touch/keyboard focus, C2 hidden DOM, C3 I17 tree; V1 layout and faces, V2 16x16 at both viewports plus zoom, V3 reduced motion and banner, V4 silence; R1 example deck and replay, R2 snapshots and malformed input, R3 timeout before replay action, R4 paused import, R5 completed import; D1 commands, D2 offline file, D3 HTTP preferences, D4 architecture.

| File | Verdict | Reason | Journey |
|---|---|---|---|
| all-sizes.e2e | trim to `the delivered game completes every pair at size 16` with 90s timeout; move oracle into deck.test.ts as `specDeal` equals `generateBoard` for every size | 512 CDP round trips under 30s, worst timeout risk; sizes 4-14 same path | P1 |
| layout.e2e | drop 1280x1400 view, absorb focus-clearance | V19 names only 360x800 and 1280x800 | V2 |
| lifecycle.e2e | keep | fake clock | P5 |
| play.e2e | keep | core journeys | P2, C1, D2 |
| browser-environment.e2e | keep, absorb idle-clock hidden test | fake clock, catches B4 | P4, V4 |
| accessibility.e2e | trim load-event loop (announcements covers it), 60s timeout | one 24s test against 30s limit | C3 |
| control-contract.e2e | absorb controls; drop `export keeps populated progress running and exposes a static imported win` | snapshots covers | C1, R2 |
| announcements.e2e | keep | catches repeated results | C3 |
| focus-entry.e2e | keep | waits a frame after scroll, deterministic | C1 |
| clock-edges.e2e | move `an overdue mismatch resolves before the next click even when its timer is delayed` into fractional-clock using its stubbed `performance.now`, setting time and clicking in one `evaluate` | real 800ms busy wait; under load the 40ms timer closes the mismatch first, so it passes without checking the overdue path | R3 |
| matched-mark.e2e | trim to 16x16 at 360x800 | mark is 16% of card, scale-free geometry | V1 |
| elapsed-cap.e2e | drop | duplicates `a capped imported clock keeps matching, closing and exporting valid progress` and elapsed-time.test.ts | R2 |
| persistence-contract.e2e | keep, absorb preferences | catches B1 | P6, D3 |
| keyboard.e2e | keep | B2 solution restores it | C1 |
| status-controls.e2e | keep | catches B1 via `none` | C1 |
| motion.e2e | keep | only catcher of B3 | V3 |
| spacing.e2e | keep | phone media query | V1 |
| preferences.e2e | merge into persistence-contract | same reload flow | D3 |
| art.e2e | one full-page screenshot cropped with pngjs instead of 128 element screenshots; drop `records the default board and focused revealed state for visual review` | 128 screenshots under 30s; dropped test asserts nothing | V1 |
| idle-clock.e2e | move hidden test, drop paused-import test | snapshots Resume test covers paused time | P4 |
| snapshots.e2e | keep | | R4, R5 |
| input-contract.e2e | keep | waits for hover color | C2 |
| product-contract.e2e | keep | | R2, V1 |
| fractional-clock.e2e | keep | stubbed clock | R3 |
| focus-clearance.e2e | merge into layout | same 16x16 phone setup | V2 |
| move-cap.e2e | drop | move-cap, session and storage unit tests cover the cap | P6 |
| saved-bests.e2e | drop | `a new completion records bests but completed imports never do` covers | R5 |
| status.e2e | keep | | V3 |
| controls.e2e | merge into control-contract | | C1 |
| card-render.e2e | keep | waits getAnimations finished | D4 |
| timer-lifecycle.e2e | keep | | D4, P5 |
| style.e2e | keep | | V1 |
| build-output.e2e | keep | no browser | D2 |
| pointer-focus.e2e | keep clicks at 360x800 and 1280x800 and tap at 360x800; wheel settle needs 10 equal frames | one equal frame can settle mid-scroll under load | C1, V2 |
| scrollbars.e2e | trim to 16x16 at 1280x800 and 6x6 at 360x640 | launches a second Chromium | V2 |
| touch-scroll.e2e | keep | V19 both viewports; waits 10 equal frames | V2 |
| zoom.e2e | keep, drop both full-page screenshots, 60s timeout | screenshots assert nothing | V2 |
| scripts/replay.test.ts | trim: call `runReplay` in process for error cases; keep one `npm run --silent replay` for caller folder with `{ timeout: 60_000 }` | 11 child processes under vitest's 5000ms default | R1, D1 |
| scripts/source-audit.test.ts | keep | file reads, ms | D1, D4, V4 |
| scripts/source-conventions.test.ts | keep | file reads | D4 |
| all 20 unit files under src/game | keep | pure | various |

### 2. Coverage proof

Every journey stays covered: P1 completion.test.ts, deck oracle, all-sizes at 16; P2 play; P3 game.test.ts, timer-lifecycle; P4 browser-environment; P5 lifecycle; P6 persistence-contract; C1 play, keyboard, focus-entry, pointer-focus, control-contract; C2 input-contract; C3 accessibility, announcements; V1 layout, art, matched-mark, spacing, style; V2 layout, touch-scroll, zoom, scrollbars; V3 motion, status; V4 browser-environment; R1 replay units; R2 snapshot.test.ts, control-contract; R3 replay-order and fractional-clock; R4 and R5 snapshots; D1 replay CLI smoke; D2 play offline, build-output; D3 persistence-contract; D4 source-audit, card-render.

### 3. Expected effect

About 60 browser tests instead of 87, about 450s instead of 664s. Every browser file also runs `npm --version` as a child process in its `before` hook in browser-test.ts (36 spawns); reading `npm_config_user_agent` instead saves roughly 30s more, unmeasured. Every remaining test uses a fake or stubbed clock or a polling wait, with explicit timeouts on long tests. The reference has 20 unit files, not the 22 the audit states.

### 4. Risks

Browser-only bugs at sizes 6 to 14 go unnoticed. Without 640x400 at scale 2, no test presses a clipped card under zoom. Longer timeouts hide slowdowns. React wiring bugs for importing at the cap or saved bests on import keep only unit coverage. In-process `runReplay` stops checking npm wiring per error path.

### 5. Experiment

Run current and trimmed suites ten times each on the unchanged reference with 2 CPUs per container and about 30 containers on 12 cores, recording failures and time per test. Then apply each seed.patch once. Right if the trimmed suite never fails on the unchanged reference and still fails on B1, B3 and B4. For B2, whose seed deletes keyboard.e2e, play and accessibility must still fail.


## Proposal D

### 1. Verdict table

Browser files are tests/<name>.e2e.test.ts. M-numbers are mutants in reference-audit/mutations.fixture.json whose declared killer is that file.

| File | Verdict | Reason | Journey |
|---|---|---|---|
| all-sizes | Trim to size 16. Move 'the independent deal reproduces the published example deck' and a per-size specDeal equality into deck.test.ts | completion.test.ts pairs cards using the game's own state.deck, so it can't see a wrong deal; unit oracles only cover 4x4 and two rejection vectors | P1, R1 |
| play | Keep | only offline-from-disk run; taps, edges, M20 | P1, C1, C2, D2 |
| keyboard | Keep | B2 seed deletes this exact file, solution restores it | C1 |
| accessibility | Keep | only I17 tree evidence | C3 |
| announcements | Keep | only browser check of consecutive identical results | C3 |
| lifecycle | Keep | Restart and New Game mid-mismatch, right-click | P3, P5 |
| timer-lifecycle | Keep | M07, stale deadline in the same deal | P3, D4 |
| clock-edges | Keep | holds the real event loop; M23 | P3 |
| elapsed-cap | Merge into clock-edges, starting its capped import at ELAPSED_MAX_MS - 1 | same import-and-play shape | P3 |
| fractional-clock | Keep | only test that drives performance.now alone, the only catcher of a non-monotonic clock (A04) | P3 |
| idle-clock | Keep | M26 and M27 need an idle period with no reads; browser-environment reads while hidden | P4, R4 |
| browser-environment | Keep | throwing localStorage getter, runtime audio (B4) | P4, A6 |
| persistence-contract | Keep | it is the journey | P6, D3 |
| preferences | Trim: move 'malformed stored preferences fall back to a playable default game' into browser-environment | other two hold R10.5 and board-load size | P6 |
| saved-bests | Trim to 'a completed import in 10 moves leaves a saved best of 8 alone' | only killer of M40, since session.test.ts uses empty preferences; session.test.ts also kills M39 | R5 |
| move-cap | Drop | covered by move-cap.test.ts, the session cap-best test, storage cap round trip and persistence-contract reload | P2 |
| snapshots | Trim: drop 'invalid drafts keep the current game and a loaded win changes neither best nor celebration' | each assertion exists in control-contract tests 1 and 3 or product-contract test 1 | R4 |
| control-contract | Keep | literal public strings | C1, R2, R5 |
| controls | Merge into control-contract: multiline selectable export and draft surviving a card click | rest duplicates control-contract | C1 |
| status-controls | Trim: move 'the best label uses one wording before and after a record exists' into status | M42, result placement on a large phone | C1, A1 |
| status | Keep | banner, stopped clock | P1 |
| product-contract | Keep | M24, status fixture | A1, R5 |
| input-contract | Keep | exact hidden DOM, hover changes only the border | C2, A1 |
| style | Keep | product values | A1 |
| layout | Keep all three views | 1280x1400 is the only view where V03's 768px cap applies | A1, A3 |
| spacing | Keep | M43, M44 | A1 |
| scrollbars | Keep | M22 at 10x10, 900x600, a view the spec doesn't name | A1 |
| pointer-focus | Keep | M32 at 360x800, M36 at 640x400 | C1, A3, A4 |
| touch-scroll | Keep | V19 touch at both stated sizes | A3 |
| focus-entry | Keep | M28 | C1, R4 |
| focus-clearance | Keep | M19, M25 | A3 |
| zoom | Keep | V20 | A4 |
| matched-mark | Keep | M34, three card sizes | A2 |
| art | Trim: move 'records the default board and focused revealed state for visual review' into style, which already reveals that card | grayscale face sheet | A2 |
| card-render | Merge into art | same page, A07 | D4 |
| motion | Keep | only catcher of B3 | A5 |
| build-output | Keep | under 1s | D2 |
| scripts/replay.test.ts | Trim: drop test 1's two npm spawns, assert valid.stderr is empty in test 2, 60s timeout | test 2 keeps npm for C14's caller-folder rule (M31) | R1 |
| scripts/source-audit.test.ts | Keep | version pins, audio pattern (B4) | D1, A6 |
| scripts/source-conventions.test.ts | Keep | ms | D4 |
| all 20 unit files under src/game | Keep | each kills declared mutants (M01-M05, M06, M08-M18, M21, M41, M45) | various |

### 2. Coverage proof

Opening journey: play test 1, then lifecycle 'Restart clears a mismatch and rejects earlier timer updates'. P1 all-sizes (16), play (4), completion.test.ts, the new oracle. P2 only game.test.ts 'first reveal starts play without a move and ignores invalid activation'; no browser test does it today, before any cut. P3 lifecycle, timer-lifecycle, clock-edges. P4 browser-environment hidden test, idle-clock. P5 lifecycle tests 2 and 3. P6 'better equal and worse completed scores persist after each reload'. C1 play, keyboard, pointer-focus, control-contract, focus-entry. C2 'hidden DOM contains only position state and identical neutral backs'. C3 accessibility, announcements test 1. A1 layout, style, spacing, scrollbars. A2 'the actual 44px faces keep distinct geometry without color cues'. A3 layout, touch-scroll, focus-clearance. A4 zoom. A5 both motion tests. A6 'keeps the initial focus and produces no audio during play', source-audit. R1 deck.test.ts, scripts/replay.test.ts. R2 snapshot.test.ts, control-contract test 1. R3 'replay resolves exact timeout edges before equal-time ordered activation'. R4 snapshots Resume test. R5 'export keeps populated progress running and exposes a static imported win', saved-bests. D1 the npm commands and source-audit. D2 'loads the built file with the network offline and no external requests'. D3 persistence-contract. D4 source-audit, source-conventions, timer-lifecycle, card-render.

### 3. Expected effect

The files register 93 browser tests (the run reported 87). These cuts leave 78, about 664s to 575s at the same load. Each browser file pays about 4s of start-up (an npm --version spawn and a Chromium launch), so merging the roughly 29 remaining files into about 12 brings the suite to about 520s without losing any assertion.

### 4. Risks

Full games at 6x6 to 14x14 no longer run in a browser; 14x14 renders in no other browser test. M39 rests on one unit test, and the best-doesn't-flicker check runs for one case only. About 520s under load is near the verifier's 600s CandidateLimits.commandSeconds, so T07.5 can still time out. The current audit's proposal would miss V03's 768px cap (needs the third layout view), M22 (900x600 isn't a named view), M36 (640x400 is V20's view, not one of V19's), M40, non-monotonic clocks, the hook mutants M07, M23, M26 and M27 (none arithmetic), and any deal error above 4x4. Among test files the seeds touch only keyboard.e2e.test.ts, which stays unchanged, but every packet still needs revalidation and the reference regrading.

### 5. Experiment

Rerun reference-audit/mutations.ts on the trimmed reference, running the full unit and browser suites per mutant instead of only the declared test. Add three mutants: 'max-height: min(70dvh, 768px)' replaced by '70dvh', a Date.now session clock, and a deal change only above 4x4. Then apply each L4 seed and time five runs under the 34-agent load. The proposal holds if every mutant and seed the full suite catches is still caught and every run finishes under 600s.


## Proposal E

### 1. Verdict table

Browser files:

| File | Verdict | Reason | Journey |
|---|---|---|---|
| all-sizes | move to unit | Oracle test needs no page: in deck.test.ts assert `generateBoard(boardCode(size,1)).deck` equals `specDeal(size,1)` for every size | P1, R1 |
| play | trim: drop 'keeps navigation focus at the board edges and leaves the board with Tab' | edges in keyboard.test.ts, Tab exit in keyboard | J1, C1, D2 |
| lifecycle | trim to 'Restart clears a mismatch and rejects earlier timer updates' | no journey asks for right-click; New Game reset in accessibility | J2, P3, P5 |
| layout | trim to 360x800 and 1280x800, one 16x16 deal per stated size; absorb focus-clearance, touch-scroll phone, scrollbars 1280 visible, control-size loop | | A2 |
| browser-environment | trim: drop 'continues playing when access to browser storage is denied' | storage.test.ts covers denied store | P4, A4 |
| accessibility | keep | only I17 tree | C3 |
| control-contract | trim to test 1, absorb controls | tests 2 and 3 repeat snapshots and accessibility | C1, R2 |
| controls | merge into control-contract | | C1 |
| announcements | drop | session.test.ts and accessibility's repeated loads cover it | C3 |
| focus-entry | drop, move 'every keyboard move after a pointer click shows the focus outline' into keyboard | alignment detail; Resume focus in snapshots | C1 |
| keyboard | trim to 'native Tab remembers board focus and matched cards stay focusable and inactive' | modifiers in keyboard.test.ts | C1 |
| clock-edges | drop | unit game, snapshot, elapsed cover rules | none |
| fractional-clock | drop | snapshot.test.ts export rounding | none |
| elapsed-cap | drop | elapsed-time.test.ts | none |
| idle-clock | drop | repeats hidden-page and Resume | none |
| move-cap | drop | move-cap.test.ts and session.test.ts | none |
| status | drop | play test 1 and accessibility | none |
| status-controls | drop | "Best: none" in control-contract, best 8 in play | none |
| product-contract | drop | repeats snapshots, style, unit formatting | none |
| matched-mark | trim to 16x16 at 360x800 | smallest card, all 128 faces | A1 |
| persistence-contract | trim to test 1, add preferences' size switch | exactly P6 | P6, D3 |
| preferences | merge test 1 into persistence-contract, drop rest | invalid saved data in unit | P6 |
| saved-bests | merge into snapshots: finish board, import completed game, best stays 8 | | R5 |
| snapshots | keep test 1, trim test 2 to the completed import | invalid drafts in control-contract | R4, R5 |
| motion | merge both into one: pulse, then emulateMedia reduced | one finished board instead of two; still fails on B3 seed | A3 |
| spacing | keep | 599px media query differs between viewports | A1 |
| art | trim to 'the actual 44px faces keep distinct geometry without color cues' | palette and geometry in face-design.test.ts; screenshot test asserts nothing | A1 |
| input-contract | trim to 'hidden DOM contains only position state and identical neutral backs' | no journey asks for hover | C2 |
| focus-clearance | merge into layout phone case | | A2 |
| card-render | drop | architecture review item | D4 |
| timer-lifecycle | drop, remove session-harness build from test:e2e | lifecycle covers J2 in browser | none |
| style | keep | 4s for the whole visual table | A1 |
| build-output | keep | under 1s | D2 |
| pointer-focus | trim to pointer at 1280x800 and touch at 360x800 | zoom covers scale-2 view | C1 |
| scrollbars | merge 1280x800 case into layout | | A2 |
| touch-scroll | merge phone case into layout | | A2 |
| zoom | keep | only V20 check | A2 |

Unit files:

| File | Verdict | Reason | Journey |
|---|---|---|---|
| scripts/replay.test.ts | trim: one npm call (relative path), other 9 calls direct `node scripts/replay.ts`, timeout 30_000 | today 6 npm spawns caused the T07.4 timeouts | R1 |
| scripts/source-audit.test.ts | keep | spec rules T02, T04, T06, A01; catches B4 seed | D1, D4, A4 |
| scripts/source-conventions.test.ts | drop | no spec rule; test-name and env-allowlist checks fail honest agent edits | none |
| replay-order | merge into src/game/replay.test.ts | same module | R3 |
| snapshot-selection | merge into snapshot.test.ts as INVALID_SNAPSHOTS entries | stays under 200 lines | R2 |
| card-state, face-design, keyboard, completion, deck-rejection, deck, elapsed-time, game, move-cap, replay, saved-format, browser-clock, session, storage, snapshot | keep | ms each, own the rules; saved-format is the only UTF-8 byte check | various |

### 2. Coverage proof

J1 play 'finishes the literal board by keyboard without inspecting hidden faces'. J2 lifecycle Restart plus session 'session generation rejects earlier callbacks after replacing a game'. P1 completion plus deck oracle. P2 game 'first reveal starts play without a move and ignores invalid activation'. P3 game 'a mismatch blocks all activation and closes together at the exact interval' plus lifecycle. P4 browser-environment hidden test plus browser-clock. P5 lifecycle. P6 persistence-contract plus storage. C1 pointer-focus (pointer and touch on 16x16), play phone taps, keyboard and keyboard.test.ts (rows, edges, Tab exit), control-contract (labels, focus). C2 input-contract plus card-state. C3 accessibility. A1 style, spacing, art, matched-mark, face-design. A2 layout and zoom. A3 motion. A4 browser-environment 'keeps the initial focus and produces no audio during play' plus source-audit. R1 deck, deck-rejection, replay, scripts/replay. R2 snapshot plus control-contract. R3 replay 'replay resolves exact timeout edges before equal-time ordered activation'. R4 snapshots Resume test. R5 trimmed snapshots test 2 plus session. D1 source-audit and the commands. D2 play offline plus build-output. D3 persistence-contract. D4 source-audit, rest human review.

### 3. Expected effect

Browser: 25 tests in 18 files, down from 93 in 37 by source count (the run reported 87). About 220s instead of 664s under the same load. test:e2e loses the session-harness build. Replay unit tests go from 6 npm spawns to 1.

### 4. Risks

Lost browser-glue checks: overdue mismatch with a delayed timer, export reading the clock between render ticks, timer cleanup at unmount, a throwing storage getter, right-click and modifier keys, Board code trimming and Enter focus, status staying in view on a phone 16x16, visible scrollbars at other sizes, touch at 1280x800, the face sheet drifting from the game card. P2's re-click is never done in a browser. Convention drift goes undetected without source-conventions. The merged layout phone case nears the 30s limit under load and could add false regressions. Every seed and solution needs regenerating, B2 most, since keyboard now holds the outline check. Seeds' expected starting failures should still hold (B1 storage and "Best: none", B3 motion, B4 source-audit and silence, B2 arrow journeys) but need revalidating, and the reference regrading.

### 5. Experiment

Apply each seed to the trimmed reference and run 10 times under the 34-agent, 12-core load, then each solution. Pass if every seed still fails `npm test` or `npm run test:e2e` as its regressionExpectations predict, every solution passes 10 of 10 with no T07.4 or T07.5 failures, the browser suite takes 250s or less and no test exceeds 15s. Then add one mutant per dropped browser file and restore a test only where the hidden grader's regression checks miss its mutant.
