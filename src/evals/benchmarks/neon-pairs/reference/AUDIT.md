# Neon Pairs Reference Audit

The Neon Pairs Reference Audit is the decision and evidence record for the game candidate built to spec version 2.

For example, a capped imported game still closes a mismatch and matches another pair while every exported elapsed value stays valid.

Status: candidate with 401 mapped clauses. Automated checks pass. I17 is tree-checked. Person confirmation of spoken output and independent final approval are review items.

## Decisions

The build keeps the product rules and changes the candidate where it differed:

| Subject             | Decision and reason                                                                                                                                                                                                                  |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A03 constants       | Removed the gathered rules file. Selection states, generation limits, timing and format versions sit beside the behavior that uses them.                                                                                             |
| R06 completion      | The banner and final announcement include moves and elapsed time.                                                                                                                                                                    |
| V08 and V09         | Body, title and secondary type use the stated sizes. Positive layout spacing uses the stated scale.                                                                                                                                  |
| V03 and V16         | Board padding is 6px. The 3px outline and 2px offset fit inside both scroll edges.                                                                                                                                                   |
| I16 results         | Turn results use an announcement counter and one keyed atomic live region. Clock ticks and mismatch closing keep the existing result unchanged.                                                                                      |
| Fractional time     | Elapsed and mismatch clocks use the same active intervals with separate fractional origins. A new mismatch gets its full interval without losing earlier elapsed fractions.                                                          |
| Delayed timer       | Every non-cancelled activation reaches clock synchronization before the core decides whether it is blocked. A click after an expired mismatch can reveal a card before the delayed timeout runs.                                     |
| Elapsed cap         | The maintainer capped elapsed time at 359999999ms. R07.9 through R07.11 describe the cap, continued play and display. D07.7 rejects larger imports. D09.5 caps replayed elapsed time. D08 timestamps stay nonnegative safe integers. |
| Format consistency  | C08.2 and C10.3 need no elapsed-cap wording change. Snapshot and replay formats stay at version 1. The maintainer kept this amendment in spec version 2. The move-cap decision below resolves R05.1.                                 |
| Tests               | Replaced the earlier runner files with adjacent node:test tests and application browser journeys. Kept the required Vitest and Playwright package pins. Earlier assertions were moved or replaced with stronger cases.               |
| Rejection vector    | Added a literal 16x16 vector calculated independently with BigInt. Its first generator draw is rejected. The application uses exact Number arithmetic within the safe range.                                                         |
| Mutation gap        | The first half-pair mutation survived because other invalid fields hid the missing assertion. A focused otherwise-valid snapshot now rejects a half pair and catches that mutation.                                                  |
| Parser review       | Rejected an earlier trailing-newline regex finding after checking the runtime behavior. Board-code parsing stays strict and its newline rejection case stays tested.                                                                 |
| Local execution     | The maintainer clarified that the trusted reference can run locally. Chromium and npm cache use project folders. No browser download was directed to the C drive.                                                                    |
| Development watcher | Vite ignores browser profiles, mutation copies, browser downloads and npm cache, so temporary verification work doesn't enter its watcher.                                                                                           |

### Move limit

R05.5 through R05.8 stop the move count at 999999 while play, matching and victory continue. D07.8 rejects larger imported move counts. D07.9 applies the same cap to saved best scores.

The maintainer's decision resolves the earlier open endpoint question under R03.1, R05.1, R05.4, C10.3 and D07.2. Snapshot and replay formats stay at version 1. The cap is implemented beside card transitions and shared by snapshot and preference validation.

Both source reviewers passed the cap behavior and focused test design. Executed checks cover reaching the cap, another match and win, mismatch closing, snapshot export and rejection, capped best persistence and equal-best retention. M21 breaks the cap by allowing one extra move and is caught by those tests.

### Screen reader

I17.1 through I17.6 are tree-checked under the maintainer's evidence decision. The [accessibility record](evidence/accessibility-journey.json) holds 22 real Chromium tree captures through the keyboard journey. It records named controls, all closed positions, revealed face names, matched disabled states, polite atomic turn payloads, the final summary, Tab exit and New Game.

A separate DOM observer checks one status change per turn. It supplements the tree captures. Neither check establishes spoken output. The record states screenReader is null and person confirmation of the spoken journey is pending.

Chromium 156.0.8078.4 and Playwright 1.64.1 are recorded in the evidence and project README. A person confirms speech and records the screen reader version as a review item.

The Structure reviewer found that the first tree projection kept Chromium's root URL. SR1 is fixed with a named state-property allowlist and a serialized-record assertion against URLs, drive paths and node identity fields. The regenerated durable record contains none of those fields. No hidden IDs or locations are substituted into the captured product names or text.

### Tool limits

