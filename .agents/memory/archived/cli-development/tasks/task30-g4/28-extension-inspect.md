---
open-forge:
  description: Extension inspect output catalogue
  tags: [Memory, CLI, Task, Plan, G4, Extension, Contextual, Archived, Historical]
---

# 28 — extension inspect

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`extension inspect` says in one sentence whether the installed package
matches the package it came from, names every file that differs, and keeps
hashes for `full`. It never tells the user to use another detail level.

## Depends on / Blocks

- Depends on: 03, 27. Lane E.
- Blocks: 29.

## Shape

Report.

## Situations

`installed-matches`, `installed-changed-and-retired`, `available-not-installed`,
`installed-source-missing` (incomplete), `newer-available`, `dependency-cycle`
(blocked), `unknown-id` (invalid), `no-ownership-record` (info),
`ambiguous-source` (blocked), `invalid-input`.

## Statuses and headlines

| Status                  | When                                                          | Headline                                                                                            | Exit | Stream |
| ----------------------- | ------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | installed and equal                                           | `<id> <version> is installed and matches the package.`                                              |    0 | stdout |
| completed               | available, not installed                                      | `<id> <version> is available and not installed.`                                                    |    0 | stdout |
| completed               | no ownership record                                           | `<id> <version> is available. No ownership record exists, so installation cannot be checked.`       |    0 | stdout |
| completed-with-warnings | files changed, missing, new or retired                        | `<id> <installed version> is installed. <N> files need attention.` (+ ` Version <v> is available.`) |    2 | stdout |
| incomplete              | source, manifest, dependency or file facts unreadable         | `<id> is installed, but the comparison could not finish: <limitation>.`                             |    3 | stdout |
| invalid-input           | bad ID, bad source, extra operand                             | `Cannot inspect <ref>: <problem>.`                                                                  |    4 | stderr |
| blocked                 | ambiguous identity, source overlap, cycle, ownership conflict | `Cannot inspect <id>: <reason>.`                                                                    |    5 | stderr |
| failed                  | unexpected error                                              | `Extension inspect stopped because of an unexpected error: <reason>.`                               |    1 | stderr |
| cancelled               | Ctrl+C                                                        | `Extension inspect was cancelled.`                                                                  |  130 | stderr |

## Text by level

`minimal`, equal:

```text
development 0.1.0 is installed and matches the package.
  3 files under .agents/workflows
```

`minimal`, differences:

```text
toolkit 1.0.0 is installed. 2 files need attention. Version 2.0.0 is available.
  Warning  .agents/changed.md   changed since it was installed
  Warning  .agents/retired.md   no longer part of the package
Next: open-forge extension update toolkit --dry-run
```

`standard` adds `Workspace:`, the source (`bundled with this CLI` or the
path), dependencies with their state, and every file with `unchanged`,
`changed`, `missing`, `new in the package` or `no longer part of the package`.

`full` adds both SHA-256 values per path, the manifest path, the resolution
order, the Entries sections the package registered in, and the record
coverage.

## Findings catalogue

| Code                                         | Severity | Family                  | Message                                                                           | Next                                                 |
| -------------------------------------------- | -------- | ----------------------- | --------------------------------------------------------------------------------- | ---------------------------------------------------- |
| extension-inspect.invalid-input              | error    | invalid-input           |                                                                                   |                                                      |
| extension-inspect.invalid-stable-id          | error    | local                   | `<value> is not a valid Extension ID. Use lowercase letters, digits and hyphens.` | `open-forge extension list`                          |
| extension-inspect.package-unavailable        | error    | unknown-id              | `No Extension has the ID <id> in <source>.`                                       | `open-forge extension list`                          |
| extension-inspect.package-invalid            | error    | local                   | `The package <id> at <path> is invalid: <reason>.`                                | fix by hand                                          |
| extension-inspect.source-invalid             | error    | local                   | `<path> is not an Extension package or package folder: <reason>.`                 | none                                                 |
| extension-inspect.source-unavailable         | warning  | local                   | `The source <path> could not be read, so the comparison could not finish.`        | none                                                 |
| extension-inspect.source-ambiguous           | error    | local                   | `<id> is found more than once in <source>.`                                       | fix by hand                                          |
| extension-inspect.source-overlap             | error    | local                   | `<path> is inside the workspace and cannot be used as a source.`                  | none                                                 |
| extension-inspect.workspace-unavailable      | error    | workspace-unavailable   |                                                                                   |                                                      |
| extension-inspect.workspace-unsafe           | error    | workspace-unsafe        |                                                                                   |                                                      |
| extension-inspect.identity-ambiguous         | error    | local                   | `The ownership record names <id> in a way that cannot be matched to one package.` | `open-forge doctor`                                  |
| extension-inspect.ownership-conflict         | error    | ownership-conflict      |                                                                                   |                                                      |
| extension-inspect.ownership-observation      | info     | ownership-observation   |                                                                                   |                                                      |
| extension-inspect.path-changed               | warning  | local                   | `<path>  changed since it was installed`                                          | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-missing               | warning  | local                   | `<path>  missing; it was installed by <id>`                                       | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-new                   | warning  | local                   | `<path>  new in the package; not installed yet`                                   | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-retired               | warning  | local                   | `<path>  no longer part of the package`                                           | `open-forge extension update <id> --prune --dry-run` |
| extension-inspect.path-invalid               | error    | local                   | `<path> in the package is not a valid workspace path.`                            | fix by hand                                          |
| extension-inspect.path-unavailable           | warning  | local                   | `<path>  could not be read`                                                       | `open-forge doctor`                                  |
| extension-inspect.fingerprint-unavailable    | warning  | local                   | `<path> could not be compared.`                                                   | `open-forge doctor`                                  |
| extension-inspect.fingerprint-fallback       | info     | local                   | `<path> was compared byte for byte because it is not Markdown.`                   | none                                                 |
| extension-inspect.generated-boundary-invalid | error    | generated-region-unsafe |                                                                                   |                                                      |
| extension-inspect.dependency-changed         | warning  | local                   | `<id> now requires <dependency>, which the installed version did not.`            | `open-forge extension update <id> --dry-run`         |
| extension-inspect.dependency-conflict        | error    | local                   | `<id> requires <dependency> <range>, but <version> is installed.`                 | fix by hand                                          |
| extension-inspect.dependency-cycle           | error    | local                   | `<a> requires <b>, which requires <a>.`                                           | fix by hand                                          |
| extension-inspect.dependency-incomplete      | warning  | local                   | `The dependency <dependency> of <id> could not be resolved: <reason>.`            | `open-forge doctor`                                  |
| extension-inspect.operation-failed           | error    | operation-failed        |                                                                                   |                                                      |
| extension-inspect.interrupted                | error    | interrupted             |                                                                                   |                                                      |

