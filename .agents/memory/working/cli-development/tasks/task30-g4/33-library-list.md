---
open-forge:
  description: Library list output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Library, Contextual, Active]
---

# 33 — library list

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`library list` shows one row per registered Library with where it comes from
and where its links live, names the links that are missing or changed, and
tells a new user how to register one.

## Depends on / Blocks

- Depends on: 03. Lane F, first.
- Blocks: 34.

## Shape

Data. `list` checks registered links only; it never scans the source folder.

## Situations

`none-registered`, `one-current`, `link-missing`, `link-changed`,
`no-ownership-record` (info), `source-folder-missing` (warnings),
`record-invalid` (incomplete), `record-unreadable` (incomplete), `link-blocked`
(blocked), `invalid-input`.

## Statuses and text

| Status                  | When                                                                       | Text                                                                                    | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | none registered                                                            | `No Libraries are registered.` + `Next: open-forge library attach <id> <source-folder>` |    0 | stdout |
| completed               | all links current                                                          | rows                                                                                    |    0 | stdout |
| completed               | no ownership record                                                        | `No ownership record exists, so Libraries cannot be listed from it.` (Info; exit 0)     |    0 | stdout |
| completed-with-warnings | a registered link is missing or changed, or a source folder cannot be read | `<N> Libraries registered. <K> links need attention.` then rows                         |    2 | stdout |
| incomplete              | record or a link fact unreadable                                           | rows plus warning rows                                                                  |    3 | stdout |
| invalid-input           | bad flag                                                               | `Cannot list Libraries: <problem>.`                                                     |    4 | stderr |
| blocked                 | unsafe identity, containment or link                                       | `Cannot list Libraries: <reason>.`                                                      |    5 | stderr |
| failed                  | unexpected error                                                           | `Library list stopped because of an unexpected error: <reason>.`                        |    1 | stderr |
| cancelled               | Ctrl+C                                                                     | `Library list was cancelled.`                                                           |  130 | stderr |

## Text by level

`minimal`, current:

```text
team-knowledge    shared/team -> docs   12 links current
```

`minimal`, attention:

```text
1 Library registered. 1 link needs attention.
  team-knowledge    shared/team -> docs   11 links current, 1 missing
    Warning  docs/review.md   missing
Next: open-forge library sync team-knowledge
```

`standard` adds `Workspace:` and every registered link with its state.

`full` adds expected and observed link targets per link and the record
coverage sentence.

## Findings catalogue

