# Reference Spec Revision

Reference Spec Revision is the change record that connects the council's spec version 2.1 decisions to the reference and its verification.

For example, a completed imported game now loads with its static win summary and no Resume button. An unfinished import still pauses and resumes at its saved state.

Status: reference verification passed. Product integration has 26 failing tests on protected v2 inputs. The v2 judges' accepted game is the starting point.

## Decisions

The [spec decision log](../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/spec-notes.md#council-revision) records every finding, decision and affected clause. The reference changes follow that log:

| Decision                 | Reference evidence                                                                                                                                                                                                       |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| I17 deliverable          | Accessibility-tree journey and README browser, Playwright and screen-reader-use record. Spoken output belongs to user acceptance.                                                                                        |
| Status and announcements | Independent status fixture, full keyboard tree journey, repeated-result browser checks and session message fixture.                                                                                                      |
| I13 scroll margin        | Computed 6px card scroll margin and interior-row outline bounds.                                                                                                                                                         |
| Acceptance               | Product journeys keep no evidence ledger or grading instructions.                                                                                                                                                        |
| Replay output            | Direct CLI and published `npm run --silent replay -- PATH` tests check JSON-only stdout, empty success stderr and nonzero rejection with stderr only. Other npm commands keep their errors and warnings.                 |
| Cap scope                | Runtime caps stay unchanged. The map moves only the requested clauses out of Layer 2.                                                                                                                                    |
| Equivalent zoom          | Fresh browser context at 640x400 CSS pixels with device scale 2. Controls, result, Resume, win and both board sizes are checked.                                                                                         |
| Spacing                  | Existing positive spacing stays on scale. Reset margins and named color targets have computed-style assertions.                                                                                                          |
| Toolchain                | The browser before hook asserts all four named versions.                                                                                                                                                                 |
| A12                      | The published prohibition matches the existing source and rubric meaning. Source review stays separate from runtime checks.                                                                                              |
| Minor details            | Fractional snapshot tests check floor and ceiling, board control trims whitespace, both load types select their size and all 128 faces have their expected fill and stroke colors. Card opacity transitions are removed. |
| Layer 3 time             | The plan uses 180 minutes provisionally. A real check batch measures times before the limit is frozen.                                                                                                                   |

## Separation

The game README points to `docs/`. Mutation and evidence exclusions leave its ignore and Vite files. Sub-clause IDs leave game test names and assertion messages. Requirement IDs stay because the product documents publish them. The unused zoom extension is deleted after replacing its journey with the specified equivalent layout.

The generator's version, previous-version reader and current subset count need a small compatibility update to write the new pack. Its history test keeps the original 180-to-187 cap addition counts. These are the only source edits outside the packet's named paths. The benchmark checks, cases, rubrics and runner are unchanged.

## Verification

The [verification record](verification.json) records the final commands and results. The initial revision passed reference install, typecheck, 128 Vitest tests, the build, three runs of 65 browser tests and all 21 declared mutation kills. The replay-command follow-up records its fresh gates below. The product typecheck passed. Product tests report 191 passed and 26 failed on protected v2 inputs. The subset generator tests report three passed. Both packs are regenerated through their materialize commands. The subset selection history records the reason for the changed clauses and wording. The trace has one entry for each of the 416 clauses across 97 requirement IDs. Layer 2 contains 189 clauses across 60 IDs.

The first browser pass exposed one old whitespace expectation for the pairs display. Updating it to the published fraction keeps the hover check's behavior. Every later browser run builds current source before it starts.

The product suite still needs the following separate integration work:

- Update the protected version-2 requirement loaders and coverage catalogs for v2.1.
- Refresh the Layer 2 and Layer 3 case input hashes, prompt, answer bank and time-limit data.
- Exclude installed reference cache files from the protected source scan.
- Revalidate the brownfield patches against this revised seed.

No Docker, WSL, model CLI or sign-in is used. All caches, browsers and temporary files are inside the worktree. No spoken output is tested by the developer.

## Replay command

C14.3 and T07.7 publish `npm run --silent replay -- PATH`. The command flag suppresses the replay banner. The seed's `.npmrc` contains only `cache=.npm-cache`, so other npm commands keep their errors and warnings. The replay tests use the published command for valid and invalid inputs. The usage message names it too. C14.4 and C14.5 keep their stdout, stderr and exit rules.

This follow-up passed install, typecheck, 128 Vitest tests, build, one complete run of 65 browser tests and all 21 mutation kills for their declared reasons on Node 26.12.1. The verification record binds these results to the updated source and frozen packs.

## Files

The [file inventory](revision-files.json) lists every created, changed and deleted source or document. Generated distributions, caches and raw logs stay outside that inventory.

Back to [Reference Audit](AUDIT.md).