## Counts

`filesUnchanged`, `filesChanged`, `filesMissing`, `filesNew`, `filesRetired`,
`dependencies`.

## JSON data by level

| Level    | `data`                                                                                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, installed { version } \| null, available { version } \| null, source { kind, path }, matches: bool, files: [ { path, relation } ] }` |
| standard | + `dependencies: [ { id, version, state } ]`, `name`, `description`                                                                         |
| full     | + per file `installedSha256`, `packageSha256`, `manifestPath`, `resolutionOrder`, `registeredIn: [ path ]`, `recordCoverage`                |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Inspect/Shared/Rendering/*` — replaced by `Presentation/Extension/Inspect/`.
- Extension inspect interface Output and Next tables (ledger only).

## Preconditions

- [ ] 03, 27 merged.

## Steps

1. [ ] Write `ExtensionInspectReportSelector` and `ExtensionInspectDataTextRenderer`.
2. [ ] Delete the old renderers.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] No hash below `full`; no `Use --detail` sentence anywhere.
- [ ] Every differing file is a row at `minimal`.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`3758e098`). The worker could not write this file or commit, for the worktree
> reasons recorded in [10](10-status.md).

- file layout/type: the legacy Extension inspect renderers are deleted and native `Presentation/Extension/Inspect/` owns the presentation, data model, selector, text and JSON renderers, wording and help. The composer closes the inspect binding over it.
- message: the legacy header and trace are replaced by the catalogue headlines per status, the file-difference rows, the incomplete-source status, the cycle wording and the unknown-ID handling.
- JSON member: `data` follows the catalogue's per-level shape, with hashes gated to the detail that selects them.
- snapshots: 95 native files covering all ten catalogue situations; 40 obsolete legacy snapshots removed.
- evidence, worker: unit 3,523 -> 3,522 passed, 0 failed, 0 skipped; integration 2,300 -> 2,291, 0 failed, 17 skipped; `PublishedExtensionInspectProcessTests` 3/3; `check:dotnet` exactly the five documented errors.
- evidence, overseer after merging: unit 3,442 passed, 0 failed, 0 skipped; integration 2,278 total, 2,261 passed, 0 failed, 17 skipped; `PublishedExtensionInspectProcessTests` 3/3.
- process: this was the first worker whose packet assigned it its own published-process class. It ran that class and passed it, so the command needed no end-to-end repair after merging — unlike Status, References, Library inspect and Cleanup, which all did. The packet change is recorded in [40](40-verification.md).

## Divergences observed

- **Needs a maintainer decision.** `ambiguous-source` currently produces the domain finding `identity-ambiguous`. The worker did not change the wording and asks for confirmation that this is the intended finding for that situation.
- [01](01-before-snapshots.md) line 164 records the older unknown-ID and source-missing statuses. The implementation follows this catalogue instead, which is the accepted direction; the before-snapshot note is historical and should not be read as current truth.
- Extra operands remain parser-level stderr diagnostics outside the native envelope, consistent with the stream boundary recorded in [01](01-before-snapshots.md).
- The worker left a `verify-artifacts/` scratch directory it could not delete under its sandbox policy. The overseer removed it before committing; nothing from it is tracked.
- The worker could not complete its own closeout; the ledger above and the commit were made by the overseer.

## Rollback

Restore the bridge registration for extension inspect.