| Code                                 | Severity | Family                | Message                                                                     | Next                              |
| ------------------------------------ | -------- | --------------------- | --------------------------------------------------------------------------- | --------------------------------- |
| library-list.invalid-input           | error    | invalid-input         |                                                                             |                                   |
| library-list.invalid-record          | warning  | local                 | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.` | `open-forge doctor`               |
| library-list.record-unavailable      | warning  | lifecycle-unavailable | `The Library section of .agents/open-forge.lock.json could not be read.`    | `open-forge doctor`               |
| library-list.record-blocked          | error    | lifecycle-blocked     |                                                                             |                                   |
| library-list.ownership-observation   | info     | ownership-observation |                                                                             |                                   |
| library-list.source-root-invalid     | error    | local                 | `The source folder of <id>, <path>, is not a folder inside the workspace.`  | `open-forge library inspect <id>` |
| library-list.source-root-unavailable | warning  | local                 | `The source folder of <id>, <path>, cannot be read.`                        | `open-forge library inspect <id>` |
| library-list.source-root-blocked     | error    | local                 | `The source folder of <id>, <path>, resolves to an unsafe location.`        | none                              |
| library-list.link-missing            | warning  | local                 | `<path>  missing`                                                           | `open-forge library sync <id>`    |
| library-list.link-changed            | warning  | local                 | `<path>  is no longer the link <id> created`                                | `open-forge library inspect <id>` |
| library-list.link-unavailable        | warning  | local                 | `<path>  could not be checked`                                              | `open-forge doctor`               |
| library-list.link-blocked            | error    | local                 | `<path>  could not be checked safely: <reason>`                             | `open-forge doctor`               |
| library-list.operation-failed        | error    | operation-failed      |                                                                             |                                   |
| library-list.interrupted             | error    | interrupted           |                                                                             |                                   |

## Counts

`libraries`, `linksCurrent`, `linksMissing`, `linksChanged`, `linksUnavailable`.

## JSON data by level

| Level    | `data`                                                                                                         |
| -------- | -------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ libraries: [ { id, sourceFolder, destinationFolder, links { current, missing, changed, unavailable } } ] }` |
| standard | + per Library `links: [ { path, state } ]`                                                                     |
| full     | + per link `expectedTarget`, `observedTarget`, `sourceId`; `recordCoverage`                                    |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/List/Shared/Rendering/*` and `Commands/Library/Shared/Rendering/*` — replaced by `Presentation/Library/List/` and `Presentation/Library/Shared/`.
- Library list interface Output (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `LibraryListReportSelector` and `LibraryListDataTextRenderer`.
2. [ ] Delete the old renderers.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] Empty `minimal` is two lines (sentence and `Next`).
- [ ] No `Source inventory: not scanned` line at any level.

## Changes ledger

- file layout: the List bridge under `Presentation/Legacy/Library/List/Shared/Rendering/` was removed; native `Presentation/Library/List/` now owns `LibraryListPresentation`, typed data, selection, text, JSON, help, and wording.
- JSON data: the legacy `record`, `inventory`, `coverage`, and per-library `paths` graph became `data.libraries[].sourceFolder`, `destinationFolder`, and detail-selected `links`; minimal links are count objects, standard links are `{ path, state }` rows, and full links add nullable `expectedTarget`, `observedTarget`, and `sourceId` plus top-level `recordCoverage`.
- message: the native List text no longer prints `Source inventory: not scanned (library list checks registered links only)` or legacy `Status:`/`Selected by:` rows; empty minimal output now includes `No Libraries are registered.` followed by `Next: open-forge library attach <id> <source-folder>`.
- status: malformed and linked-invalid ownership records changed from the previous completed ownership-observation result to invalid or incomplete record findings; missing or unreadable source folders now produce the accepted attention classification; invalid binder input now carries `library-list.invalid-input` with the binder's actual cause.
- projection: native selection now exposes scalar workspace coordinates, selects link visibility and typed link totals once, and keeps the command's link state enum separate from its human (`not checked`) and finite JSON (`not-started`) vocabularies.
- failure projection: invalid and blocked reports use the command-local `Cannot list Libraries: <cause>.` headline and the failed status requests bounded `open-forge library list --detail debug`; only the binder `InvalidInput` finding is trimmed into that headline, so invalid-record findings retain their lock path and concrete parse cause at minimal detail.
- wording: link rows, link summaries, expected/observed target labels, source identifiers, and the failed retry reason now come from named `LibraryListWording` factories; the text renderer consumes selector-owned visibility, totals, and record coverage instead of recomputing detail policy.
- count coverage: authoritative complete and known-missing records publish their known Library/link counts; invalid, unavailable, blocked, failed, or interrupted record boundaries publish nullable roster/link counts with the actual boundary cause, which the shared report trimmer exposes as limitations. Known empty and no-ownership cases remain zero-count results without limitations.
- test: List unit, integration, and published-process callers were migrated from the legacy result graph to the native data graph; detail-shape, nullable full-target, finite vocabulary, stream/exit, no-inventory, workspace, and unchanged-fixture assertions were added or updated.
- test retirement: the executable four-case `LibraryListOutputSnapshotTests` class was deleted while its original Imprint files were preserved; `LibraryListHumanViewTests` now checks selected depth and status/stream semantics, leaving concrete human wording to the native snapshot suites.
- snapshots: `LibraryListBeforeOutputSnapshotTests` now captures each scenario through `MatchDetails`/`MatchDetailsAsync` at the four native detail levels under 98 generated files; the historical `Commands/Library/List/__snapshots__` before files remain unchanged.
- verification: `dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-restore` completed with 0 warnings and 0 errors after the bounded caller migration.
- verification: `dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore` completed with 0 warnings and 0 errors.
- verification: `dotnet test --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --filter-class "*LibraryListPresentationTests" --minimum-expected-tests 15` passed 15/15; the focused `LibraryListHumanViewTests`, `LibraryListFiniteValueTests`, and `LibraryListFindingTests` runs passed 2/2, 18/18, and 15/15 respectively.
- verification: the sequential focused integration commands filtered by `LibraryListObservationTests`, `LibraryListBoundaryTests`, `LibraryListAvailabilityTests`, `LibraryListOrderingAndPrecedenceTests`, and `LibraryListCompositionTests` produced 7/7, 9/10 with 1 accepted Linux skip, 0/2 with 2 accepted Linux skips, 4/4, and 9/9 passing cases respectively.
- verification: `artifacts/g4-04-libraries-native-unit-2.log` records the native unit snapshot run at 44 passed and 5 failed; the five failures are stale legacy snapshot expectations, with no snapshot files updated.
- verification: after the vocabulary test and headline-trimming correction, the unit Presentation filter passed 15/15; the integration project build passed with 0 warnings and 0 errors; the snapshot update and repeat runs for `LibraryListBeforeOutputSnapshotTests` passed 10/10 each, including the retained invalid-record row at minimal detail.
- verification: `dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-restore /m:1 --disable-build-servers` passed with 0 warnings and 0 errors; the receipt is `artifacts/g4-33-library-list-e2e-build-2.log`.
- verification: `dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedLibraryListProcessTests" --minimum-expected-tests 3` passed 3/3.
- verification: after R-LIB8 and test retirement, `dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-restore /m:1 --disable-build-servers` passed with 0 warnings and 0 errors; the List unit filter `--filter-class "*LibraryList*" --minimum-expected-tests 1` passed 59/59.
- verification: `dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore /m:1 --disable-build-servers` passed with 0 warnings and 0 errors; the List integration filter `--filter-class "*LibraryList*" --minimum-expected-tests 1` passed 39/42 with 3 accepted Linux permission skips.
- verification: after regenerating the existing `record-invalid` and `record-unreadable` native boundary captures, `LibraryListBeforeOutputSnapshotTests` update and repeat commands each passed 10/10; native snapshot inventory remains 98 files and historical before snapshots remain unchanged.
- verification: after the R-LIB8 count correction and active legacy test retirement, `dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-restore /m:1 --disable-build-servers` passed with 0 warnings and 0 errors; receipt `artifacts/g4-33-library-list-e2e-build-3.log`, followed by `PublishedLibraryListProcessTests` 3/3.

- test: `LibraryOwnershipReaderIntegrationTests.MissingOwnershipIsInformational`
  expected `list` to report completed for every ownership condition -> it
  expects completed for an absent record, invalid-input for an invalid one
  and incomplete for an unreadable one, which is the status table above.
  The no-legacy-fallback and no-link-deletion assertions the test owns are
  unchanged.

- status or exit: malformed or duplicate Library records now return `incomplete` / exit 3 instead of List's previous `invalid-input` / exit 4; the invalid-input row now covers bad flags only.
- message: invalid JSON is presented as `the content is not valid JSON` in the headline, finding and unavailable count reasons; the existing catalogue finding sentence remains unchanged.
- snapshots: regenerated the ten `ObservationBoundary_record-invalid/record-invalid` native captures across text and JSON detail levels; only the status/severity and normalized reason fields changed.

- catalogue: added `library-list.invalid-input` using the shared
  `invalid-input` family; the binder raises it as `Invalid`, which exits 4.

## Divergences observed

- The binder already emits `library-list.invalid-input` for invalid request
  input, but the findings catalogue had no row. The shared-family row now
  records that existing output; no source or capture changed.

- snapshot migration: native four-level files are now generated under `src/cli/tests/integration/snapshots/LibraryListBeforeOutputSnapshotTests/`; generation is authorized after corrected output is reviewed, and the original before files remain untouched.
- snapshot harness: `LibraryListBeforeOutputSnapshotTests` uses unique per-scenario `MatchDetails`/`MatchDetailsAsync` names so parallel theories do not share snapshot directories; shell diagnostic output remains captured only where the frozen harness permits it.
- end-to-end verification: the released checkpoint's `dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-restore /m:1 --disable-build-servers` passed with 0 warnings and 0 errors; published List process execution remains covered by the existing focused process test.
- legacy consumers: no `Presentation/Legacy/Library/List` production files or List bridge references remain; the other Legacy Library command folders are still active owners for their separate commands.
- review queue: R-LIB3 through R-LIB8 selector, wording, renderer, count, and snapshot corrections are applied; root review of the 98 native snapshot files and final Task 33 to Task 34 handoff remain, while the retired class's original snapshot files stay preserved as historical evidence.

- The List catalogue required an explicit status/finding-severity amendment for malformed records; its finding message template was retained verbatim. Raw parser detail remains in the existing internal diagnostics/evidence.

## Rollback

Restore the bridge registration for library list.
