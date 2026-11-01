# Neon Pairs Reference Audit

Neon Pairs Reference Audit is the decision and evidence record for the game candidate built to product spec version 2.2.

For example, a malformed snapshot is checked against populated progress so resetting the game cannot hide a validation failure.

Status: version 2.2 candidate with 96 requirements and 412 clauses. Local verification is recorded below. Spoken confirmation belongs to user acceptance.

## Current revision

The [spec revision record](revision-v2.1.md) connects the council decisions to the changed behavior and tests. The v2 judges scored the starting reference 10 out of 10. This revised candidate follows the newly published product choices. The replay follow-up publishes `npm run --silent replay -- PATH`. Version 2.2 trims rarely reached corners from the spec, and the reference follows it. It deletes `.npmrc`, because its relative cache setting left `.npm-cache` folders wherever replay ran.

## Corrections

The first review is recorded in [Reference Review Corrections](review-round-1.md). The eight later corrections are recorded in [Reference Review Two](review-round-2.md). The load-status announcement correction is recorded in [Reference Review Three](review-round-3.md). The v2.1 judge corrections from all four rounds are recorded in [Version 2.1 Review](review-v2.1.md). The 2.2 trim, the fifth round and the polish after two judges scored it are recorded in [Version 2.2 Review](review-v2.2.md). Protected benchmark-check updates stay with the overseer. The current revision applies the approved spec changes.

## Decisions

The candidate keeps these product decisions:

| Subject              | Decision and reason                                                                                                                                                                                                                                                                                                                                                                            |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Rules and constants  | Pure transitions, frozen decks and local states, limits and versions replace the pilot's gathered rules file.                                                                                                                                                                                                                                                                                  |
| Completion           | The banner and final turn announce moves and time.                                                                                                                                                                                                                                                                                                                                             |
| Type and spacing     | Body, title and secondary text follow the stated sizes and layout scale. Board padding stays 6px for the 3px outline and 2px offset.                                                                                                                                                                                                                                                           |
| Announcements        | One keyed polite atomic live region gives each turn result and load notice once, including repeated messages and the final match with its win. A new deal and Resume clear the last result without announcing anything unpublished.                                                                                                                                                            |
| Active time          | One monotonic clock feeds elapsed and mismatch time through the same delta. The game keeps fractional active time, so a mismatch runs from its own activation.                                                                                                                                                                                                                                 |
| Delayed timer        | Synchronize active time before deciding whether an activation is blocked.                                                                                                                                                                                                                                                                                                                      |
| Elapsed cap          | The maintainer capped elapsed time at 359999999ms. Play continues. Snapshot validation rejects larger values. Replay timestamps keep their safe-integer range.                                                                                                                                                                                                                                 |
| Move cap             | The maintainer capped moves at 999999. Matching and winning continue. Snapshots and saved bests reject larger values.                                                                                                                                                                                                                                                                          |
| Formats              | Board code, snapshot and replay format versions stay at 1. Product spec is 2.2. Local data formats stay at 1.                                                                                                                                                                                                                                                                                  |
| Test runner          | The maintainer requires Vitest 5.0.5 for game unit tests. Product-rule coverage is kept. The removed second-clock parameter's test is replaced with shared-time assertions.                                                                                                                                                                                                                    |
| Rejection vector     | A literal BigInt-calculated shuffle fixture exercises the rejection branch independently.                                                                                                                                                                                                                                                                                                      |
| Half-pair gap        | The first half-pair mutation exposed an overlapping-invalid-input gap. An otherwise-valid fixture now isolates it.                                                                                                                                                                                                                                                                             |
| Reference separation | Game files stay in reference. Audit records, captures and mutation tools stay here.                                                                                                                                                                                                                                                                                                            |
| Focus and controls   | Board handles its remembered focus. Input reset uses a keyed input while keeping action buttons mounted. Game results sit beside the board. Load feedback follows reproduction controls.                                                                                                                                                                                                       |
| Mutations            | Several clauses can break together. A kill requires the declared test and assertion reason. M07 exercises real timer cancellation. M19 leaves padding unchanged and fails outline bounds. M25 removes the card scroll margin. M28 removes the restored board scroll position. M32 scrolls on pointer focus again. M41 lets modified arrows move focus. M43 and M44 move spacing off the scale. |
| Board width          | The board takes its width from its scroll area, so visible page and board scrollbars never cut off the last column.                                                                                                                                                                                                                                                                            |
| Export time          | Export synchronizes the active clock first, then writes whole milliseconds. Pending mismatch time rounds up, so it stays at 1ms or more.                                                                                                                                                                                                                                                       |
| Labels               | Label and limit owners sit beside their components. Browser journeys import them. The control contract keeps independent literals for the published text.                                                                                                                                                                                                                                      |
| Focus scroll         | Keyboard focus, Tab, Shift+Tab and Resume end with the card's nearest scroll and 6px scroll margin, after the browser's own focus scroll is undone. Pointer and touch focus leave a pressed card still, so the click lands, and the card scrolls fully into view once the click or tap completes.                                                                                              |
| Mutation reports     | A Node test reporter in this folder records browser failures, so the game's tests carry no audit hook.                                                                                                                                                                                                                                                                                         |
| Pointer input        | Every completed click or tap activates. Chromium sends no click after a context menu, a long press or a touch scroll, so the card keeps no pointer state. A press that drifts is out of scope. Headless Chromium can't show a long press, so I03.2 needs a real-device review.                                                                                                                 |
| Keyboard             | The board moves focus only for the four arrow keys without modifiers. Enter and Space use the native button. Every other key and modifier combination stays with the browser.                                                                                                                                                                                                                  |
| Card rendering       | Cards are memoized, and the board reads face-up and matched sets once per state, so a clock tick renders no card. Only matched cards carry aria-disabled. A blocked card ignores activation without it.                                                                                                                                                                                        |
| Spacing              | A browser check reads every computed margin, padding and gap and accepts only the V09 scale, auto and normal. Options inside the native select are excluded.                                                                                                                                                                                                                                   |
| Replay messages      | A relative replay path follows the caller's folder from INIT_CWD. The message names a missing file, malformed JSON or the broken rule.                                                                                                                                                                                                                                                         |
| Style ownership      | Card, board, deal controls, reproduction panel and status rows each import their own styles. Native controls share control.css, so cards don't depend on control styles.                                                                                                                                                                                                                       |
| Type ownership       | Each type lives in the module that owns its behavior. The game core decides which cards face up.                                                                                                                                                                                                                                                                                               |
| Idle time            | Browser tests stay idle through a hidden page and a paused import before checking elapsed and mismatch time.                                                                                                                                                                                                                                                                                   |
| Check mark           | The mark takes 16% of the card in its top-right corner. Wider marks reach the top-right marker.                                                                                                                                                                                                                                                                                                |
| Saved bests          | Each preference write saves this tab's size and bests. Several open tabs are out of scope, so one tab may overwrite another.                                                                                                                                                                                                                                                                   |
| Bundle               | Module preload is off, so the page carries no fetch call or preload helper.                                                                                                                                                                                                                                                                                                                    |
| Screen reader        | I17.1 through I17.6 are tree-checked. The README records the browser and tool versions and that no screen reader was used.                                                                                                                                                                                                                                                                     |

