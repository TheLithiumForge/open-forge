---
open-forge:
  description: Status output catalogue with every status, level, format and finding message
  tags: [Memory, Working, CLI, Task, Plan, G4, Status, Contextual, Active]
---

# 10 — status

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`status` answers "Is Open Forge installed here, is it current, and how much
does startup cost?" in two lines when everything is fine, and names every
file that needs a look when something is not.

## Depends on / Blocks

- Depends on: 03. Lane A, before 11.
- Blocks: 40.

## Shape

Report. Read-only. Never prints `No files changed.`

## Situations

`not-installed`, `healthy`, `healthy-with-extension`, `changed-managed-file`,
`missing-managed-file`, `stale-entries`, `recovery-bundle-present`,
`library-link-missing`, `no-ownership-record`, `unreadable-entry-file`,
`blocked-workspace`, `invalid-input`. Each at `minimal`, `standard`, `full`,
text and JSON.

## Statuses and headlines

| Status                  | When                                                                                                           | Headline                                                                                                                  | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | installed, every check complete, nothing needs a look                                                          | `Open Forge is installed and current.`                                                                                    |    0 | stdout |
| completed               | not installed (no `.agents/loader.md`)                                                                         | `Open Forge is not installed in <workspace path>.`                                                                        |    0 | stdout |
| completed               | installed, only Info findings (no ownership record)                                                            | `Open Forge is installed and current.` then the Info rows at `full`                                                       |    0 | stdout |
| completed-with-warnings | changed or missing Framework or Extension files, stale Entries, missing Library links, recovery bundle present | `Open Forge is installed, but <N> files need attention.` or `... <N> things need attention.` when a warning is not a file |    2 | stdout |
| incomplete              | a measurement or record could not be read                                                                      | `Open Forge is installed, but some checks could not finish.`                                                              |    3 | stdout |
| incomplete + warnings   | both                                                                                                           | `Open Forge is installed, but <N> files need attention and some checks could not finish.`                                 |    3 | stdout |
| invalid-input           | bad flag or operand                                                                                            | family `invalid-input`: `Cannot check status: <problem>.`                                                                 |    4 | stderr |
| blocked                 | workspace missing, not a directory, unsafe, or a boundary cannot be checked safely                             | `Cannot check this workspace: <reason>.`                                                                                  |    5 | stderr |
| failed                  | unexpected error                                                                                               | `Status stopped because of an unexpected error: <reason>.`                                                                |    1 | stderr |
| cancelled               | Ctrl+C                                                                                                         | `Status was cancelled.`                                                                                                   |  130 | stderr |

## Text by level

`minimal`, healthy:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
```

`minimal`, healthy with Extensions and a Library:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
  Extensions: development 0.1.0, planning 0.1.0
  Libraries: team-knowledge (12 links)
```

`minimal`, warnings:

```text
Open Forge is installed, but 2 files need attention.
  Warning  .agents/maps/_maps.md         changed since it was installed
  Warning  .agents/patterns/_patterns.md  missing; it was installed by the Framework
  Startup reads 19 of 22 routed files, about 8.0k tokens.
Next: open-forge update
```

`minimal`, incomplete:

```text
Open Forge is installed, but some checks could not finish.
  Startup context could not be measured: .agents/maps/_maps.md could not be read completely.
Next: open-forge doctor
```

`minimal`, not installed:

```text
Open Forge is not installed in D:/work/myrepo.
Next: open-forge install --dry-run
```

`standard` adds, in this order after the findings: `Workspace: <path>`, then

```text
Startup context
  Shipped by this CLI:  19 files, about 8.0k tokens
  This workspace:       19 files, about 8.0k tokens
  May load again later:  4 files, about 1.0k tokens
  All routed files:     22 files, about 9.9k tokens (startup is 81%)
Routes: 8 root categories, 20 Entries sections current
Framework files: 21 current
Extensions: development 0.1.0 (3 files current)
Libraries: team-knowledge, shared/team -> docs (12 links current)
Recovery data: none
```