The overseer regenerates the Layer 2 subset pack after integration. Its declaration now has 187 clauses, including the four move-cap clauses. This worktree's materialize CLI has no subset generator.

Manual removal of dist and the worktree-local Chromium folder was previously rejected by automatic approval review as "blocked by policy". That cleanup stays with the overseer under the maintainer's latest direction. No alternate deletion route was attempted. The current build-first browser command rebuilt source and passed all 44 journeys.

Chromium, node_modules and npm cache stay inside this worktree. Temporary verification work also stays local.

## Checks

The named combination is Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. Prettier 3.9.6 and its configuration are pinned. Other runtime combinations have no results here.

The commands ran in the reference folder except the product checks:

| Gate                            | Result                                                        |
| ------------------------------- | ------------------------------------------------------------- |
| Product locked install          | Passed                                                        |
| Product type check              | Passed                                                        |
| Product tests                   | 92 passed, zero failed and zero skipped                       |
| Reference locked install        | Passed from the lockfile                                      |
| Reference type check            | All three compiler projects passed                            |
| Reference unit and static tests | 115 passed, zero failed and zero skipped                      |
| Build                           | Type check and single HTML build passed                       |
| Build-first browser command     | 44 passed, zero failed and zero skipped                       |
| Mutation script                 | 21 caught, zero surviving material mutations                  |
| Format check                    | Passed                                                        |
| Development command             | HTTP 200 with the application entry. Server stopped afterward |

The browser suite covers every board size through completion, file delivery with network blocked, served preferences, denied storage, paused imports, exact mismatch edges, delayed callbacks, touch gestures at both stated viewports and actual 200% page zoom. The zoom fixture uses Chrome's page zoom API and checks the resulting 640px CSS viewport and pixel ratio of 2.

The lexical source checks supplement type checking and source review. They don't replace a parser or the council's structure review.

The [verification record](verification.json) holds the commands, counts and limits. The [candidate record](seed-record.json) binds source, configuration, documents and the built artifact to file hashes.

## Reviews

The Core reviewer and Structure reviewer read independently and changed no files. Their verdicts are source reviews unless a rendered artifact is named. They read tests but didn't run them. Runtime results above come from the builder's execution.

The planned review clauses have these verdicts:

| Clauses                       | Core reviewer    | Structure reviewer | Reason                                                                                                                                                                                                                                   |
| ----------------------------- | ---------------- | ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A01.1 through A03.2           | Pass             | Pass               | Pure direct inputs, readonly board data, frozen deck and exhaustive selection branches. Versions and limits sit beside their behavior.                                                                                                   |
| A04.1 through A06.3           | Pass             | Pass               | Session hooks handle monotonic time, visibility, cleanup and generation checks. Storage validates unknown data and catches failures. Completed imports bypass best updates and celebration.                                              |
| A07.1 through A09.4           | Pass             | Pass               | Game composes the four parts. Props and styles are local. Hidden identity stays out of DOM data. Board focus stays separate from activation.                                                                                             |
| A10.1 through A12.3           | Pass             | Pass               | Focused tests are adjacent. Browser journeys are at application scope. Literal vectors are independent. Diagnostics use a separate entry. Shared support has actual consumers. Historical edit order isn't established by source review. |
| T03.5 and T03.6               | Record completed | Record completed   | README names the tested combination and separates other runtimes. This audit gives actual commands and counts. The reviewers requested replacement of the old blocked-state audit.                                                       |
| T05.1 through T05.4           | Pass             | Pass               | Exact readonly data, narrowing at input and named discriminants. No unchecked casts, any types or suppressed diagnostics found.                                                                                                          |
| T06.1, T06.2, T06.4 and T06.5 | Pass             | Pass               | Focused modules, direct imports and adjacent tests. Files above the 100-line aim are listed under Size. Every authored TypeScript file is within 200 lines.                                                                              |
| T10.1 through T10.3           | Pass             | Pass               | Only React and React DOM are runtime dependencies. Services, external assets, accounts, analytics and API keys are absent. Install networking is separate from offline play.                                                             |
| V11.1, V11.2 and V12.7        | Pass             | Pass               | Both viewed the original-resolution grayscale sheet. All 128 identities are distinguishable at 44px. Shape, marker and fill keep their distinctions without hue.                                                                         |
| I17.1 through I17.6           | Tree-checked     | Tree-checked       | Real Chromium tree captures and one status change per turn. Person confirmation of spoken output is a review item.                                                                                                                       |

Both visual reviewers checked solid diagonal-marker squares 9, 25, 41 and 57 and outline squares 73, 89, 105 and 121. Their markers stay separate from the square. The reviewed sheet is in test-results/the-actual-44px-faces-keep-distinct-geometry-without-color-cues/face-sheet-grayscale.png.