## Checks

The named combination is Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. The browser before hook asserts all four actual versions. Prettier 3.9.6 formats the source.

The locked install and checks have these results:

| Gate                           | Result                                                                                                                                    |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Product install and type check | Passed                                                                                                                                    |
| Product tests                  | 242 of 273 passed, zero skipped. 29 failures come from the protected checks, which still expect spec 2.1. Two failures predate this round |
| Reference install              | Passed offline from the lockfile with the worktree-local npm cache                                                                        |
| Reference type check           | All three projects passed                                                                                                                 |
| Vitest unit and static checks  | 131 passed, zero failed and zero skipped                                                                                                  |
| Build                          | Current source emitted one offline HTML file                                                                                              |
| Browser command                | 93 passed in each of three consecutive runs, zero failed and zero skipped                                                                 |
| Mutation harness               | 39 caught for their declared assertion reasons                                                                                            |
| Harness self-test              | Passed. Unrelated test names and failure reasons are rejected. The failure reporter keeps each failed name and assertion message          |
| Audit type check               | Passed                                                                                                                                    |

Each browser command builds the game and diagnostic sheet afresh. Vite clears its output before writing. Fresh contexts run against that build. The zoom journey uses the specified 640x400 CSS-pixel viewport and device scale 2. Text bounds check its contents as well as outer layout. The scrollbar journey launches Chromium with scrollbars that take layout space, as on a Windows desktop. Offline delivery permits only the HTML's own file request.

The [verification record](verification.json) gives commands and counts. The [seed record](seed-record.json) binds source, configuration, trace and evidence hashes. Raw command logs are disposable outputs. The seed is not frozen.

## Screen reader

The [accessibility record](evidence/accessibility-journey.json) contains 28 sanitized Chromium accessibility-tree captures. It includes independent control names, all closed positions, revealed names, disabled matched states, atomic result payloads, the final summary, Tab exit and New Game. Six captures check both invalid load notices and the paused snapshot notice, each submitted twice. A separate observer checks one live-region change per turn and per load event.

Tree checks do not record speech. You follow [Screen Reader Journey](screen-reader-journey.md) during user acceptance and record NVDA and browser versions there. Spoken confirmation is not a developer delivery gate.

## Visual review

The [grayscale sheet](face-sheet-grayscale.png) contains every face on an actual 44px card. The mask test still checks 128 different geometries. Mask uniqueness and perceptual distinction are different results. The round-2 judge passed all 128 grayscale faces as distinguishable at 44px. The accepted PNG is unchanged.

## Process

T07.9 concerns stopping development processes. A12.1 concerns the order used when changing a rule. They describe work rather than game behavior. Their trace entries therefore cite process review, not product assertions.