Lines whose count is zero are omitted (`Recovery data: none` is shown only at
`full`). `Difference` appears only when non-zero: `Added since shipped: 2
files, about 400 tokens`. Root categories added or removed appear as
`added: custom` and `removed: patterns` on the Routes line.

`full` adds every Framework file with its state, every Entries section with
its state, every Extension file, every Library link with expected and
observed targets, the three largest may-load-again sources, and the recovery
bundle paths with their integrity.

`debug` adds the operational contributor coverage on stderr.

## Findings catalogue

Codes gain the `status.` prefix (ledger). Subjects are paths unless stated.

| Code (new)                              | Severity | Family                  | Message                                                                                                   | Next                                |
| --------------------------------------- | -------- | ----------------------- | --------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| status.invalid-input                    | error    | invalid-input           |                                                                                                           |                                     |
| status.workspace-unavailable            | error    | workspace-unavailable   |                                                                                                           |                                     |
| status.workspace-not-directory          | error    | workspace-not-directory |                                                                                                           |                                     |
| status.workspace-unsafe                 | error    | workspace-unsafe        |                                                                                                           |                                     |
| status.entry-unavailable                | warning  | local                   | `AGENTS.md could not be read.`                                                                            | `open-forge doctor`                 |
| status.embedded-framework-unavailable   | warning  | payload-unavailable     |                                                                                                           |                                     |
| status.context-inventory-incomplete     | warning  | local                   | `Startup context could not be measured completely: <path> could not be read completely.`                  | `open-forge doctor`                 |
| status.startup-context-unavailable      | warning  | local                   | `The current startup context could not be measured.`                                                      | `open-forge doctor`                 |
| status.continuity-context-unavailable   | warning  | local                   | `The may-load-again context could not be measured.`                                                       | `open-forge doctor`                 |
| status.root-categories-unavailable      | warning  | local                   | `The root categories could not be read from .agents/loader.md.`                                           | `open-forge doctor`                 |
| status.generated-navigation-changed     | warning  | local                   | `The Entries section of <path> is stale.`                                                                 | `open-forge index`                  |
| status.generated-navigation-missing     | warning  | local                   | `<path> has no Entries section.`                                                                          | `open-forge index`                  |
| status.generated-navigation-unavailable | warning  | local                   | `The Entries section of <path> could not be read.`                                                        | `open-forge doctor`                 |
| status.generated-navigation-blocked     | error    | generated-region-unsafe |                                                                                                           |                                     |
| status.framework-ownership-observation  | info     | ownership-observation   | `No ownership record exists, so Framework files cannot be checked against it.`                            |                                     |
| status.extension-ownership-observation  | info     | ownership-observation   | `No ownership record exists, so installed Extensions cannot be listed from it.`                           |                                     |
| status.library-ownership-observation    | info     | ownership-observation   | `No ownership record exists, so Libraries cannot be listed from it.`                                      |                                     |
| status.framework-lifecycle-untrusted    | warning  | lifecycle-unavailable   | `.agents/open-forge.lock.json cannot be used for Framework files: <reason>.`                              | `open-forge doctor`                 |
| status.framework-lifecycle-incomplete   | warning  | lifecycle-unavailable   |                                                                                                           |                                     |
| status.framework-lifecycle-blocked      | error    | lifecycle-blocked       |                                                                                                           |                                     |
| status.framework-target-changed         | warning  | local                   | `<path>  changed since it was installed`                                                                  | `open-forge update`                 |
| status.framework-target-missing         | warning  | local                   | `<path>  missing; it was installed by the Framework`                                                      | `open-forge update`                 |
| status.framework-target-unavailable     | warning  | local                   | `<path>  could not be read`                                                                               | `open-forge doctor`                 |
| status.framework-target-blocked         | error    | local                   | `<path>  could not be checked safely: <it is a link \| its location could not be verified>`               | `open-forge doctor`                 |
| status.extension-lifecycle-untrusted    | warning  | lifecycle-unavailable   | `.agents/open-forge.lock.json cannot be used for Extensions: <reason>.`                                   | `open-forge doctor`                 |
| status.extension-lifecycle-incomplete   | warning  | lifecycle-unavailable   |                                                                                                           |                                     |
| status.extension-lifecycle-blocked      | error    | lifecycle-blocked       |                                                                                                           |                                     |
| status.extension-source-unavailable     | warning  | local                   | `The source of the <id> Extension, <path>, cannot be read, so its files were not compared.` (subject: id) | `open-forge extension inspect <id>` |
| status.extension-target-changed         | warning  | local                   | `<path>  changed since it was installed by <id>`                                                          | `open-forge extension update <id>`  |
| status.extension-target-missing         | warning  | local                   | `<path>  missing; it was installed by <id>`                                                               | `open-forge extension update <id>`  |
| status.extension-target-unavailable     | warning  | local                   | `<path>  could not be read`                                                                               | `open-forge doctor`                 |
| status.extension-target-blocked         | error    | local                   | `<path>  could not be checked safely: <reason>`                                                           | `open-forge doctor`                 |
| status.recovery-candidate-verified      | warning  | local                   | `A recovery bundle from an earlier command is kept at <path>.`                                            | `open-forge cleanup`                |
| status.recovery-draft-incomplete        | warning  | local                   | `An unfinished recovery draft is at <path>. A command did not finish.`                                    | `open-forge cleanup --dry-run`      |
| status.recovery-final-malformed         | warning  | local                   | `The recovery bundle at <path> is damaged and cannot be used.`                                            | `open-forge cleanup --dry-run`      |
| status.recovery-final-unsupported       | warning  | local                   | `The recovery bundle at <path> was written by an unsupported version.`                                    | `open-forge cleanup --dry-run`      |
| status.recovery-final-unavailable       | warning  | local                   | `The recovery bundle at <path> could not be read.`                                                        | `open-forge doctor`                 |
| status.recovery-catalogue-unavailable   | warning  | local                   | `The recovery store at <path> could not be read.`                                                         | `open-forge doctor`                 |
| status.library-record-malformed         | error    | local                   | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.`                               | `open-forge doctor`                 |
| status.library-record-unavailable       | warning  | lifecycle-unavailable   | `The Library section of .agents/open-forge.lock.json could not be read.`                                  | `open-forge doctor`                 |
| status.library-source-root-invalid      | error    | local                   | `The source folder of the <id> Library, <path>, is not a folder inside the workspace.` (subject: id)      | `open-forge library inspect <id>`   |
| status.library-source-root-aliased      | error    | local                   | `The source folder of the <id> Library, <path>, resolves to an ambiguous location.`                       | `open-forge library inspect <id>`   |
| status.library-source-root-unavailable  | warning  | local                   | `The source folder of the <id> Library, <path>, cannot be read.`                                          | `open-forge library inspect <id>`   |
| status.library-projection-missing       | warning  | local                   | `<path>  missing; it is a link of the <id> Library`                                                       | `open-forge library sync <id>`      |
| status.library-projection-changed       | warning  | local                   | `<path>  is no longer the link the <id> Library created`                                                  | `open-forge library inspect <id>`   |
| status.library-projection-unavailable   | warning  | local                   | `<path>  could not be checked`                                                                            | `open-forge doctor`                 |
| status.library-projection-blocked       | error    | local                   | `<path>  could not be checked safely: <reason>`                                                           | `open-forge doctor`                 |
| status.library-extension-collision      | error    | local                   | `<path> is claimed by both the <id> Library and the <id> Extension.`                                      | `open-forge doctor`                 |
| status.operation-failed                 | error    | operation-failed        |                                                                                                           |                                     |
| status.interrupted                      | error    | interrupted             |                                                                                                           |                                     |

Rows written as `<path>  <phrase>` are rendered as finding rows with the
phrase as the message; the title is the phrase's first words capitalized
(`Changed since it was installed`).

## Counts and limitations

Counts (JSON `counts`, text sentence at `standard`): `routedFiles`,
`startupFiles`, `startupTokens`, `mayLoadAgainFiles`, `mayLoadAgainTokens`,
`allTokens`, `startupShare`, `rootCategories`, `entriesSectionsCurrent`,
`entriesSectionsStale`, `entriesSectionsMissing`, `frameworkFilesCurrent`,
`frameworkFilesChanged`, `frameworkFilesMissing`, `extensionsInstalled`,
`librariesRegistered`, `libraryLinksCurrent`, `libraryLinksMissing`,
`libraryLinksChanged`, `recoveryBundles`, `recoveryDrafts`. Unavailable
measurements are `null` with a limitation naming why.

Limitations render at every level as one sentence each, under the findings.

## Next rules

One line, chosen in this order: any Framework file warning ->
`open-forge update`; any Extension file warning -> `open-forge extension
update <id>` (first id); any Library link warning -> `open-forge library sync
<id>`; stale Entries -> `open-forge index`; recovery bundle ->
`open-forge cleanup`; any limitation or blocked -> `open-forge doctor`; not
installed -> `open-forge install --dry-run`; otherwise none.

## JSON data by level

| Level    | `data` members                                                                                                                                                                                                                                                                                 |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `installation { state, entryPath, loaderPath }`, `context { startup { shipped, current, difference, mayLoadAgain } each { files, characters, bytes, tokens }, allRouted { same }, startupShare }`, `extensions [ { id, version } ]`, `libraries [ { id, sourceRoot, destinationRoot } ]`       |
| standard | adds `structure { rootCategories { count, added, removed } }`, per-Extension `files { current, changed, missing }`, per-Library `links { current, missing, changed }`                                                                                                                          |
| full     | adds `frameworkFiles [ { path, state } ]`, `entriesSections [ { path, state } ]`, per-Extension `files [ { path, state } ]`, per-Library `links [ { path, state, expectedTarget, observedTarget } ]`, `context.mayLoadAgainSources [...]`, `recovery.candidates [ { path, kind, integrity } ]` |

Scalars are plain numbers or `null`. Every non-current file is also a finding.

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Status/Shared/Rendering/*` — replaced by `Presentation/Status/StatusReportSelector.cs` and `StatusDataTextRenderer.cs`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Status/Shared/Aggregation/*` — finding causes and codes.
- Status interface contract, sections Human Output, Structured Output, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 merged with the bridge registered for status.

## Steps

1. [ ] Write `StatusReportSelector`: headline per the table; one finding per
       code with the message column; counts; limitations; data per level.
2. [ ] Write `StatusDataTextRenderer` for the startup, routes, Framework,
       Extensions, Libraries and recovery blocks at `standard` and `full`.
3. [ ] Delete the three old renderers and the compact JSON projection.
4. [ ] Regenerate snapshots for every situation and review against this file.
5. [ ] Three suites green.

## Acceptance

- [ ] Healthy `minimal` is exactly two lines.
- [ ] Every situation's snapshot matches this catalogue at all levels and both formats.
- [ ] No Framework file is listed twice.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`6347237c`). The worker could not write this file or commit: a worktree's
> `.git` points into the main repository, outside its writable root, so
> `index.lock` is refused, and it attempted the task file through the other
> checkout's absolute path, which its sandbox refused as "writing outside of the
> project".

- file layout/type: the sixteen legacy Status renderers under `Presentation/Legacy/Status/Shared/Rendering/` and the seven `Commands/Status/Models/Presentation/` wire models are deleted. Native `Presentation/Status/` owns the selector, the data model, the text and JSON renderers, help and wording, and `CliStandaloneComposer` closes the Status binding over it.
- type: `Commands/Status/Models/Result/StatusStateModels.cs`, `StatusFindingVocabulary.cs` and `Shared/Aggregation/StatusStateMap.cs` are added so rendering consumes command-owned state values instead of importing the Framework layer, which `LayerBoundaryTests` forbids.
- message: the legacy header block and `Status:` line are replaced by one headline that states the outcome, for example `Open Forge is installed, but 20 files need attention.` A healthy workspace answers the startup cost in one sentence, `Startup reads 19 of 22 routed files, about 8.0k tokens.`
- message: `standard` opens that sentence into the `Startup context` block with `Shipped by this CLI:`, `This workspace:` and `May load again later:` rows, plus `All routed files:`, `Routes:` and `Framework files:` lines and the `Workspace:` echo. The retired `Total available context` and `Largest continuity sources` headings are gone.
- JSON member: `data` changed from the legacy lifecycle graph to `{ installation, context, extensions, libraries }` at minimal, adding `structure` at standard and `frameworkFiles`, `entriesSections` and `recovery` at full. `data.lifecycle.framework.state` is replaced by `data.installation.state`; `data.recovery` exposes `candidates[]` of `{ path, kind, integrity }` rather than an `incompleteDrafts` counter; findings moved to the envelope root and carry the `status.` prefix.
- counts: the result publishes `routedFiles`, `startupFiles`, `startupTokens`, `mayLoadAgainFiles`, `mayLoadAgainTokens`, `allTokens`, `startupShare`, `rootCategories`, `entriesSections` and Framework-file counts, with nullable counts and a stated limitation where a boundary prevented measurement.
- test retirement: eight legacy unit test files under `Commands/Status/` are deleted, including `StatusCompactJsonTests`, `StatusHumanViewTests`, `StatusJsonRenderingTests`, `StatusLifecycleOutputSnapshotTests`, `StatusFiniteMappingTests` and `StatusPresentationTests`. `Presentation/Status/StatusNativePresentationTests.cs` replaces them for the native contract.
- snapshots: eleven situations captured at four detail levels in text and JSON, 117 files under `src/cli/tests/integration/snapshots/StatusBeforeOutputSnapshotTests/`.
- evidence, worker: build 0 warnings and 0 errors; unit 3,382 -> 3,478 passed, 0 failed, 0 skipped; integration 2,180/2,197 -> 2,275/2,292 passed, 0 failed, 17 skipped unchanged; `check:dotnet` exactly the five documented errors.
- evidence, overseer after merging: unit 3,443 passed, 0 failed, 0 skipped; integration 2,287 total, 2,270 passed, 0 failed, 17 skipped; end-to-end 163 passed, 0 failed, 0 skipped.
- test, overseer: `PublishedStatusProcessTests`, and the Status assertions inside `PublishedUpdateProcessTests`, still read the legacy graph and were migrated after the merge. See the same entry in [19](19-references.md) for why every command's published journey needed this.

## Divergences observed

- **Needs a maintainer decision.** The worker reported four places where this catalogue and the code disagree on frozen wording, and correctly changed neither: the lifecycle-unavailable message, the generated-navigation-blocked message, the interrupted message, and the recovery-catalogue-unavailable message. Each needs the maintainer to say which spelling is authoritative before [41](41-documentation-propagation.md) runs.
- The worker could not complete its own closeout; the ledger above and the commit were made by the overseer.

- test, overseer: `PublishedStatusProcessTests` and the Status assertions inside `PublishedUpdateProcessTests` still read the legacy graph and were migrated after the merge. `data.lifecycle.framework.state` became `data.installation.state`, findings moved from `data.findings` to the envelope root and carry the `status.` prefix, and `data.recovery` is selected from `full` and exposes `candidates[]` with `kind` and `integrity` instead of an `incompleteDrafts` counter. In text, the `Status:` line is gone, the incomplete draft is stated as a finding, and the detail ladder is asserted through `Startup context`, `Shipped by this CLI:` and `All routed files:` instead of the retired `Total available context` and `Largest continuity sources` headings.

## Rollback

Restore the bridge registration for status.
