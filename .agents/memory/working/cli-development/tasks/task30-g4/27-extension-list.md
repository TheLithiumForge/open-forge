---
open-forge:
  description: Extension list output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Extension, Contextual, Active]
---

# 27 — extension list

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`extension list` shows what is installed and what can be installed, one row
per package with its version and description, and how to install one.

## Depends on / Blocks

- Depends on: 03. Lane E, first.
- Blocks: 28.

## Shape

Data.

## Situations

`none-installed`, `one-installed`, `installed-only`, `available-only`,
`explicit-source`, `no-ownership-record` (info), `source-unreadable`
(incomplete), `installed-source-missing` (warnings),
`installed-source-unavailable` (incomplete), `installed-source-invalid`
(incomplete), `installed-source-blocked` (blocked),
`installed-files-changed` (warnings), `installed-files-missing` (warnings),
`installed-target-blocked` (blocked), `installed-target-unavailable`
(incomplete), `installed-files-unavailable` (incomplete), `source-invalid`,
`source-blocked`, `invalid-input`.

## Statuses and text

| Status                  | When                                                                          | Text                                                                                                                                       | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | sections listed                                                               | the two blocks below                                                                                                                       |    0 | stdout |
| completed               | no ownership record                                                           | blocks; the Installed block reads `Installed  (no ownership record, so installed packages cannot be listed)` and an Info finding at `full` |    0 | stdout |
| completed-with-warnings | an installed package's source is missing, or its files changed or are missing | rows with a note, warning rows                                                                                                             |    2 | stdout |
| incomplete              | the source or a manifest could not be read                                    | the safe rows plus warning rows                                                                                                            |    3 | stdout |
| invalid-input           | bad flag, invalid `--source` package                                          | `Cannot list Extensions: <problem>.`                                                                                                       |    4 | stderr |
| blocked                 | source overlaps the workspace, unsafe path                                    | `Cannot list Extensions: <reason>.`                                                                                                        |    5 | stderr |
| failed                  | unexpected error                                                              | `Extension list stopped because of an unexpected error: <reason>.`                                                                         |    1 | stderr |
| cancelled               | Ctrl+C                                                                        | `Extension list was cancelled.`                                                                                                            |  130 | stderr |

## Text by level

`minimal`:

```text
Installed
  development 0.1.0

Available (bundled with this CLI)
  development-toolkit 0.1.0   An optional bundle of project documents, Memory starters, planning, and development packages   (5 packages)
  memory-starters 0.1.0       Copy-ready Memory Templates for decisions, ideas, analyses, observations, and handoffs
  orchestration 0.1.0         Coordinate dependent tasks through one optional managed-delivery workflow   (3 packages)
  planning 0.1.0              An optional planning Workflow, Work Records Pattern, and Templates for tasks, plans, backlogs, and checkpoints
  project-documents 0.1.0     Optional Vision and Architecture Workflows with document Templates for a project's direction and structure
Next: open-forge extension install <id>
```

`Installed  none` when nothing is installed. The `Available` heading names
the source: `(bundled with this CLI)` or `(from <path>)`. An installed
package already in the available list is shown once in each block; the
Available row adds `installed` when versions match or `installed: 0.1.0`
when they differ. A package with `(N packages)` bundles that many.

`standard` adds `Workspace:`, the source path, and per Available row its
dependencies (`needs: planning, project-documents`).

`full` adds per Installed row the file count and the recorded source, and
the lock coverage sentence.

## Findings catalogue

