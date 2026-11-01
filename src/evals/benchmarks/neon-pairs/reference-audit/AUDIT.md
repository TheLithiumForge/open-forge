# Neon Pairs Reference Audit

Neon Pairs Reference Audit is the decision and evidence record for the game candidate built to product spec version 2.

For example, a malformed snapshot is checked against populated progress so resetting the game cannot hide a validation failure.

Status: corrected candidate with 97 requirements and 401 clauses. Independent judgment is pending. I17.6 is the one open person item.

## Corrections

Every judge and auditor finding has a disposition in [Reference Review Corrections](review-round-1.md). Protected benchmark-check defects stay with the overseer. No specification change is proposed.

## Decisions

The candidate keeps these product decisions:

| Subject              | Decision and reason                                                                                                                                                                       |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Rules and constants  | Pure transitions, frozen decks and local states, limits and versions replace the pilot's gathered rules file.                                                                             |
| Completion           | The banner and final turn announce moves and time.                                                                                                                                        |
| Type and spacing     | Body, title and secondary text follow the stated sizes and layout scale. Board padding stays 6px for the 3px outline and 2px offset.                                                      |
| Announcements        | One keyed polite atomic live region gives each result once, including repeated results and the final match with its win.                                                                  |
| Active time          | One monotonic clock feeds elapsed and mismatch time through the same delta. The unsupported second advanceGame time parameter is removed.                                                 |
| Delayed timer        | Synchronize active time before deciding whether an activation is blocked.                                                                                                                 |
| Elapsed cap          | The maintainer capped elapsed time at 359999999ms. Play continues. Snapshot validation rejects larger values. Replay timestamps keep their safe-integer range.                            |
| Move cap             | The maintainer capped moves at 999999. Matching and winning continue. Snapshots and saved bests reject larger values.                                                                     |
| Formats              | Board code, snapshot and replay format versions stay at 1. Product spec stays at 2 under the maintainer's cap decisions.                                                                  |
| Test runner          | The maintainer requires Vitest 5.0.5 for game unit tests. Product-rule coverage is kept. The removed second-clock parameter's test is replaced with shared-time assertions.               |
| Rejection vector     | A literal BigInt-calculated shuffle fixture exercises the rejection branch independently.                                                                                                 |
| Half-pair gap        | The first half-pair mutation exposed an overlapping-invalid-input gap. An otherwise-valid fixture now isolates it.                                                                        |
| Reference separation | Game files stay in reference. Audit records, captures and mutation tools stay here.                                                                                                       |
| Focus and controls   | Board handles its remembered focus. Input reset uses a keyed input while keeping action buttons mounted. Feedback follows reproduction controls.                                          |
| Mutations            | Several clauses can break together. A kill requires the declared test and assertion reason. M07 exercises real timer cancellation. M19 leaves padding unchanged and fails outline bounds. |
| Screen reader        | I17.1 through I17.5 are tree-checked. I17.6 records the pending person-run spoken journey.                                                                                                |

## Checks

The named combination is Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. The browser before hook asserts all four actual versions. Prettier 3.9.6 formats the source.

The locked install and checks have these results:

| Gate                           | Result                                                                    |
| ------------------------------ | ------------------------------------------------------------------------- |
| Product install and type check | Passed                                                                    |
| Product tests                  | 93 passed, zero failed and zero skipped                                   |
| Reference install              | Passed from the lockfile with the worktree-local npm cache                |
| Reference type check           | All three projects passed                                                 |
| Vitest unit and static checks  | 123 passed, zero failed and zero skipped                                  |
| Build                          | Current source emitted one offline HTML file                              |
| Browser command                | 57 passed in each of three consecutive runs, zero failed and zero skipped |
| Mutation harness               | 21 caught for their declared assertion reasons                            |
| Harness self-test              | Passed. Unrelated test names and failure reasons are rejected             |
| Audit type check               | Passed                                                                    |

