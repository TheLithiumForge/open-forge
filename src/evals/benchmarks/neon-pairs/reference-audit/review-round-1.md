# Reference Review Corrections

Reference Review Corrections is the work record that connects each first-review finding to its repair and evidence.

For example, M19 now increases the focus outline offset and fails the outline-bounds assertion. Changing padding no longer counts as its result.

Status: corrections implemented. Final execution results are in AUDIT.md. The next independent judgment is pending.

## Judge items

The numbered rows follow the judge's report:

| Item                 | Change                                                                                                                                                           | Evidence                                                                        |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| 1, zoom              | The extension queries the game's loaded tab and sets that tab's zoom.                                                                                            | `reference/tests/zoom.e2e.test.ts`, actual 640px CSS viewport and pixel ratio 2 |
| 2, spoken journey    | The exact NVDA procedure and version record are ready. I17.6 stays open for a person.                                                                            | `screen-reader-journey.md`                                                      |
| 3, runner            | All game unit and static tests use Vitest 5.0.5. Product-rule coverage is kept. The removed second-clock parameter's test is replaced with one shared-time test. | `reference/vitest.config.ts`, `reference/scripts/source-audit.test.ts`          |
| 4, intro             | Secondary intro text is 14px.                                                                                                                                    | `reference/tests/style.e2e.test.ts`                                             |
| 5, matched mark      | The green check is 16px. The browser checks its geometry, path, border and full opacity.                                                                         | `reference/tests/input-contract.e2e.test.ts`                                    |
| 6, best label        | An empty score reads Best: none once. A recorded score uses Best moves.                                                                                          | `reference/tests/control-contract.e2e.test.ts`                                  |
| 7, stats             | Stat values have no aria-label. Their native terms and values are exposed.                                                                                       | `reference/src/game/status/GameStatus.tsx`, control contract test               |
| 8, clock             | One active-time clock feeds elapsed and mismatch time. advanceGame takes one time delta.                                                                         | `reference/src/game/session/use-session-clock.ts`, game and clock tests         |
| 9, feedback          | Feedback and Resume follow the reproduction controls, next to the actions that caused them.                                                                      | `reference/src/game/controls/Reproduction.tsx`, control journeys                |
| 10, focus and labels | Board exposes focusRemembered and keeps its card position itself. Labels sit in their components. Browser expectations use independent labels.                   | Board, Controls, Reproduction and GameStatus components                         |
| 11, constants        | The elapsed cap sits beside advanceGame. The board pattern uses BOARD_VERSION.                                                                                   | `reference/src/game/game.ts`, `reference/src/game/deck.ts`                      |
| 12, draft reset      | Only the uncontrolled board-code input is keyed by deal generation. The stable load button keeps focus. No draft-sync effect is needed.                          | `reference/tests/control-contract.e2e.test.ts`                                  |
| 13, starting code    | Audit records, captures and mutation tooling moved to reference-audit. The game README has only game and development information.                                | Folder separation and updated trace paths                                       |
| 14, weak checks      | Added exact visibility edges, timer cleanup, earlier-turn timing and complete hover checks. Removed the descendant-selector assertion that always returned zero. | Browser environment, timer lifecycle, input contract and keyboard tests         |
| 15, trace            | Replaced unrelated citations. T02.7 names real package and command checks. Every reference path resolves.                                                        | `trace.json`, trace validation                                                  |

## Rules audit

These rows cover every evidence group raised by the rules auditor:

| Clauses                                                                                   | New or corrected evidence                                                                                          |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| I17.1 through I17.5                                                                       | Full tree-checked keyboard journey with independent literal names, roles, states and single result payloads        |
| I17.6                                                                                     | One open person item with the exact NVDA procedure and actual-version record                                       |
| R06.2, R07.11                                                                             | Displayed 1 / 8 pair count and 5999:59 capped clock assertions                                                     |
| R07.8                                                                                     | 599ms after return stays revealed. The next millisecond closes the mismatch                                        |
| R08.4 through R08.6                                                                       | Fresh progress after reload and successful and failed snapshot-load size persistence                               |
| R09.4, R09.5                                                                              | Clear populated matches, selection, moves and elapsed time. Compare seeds after a size change                      |
| R11.1, R11.2, C11.1                                                                       | Invalid drafts keep an already matched pair, one revealed card, moves, elapsed time and the entered draft          |
| R12.1, R12.2                                                                              | Unmount cancels a pending timer and removes the listener. A later mismatch in the same deal keeps its own deadline |
| C05.3                                                                                     | New Game, Restart and snapshot load replace stale board-code drafts                                                |
| C07.3                                                                                     | Trace includes the multibyte UTF-8 test and exact snapshot byte edge                                               |
| C08.4                                                                                     | Export keeps populated progress unchanged. Another second still advances the clock                                 |
| C09.4, C10.1, C10.6                                                                       | Exact loaded notice, Resume presence, board focus while Resume is still in the DOM and later Resume removal        |
| C12.1                                                                                     | Imported completed progress has a visible static banner with final moves and time                                  |
| C13.2, C13.5, I12.2                                                                       | Successful load keeps button focus and resets a previously remembered nonfirst card                                |
| I03.1, I03.2, I04.1                                                                       | Right-click, held touch, uncancelled contextmenu, hover and cancelled-pointer checks                               |
| I13.1                                                                                     | Browser records nearest alignment on both axes                                                                     |
| I14.3                                                                                     | Exact whole hidden-card DOM allows only position, state and the common neutral back                                |
| C14.2                                                                                     | Browser asserts absence of replay buttons                                                                          |
| C01.1, C02.1, C03.1, C04.3, C05.1, C05.2, C06.1, C07.1, C07.2, C08.1, C09.1, C11.3, C11.4 | Expectations use independent literal labels, limits and exact statuses                                             |