This correction round started no development server or watcher. Builds and tests completed as child processes. Browser hooks close their contexts, browser and private HTTP server. The earlier development-command record reported its server stopped. A reviewer checks the work record for T07.9.

The cap rules were decided and recorded in the preceding packet. This round changes the implementation, tests and evidence under existing product rules. No new product rule or format was added. A reviewer checks that record for A12.1. Source placement alone doesn't show the historical edit order.

## Review clauses

The following 51 clauses use a review note or still need judgment. The first review's source verdicts do not approve the changed candidate. The next judgment reviews these reasons alongside executable evidence:

| Clause | Reason                                                                                                                                                                                                |
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| I03.2  | Real-device review. Right-click has executable evidence. Headless Chromium's gesture tool sends no click even for a short tap, so only a touch device shows that a long press leaves the card closed. |
| V11.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| V11.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| V12.7  | Passed by the round-2 judge. All 128 faces are distinguishable at 44px in grayscale.                                                                                                                  |
| T03.5  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T03.6  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T05.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T05.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T05.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T05.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T06.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T06.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T06.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T06.5  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T07.9  | Working-process review. Product behavior doesn't show development history.                                                                                                                            |
| T10.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T10.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| T10.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A01.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A01.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A01.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A02.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A02.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A02.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A02.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A03.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A03.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A04.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A04.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A04.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A04.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A05.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A05.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A05.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A05.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A05.5  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A06.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A07.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A07.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A07.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A09.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A09.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A09.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A10.1  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A10.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A10.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A10.4  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A10.5  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A12.1  | Working-process review. Product behavior doesn't show development history.                                                                                                                            |
| A12.2  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |
| A12.3  | Source or design review. The rule requires reading the final structure, types or dependency choices.                                                                                                  |

## Clause ledger

Every clause has real paths in the [trace map](../trace.json). Existing check slots stay as recorded for v2. New clause check slots wait for the separate checks packet. This ledger records the reference evidence type:

| Clause | Evidence type         |
| ------ | --------------------- |
| R01.1  | Executable evidence   |
| R01.2  | Executable evidence   |
| R01.3  | Executable evidence   |
| R01.4  | Executable evidence   |
| R02.1  | Executable evidence   |
| R02.2  | Executable evidence   |
| R02.3  | Executable evidence   |
| R03.1  | Executable evidence   |
| R03.2  | Executable evidence   |
| R03.3  | Executable evidence   |
| R04.1  | Executable evidence   |
| R04.2  | Executable evidence   |
| R04.3  | Executable evidence   |
| R04.4  | Executable evidence   |
| R04.5  | Executable evidence   |
| R05.1  | Executable evidence   |
| R05.2  | Executable evidence   |
| R05.3  | Executable evidence   |
| R05.4  | Executable evidence   |
| R05.5  | Executable evidence   |
| R05.6  | Executable evidence   |
| R05.7  | Executable evidence   |
| R05.8  | Executable evidence   |
| R05.9  | Executable evidence   |
| R06.1  | Executable evidence   |
| R06.2  | Executable evidence   |
| R06.3  | Executable evidence   |
| R06.4  | Executable evidence   |
| R06.5  | Executable evidence   |
| R06.6  | Executable evidence   |
| R07.1  | Executable evidence   |
| R07.2  | Executable evidence   |
| R07.3  | Executable evidence   |
| R07.4  | Executable evidence   |
| R07.5  | Executable evidence   |
| R07.6  | Executable evidence   |
| R07.7  | Executable evidence   |
| R07.8  | Executable evidence   |
| R07.9  | Executable evidence   |
| R07.10 | Executable evidence   |
| R07.11 | Executable evidence   |
| R07.12 | Executable evidence   |
| R08.1  | Executable evidence   |
| R08.2  | Executable evidence   |
| R08.3  | Executable evidence   |
| R08.4  | Executable evidence   |
| R08.5  | Executable evidence   |
| R08.6  | Executable evidence   |
| R09.1  | Executable evidence   |
| R09.2  | Executable evidence   |
| R09.3  | Executable evidence   |
| R09.4  | Executable evidence   |
| R09.5  | Executable evidence   |
| R09.6  | Executable evidence   |
| R10.1  | Executable evidence   |
| R10.2  | Executable evidence   |
| R10.3  | Executable evidence   |
| R10.4  | Executable evidence   |
| R10.5  | Executable evidence   |
| R10.6  | Executable evidence   |
| R10.7  | Executable evidence   |
| R11.1  | Executable evidence   |
| R11.2  | Executable evidence   |
| R11.3  | Executable evidence   |
| R11.4  | Executable evidence   |
| R12.1  | Executable evidence   |
| R12.2  | Executable evidence   |
| C01.1  | Executable evidence   |
| C01.2  | Executable evidence   |
| C01.3  | Executable evidence   |
| C01.4  | Executable evidence   |
| C02.1  | Executable evidence   |
| C02.2  | Executable evidence   |
| C02.3  | Executable evidence   |
| C03.1  | Executable evidence   |
| C03.2  | Executable evidence   |
| C03.3  | Executable evidence   |
| C04.1  | Executable evidence   |
| C04.2  | Executable evidence   |
| C04.3  | Executable evidence   |
| C05.1  | Executable evidence   |
| C05.2  | Executable evidence   |
| C05.3  | Executable evidence   |
| C05.4  | Executable evidence   |
| C06.1  | Executable evidence   |
| C06.2  | Executable evidence   |
| C07.1  | Executable evidence   |
| C07.2  | Executable evidence   |
| C07.3  | Executable evidence   |
| C08.1  | Executable evidence   |
| C08.2  | Executable evidence   |
| C08.3  | Executable evidence   |
| C08.4  | Executable evidence   |
| C08.5  | Executable evidence   |
| C08.6  | Executable evidence   |
| C08.7  | Executable evidence   |
| C09.1  | Executable evidence   |
| C09.2  | Executable evidence   |
| C09.3  | Executable evidence   |
| C09.4  | Executable evidence   |
| C09.5  | Executable evidence   |
| C10.1  | Executable evidence   |
| C10.2  | Executable evidence   |
| C10.3  | Executable evidence   |
| C10.4  | Executable evidence   |
| C10.5  | Executable evidence   |
| C10.6  | Executable evidence   |
| C10.7  | Executable evidence   |
| C11.1  | Executable evidence   |
| C11.2  | Executable evidence   |
| C11.3  | Executable evidence   |
| C11.4  | Executable evidence   |
| C12.1  | Executable evidence   |
| C12.2  | Executable evidence   |
| C12.3  | Executable evidence   |
| C12.4  | Executable evidence   |
| C13.1  | Executable evidence   |
| C13.2  | Executable evidence   |
| C13.3  | Executable evidence   |
| C13.4  | Executable evidence   |
| C13.5  | Executable evidence   |
| C14.1  | Executable evidence   |
| C14.2  | Executable evidence   |
| C14.3  | Executable evidence   |
| C14.4  | Executable evidence   |
| C14.5  | Executable evidence   |
| I01.1  | Executable evidence   |
| I01.2  | Executable evidence   |
| I03.1  | Executable evidence   |
| I03.2  | Executable and review |
| I04.1  | Executable evidence   |
| I04.2  | Executable evidence   |
| I05.1  | Executable evidence   |
| I05.2  | Executable evidence   |
| I05.3  | Executable evidence   |
| I06.1  | Executable evidence   |
| I06.2  | Executable evidence   |
| I06.3  | Executable evidence   |
| I06.4  | Executable evidence   |
| I06.9  | Executable evidence   |
| I06.10 | Executable evidence   |
| I07.1  | Executable evidence   |
| I08.1  | Executable evidence   |
| I08.2  | Executable evidence   |
| I09.1  | Executable evidence   |
| I09.2  | Executable evidence   |
| I09.3  | Executable evidence   |
| I10.1  | Executable evidence   |
| I10.2  | Executable evidence   |
| I11.1  | Executable evidence   |
| I12.1  | Executable evidence   |
| I12.2  | Executable evidence   |
| I13.1  | Executable evidence   |
| I13.2  | Executable evidence   |
| I14.1  | Executable evidence   |
| I14.2  | Executable evidence   |
| I14.3  | Executable evidence   |
| I15.1  | Executable evidence   |
| I15.2  | Executable evidence   |
| I16.1  | Executable evidence   |
| I16.2  | Executable evidence   |
| I16.3  | Executable evidence   |
| I16.4  | Executable evidence   |
| I16.5  | Executable evidence   |
| I16.6  | Executable evidence   |
| I16.7  | Executable evidence   |
| I16.8  | Executable evidence   |
| I16.9  | Executable evidence   |
| I17.1  | Tree-checked          |
| I17.2  | Tree-checked          |
| I17.3  | Tree-checked          |
| I17.4  | Tree-checked          |
| I17.5  | Tree-checked          |
| I17.6  | Tree-checked          |
| V01.1  | Executable evidence   |
| V01.2  | Executable evidence   |
| V01.3  | Executable evidence   |
| V01.4  | Executable evidence   |
| V02.1  | Executable evidence   |
| V02.2  | Executable evidence   |
| V02.3  | Executable evidence   |
| V03.1  | Executable evidence   |
| V03.2  | Executable evidence   |
| V03.3  | Executable evidence   |
| V04.1  | Executable evidence   |
| V04.2  | Executable evidence   |
| V04.3  | Executable evidence   |
| V04.4  | Executable evidence   |
| V05.1  | Executable evidence   |
| V05.2  | Executable evidence   |
| V06.1  | Executable evidence   |
| V06.2  | Executable evidence   |
| V06.3  | Executable evidence   |
| V06.4  | Executable evidence   |
| V06.5  | Executable evidence   |
| V06.6  | Executable evidence   |
| V06.7  | Executable evidence   |
| V06.8  | Executable evidence   |
| V07.1  | Executable evidence   |
| V08.1  | Executable evidence   |
| V08.2  | Executable evidence   |
| V08.3  | Executable evidence   |
| V08.4  | Executable evidence   |
| V09.1  | Executable evidence   |
| V09.2  | Executable evidence   |
| V10.1  | Executable evidence   |
| V10.2  | Executable evidence   |
| V10.3  | Executable evidence   |
| V10.4  | Executable evidence   |
| V10.5  | Executable evidence   |
| V10.6  | Executable evidence   |
| V10.7  | Executable evidence   |
| V10.8  | Executable evidence   |
| V10.9  | Executable evidence   |
| V11.1  | Review note           |
| V11.2  | Review note           |
| V12.1  | Executable evidence   |
| V12.2  | Executable evidence   |
| V12.3  | Executable evidence   |
| V12.4  | Executable evidence   |
| V12.5  | Executable evidence   |
| V12.6  | Executable evidence   |
| V12.7  | Review note           |
| V12.8  | Executable evidence   |
| V13.1  | Executable evidence   |
| V13.2  | Executable evidence   |
| V14.1  | Executable evidence   |
| V14.2  | Executable evidence   |
| V15.1  | Executable evidence   |
| V15.2  | Executable evidence   |
| V15.3  | Executable evidence   |
| V16.1  | Executable evidence   |
| V16.2  | Executable evidence   |
| V17.1  | Executable evidence   |
| V17.2  | Executable evidence   |
| V18.1  | Executable evidence   |
| V18.2  | Executable evidence   |
| V18.3  | Executable evidence   |
| V18.4  | Executable evidence   |
| V19.1  | Executable evidence   |
| V19.2  | Executable evidence   |
| V19.3  | Executable evidence   |
| V19.4  | Executable evidence   |
| V19.5  | Executable evidence   |
| V20.1  | Executable evidence   |
| V20.2  | Executable evidence   |
| V20.3  | Executable evidence   |
| V20.4  | Executable evidence   |
| V20.5  | Executable evidence   |
| S01.1  | Executable evidence   |
| S02.1  | Executable evidence   |
| S02.2  | Executable evidence   |
| S02.3  | Executable evidence   |
| T01.1  | Executable evidence   |
| T01.2  | Executable evidence   |
| T01.3  | Executable evidence   |
| T01.4  | Executable evidence   |
| T02.1  | Executable evidence   |
| T02.2  | Executable evidence   |
| T02.3  | Executable evidence   |
| T02.4  | Executable evidence   |
| T02.5  | Executable evidence   |
| T02.6  | Executable evidence   |
| T02.7  | Executable evidence   |
| T02.8  | Executable evidence   |
| T02.9  | Executable evidence   |
| T02.10 | Executable evidence   |
| T03.1  | Executable evidence   |
| T03.2  | Executable evidence   |
| T03.3  | Executable evidence   |
| T03.4  | Executable evidence   |
| T03.5  | Review note           |
| T03.6  | Review note           |
| T04.1  | Executable evidence   |
| T04.2  | Executable evidence   |
| T05.1  | Review note           |
| T05.2  | Review note           |
| T05.3  | Review note           |
| T05.4  | Review note           |
| T06.1  | Review note           |
| T06.2  | Review note           |
| T06.3  | Executable evidence   |
| T06.4  | Review note           |
| T06.5  | Review note           |
| T07.1  | Executable evidence   |
| T07.2  | Executable evidence   |
| T07.3  | Executable evidence   |
| T07.4  | Executable evidence   |
| T07.5  | Executable evidence   |
| T07.6  | Executable evidence   |
| T07.7  | Executable evidence   |
| T07.8  | Executable evidence   |
| T07.9  | Review note           |
| T08.1  | Executable evidence   |
| T08.2  | Executable evidence   |
| T08.3  | Executable evidence   |
| T08.4  | Executable evidence   |
| T09.1  | Executable evidence   |
| T09.2  | Executable evidence   |
| T10.1  | Review note           |
| T10.2  | Review note           |
| T10.3  | Review note           |
| T10.4  | Executable evidence   |
| A01.1  | Review note           |
| A01.2  | Review note           |
| A01.3  | Review note           |
| A02.1  | Review note           |
| A02.2  | Review note           |
| A02.3  | Review note           |
| A02.4  | Review note           |
| A03.1  | Review note           |
| A03.2  | Review note           |
| A04.1  | Review note           |
| A04.2  | Review note           |
| A04.3  | Review note           |
| A04.4  | Review note           |
| A04.5  | Executable evidence   |
| A05.1  | Review note           |
| A05.2  | Review note           |
| A05.3  | Review note           |
| A05.4  | Review note           |
| A05.5  | Review note           |
| A05.6  | Executable evidence   |
| A06.1  | Review note           |
| A06.2  | Executable evidence   |
| A06.3  | Executable evidence   |
| A07.1  | Review note           |
| A07.2  | Review note           |
| A07.3  | Review note           |
| A08.1  | Executable evidence   |
| A08.2  | Executable evidence   |
| A08.3  | Executable evidence   |
| A08.4  | Executable evidence   |
| A09.1  | Review note           |
| A09.2  | Review note           |
| A09.3  | Executable evidence   |
| A09.4  | Review note           |
| A10.1  | Review note           |
| A10.2  | Review note           |
| A10.3  | Review note           |
| A10.4  | Review note           |
| A10.5  | Review note           |
| A11.1  | Executable evidence   |
| A11.2  | Executable evidence   |
| A11.3  | Executable evidence   |
| A12.1  | Review note           |
| A12.2  | Review note           |
| A12.3  | Review note           |
| D01.1  | Executable evidence   |
| D01.2  | Executable evidence   |
| D01.3  | Executable evidence   |
| D01.4  | Executable evidence   |
| D02.1  | Executable evidence   |
| D02.2  | Executable evidence   |
| D02.3  | Executable evidence   |
| D02.4  | Executable evidence   |
| D03.1  | Executable evidence   |
| D03.2  | Executable evidence   |
| D03.3  | Executable evidence   |
| D04.1  | Executable evidence   |
| D04.2  | Executable evidence   |
| D04.3  | Executable evidence   |
| D04.4  | Executable evidence   |
| D04.5  | Executable evidence   |
| D04.6  | Executable evidence   |
| D05.1  | Executable evidence   |
| D05.2  | Executable evidence   |
| D05.3  | Executable evidence   |
| D05.4  | Executable evidence   |
| D05.5  | Executable evidence   |
| D05.6  | Executable evidence   |
| D05.7  | Executable evidence   |
| D06.1  | Executable evidence   |
| D06.2  | Executable evidence   |
| D06.3  | Executable evidence   |
| D06.4  | Executable evidence   |
| D06.5  | Executable evidence   |
| D06.6  | Executable evidence   |
| D06.7  | Executable evidence   |
| D07.1  | Executable evidence   |
| D07.2  | Executable evidence   |
| D07.3  | Executable evidence   |
| D07.4  | Executable evidence   |
| D07.5  | Executable evidence   |
| D07.6  | Executable evidence   |
| D07.7  | Executable evidence   |
| D07.8  | Executable evidence   |
| D07.9  | Executable evidence   |
| D08.1  | Executable evidence   |
| D08.2  | Executable evidence   |
| D08.3  | Executable evidence   |
| D08.4  | Executable evidence   |
| D08.5  | Executable evidence   |
| D08.6  | Executable evidence   |
| D08.7  | Executable evidence   |
| D09.1  | Executable evidence   |
| D09.2  | Executable evidence   |
| D09.3  | Executable evidence   |
| D09.4  | Executable evidence   |
| D09.5  | Executable evidence   |
| D09.6  | Executable evidence   |
| D09.7  | Executable evidence   |
| D10.1  | Executable evidence   |
| D10.2  | Executable evidence   |
| D10.3  | Executable evidence   |
| D10.4  | Executable evidence   |
| D10.5  | Executable evidence   |
| D10.6  | Executable evidence   |
| D10.7  | Executable evidence   |
| D10.8  | Executable evidence   |
| D10.9  | Executable evidence   |
| D10.10 | Executable evidence   |