Each browser command builds the game and diagnostic sheet afresh. Vite clears its output before writing. Fresh contexts run against that build. The zoom API targets the loaded game tab. Text bounds check its contents as well as outer layout. Offline delivery permits only the HTML's own file request.

The [verification record](verification.json) gives commands and counts. The [seed record](seed-record.json) binds source, configuration, trace and evidence hashes. Raw command logs are disposable outputs. The seed is not frozen.

## Screen reader

The [accessibility record](evidence/accessibility-journey.json) contains 22 sanitized Chromium accessibility-tree captures. It includes independent control names, all closed positions, revealed names, disabled matched states, atomic result payloads, the final summary, Tab exit and New Game. A separate observer checks one result change per turn.

Tree checks do not record speech. A person follows [Screen Reader Journey](screen-reader-journey.md), records NVDA and browser versions and copies the result into the game README. I17.6 stays open until that happens.

## Visual review

The [grayscale sheet](face-sheet-grayscale.png) contains every face on an actual 44px card. The mask test still checks 128 different geometries. Mask uniqueness and perceptual distinction are different results. The builder viewed this PNG. V12.7 is ready for the next judge with vision. No earlier absent image is counted as current visual evidence.

## Process

T07.9 concerns stopping development processes. A12.1 concerns the order used when changing a rule. They describe work rather than game behavior. Their trace entries therefore cite process review, not product assertions.

This correction round started no development server or watcher. Builds and tests completed as child processes. Browser hooks close their contexts, browser and private HTTP server. The earlier development-command record reported its server stopped. A reviewer checks the work record for T07.9.

The cap rules were decided and recorded in the preceding packet. This round changes the implementation, tests and evidence under existing product rules. No new product rule or format was added. A reviewer checks that record for A12.1. Source placement alone doesn't show the historical edit order.

## Review clauses

The following 51 clauses use a review note or still need judgment. The first review's source verdicts do not approve the changed candidate. The next judgment reviews these reasons alongside executable evidence:

| Clause | Reason                                                                                               |
| ------ | ---------------------------------------------------------------------------------------------------- |
| I17.6  | Open person item. Record the spoken NVDA journey and versions.                                       |
| V11.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| V11.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| V12.7  | Next vision judgment. The 44px PNG and distinct masks are separate evidence.                         |
| T03.5  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T03.6  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T05.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T05.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T05.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T05.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T06.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T06.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T06.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T06.5  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T07.9  | Working-process review. Product behavior doesn't show development history.                           |
| T10.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T10.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| T10.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A01.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A01.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A01.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A02.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A02.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A02.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A02.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A03.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A03.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A04.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A04.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A04.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A04.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A05.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A05.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A05.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A05.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A05.5  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A06.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A07.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A07.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A07.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A09.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A09.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A09.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A10.1  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A10.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A10.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A10.4  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A10.5  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A12.1  | Working-process review. Product behavior doesn't show development history.                           |
| A12.2  | Source or design review. The rule requires reading the final structure, types or dependency choices. |
| A12.3  | Source or design review. The rule requires reading the final structure, types or dependency choices. |

## Clause ledger

Every clause has real paths in the [trace map](../trace.json). Existing checks slots are unchanged. This ledger describes the evidence type. It does not replace independent acceptance:

| Clause | Evidence type                |
| ------ | ---------------------------- |
| R01.1  | Executable evidence          |
| R01.2  | Executable evidence          |
| R01.3  | Executable evidence          |
| R01.4  | Executable evidence          |
| R02.1  | Executable evidence          |
| R02.2  | Executable evidence          |
| R02.3  | Executable evidence          |
| R03.1  | Executable evidence          |
| R03.2  | Executable evidence          |
| R03.3  | Executable evidence          |
| R04.1  | Executable evidence          |
| R04.2  | Executable evidence          |
| R04.3  | Executable evidence          |
| R04.4  | Executable evidence          |
| R04.5  | Executable evidence          |
| R05.1  | Executable evidence          |
| R05.2  | Executable evidence          |
| R05.3  | Executable evidence          |
| R05.4  | Executable evidence          |
| R05.5  | Executable evidence          |
| R05.6  | Executable evidence          |
| R05.7  | Executable evidence          |
| R05.8  | Executable evidence          |
| R06.1  | Executable evidence          |
| R06.2  | Executable evidence          |
| R06.3  | Executable evidence          |
| R06.4  | Executable evidence          |
| R06.5  | Executable evidence          |
| R06.6  | Executable evidence          |
| R07.1  | Executable evidence          |
| R07.2  | Executable evidence          |
| R07.3  | Executable evidence          |
| R07.4  | Executable evidence          |
| R07.5  | Executable evidence          |
| R07.6  | Executable evidence          |
| R07.7  | Executable evidence          |
| R07.8  | Executable evidence          |
| R07.9  | Executable evidence          |
| R07.10 | Executable evidence          |
| R07.11 | Executable evidence          |
| R08.1  | Executable evidence          |
| R08.2  | Executable evidence          |
| R08.3  | Executable evidence          |
| R08.4  | Executable evidence          |
| R08.5  | Executable evidence          |
| R08.6  | Executable evidence          |
| R09.1  | Executable evidence          |
| R09.2  | Executable evidence          |
| R09.3  | Executable evidence          |
| R09.4  | Executable evidence          |
| R09.5  | Executable evidence          |
| R09.6  | Executable evidence          |
| R10.1  | Executable evidence          |
| R10.2  | Executable evidence          |
| R10.3  | Executable evidence          |
| R10.4  | Executable evidence          |
| R10.5  | Executable evidence          |
| R10.6  | Executable evidence          |
| R10.7  | Executable evidence          |
| R11.1  | Executable evidence          |
| R11.2  | Executable evidence          |
| R11.3  | Executable evidence          |
| R11.4  | Executable evidence          |
| R12.1  | Executable evidence          |
| R12.2  | Executable evidence          |
| C01.1  | Executable evidence          |
| C01.2  | Executable evidence          |
| C01.3  | Executable evidence          |
| C02.1  | Executable evidence          |
| C02.2  | Executable evidence          |
| C02.3  | Executable evidence          |
| C03.1  | Executable evidence          |
| C03.2  | Executable evidence          |
| C03.3  | Executable evidence          |
| C04.1  | Executable evidence          |
| C04.2  | Executable evidence          |
| C04.3  | Executable evidence          |
| C05.1  | Executable evidence          |
| C05.2  | Executable evidence          |
| C05.3  | Executable evidence          |
| C05.4  | Executable evidence          |
| C06.1  | Executable evidence          |
| C06.2  | Executable evidence          |
| C07.1  | Executable evidence          |
| C07.2  | Executable evidence          |
| C07.3  | Executable evidence          |
| C08.1  | Executable evidence          |
| C08.2  | Executable evidence          |
| C08.3  | Executable evidence          |
| C08.4  | Executable evidence          |
| C08.5  | Executable evidence          |
| C09.1  | Executable evidence          |
| C09.2  | Executable evidence          |
| C09.3  | Executable evidence          |
| C09.4  | Executable evidence          |
| C10.1  | Executable evidence          |
| C10.2  | Executable evidence          |
| C10.3  | Executable evidence          |
| C10.4  | Executable evidence          |
| C10.5  | Executable evidence          |
| C10.6  | Executable evidence          |
| C10.7  | Executable evidence          |
| C11.1  | Executable evidence          |
| C11.2  | Executable evidence          |
| C11.3  | Executable evidence          |
| C11.4  | Executable evidence          |
| C12.1  | Executable evidence          |
| C12.2  | Executable evidence          |
| C12.3  | Executable evidence          |
| C12.4  | Executable evidence          |
| C13.1  | Executable evidence          |
| C13.2  | Executable evidence          |
| C13.3  | Executable evidence          |
| C13.4  | Executable evidence          |
| C13.5  | Executable evidence          |
| C14.1  | Executable evidence          |
| C14.2  | Executable evidence          |
| C14.3  | Executable evidence          |
| I01.1  | Executable evidence          |
| I01.2  | Executable evidence          |
| I02.1  | Executable evidence          |
| I03.1  | Executable evidence          |
| I03.2  | Executable evidence          |
| I04.1  | Executable evidence          |
| I04.2  | Executable evidence          |
| I05.1  | Executable evidence          |
| I05.2  | Executable evidence          |
| I05.3  | Executable evidence          |
| I06.1  | Executable evidence          |
| I06.2  | Executable evidence          |
| I06.3  | Executable evidence          |
| I06.4  | Executable evidence          |
| I06.5  | Executable evidence          |
| I06.6  | Executable evidence          |
| I06.7  | Executable evidence          |
| I06.8  | Executable evidence          |
| I06.9  | Executable evidence          |
| I07.1  | Executable evidence          |
| I08.1  | Executable evidence          |
| I08.2  | Executable evidence          |
| I09.1  | Executable evidence          |
| I09.2  | Executable evidence          |
| I09.3  | Executable evidence          |
| I10.1  | Executable evidence          |
| I10.2  | Executable evidence          |
| I11.1  | Executable evidence          |
| I12.1  | Executable evidence          |
| I12.2  | Executable evidence          |
| I13.1  | Executable evidence          |
| I14.1  | Executable evidence          |
| I14.2  | Executable evidence          |
| I14.3  | Executable evidence          |
| I15.1  | Executable evidence          |
| I15.2  | Executable evidence          |
| I16.1  | Executable evidence          |
| I16.2  | Executable evidence          |
| I16.3  | Executable evidence          |
| I16.4  | Executable evidence          |
| I16.5  | Executable evidence          |
| I17.1  | Tree-checked                 |
| I17.2  | Tree-checked                 |
| I17.3  | Tree-checked                 |
| I17.4  | Tree-checked                 |
| I17.5  | Tree-checked                 |
| I17.6  | Open person item             |
| V01.1  | Executable evidence          |
| V01.2  | Executable evidence          |
| V01.3  | Executable evidence          |
| V01.4  | Executable evidence          |
| V02.1  | Executable evidence          |
| V02.2  | Executable evidence          |
| V02.3  | Executable evidence          |
| V03.1  | Executable evidence          |
| V03.2  | Executable evidence          |
| V03.3  | Executable evidence          |
| V04.1  | Executable evidence          |
| V04.2  | Executable evidence          |
| V04.3  | Executable evidence          |
| V04.4  | Executable evidence          |
| V05.1  | Executable evidence          |
| V05.2  | Executable evidence          |
| V06.1  | Executable evidence          |
| V06.2  | Executable evidence          |
| V06.3  | Executable evidence          |
| V06.4  | Executable evidence          |
| V06.5  | Executable evidence          |
| V06.6  | Executable evidence          |
| V06.7  | Executable evidence          |
| V06.8  | Executable evidence          |
| V07.1  | Executable evidence          |
| V08.1  | Executable evidence          |
| V08.2  | Executable evidence          |
| V08.3  | Executable evidence          |
| V08.4  | Executable evidence          |
| V09.1  | Executable evidence          |
| V09.2  | Executable evidence          |
| V10.1  | Executable evidence          |
| V10.2  | Executable evidence          |
| V10.3  | Executable evidence          |
| V10.4  | Executable evidence          |
| V10.5  | Executable evidence          |
| V10.6  | Executable evidence          |
| V10.7  | Executable evidence          |
| V10.8  | Executable evidence          |
| V10.9  | Executable evidence          |
| V11.1  | Review                       |
| V11.2  | Review                       |
| V12.1  | Executable evidence          |
| V12.2  | Executable evidence          |
| V12.3  | Executable evidence          |
| V12.4  | Executable evidence          |
| V12.5  | Executable evidence          |
| V12.6  | Executable evidence          |
| V12.7  | Masks checked, vision review |
| V13.1  | Executable evidence          |
| V13.2  | Executable evidence          |
| V14.1  | Executable evidence          |
| V14.2  | Executable evidence          |
| V15.1  | Executable evidence          |
| V15.2  | Executable evidence          |
| V15.3  | Executable evidence          |
| V16.1  | Executable evidence          |
| V16.2  | Executable evidence          |
| V17.1  | Executable evidence          |
| V17.2  | Executable evidence          |
| V18.1  | Executable evidence          |
| V18.2  | Executable evidence          |
| V18.3  | Executable evidence          |
| V18.4  | Executable evidence          |
| V19.1  | Executable evidence          |
| V19.2  | Executable evidence          |
| V19.3  | Executable evidence          |
| V19.4  | Executable evidence          |
| V19.5  | Executable evidence          |
| V20.1  | Executable evidence          |
| V20.2  | Executable evidence          |
| V20.3  | Executable evidence          |
| V20.4  | Executable evidence          |
| V20.5  | Executable evidence          |
| S01.1  | Executable evidence          |
| S02.1  | Executable evidence          |
| S02.2  | Executable evidence          |
| S02.3  | Executable evidence          |
| T01.1  | Executable evidence          |
| T01.2  | Executable evidence          |
| T01.3  | Executable evidence          |
| T02.1  | Executable evidence          |
| T02.2  | Executable evidence          |
| T02.3  | Executable evidence          |
| T02.4  | Executable evidence          |
| T02.5  | Executable evidence          |
| T02.6  | Executable evidence          |
| T02.7  | Executable evidence          |
| T02.8  | Executable evidence          |
| T02.9  | Executable evidence          |
| T02.10 | Executable evidence          |
| T03.1  | Executable evidence          |
| T03.2  | Executable evidence          |
| T03.3  | Executable evidence          |
| T03.4  | Executable evidence          |
| T03.5  | Review                       |
| T03.6  | Review                       |
| T04.1  | Executable evidence          |
| T04.2  | Executable evidence          |
| T05.1  | Review                       |
| T05.2  | Review                       |
| T05.3  | Review                       |
| T05.4  | Review                       |
| T06.1  | Review                       |
| T06.2  | Review                       |
| T06.3  | Executable evidence          |
| T06.4  | Review                       |
| T06.5  | Review                       |
| T07.1  | Executable evidence          |
| T07.2  | Executable evidence          |
| T07.3  | Executable evidence          |
| T07.4  | Executable evidence          |
| T07.5  | Executable evidence          |
| T07.6  | Executable evidence          |
| T07.7  | Executable evidence          |
| T07.8  | Executable evidence          |
| T07.9  | Review                       |
| T08.1  | Executable evidence          |
| T08.2  | Executable evidence          |
| T08.3  | Executable evidence          |
| T08.4  | Executable evidence          |
| T09.1  | Executable evidence          |
| T09.2  | Executable evidence          |
| T10.1  | Review                       |
| T10.2  | Review                       |
| T10.3  | Review                       |
| T10.4  | Executable evidence          |
| A01.1  | Review                       |
| A01.2  | Review                       |
| A01.3  | Review                       |
| A02.1  | Review                       |
| A02.2  | Review                       |
| A02.3  | Review                       |
| A02.4  | Review                       |
| A03.1  | Review                       |
| A03.2  | Review                       |
| A04.1  | Review                       |
| A04.2  | Review                       |
| A04.3  | Review                       |
| A04.4  | Review                       |
| A04.5  | Executable evidence          |
| A05.1  | Review                       |
| A05.2  | Review                       |
| A05.3  | Review                       |
| A05.4  | Review                       |
| A05.5  | Review                       |
| A05.6  | Executable evidence          |
| A06.1  | Review                       |
| A06.2  | Executable evidence          |
| A06.3  | Executable evidence          |
| A07.1  | Review                       |
| A07.2  | Review                       |
| A07.3  | Review                       |
| A08.1  | Executable evidence          |
| A08.2  | Executable evidence          |
| A08.3  | Executable evidence          |
| A08.4  | Executable evidence          |
| A09.1  | Review                       |
| A09.2  | Review                       |
| A09.3  | Executable evidence          |
| A09.4  | Review                       |
| A10.1  | Review                       |
| A10.2  | Review                       |
| A10.3  | Review                       |
| A10.4  | Review                       |
| A10.5  | Review                       |
| A11.1  | Executable evidence          |
| A11.2  | Executable evidence          |
| A11.3  | Executable evidence          |
| A12.1  | Review                       |
| A12.2  | Review                       |
| A12.3  | Review                       |
| D01.1  | Executable evidence          |
| D01.2  | Executable evidence          |
| D01.3  | Executable evidence          |
| D01.4  | Executable evidence          |
| D02.1  | Executable evidence          |
| D02.2  | Executable evidence          |
| D02.3  | Executable evidence          |
| D02.4  | Executable evidence          |
| D03.1  | Executable evidence          |
| D03.2  | Executable evidence          |
| D03.3  | Executable evidence          |
| D04.1  | Executable evidence          |
| D04.2  | Executable evidence          |
| D04.3  | Executable evidence          |
| D04.4  | Executable evidence          |
| D04.5  | Executable evidence          |
| D04.6  | Executable evidence          |
| D05.1  | Executable evidence          |
| D05.2  | Executable evidence          |
| D05.3  | Executable evidence          |
| D05.4  | Executable evidence          |
| D05.5  | Executable evidence          |
| D05.6  | Executable evidence          |
| D05.7  | Executable evidence          |
| D06.1  | Executable evidence          |
| D06.2  | Executable evidence          |
| D06.3  | Executable evidence          |
| D06.4  | Executable evidence          |
| D06.5  | Executable evidence          |
| D06.6  | Executable evidence          |
| D06.7  | Executable evidence          |
| D07.1  | Executable evidence          |
| D07.2  | Executable evidence          |
| D07.3  | Executable evidence          |
| D07.4  | Executable evidence          |
| D07.5  | Executable evidence          |
| D07.6  | Executable evidence          |
| D07.7  | Executable evidence          |
| D07.8  | Executable evidence          |
| D07.9  | Executable evidence          |
| D08.1  | Executable evidence          |
| D08.2  | Executable evidence          |
| D08.3  | Executable evidence          |
| D08.4  | Executable evidence          |
| D08.5  | Executable evidence          |
| D08.6  | Executable evidence          |
| D08.7  | Executable evidence          |
| D09.1  | Executable evidence          |
| D09.2  | Executable evidence          |
| D09.3  | Executable evidence          |
| D09.4  | Executable evidence          |
| D09.5  | Executable evidence          |
| D09.6  | Executable evidence          |
| D09.7  | Executable evidence          |
| D10.1  | Executable evidence          |
| D10.2  | Executable evidence          |
| D10.3  | Executable evidence          |
| D10.4  | Executable evidence          |
| D10.5  | Executable evidence          |
| D10.6  | Executable evidence          |
| D10.7  | Executable evidence          |
| D10.8  | Executable evidence          |
| D10.9  | Executable evidence          |
| D10.10 | Executable evidence          |