| Code                                 | Severity | Family                | Message                                                                                                                                      | Next |
| ------------------------------------ | -------- | --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- | ---- |
| extension-list.invalid-input         | error    | invalid-input         |                                                                                                                                              |      |
| extension-list.workspace-unavailable | error    | workspace-unavailable |                                                                                                                                              |      |
| extension-list.source-invalid        | error    | local                 | `<path> is not an Extension package or package folder: <reason>. Expected extension.json with id, name, description, version, dependencies.` | none |
| extension-list.source-blocked        | error    | local                 | `<path> cannot be used as a source: <it is inside the workspace \| it resolves to an unsafe location>.`                                      | none |
| extension-list.source-unavailable    | warning  | local                 | `<path> could not be read, so available packages are not listed.`                                                                            | none |
| extension-list.installed-source-missing     | warning  | local  | `The <id> Extension's source <path> is missing.`                                                    | `open-forge extension inspect <id>` |
| extension-list.installed-source-unavailable | warning  | local  | `The <id> Extension's source <path> could not be read.`                                             | `open-forge extension inspect <id>` |
| extension-list.installed-source-invalid     | warning  | local  | `<path> is not an Extension package or package folder: <reason>. Expected extension.json with id, name, description, version, dependencies.` | `open-forge extension inspect <id>` |
| extension-list.installed-source-blocked     | error    | local  | `Cannot list Extensions: <reason>.`                                                                | `open-forge extension inspect <id>` |
| extension-list.installed-files-changed      | warning  | local  | `<N> file installed by <id> has changed since installation.` / `<N> files installed by <id> have changed since installation.` | `open-forge extension inspect <id>` |
| extension-list.installed-files-missing      | warning  | local  | `<N> file installed by <id> is missing.` / `<N> files installed by <id> are missing.`                                     | `open-forge extension inspect <id>` |
| extension-list.installed-target-blocked     | error    | local  | `The installed file <path> could not be checked safely.`                                          | `open-forge extension inspect <id>` |
| extension-list.installed-target-unavailable | warning  | local  | `<path> could not be read completely.`                                                            | `open-forge extension inspect <id>` |
| extension-list.installed-files-unavailable  | warning  | local  | `The files installed by <id> could not be compared.`                                               | `open-forge extension inspect <id>` |
| extension-list.ownership-observation | info     | ownership-observation |                                                                                                                                              |      |
| extension-list.operation-failed      | error    | operation-failed      |                                                                                                                                              |      |
| extension-list.interrupted           | error    | interrupted           |                                                                                                                                              |      |

Row notes for installed packages come from the lock and target checks:
`source missing` (the recorded source cannot be read), `files changed`,
`files missing`. Each also produces a warning finding with the package as
subject: `The <id> Extension's source <path> cannot be read.`, `<N> files
installed by <id> have changed since installation.`, `<N> files installed by
<id> are missing.` with `Next: open-forge extension inspect <id>`.

## Counts

`installed`, `available`.

## JSON data by level

| Level    | `data`                                                                                                                         |
| -------- | ------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ source { kind, path }, installed: [ { id, version, note } ], available: [ { id, version, name, description, packages } ] }` |
| standard | + per available `dependencies: [...]`                                                                                          |
| full     | + per installed `files`, `recordedSource`, `coverage`                                                                          |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/Shared/Rendering/*` — replaced by `Presentation/Extension/List/`.
- `Commands/Extension/Shared/Rendering/ExtensionHumanText.cs`, `ExtensionTextEscaping.cs` — deleted when the last Extension command migrates.
- Extension list interface Output section (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `ExtensionListReportSelector` and `ExtensionListDataTextRenderer` using `CliTable`.
2. [ ] Delete the old renderers.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] No `COMPLETE:` prefix anywhere.
- [ ] Descriptions are on the Available rows at `minimal`.

## Changes ledger

- 2026-09-14 — file layout/type: the List presentation bridge and legacy
  renderers were replaced by the command-owned native
  `Presentation/Extension/List` selector, data model, text/JSON renderers,
  help sections, and `ExtensionListWording`; the obsolete List JSON document
  and legacy human/JSON/diagnostic renderers were removed. The native selector
  has no Commands or Framework import, and it projects one typed row meaning
  for both text and JSON.
- 2026-09-14 — message: healthy text changed from the legacy `Extension list`,
  `Status: complete`, and coverage-prefixed body to the accepted Installed and
  Available blocks, with descriptions on minimal Available rows, a blank
  section separator, `Installed  (no ownership record, so installed packages
  cannot be listed)` for unavailable ownership, `Installed  none` for a known
  empty set, and the existing install Next command. The human source line says
  `Source: embedded catalogue` or `Source: <path>`; the machine token
  `embedded-catalogue` remains JSON-only.
- 2026-09-14 — JSON member: native `data` now carries source, installed rows,
  available rows, and the requested detail fields directly; JSON-only complete
  results add the invariant factual summary `The result contains <N> installed
  package(s) and <N> available package(s).` without adding a healthy text
  headline.