## Mutations

Run from reference with the documented local browser settings, then call `node ../reference-audit/mutations.ts`. The harness checks clean unit and browser baselines first. It changes temporary copies under the game test-work folder and removes each copy afterward. Browser failures reach it through [failure-reporter.ts](failure-reporter.ts), a Node test reporter, so the game's tests need no hook for it. Each browser test name reaches Node as an exact escaped pattern.

Each fixture lists directly affected clauses, one test and a required assertion reason. The report records the actual assertion and other failed tests. An unrelated failure, build error or missing report cannot count as a kill. The [mutation results](mutation-results.json) contain all 39 reasons:

| Mutation | Clauses                    | Result | Required assertion                              |
| -------- | -------------------------- | ------ | ----------------------------------------------- |
| M01      | R03.1, R03.2, R04.1, R04.2 | Caught | R03 matching pair stays matched                 |
| M02      | R05.1, R05.4               | Caught | R05 second distinct reveal adds one move        |
| M03      | R07.1, D09.1               | Caught | R07 first reveal starts time                    |
| M04      | R04.3, I10.2, D09.4        | Caught | R04 mismatch blocks activation                  |
| M05      | R04.1, D09.3               | Caught | R04 mismatch closes at 800ms                    |
| M06      | R07.9, R07.11, D09.5       | Caught | R07 elapsed time saturates                      |
| M07      | A04.3, R12.1               | Caught | A04 active timer cancelled at unmount           |
| M08      | C10.4                      | Caught | C10 paused import blocks activation             |
| M09      | R10.1, R10.2               | Caught | R10 size six has its separate best              |
| M10      | R10.2, R10.4               | Caught | R10 worse score keeps best                      |
| M11      | R10.3                      | Caught | R10 scores survive storage round trip           |
| M12      | D03.2, D03.3, D04.6        | Caught | D03 generator follows independent states        |
| M13      | D01.2                      | Caught | D01 inclusive maximum seed                      |
| M14      | D05.1, D06.2               | Caught | snapshot rejects isolated field case 1          |
| M15      | D06.3                      | Caught | D06 half pair is invalid                        |
| M16      | D09.1, D09.2, D09.3        | Caught | D09 time advances before activation             |
| M17      | D08.5, D08.7               | Caught | D08 equal timestamps keep array order           |
| M18      | I06.1, I07.1               | Caught | I07 left movement stops at row edge             |
| M19      | V16.1, V19.5, V20.4        | Caught | V19 full focus outline                          |
| M20      | I14.2, A08.1, V14.2        | Caught | I14 hidden cards mount no face                  |
| M21      | R05.5, R05.8, D07.8        | Caught | R05 move count saturates                        |
| M22      | V04.1, V05.2               | Caught | V04 board fits beside its scrollbars            |
| M23      | C08.6, C08.7               | Caught | C08 export reads the active clock               |
| M24      | V06.6                      | Caught | V06 secondary color .eyebrow                    |
| M25      | I13.2, V19.5               | Caught | V19 full focus outline                          |
| M26      | R04.4, R07.3, R07.8        | Caught | R04 hidden time leaves the mismatch             |
| M27      | R04.5, R07.4, C10.3        | Caught | R04 paused time leaves the mismatch             |
| M28      | I05.1, I13.1               | Caught | I13 Shift+Tab enters with nearest alignment     |
| M31      | C14.3                      | Caught | C14 relative path follows the caller folder     |
| M32      | I01.2, I13.1, V19.4, V20.3 | Caught | I01 click after a wheel scroll                  |
| M33      | C08.7, R04.1               | Caught | C08 the last quarter millisecond exports as 1ms |
| M34      | V15.1, V12.3               | Caught | V15 check marks cover no face artwork           |
| M36      | I13.1, V19.5, V20.4        | Caught | I13 a clicked card ends in view                 |
| M39      | C12.3, A06.2               | Caught | C12 import leaves the shown best alone          |
| M40      | C12.3                      | Caught | C12 import leaves the shown best alone          |
| M41      | I06.10                     | Caught | I06 unlisted key                                |
| M42      | C11.2, C13.2               | Caught | C11 an invalid code keeps focus in its draft    |
| M43      | V09.1                      | Caught | V09 won game with the reproduction panel open   |
| M44      | V09.1                      | Caught | V09 won game with the reproduction panel open   |