## Mutations

Run from reference with the documented worktree-local browser settings, then call `node ../reference-audit/mutations.ts`. The harness checks clean unit and browser baselines first. It changes temporary copies under the game test-work folder and removes each copy afterward.

Each fixture lists directly affected clauses, one test and a required assertion reason. The report records the actual assertion and other failed tests. An unrelated failure, build error or missing report cannot count as a kill. The [mutation results](mutation-results.json) contain all 21 reasons:

| Mutation | Clauses                    | Result | Required assertion                         |
| -------- | -------------------------- | ------ | ------------------------------------------ |
| M01      | R03.1, R03.2, R04.1, R04.2 | Caught | R03.2 matching pair stays matched          |
| M02      | R05.1, R05.4               | Caught | R05.1 second distinct reveal adds one move |
| M03      | R07.1, D09.1               | Caught | R07.1 first reveal starts time             |
| M04      | R04.3, I10.2, D09.4        | Caught | R04.3 mismatch blocks activation           |
| M05      | R04.1, D09.3               | Caught | R04.1 mismatch closes at 800ms             |
| M06      | R07.9, R07.11, D09.5       | Caught | R07.9 elapsed time saturates               |
| M07      | A04.3, R12.1               | Caught | A04.3 active timer cancelled at unmount    |
| M08      | C10.4                      | Caught | C10.4 paused import blocks activation      |
| M09      | R10.1, R10.2               | Caught | R10.1 size six has its separate best       |
| M10      | R10.2, R10.4               | Caught | R10.4 worse score keeps best               |
| M11      | R10.3                      | Caught | R10.3 scores survive storage round trip    |
| M12      | D03.2, D03.3, D04.6        | Caught | D03.2 generator follows independent states |
| M13      | D01.2                      | Caught | D01.2 inclusive maximum seed               |
| M14      | D05.1, D06.2               | Caught | snapshot rejects isolated field case 1     |
| M15      | D06.3                      | Caught | D06.3 half pair is invalid                 |
| M16      | D09.1, D09.2, D09.3        | Caught | D09.2 time advances before activation      |
| M17      | D08.5, D08.7               | Caught | D08.7 equal timestamps keep array order    |
| M18      | I06.1, I07.1               | Caught | I07.1 left movement stops at row edge      |
| M19      | V16.1, V19.5, V20.4        | Caught | V19.5 full focus outline                   |
| M20      | I14.2, A08.1, V14.2        | Caught | I14.2 hidden cards mount no face           |
| M21      | R05.5, R05.8, D07.8        | Caught | R05.5 move count saturates                 |