The held-touch test sends real touch events through Chromium. It then dispatches the contextmenu event to check that the game leaves it uncancelled. It does not claim to automate an operating-system menu.

## Look audit

These rows cover the look, technology and architecture findings:

| Clauses                    | Change or result                                                                                                               |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| V12.7                      | The complete grayscale PNG is present here. Automated distinct-mask checks stay separate from the next vision judgment         |
| T03.4                      | The browser run asserts actual Node, the npm version command, installed Playwright and running Chromium                        |
| T07.9, A12.1               | Process review clauses. Product tests doesn't show the builder's work history                                                  |
| V07.1, V10.6, V10.7, V10.9 | All 128 designs are checked against independently written ordered parts and colors                                             |
| V03.1                      | A 1280x1400 viewport exercises the 768px height cap                                                                            |
| V16.2                      | All computed properties are compared. Only physical and logical border-color properties can change                             |
| V19.5, V20.4               | Focus bounds intersect the board scroll area with the browser viewport. A native keyboard-entry check catches clipped outlines |
| V20.1                      | Individual text rectangles are checked for overflow and overlap within controls, status and the win summary                    |
| A04.3                      | Unmount checks pending timer cancellation directly, independently of the inactive-callback guard                               |
| T07.3                      | Static command check requires all three compiler projects                                                                      |
| T08.2                      | Offline delivery permits exactly the main HTML request, including file requests                                                |

The protected benchmark checks have separate reported defects. V14.1 and V15.2 use markup heuristics that miss the valid CSS diamond and SVG check. A11.1 counts an unshipped fixture as shipped. T07.5 expects a different browser-runner summary. These check files are outside this packet. The reference provides direct assertions for the affected behavior. The overseer handles those check repairs.

## Repro audit

These rows cover the reproduction and mutation findings:

| Clauses or item       | Change or result                                                                                                                                                                         |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D05.5, D05.7, D06.2   | Valid closed, one-card and mismatch baselines each reject one extra selection key                                                                                                        |
| D06.3                 | Otherwise-valid duplicate matched indices [2,2] are rejected                                                                                                                             |
| D06.6                 | Remaining time accepts 1 and 800 and rejects 1.5                                                                                                                                         |
| D07.1                 | A valid started one-card state rejects a string started flag                                                                                                                             |
| D07.2                 | Valid started progress isolates negative and fractional moves and elapsed time. Values beyond safe integers also exceed the tighter caps, so those rejection reasons overlap by contract |
| D07.3                 | Zero-move mismatch and insufficient moves with whole matched pairs isolate the minimum-move rule                                                                                         |
| D09.4                 | Trace cites ordered equal-time actions and the blocked third activation                                                                                                                  |
| D10.2                 | Valid padded replay JSON passes at 1048576 bytes and fails at 1048577 bytes                                                                                                              |
| R10.2, R10.4          | Better, equal and worse completed scores are checked after each reload                                                                                                                   |
| S01.1                 | Completion and reset run with media, Web Audio and synthesized-speech traps                                                                                                              |
| Mutation declarations | Every mutation lists all directly affected clauses. Multiple effects are allowed and recorded                                                                                            |
| M07                   | Real unmount timer cancellation replaces the duplicated helper-guard mutation                                                                                                            |
| M19                   | Larger outline offset leaves padding unchanged. The kill names the failed full-outline assertion                                                                                         |
| Harness               | A kill needs the declared test name and declared assertion reason. Results include the assertion message and other failed tests. The harness unit test rejects unrelated failures        |

The earlier M19 padding and scroll-clearance candidates survived the focused assertion and were rejected. A programmatic focus check used the browser's default ring. Native Tab entry now exercises the product's keyboard outline before checking its bounds.

Back to [Reference Audit](AUDIT.md).