- 2026-09-14 — finding: installed lifecycle observations now use typed source
  and target states, owner/package subjects for aggregate changed/missing
  counts, separate path evidence, and the accepted installed source/file/target
  finding codes with owner-specific inspect actions. A readable source with a
  missing or ambiguous installed package produces `installed-files-unavailable`
  at incomplete/exit 3 rather than a selected-source-unavailable finding.
- 2026-09-14 — test: List unit presentation tests and native snapshots were
  migrated to `CliReportSelection`, `CliTextRenderer`, and `CliJsonRenderer`;
  integration semantic fixtures that require a genuinely installed package
  now use `ExtensionInstallIntegrationWorkspace`, while informational ownership
  assertions request full detail so the shared info listing policy is explicit.
- 2026-09-14 — verification: `dotnet build
  src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj -c Release
  --no-restore` passed with 0 warnings and 0 errors; the root CLI and Unit
  projects also passed with 0 warnings and 0 errors. The focused List Unit
  command (`OpenForge.Cli.Core.UnitTests.exe --filter-class
  '*OpenForge.Cli.Core.UnitTests.Commands.Extension.List.*' --parallel none
  --no-ansi --progress off --minimum-expected-tests 1 --fail-warns on
  --fail-skips off --report-xunit-ctrf --report-xunit-ctrf-filename results.json
  --results-directory artifacts/g4-27-extensions-unit-3`) passed 18/18 tests,
  with 0 failures and 0 skips. The final Integration project build command
  (`dotnet build
  src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj
  -c Release --no-restore`) passed with 0 warnings and 0 errors in
  `artifacts/g4-27-extensions-integration-build-6.log`.

- message: source-boundary parser failures now use the shared `the content is not valid JSON` cause in headlines, finding messages and unavailable counts; the parser text remains in the existing Cause evidence and debug diagnostics.
- snapshots: regenerated the eight `SourceBoundary/source-invalid` native captures across text and JSON detail levels; no other Extension List situation changed.
- catalogue: added the nine installed-source/file/target finding rows with the selector's emitted messages and owner-specific inspect actions, plus the eight missing Situations entries; no capture or gate changed.

- message: `installed-source-missing` and `installed-source-unavailable` shared
  one message, `The <id> Extension's source <path> cannot be read.`, so the two
  titles distinguished them but the sentences did not -> they now read
  `is missing.` and `could not be read.` respectively, matching their titles
  and the sibling `installed-files-*` wording.
- message: the `installed-target-blocked` title claimed `Installed file is unsafe`
  while its message says the file `could not be checked safely` -> the title is
  now `Installed file could not be checked`, which is what the code establishes.

## Divergences observed

- The working Task27 text and legacy integration snapshots still describe the
  pre-native `Extension list` headline/status envelope. The accepted Lane E
  rendering rules use the native Installed/Available blocks and leave the
  original Task01 before baselines untouched; native integration snapshot
  capture/migration is now complete in the authorized native tree.
- The shared presentation ladder hides info findings from minimal output,
  while the ownership contract requires the informational finding at full
  detail. The migrated integration assertion therefore uses JSON full detail;
  no shared renderer policy was changed.
- Existing List fixtures wrote an incomplete ownership record path when they
  intended a trusted installed package, and a marker-owned TemporaryWorkspace
  could not safely clean files created by a real install. Those cases now use
  the existing `ExtensionInstallIntegrationWorkspace`; no production cleanup
  or ownership safety was weakened.
- The first focused Integration run before the final fixture repair reported
  6/24 passed, 18 failed. Its failures included the old before-snapshot body
  and the now-repaired incomplete-fixture/info-detail assumptions. The
  repaired List application run passed 13/13, and native before-output update
  and verify runs passed 4/4 each.
- The four installed findings carrying `CliSemanticStatus.Incomplete`
  (`installed-source-unavailable`, `installed-source-invalid`,
  `installed-target-unavailable`, and `installed-files-unavailable`) are
  rendered as warnings by `CliReportVocabulary`, although the task's stated
  status mapping calls `Incomplete` an error. Their rows record the emitted
  warning severity; no source was changed.
- The eight newly catalogued installed situations other than
  `installed-source-missing` have no native before-output capture covering
  them. The existing `SourceBoundary/installed-source-missing` capture is the
  only installed finding capture in the current snapshot inventory.
- `installed-target-unavailable` uses the shared
  `<path> could not be read completely.` sentence but its installed-owner next
  action is `open-forge extension inspect <id>`, not the shared family's
  `open-forge doctor`; it remains a local row so the catalogue records the
  complete emitted finding.