Both reviewers also passed command wiring, inline distribution structure, storage failure handling, native context menus, hover behavior, spacing, card states, reduced motion and pinch-zoom settings by source review. These verdicts support the executable checks and don't replace runtime checks.

## Clauses

The [requirements map](../requirements.json) has 97 IDs and 401 clauses. The [trace map](../trace.json) has one reference array for every clause. Existing checks arrays stay untouched. Each reference gives a code path and symbol, test selection or review note.

Pass means the stated behavior has executed assertions or the recorded source and visual verdicts. Tree-checked rows cover automated accessibility data and keep spoken confirmation as a person review item. Each row below gives its first code and check reference. The trace gives the complete evidence list.

| Clause | Result                      | Code and check                                                                 |
| ------ | --------------------------- | ------------------------------------------------------------------------------ |
| R01.1  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| R01.2  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| R01.3  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| R01.4  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| R02.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R02.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R02.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R03.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R03.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R03.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R04.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R04.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R04.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R04.4  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R04.5  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R05.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R05.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R05.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R05.4  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R05.5  | Pass                        | `src/game/game.ts` and `src/game/move-cap.test.ts`                             |
| R05.6  | Pass                        | `src/game/game.ts` and `src/game/move-cap.test.ts`                             |
| R05.7  | Pass                        | `src/game/game.ts` and `src/game/move-cap.test.ts`                             |
| R05.8  | Pass                        | `src/game/game.ts` and `src/game/move-cap.test.ts`                             |
| R06.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R06.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R06.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R06.4  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R06.5  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R06.6  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.1  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.2  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.3  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.4  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.5  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.6  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.7  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.8  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.9  | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.10 | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R07.11 | Pass                        | `src/game/game.ts` and `src/game/game.test.ts`                                 |
| R08.1  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R08.2  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R08.3  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R08.4  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R08.5  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R08.6  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R09.1  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R09.2  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R09.3  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R09.4  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R09.5  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R09.6  | Pass                        | `src/game/session/use-game.ts` and `tests/lifecycle.e2e.test.ts`               |
| R10.1  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.2  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.3  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.4  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.5  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.6  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R10.7  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R11.1  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R11.2  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R11.3  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R11.4  | Pass                        | `src/game/session/storage.ts` and `src/game/session/storage.test.ts`           |
| R12.1  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| R12.2  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| C01.1  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C01.2  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C01.3  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C02.1  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C02.2  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C02.3  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C03.1  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C03.2  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C03.3  | Pass                        | `src/game/controls/Controls.tsx` and `tests/controls.e2e.test.ts`              |
| C04.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C04.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C04.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C05.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C05.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C05.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C05.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C06.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C06.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C07.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C07.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C07.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C08.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C08.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C08.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C08.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C08.5  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/controls.e2e.test.ts`          |
| C09.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C09.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C09.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C09.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.5  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.6  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C10.7  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C11.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C11.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C11.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C11.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C12.1  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C12.2  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C12.3  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C12.4  | Pass                        | `src/game/controls/Reproduction.tsx` and `tests/snapshots.e2e.test.ts`         |
| C13.1  | Pass                        | `src/game/Game.tsx` and `tests/controls.e2e.test.ts`                           |
| C13.2  | Pass                        | `src/game/Game.tsx` and `tests/controls.e2e.test.ts`                           |
| C13.3  | Pass                        | `src/game/Game.tsx` and `tests/controls.e2e.test.ts`                           |
| C13.4  | Pass                        | `src/game/Game.tsx` and `tests/controls.e2e.test.ts`                           |
| C13.5  | Pass                        | `src/game/Game.tsx` and `tests/controls.e2e.test.ts`                           |
| C14.1  | Pass                        | `scripts/replay-cli.ts` and `scripts/replay.test.ts`                           |
| C14.2  | Pass                        | `scripts/replay-cli.ts` and `scripts/replay.test.ts`                           |
| C14.3  | Pass                        | `scripts/replay-cli.ts` and `scripts/replay.test.ts`                           |
| I01.1  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| I01.2  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| I02.1  | Pass                        | `src/game/board/Card.tsx` and `tests/keyboard.e2e.test.ts`                     |
| I03.1  | Pass                        | `src/game/board/Card.tsx` and `tests/lifecycle.e2e.test.ts`                    |
| I03.2  | Pass                        | `src/game/board/Card.tsx` and `tests/lifecycle.e2e.test.ts`                    |
| I04.1  | Pass                        | `src/game/board/Card.tsx` and `tests/lifecycle.e2e.test.ts`                    |
| I04.2  | Pass                        | `src/game/board/Card.tsx` and `tests/lifecycle.e2e.test.ts`                    |
| I05.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I05.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I05.3  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.3  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.4  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.5  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.6  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.7  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.8  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I06.9  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I07.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I08.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I08.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I09.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I09.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I09.3  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I10.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I10.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I11.1  | Pass                        | `src/game/session/use-game.ts` and `tests/browser-environment.e2e.test.ts`     |
| I12.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I12.2  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I13.1  | Pass                        | `src/game/board/keyboard.ts` and `src/game/board/keyboard.test.ts`             |
| I14.1  | Pass                        | `src/game/board/Card.tsx` and `src/game/board/card-state.test.ts`              |
| I14.2  | Pass                        | `src/game/board/Card.tsx` and `src/game/board/card-state.test.ts`              |
| I14.3  | Pass                        | `src/game/board/Card.tsx` and `src/game/board/card-state.test.ts`              |
| I15.1  | Pass                        | `src/game/board/Card.tsx` and `src/game/board/card-state.test.ts`              |
| I15.2  | Pass                        | `src/game/board/Card.tsx` and `src/game/board/card-state.test.ts`              |
| I16.1  | Pass                        | `src/game/session/session.ts` and `src/game/session/session.test.ts`           |
| I16.2  | Pass                        | `src/game/session/session.ts` and `src/game/session/session.test.ts`           |
| I16.3  | Pass                        | `src/game/session/session.ts` and `src/game/session/session.test.ts`           |
| I16.4  | Pass                        | `src/game/session/session.ts` and `src/game/session/session.test.ts`           |
| I16.5  | Pass                        | `src/game/session/session.ts` and `src/game/session/session.test.ts`           |
| I17.1  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| I17.2  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| I17.3  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| I17.4  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| I17.5  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| I17.6  | Tree-checked, spoken review | `tests/accessibility-tree.ts` and `tests/accessibility.e2e.test.ts`            |
| V01.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V01.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V01.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V01.4  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V02.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V02.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V02.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V03.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V03.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V03.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V04.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V04.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V04.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V04.4  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V05.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V05.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.4  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.5  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.6  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.7  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V06.8  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V07.1  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V08.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V08.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V08.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V08.4  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V09.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V09.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V10.1  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.2  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.3  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.4  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.5  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.6  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.7  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.8  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V10.9  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V11.1  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V11.2  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.1  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.2  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.3  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.4  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.5  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.6  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V12.7  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V13.1  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V13.2  | Pass                        | `src/game/board/face-design.ts` and `src/game/board/face-design.test.ts`       |
| V14.1  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V14.2  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V15.1  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V15.2  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V15.3  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V16.1  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V16.2  | Pass                        | `src/game/board/Card.tsx` and `tests/play.e2e.test.ts`                         |
| V17.1  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V17.2  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V18.1  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V18.2  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V18.3  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V18.4  | Pass                        | `src/game/board/board.css` and `tests/motion.e2e.test.ts`                      |
| V19.1  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V19.2  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V19.3  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V19.4  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V19.5  | Pass                        | `src/game/board/board.css` and `tests/layout.e2e.test.ts`                      |
| V20.1  | Pass                        | `src/game/board/board.css` and `tests/zoom.e2e.test.ts`                        |
| V20.2  | Pass                        | `src/game/board/board.css` and `tests/zoom.e2e.test.ts`                        |
| V20.3  | Pass                        | `src/game/board/board.css` and `tests/zoom.e2e.test.ts`                        |
| V20.4  | Pass                        | `src/game/board/board.css` and `tests/zoom.e2e.test.ts`                        |
| V20.5  | Pass                        | `src/game/board/board.css` and `tests/zoom.e2e.test.ts`                        |
| S01.1  | Pass                        | `src/game/Game.tsx` and `scripts/source-audit.test.ts`                         |
| S02.1  | Pass                        | `src/game/Game.tsx` and `scripts/source-audit.test.ts`                         |
| S02.2  | Pass                        | `src/game/Game.tsx` and `scripts/source-audit.test.ts`                         |
| S02.3  | Pass                        | `src/game/Game.tsx` and `scripts/source-audit.test.ts`                         |
| T01.1  | Pass                        | `src/main.tsx` and `scripts/source-audit.test.ts`                              |
| T01.2  | Pass                        | `src/main.tsx` and `scripts/source-audit.test.ts`                              |
| T01.3  | Pass                        | `src/main.tsx` and `scripts/source-audit.test.ts`                              |
| T02.1  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.2  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.3  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.4  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.5  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.6  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.7  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.8  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.9  | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T02.10 | Pass                        | `` and `scripts/source-audit.test.ts`                                          |
| T03.1  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T03.2  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T03.3  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T03.4  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T03.5  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T03.6  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T04.1  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T04.2  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T05.1  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T05.2  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T05.3  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T05.4  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T06.1  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T06.2  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T06.3  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T06.4  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T06.5  | Pass                        | `src/game/game.types.ts` and `scripts/source-audit.test.ts`                    |
| T07.1  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.2  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.3  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.4  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.5  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.6  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.7  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.8  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T07.9  | Pass                        | `vite.config.ts` and `tests/browser-test.ts`                                   |
| T08.1  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T08.2  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T08.3  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T08.4  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T09.1  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T09.2  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T10.1  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T10.2  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T10.3  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| T10.4  | Pass                        | `vite.config.ts` and `tests/play.e2e.test.ts`                                  |
| A01.1  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A01.2  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A01.3  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A02.1  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A02.2  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A02.3  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A02.4  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A03.1  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A03.2  | Pass                        | `src/game/game.ts` and `scripts/source-audit.test.ts`                          |
| A04.1  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A04.2  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A04.3  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A04.4  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A04.5  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.1  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.2  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.3  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.4  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.5  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A05.6  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A06.1  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A06.2  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A06.3  | Pass                        | `src/game/session/use-session-clock.ts` and `src/game/session/session.test.ts` |
| A07.1  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A07.2  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A07.3  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A08.1  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A08.2  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A08.3  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A08.4  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A09.1  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A09.2  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A09.3  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A09.4  | Pass                        | `src/game/Game.tsx` and `tests/play.e2e.test.ts`                               |
| A10.1  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A10.2  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A10.3  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A10.4  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A10.5  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A11.1  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A11.2  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A11.3  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A12.1  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A12.2  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| A12.3  | Pass                        | `src/game/game.fixture.ts` and `scripts/source-audit.test.ts`                  |
| D01.1  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D01.2  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D01.3  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D01.4  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D02.1  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D02.2  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D02.3  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D02.4  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D03.1  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D03.2  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D03.3  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.1  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.2  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.3  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.4  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.5  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D04.6  | Pass                        | `src/game/deck.ts` and `src/game/deck.test.ts`                                 |
| D05.1  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.2  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.3  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.4  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.5  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.6  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D05.7  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.1  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.2  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.3  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.4  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.5  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.6  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D06.7  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.1  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.2  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.3  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.4  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.5  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.6  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.7  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D07.8  | Pass                        | `src/game/snapshot.ts` and `src/game/move-cap.test.ts`                         |
| D07.9  | Pass                        | `src/game/session/storage.ts` and `src/game/move-cap.test.ts`                  |
| D08.1  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.2  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.3  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.4  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.5  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.6  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D08.7  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.1  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.2  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.3  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.4  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.5  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.6  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D09.7  | Pass                        | `src/game/replay.ts` and `src/game/replay.test.ts`                             |
| D10.1  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.2  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.3  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.4  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.5  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.6  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.7  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.8  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.9  | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |
| D10.10 | Pass                        | `src/game/snapshot.ts` and `src/game/snapshot.test.ts`                         |

## Mutations

The [mutation result](mutation-results.json) records selected test counts, failed assertions and exit status for every change. The patches live in scripts/mutations.fixture.json. The runner copies inputs into isolated worktree folders, changes one rule, runs its target check and removes that copy. Browser mutations must build before their tests. The clean unit and selected browser baselines pass first.

| Mutation | Clause | Broken rule                                  | Result |
| -------- | ------ | -------------------------------------------- | ------ |
| M01      | R03.2  | Matching comparison                          | Caught |
| M02      | R05.1  | Second reveal adds two moves                 | Caught |
| M03      | R07.1  | First reveal does not start time             | Caught |
| M04      | R04.3  | Pending mismatch accepts input               | Caught |
| M05      | R04.1  | Close one millisecond late                   | Caught |
| M06      | R07.9  | Elapsed time exceeds its cap                 | Caught |
| M07      | R12.2  | Old generation changes an active replacement | Caught |
| M08      | C10.4  | Paused import accepts input                  | Caught |
| M09      | R10.1  | Best scores use the wrong size               | Caught |
| M10      | R10.4  | A worse score replaces a better one          | Caught |
| M11      | R10.3  | Preference writes are skipped                | Caught |
| M12      | D03.2  | PRNG multiplier changes                      | Caught |
| M13      | D01.2  | Maximum valid seed is rejected               | Caught |
| M14      | D06.2  | Snapshot unknown keys are accepted           | Caught |
| M15      | D06.3  | A half-pair snapshot is accepted             | Caught |
| M16      | D09.2  | Replay activates before advancing time       | Caught |
| M17      | D08.7  | Equal-time replay actions are reordered      | Caught |
| M18      | I07.1  | Arrow movement crosses a row edge            | Caught |
| M19      | V19.5  | Board padding clips the focus ring           | Caught |
| M20      | I14.2  | Closed cards mount face artwork              | Caught |
| M21      | R05.5  | Move count exceeds the cap by one            | Caught |

M15 uses snapshot-pairs.test.ts after the first surviving result. M21 adds the new move-cap regression to the original 20 rules. M17 uses replay-order.test.ts to check equal-time order directly. No equivalent mutation was counted as caught. Material survivors must be resolved before seed eligibility.

## Size

Counts use the authored files after Prettier 3.9.6. Physical lines include blank lines. Nonblank lines exclude them. Components include their local presentation functions and the separate face-sheet entry. Tests include their fixtures and browser support.

| Tier               | Files | Physical lines | Nonblank lines |
| ------------------ | ----- | -------------- | -------------- |
| Game functions     | 9     | 366            | 340            |
| Browser session    | 7     | 369            | 337            |
| Components         | 15    | 586            | 564            |
| Styles             | 5     | 302            | 302            |
| Tests and fixtures | 55    | 2782           | 2664           |
| Scripts            | 4     | 183            | 174            |
| Configuration      | 12    | 162            | 160            |

The authored TypeScript maximum is 143 lines. Files above the 100-line aim are tests/accessibility.e2e.test.ts at 112 lines, scripts/mutations.ts at 143 lines, src/game/snapshot.test.ts at 111 lines. No authored TypeScript file exceeds the 200-line limit.

Counts exclude Markdown, the generated dependency lock, output HTML, recorded result JSON including accessibility captures, dependency files, downloaded browsers, caches and temporary verification folders. The [spec notes](../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/spec-notes.md) record 6022 full-pack words and 1489 player-subset words after the cap amendment.

## Files

This inventory compares the candidate with the pilot reference at packet start. Deleted tests were moved or replaced as described under Decisions. Paths below are relative to this folder.

| Change  | Path                                                                                             |
| ------- | ------------------------------------------------------------------------------------------------ |
| Created | [.gitignore](.gitignore)                                                                         |
| Created | [.prettierignore](.prettierignore)                                                               |
| Created | [.prettierrc.json](.prettierrc.json)                                                             |
| Changed | [art/face-sheet.css](art/face-sheet.css)                                                         |
| Changed | [art/face-sheet.tsx](art/face-sheet.tsx)                                                         |
| Changed | [art/index.html](art/index.html)                                                                 |
| Created | [AUDIT.md](AUDIT.md)                                                                             |
| Created | [evidence/accessibility-journey.json](evidence/accessibility-journey.json)                       |
| Changed | [index.html](index.html)                                                                         |
| Created | [mutation-results.json](mutation-results.json)                                                   |
| Changed | [package.json](package.json)                                                                     |
| Changed | [package-lock.json](package-lock.json)                                                           |
| Deleted | `playwright.config.ts`                                                                           |
| Created | [README.md](README.md)                                                                           |
| Deleted | `scripts/__tests__/replay.integration.test.ts`                                                   |
| Created | [scripts/mutations.fixture.json](scripts/mutations.fixture.json)                                 |
| Created | [scripts/mutations.ts](scripts/mutations.ts)                                                     |
| Created | [scripts/package.fixture.json](scripts/package.fixture.json)                                     |
| Created | [scripts/replay.test.ts](scripts/replay.test.ts)                                                 |
| Changed | [scripts/replay.ts](scripts/replay.ts)                                                           |
| Created | [scripts/replay-cli.ts](scripts/replay-cli.ts)                                                   |
| Created | [scripts/source-audit.test.ts](scripts/source-audit.test.ts)                                     |
| Created | [scripts/source-files.ts](scripts/source-files.ts)                                               |
| Created | [seed-record.json](seed-record.json)                                                             |
| Deleted | `src/game/__tests__/deck.test.ts`                                                                |
| Deleted | `src/game/__tests__/game.test.ts`                                                                |
| Deleted | `src/game/__tests__/game-fixture.ts`                                                             |
| Deleted | `src/game/__tests__/replay.test.ts`                                                              |
| Deleted | `src/game/__tests__/snapshot.test.ts`                                                            |
| Deleted | `src/game/board/__tests__/face-design.test.ts`                                                   |
| Deleted | `src/game/board/__tests__/keyboard.test.ts`                                                      |
| Changed | [src/game/board/board.css](src/game/board/board.css)                                             |
| Changed | [src/game/board/Board.tsx](src/game/board/Board.tsx)                                             |
| Changed | [src/game/board/Card.tsx](src/game/board/Card.tsx)                                               |
| Created | [src/game/board/card-state.fixture.json](src/game/board/card-state.fixture.json)                 |
| Created | [src/game/board/card-state.test.ts](src/game/board/card-state.test.ts)                           |
| Created | [src/game/board/card-state.ts](src/game/board/card-state.ts)                                     |
| Changed | [src/game/board/Face.tsx](src/game/board/Face.tsx)                                               |
| Created | [src/game/board/face-design.fixture.json](src/game/board/face-design.fixture.json)               |
| Created | [src/game/board/face-design.test.ts](src/game/board/face-design.test.ts)                         |
| Changed | [src/game/board/face-design.ts](src/game/board/face-design.ts)                                   |
| Created | [src/game/board/keyboard.test.ts](src/game/board/keyboard.test.ts)                               |
| Changed | [src/game/board/keyboard.ts](src/game/board/keyboard.ts)                                         |
| Created | [src/game/completion.test.ts](src/game/completion.test.ts)                                       |
| Changed | [src/game/controls/controls.css](src/game/controls/controls.css)                                 |
| Changed | [src/game/controls/Controls.tsx](src/game/controls/Controls.tsx)                                 |
| Created | [src/game/controls/deal-controls.ts](src/game/controls/deal-controls.ts)                         |
| Changed | [src/game/controls/Reproduction.tsx](src/game/controls/Reproduction.tsx)                         |
| Created | [src/game/controls/reproduction-controls.ts](src/game/controls/reproduction-controls.ts)         |
| Created | [src/game/deck.test.ts](src/game/deck.test.ts)                                                   |
| Changed | [src/game/deck.ts](src/game/deck.ts)                                                             |
| Created | [src/game/deck-rejection.fixture.json](src/game/deck-rejection.fixture.json)                     |
| Created | [src/game/deck-rejection.test.ts](src/game/deck-rejection.test.ts)                               |
| Created | [src/game/elapsed-time.test.ts](src/game/elapsed-time.test.ts)                                   |
| Created | [src/game/elapsed-time.ts](src/game/elapsed-time.ts)                                             |
| Changed | [src/game/game.css](src/game/game.css)                                                           |
| Created | [src/game/game.fixture.ts](src/game/game.fixture.ts)                                             |
| Created | [src/game/game.test.ts](src/game/game.test.ts)                                                   |
| Changed | [src/game/game.ts](src/game/game.ts)                                                             |
| Changed | [src/game/Game.tsx](src/game/Game.tsx)                                                           |
| Changed | [src/game/game.types.ts](src/game/game.types.ts)                                                 |
| Created | [src/game/move-cap.test.ts](src/game/move-cap.test.ts)                                           |
| Created | [src/game/replay.test.ts](src/game/replay.test.ts)                                               |
| Changed | [src/game/replay.ts](src/game/replay.ts)                                                         |
| Created | [src/game/replay-order.test.ts](src/game/replay-order.test.ts)                                   |
| Deleted | `src/game/rules.ts`                                                                              |
| Created | [src/game/saved-format.test.ts](src/game/saved-format.test.ts)                                   |
| Changed | [src/game/saved-format.ts](src/game/saved-format.ts)                                             |
| Created | [src/game/selection.ts](src/game/selection.ts)                                                   |
| Deleted | `src/game/session/__tests__/session.test.ts`                                                     |
| Deleted | `src/game/session/__tests__/storage.test.ts`                                                     |
| Created | [src/game/session/browser-clock.test.ts](src/game/session/browser-clock.test.ts)                 |
| Changed | [src/game/session/browser-clock.ts](src/game/session/browser-clock.ts)                           |
| Changed | [src/game/session/fresh-board.ts](src/game/session/fresh-board.ts)                               |
| Created | [src/game/session/session.feedback.fixture.json](src/game/session/session.feedback.fixture.json) |
| Created | [src/game/session/session.test.ts](src/game/session/session.test.ts)                             |
| Changed | [src/game/session/session.ts](src/game/session/session.ts)                                       |
| Changed | [src/game/session/session.types.ts](src/game/session/session.types.ts)                           |
| Created | [src/game/session/storage.test.ts](src/game/session/storage.test.ts)                             |
| Changed | [src/game/session/storage.ts](src/game/session/storage.ts)                                       |
| Changed | [src/game/session/use-game.ts](src/game/session/use-game.ts)                                     |
| Created | [src/game/session/use-session-clock.ts](src/game/session/use-session-clock.ts)                   |
| Created | [src/game/snapshot.test.ts](src/game/snapshot.test.ts)                                           |
| Changed | [src/game/snapshot.ts](src/game/snapshot.ts)                                                     |
| Created | [src/game/snapshot-pairs.test.ts](src/game/snapshot-pairs.test.ts)                               |
| Changed | [src/game/snapshot-selection.ts](src/game/snapshot-selection.ts)                                 |
| Created | [src/game/status/GameStatus.tsx](src/game/status/GameStatus.tsx)                                 |
| Created | [src/game/status/status.css](src/game/status/status.css)                                         |
| Created | [src/game/status/status-labels.ts](src/game/status/status-labels.ts)                             |
| Changed | [src/main.tsx](src/main.tsx)                                                                     |
| Created | [tests/accessibility.e2e.test.ts](tests/accessibility.e2e.test.ts)                               |
| Created | [tests/accessibility.fixture.json](tests/accessibility.fixture.json)                             |
| Created | [tests/accessibility-fixture.ts](tests/accessibility-fixture.ts)                                 |
| Created | [tests/accessibility-tree.ts](tests/accessibility-tree.ts)                                       |
| Created | [tests/all-sizes.e2e.test.ts](tests/all-sizes.e2e.test.ts)                                       |
| Created | [tests/announcements.e2e.test.ts](tests/announcements.e2e.test.ts)                               |
| Changed | [tests/art.e2e.test.ts](tests/art.e2e.test.ts)                                                   |
| Changed | [tests/artifact-server.ts](tests/artifact-server.ts)                                             |
| Changed | [tests/browser-environment.e2e.test.ts](tests/browser-environment.e2e.test.ts)                   |
| Created | [tests/browser-test.ts](tests/browser-test.ts)                                                   |
| Created | [tests/clock-edges.e2e.test.ts](tests/clock-edges.e2e.test.ts)                                   |
| Created | [tests/controls.e2e.test.ts](tests/controls.e2e.test.ts)                                         |
| Created | [tests/focus-bounds.ts](tests/focus-bounds.ts)                                                   |
| Changed | [tests/game-fixture.ts](tests/game-fixture.ts)                                                   |
| Created | [tests/keyboard.e2e.test.ts](tests/keyboard.e2e.test.ts)                                         |
| Changed | [tests/layout.e2e.test.ts](tests/layout.e2e.test.ts)                                             |
| Changed | [tests/lifecycle.e2e.test.ts](tests/lifecycle.e2e.test.ts)                                       |
| Created | [tests/motion.e2e.test.ts](tests/motion.e2e.test.ts)                                             |
| Created | [tests/move-cap.e2e.test.ts](tests/move-cap.e2e.test.ts)                                         |
| Changed | [tests/play.e2e.test.ts](tests/play.e2e.test.ts)                                                 |
| Created | [tests/preferences.e2e.test.ts](tests/preferences.e2e.test.ts)                                   |
| Created | [tests/snapshots.e2e.test.ts](tests/snapshots.e2e.test.ts)                                       |
| Created | [tests/status.e2e.test.ts](tests/status.e2e.test.ts)                                             |
| Created | [tests/style.e2e.test.ts](tests/style.e2e.test.ts)                                               |
| Created | [tests/touch-scroll.e2e.test.ts](tests/touch-scroll.e2e.test.ts)                                 |
| Created | [tests/ui.fixture.json](tests/ui.fixture.json)                                                   |
| Created | [tests/ui-snapshot.ts](tests/ui-snapshot.ts)                                                     |
| Created | [tests/zoom.e2e.test.ts](tests/zoom.e2e.test.ts)                                                 |
| Created | [tests/zoom-extension.ts](tests/zoom-extension.ts)                                               |
| Changed | [tsconfig.app.json](tsconfig.app.json)                                                           |
| Changed | [tsconfig.base.json](tsconfig.base.json)                                                         |
| Changed | [tsconfig.e2e.json](tsconfig.e2e.json)                                                           |
| Changed | [tsconfig.node.json](tsconfig.node.json)                                                         |
| Created | [verification.json](verification.json)                                                           |
| Changed | [vite.config.ts](vite.config.ts)                                                                 |
| Deleted | `vitest.config.ts`                                                                               |

The cap amendment also updates these files outside the reference:

| Change      | Repository path                                                                |
| ----------- | ------------------------------------------------------------------------------ |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/spec/rules.md           |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/spec/reproducibility.md |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/spec-notes.md           |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/layer-2.md              |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/layer-3.md              |
| Changed     | .agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md              |
| Changed     | src/evals/benchmarks/neon-pairs/requirements.json                              |
| Changed     | src/evals/benchmarks/neon-pairs/trace.json                                     |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/README.md                                |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/acceptance.md                            |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/architecture.md                          |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/controls.md                              |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/input.md                                 |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/reproducibility.md                       |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/rules.md                                 |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/sound.md                                 |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/technology.md                            |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/visuals.md                               |
| Regenerated | src/evals/benchmarks/neon-pairs/specs/manifest.json                            |
| Regenerated | src/evals/benchmarks/neon-pairs/projection.json                                |

Layer 2's clause list changed only for the elapsed and move caps. Layers 3 and 4 changed only their clause counts from 391 to 401. Materialize refroze all ten product documents. Only Rules and Reproducibility changed product bodies. No checks, case definitions, rubrics or other product code was edited.

## Review items

The remaining work before freezing is:

- Have a person confirm the spoken journey for I17.1 through I17.6 and record the screen reader version.
- Have the overseer regenerate the Layer 2 subset pack after integration.
- Leave manual dist and local Chromium cleanup to the overseer while deletion is blocked.
- Have independent clause auditors and the final judge approve the candidate.

Back to [Neon Pairs](README.md).