## Size

These counts follow Prettier 3.9.6. Physical lines include blank lines. The nonblank column excludes them. Tiers cover the game source, styles, tests, scripts and configuration:

| Tier                | Files | Physical lines | Nonblank lines |
| ------------------- | ----- | -------------- | -------------- |
| Configuration       | 9     | 119            | 117            |
| Styles              | 5     | 303            | 303            |
| Development scripts | 5     | 93             | 87             |
| Tests and fixtures  | 62    | 3383           | 3243           |
| Components          | 8     | 457            | 437            |
| Game functions      | 12    | 473            | 441            |
| Browser session     | 7     | 348            | 316            |

Counts exclude Markdown, package-lock.json, HTML, generated distributions, captured evidence, audit tools, command logs, caches, browser downloads and dependencies. Every authored game TypeScript file is within 200 lines. Files above the 100-line aim stay listed in verification.json.

## File changes

The following inventory compares the prior candidate record with this correction packet. Audit moves appear as a deleted old path and created new path. Generated output and ignored work folders are excluded:

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

I17.6 needs the person-run NVDA journey and version record. V12.7 needs the next vision judgment. The source and process review clauses above need the next independent judgment. The overseer handles the protected benchmark checks and final freeze decision.

The overseer also handles Layer 2 subset generation and manual dist and local Chromium cleanup. Earlier cleanup was rejected by automatic approval review as blocked by policy. No alternate deletion route was used.

Back to [Neon Pairs](../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/_neon-pairs.md).