## Native checkpoint — 2026-09-14

- R-EXT4: the known empty blocks are now single authored lines, exactly
  `Installed  none` and `Installed  (no ownership record, so installed packages cannot be listed)`;
  singleton Available rows omit the bundle count annotation, while bundles
  retain `(N packages)`. The installed marker is a text-only wording factory
  value and is absent from JSON `data.available[]`.
- R-EXT9: counts and source lines follow the selected side. A selected side
  with incomplete coverage reports a typed unavailable `CliCount` with the
  actual finding cause; an unselected side is omitted rather than reported as
  unreadable. Installed-only output therefore has no `Source: unavailable`.
  Existing typed mappings preserve the accepted source missing/invalid/
  unavailable/blocked, installed files unavailable, and target unavailable/
  blocked severities and inspect actions; version/dependency mismatches alone
  do not become file-change findings.
- R-EXT10: full detail is rendered per selected row, with that row's file,
  recorded-source, and dependency details adjacent to its aligned table row;
  details no longer form an ambiguous second document-wide section.
- R-EXT11: machine finding-code mapping remains command-owned in
  `ExtensionListDefinitions`; `ExtensionListFinding.MachineCode` is the narrow
  read-only model seam consumed by native selection. The obsolete
  `ExtensionListResultVocabulary` model behavior was removed.
- R-EXT12: installed `note` is present at every JSON detail level, including
  `null`; full detail additionally writes `files`, nullable `recordedSource`
  (including JSON `null`), and `coverage`. The concrete typed JSON converter
  owns this full-presence shape; no runtime detail policy or untyped payload was
  added.
- Before-output coverage now uses `MatchDetailsAsync` at all four levels while
  preserving the original Task01 before snapshots. The authorized native tree
  contains 106 files across 11 captured cases/scopes; update and verify runs
  both passed 4/4. The Windows source-missing fixture escapes backslashes in
  its lock JSON so the intended reader boundary is exercised.
- Focused evidence: Core build-10, Unit build-5, Integration build-12, and
  End-to-End build-1 each passed with 0 warnings/errors. List Unit passed 19/19
  with 0 failures/skips; List application Integration passed 13/13; published
  List E2E passed 3/3; native snapshot update and verify each passed 4/4.
  The full `OpenForge.Cli.slnx` Release build passed with 0 warnings and 0
  errors in `artifacts/g4-27-extensions-solution-build-1.log`.

### Native checkpoint divergences

- The native before-output tree intentionally adds the four-level detail
  captures needed for Task27; old Task01 snapshots and unrelated command
  baselines remain unchanged.
- The published List process fixture keeps a trusted installed lock row with
  no target paths so the process-backed listing exercises a complete
  ownership observation without introducing an unrelated mutation target.
- Remaining scope is parent review/integration; no later native command lane
  was started.

## R-EXT13/14 correction checkpoint — 2026-09-14

- R-EXT13: installed count coverage now follows only the selected ownership
  roster observation. An absent lock (known zero) and a trustworthy complete
  roster produce a numeric installed count even when package comparisons have
  changed, missing, unavailable, or blocked target findings, or when the
  selected available source is unavailable. Those row coverage states and
  findings remain unchanged and continue to carry their own evidence.
- R-EXT14: when Available is selected and the source finding is
  `SourceUnavailable`, `SourceInvalid`, or `SourceBlocked`, native `Next` is
  `null` unless an installed-owner inspect action takes precedence. This
  removes the generic doctor/help fallback from the source-boundary captures.
- The selector's three routine null-forgiving projections now use typed
  `OfType<string>()` flows with no output change outside the accepted Next
  correction.
- Authorized native snapshot update and verify each passed 4/4. The changed
  files are limited to the selected source-boundary captures (human Next lines
  removed and JSON `next` set to `null`); healthy and mixed installed/source
  captures retain their existing rows and counts.
- Post-correction verification: Core build-11 and Integration build-13 passed
  with 0 warnings/errors; focused List Unit passed 19/19, List application
  Integration passed 13/13, and published List E2E passed 3/3, all with zero
  failures/skips. No later native lane was started.

- The Source Boundary catalogue template already had the correct shape; only its raw parser reason was normalized at the presentation boundary. No catalogue wording was amended.

## Rollback

Restore the bridge registration for extension list.