## Size

These counts follow Prettier 3.9.6. Physical lines include blank lines. The nonblank column excludes them. Tiers cover game source, styles, tests, scripts and configuration:

| Tier                | Files | Physical lines | Nonblank lines |
| ------------------- | ----- | -------------- | -------------- |
| Configuration       | 10    | 126            | 123            |
| Styles              | 8     | 303            | 303            |
| Development scripts | 5     | 126            | 118            |
| Tests and fixtures  | 76    | 4701           | 4489           |
| Components          | 13    | 527            | 502            |
| Game functions      | 11    | 527            | 492            |
| Browser session     | 7     | 370            | 336            |

Counts exclude Markdown, package-lock.json, HTML, generated distributions, captured evidence, audit tools, command logs, caches, browser downloads and dependencies. Components include the two label modules and the board scroll and focus hooks beside them. Every authored game TypeScript file is within 200 lines. Files above the 100-line aim are listed in verification.json.

## Revision files

The [revision file inventory](revision-files.json) lists every created, changed and deleted file for v2.1, with separate sections for the four v2.1 review rounds and the 2.2 trim with the fifth round and its polish. The following table keeps the earlier v2 correction history.

## File changes

The following inventory records the first correction packet. Reference Review Two lists the later file changes. Audit moves appear as a deleted old path and created new path. Generated output and ignored work folders are excluded:

| Change  | Path                                                   |
| ------- | ------------------------------------------------------ |
| Created | `reference-audit/.gitignore`                           |
| Created | `reference-audit/AUDIT.md`                             |
| Created | `reference-audit/evidence/accessibility-journey.json`  |
| Created | `reference-audit/face-sheet-grayscale.png`             |
| Created | `reference-audit/mutation-report.test.ts`              |
| Created | `reference-audit/mutation-report.ts`                   |
| Created | `reference-audit/mutation-results.json`                |
| Created | `reference-audit/mutations.fixture.json`               |
| Created | `reference-audit/mutations.ts`                         |
| Created | `reference-audit/review-round-1.md`                    |
| Created | `reference-audit/screen-reader-journey.md`             |
| Created | `reference-audit/seed-record.json`                     |
| Created | `reference-audit/tsconfig.json`                        |
| Created | `reference-audit/verification.json`                    |
| Deleted | `reference/AUDIT.md`                                   |
| Changed | `reference/README.md`                                  |
| Changed | `reference/art/face-sheet.tsx`                         |
| Created | `reference/art/session-harness.tsx`                    |
| Deleted | `reference/evidence/accessibility-journey.json`        |
| Deleted | `reference/mutation-results.json`                      |
| Changed | `reference/package.json`                               |
| Deleted | `reference/scripts/mutations.fixture.json`             |
| Deleted | `reference/scripts/mutations.ts`                       |
| Changed | `reference/scripts/replay.test.ts`                     |
| Changed | `reference/scripts/source-audit.test.ts`               |
| Changed | `reference/src/game/Game.tsx`                          |
| Changed | `reference/src/game/board/Board.tsx`                   |
| Changed | `reference/src/game/board/board.css`                   |
| Changed | `reference/src/game/board/card-state.test.ts`          |
| Changed | `reference/src/game/board/face-design.test.ts`         |
| Changed | `reference/src/game/board/keyboard.test.ts`            |
| Changed | `reference/src/game/completion.test.ts`                |
| Changed | `reference/src/game/controls/Controls.tsx`             |
| Changed | `reference/src/game/controls/Reproduction.tsx`         |
| Deleted | `reference/src/game/controls/deal-controls.ts`         |
| Deleted | `reference/src/game/controls/reproduction-controls.ts` |
| Changed | `reference/src/game/deck-rejection.test.ts`            |
| Changed | `reference/src/game/deck.test.ts`                      |
| Changed | `reference/src/game/deck.ts`                           |
| Changed | `reference/src/game/elapsed-time.test.ts`              |
| Changed | `reference/src/game/elapsed-time.ts`                   |
| Changed | `reference/src/game/game.css`                          |
| Changed | `reference/src/game/game.test.ts`                      |
| Changed | `reference/src/game/game.ts`                           |
| Changed | `reference/src/game/move-cap.test.ts`                  |
| Changed | `reference/src/game/replay-order.test.ts`              |
| Changed | `reference/src/game/replay.test.ts`                    |
| Changed | `reference/src/game/saved-format.test.ts`              |
| Changed | `reference/src/game/session/browser-clock.test.ts`     |
| Changed | `reference/src/game/session/session.test.ts`           |
| Changed | `reference/src/game/session/session.ts`                |
| Changed | `reference/src/game/session/storage.test.ts`           |
| Changed | `reference/src/game/session/use-game.ts`               |
| Changed | `reference/src/game/session/use-session-clock.ts`      |
| Created | `reference/src/game/snapshot-isolation.test.ts`        |
| Changed | `reference/src/game/snapshot-pairs.test.ts`            |
| Changed | `reference/src/game/snapshot.test.ts`                  |
| Changed | `reference/src/game/snapshot.ts`                       |
| Changed | `reference/src/game/status/GameStatus.tsx`             |
| Deleted | `reference/src/game/status/status-labels.ts`           |
| Changed | `reference/tests/all-sizes.e2e.test.ts`                |
| Changed | `reference/tests/browser-environment.e2e.test.ts`      |
| Changed | `reference/tests/browser-test.ts`                      |
| Changed | `reference/tests/clock-edges.e2e.test.ts`              |
| Created | `reference/tests/control-contract.e2e.test.ts`         |
| Created | `reference/tests/control-labels.ts`                    |
| Changed | `reference/tests/controls.e2e.test.ts`                 |
| Changed | `reference/tests/focus-bounds.ts`                      |
| Created | `reference/tests/focus-clearance.e2e.test.ts`          |
| Changed | `reference/tests/game-fixture.ts`                      |
| Created | `reference/tests/input-contract.e2e.test.ts`           |
| Changed | `reference/tests/keyboard.e2e.test.ts`                 |
| Changed | `reference/tests/layout.e2e.test.ts`                   |
| Changed | `reference/tests/lifecycle.e2e.test.ts`                |
| Changed | `reference/tests/move-cap.e2e.test.ts`                 |
| Created | `reference/tests/persistence-contract.e2e.test.ts`     |
| Changed | `reference/tests/play.e2e.test.ts`                     |
| Changed | `reference/tests/preferences.e2e.test.ts`              |
| Created | `reference/tests/readability.ts`                       |
| Changed | `reference/tests/snapshots.e2e.test.ts`                |
| Changed | `reference/tests/status.e2e.test.ts`                   |
| Changed | `reference/tests/style.e2e.test.ts`                    |
| Created | `reference/tests/timer-lifecycle.e2e.test.ts`          |
| Changed | `reference/tests/touch-scroll.e2e.test.ts`             |
| Changed | `reference/tests/ui.fixture.json`                      |
| Changed | `reference/tests/zoom-extension.ts`                    |
| Changed | `reference/tests/zoom.e2e.test.ts`                     |
| Changed | `reference/tsconfig.node.json`                         |
| Deleted | `reference/verification.json`                          |
| Created | `reference/vitest.config.ts`                           |
| Changed | `requirements.json`                                    |
| Changed | `trace.json`                                           |

## Review items

The source and process review clauses above need the next independent judgment. I17 is complete through the automated journey and tool record. Spoken output is left to user acceptance. The overseer handles the protected benchmark checks and final freeze decision.

Both packs are regenerated in this revision. Manual dist and local Chromium cleanup stays with the overseer. Earlier cleanup was rejected by automatic approval review as blocked by policy. No alternate deletion route was used.

Back to [Neon Pairs](../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/_neon-pairs.md).
